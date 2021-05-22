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

using Microsoft.Xna.Framework.Input;

namespace PixelVision8.Player
{
    public partial class Scene
    {
        
        #region GameChip
        
        public int BackgroundColor(int? id = null) => _gameChip.BackgroundColor(id);

        public string Color(int id, string value = null) => _gameChip.Color(id, value);

        public int TotalColors(bool ignoreEmpty = false) => _gameChip.TotalColors(ignoreEmpty);

        public void ReplaceColor(int index, int id) => _gameChip.ReplaceColor(index, id);

        public Point Display() => _gameChip.Display();

        #endregion


        #region Draw
        public void DrawPixels
        (
            int[] pixelData, 
            int x, 
            int y, 
            int blockWidth, 
            int blockHeight, 
            bool flipH = false, 
            bool flipV = false, 
            DrawMode drawMode = DrawMode.Sprite, 
            int colorOffset = 0
        ) => _gameChip.DrawPixels(pixelData, x, y, blockWidth, blockHeight, flipH, flipV, drawMode, colorOffset);
        
        public void DrawSprite
        (
            int id, 
            int x, 
            int y, 
            bool flipH = false,
            bool flipV = false,
            DrawMode drawMode = DrawMode.Sprite, 
            int colorOffset = 0, 
            SpriteChip srcChip = null
        ) => _gameChip.DrawSprite(id, x, y, flipH, flipV, drawMode, colorOffset, srcChip);
        
        public void DrawRect
        (
            int x, 
            int y, 
            int width, 
            int height, 
            int color = -1,
            DrawMode drawMode = DrawMode.TilemapCache
        ) => _gameChip.DrawRect(x, y, width, height, color, drawMode);
        
        public void DrawTilemap
        (
            int x = 0, 
            int y = 0, 
            int columns = 0, 
            int rows = 0, 
            int? offsetX = null,
            int? offsetY = null
        ) => _gameChip.DrawTilemap(x, y, columns, rows, offsetX, offsetY);
                
        #endregion

        #region Factories

        public Canvas NewCanvas( int width, int height ) => _gameChip.NewCanvas(width, height);

        public Rectangle NewRect(int x = 0, int y = 0, int w = 0, int h = 0) => _gameChip.NewRect(x, y, w, h);

        public Point NewPoint(int x = 0, int y = 0) => _gameChip.NewPoint(x, y);

        public SpriteData NewSpriteData(int id, int x = 0, int y = 0, bool flipH = false, bool flipV = false, int colorOffset = 0) => _gameChip.NewSpriteData(id, x, y, flipH, flipV, colorOffset);

        public SpriteCollection NewSpriteCollection(string name, SpriteData[] sprites = null) => _gameChip.NewSpriteCollection(name, sprites);

        public SpriteCollection NewMetaSprite(int id, string name, int[] spriteIDs, int width, int colorOffset = 0) => _gameChip.NewMetaSprite(id, name, spriteIDs, width, colorOffset);

        #endregion

        #region Input

        public bool Button(Buttons button, InputState state = InputState.Down, int controllerID = 0) => _gameChip.Button(button, state, controllerID);

        #endregion

        #region Keyboard

        public bool Key(Keys key, InputState state = InputState.Down) => _gameChip.Key(key, state);

        public string InputString() => _gameChip.InputString();

        #endregion

        #region Math
        
        public int Clamp(int val, int min, int max) => _gameChip.Clamp(val, min, max);

        public int Repeat(int val, int max) => _gameChip.Repeat(val, max);

        public int CalculateIndex(int x, int y, int width) => _gameChip.CalculateIndex(x, y, width);

        public Point CalculatePosition(int index, int width) => _gameChip.CalculatePosition(index, width);

        public int CalculateDistance(int x0, int y0, int x1, int y1) => _gameChip.CalculateDistance(x0, y0, x1, y1);

        #endregion

        #region MetaSprite

        public SpriteCollection MetaSprite(string name, SpriteCollection spriteCollection = null) => _gameChip.MetaSprite(name, spriteCollection);
        
        public SpriteCollection MetaSprite(int id, SpriteCollection spriteCollection = null) => _gameChip.MetaSprite(id, spriteCollection);

        public int FindMetaSpriteId(string name) => _gameChip.FindMetaSpriteId(name);

        public void DrawMetaSprite(string name, int x, int y, bool flipH = false, bool flipV = false, DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0) => _gameChip.DrawMetaSprite(name, x, y, flipH, flipV, drawMode, colorOffset);

        public void DrawMetaSprite(SpriteCollection spriteCollection, int x, int y, bool flipH = false, bool flipV = false, DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0) => _gameChip.DrawMetaSprite(spriteCollection, x, y, flipH, flipV, drawMode, colorOffset);

        #endregion

        #region Mouse

        public bool MouseButton(int button, InputState state = InputState.Down) => _gameChip.MouseButton(button, state);

        public Point MouseWheel() => _gameChip.MouseWheel();

        public Point MousePosition() => _gameChip.MousePosition();

        #endregion

        #region Sound

        public void PlaySound(int id, int channel = 0) => _gameChip.PlaySound(id, channel);

        public void StopSound(int channel = 0) => _gameChip.StopSound(channel);

        public bool IsChannelPlaying(int channel) => _gameChip.IsChannelPlaying(channel);


        #endregion

        #region Sprites

        public void ChangeSizeMode(SpriteSizes mode) => _gameChip.ChangeSizeMode(mode);

        public Point SpriteSize() => _gameChip.SpriteSize();

        public int ColorsPerSprite() => _gameChip.ColorsPerSprite();

        public int PaletteOffset(int paletteId, int paletteColorId = 0) => _gameChip.PaletteOffset(paletteId, paletteColorId);

        public int[] Sprite(int id, int[] data = null) => _gameChip.Sprite(id, data);

        #endregion

        #region Text

        public int[] ConvertTextToSprites(string text, string fontName = "default") => _gameChip.ConvertTextToSprites(text, fontName);

        public int[] FontChar(char character, string fontName, int[] data = null) => _gameChip.FontChar(character, fontName, data);

        public void DrawText(string text, int x, int y, DrawMode drawMode = DrawMode.Sprite, string font = "large",
            int colorOffset = 0, int spacing = 0) => _gameChip.DrawText(text, x, y, drawMode, font, colorOffset, spacing);
        
        #endregion

        #region Tilemap

        public Point ScrollPosition(int? x = null, int? y = null) => _gameChip.ScrollPosition(x, y);
        
        public int Flag(int column, int row, int? value = null) => _gameChip.Flag(column, row, value);

        public TileData Tile(int column, int row, int? spriteId = null, int? colorOffset = null, int? flag = null,
            bool? flipH = null, bool? flipV = null) => _gameChip.Tile(column, row, spriteId, colorOffset, flag, flipH, flipV);

        public Point TilemapSize(int? width = null, int? height = null, bool clear = false) => _gameChip.TilemapSize(width, height, clear);

        public void UpdateTiles(int[] ids, int? colorOffset = null, int? flag = null) => _gameChip.UpdateTiles(ids, colorOffset, flag);



        #endregion

        #region Utils

        public string WordWrap(string text, int width) => _gameChip.WordWrap(text, width);

        public string[] SplitLines(string str) => _gameChip.SplitLines(str);
        
        public int[] BitArray(int value) => _gameChip.BitArray(value);

        #endregion

    }

}