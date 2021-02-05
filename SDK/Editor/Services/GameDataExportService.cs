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

using PixelVision8.Player;
using PixelVision8.Runner.Exporters;
using PixelVision8.Runner;
using System.Collections.Generic;

namespace PixelVision8.Runner
{
    public class GameDataExportService : BaseExportService
    {
        private PixelVision targetEngine;

        public void ExportGame(string path, PixelVision engine, FileFlags fileFlags, bool useSteps = true)
        {
            Clear();

            // Save the engine so we can work with it during loading
            targetEngine = engine;

            // Step 1. Load the system snapshot
            if ((fileFlags & FileFlags.System) == FileFlags.System)
                AddExporter(new SystemExporter(path + "data.json", targetEngine));

            // Step 3 (optional). Look for new colors
            if ((fileFlags & FileFlags.Colors) == FileFlags.Colors)
            {
                var colorChip = targetEngine.ColorChip;

                AddExporter(new ColorPaletteExporter(path + "colors.png", colorChip, new PNGWriter()));
            }

            // Step 5 (optional). Look for new sprites
            if ((fileFlags & FileFlags.Sprites) == FileFlags.Sprites)
            {
                //                Console.WriteLine("Export Sprite");

                var imageExporter = new PNGWriter();

                AddExporter(new SpriteExporter(path + "sprites.png", targetEngine, imageExporter));
            }

            // Step 7 (optional). Look for fonts to load
            if ((fileFlags & FileFlags.Fonts) == FileFlags.Fonts)
            {
                var fontChip = targetEngine.FontChip;
                var spriteChip = targetEngine.SpriteChip;
                var tmpTextureData = new PixelData(96, 64);

                var fonts = fontChip.Fonts;

                foreach (var font in fonts)
                {
                    var name = font.Key;
                    var sprites = font.Value;

                    // Clear the texture
                    Utilities.Clear(tmpTextureData);
                    // tmpTextureData.Clear();

                    // Loop through all the characters and copy their texture data over
                    var total = sprites.Length;
                    for (var i = 0; i < total; i++)
                    {
                    }
                }
            }

            if ((fileFlags & FileFlags.Tilemap) == FileFlags.Tilemap)
                AddExporter(new TilemapJsonExporter(path + "tilemap.json", targetEngine));

            // Step 8 (optional). Look for meta data and override the game
            if ((fileFlags & FileFlags.Meta) == FileFlags.Meta)
                AddExporter(new MetadataExporter(path + "info.json", targetEngine));

            // Step 9 (optional). Look for meta data and override the game
            if ((fileFlags & FileFlags.Sounds) == FileFlags.Sounds)
                AddExporter(new SfxrSoundExporter(path + "sounds.json", targetEngine));

            // Step 10 (optional). Look for meta data and override the game
            if ((fileFlags & FileFlags.Music) == FileFlags.Music)
                AddExporter(new MusicExporter(path + "music.json", targetEngine));

            // Step 11 (optional). Look for meta data and override the game
            if ((fileFlags & FileFlags.SaveData) == FileFlags.SaveData)
                AddExporter(new SavedDataExporter(path + "saves.json", targetEngine));

            // Step 11 (optional). Look for meta data and override the game
            if ((fileFlags & FileFlags.MetaSprites) == FileFlags.MetaSprites)
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

        public void ExportSpriteBuilder(string path, PixelVision engine, Dictionary<string, byte[]> files)
        {
            Restart();
            // TODO need to create a new Sprite Builder Exporter
            AddExporter(new SpriteBuilderExporter(path, engine.ColorChip, engine.SpriteChip, files));
        }

        public void ExportImage(string path, int[] pixelData, PixelVision engine, int width, int height)
        {
            Restart();


            var imageExporter = new PNGWriter();

            // TODO need to double check that we should force this into debug so transparent images have the mask color in them by default
            var colors = DisplayTarget.ConvertColors(engine.ColorChip.HexColors, engine.ColorChip.MaskColor, true);


            AddExporter(new PixelDataExporter(path, pixelData, width, height, colors, imageExporter,
                engine.ColorChip.MaskColor));
        }
    }
}