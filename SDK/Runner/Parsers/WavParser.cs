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

namespace PixelVision8.Runner
{
    public class WavParser : AbstractParser
    {
        public string[] files;
        public ISoundChip soundChip;
        private IFileLoader _fileLoadHelper;
        public WavParser(string[] files, IFileLoader fileLoadHelper, IPlayerChips engine)
        {
            _fileLoadHelper = fileLoadHelper;
            soundChip = engine.SoundChip;
            this.files = files;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();
            _steps.Add(ParseWavData);
            _steps.Add(ConfigureSamples);
        }

        public void ParseWavData()
        {
            foreach (var file in files)
            {
                var name = _fileLoadHelper.GetFileName(file).Replace(".wav", "");
                soundChip.AddSample(name, _fileLoadHelper.ReadAllBytes(file));
            }

            CurrentStep++;
        }

        public void ConfigureSamples()
        {
            soundChip.RefreshSamples();

            CurrentStep++;
        }
    }

    public partial class Loader
    {
        [FileParser(".wav")]
        public void ParseWave(string[] file, IPlayerChips engine)
        {
            AddParser(new WavParser(file, _fileLoadHelper, engine ));
        }
    }
    
}