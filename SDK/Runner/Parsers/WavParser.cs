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
        private readonly SoundChip _soundChip;
        private readonly IFileLoader _fileLoadHelper;

        public WavParser(string file, IFileLoader fileLoadHelper, SoundChip soundChip)
        {
            _fileLoadHelper = fileLoadHelper;
            _soundChip = soundChip;
            SourcePath = file;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();
            Steps.Add(ParseWavData);
        }

        public void ParseWavData()
        {
            
            var name = _fileLoadHelper.GetFileName(SourcePath).Replace(".wav", "");
            _soundChip.AddSample(name, _fileLoadHelper.ReadAllBytes(SourcePath));

            CurrentStep++;
        }

    }

    public partial class Loader
    {
        [FileParser(".wav", FileFlags.Sounds)]
        public void ParseWave(string file, PixelVision engine)
        {
            AddParser(new WavParser(file, _fileLoadHelper, engine.SoundChip));
        }
    }
}