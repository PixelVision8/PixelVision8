using System.Collections.Generic;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;

namespace PixelVision8.Runner.Parsers
{
    public class WavParser : AbstractParser
    {
        public Dictionary<string, byte[]> files;
        public SoundChip soundChip;

        public WavParser(IEngine engine, Dictionary<string, byte[]> files)
        {
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
                var name = file.Key.Replace(".wav", "");
                soundChip.AddSample(name, file.Value);
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