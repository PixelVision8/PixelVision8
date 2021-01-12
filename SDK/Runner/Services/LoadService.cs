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

using Microsoft.Xna.Framework;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Services;
using PixelVision8.Engine.Utils;
using PixelVision8.Runner.Importers;
using PixelVision8.Runner.Parsers;
using PixelVision8.Runner.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace PixelVision8.Runner.Services
{


    public class LoadService : AbstractService
    {

        protected readonly List<IAbstractParser> parsers = new List<IAbstractParser>();

        protected int currentParserID;
        public int currentStep;
        protected BackgroundWorker loadingWorker;
        protected Color maskColor = ColorUtils.HexToColor("#ff00ff"); // TODO this shouldn't be hard coded 
        protected AbstractParser parser;

        public IEngine targetEngine;

        public List<string> textExtensions = new List<string>
        {
            ".txt",
            ".json",
            ".lua",
            ".cs"
        };

        protected int TotalParsers => parsers.Count;

        public int TotalSteps;
        private readonly IFileLoadHelper _fileLoadHelper;

        public LoadService(IFileLoadHelper fileLoadHelper)
        {
            _fileLoadHelper = fileLoadHelper;
        }

        public bool Completed => currentParserID >= TotalParsers;

        public float Percent => TotalSteps == 0 ? 1f : currentStep / (float)TotalSteps;

        /// <summary>
        ///     This can be used to display a message while preloading
        /// </summary>
        public string Message { get; protected set; }

        public void Reset()
        {
            parsers.Clear();
            currentParserID = 0;
            TotalSteps = 0;
            currentStep = 0;
        }


        public virtual void ParseFiles(string[] files, IEngine engine, SaveFlags saveFlags)
        {
            Reset();

            // Save the engine so we can work with it during loading
            targetEngine = engine;

            // Step 1. Load the system snapshot
            if ((saveFlags & SaveFlags.System) == SaveFlags.System) LoadSystem(files);


            // Step 3 (optional). Look for new colors
            if ((saveFlags & SaveFlags.Colors) == SaveFlags.Colors)
            {

                // Add the color parser
                parser = LoadColors(files);
                if (parser != null) AddParser(parser);
            }

            // Step 4 (optional). Look for color map for sprites and tile map
            if ((saveFlags & SaveFlags.ColorMap) == SaveFlags.ColorMap)
            {
                // TODO this is a legacy parcer and should be depricated
                parser = LoadColorMap(files);
                if (parser != null) AddParser(parser);

            }

            // Step 5 (optional). Look for new sprites
            if ((saveFlags & SaveFlags.Sprites) == SaveFlags.Sprites)
            {
                parser = LoadSprites(files);
                if (parser != null) AddParser(parser);
            }

            // Step 6 (optional). Look for tile map to load
            if ((saveFlags & SaveFlags.Tilemap) == SaveFlags.Tilemap) LoadTilemap(files);

            // Step 7 (optional). Look for fonts to load
            if ((saveFlags & SaveFlags.Fonts) == SaveFlags.Fonts)
            {

                // these are the defaul font names
                var defaultFonts = new string[]{
                    "large",
                    "medium",
                    "small",
                };
                
                // Get the list of fonts in the directory
                var paths = files.Where(s => s.EndsWith(".font.png")).ToList();

                // Make sure the default fonts are either in the project or in /App/Fonts/*
                foreach (var font in defaultFonts)
                {
                    if(paths.Contains("/Game/" + font + ".font.png") == false)
                    {
                        paths.Add("/App/Fonts/" + font + ".font.png");
                    }
                }

                // Loop through each of the fonts and load them up
                foreach (var fileName in paths)
                {
                    var imageParser = new PNGFileReader(fileName, _fileLoadHelper, targetEngine.ColorChip.maskColor);

                    AddParser(new FontParser(imageParser, targetEngine.ColorChip, targetEngine.FontChip));
                }
            }

            // Step 8 (optional). Look for meta data and override the game
            if ((saveFlags & SaveFlags.Meta) == SaveFlags.Meta)
            {
                parser = LoadMetaData(files);
                if (parser != null) AddParser(parser);
            }

            // Step 9 (optional). Look for meta data and override the game
            if ((saveFlags & SaveFlags.Sounds) == SaveFlags.Sounds)
            {
                LoadSounds(files);

                // Get all of the wav files
                var wavFiles = files.Where(x => x.EndsWith(".wav")).ToArray();

                if (wavFiles.Length > 0)
                    AddParser(new WavParser(wavFiles, _fileLoadHelper, targetEngine));
            }

            // Step 10 (optional). Look for meta data and override the game
            if ((saveFlags & SaveFlags.Music) == SaveFlags.Music) LoadMusic(files);

            // Step 11 (optional). Look for meta data and override the game
            if ((saveFlags & SaveFlags.SaveData) == SaveFlags.SaveData) LoadSaveData(files);

            // Step 12 (optional). Look for meta sprites
            if ((saveFlags & SaveFlags.MetaSprites) == SaveFlags.MetaSprites) LoadMetaSprites(files);

            ParseExtraFileTypes(files, engine, saveFlags);

        }

        public virtual void ParseExtraFileTypes(string[] files, IEngine engine, SaveFlags saveFlags)
        {
            // TODO Override and add extra file parsers here.
        }

        public void AddParser(IAbstractParser parser)
        {
            parser.CalculateSteps();

            parsers.Add(parser);

            TotalSteps += parser.totalSteps;
        }

        public void LoadAll()
        {
            while (Completed == false) NextParser();

            parsers.Clear();
        }

        public void NextParser()
        {
            if (Completed)
            {
                parsers.Clear();
                return;
            }

            var parser = parsers[currentParserID];

            parser.NextStep();

            currentStep++;

            if (parser.completed)
            {
                parser.Dispose();
                currentParserID++;
            }
        }

        public void StartLoading()
        {

            loadingWorker = new BackgroundWorker
            {
                // TODO need a way to of locking this.

                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };

            loadingWorker.DoWork += WorkerLoaderSteps;
            //            bgw.ProgressChanged += WorkerLoaderProgressChanged;
            loadingWorker.RunWorkerCompleted += WorkerLoaderCompleted;
            //            bgw.WorkerReportsProgress = true;
            loadingWorker.RunWorkerAsync();
        }

        protected void WorkerLoaderSteps(object sender, DoWorkEventArgs e)
        {

            for (var i = 0; i <= TotalSteps; i++) //some number (total)
            {
                NextParser();
                Thread.Sleep(1);
                loadingWorker.ReportProgress((int)(Percent * 100), i);
            }
        }

        protected void WorkerLoaderCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // TODO need a way to tell if this failed

            if (e.Error != null)
            {
                //                DisplayError(RunnerGame.ErrorCode.Exception, new Dictionary<string, string>(){{"@{error}","There was a error while loading. See the log for more information."}}, e.Error);
            }
        }

        protected AbstractParser LoadMetaData(string[] files)
        {

            var file = files.FirstOrDefault(x => x.EndsWith("info.json"));

            if (!string.IsNullOrEmpty(file))
            {
                // var fileContents = Encoding.UTF8.GetString(ReadAllBytes(file));

                return new MetaDataParser(file, _fileLoadHelper, ((PixelVisionEngine)targetEngine));
            }

            return null;
        }

        protected void LoadTilemap(string[] files)
        {

            // If a tilemap json file exists, try to load that
            var file = files.FirstOrDefault(x => x.EndsWith("tilemap.json"));

            if (!string.IsNullOrEmpty(file))
            {
                // var fileContents = Encoding.UTF8.GetString(ReadAllBytes(file));

                var jsonParser = new TilemapJsonParser(file, _fileLoadHelper, targetEngine);

                AddParser(jsonParser);

                return;
            }

            // Try to load the tilemap png file next
            file = files.FirstOrDefault(x => x.EndsWith("tilemap.png"));

            if (!string.IsNullOrEmpty(file))
            {

                var imageParser = new PNGFileReader(file, _fileLoadHelper, targetEngine.ColorChip.maskColor);
                AddParser(new TilemapParser(imageParser, targetEngine.ColorChip, targetEngine.SpriteChip, targetEngine.TilemapChip));

            }


        }

        protected AbstractParser LoadSprites(string[] files)
        {
            // // TODO need to tell if the cache should be ignore, important when in tools
            // var srcFile = "sprites.png";
            //
            // // TODO this in here to support legacy games but can be removed in future releases
            // var cacheFile = "sprites.cache.png";

            // string fileName = null;

            // TODO need to depricate this
            var file = files.FirstOrDefault(x => x.EndsWith("sprites.png"));

            // If there is no sprites cache file, load the png file instead
            // if (string.IsNullOrEmpty(file))
            // {
            //     file = files.FirstOrDefault(x => x.EndsWith("sprites.png"));
            // }

            if (!string.IsNullOrEmpty(file))
            {
                var imageParser = new PNGFileReader(file, _fileLoadHelper, targetEngine.ColorChip.maskColor);

                var colorChip = targetEngine.GetChip(ColorMapParser.chipName, false) is ColorChip colorMapChip
                    ? colorMapChip
                    : targetEngine.ColorChip;

                return new SpriteImageParser(imageParser, colorChip, targetEngine.SpriteChip);
            }

            return null;
        }

        protected AbstractParser LoadColorMap(string[] files)
        {
            // var fileName = "color-map.png";

            var file = files.FirstOrDefault(x => x.EndsWith("color-map.png"));

            if (!string.IsNullOrEmpty(file))
            {

                // Create new color map chip
                var colorMapChip = new ColorChip();

                // Add the chip to the engine
                targetEngine.ActivateChip(ColorMapParser.chipName, colorMapChip, false);

                //                targetEngine.colorMapChip = colorMapChip;

                var imageParser = new PNGFileReader(file, _fileLoadHelper, targetEngine.ColorChip.maskColor);

                // Pass the chip to the new parser
                return new ColorMapParser(imageParser, colorMapChip, maskColor);
            }

            return null;
        }

        protected AbstractParser LoadColors(string[] files)
        {
            // var fileName = "colors.png";


            var file = files.FirstOrDefault(x => x.EndsWith("colors.png"));

            if (!string.IsNullOrEmpty(file))
            {
                //                var tex = ReadTexture(ReadAllBytes(file));
                var imageParser = new PNGFileReader(file, _fileLoadHelper, targetEngine.ColorChip.maskColor);

                return new ColorParser(imageParser, targetEngine.ColorChip);
            }

            return null;
        }
        protected void LoadSystem(string[] files)
        {
            // var fileName = ;

            var file = files.FirstOrDefault(x => x.EndsWith("data.json"));

            if (!string.IsNullOrEmpty(file))
            {
                // var fileContents = Encoding.UTF8.GetString(ReadAllBytes(file));

                var jsonParser = new SystemParser(file, _fileLoadHelper, targetEngine);

                jsonParser.CalculateSteps();

                while (jsonParser.completed == false) jsonParser.NextStep();
            }

        }

        protected void LoadSounds(string[] files)
        {

            var file = files.FirstOrDefault(x => x.EndsWith("sounds.json"));


            if (!string.IsNullOrEmpty(file))
            {
                // var fileContents = Encoding.UTF8.GetString(ReadAllBytes(file));

                AddParser(new SystemParser(file, _fileLoadHelper, targetEngine));
            }

        }

        protected void LoadMusic(string[] files)
        {
            // var fileName = ;

            var file = files.FirstOrDefault(x => x.EndsWith("music.json"));


            if (!string.IsNullOrEmpty(file))
            {
                // var fileContents = Encoding.UTF8.GetString(ReadAllBytes(file));

                AddParser(new SystemParser(file, _fileLoadHelper, targetEngine));
            }
        }

        protected void LoadMetaSprites(string[] files)
        {
            // var fileName = ;
            var file = files.FirstOrDefault(x => x.EndsWith("meta-sprites.json"));

            if (!string.IsNullOrEmpty(file))
            {
                AddParser(new SystemParser(file, _fileLoadHelper, targetEngine));
            }
        }

        protected void LoadSaveData(string[] files)
        {

            var file = files.FirstOrDefault(x => x.EndsWith("saves.json"));

            if (!string.IsNullOrEmpty(file))
            {


                // var fileContents = Encoding.UTF8.GetString(ReadAllBytes(file));

                AddParser(new SystemParser(file, _fileLoadHelper, targetEngine));

            }
        }

    }
}