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
    
    public partial class GameChip
    {
        // TODO this shares tmpSpriteData with GameChip_Display

        protected SpriteChip SpriteChip => Player.SpriteChip;
        protected int SpriteWidth = Constants.SpriteSize;
        protected int SpriteHeight = Constants.SpriteSize;
        
        private int[] _tmpSpriteData = new int[64];

        private Point[] _spriteSizes = new[]
        {
            new Point(1, 1),
            new Point(1, 2),
            new Point(2, 1),
            new Point(2, 2),
            new Point(4, 4),
        };
        
        public void ChangeSizeMode(SpriteSizes mode)
        {
            SpriteWidth = _spriteSizes[(int) mode].X * Constants.SpriteSize;
            SpriteHeight = _spriteSizes[(int) mode].Y * Constants.SpriteSize;
        }

        /// <summary>
        ///     Returns the size of the sprite as a Vector where X and Y represent the width and height.
        /// </summary>
        /// <param name="width">
        ///     Optional argument to change the width of the sprite. Currently not enabled.
        /// </param>
        /// <param name="height">
        ///     Optional argument to change the height of the sprite. Currently not enabled.
        /// </param>
        /// <returns>
        ///     Returns a vector where the X and Y for the sprite's width and height.
        /// </returns>
        public Point SpriteSize()
        {
            
            // We return a new point because we don't want any external code to modify the internal sprite size.
            return new Point(SpriteWidth, SpriteHeight);
        }
        
        /// <summary>
        ///     Pixel Vision 8 sprites have limits around how many colors they can display at once which is called
        ///     the Colors Per Sprite or CPS. The ColorsPerSprite() method returns this value from the SpriteChip.
        ///     While this is read-only at run-time, it has other important uses. If you set up your ColorChip in
        ///     palettes, grouping sets of colors together based on the SpriteChip's CPS value, you can use this to
        ///     shift a sprite's color offset up or down by a fixed amount when drawing it to the display. Since this
        ///     value does not change when a game is running, it is best to get a reference to it when the game starts
        ///     up and store it in a local variable.
        /// </summary>
        /// <returns>
        ///     This method returns the Color Per Sprite limit value as an int.
        /// </returns>
        public int ColorsPerSprite()
        {
            // This can not be changed at run time so it will never need to be invalidated
            return SpriteChip.ColorsPerSprite; //colorsPerSpriteCached;//;
        }

        /// <summary>
        ///     This method will automatically calculate the start color offset for palettes in the color chip.
        /// </summary>
        /// <param name="paletteId">The palette number, 1 - 8</param>
        /// <param name="paletteColorId"></param>
        /// <returns></returns>
        public int PaletteOffset(int paletteId, int paletteColorId = 0)
        {
            // TODO this is hardcoded right now but there are 8 palettes with a max of 16 colors each
            return 128 + Utilities.Clamp(paletteId, 0, 7) * 16 +
                   Utilities.Clamp(paletteColorId, 0, ColorsPerSprite() - 1);
        }

        #region Sprite

        

        /// <summary>
        ///     This allows you to return the pixel data of a sprite or overwrite it with new data. Sprite
        ///     pixel data is an array of color reference ids. When calling the method with only an id
        ///     argument, you will get the sprite's pixel data. If you supply data, it will overwrite the
        ///     sprite. It is important to make sure that any new pixel data should be the same length of
        ///     the existing sprite's pixel data. This can be calculated by multiplying the sprite's width
        ///     and height. You can add the transparent area to a sprite's data by using -1.
        /// </summary>
        /// <param name="id">
        ///     The sprite to access.
        /// </param>
        /// <param name="data">
        ///     Optional data to write over the sprite's current pixel data.
        /// </param>
        /// <returns>
        ///     Returns an array of int data which points to color ids.
        /// </returns>
        public int[] Sprite(int id, int[] data = null)
        {
            if (data != null)
            {
                SpriteChip.UpdateSpriteAt(id, data);

                TilemapChip.InvalidateTileId(id);

                return data;
            }

            SpriteChip.ReadSpriteAt(id, ref _tmpSpriteData);

            return _tmpSpriteData;
        }

        #endregion
    }
}