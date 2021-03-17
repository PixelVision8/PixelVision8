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

using System.Collections.Generic;

namespace PixelVision8.Player
{
    /// <summary>
    ///     Wrapper for texture data that includes Hex color data to rebuild colors when
    ///     exporting and cutting up sprites. 
    /// </summary>
    public struct ImageData
    {
        public PixelData PixelData { get; }
        public int Width => PixelData.Width;
        public int Height => PixelData.Height;
        public int Columns => Width / _spriteSize.X;
        public int Rows => Height / _spriteSize.Y;

        public string[] Colors;


        public int TotalSprites => Columns * Rows;

        private Point _spriteSize;
        private List<int> _colorIDs;
        private int _colorId;
        private Point _pos;
        private int[] _tmpPixelData;

        // public int SpriteWidth
        // {
        //     get => _spriteSize.X;
        //     set => _spriteSize.X = value; // TODO this should be divisible by 8
        // }
        //
        // public int SpriteHeight
        // {
        //     get => _spriteSize.Y;
        //     set => _spriteSize.Y = value; // TODO this should be divisible by 8
        // }

        public ImageData(int width, int height, int[] pixels = null, string[] colors = null) // : base(width, height)
        {
            PixelData = new PixelData(width, height);

            if (pixels != null)
            {
                PixelData.SetPixels(pixels);
            }

            Colors = colors;

            _spriteSize = new Point(8, 8);
            _colorIDs = new List<int>();
            _colorId = 0;
            _pos = new Point();
            _tmpPixelData = null;
        }

        /// <summary>
        ///     Get a single sprite from the Image.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cps">Total number of colors supported by the sprite.</param>
        /// <returns></returns>
        public int[] GetSpriteData(int id, int? cps = null)
        {
            _pos = Utilities.CalculatePosition(id, Columns);

            // _pixelData = GetPixels(_pos.X * 8, _pos.Y * 8, _spriteSize.X, _spriteSize.Y);
            _tmpPixelData = Utilities.GetPixels(PixelData, _pos.X * _spriteSize.X, _pos.Y * _spriteSize.Y, _spriteSize.X, _spriteSize.Y);
            // If there is a CPS cap, we need to go through all the pixels and make sure they are in range.
            if (cps.HasValue)
            {
                _colorIDs.Clear();

                for (int i = 0; i < _tmpPixelData.Length; i++)
                {
                    _colorId = _tmpPixelData[i];

                    if (_colorId > -1 && _colorIDs.IndexOf(_colorId) == -1)
                    {
                        if (_colorIDs.Count < cps.Value)
                        {
                            _colorIDs.Add(_colorId);
                        }
                        else
                        {
                            _tmpPixelData[i] = -1;
                        }
                    }
                }
            }

            // Return the new sprite image
            return _tmpPixelData;
        }

        public void WriteSpriteData(int id, int[] pixels)
        {
            // The total sprite pixel size should be cached
            if (pixels.Length != _spriteSize.X * _spriteSize.Y)
                return;

            // Make sure we stay in bounds
            id = Utilities.Clamp(id, 0, TotalSprites - 1);

            var pos = Utilities.CalculatePosition(id, Columns);
            pos.X *= _spriteSize.X;
            pos.Y *= _spriteSize.Y;

            Utilities.SetPixels(pixels, pos.X, pos.Y, _spriteSize.X, _spriteSize.Y, PixelData);
        }

        public int[] GetPixels() => Utilities.GetPixels(PixelData);

        public void Resize(int newWidth, int newHeight) => Utilities.Resize(PixelData, newWidth, newHeight);

        public void Clear(int color = -1) => Utilities.Clear(PixelData, color);
    }
}