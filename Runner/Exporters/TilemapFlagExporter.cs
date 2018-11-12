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

using System.Linq;
using PixelVisionRunner.Parsers;
using PixelVisionSDK;
using PixelVisionSDK.Chips;

namespace PixelVisionRunner.Exporters
{
    public class TilemapFlagExporter : TilemapExporter
    {
        
        public TilemapFlagExporter(string fileName, IEngine engine, IImageExporter imageExporter) : base(fileName, engine, imageExporter)
        {
            
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();
            
            // Remap the colors to the flag colors and remove the old color references
            var totalColors = 16;
            colors = new ColorData[totalColors];

            var flagColors = ((ColorChip) engine.chipManager.GetChip(FlagColorParser.flagColorChipName, false)).colors;
            
            for (int i = 0; i < totalColors; i++)
            {
                // TODO remved util but so now the convert flag methd is on the FlagTileExporter
                colors[i] = flagColors[i];//FlagTileExporter.ConvertFlagColor(i, totalColors);
            }
        }

        public override int[] GetPixelData(TileData tileData)
        {

            if (tileData.flag == -1)
            {
                return Enumerable.Repeat(-1, totalPixels).ToArray();
            }

            var colorID = tileData.flag;

            if (colorID < -1 || colorID > colors.Length-1)
                colorID = -1;
            
            return Enumerable.Repeat(colorID, totalPixels).ToArray();
            
        }
        
    }
}