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
using PixelVision8.Player;

namespace PixelVision8.Runner
{
    public partial class DisplayTarget : AbstractData
    {
    
        private int _gameWidth;
        private int _gameHeight;
        private readonly int _displayHeight;
        private readonly int _displayWidth;
        private int _monitorScale = 1;
        private int _totalPixels;
        private int _visibleWidth;
        private int _visibleHeight;
        private float _offsetX;
        private float _offsetY;
        private float _scaleX;
        private float _scaleY;

        public bool Fullscreen { get; set; }

        public DisplayTarget(int width, int height)
        {
            _displayWidth = Utilities.Clamp(width, 64, 640);
            _displayHeight = Utilities.Clamp(height, 64, 480);
            
            ResetResolution(_displayWidth, _displayHeight);
        }

        public int MonitorScale
        {
            get => _monitorScale;
            set
            {
                var fits = false;

                while (fits == false)
                {
                    var newWidth = _displayWidth * value;
                    var newHeight = _displayHeight * value;

                    if (newWidth < RealWidth &&
                        newHeight < RealHeight)
                    {
                        fits = true;
                        _monitorScale = value;
                    }
                    else
                    {
                        value--;
                    }
                }
            }
        }

        public void ResetResolution(int gameWidth, int gameHeight)
        {
            _gameWidth = gameWidth;
            _gameHeight = gameHeight;

            Invalidate();
            
        }

        private void CalculateResolution()
        {
            
            var tmpMonitorScale = Fullscreen ? 1 : MonitorScale;

            // Calculate the monitor's resolution
            _visibleWidth = Fullscreen
                ? RealWidth
                : _displayWidth *
                  tmpMonitorScale;
            _visibleHeight = Fullscreen
                ? RealHeight
                : _displayHeight * tmpMonitorScale;
        }

        private void CalculateDisplayScale()
        {
            // Calculate the game scale
            _scaleX = (float) _visibleWidth / _gameWidth;
            _scaleY = (float) _visibleHeight / _gameHeight;

            // To preserve the aspect ratio,
            // use the smaller scale factor.
            _scaleX = Math.Min(_scaleX, _scaleY);
            _scaleY = _scaleX;
        }

        private void CalculateDisplayOffset()
        {
            _offsetX = (_visibleWidth - _gameWidth * _scaleX) * .5f;
            _offsetY = (_visibleHeight - _gameHeight * _scaleY) * .5f;
        }

    }
}