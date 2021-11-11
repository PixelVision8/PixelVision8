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
        // int _counter = 0;
        private int _sx;
        private int _sy;


        public void DrawLine(int x0, int y0, int x1, int y1)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest;

            newRequest.Action = DrawLineAction;

            newRequest.Bounds.X = x0;
            newRequest.Bounds.Y = y0;

            // Well be using the width as the second point
            newRequest.Bounds.Width = x1;
            newRequest.Bounds.Height = y1;

            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }

        [DrawAction]
        private void DrawLineAction(CanvasDrawRequest drawRequest)
        {
            int _x0, _y0, _x1, _y1, _dx, _dy, _err, _e2;

            _x0 = drawRequest.Bounds.X;
            _y0 = drawRequest.Bounds.Y;
            _x1 = drawRequest.Bounds.Width; // - stroke.Width;
            _y1 = drawRequest.Bounds.Height; // - stroke.Width;

            // _counter = 0;

            _dx = _x1 - _x0;

            if (_dx < 0)
            {
                _dx = -_dx;
                _sx = -1;
            }
            else
            {
                _sx = 1;
            }

            _dy = _y1 - _y0;

            if (_dy < 0)
            {
                _dy = -_dy;
                _sy = -1;
            }
            else
            {
                _sy = 1;
            }

            _err = (_dx > _dy ? _dx : -_dy) / 2;

            for (;;)
            {
                SetStrokePixel(_x0, _y0);

                // _counter++;
                if (_x0 == _x1 && _y0 == _y1) break;

                _e2 = _err;
                if (_e2 > -_dx)
                {
                    _err -= _dy;
                    _x0 += _sx;
                }

                if (_e2 < _dy)
                {
                    _err += _dx;
                    _y0 += _sy;
                }
            }
        }
    }
}