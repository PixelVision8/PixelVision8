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

using PixelVisionSDK;

namespace PixelVisionRunner.Parsers
{

    public class MetaDataParser : JsonParser
    {

        private readonly IEngine engine;

        public MetaDataParser(string jsonString, IEngine target) : base(jsonString)
        {
            engine = target;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();
            steps.Add(ApplySettings);
        }

        public void ApplySettings()
        {
            foreach (var d in data)
            {
                engine.SetMetaData(d.Key, d.Value as string);

                if (d.Key == "name")
                {
                    engine.gameChip.name = d.Value as string;
                }else if (d.Key == "ext")
                {
                    engine.gameChip.ext = d.Value as string;
                }
            }

            currentStep++;
        }

    }

}