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

namespace PixelVision8.Runner
{
    public class ImageParser : AbstractParser
    {
        protected IImageParser Parser;
        protected string MaskHex = "#FF00FF";

        public ImageParser(IImageParser parser)
        {
            Parser = parser;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();
            Steps.Add(ParseImageData);
        }

        public void ParseImageData()
        {
            Parser.ReadStream(SourcePath, MaskHex);

            StepCompleted();
        }

        public override void Dispose()
        {
            base.Dispose();
            Parser = null;
        }
    }
}