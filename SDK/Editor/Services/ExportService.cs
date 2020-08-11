﻿//   
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
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Services;
using PixelVision8.Runner.Exporters;
using PixelVision8.Runner.Parsers;

namespace PixelVision8.Runner.Services
{
    public class ExportService : AbstractService
    {
        private readonly List<IAbstractExporter> exporters = new List<IAbstractExporter>();

//        private ITextureFactory textureFactory;
//        private readonly IAudioClipFactory audioClipFactory;

        private int currentParserID;

//        protected bool microSteps = true;
        protected int currentStep;
        private bool exporting;
        protected BackgroundWorker exportWorker;

        public Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();

        protected string message;
        private IEngine targetEngine;
        private int totalParsers;

        public int totalSteps;

        public bool completed => currentParserID >= totalParsers;

        protected float percent => currentStep / (float) totalSteps;


        public bool IsExporting()
        {
            return exporting;
        }

        // TODO need to make this work like loading does
        public int ReadExportPercent()
        {
            return (int) (percent * 100);
        }

        public string ReadExportMessage()
        {
            return message;
        }

        public void ExportGame(string path, IEngine engine, SaveFlags saveFlags)
        {
            Reset();

//            exporting = true;

            // Make sure that the texture factory has the current mask color if any images are going to be exported.
//            textureFactory.maskColor = new ColorData(engine.colorChip.maskColor);

//            Console.WriteLine("MASK COLOR " + engine.colorChip.maskColor);

//            Console.WriteLine("Export Game " + path);

            var watch = Stopwatch.StartNew();

            // Save the engine so we can work with it during loading
            targetEngine = engine;

            // Step 1. Load the system snapshot
            if ((saveFlags & SaveFlags.System) == SaveFlags.System)
                AddExporter(new SystemExporter(path + "data.json", targetEngine));

            // Step 2 (optional). Load up the Lua script
            if ((saveFlags & SaveFlags.Code) == SaveFlags.Code)
            {
//                //var scriptExtension = ".lua";
//                    
//                var paths = files.Keys.Where(s => textExtensions.Any(x => s.EndsWith(x))).ToList();
//
//                foreach (var fileName in paths)
//                {
//                    parser = LoadScript(fileName, files[fileName]);
//                    AddExporter(parser);
//                }
            }

            // Step 3 (optional). Look for new colors
            if ((saveFlags & SaveFlags.Colors) == SaveFlags.Colors)
            {
                var colorChip = targetEngine.colorChip;

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
//                var spriteChip = targetEngine.spriteChip;
//
//                var pixelData = spriteChip.texture.GetPixels();
//                
//                //TODO need to crop the pixel data so we only save out what we need
//                
//                AddExporter(new ImageExporter("sprite.cache.png", pixelData, spriteChip.textureWidth, spriteChip.textureHeight, targetEngine.colorChip.colors, textureFactory));
////
////                
//                parser = LoadSprites(files);
//                if (parser != null)
//                    AddExporter(parser);
            }

            // Step 6 (optional). Look for tile map to load
//            if ((saveFlags & SaveFlags.Tilemap) == SaveFlags.Tilemap)
//            {
//                var imageExporter = new PNGWriter();
//                
//                var tmp = new TilemapExporter(path + "tilemap.png", targetEngine, imageExporter);
//                AddExporter(tmp);
//            }
//            
//            if ((saveFlags & SaveFlags.TilemapFlags) == SaveFlags.TilemapFlags)
//            {
//                var imageExporter = new PNGWriter();
//
//                AddExporter(new TilemapFlagExporter(path + "tilemap-flags.png", targetEngine, imageExporter));
//            }

//            if ((saveFlags & SaveFlags.FlagColors) == SaveFlags.FlagColors)
//            {
//                AddExporter(new FlagColorExporter(path + "flags.png", targetEngine, textureFactory));
//                //AddExporter(new FlagTileExporter(path + "flags.png", targetEngine, textureFactory));
//            }

//            if ((saveFlags & SaveFlags.TileColorOffset) == SaveFlags.TileColorOffset)
//            {
//                AddExporter(new TileColorOffsetExporter(path + "tile-color-offsets.json", targetEngine));
//            }

//
            // Step 7 (optional). Look for fonts to load
            if ((saveFlags & SaveFlags.Fonts) == SaveFlags.Fonts)
            {
                var fontChip = targetEngine.fontChip;
                var spriteChip = targetEngine.spriteChip;
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


//                var fontExtension = ".font.png";
//
//                var paths = files.Keys.Where(s => s.EndsWith(fontExtension)).ToArray();
//
//                foreach (var fileName in paths)
//                {
//                    var fontName = fileName.Split('.')[0];
//
//                    parser = LoadFont(fontName, files[fileName]);
//                    if (parser != null)
//                        AddExporter(parser);
//                }
            }
//


//                if ((saveFlags & SaveFlags.TilemapFlags) == SaveFlags.TilemapFlags)
//                {
////                    ExportUtil.CreateTileMapFlagTexture(ref tex, engine);
////                    SaveTextureToFile(path, "tilemap-flags", tex);
//                }
//    
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

            totalParsers = exporters.Count;
            currentParserID = 0;

            watch.Stop();

//            UnityEngine.Debug.Log("Game Exporter Setup Time - " + watch.ElapsedMilliseconds);
        }

