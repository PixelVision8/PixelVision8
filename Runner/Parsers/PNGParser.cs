using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Desktop.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Utilities;
using MonoGameRunner.Data;
using PixelVisionSDK;

namespace PixelVisionRunner.Parsers
{
    public class PNGParser: AbstractParser
    {
        protected ITextureFactory textureFactory;
        protected ITexture2D tex;
        
        public PNGParser(ITextureFactory textureFactory, byte[] bytes)
        {
            this.textureFactory = textureFactory;
            this.bytes = bytes;
            
            chunks = new List<PngChunk>();
            dataChunks = new List<PngChunk>();
        }
        
        public override void CalculateSteps()
        {
            base.CalculateSteps();
            steps.Add(ParseImageData);
        }
        
        public void ParseImageData()
        {
            if (bytes != null)
            {
            
                var graphic = ((TextureFactory) textureFactory).graphicsDevice;
                
                Texture2D tmpTexture;
    
                using (var stream = new MemoryStream(bytes))
                {
                    tmpTexture = Read(stream, graphic);
                }
                
                tex = new Texture2DAdapter(tmpTexture, new ColorData("#FF00FF"));
            
            }
            
            currentStep++;
        }

        #region PNG Reader API
    
        protected int width;
        protected int height;
        protected int bitsPerSample;
        protected int bytesPerSample;
        protected int bytesPerPixel;
        protected int bytesPerScanline;
        protected IList<PngChunk> chunks;
        protected IList<PngChunk> dataChunks;
        protected ColorType colorType;
        protected Palette palette;
        protected Texture2D texture;
        protected Color[] data;
        
        public Texture2D Read(Stream inputStream, GraphicsDevice graphicsDevice)
        {
            if (!IsPngImage(inputStream))
                throw new Exception("File does not have PNG signature.");
            inputStream.Position = 8L;
            while (inputStream.Position != inputStream.Length)
            {
                byte[] numArray1 = new byte[4];
                inputStream.Read(numArray1, 0, 4);
                uint num = numArray1.ToUInt();
                inputStream.Position -= 4L;
                byte[] numArray2 = new byte[12 + (int) num];
                inputStream.Read(numArray2, 0, 12 + (int) num);
                ProcessChunk(numArray2);
            }
            UnpackDataChunks();
            texture = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Color);
            texture.SetData(data);
            return texture;
        }

        public bool IsPngImage(Stream stream)
        {
            stream.Position = 0L;
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            int num = buffer.SequenceEqual(HeaderChunk.PngSignature) ? 1 : 0;
            stream.Position = 0L;
            return num != 0;
        }

        private void ProcessChunk(byte[] chunkBytes)
        {
            string chunkTypeString = PngChunk.GetChunkTypeString(chunkBytes.Skip(4).Take(4).ToArray());
            if (!(chunkTypeString == "IHDR"))
            {
                if (!(chunkTypeString == "PLTE"))
                {
                    if (!(chunkTypeString == "tRNS"))
                    {
                        if (!(chunkTypeString == "IDAT"))
                            return;
                        DataChunk dataChunk = new DataChunk();
                        dataChunk.Decode(chunkBytes);
                        dataChunks.Add(dataChunk);
                    }
                    else
                    {
                        TransparencyChunk transparencyChunk = new TransparencyChunk();
                        transparencyChunk.Decode(chunkBytes);
                        palette.AddAlphaToColors(transparencyChunk.PaletteTransparencies);
                    }
                }
                else
                {
                    PaletteChunk paletteChunk = new PaletteChunk();
                    paletteChunk.Decode(chunkBytes);
                    palette = paletteChunk.Palette;
                    chunks.Add(paletteChunk);
                }
            }
            else
            {
                HeaderChunk headerChunk = new HeaderChunk();
                headerChunk.Decode(chunkBytes);
                width = (int) headerChunk.Width;
                height = (int) headerChunk.Height;
                bitsPerSample = headerChunk.BitDepth;
                colorType = headerChunk.ColorType;
                chunks.Add(headerChunk);
            }
        }

        private void UnpackDataChunks()
        {
            List<byte> byteList = new List<byte>();
            foreach (PngChunk dataChunk in dataChunks)
            {
                if (dataChunk.Type == "IDAT")
                    byteList.AddRange(dataChunk.Data);
            }
            MemoryStream memoryStream1 = new MemoryStream(byteList.ToArray());
            MemoryStream memoryStream2 = new MemoryStream();
            try
            {
                using (ZlibStream zlibStream = new ZlibStream(memoryStream1, CompressionMode.Decompress))
                    zlibStream.CopyTo(memoryStream2);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred during DEFLATE decompression.", ex);
            }
            DecodePixelData(DeserializePixelData(memoryStream2.ToArray()));
        }

