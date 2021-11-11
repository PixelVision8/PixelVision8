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
        public int[] Pixels => GetPixels();

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

        // public void Resize(int width, int height) => Utilities.Resize(defaultLayer, width, height);

        public void Resize(int newWidth, int newHeight, bool savePixels = false)
        {
            
            if(newWidth == Width && newHeight == Height)
                return;

            int[] pixels = null;

            var sampleWidth = Math.Min(Width, newWidth);
            var sampleHeight = Math.Min(Height, newHeight);

            if(savePixels)
            {
                pixels = GetPixels(0, 0, sampleWidth, sampleHeight);
            }

            Utilities.Resize(defaultLayer, newWidth, newHeight);

            if(savePixels && pixels != null)
            {
                SetPixels(0, 0, sampleWidth, sampleHeight, pixels);
            }

        }

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

        private int GetPixel(PixelData<int> pixelData, int x, int y)
        {
            // TODO this needs to be removed

            var size = pixelData.Height;
            y = (y % size + size) % size;
            size = pixelData.Width;
            x = (x % size + size) % size;

            return pixelData[x + pixelData.Width * y];
        }

        private void SetPixel(PixelData<int> pixelData, int x, int y, int color)
        {
            // TODO this needs to be removed

            // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
            var size = pixelData.Height;
            y = (y % size + size) % size;
            size = pixelData.Width;
            x = (x % size + size) % size;

            pixelData[x + pixelData.Width * y] = color;
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
        public void DrawPixels(int x = 0, int y = 0, DrawMode drawMode = DrawMode.TilemapCache, float scale = 1f, int colorOffset = 0, int maskId = Constants.EmptyPixel, Rectangle? viewport = null)
        {
            // This only works when the canvas has a reference to the gameChip
            if (gameChip == null) return;

            int tmpX, tmpY, tmpW, tmpH;

            if (viewport.HasValue)
            {
                tmpX = viewport.Value.X;
                tmpY = viewport.Value.Y;
                tmpW = Math.Min(viewport.Value.Width, Width);
                tmpH = Math.Min(viewport.Value.Height, Height);
            }
            else
            {
                tmpX = 0;
                tmpY = 0;
                tmpW = Width;
                tmpH = Height;
            }

            var srcPixels = GetPixels(tmpX, tmpY, tmpW, tmpH);

            // Covert the width and height into ints based on scale
            var newWidth = (int) (tmpW * scale);
            var newHeight = (int) (tmpH * scale);

            var destPixels = ResizePixels(srcPixels, tmpW, tmpH, newWidth, newHeight);

            gameChip.DrawPixels(destPixels, x, y, newWidth, newHeight, false, false, drawMode, colorOffset, maskId);
        }

        public int[] GetPixels()
        {
            // Flatten the canvas
            Draw();

            return Utilities.GetPixels(defaultLayer);
        }

        public void Clear()
        {
            Utilities.Clear( defaultLayer );
            ResetValidation();
        }

        // TODO should this be a color offset? If so, you would not be able to use it to clear an area
        public void Fill(int colorRef = Constants.EmptyPixel, int x = 0, int y = 0, int? width = null, int? height = null)
        {
            
            if (width.HasValue || height.HasValue)
            {
                var tmpWidth = Math.Max(1, width ?? 1);
                var tmpHeight = Math.Max(1, height ?? 1);

                var tmpPixels = new int[tmpWidth * tmpHeight];

                if(colorRef > 0)
                {
                    for (int i = 0; i < tmpPixels.Length; i++)
                    {
                        tmpPixels[i] = colorRef;
                    }
                }

                SetPixels(x, y, tmpWidth, tmpHeight, tmpPixels);
            }
            else
            {
                Utilities.Fill(defaultLayer, colorRef);
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


        // TODO Make sure there is a SetPixels where you can pass in coordinates 
        
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