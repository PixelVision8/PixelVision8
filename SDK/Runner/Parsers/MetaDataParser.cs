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
using PixelVision8.Runner;

namespace PixelVision8.Runner
{
    public class MetaDataParser : JsonParser
    {
        private readonly PixelVision engine;

        public MetaDataParser(string filePath, IFileLoader fileLoadHelper, PixelVision target) : base(filePath,
            fileLoadHelper)
        {
            engine = target;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();
            Steps.Add(ApplySettings);
        }

        public void ApplySettings()
        {
            foreach (var d in Data) engine.SetMetadata(d.Key, d.Value as string);

            StepCompleted();
        }
    }

    public partial class Loader
    {
        [FileParser("info.json", FileFlags.Meta)]
        public void ParseMetaData(string file, PixelVision engine)
        {
            AddParser(new MetaDataParser(file, _fileLoadHelper, engine));
        }
    }
}