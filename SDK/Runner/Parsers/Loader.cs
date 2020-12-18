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


using com.pixelvision8.lite.SDK.Runner.Parsers;
using Microsoft.Xna.Framework.Graphics;
using PixelVision8.Engine;
using PixelVision8.Runner.Parsers;

/* Unmerged change from project 'PixelVision8.CoreDesktop'
Before:
using PixelVision8.Runner.Utils;
After:
using PixelVision8.Runner.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
*/
using PixelVision8.Runner.Utils;
using System;
using System.Collections.Generic;

namespace PixelVision8.Runner.Services

/* Unmerged change from project 'PixelVision8.CoreDesktop'
Before:
{
    
    public class Loader
    {
        
        protected readonly List<IAbstractParser> parsers = new List<IAbstractParser>();
After:
{

    public class Loader
    {

        protected readonly List<IAbstractParser> parsers = new List<IAbstractParser>();
*/
{

    public class Loader
    {

        protected readonly List<IAbstractParser> parsers = new List<IAbstractParser>();
        protected int currentParserID;
        public bool Completed => currentParserID >= TotalParsers;

        protected int TotalParsers => parsers.Count;

        public int TotalSteps;
        private readonly IFileLoadHelper _fileLoadHelper;

        private GraphicsDevice _graphicsDevice;

        public Loader(IFileLoadHelper fileLoadHelper, GraphicsDevice graphicsDevice)
        {
            _fileLoadHelper = fileLoadHelper;
            _graphicsDevice = graphicsDevice;
        }

        public void Reset()
        {
            parsers.Clear();
            currentParserID = 0;
            TotalSteps = 0;
        }

        public virtual void ParseFiles(string[] files, IEngine engine)
        {
            Reset();

            List<string> wavs = new List<string>();

            Array.Sort(files);

            foreach (var file in files)
            {
                if (file.EndsWith("colors.png"))
                {

/* Unmerged change from project 'PixelVision8.CoreDesktop'
Before:
                    var imageParser = new PNGParser(file, _graphicsDevice, engine.ColorChip.maskColor);
                    
                    AddParser(new ColorParser(imageParser, engine.ColorChip));
After:
                    var imageParser = new PNGParser(file, _graphicsDevice, engine.ColorChip.maskColor);

                    AddParser(new ColorParser(imageParser, engine.ColorChip));
*/
                    var imageParser = new PNGParser(file, _graphicsDevice, engine.ColorChip.maskColor);

                    AddParser(new ColorParser(imageParser, engine.ColorChip));
                }
                // Look for sprites
                if (file.EndsWith("sprites.png"))
                {
                    var imageParser = new PNGParser(file, _graphicsDevice, engine.ColorChip.maskColor);

                    AddParser(new SpriteImageParser(imageParser, engine.ColorChip, engine.SpriteChip));
                }

                // Look for fonts
                else if (file.EndsWith(".font.png"))
                {
                    var imageParser = new PNGParser(file, _graphicsDevice, engine.ColorChip.maskColor);

                    AddParser(new FontParser(imageParser, engine.ColorChip, engine.FontChip));

                }
                // Look for tiles
                else if (file.EndsWith("tilemap.png"))
                {
                    var imageParser = new PNGParser(file, _graphicsDevice, engine.ColorChip.maskColor);

                    AddParser(new TilemapParser(imageParser, engine.ColorChip, engine.SpriteChip, engine.TilemapChip, true));

                }
                // Look for wavs
                else if (file.EndsWith(".wav"))
                {
                    wavs.Add(file);
                }

/* Unmerged change from project 'PixelVision8.CoreDesktop'
Before:
            }
            
            AddParser(new WavParser(wavs.ToArray(), _fileLoadHelper, engine ));
After:
            }

            AddParser(new WavParser(wavs.ToArray(), _fileLoadHelper, engine ));
*/
            }

            AddParser(new WavParser(wavs.ToArray(), _fileLoadHelper, engine));

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

            if (parser.completed)
            {
                parser.Dispose();
                currentParserID++;
            }
        }

    }
}