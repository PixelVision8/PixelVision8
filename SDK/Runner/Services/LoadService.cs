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
using System.Diagnostics;
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
//        
//        public enum FileType
//        {
//            System,
//            Colors,
//            SystemColors,
//            Palettes,
//            ColorMap,
//            Sprites,
//            Tilemap,
//            Sounds,
//            Music
//            
//        }

        protected readonly List<IAbstractParser> parsers = new List<IAbstractParser>();

        protected int currentParserID;

//        public ITextureFactory textureFactory;
//        public ColorFactory colorFactory;
        public int currentStep;
        protected BackgroundWorker loadingWorker;
        protected Color maskColor = ColorUtils.HexToColor("#ff00ff"); // TODO this shouldn't be hard coded 

//        protected bool microSteps = true;
        protected AbstractParser parser;

        protected IEngine targetEngine;

        public List<string> textExtensions = new List<string>
        {
            ".txt",
            ".json",
            ".lua"
        };

        protected int totalParsers;
        public int totalSteps;

        public bool completed => currentParserID >= totalParsers;

        public float percent => currentStep / (float) totalSteps;

        /// <summary>
        ///     This can be used to display a message while preloading
        /// </summary>
        public string message { get; protected set; }

        public void Reset()
        {
            parsers.Clear();
            currentParserID = 0;
            totalSteps = 0;
            currentStep = 0;
        }

