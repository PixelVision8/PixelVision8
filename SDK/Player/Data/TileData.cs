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

namespace PixelVision8.Player
{
    public sealed class TileData : AbstractData
    {
        private int _colorOffset;
        private int _flag = -1;
        private bool _flipH;
        private bool _flipV;
        private int _spriteId;
        private int _index;

        public TileData(int index, int spriteId = -1, int colorOffset = 0, int flag = -1, bool flipH = false,
            bool flipV = false)
        {
            Index = index;
            SpriteId = spriteId;
            ColorOffset = colorOffset;
            Flag = flag;

            FlipH = flipH;
            FlipV = flipV;
            Invalidate();
        }

        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                Invalidate();
            }
        }

        public int SpriteId
        {
            get => _spriteId;
            set
            {
                _spriteId = value;
                Invalidate();
            }
        }

        public int ColorOffset
        {
            get => _colorOffset;
            set
            {
                _colorOffset = value;
                Invalidate();
            }
        }

        public int Flag
        {
            get => _flag;
            set
            {
                _flag = MathHelper.Clamp(value, -1, 255);
                Invalidate();
            }
        }

        public bool FlipH
        {
            get => _flipH;
            set
            {
                _flipH = value;
                Invalidate();
            }
        }


        public bool FlipV
        {
            get => _flipV;
            set
            {
                _flipV = value;
                Invalidate();
            }
        }

        public void Clear()
        {
            SpriteId = -1;
            ColorOffset = 0;
            Flag = 0;

            FlipH = false;
            FlipV = false;

            Invalidate();
        }
    }
}