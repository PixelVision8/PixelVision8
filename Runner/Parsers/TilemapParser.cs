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

using System;
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

        private int flag;
        private int offset;

        public TilemapParser(ITexture2D tex, ITexture2D flagTex, ITexture2D colorTex, IEngineChips chips, IColorFactory colorFactory, bool autoImport = false) : base(tex, chips, colorFactory)
        {
            tilemapChip = chips.tilemapChip;
            this.flagTex = flagTex;
            this.autoImport = autoImport;
            this.colorTex = colorTex;
            this.clear = colorFactory.clear;
            
            CalculateSteps();
        }

        public override void PrepareSprites()
        {
            var realWidth = spriteChip.width * tilemapChip.columns;
            var realHeight = spriteChip.height * tilemapChip.rows;

            // Test to see if the tilemap image is larger than the tilemap chip can allow
            if (tex.GetPixels32().Length > (realWidth * realHeight))
            {
                var newWidth = Math.Min(tex.width, realWidth);
                var newHeight = Math.Min(tex.width, realHeight);
                
                // Need to resize the texture so we only parse what can fit into the tilemap chip's memory
                var pixelData = tex.GetPixels(0, 0, newWidth, Math.Min(tex.width, newHeight));
                
                // Resize the texture
                tex.Resize(realWidth, realHeight);
                
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