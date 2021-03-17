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

namespace PixelVision8.Player
{
    public sealed partial class Canvas
    {
        private Rectangle _tmpRect;

        public void DrawRectangle(int x, int y, int rectWidth, int rectHeight, bool fill = false)
        {
            _tmpRect.X = x;
            _tmpRect.Y = y;

            if (fill)
            {
                ChangeTargetCanvas(rectWidth, rectHeight);

                // Reset the tmp Rect to 0,0 when drawing into the new layer
                _tmpRect.X = 0;
                _tmpRect.Y = 0;
            }

            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest;

            newRequest.Action = DrawRectangleAction;

            newRequest.Bounds.X = _tmpRect.X;
            newRequest.Bounds.Y = _tmpRect.Y;

            // Well be using the wid as the second point
            newRequest.Bounds.Width = rectWidth;
            newRequest.Bounds.Height = rectHeight;

            // Save the changes to the request
            requestPool[currentRequest] = newRequest;

            // Check again to see if we need to fill the rectangle
            if (fill)
            {
                // Make sure there are enough pixels to fill
                if (rectWidth > stroke.Width && rectHeight > stroke.Height)
                {
                    // Trigger a flood fill
                    FloodFill(newRequest.Bounds.Center.X, newRequest.Bounds.Center.Y);

                    // Copy pixels data to the main drawing surface
                    SaveTmpLayer(x, y, rectWidth, rectHeight);
                }
            }
        }

        [DrawAction]
        public void DrawRectangleAction(CanvasDrawRequest request)
        {
            _tmpRect.X = request.Bounds.X;
            _tmpRect.Y = request.Bounds.Y;
            _tmpRect.Width = request.Bounds.Width - stroke.Width;
            _tmpRect.Height = request.Bounds.Height - stroke.Height;

            // Top
            request.Bounds.X = _tmpRect.Left;
            request.Bounds.Y = _tmpRect.Top;

            // Well be using the wid as the second point
            request.Bounds.Width = _tmpRect.Right;
            request.Bounds.Height = _tmpRect.Top;

            DrawLineAction(request);

            // Right
            request.Bounds.X = _tmpRect.Right;
            request.Bounds.Y = _tmpRect.Top;

            // Well be using the wid as the second point
            request.Bounds.Width = _tmpRect.Right;
            request.Bounds.Height = _tmpRect.Bottom;

            DrawLineAction(request);

            // Bottom
            request.Bounds.X = _tmpRect.Left;
            request.Bounds.Y = _tmpRect.Bottom;

            // Well be using the wid as the second point
            request.Bounds.Width = _tmpRect.Right;
            request.Bounds.Height = _tmpRect.Bottom;

            DrawLineAction(request);

            // Left
            request.Bounds.X = _tmpRect.Left;
            request.Bounds.Y = _tmpRect.Top;

            // Well be using the wid as the second point
            request.Bounds.Width = _tmpRect.Left;
            request.Bounds.Height = _tmpRect.Bottom;

            DrawLineAction(request);
        }
    }
}