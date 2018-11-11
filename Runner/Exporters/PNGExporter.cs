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

using System;
using System.Collections.Generic;
using System.IO;
using Desktop.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Utilities;
using MonoGameRunner.Data;
using PixelVisionSDK;
using PixelVisionSDK.Utils;

namespace PixelVisionRunner.Exporters
{
    public class PNGExporter : AbstractExporter
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
        
        public PNGExporter(string fileName, ITextureFactory textureFactory, IColor[] colors = null) : base(fileName)
        {
            this.textureFactory = textureFactory;
            this.colors = colors;
            
            this.colorType = ColorType.RgbWithAlpha;
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
//            texture2D.Apply();
//            bytes = texture2D.EncodeToPNG();
            
            
            var stream = new MemoryStream();
//            texture.SaveAsPng(stream, width, height);
//            return stream.ToArray();
//            
//            Console.WriteLine("Texture Format " + texture.Format);
            Write(((Texture2DAdapter)texture2D).texture, stream);
            
            bytes = stream.ToArray();
            
            currentStep++;
        }

        #region PNG Writer APIs

//        private const int bitsPerSample = 8;
        private ColorType colorType;
        private Color[] colorData;
//        private int width;
//        private int height;
    
        public void Write(Texture2D texture2D, Stream outputStream)
      {
        width = texture2D.Width;
        height = texture2D.Height;
        GetColorData(texture2D);
        outputStream.Write(HeaderChunk.PngSignature, 0, HeaderChunk.PngSignature.Length);
        byte[] buffer1 = new HeaderChunk
        {
          Width = ((uint) texture2D.Width),
          Height = ((uint) texture2D.Height),
          BitDepth = 8,
          ColorType = colorType,
          CompressionMethod = 0,
          FilterMethod = 0,
          InterlaceMethod = 0
        }.Encode();
        outputStream.Write(buffer1, 0, buffer1.Length);
        byte[] buffer2 = EncodePixelData(texture2D);
        MemoryStream memoryStream = new MemoryStream();
        try
        {
          using (ZlibStream zlibStream = new ZlibStream(new MemoryStream(buffer2), CompressionMode.Compress))
            zlibStream.CopyTo(memoryStream);
        }
        catch (Exception ex)
        {
          throw new Exception("An error occurred during DEFLATE compression.", ex);
        }

        DataChunk dataChunk = new DataChunk();
        dataChunk.Data = memoryStream.ToArray();
        byte[] buffer3 = dataChunk.Encode();
        outputStream.Write(buffer3, 0, buffer3.Length);
        byte[] buffer4 = new EndChunk().Encode();
        outputStream.Write(buffer4, 0, buffer4.Length);
      }

      private byte[] EncodePixelData(Texture2D texture2D)
      {
        List<byte[]> numArrayList = new List<byte[]>();
        int bytesPerPixel = CalculateBytesPerPixel();
        byte[] previousScanline = new byte[width * bytesPerPixel];
        for (int y = 0; y < height; ++y)
        {
          byte[] rawScanline = GetRawScanline(y);
          byte[] filteredScanline = GetOptimalFilteredScanline(rawScanline, previousScanline, bytesPerPixel);
          numArrayList.Add(filteredScanline);
          previousScanline = rawScanline;
        }

        List<byte> byteList = new List<byte>();
        foreach (byte[] numArray in numArrayList)
          byteList.AddRange(numArray);
        return byteList.ToArray();
      }

      /// <summary>
      /// Applies all PNG filters to the given scanline and returns the filtered scanline that is deemed
      /// to be most compressible, using lowest total variation as proxy for compressibility.
      /// </summary>
      /// <param name="rawScanline"></param>
      /// <param name="previousScanline"></param>
      /// <param name="bytesPerPixel"></param>
      /// <returns></returns>
      private byte[] GetOptimalFilteredScanline(byte[] rawScanline, byte[] previousScanline, int bytesPerPixel)
      {
        List<Tuple<byte[], int>> tupleList = new List<Tuple<byte[], int>>();
        byte[] input1 = SubFilter.Encode(rawScanline, bytesPerPixel);
        tupleList.Add(new Tuple<byte[], int>(input1, CalculateTotalVariation(input1)));
        byte[] input2 = UpFilter.Encode(rawScanline, previousScanline);
        tupleList.Add(new Tuple<byte[], int>(input2, CalculateTotalVariation(input2)));
        byte[] input3 = AverageFilter.Encode(rawScanline, previousScanline, bytesPerPixel);
        tupleList.Add(new Tuple<byte[], int>(input3, CalculateTotalVariation(input3)));
        byte[] input4 = PaethFilter.Encode(rawScanline, previousScanline, bytesPerPixel);
        tupleList.Add(new Tuple<byte[], int>(input4, CalculateTotalVariation(input4)));
        int maxValue = int.MaxValue;
        int index1 = 0;
        for (int index2 = 0; index2 < tupleList.Count; ++index2)
        {
          if (tupleList[index2].Item2 < maxValue)
          {
            index1 = index2;
            maxValue = tupleList[index2].Item2;
          }
        }

        return tupleList[index1].Item1;
      }

      /// <summary>
      /// Calculates the total variation of given byte array.  Total variation is the sum of the absolute values of
      /// neighbour differences.
      /// </summary>
      /// <param name="input"></param>
      /// <returns></returns>
      private int CalculateTotalVariation(byte[] input)
      {
        int num = 0;
        for (int index = 1; index < input.Length; ++index)
          num += Math.Abs(input[index] - input[index - 1]);
        return num;
      }

      private byte[] GetRawScanline(int y)
      {
        byte[] numArray = new byte[4 * width];
        for (int index = 0; index < width; ++index)
        {
          Color color = colorData[y * width + index];
          numArray[4 * index] = color.R;
          numArray[4 * index + 1] = color.G;
          numArray[4 * index + 2] = color.B;
          numArray[4 * index + 3] = color.A;
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

      private void GetColorData(Texture2D texture2D)
      {
        int length = texture2D.Width * texture2D.Height;
        colorData = new Color[length];
//        switch (texture2D.Format)
//        {
//          case SurfaceFormat.Color:
            texture2D.GetData(colorData);
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
    }
        #endregion
    }
}