        private byte[][] DeserializePixelData(byte[] pixelData)
        {
            bytesPerPixel = CalculateBytesPerPixel();
            bytesPerSample = bitsPerSample / 8;
            bytesPerScanline = bytesPerPixel * width + 1;
            int length = pixelData.Length / bytesPerScanline;
            if (pixelData.Length % bytesPerScanline != 0)
                throw new Exception("Malformed pixel data - total length of pixel data not multiple of ((bytesPerPixel * width) + 1)");
            byte[][] numArray = new byte[length][];
            for (int index1 = 0; index1 < length; ++index1)
            {
                numArray[index1] = new byte[bytesPerScanline];
                for (int index2 = 0; index2 < bytesPerScanline; ++index2)
                    numArray[index1][index2] = pixelData[index1 * bytesPerScanline + index2];
            }
            return numArray;
        }

        private void DecodePixelData(byte[][] pixelData)
        {
            data = new Color[width * height];
            byte[] previousScanline = new byte[bytesPerScanline];
            for (int y = 0; y < height; ++y)
            {
                byte[] scanline = pixelData[y];
                byte[] defilteredScanline;
                switch (scanline[0])
                {
                    case 0:
                        defilteredScanline = NoneFilter.Decode(scanline);
                        break;
                    case 1:
                        defilteredScanline = SubFilter.Decode(scanline, bytesPerPixel);
                        break;
                    case 2:
                        defilteredScanline = UpFilter.Decode(scanline, previousScanline);
                        break;
                    case 3:
                        defilteredScanline = AverageFilter.Decode(scanline, previousScanline, bytesPerPixel);
                        break;
                    case 4:
                        defilteredScanline = PaethFilter.Decode(scanline, previousScanline, bytesPerPixel);
                        break;
                    default:
                        throw new Exception("Unknown filter type.");
                }
                previousScanline = defilteredScanline;
                ProcessDefilteredScanline(defilteredScanline, y);
            }
        }

        private void ProcessDefilteredScanline(byte[] defilteredScanline, int y)
        {
            switch (colorType)
            {
                case ColorType.Grayscale:
                    for (int index1 = 0; index1 < width; ++index1)
                    {
                        int index2 = 1 + index1 * bytesPerPixel;
                        byte num = defilteredScanline[index2];
                        data[y * width + index1] = new Color(num, num, num);
                    }
                    break;
                case ColorType.Rgb:
                    for (int index1 = 0; index1 < width; ++index1)
                    {
                        int index2 = 1 + index1 * bytesPerPixel;
                        int r = defilteredScanline[index2];
                        int g = defilteredScanline[index2 + bytesPerSample];
                        int b = defilteredScanline[index2 + 2 * bytesPerSample];
                        data[y * width + index1] = new Color(r, g, b);
                    }
                    break;
                case ColorType.Palette:
                    for (int index = 0; index < width; ++index)
                    {
                        Color color = palette[defilteredScanline[index + 1]];
                        data[y * width + index] = color;
                    }
                    break;
                case ColorType.GrayscaleWithAlpha:
                    for (int index1 = 0; index1 < width; ++index1)
                    {
                        int index2 = 1 + index1 * bytesPerPixel;
                        byte num = defilteredScanline[index2];
                        byte alpha = defilteredScanline[index2 + bytesPerSample];
                        data[y * width + index1] = new Color(num, num, num, alpha);
                    }
                    break;
                case ColorType.RgbWithAlpha:
                    for (int index1 = 0; index1 < width; ++index1)
                    {
                        int index2 = 1 + index1 * bytesPerPixel;
                        int r = defilteredScanline[index2];
                        int g = defilteredScanline[index2 + bytesPerSample];
                        int b = defilteredScanline[index2 + 2 * bytesPerSample];
                        int alpha = defilteredScanline[index2 + 3 * bytesPerSample];
                        data[y * width + index1] = new Color(r, g, b, alpha);
                    }
                    break;
            }
        }

        private int CalculateBytesPerPixel()
        {
            switch (colorType)
            {
                case ColorType.Grayscale:
                    return bitsPerSample / 8;
                case ColorType.Rgb:
                    return 3 * bitsPerSample / 8;
                case ColorType.Palette:
                    return bitsPerSample / 8;
                case ColorType.GrayscaleWithAlpha:
                    return 2 * bitsPerSample / 8;
                case ColorType.RgbWithAlpha:
                    return 4 * bitsPerSample / 8;
                default:
                    throw new Exception("Unknown color type.");
            }
        }
        #endregion
    }
}