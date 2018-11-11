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

using PixelVisionSDK.Utils;


namespace PixelVisionRunner.Exporters
{
    public class PixelDataExporter : PNGExporter
    {

        public PixelDataExporter(string fileName, int[] pixelData, int width, int height, IColor[] colors, ITextureFactory textureFactory) : base(fileName, textureFactory, colors)
        {
            this.pixelData = pixelData;
            this.width = width;
            this.height = height;
        }

        public override void CalculateSteps()
        {

            currentStep = 0;
            
            steps.Add(CreateTexture);
            
            if(textureFactory.flip)
                steps.Add(FlipPixels);
            
            steps.Add(CopyPixels);
            steps.Add(ConvertTexture);
            
        }

        protected override void FlipPixels()
        {
            // TODO maybe the base class should just expect to have the pixel data so this doens't need to override it
            SpriteChipUtil.FlipSpriteData(ref pixelData, width, height, false, true);
            currentStep ++;
        }

    }
}