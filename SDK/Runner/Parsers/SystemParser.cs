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
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

namespace PixelVision8.Runner
{
    
    public class ChipParser : Attribute
    {
        public string ChipName;
        public MethodInfo MethodInfo;
            
        public ChipParser(string chipName)
        {
            ChipName = chipName;
        }
    }
    
    public partial class SystemParser : JsonParser
    {
        protected PixelVision Target;
        public List<ChipParser> ParserMapping = new List<ChipParser>();

        public SystemParser(string filePath, IFileLoader fileLoadHelper, PixelVision target) : base(filePath,
            fileLoadHelper)
        {
            Target = target;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();
            Steps.Add(FindParsers);
            Steps.Add(ApplySettings);
        }

        public void FindParsers()
        {
            var methods = GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(ChipParser), false).Length > 0)
                .ToArray();
            
            for (int i = 0; i < methods.Length; i++)
            {
                Type thisType = this.GetType();
                MethodInfo theMethod = thisType.GetMethod(methods[i].Name);

                // Get the File Parser attribute
                var attributes = theMethod.GetCustomAttribute(typeof(ChipParser)) as ChipParser;

                // Cache the method info on the attribute instance
                attributes.MethodInfo = theMethod;

                ParserMapping.Add(attributes);
            }
            
            StepCompleted();
        }
        

        public virtual void ApplySettings()
        {
            if (Target != null)
            {
                
                foreach (var entry in Data)
                {
                    var fullName = entry.Key;
                    var split = fullName.Split('.');
                    var chipName = split[split.Length - 1];
                    var chipData = entry.Value as Dictionary<string, object>;

                    var parserInfo = ParserMapping.FirstOrDefault(p => p.ChipName == chipName);

                    if(parserInfo == null)
                        continue;
                    
                    parserInfo.MethodInfo.Invoke(this, new object[] {chipData});

                }

            }

            StepCompleted();
        }

    }

    public partial class Loader
    {
        [FileParser("data.json", FileFlags.System)]
        public void ParseSystem(string file, PixelVision engine)
        {
            
            var jsonParser = new SystemParser(file, _fileLoadHelper, engine);

            jsonParser.CalculateSteps();

            while (jsonParser.Completed == false) jsonParser.NextStep();
            
        }
    }
}