        public void AddExporter(IAbstractExporter exporter)
        {
            // Calculate the steps for the exporter
            exporter.CalculateSteps();

            exporters.Add(exporter);

            totalSteps += exporter.totalSteps;
        }

        public void ExportSong(string path, MusicChip musicChip, SoundChip soundChip, int[] patterns)
        {
            Reset();

            // TODO this should just use the active engine?
//            var name = targetEngine.musicChip.activeSongData.songName;
//            UnityEngine.Debug.Log("Export Song " + path);
            // TODO need to add a base path to the file name so we can create folders.
            AddExporter(new SongExporter(path, musicChip, soundChip, patterns));
        }

        public void ExportSpriteBuilder(string path, IEngine engine, Dictionary<string, byte[]> files)
        {
            Reset();
            // TODO need to create a new Sprite Builder Exporter
            AddExporter(new SpriteBuilderExporter(path, engine, files));
        }

        public void ExportImage(string path, int[] pixelData, IEngine engine, int width, int height)
        {
            Reset();


            var imageExporter = new PNGWriter();

            AddExporter(new PixelDataExporter(path, pixelData, width, height, engine.colorChip.colors, imageExporter,
                engine.colorChip.maskColor));
        }

        public void StartExport(bool useSteps = true)
        {
            // TODO saving should be fast enough to do without threading
//            useSteps = false;

            if (useSteps == false)
            {
                while (completed == false) NextExporter();

                WorkerExporterCompleted(null, null);
            }
            else
            {
                exportWorker = new BackgroundWorker();

                // TODO need a way to of locking this.

                exportWorker.WorkerSupportsCancellation = true;
                exportWorker.WorkerReportsProgress = true;


//                Console.WriteLine("Start export " + exportService.totalSteps + " steps");

                exportWorker.DoWork += WorkerExportSteps;
//            bgw.ProgressChanged += WorkerLoaderProgressChanged;
                exportWorker.RunWorkerCompleted += WorkerExporterCompleted;

//            bgw.WorkerReportsProgress = true;
                exportWorker.RunWorkerAsync();

                exporting = true;
            }
        }

        private void WorkerExportSteps(object sender, DoWorkEventArgs e)
        {
//            var result = e.Result;

            var total = totalSteps; //some number (this is your variable to change)!!

            for (var i = 0; i <= total; i++) //some number (total)
            {
                try
                {
                    NextExporter();
                }
                catch
                {
//                    DisplayError(RunnerGame.ErrorCode.Exception, new Dictionary<string, string>(){{"@{error}",exception.Message}}, exception);
//                    throw;
                }

                Thread.Sleep(1);
                exportWorker.ReportProgress((int) (percent * 100), i);
            }
        }

        public void WorkerExporterCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (locator.GetService(typeof(WorkspaceService).FullName) is WorkspaceService workspaceService)
            {
                workspaceService.SaveExporterFiles(files);

                files.Clear();
            }

            exporting = false;
        }

        #region Main APIs

        public void ExportAll()
        {
            while (completed == false)
                NextExporter();
        }


        public void NextExporter()
        {
            if (completed) return;

            var parser = exporters[currentParserID];

            parser.NextStep();

            currentStep++;

            if (parser.completed)
            {
                currentParserID++;
                files.Add(parser.fileName, parser.bytes);
            }
        }

        public void Reset()
        {
            files.Clear();
            exporters.Clear();
            currentParserID = 0;
            totalSteps = 0;
            currentStep = 0;
        }

        #endregion
    }
}