//        public LoadService()
//        {
////            this.textureFactory = textureFactory;
////            this.colorFactory = colorFactory;
//        }

        public virtual void ParseFiles(Dictionary<string, byte[]> files, IEngine engine, SaveFlags saveFlags)
        {
            Reset();

            var watch = Stopwatch.StartNew();

            // Save the engine so we can work with it during loading
            targetEngine = engine;

            // Step 1. Load the system snapshot
            if ((saveFlags & SaveFlags.System) == SaveFlags.System)
                LoadSystem(files);

            

            // Step 3 (optional). Look for new colors
            if ((saveFlags & SaveFlags.Colors) == SaveFlags.Colors)
            {
//                parser = LoadSystemColors(files);
//                if (parser != null)
//                    AddParser(parser);

                // Add the color parser
                parser = LoadColors(files);
                if (parser != null)
                    AddParser(parser);
            }

            // Step 4 (optional). Look for color map for sprites and tile map
            if ((saveFlags & SaveFlags.ColorMap) == SaveFlags.ColorMap)
            {
                // TODO this is a legacy parcer and should be depricated
                parser = LoadColorMap(files);
                if (parser != null)
                    AddParser(parser);

//                // This will be the new parser moving forward
//                parser = LoadColorPalette(files);
//                if(parser != null)
//                    AddParser(parser);

                // TODO need to rename SaveFlags.ColorMap to SaveFlags.ColorPalette
            }

            // Step 5 (optional). Look for new sprites
            if ((saveFlags & SaveFlags.Sprites) == SaveFlags.Sprites)
            {
                parser = LoadSprites(files);
                if (parser != null)
                    AddParser(parser);
            }


            // Step 6 (optional). Look for tile map to load
//            if ((saveFlags & SaveFlags.FlagColors) == SaveFlags.FlagColors) LoadFlagColors(files);

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

                    parser = LoadFont(fontName, files[fileName]);
                    if (parser != null)
                        AddParser(parser);
                }
            }

            // Step 8 (optional). Look for meta data and override the game
            if ((saveFlags & SaveFlags.Meta) == SaveFlags.Meta)
            {
                parser = LoadMetaData(files);
                if (parser != null)
                    AddParser(parser);
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

            totalParsers = parsers.Count;
            currentParserID = 0;

            watch.Stop();

//            UnityEngine.Debug.Log("Parser Setup Time - " + watch.ElapsedMilliseconds);
        }

        public virtual void ParseExtraFileTypes(Dictionary<string, byte[]> files, IEngine engine, SaveFlags saveFlags)
        {
            // TODO Override and add extra file parsers here.
        }

        public void AddParser(IAbstractParser parser)
        {
            parser.CalculateSteps();

            parsers.Add(parser);

            totalSteps += parser.totalSteps;
        }

        public void LoadAll()
        {
            while (completed == false) NextParser();
        }

        public void NextParser()
        {
            if (completed)
                return;

            var parser = parsers[currentParserID];

            parser.NextStep();

            currentStep++;

            if (parser.completed)
                currentParserID++;
        }

        public void StartLoading()
        {
//            loadService.LoadAll();

            loadingWorker = new BackgroundWorker();

            // TODO need a way to of locking this.

            loadingWorker.WorkerSupportsCancellation = true;
            loadingWorker.WorkerReportsProgress = true;


//            DisplayWarning("Start worker " +  loadService.totalSteps + " steps");

            loadingWorker.DoWork += WorkerLoaderSteps;
//            bgw.ProgressChanged += WorkerLoaderProgressChanged;
            loadingWorker.RunWorkerCompleted += WorkerLoaderCompleted;
//            bgw.WorkerReportsProgress = true;
            loadingWorker.RunWorkerAsync();
        }

        protected void WorkerLoaderSteps(object sender, DoWorkEventArgs e)
        {
//            var result = e.Result;

//            int total = loadService.totalSteps; //some number (this is your variable to change)!!

            for (var i = 0; i <= totalSteps; i++) //some number (total)
            {
                NextParser();
                Thread.Sleep(1);
                loadingWorker.ReportProgress((int) (percent * 100), i);
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

        protected AbstractParser LoadMetaData(Dictionary<string, byte[]> files)
        {
            var fileName = "info.json";

            if (files.ContainsKey(fileName))
            {
                var fileContents = Encoding.UTF8.GetString(files[fileName]);

                return new MetaDataParser(fileContents, targetEngine);
            }

            return null;
        }

        protected AbstractParser LoadFont(string fontName, byte[] data)
        {
//            var tex = ReadTexture(data);

            //var fontName = fileSystem.GetFileNameWithoutExtension(file);
            //fontName = fontName.Substring(0, fontName.Length - 5);

            var imageParser = new PNGReader(data, targetEngine.colorChip.maskColor);

            return new FontParser(imageParser, targetEngine, fontName);
        }

//        protected void LoadFlagColors(Dictionary<string, byte[]> files)
//        {
//            // First thing we do is check for any custom tilemap flag colors
//            byte[] flagTex = null;
//            var flags = "flags.png";
//
//            if (files.ContainsKey(flags)) flagTex = files[flags];
//
//            var imageParser = new PNGReader(flagTex, targetEngine.colorChip.maskColor);
//
//
//            // This will also create the custom flag color chip we need for parsing the tilemap later on
//            AddParser(new FlagColorParser(imageParser, targetEngine));
//        }

        protected void LoadTilemap(Dictionary<string, byte[]> files)
        {
            var tilemapFile = "tilemap.png";
            var tilemapJsonFile = "tilemap.json";
//            var colorOffsetFile = "tile-color-offsets.json";

            // TODO should this be manually called?
            // Make sure we have the flag color chip
//            LoadFlagColors(files);

//            var tilemapExists = false;

            // If a tilemap json file exists, try to load that
            if (files.ContainsKey(tilemapJsonFile))
            {
                var fileContents = Encoding.UTF8.GetString(files[tilemapJsonFile]);

                var jsonParser = new TilemapJsonParser(fileContents, targetEngine);

                AddParser(jsonParser);

//                tilemapExists = true;
            }
            else if (files.ContainsKey(tilemapFile))
            {
                // If a tilemap file exists, load that instead
//                var tex = ReadTexture(files[tilemapFile]);
                byte[] tileFlagTex = null;


//                var tileFlags = "tilemap-flags.png";
//
//
//                if (files.ContainsKey(tileFlags)) tileFlagTex = files[tileFlags];


                var imageParser = new PNGReader(files[tilemapFile], targetEngine.colorChip.maskColor);
                AddParser(new TilemapParser(imageParser, tileFlagTex, targetEngine));

//                var colorFile = "tile-color-offsets.json";
//
//                if (files.ContainsKey(colorFile))
//                {
//                    colorTex = ReadTexture(files[colorFile]);
//                }
//                tilemapExists = true;
            }

            // Always load the color offset parser
//            if (files.ContainsKey(colorOffsetFile) && tilemapExists)
//            {
//                var fileContents = Encoding.UTF8.GetString(files[colorOffsetFile]);
//                
//                AddParser(new TileColorOffsetJson(fileContents, targetEngine));
//            }


//            return null;
        }

        protected AbstractParser LoadSprites(Dictionary<string, byte[]> files)
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
//                var tex = ReadTexture(files[fileName]);
                var imageParser = new PNGReader(files[fileName], targetEngine.colorChip.maskColor);

                return new SpriteParser(imageParser, targetEngine);
            }

            return null;
        }

        protected AbstractParser LoadColorMap(Dictionary<string, byte[]> files)
        {
            var fileName = "color-map.png";

            if (files.ContainsKey(fileName))
            {
//                UnityEngine.Debug.Log("Create color map");

//                var tex = ReadTexture(files[fileName]);

                // Create new color map chip
                var colorMapChip = new ColorChip();

                // Add the chip to the engine
                targetEngine.ActivateChip(ColorMapParser.chipName, colorMapChip, false);

//                targetEngine.colorMapChip = colorMapChip;

                var imageParser = new PNGReader(files[fileName], targetEngine.colorChip.maskColor);

                // Pass the chip to the new parser
                return new ColorMapParser(imageParser, colorMapChip, maskColor);
            }

            return null;
        }

