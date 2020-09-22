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
using Microsoft.Xna.Framework;

namespace PixelVision8.Engine
{
    
    public sealed class TileData : AbstractData
    {
        private int _colorOffset;
        private int _flag = -1;
        private bool _flipH;
        private bool _flipV;
        private int _spriteID;

        private int _index;

        public TileData(int index, int spriteID = -1, int colorOffset = 0, int flag = -1, bool flipH = false,
            bool flipV = false)
        {
            Index = index;
            this.spriteID = spriteID;
            this.colorOffset = colorOffset;
            this.flag = flag;

            this.flipH = flipH;
            this.flipV = flipV;
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

        public int spriteID
        {
            get => _spriteID;
            set
            {
                _spriteID = value;
                Invalidate();
            }
        }

        public int colorOffset
        {
            get => _colorOffset;
            set
            {
                _colorOffset = value;
                Invalidate();
            }
        }

        public int flag
        {
            get => _flag;
            set
            {
                _flag = MathHelper.Clamp(value, -1, 255);
                Invalidate();
            }
        }

        public bool flipH
        {
            get => _flipH;
            set
            {
                _flipH = value;
                Invalidate();
            }
        }


        public bool flipV
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
            spriteID = -1;
            colorOffset = 0;
            flag = 0;

            flipH = false;
            flipV = false;

            Invalidate();
        }

    }
}