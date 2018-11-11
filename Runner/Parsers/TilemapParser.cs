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

using System.Collections.Generic;
using System.Linq;
using PixelVisionSDK;
using PixelVisionSDK.Chips;

namespace PixelVisionRunner.Parsers
{

    public class TilemapParser : SpriteParser
    {

        private readonly bool autoImport;
        private readonly TilemapChip tilemapChip;
        private ITexture2D tileFlagTex;
        private IColor clear;
        
        private int flag;
        private int offset;
        private int realWidth;
        private int realHeight;

        private ColorChip flagColorChip;

        public TilemapParser(ITextureFactory textureFactory, byte[] data, byte[] tileFlagData, IEngineChips chips) :
            base(textureFactory, data, chips)
        {
            //Debug.Log("Parse Tilemap");

            tilemapChip = chips.tilemapChip;


            if (tileFlagData != null)
            {
                tileFlagTex = textureFactory.NewTexture2D(1, 1);
    
                tileFlagTex.LoadImage(tileFlagData);
            }

        autoImport = tilemapChip.autoImport;
            
            flagColorChip = chips.chipManager.GetChip(FlagColorParser.flagColorChipName, false) as ColorChip;

            
            clear = new ColorData(0f){a = 0f};
            maskColor = new ColorData(chips.colorChip.maskColor);
            
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
                
                // TODO should this be part of the ITexture2D class?
                // Clean up pixel data by removing any transparent colors
                for (int i = 0; i < pixelData.Length; i++)
                {
                    if (pixelData[i].a < 1)
                    {
                        pixelData[i] = maskColor;
                    }
                }
                
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
            var color = tileFlagTex != null ? tileFlagTex.GetPixel(x, y) : clear;

            if (Equals(color, maskColor))
                color = clear;
            
            flag = color.a < 1 ? -1 : flagColorChip.FindColorID(ColorData.ColorToHex(color.r, color.g, color.b));// == 1 ? (int) (color.r * 256) / tilemapChip.totalFlags : -1));
            
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