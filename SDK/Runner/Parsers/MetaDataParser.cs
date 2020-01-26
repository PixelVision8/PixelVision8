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

using PixelVision8.Engine;

namespace PixelVision8.Runner.Parsers
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
            foreach (var d in data) engine.SetMetadata(d.Key, d.Value as string);

            currentStep++;
        }
    }
}