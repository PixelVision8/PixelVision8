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

// using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using PixelVision8.Runner;
using PixelVisionRunner.Utils;

namespace PixelVision8.Editor
{
    public class PNGWriter : IImageExporter
    {
        //          private const int bitsPerSample = 8;
        private readonly ColorType colorType;
        private ColorData[] colorData;

        public PNGWriter()
        {
            // TODO this shouldn't save PNGs with alpha
            colorType = ColorType.RgbWithAlpha;
        }

        public int height { get; private set; }
        public int width { get; private set; }

        //        private int width;
        //        private int height;

        public void Write(int width1, int height1, Stream outputStream, ColorData[] colors)
        {
            width = width1;
            height = height1;

            colorData = colors;

            //        switch (texture2D.Format)
            //        {
            //          case SurfaceFormat.Color:
            //        texture2D.GetData(colorData);
            //        
            //        GetColorData(texture2D);


            outputStream.Write(HeaderChunk.PngSignature, 0, HeaderChunk.PngSignature.Length);
            var buffer1 = new HeaderChunk
            {
                Width = (uint) width1,
                Height = (uint) height1,
                BitDepth = 8,
                ColorType = colorType,
                CompressionMethod = 0,
                FilterMethod = 0,
                InterlaceMethod = 0
            }.Encode();
            outputStream.Write(buffer1, 0, buffer1.Length);
            var buffer2 = EncodePixelData();
            var memoryStream = new MemoryStream();
            try
            {
                using (var zlibStream = new ZlibStream(new MemoryStream(buffer2), CompressionMode.Compress))
                {
                    zlibStream.CopyTo(memoryStream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred during DEFLATE compression.", ex);
            }

            var dataChunk = new DataChunk
            {
                Data = memoryStream.ToArray()
            };
            var buffer3 = dataChunk.Encode();
            outputStream.Write(buffer3, 0, buffer3.Length);
            var buffer4 = new EndChunk().Encode();
            outputStream.Write(buffer4, 0, buffer4.Length);
        }

        private byte[] EncodePixelData()
        {
            var numArrayList = new List<byte[]>();
            var bytesPerPixel = CalculateBytesPerPixel();
            var previousScanline = new byte[width * bytesPerPixel];
            for (var y = 0; y < height; ++y)
            {
                var rawScanline = GetRawScanline(y);
                var filteredScanline = GetOptimalFilteredScanline(rawScanline, previousScanline, bytesPerPixel);
                numArrayList.Add(filteredScanline);
                previousScanline = rawScanline;
            }

            var byteList = new List<byte>();
            foreach (var numArray in numArrayList) byteList.AddRange(numArray);

            return byteList.ToArray();
        }

        /// <summary>
        ///     Applies all PNG filters to the given scanline and returns the filtered scanline that is deemed
        ///     to be most compressible, using lowest total variation as proxy for compressibility.
        /// </summary>
        /// <param name="rawScanline"></param>
        /// <param name="previousScanline"></param>
        /// <param name="bytesPerPixel"></param>
        /// <returns></returns>
        private byte[] GetOptimalFilteredScanline(byte[] rawScanline, byte[] previousScanline, int bytesPerPixel)
        {
            var tupleList = new List<Tuple<byte[], int>>();
            var input1 = SubFilter.Encode(rawScanline, bytesPerPixel);
            tupleList.Add(new Tuple<byte[], int>(input1, CalculateTotalVariation(input1)));
            var input2 = UpFilter.Encode(rawScanline, previousScanline);
            tupleList.Add(new Tuple<byte[], int>(input2, CalculateTotalVariation(input2)));
            var input3 = AverageFilter.Encode(rawScanline, previousScanline, bytesPerPixel);
            tupleList.Add(new Tuple<byte[], int>(input3, CalculateTotalVariation(input3)));
            var input4 = PaethFilter.Encode(rawScanline, previousScanline, bytesPerPixel);
            tupleList.Add(new Tuple<byte[], int>(input4, CalculateTotalVariation(input4)));
            var maxValue = int.MaxValue;
            var index1 = 0;
            for (var index2 = 0; index2 < tupleList.Count; ++index2)
                if (tupleList[index2].Item2 < maxValue)
                {
                    index1 = index2;
                    maxValue = tupleList[index2].Item2;
                }

            return tupleList[index1].Item1;
        }

        /// <summary>
        ///     Calculates the total variation of given byte array.  Total variation is the sum of the absolute values of
        ///     neighbour differences.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private int CalculateTotalVariation(byte[] input)
        {
            var num = 0;
            for (var index = 1; index < input.Length; ++index) num += Math.Abs(input[index] - input[index - 1]);

            return num;
        }

        private byte[] GetRawScanline(int y)
        {
            var numArray = new byte[4 * width];
            for (var index = 0; index < width; ++index)
            {
                var color = colorData[y * width + index];

                //          if(color.a < 0) color = new ColorData("#FF00FF");

                numArray[4 * index] = color.R; //Convert.ToByte(color.R * byte.MaxValue);
                numArray[4 * index + 1] = color.G; //Convert.ToByte(color.G * byte.MaxValue);
                numArray[4 * index + 2] = color.B; //Convert.ToByte(color.B * byte.MaxValue);
                numArray[4 * index + 3] = byte.MaxValue; //color.A;
            }

            return numArray;
        }

        private int CalculateBytesPerPixel()
        {
            switch (colorType)
            {
                case ColorType.Grayscale:
                    return 1;
                case ColorType.Rgb:
                    return 3;
                case ColorType.Palette:
                    return 1;
                case ColorType.GrayscaleWithAlpha:
                    return 2;
                case ColorType.RgbWithAlpha:
                    return 4;
                default:
                    return -1;
            }
        }

        //      private void GetColorData(Texture2D texture2D)
        //      {
        //        int length = texture2D.Width * texture2D.Height;
        //        colorData = new Color[length];
        ////        switch (texture2D.Format)
        ////        {
        ////          case SurfaceFormat.Color:
        //            texture2D.GetData(colorData);
        //            break;
        //          case SurfaceFormat.Bgr565:
        //            Bgr565[] data1 = new Bgr565[length];
        //            texture2D.GetData<Bgr565>(data1);
        //            for (int index = 0; index < length; ++index)
        //              this.colorData[index] = new Color(((IPackedVector) data1[index]).ToVector4());
        //            break;
        //          case SurfaceFormat.Bgra5551:
        //            Bgra5551[] data2 = new Bgra5551[length];
        //            texture2D.GetData<Bgra5551>(data2);
        //            for (int index = 0; index < length; ++index)
        //              this.colorData[index] = new Color(((IPackedVector) data2[index]).ToVector4());
        //            break;
        //          case SurfaceFormat.Bgra4444:
        //            Bgra4444[] data3 = new Bgra4444[length];
        //            texture2D.GetData<Bgra4444>(data3);
        //            for (int index = 0; index < length; ++index)
        //              this.colorData[index] = new Color(((IPackedVector) data3[index]).ToVector4());
        //            break;
        //          case SurfaceFormat.NormalizedByte2:
        //            NormalizedByte2[] data4 = new NormalizedByte2[length];
        //            texture2D.GetData<NormalizedByte2>(data4);
        //            for (int index = 0; index < length; ++index)
        //              this.colorData[index] = new Color(((IPackedVector) data4[index]).ToVector4());
        //            break;
        //          case SurfaceFormat.NormalizedByte4:
        //            NormalizedByte4[] data5 = new NormalizedByte4[length];
        //            texture2D.GetData<NormalizedByte4>(data5);
        //            for (int index = 0; index < length; ++index)
        //              this.colorData[index] = new Color(((IPackedVector) data5[index]).ToVector4());
        //            break;
        //          case SurfaceFormat.Rgba1010102:
        //            Rgba1010102[] data6 = new Rgba1010102[length];
        //            texture2D.GetData<Rgba1010102>(data6);
        //            for (int index = 0; index < length; ++index)
        //              this.colorData[index] = new Color(((IPackedVector) data6[index]).ToVector4());
        //            break;
        //          case SurfaceFormat.Rg32:
        //            Rg32[] data7 = new Rg32[length];
        //            texture2D.GetData<Rg32>(data7);
        //            for (int index = 0; index < length; ++index)
        //              this.colorData[index] = new Color(((IPackedVector) data7[index]).ToVector4());
        //            break;
        //          case SurfaceFormat.Rgba64:
        //            Rgba64[] data8 = new Rgba64[length];
        //            texture2D.GetData<Rgba64>(data8);
        //            for (int index = 0; index < length; ++index)
        //              this.colorData[index] = new Color(((IPackedVector) data8[index]).ToVector4());
        //            break;
        //          case SurfaceFormat.Alpha8:
        //            Alpha8[] data9 = new Alpha8[length];
        //            texture2D.GetData<Alpha8>(data9);
        //            for (int index = 0; index < length; ++index)
        //              this.colorData[index] = new Color(((IPackedVector) data9[index]).ToVector4());
        //            break;
        //          case SurfaceFormat.Single:
        //            float[] data10 = new float[length];
        //            texture2D.GetData<float>(data10);
        //            for (int index = 0; index < length; ++index)
        //            {
        //              float num = data10[index];
        //              this.colorData[index] = new Color(num, num, num);
        //            }
        //
        //            break;
        //          case SurfaceFormat.HalfSingle:
        //            HalfSingle[] data11 = new HalfSingle[length];
        //            texture2D.GetData<HalfSingle>(data11);
        //            for (int index = 0; index < length; ++index)
        //              this.colorData[index] = new Color(((IPackedVector) data11[index]).ToVector4());
        //            break;
        //          case SurfaceFormat.HalfVector2:
        //            HalfVector2[] data12 = new HalfVector2[length];
        //            texture2D.GetData<HalfVector2>(data12);
        //            for (int index = 0; index < length; ++index)
        //              this.colorData[index] = new Color(((IPackedVector) data12[index]).ToVector4());
        //            break;
        //          case SurfaceFormat.HalfVector4:
        //            HalfVector4[] data13 = new HalfVector4[length];
        //            texture2D.GetData<HalfVector4>(data13);
        //            for (int index = 0; index < length; ++index)
        //              this.colorData[index] = new Color(((IPackedVector) data13[index]).ToVector4());
        //            break;
        //          default:
        //            throw new Exception("Texture surface format not supported");
        //        }
        //    }
    }
}