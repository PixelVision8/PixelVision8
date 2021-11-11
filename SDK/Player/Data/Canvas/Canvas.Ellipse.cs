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
    public sealed partial class Canvas
    {
        private long _a;
        private long _b;
        private long _b1;

        public void DrawEllipse(int x, int y, int ellipseWidth, int ellipseHeight, bool fill = false)
        {
            if (fill)
                ChangeTargetCanvas(ellipseWidth, ellipseHeight);

            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest;

            newRequest.Bounds.X = x;
            newRequest.Bounds.Y = y;
            newRequest.Bounds.Width = ellipseWidth;
            newRequest.Bounds.Height = ellipseHeight;

            newRequest.Fill = fill;

            newRequest.Action = DrawEllipseAction;

            // Save the changes to the request
            requestPool[currentRequest] = newRequest;

            if (fill)
                SaveTmpLayer(x, y, ellipseWidth, ellipseHeight);

            // Change back to default drawing surface
            // ChangeTargetCanvas(defaultLayer);
        }

        [DrawAction]
        public void DrawEllipseAction(CanvasDrawRequest request)
        {
            int _x0, _y0, _x1, _y1;
            double _dx, _dy, _err, _e2;

            // Save the x and y values to calculate below
            _x0 = request.Bounds.Left;
            _y0 = request.Bounds.Top;
            _x1 = request.Bounds.Right - stroke.Width;
            _y1 = request.Bounds.Bottom - stroke.Height;

            if (request.Fill)
            {
                request.Bounds.X = 0;
                request.Bounds.Y = 0;

                _x0 = request.Bounds.X;
                _y0 = request.Bounds.Y;
                _x1 = request.Bounds.Right - stroke.Width;
                _y1 = request.Bounds.Bottom - stroke.Height;
            }

            // Adjust for border
            // _y0 += stroke.Height;
            // _x1 -= stroke.Width;
            // _y1 -= stroke.Height;

            /* rectangular parameter enclosing the ellipse */
            _a = Math.Abs(_x1 - _x0);
            _b = Math.Abs(_y1 - _y0);
            _b1 = _b & 1; /* diameter */
            _dx = 4 * (1.0 - _a) * _b * _b;
            _dy = 4 * (_b1 + 1) * _a * _a; /* error increment */
            _err = _dx + _dy + _b1 * _a * _a;

            if (_x0 > _x1)
            {
                _x0 = _x1;
                _x1 += (int) (_a);
            } /* if called with swapped points */

            if (_y0 > _y1)
                _y0 = _y1; /* .. exchange them */

            _y0 += (int) ((_b + 1) / 2);
            _y1 = (int) (_y0 - _b1); /* starting pixel */
            _a = 8 * _a * _a;
            _b1 = 8 * _b * _b;
            do
            {
                SetStrokePixel(_x1, _y0); /* I. Quadrant */
                SetStrokePixel(_x0, _y0); /* II. Quadrant */
                SetStrokePixel(_x0, _y1); /* III. Quadrant */
                SetStrokePixel(_x1, _y1); /* IV. Quadrant */
                _e2 = 2 * _err;
                if (_e2 <= _dy)
                {
                    _y0++;
                    _y1--;
                    _err += _dy += _a;
                } /* y step */

                if (_e2 >= _dx || 2 * _err > _dy)
                {
                    _x0++;
                    _x1--;
                    _err += _dx += _b1;
                } /* x */
            } while (_x0 <= _x1);

            while (_y0 - _y1 <= _b)
            {
                /* to early stop of flat ellipses a=1 */
                SetStrokePixel(_x0 - 1, _y0); /* -> finish tip of ellipse */
                SetStrokePixel(_x1 + 1, _y0++);
                SetStrokePixel(_x0 - 1, _y1);
                SetStrokePixel(_x1 + 1, _y1--);
            }

            if (request.Fill)
            {
                // Save the center X & Y position before we save it back
                _x1 = request.Bounds.Center.X;
                _y1 = request.Bounds.Center.Y;

                if (request.Bounds.Width > stroke.Width && request.Bounds.Height > stroke.Height)
                {
                    request.Bounds.X = _x1;
                    request.Bounds.Y = _y1;

                    FloodFillAction(request);
                }
            }
        }
    }
}