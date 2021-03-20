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

using System;

namespace PixelVision8.Player
{
    internal class DrawActionAttribute : Attribute
    {
    }

    public sealed partial class Canvas : AbstractData, IDraw
    {
        private readonly CanvasDrawRequest[] requestPool = new CanvasDrawRequest[1024];

        private int currentRequest = -1;

        public int Width => defaultLayer.Width;
        public int Height => defaultLayer.Height;
        public int[] Pixels => defaultLayer.Pixels;

        public Canvas(int width, int height)
        {
            Configure(width, height);
        }

        private void Configure(int width, int height)
        {
            Resize(width, height);

            // Make the canvas the default drawing surface
            currentTexture = defaultLayer;

            // Create a pool of draw requests
            for (int i = 0; i < requestPool.Length; i++)
            {
                requestPool[i] = new CanvasDrawRequest();
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
            Utilities.MergePixels(request.PixelData, request.Bounds.X, request.Bounds.Y, request.Bounds.Width,
                request.Bounds.Height, defaultLayer, request.X, request.Y, request.FlipH, request.FlipV,
                request.ColorOffset);
        }

        private int GetPixel(PixelData pixelData, int x, int y)
        {
            // TODO this needs to be removed

            var size = pixelData.Height;
            y = (y % size + size) % size;
            size = pixelData.Width;
            x = (x % size + size) % size;

            return pixelData[x + pixelData.Width * y];
        }

        private void SetPixel(PixelData pixelData, int x, int y, int color)
        {
            // TODO this needs to be removed

            // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
            var size = pixelData.Height;
            y = (y % size + size) % size;
            size = pixelData.Width;
            x = (x % size + size) % size;

            pixelData[x + pixelData.Width * y] = color;
        }


        // public void CopyPixels(ref int[] data, int x, int y, int blockWidth, int blockHeight)
        // {
        //     // Flatten the canvas
        //     Draw();
        //
        //     Utilities.CopyPixels(defaultLayer, x, y, blockWidth, blockHeight, ref data);
        // }

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

            int tmpX, tmpY, tmpW, tmpH;

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
                tmpW = Width;
                tmpH = Height;
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
            var newWidth = (int) (tmpW * scale);
            var newHeight = (int) (tmpH * scale);

            var destPixels = scale > 1 ? ResizePixels(srcPixels, tmpW, tmpH, newWidth, newHeight) : srcPixels;

            gameChip.DrawPixels(destPixels, x, y, newWidth, newHeight, false, false, drawMode, colorOffset);
        }

        public int[] GetPixels()
        {
            // Flatten the canvas
            Draw();

            return Utilities.GetPixels(defaultLayer);
        }

        public void Clear(int colorRef = -1, int x = 0, int y = 0, int? width = null, int? height = null)
        {
            
            if (width.HasValue || height.HasValue)
            {
                var tmpWidth = Math.Max(1, width ?? 1);
                var tmpHeight = Math.Max(1, height ?? 1);

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

        public void SetPixels(int[] pixels)
        {
            // Flatten the canvas
            Draw();

            defaultLayer.SetPixels(pixels, defaultLayer.Width, defaultLayer.Height);

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
                requestPool[i].Action(requestPool[i]);
            }

            // Reset the request
            currentRequest = -1;

            ResetValidation();
        }
    }
}