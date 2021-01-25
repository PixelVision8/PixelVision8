//   
// Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by Pixel Vision 8 are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman - @JesseFreeman
// Christina-Antoinette Neofotistou @CastPixel
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany
//

using Microsoft.Xna.Framework;
using PixelVision8.Player;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PixelVision8.Player
{

    internal class DrawActionAttribute : Attribute
    {
    }
    
    public partial class Canvas : AbstractData, IDraw
    {

        private readonly GameChip gameChip;
        private PixelData stroke = new PixelData();
        private PixelData pattern = new PixelData();
        private readonly Point spriteSize;
        private readonly CanvasDrawRequest[] requestPool = new CanvasDrawRequest[1024];
        private PixelData defaultLayer = new PixelData();
        private PixelData tmpLayer = new PixelData();
        private Rectangle _tmpRect = Rectangle.Empty;
        private int currentRequest = -1;
        private bool canDraw;
        private Point linePattern = new Point(1, 0);
        // public bool wrap = false;
        public Dictionary<string, Action<CanvasDrawRequest>> Actions = new Dictionary<string, Action<CanvasDrawRequest>>();
        private PixelData currentTexture;

        // These are temporary values we use to help speed up calculations
        private int _x0;
        private int _y0;
        private int _x1;
        private int _y1;
        private int _w;
        private int _h;
        private int _total;
        private CanvasDrawRequest _request;
        private int tmpX;
        private int tmpY;
        private int tmpW;
        private int tmpH;
        int _counter = 0;
        int _sx;
        int _sy;
        private long _a;
        private long _b;
        private long _b1;
        private double _dx;
        private double _dy;
        private double _err;
        private double _e2;
        private int _size;
        private PixelData _tmpPixelData = new PixelData();

        public int width => defaultLayer.Width;
        public int height => defaultLayer.Height;
        public int[] Pixels => defaultLayer.Pixels;

        public Canvas(int width, int height, GameChip gameChip = null)
        {

            Resize(width, height);

            // Make the canvas the default drawing surface
            currentTexture = defaultLayer;

            this.gameChip = gameChip;

            // Create a pool of draw requests
            for (int i = 0; i < requestPool.Length; i++)
            {
                requestPool[i] = new CanvasDrawRequest();
            }
            
            MethodInfo[] methods = typeof(Canvas).GetMethods(); 
  
            // for loop to read through all methods 
  
            // for (int i = 0; i < methods.GetLength(0); i++) { 
            //
            //     object[] attributesArray = methods[i].GetCustomAttributes(true); 
            //
            //     // foreach loop to read through  
            //     // all attributes of the method 
            //     foreach(Attribute item in attributesArray) 
            //     { 
            //         if (item is DrawActionAttribute) { 
            //
            //             // Display the fields of the NewAttribute 
            //             DrawActionAttribute attributeObject = (DrawActionAttribute)item; 
            //             Console.WriteLine("{0} - {1}", methods[i].Name, 
            //                 attributeObject);
            //             
            //             
            //             // Actions.Add("SetStroke", methods[i].CreateDelegate() => SetStrokeAction(reqatt););
            //
            //             // Actions.Add(new Action<CanvasDrawRequest>(request => methods[i]);
            //             //     {method[i].Name.Replace("Action", ""), request => method[i]});
            //
            //             // Actions.Add("SetStroke", request => SetStrokeAction(request));
            //         } 
            //     } 
            // } 

            // TODO could we register external drawing calls to this?
            Actions = new Dictionary<string, Action<CanvasDrawRequest>>()
            {
                {"SetStroke", request => SetStrokeAction(request)},
                {"SetPattern", request => SetPatternAction(request)},
                {"DrawLine", request => DrawLineAction(request)},
                {"DrawRectangle", request => DrawRectangleAction(request)},
                {"DrawEllipse", request => DrawEllipseAction(request)},
                {"DrawPixelData", request => DrawPixelDataAction(request)},
                {"FloodFill", request => FloodFillAction(request)},
                {"ChangeTargetCanvas", request => ChangeTargetCanvasAction(request)},
                {"SaveTmpLayer", request => SaveTmpLayerAction(request)},
            };

            if (gameChip != null)
            {
                spriteSize = gameChip.SpriteSize();
            }

        }

        public void Resize(int width, int height) => Utilities.Resize(defaultLayer, width, height);

        private CanvasDrawRequest NextRequest()
        {
            // Test to see if there is another available request
            if (currentRequest + 1 >= requestPool.Length)
                return null;

            // Increase the request
            currentRequest++;

            // Invalidate the canvas so the request will be called during the draw cycle
            Invalidate();

            // Return the new request
            return requestPool[currentRequest];
        }

        [DrawAction]
        private void DrawPixelDataAction(CanvasDrawRequest request)
        {
            Utilities.MergePixels(request.PixelData, request.Bounds.X, request.Bounds.Y, request.Bounds.Width, request.Bounds.Height, defaultLayer, request.x, request.y, request.FlipH, request.FlipV, request.ColorOffset);
        }

        private int GetPixel(PixelData pixelData, int x, int y)
        {
            _size = pixelData.Height;
            y = (y % _size + _size) % _size;
            _size = pixelData.Width;
            x = (x % _size + _size) % _size;

            return pixelData[x + pixelData.Width * y];
        }

        private void SetPixel(PixelData pixelData, int x, int y, int color)
        {

            // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
            _size = pixelData.Height;
            y = (y % _size + _size) % _size;
            _size = pixelData.Width;
            x = (x % _size + _size) % _size;

            pixelData[x + pixelData.Width * y] = color;

        }
        

        public void CopyPixels(ref int[] data, int x, int y, int blockWidth, int blockHeight)
        {
            // Flatten the canvas
            Draw();

            Utilities.CopyPixels(defaultLayer, x, y, blockWidth, blockHeight, ref data);
        }

        public void SetPixels(int x, int y, int blockWidth, int blockHeight, int[] pixels)
        {
            // Flatten the canvas
            Draw();

            Utilities.SetPixels(pixels, x, y, blockWidth, blockHeight, defaultLayer);

        }

        /// <summary>
        ///     Fast blit to the display through the draw request API
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="drawMode"></param>
        /// <param name="scale"></param>
        /// <param name="maskColor"></param>
        /// <param name="maskColorID"></param>
        /// <param name="viewport"></param>
        public void DrawPixels(int x = 0, int y = 0, DrawMode drawMode = DrawMode.TilemapCache, float scale = 1f,
            int maskColor = -1, int maskColorID = -1, int colorOffset = 0, Rectangle? viewport = null,
            int[] isolateColors = null)
        {

            // This only works when the canvas has a reference to the gameChip
            if (gameChip == null) return;

            if (viewport.HasValue)
            {
                tmpX = viewport.Value.X;
                tmpY = viewport.Value.Y;
                tmpW = viewport.Value.Width;
                tmpH = viewport.Value.Height;
            }
            else
            {
                tmpX = 0;
                tmpY = 0;
                tmpW = width;
                tmpH = height;
            }

            var srcPixels = GetPixels(tmpX, tmpY, tmpW, tmpH);

            // Loop through and replace mask colors
            for (int i = 0; i < srcPixels.Length; i++)
            {
                // Check to see if colors should be isolated
                if (isolateColors != null && Array.IndexOf(isolateColors, srcPixels[i]) == -1)
                {
                    srcPixels[i] = -1;
                }

                // Replace any mask color with the supplied mask color
                if (srcPixels[i] == maskColor)
                {
                    srcPixels[i] = maskColorID;
                }
            }

            // Covert the width and height into ints based on scale
            var newWidth = (int)(tmpW * scale);
            var newHeight = (int)(tmpH * scale);

            var destPixels = scale > 1 ? ResizePixels(srcPixels, tmpW, tmpH, newWidth, newHeight) : srcPixels;

            gameChip.DrawPixels(destPixels, x, y, newWidth, newHeight, false, false, drawMode, colorOffset);
        }

        public int[] GetPixels()
        {
            // Flatten the canvas
            Draw();

            return Utilities.GetPixels(defaultLayer);

        }

        public virtual void Clear(int colorRef = -1, int x = 0, int y = 0, int? width = null, int? height = null)
        {
            if (width.HasValue || height.HasValue)
            {
                var tmpWidth = width ?? 1;
                var tmpHeight = height ?? 1;

                var tmpPixels = new int[tmpWidth * tmpHeight];

                for (int i = 0; i < tmpPixels.Length; i++)
                {
                    tmpPixels[i] = colorRef;
                }

                SetPixels(x, y, tmpWidth, tmpHeight, tmpPixels);
            }
            else
            {
                Utilities.Clear(defaultLayer, colorRef);
                ResetValidation();
            }

        }

        public int[] GetPixels(int x, int y, int blockWidth, int blockHeight)
        {
            // Flatten the canvas
            Draw();

            return Utilities.GetPixels(defaultLayer, x, y, blockWidth, blockHeight);
        }

        public virtual void SetPixels(int[] pixels)
        {
            // Flatten the canvas
            Draw();

            Utilities.SetPixels(pixels, defaultLayer);

            // Invalidate();
        }

        
        public void Draw()
        {
            if (Invalid == false)
                return;

            // Calculate the total requests based on the current request number
            _total = currentRequest + 1;

            // Loop through all off the requests
            for (int i = 0; i < _total; i++)
            {
                // Get the next request
                _request = requestPool[i];

                // Check to see if the action exists
                if (Actions.ContainsKey(_request.Action))
                {
                    // Pass the request into the action
                    Actions[_request.Action](_request);
                }
            }

            // Reset the request
            currentRequest = -1;

            ResetValidation();
        }

    }

    

}