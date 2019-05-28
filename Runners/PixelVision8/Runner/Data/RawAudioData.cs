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

using System;
using PixelVision8.Engine;

namespace PixelVision8.Runner.Data
{
    public class RawAudioData : AbstractData
    {
        
        private float[] data;
        public int samples { get; set; }
        public int channels { get;}

        public int frequency { get; }
//        public static RawAudioData NewAudioClip(int lengthSamples, int channels, int frequency, bool stream)
//        {
//            return new RawAudioData(lengthSamples, channels, frequency);    
//        }
        
        public RawAudioData(int samples, int channels, int frequency)
        {
            this.samples = samples;
            this.channels = channels;
            this.frequency = frequency;
            this.data = new float[samples];
        }

        public void SetData(float[] data, int offsetSamples = 0)
        {

            var total = data.Length;
            for (int i = 0; i < total; i++)
            {
                var index = i + offsetSamples;
                
                if (index < samples)
                {
                    this.data[index] = data[i];
                }
            }

        }
        
        public void GetData(float[] data)
        {
            Array.Copy(this.data, data, samples);
        }

        public void Resize(int samples)
        {
            Array.Resize(ref data, samples);
            this.samples = samples;
        }

    }
}