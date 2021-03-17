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
    // /// <summary>
    ///     The GameChip represents the foundation of a game class
    ///     with all the logic it needs to work correctly in the PixelVisionEngine.
    ///     The AbstractChip class manages configuring the game when created via the
    ///     chip life-cycle. The engine manages the game's state, the game's own life-cycle and
    ///     serialization/deserialization of the game's data.
    /// </summary>
    public partial class GameChip
    {
        #region Factories

        /// <summary>
        ///     A Rect is a Pixel Vision 8 primitive used for defining the bounds of an object on the display. It
        ///     contains an x, y, width and height property. The Rect class also has some additional methods to aid with
        ///     collision detection such as Intersect(rect, rect), IntersectsWidth(rect) and Contains(x,y).
        /// </summary>
        /// <param name="x">The x position of the rect as an int.</param>
        /// <param name="y">The y position of the rect as an int.</param>
        /// <param name="w">The width value of the rect as an int.</param>
        /// <param name="h">The height value of the rect as an int.</param>
        /// <returns>Returns a new instance of a Rect to be used as a Lua object.</returns>
        public Rectangle NewRect(int x = 0, int y = 0, int w = 0, int h = 0)
        {
            return new Rectangle(x, y, w, h);
        }

        /// <summary>
        ///     A Vector is a Pixel Vision 8 primitive used for defining a position on the display as an x,y value.
        /// </summary>
        /// <param name="x">The x position of the Vector as an int.</param>
        /// <param name="y">The y position of the Vector as an int.</param>
        /// <returns>Returns a new instance of a Vector to be used as a Lua object.</returns>
        public Point NewPoint(int x = 0, int y = 0)
        {
            return new Point(x, y);
        }

        public Canvas NewCanvas(int width, int height)
        {
            return new Canvas(width, height, this);
        }

        public SpriteData NewSpriteData(int id, int x = 0, int y = 0, bool flipH = false, bool flipV = false,
            int colorOffset = 0)
        {
            return new SpriteData(id, x, y, flipH, flipV, colorOffset);
        }

        public SpriteCollection NewSpriteCollection(string name, SpriteData[] sprites = null)
        {
            return new SpriteCollection(name, sprites);
        }

        public SpriteCollection NewMetaSprite(int id, string name, int[] spriteIDs, int width, int colorOffset = 0)
        {
            if (id < 0 || id > metaSprites.Length)
                return null;

            var collection = NewSpriteCollection(name);

            for (int i = 0; i < spriteIDs.Length; i++)
            {
                var pos = CalculatePosition(i, width);

                collection.AddSprite(spriteIDs[i], pos.X * SpriteChip.DefaultSpriteSize, pos.Y * SpriteChip.DefaultSpriteSize, false, false,
                    colorOffset);
            }

            // TODO need to figure out how to do this better where meta sprites 


            return MetaSprite(id, collection);
        }

        #endregion
    }
}