//        protected AbstractParser LoadColorPalette(Dictionary<string, byte[]> files)
//        {
//            var fileName = "color-palette.png";
//
//            if (files.ContainsKey(fileName))
//            {
//                
////                UnityEngine.Debug.Log("Create color map");
//                
////                var tex = ReadTexture(files[fileName]);
//                
//                // Create new color map chip
//                var colorMapChip = new ColorChip();
//                
//                // Add the chip to the engine
//                targetEngine.chipManager.ActivateChip(ColorPaletteParser.chipName, colorMapChip, false);
//                
////                targetEngine.colorMapChip = colorMapChip;
//                
//                var imageParser = new PNGReader(files[fileName], targetEngine.colorChip.maskColor);
//
//                
//                // Pass the chip to the new parser
//                return new ColorPaletteParser(imageParser, colorMapChip, maskColor);
//            }
//
//            return null;
//        }

//        protected AbstractParser LoadSystemColors(Dictionary<string, byte[]> files)
//        {
//            var fileName = "system-colors.png";
//
//            if (files.ContainsKey(fileName))
//            {
//                
////                UnityEngine.Debug.Log("Create color map");
//                
////                var tex = ReadTexture(files[fileName]);
//                
//                // Create new color map chip
////                var systemColorChip = new ColorChip();
////                
////                // Add the chip to the engine
////                targetEngine.chipManager.ActivateChip("PixelVisionSDK.Chips.SystemColorChip", systemColorChip, false);
////                
////                targetEngine.colorMapChip = colorMapChip;
//                
//                var imageParser = new PNGReader(files[fileName], targetEngine.colorChip.maskColor);
//
//                
//                // Pass the chip to the new parser
//                return new SupportedColorParser(imageParser, targetEngine.colorChip, maskColor);
//            }
//
//            return null;
//        }

        protected AbstractParser LoadColors(Dictionary<string, byte[]> files)
        {
            var fileName = "colors.png";

            if (files.ContainsKey(fileName))
            {
//                var tex = ReadTexture(files[fileName]);
                var imageParser = new PNGReader(files[fileName], targetEngine.colorChip.maskColor);

                return new ColorParser(imageParser, targetEngine.colorChip);
            }

            return null;
        }

       

        protected void LoadSystem(Dictionary<string, byte[]> files)
        {
            var fileName = "data.json";

            if (files.ContainsKey(fileName))
            {
                var fileContents = Encoding.UTF8.GetString(files[fileName]);

//                AddParser(new SystemParser(fileContents, targetEngine));
                var jsonParser = new SystemParser(targetEngine, fileContents);

                jsonParser.CalculateSteps();

                while (jsonParser.completed == false)
                    jsonParser.NextStep();
            }

//            else
//            {
//                throw new Exception("Can't find 'data.json' file");
//            }
        }

//        public ITexture2D ReadTexture(byte[] data)
//        {
//
////            var tmpTxt = new PNGAdaptor(((TextureFactory) textureFactory).graphicsDevice);
////            tmpTxt.LoadImage(data);
//            
//            // Create a texture to store data in
//            var tex = textureFactory.NewTexture2D(1, 1);
//
//            // Load bytes into texture
//            tex.LoadImage(data);
//            
////            Console.WriteLine("Texture Test " + tmpTxt.width + "x" + tmpTxt.height + " " + tex.width +"x"+tex.height);
//            // Return texture
//            return tex;
//        }

        protected void LoadSounds(Dictionary<string, byte[]> files)
        {
            var fileName = "sounds.json";

            if (files.ContainsKey(fileName))
            {
                var fileContents = Encoding.UTF8.GetString(files[fileName]);

                AddParser(new SystemParser(targetEngine, fileContents));
            }

            // TODO go through all wav files that were loaded and pass them off to the Sample parser
            var wavFiles =
                (from p in files where p.Key.EndsWith("wav") select p).ToDictionary(x => x.Key, x => x.Value);

            AddParser(new WavParser(targetEngine, wavFiles));

//            Console.WriteLine("Selecting wavs " + wav.ToList().Count);
        }

        protected void LoadMusic(Dictionary<string, byte[]> files)
        {
            var fileName = "music.json";

            if (files.ContainsKey(fileName))
            {
                var fileContents = Encoding.UTF8.GetString(files[fileName]);

                AddParser(new SystemParser(targetEngine, fileContents));
            }
        }

        protected void LoadMetaSprites(Dictionary<string, byte[]> files)
        {
            var fileName = "meta-sprites.json";

            if (files.ContainsKey(fileName))
            {
                var fileContents = Encoding.UTF8.GetString(files[fileName]);

                AddParser(new SystemParser(targetEngine, fileContents));

            }
        }

        protected void LoadSaveData(Dictionary<string, byte[]> files)
        {
            var fileName = "saves.json";

            if (files.ContainsKey(fileName))
            {
                var fileContents = Encoding.UTF8.GetString(files[fileName]);

                AddParser(new SystemParser(targetEngine, fileContents));
//                
//                var jsonParser = new SystemParser(fileContents, targetEngine);
//                jsonParser.CalculateSteps();
//                
//                while (jsonParser.completed == false)
//                    jsonParser.NextStep();
            }
        }
    }
}