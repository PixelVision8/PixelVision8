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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PixelVision8.Runner

{
    
    public class FileParser : Attribute
    {
        public string FileType;
        public MethodInfo MethodInfo;
        public FileFlags FileFlag;
            
        public FileParser(string fileType, FileFlags fileFlag)
        {
            FileType = fileType;
            FileFlag = fileFlag;
        }
    }
    
    public partial class Loader
    {

        public int currentStep;

        protected readonly List<IParser> parsers = new List<IParser>();
        protected int currentParserID;
        public bool Completed => currentParserID >= TotalParsers;
        public float Percent => TotalSteps == 0 ? 1f : currentStep / (float) TotalSteps;

        public int TotalParsers => parsers.Count;

        public int TotalSteps;
        private readonly IFileLoader _fileLoadHelper;

        public List<FileParser> ParserMapping = new List<FileParser>();

        private IImageParser _imageParser;

        public Loader(IFileLoader fileLoadHelper, IImageParser imageParser)
        {
            _fileLoadHelper = fileLoadHelper;
            _imageParser = imageParser;

            var methods = GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(FileParser), false).Length > 0)
                .ToArray();

            for (int i = 0; i < methods.Length; i++)
            {
                // Console.WriteLine("Method " + i + " " + methods[i].Name);

                Type thisType = this.GetType();
                MethodInfo theMethod = thisType.GetMethod(methods[i].Name);

                // Get the File Parser attribute
                var attributes = theMethod.GetCustomAttribute(typeof(FileParser)) as FileParser;

                // Cache the method info on the attribute instance
                attributes.MethodInfo = theMethod;

                ParserMapping.Add(attributes);
            }

        }

        public void Reset()
        {
            parsers.Clear();
            currentParserID = 0;
            TotalSteps = 0;
            currentStep = 0;
        }

        public void AddParser(IParser parser)
        {
            parser.CalculateSteps();

            parsers.Add(parser);

            TotalSteps += parser.TotalSteps;
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

            if (parser.Completed)
            {
                parser.Dispose();
                currentParserID++;
            }
        }

        public virtual void ParseFiles(string[] files, PixelVision engine)
        {
            Reset();

            var values = Enum.GetValues(typeof(FileFlags)).Cast<FileFlags>().ToArray();

            for (var i = 0; i < values.Length; i++)
            {
                var parserInfo = ParserMapping.FirstOrDefault(p => p.FileFlag == values[i]);
                
                if(parserInfo == null)
                    continue;
                
                var filesToParse = files.Where(f => f.EndsWith(parserInfo.FileType)).ToArray();
                
                if (filesToParse.Length > 0)
                {
                    for (int j = 0; j < filesToParse.Length; j++)
                    {
                        parserInfo.MethodInfo.Invoke(this, new object[] {filesToParse[j], engine});
                    }
                }
            }
        }
    }
}