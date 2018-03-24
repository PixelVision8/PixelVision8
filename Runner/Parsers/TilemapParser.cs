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

        public TilemapParser(ITexture2D tex, ITexture2D flagTex, ITexture2D colorTex, IEngineChips chips, bool autoImport = false) : base(tex, chips)
        {
            tilemapChip = chips.tilemapChip;
            this.flagTex = flagTex;
            this.autoImport = autoImport;
            this.colorTex = colorTex;
            
            clear = new ColorData{a = 0};
            mask = new ColorData("ff00ff");

//            this.clear = colorFactory.clear;

//            CalculateSteps();
        }


        protected override void CalculateBounds()
        {
            
            // Calculate the texture's bounds
            base.CalculateBounds();
            
            
            // Need to calculate the tilemap chip's bounds not the texture
            width = Math.Min(width, tilemapChip.columns);
            height = Math.Min(height, tilemapChip.rows);
            
        }

        public override void PrepareSprites()
        {
            var realWidth = spriteChip.width * width;
            var realHeight = spriteChip.height * tilemapChip.rows;
            
            // Test to see if the tilemap image is larger than the tilemap chip can allow
            if (tex.GetPixels().Length > (realWidth * realHeight))
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
            
            //TODO there is a race condition here where this may fail.

//            x = index % width * sWidth;
//            y = index / width * sHeight;
//
//            // Flip Y position
////            y = tex.height - y - sHeight;
//
////            if (x + sWidth < tex.width && y + sHeight > tex.height)
////            {
////                Debug.Log("index " + index + " out of range");
////            }
////            
//            tmpPixels = tex.GetPixels(x, y, sWidth, sHeight);

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