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
using PixelVision8.Engine.Chips;
using PixelVision8.Runner.Services;

namespace PixelVision8.Runner.Parsers
{
    public class WavParser : AbstractParser
    {
        public string[] files;
        public SoundChip soundChip;
        private IFileLoadHelper _fileLoadHelper;
        public WavParser(string[] files, IFileLoadHelper fileLoadHelper, IEngine engine)
        {
            _fileLoadHelper = fileLoadHelper;
            soundChip = engine.SoundChip;
            this.files = files;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();
            steps.Add(ParseWavData);
            steps.Add(ConfigureSamples);
        }

        public void ParseWavData()
        {
            foreach (var file in files)
            {
                var name = _fileLoadHelper.GetFileName(file).Replace(".wav", "");
                soundChip.AddSample(name, _fileLoadHelper.ReadAllBytes(file));
            }

            currentStep++;
        }

        public void ConfigureSamples()
        {
            soundChip.RefreshSamples();

            currentStep++;
        }
    }
}