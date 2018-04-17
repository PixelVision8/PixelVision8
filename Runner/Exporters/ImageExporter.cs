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
using PixelVisionSDK.Utils;

namespace PixelVisionRunner.Exporters
{
    public class ImageExporter : AbstractExporter
    {
//        protected IEngine engine;
        protected TextureData tmpTextureData;
        protected  IColor[] colors;
        protected int[] pixelData;
        protected ITexture2D texture2D;
        protected  ITextureFactory textureFactory;
        protected  int height;
        protected  int width;
        protected int loops;
        
        public ImageExporter(string fileName, ITextureFactory textureFactory, IColor[] colors = null) : base(fileName)
        {
            this.textureFactory = textureFactory;
            this.colors = colors;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();
            
            steps.Add(ConfigurePixelData);

            CalculateProcessingSteps();
            
            if(textureFactory.flip)
                steps.Add(FlipPixels);
            
            steps.Add(CreateTexture);

            steps.Add(CopyPixels);

            steps.Add(ConvertTexture);
        }

        protected virtual void CalculateProcessingSteps()
        {
            for (var i = 0; i < loops; i++) steps.Add(ProcessPixelData);
        }

        // TODO this should be a step in the exporter
        protected virtual void ConfigurePixelData()
        {
            tmpTextureData = new TextureData(width, height);
            tmpTextureData.Clear();

            currentStep++;
        }

        protected virtual void ProcessPixelData()
        {
            
            // Add custom logic here for processing pixel data

            currentStep++;
        }
        
        protected virtual void FlipPixels()
        {
            pixelData = tmpTextureData.GetPixels();
            SpriteChipUtil.FlipSpriteData(ref pixelData, width, height, false, true);
            currentStep++;
        }

        // TODO need to figure out a better way to share this with the Image Exporter
        protected virtual void CreateTexture()
        {
            texture2D = textureFactory.NewTexture2D(width, height);
            currentStep++;
        }

        protected virtual void CopyPixels()
        {
            // Need to make sure we have colors to copy over
            if (colors != null)
            {
                var pixels = texture2D.GetPixels();

                var total = pixels.Length;

                for (var i = 0; i < total; i++)
                {
                    var refID = pixelData[i];

                    if (refID > -1 && refID < total)
                        pixels[i] = colors[refID];
                }

                texture2D.SetPixels(pixels);

            }
            
            currentStep++;
        }

        protected virtual void ConvertTexture()
        {
            texture2D.Apply();
            bytes = texture2D.EncodeToPNG();
            currentStep++;
        }
    }
}