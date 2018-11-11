using System;
using PixelVisionSDK;
using PixelVisionSDK.Chips;

namespace PixelVisionRunner.Parsers
{
    /// <summary>
    ///     
    /// </summary>
    public class SupportedColorParser: ColorParser
    {
        public SupportedColorParser(ITextureFactory textureFactory, byte[] bytes, ColorChip colorChip, IColor magenta) : base(textureFactory, bytes, colorChip, magenta, true)
        {
            
        }
        
        public override void CalculateSteps()
        {
            
            currentStep = 0;
            steps.Add(ParseImageData);
            steps.Add(IndexColors);
            steps.Add(ReadColors);
            steps.Add(ResetColorChip);
            steps.Add(AddSupportedColors);
            steps.Add(UpdateColors);
            
        }
        
        public void AddSupportedColors()
        {
            colorChip.Clear();

            for (var i = 0; i < totalColors; i++)
            {
                var tmpColor = colors[i];
                var hex = ColorData.ColorToHex(tmpColor.r, tmpColor.g, tmpColor.b);
                
                colorChip.AddSupportedColor(hex);
            }

            currentStep++;
        }
    }
}