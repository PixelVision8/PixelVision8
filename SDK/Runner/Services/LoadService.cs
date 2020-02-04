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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Services;
using PixelVision8.Engine.Utils;
using PixelVision8.Runner.Importers;
using PixelVision8.Runner.Parsers;

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
            ".lua"
        };

        protected int TotalParsers => parsers.Count;

        public int TotalSteps;

        public bool Completed => currentParserID >= TotalParsers;

        public float Percent => currentStep / (float) TotalSteps;

        /// <summary>
        ///     This can be used to display a message while preloading
        /// </summary>
        public string message { get; protected set; }

        public void Reset()
        {
            parsers.Clear();
            currentParserID = 0;
            TotalSteps = 0;
            currentStep = 0;
        }


        public virtual void ParseFiles(Dictionary<string, string> files, IEngine engine, SaveFlags saveFlags)
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
                var fontExtension = ".font.png";

                var paths = files.Keys.Where(s => s.EndsWith(fontExtension)).ToArray();

                foreach (var fileName in paths)
                {
                    var fontName = fileName.Split('.')[0];

                    parser = LoadFont(fontName, ReadAllBytes(files[fileName]));
                    if (parser != null) AddParser(parser);
                }
            }

            // Step 8 (optional). Look for meta data and override the game
            if ((saveFlags & SaveFlags.Meta) == SaveFlags.Meta)
            {
                parser = LoadMetaData(files);
                if (parser != null) AddParser(parser);
            }

            // Step 9 (optional). Look for meta data and override the game
            if ((saveFlags & SaveFlags.Sounds) == SaveFlags.Sounds) LoadSounds(files);

            // Step 10 (optional). Look for meta data and override the game
            if ((saveFlags & SaveFlags.Music) == SaveFlags.Music) LoadMusic(files);

            // Step 11 (optional). Look for meta data and override the game
            if ((saveFlags & SaveFlags.SaveData) == SaveFlags.SaveData) LoadSaveData(files);

            // Step 12 (optional). Look for meta sprites
            if ((saveFlags & SaveFlags.MetaSprites) == SaveFlags.MetaSprites) LoadMetaSprites(files);

            ParseExtraFileTypes(files, engine, saveFlags);

        }

        public virtual void ParseExtraFileTypes(Dictionary<string, string> files, IEngine engine, SaveFlags saveFlags)
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
        }

        public void NextParser()
        {
            if (Completed) return;

            var parser = parsers[currentParserID];

            parser.NextStep();

            currentStep++;

            if (parser.completed) currentParserID++;
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
                loadingWorker.ReportProgress((int) (Percent * 100), i);
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

        protected AbstractParser LoadMetaData(Dictionary<string, string> files)
        {
            var fileName = "info.json";

            if (files.ContainsKey(fileName))
            {
                var fileContents = Encoding.UTF8.GetString(ReadAllBytes(files[fileName]));

                return new MetaDataParser(fileContents, targetEngine);
            }

            return null;
        }

        protected AbstractParser LoadFont(string fontName, byte[] data)
        {

            var imageParser = new PNGReader(data, targetEngine.ColorChip.maskColor);

            return new FontParser(imageParser, targetEngine, fontName);
        }

        protected void LoadTilemap(Dictionary<string, string> files)
        {
            var tilemapFile = "tilemap.png";
            var tilemapJsonFile = "tilemap.json";

            // If a tilemap json file exists, try to load that
            if (files.ContainsKey(tilemapJsonFile))
            {
                var fileContents = Encoding.UTF8.GetString(ReadAllBytes(files[tilemapJsonFile]));

                var jsonParser = new TilemapJsonParser(fileContents, targetEngine);

                AddParser(jsonParser);

                //                tilemapExists = true;
            }
            else if (files.ContainsKey(tilemapFile))
            {

                byte[] tileFlagTex = null;

                var imageParser = new PNGReader(ReadAllBytes(files[tilemapFile]), targetEngine.ColorChip.maskColor);
                AddParser(new TilemapParser(imageParser, tileFlagTex, targetEngine));

            }

        }

        protected AbstractParser LoadSprites(Dictionary<string, string> files)
        {
            // TODO need to tell if the cache should be ignore, important when in tools
            var srcFile = "sprites.png";

            // TODO this in here to support legacy games but can be removed in future releases
            var cacheFile = "sprites.cache.png";

            string fileName = null;

            if (files.ContainsKey(cacheFile))
                fileName = cacheFile;
            else if (files.ContainsKey(srcFile)) fileName = srcFile;

            if (fileName != null)
            {
                //                var tex = ReadTexture(ReadAllBytes(files[fileName]));
                var imageParser = new PNGReader(ReadAllBytes(files[fileName]), targetEngine.ColorChip.maskColor);

                return new SpriteParser(imageParser, targetEngine);
            }

            return null;
        }

        protected AbstractParser LoadColorMap(Dictionary<string, string> files)
        {
            var fileName = "color-map.png";

            if (files.ContainsKey(fileName))
            {
                
                // Create new color map chip
                var colorMapChip = new ColorChip();

                // Add the chip to the engine
                targetEngine.ActivateChip(ColorMapParser.chipName, colorMapChip, false);

                //                targetEngine.colorMapChip = colorMapChip;

                var imageParser = new PNGReader(ReadAllBytes(files[fileName]), targetEngine.ColorChip.maskColor);

                // Pass the chip to the new parser
                return new ColorMapParser(imageParser, colorMapChip, maskColor);
            }

            return null;
        }

        protected AbstractParser LoadColors(Dictionary<string, string> files)
        {
            var fileName = "colors.png";

            if (files.ContainsKey(fileName))
            {
                //                var tex = ReadTexture(ReadAllBytes(files[fileName]));
                var imageParser = new PNGReader(ReadAllBytes(files[fileName]), targetEngine.ColorChip.maskColor);

                return new ColorParser(imageParser, targetEngine.ColorChip);
            }

            return null;
        }
        protected void LoadSystem(Dictionary<string, string> files)
        {
            var fileName = "data.json";

            if (files.ContainsKey(fileName))
            {
                var fileContents = Encoding.UTF8.GetString(ReadAllBytes(files[fileName]));

                var jsonParser = new SystemParser(targetEngine, fileContents);

                jsonParser.CalculateSteps();

                while (jsonParser.completed == false) jsonParser.NextStep();
            }

        }

        protected void LoadSounds(Dictionary<string, string> files)
        {
            var fileName = "sounds.json";

            if (files.ContainsKey(fileName))
            {
                var fileContents = Encoding.UTF8.GetString(ReadAllBytes(files[fileName]));

                AddParser(new SystemParser(targetEngine, fileContents));
            }

            // TODO go through all wav files that were loaded and pass them off to the Sample parser
            var wavFiles =
                (from p in files where p.Key.EndsWith("wav") select p).ToDictionary(x => x.Key, x => (ReadAllBytes(x.Value)));

            AddParser(new WavParser(targetEngine, wavFiles));

        }

        protected void LoadMusic(Dictionary<string, string> files)
        {
            var fileName = "music.json";

            if (files.ContainsKey(fileName))
            {
                var fileContents = Encoding.UTF8.GetString(ReadAllBytes(files[fileName]));

                AddParser(new SystemParser(targetEngine, fileContents));
            }
        }

        protected void LoadMetaSprites(Dictionary<string, string> files)
        {
            var fileName = "meta-sprites.json";

            if (files.ContainsKey(fileName))
            {
                var fileContents = Encoding.UTF8.GetString(ReadAllBytes(files[fileName]));

                AddParser(new SystemParser(targetEngine, fileContents));
            }
        }

        protected void LoadSaveData(Dictionary<string, string> files)
        {
            var fileName = "saves.json";

            if (files.ContainsKey(fileName))
            {
                var fileContents = Encoding.UTF8.GetString(ReadAllBytes(files[fileName]));

                AddParser(new SystemParser(targetEngine, fileContents));
                
            }
        }


        public virtual byte[] ReadAllBytes(string file)
        {
            
            // TODO this should be a service
            return File.ReadAllBytes(file);
        }
    }
}