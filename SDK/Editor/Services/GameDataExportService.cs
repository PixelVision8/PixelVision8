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

using System.Collections.Generic;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Utils;
using PixelVision8.Runner.Exporters;
using PixelVision8.Runner.Parsers;

namespace PixelVision8.Runner.Services
{
    public class GameDataExportService : BaseExportService
    {
        
        private IEngine targetEngine;
        
        public void ExportGame(string path, IEngine engine, SaveFlags saveFlags, bool useSteps = true)
        {

            Clear();

            // Save the engine so we can work with it during loading
            targetEngine = engine;

            // Step 1. Load the system snapshot
            if ((saveFlags & SaveFlags.System) == SaveFlags.System)
                AddExporter(new SystemExporter(path + "data.json", targetEngine));

            // Step 3 (optional). Look for new colors
            if ((saveFlags & SaveFlags.Colors) == SaveFlags.Colors)
            {
                var colorChip = targetEngine.ColorChip;

                AddExporter(new ColorPaletteExporter(path + "colors.png", colorChip, new PNGWriter()));
            }

            // Step 4 (optional). Look for color map for sprites and tile map
            if ((saveFlags & SaveFlags.ColorMap) == SaveFlags.ColorMap)
                if (targetEngine.GetChip(ColorMapParser.chipName, false) is ColorChip colorChip)
                    AddExporter(new ColorPaletteExporter(path + "color-map.png", colorChip, new PNGWriter()));

            // Step 5 (optional). Look for new sprites
            if ((saveFlags & SaveFlags.Sprites) == SaveFlags.Sprites)
            {
                //                Console.WriteLine("Export Sprite");

                var imageExporter = new PNGWriter();

                AddExporter(new SpriteExporter(path + "sprites.png", targetEngine, imageExporter));
                
            }

            // Step 7 (optional). Look for fonts to load
            if ((saveFlags & SaveFlags.Fonts) == SaveFlags.Fonts)
            {
                var fontChip = targetEngine.FontChip;
                var spriteChip = targetEngine.SpriteChip;
                var tmpTextureData = new TextureData(96, 64);

                var fonts = fontChip.fonts;

                foreach (var font in fonts)
                {
                    var name = font.Key;
                    var sprites = font.Value;

                    // Clear the texture
                    tmpTextureData.Clear();

                    // Loop through all the characters and copy their texture data over
                    var total = sprites.Length;
                    for (var i = 0; i < total; i++)
                    {
                    }
                }
            }

            if ((saveFlags & SaveFlags.Tilemap) == SaveFlags.Tilemap)
                AddExporter(new TilemapJsonExporter(path + "tilemap.json", targetEngine));

            // Step 8 (optional). Look for meta data and override the game
            if ((saveFlags & SaveFlags.Meta) == SaveFlags.Meta)
                AddExporter(new MetadataExporter(path + "info.json", targetEngine));

            // Step 9 (optional). Look for meta data and override the game
            if ((saveFlags & SaveFlags.Sounds) == SaveFlags.Sounds)
                AddExporter(new SoundExporter(path + "sounds.json", targetEngine));

            // Step 10 (optional). Look for meta data and override the game
            if ((saveFlags & SaveFlags.Music) == SaveFlags.Music)
                AddExporter(new MusicExporter(path + "music.json", targetEngine));

            // Step 11 (optional). Look for meta data and override the game
            if ((saveFlags & SaveFlags.SaveData) == SaveFlags.SaveData)
                AddExporter(new SavedDataExporter(path + "saves.json", targetEngine));

            // Step 11 (optional). Look for meta data and override the game
            if ((saveFlags & SaveFlags.MetaSprites) == SaveFlags.MetaSprites)
                AddExporter(new MetaSpriteExporter(path + "meta-sprites.json", targetEngine));

            StartExport(useSteps);
        }

        public void ExportSong(string path, MusicChip musicChip, SoundChip soundChip, int[] patterns)
        {
            Restart();

            // TODO this should just use the active engine?
            //            var name = targetEngine.musicChip.activeSongData.songName;
            //            UnityEngine.Debug.Log("Export Song " + path);
            // TODO need to add a base path to the file name so we can create folders.
            AddExporter(new SongExporter(path, musicChip, soundChip, patterns));
        }

        public void ExportSpriteBuilder(string path, IEngine engine, Dictionary<string, byte[]> files)
        {
            Restart();
            // TODO need to create a new Sprite Builder Exporter
            AddExporter(new SpriteBuilderExporter(path, engine.ColorChip, engine.SpriteChip, files));
        }

        public void ExportImage(string path, int[] pixelData, IEngine engine, int width, int height)
        {
            Restart();


            var imageExporter = new PNGWriter();

            // TODO need to double check that we should force this into debug so transparent images have the mask color in them by default
            var colors = ColorUtils.ConvertColors(engine.ColorChip.hexColors, engine.ColorChip.maskColor, true);


            AddExporter(new PixelDataExporter(path, pixelData, width, height, colors, imageExporter,
                engine.ColorChip.maskColor));
        }

    }
}