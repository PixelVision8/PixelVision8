//   
// Copyright (c) Jesse Freeman. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) License. 
// See LICENSE file in the project root for full license information. 
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman - @JesseFreeman
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany

using PixelVisionSDK;
using PixelVisionSDK.Chips;

namespace PixelVisionRunner.Parsers
{

    public class TilemapParser : SpriteParser
    {

        private readonly bool autoImport;
        private readonly TilemapChip tilemapChip;
        private ITexture2D flagTex;
        private ITexture2D colorTex;
        private IColor clear;
        private IColor mask;
        
        private int flag;
        private int offset;
        private int realWidth;
        private int realHeight;
        
        public TilemapParser(ITexture2D tex, ITexture2D flagTex, ITexture2D colorTex, IEngineChips chips, bool autoImport = false) : base(tex, chips)
        {
            tilemapChip = chips.tilemapChip;
            this.flagTex = flagTex;
            this.autoImport = autoImport;
            this.colorTex = colorTex;
            
            clear = new ColorData{a = 0};
            mask = new ColorData(chips.colorChip.maskColor);

        }

        protected override void CalculateBounds()
        {
            
            // Calculate the texture's bounds
            base.CalculateBounds();
            
            width = width > tilemapChip.columns ? tilemapChip.columns : width;
            
            height = height > tilemapChip.rows ? tilemapChip.rows : height;

            realWidth = sWidth * width;
            realHeight = sHeight * height;
            
            // Recalculate total sprites
            totalSprites = width * height;

        }

        public override void PrepareSprites()
        {
            
            // Test to see if the tilemap image is larger than the tilemap chip can allow
            if (tex.GetPixels().Length != (realWidth * realHeight))
            {
                var newWidth = tex.width >= realWidth ? realWidth : tex.width;//Math.Min(tex.width, realWidth);
                var newHeight = tex.height >= realHeight ? realHeight : tex.height;// Math.Min(tex.width, realHeight);
//              
                // Need to resize the texture so we only parse what can fit into the tilemap chip's memory
                var pixelData = tex.GetPixels(0, 0, newWidth, newHeight);
                
                // Resize the texture
                tex.Resize(newWidth, newHeight);
                
                // Set the pixels back into the texture
                tex.SetPixels(0, 0, newWidth, newHeight, pixelData);
            }
            
            // Prepare the sprites
            base.PrepareSprites();
        }
        
        public override void CutOutSpriteFromTexture2D()
        {
            base.CutOutSpriteFromTexture2D();
            
            // Calculate flag value
            var color = flagTex != null ? flagTex.GetPixel(x, y) : clear;

            if (Equals(color, mask))
                color = clear;
            
            flag = color.a == 1 ? (int) (color.r * 256) / tilemapChip.totalFlags : -1;
            
            color = colorTex != null ? colorTex.GetPixel(x, y) : clear;

            offset = color.a == 1 ? (int) (color.r * 256) : 0;
            
        }
        
        protected override void ProcessSpriteData()
        {
            var id = spriteChip.FindSprite(spriteData);

            if (id == -1 && autoImport)
            {
                id = spriteChip.NextEmptyID();
                spriteChip.UpdateSpriteAt(id, spriteData);
            }

            x = index % width;
            y = index / width;
            
            // Update the tile data in the map
            tilemapChip.UpdateTileAt(id, x, y, flag, offset);

        }

    }

}