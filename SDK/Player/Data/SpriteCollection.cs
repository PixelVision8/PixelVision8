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
using System;
using System.Collections.Generic;
using PixelVision8.Player;

namespace PixelVisionSDK.Player
{
    public struct SpriteData
    {
        public int Id;
        public int X;
        public int Y;
        public bool FlipH;
        public bool FlipV;
        public int ColorOffset;

        public SpriteData(int id, int x = 0, int y = 0, bool flipH = false, bool flipV = false, int colorOffset = 0)
        {
            Id = id;
            FlipH = flipH;
            FlipV = flipV;
            X = x;
            Y = y;
            ColorOffset = colorOffset;
        }
    }

    public class SpriteCollection
    {
        public readonly List<SpriteData> Sprites;

        public Rectangle Bounds;

        // public Rectangle MaxBoundary;
        public string Name;
        public int SpriteHeight;
        public int SpriteMax;

        public int SpriteWidth;

        public int Width => Bounds.Width;
        public int Height => Bounds.Height;

        public SpriteCollection(string name, SpriteData[] sprites = null)
        {
            Name = name;
            Sprites = new List<SpriteData>();
            Bounds = Rectangle.Empty;
            SpriteMax = 1024;
            // MaxBoundary = new Rectangle(0, 0, 128, 128);
            SpriteWidth = 8;
            SpriteHeight = 8;

            if (sprites != null)
                for (var i = 0; i < sprites.Length; i++)
                {
                    var sprite = sprites[i];
                    AddSprite(sprite.Id, sprite.X, sprite.Y, sprite.FlipH, sprite.FlipH, sprite.ColorOffset);
                }
        }

        public void AddSprite(int id, int x = 0, int y = 0, bool flipH = false, bool flipV = false, int colorOffset = 0)
        {
            // var newX = MathHelper.Clamp(x, MaxBoundary.X, MaxBoundary.Width);
            // var newY = MathHelper.Clamp(y, MaxBoundary.Y, MaxBoundary.Height);

            // TODO is there a way to abstract this without knowing the sprite size?
            Bounds.Width = Math.Max(Bounds.Width, x + SpriteWidth);
            Bounds.Height = Math.Max(Bounds.Height, y + SpriteHeight);

            Sprites.Add(new SpriteData(MathHelper.Clamp(id, 0, SpriteMax), x, y, flipH, flipV, colorOffset));
        }

        public void Clear()
        {
            Sprites.Clear();
        }

        // TODO need a ToString() method
    }
}