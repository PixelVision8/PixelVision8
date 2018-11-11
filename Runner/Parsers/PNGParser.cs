using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Desktop.Util;
using Microsoft.Xna.Framework;
using MonoGame.Utilities;
using PixelVisionSDK;

namespace PixelVisionRunner.Parsers
{
    public class PNGParser: AbstractParser
    {
//        protected ITextureFactory textureFactory;
//        protected ITexture2D tex;
        protected Stream inputStream;
        public int imageWidth;
        public int imageHeight;
        protected int bitsPerSample;
        protected int bytesPerSample;
        protected int bytesPerPixel;
        protected int bytesPerScanline;
        protected IList<PngChunk> chunks;
        protected IList<PngChunk> dataChunks;
        protected ColorType colorType;
        protected Palette palette;
//        protected Texture2D texture;
        protected Color[] data;
        protected IColor[] colorPixels;
        protected IList<IColor> colorPalette;
        protected int[] colorRefs;
        protected int[] pixels;

        public PNGParser(byte[] bytes)
        {
//            this.textureFactory = textureFactory;
            this.bytes = bytes;
            
            chunks = new List<PngChunk>();
            dataChunks = new List<PngChunk>();
            colorPalette = new List<IColor>();
            
            if (bytes != null)
            {
                inputStream = new MemoryStream(bytes);
                
                if (!IsPngImage(inputStream))
                    throw new Exception("File does not have PNG signature.");
                
                ReadHeader(inputStream);
            }
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
                ReadStream();
//                tex = new Texture2DAdapter(ReadStream(), new ColorData("#FF00FF"));

            }
            
            currentStep++;
        }

        #region PNG Reader API
    
        
        public void ReadStream()
        {
            if (!IsPngImage(inputStream))
                throw new Exception("File does not have PNG signature.");
                
            UnpackDataChunks();
            
//            var graphicsDevice = ((TextureFactory) textureFactory).graphicsDevice;
            
//            var texture = new Texture2D(graphicsDevice, imageWidth, imageHeight, false, SurfaceFormat.Color);
//            texture.SetData(data);

            
            
//            textureData.SetPixels(pixels);
//            return texture;
        }

        public void ReadHeader(Stream inputStream)
        {
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
                imageWidth = (int) headerChunk.Width;
                imageHeight = (int) headerChunk.Height;
                
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
            bytesPerScanline = bytesPerPixel * imageWidth + 1;
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
            data = new Color[imageWidth * imageHeight];
            colorPixels = new IColor[imageWidth * imageHeight];
            pixels = new int[imageWidth * imageHeight];
            
            byte[] previousScanline = new byte[bytesPerScanline];
            for (int y = 0; y < imageHeight; ++y)
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
//                case ColorType.Grayscale:
//                    for (int index1 = 0; index1 < imageWidth; ++index1)
//                    {
//                        int index2 = 1 + index1 * bytesPerPixel;
//                        byte num = defilteredScanline[index2];
//                        data[y * imageWidth + index1] = new Color(num, num, num);
//                    }
//                    break;
                case ColorType.Rgb:
                    for (int index1 = 0; index1 < imageWidth; ++index1)
                    {
                        int index2 = 1 + index1 * bytesPerPixel;
                        int r = defilteredScanline[index2];
                        int g = defilteredScanline[index2 + bytesPerSample];
                        int b = defilteredScanline[index2 + 2 * bytesPerSample];
                        data[y * imageWidth + index1] = new Color(r, g, b);
                        
                        var color = new ColorData(r, g, b);
                        colorPixels[y * imageWidth + index1] = color;

                        pixels[y * imageWidth + index1] = IndexColor(color);



                    }
                    break;
                case ColorType.Palette:
                    for (int index = 0; index < imageWidth; ++index)
                    {
                        Color color = palette[defilteredScanline[index + 1]];
                        data[y * imageWidth + index] = color;
                        
                        var colorData = new ColorData(color.R, color.G, color.B, color.A);
                        colorPixels[y * imageWidth + index] = colorData;
                        
                        pixels[y * imageWidth + index] = IndexColor(colorData);
                        
//                        IndexColor(colorData);
                    }
                    break;
//                case ColorType.GrayscaleWithAlpha:
//                    for (int index1 = 0; index1 < imageWidth; ++index1)
//                    {
//                        int index2 = 1 + index1 * bytesPerPixel;
//                        byte num = defilteredScanline[index2];
//                        byte alpha = defilteredScanline[index2 + bytesPerSample];
//                        data[y * imageWidth + index1] = new Color(num, num, num, alpha);
//                        pixels[y * imageWidth + index1] = new ColorData(num, num, num, alpha);
//                    }
//                    break;
                case ColorType.RgbWithAlpha:
                    for (int index1 = 0; index1 < imageWidth; ++index1)
                    {
                        int index2 = 1 + index1 * bytesPerPixel;
                        int r = defilteredScanline[index2];
                        int g = defilteredScanline[index2 + bytesPerSample];
                        int b = defilteredScanline[index2 + 2 * bytesPerSample];
                        int alpha = defilteredScanline[index2 + 3 * bytesPerSample];
                        data[y * imageWidth + index1] = new Color(r, g, b, alpha);
//                        pixels[y * imageWidth + index1] = new ColorData(r, g, b){a=alpha};
                        
                        var color = new ColorData(r, g, b);
                        colorPixels[y * imageWidth + index1] = color;
                        pixels[y * imageWidth + index1] = IndexColor(color);
//                        IndexColor(color);
                        
                    }
                    break;
            }
        }

        private int IndexColor(IColor color)
        {
            var id = colorPalette.IndexOf(color);
            // Add color to unique color
            if (id == -1)
            {
                id = colorPalette.Count;
                colorPalette.Add(color);
                            
//                Console.WriteLine("New Color " + id + " " + color.r + " " + color.g + " " + color.b + " " + color.a);
            }

            return id;
        }
        
        private int CalculateBytesPerPixel()
        {
            switch (colorType)
            {
//                case ColorType.Grayscale:
//                    return bitsPerSample / 8;
                case ColorType.Rgb:
                    return 3 * bitsPerSample / 8;
                case ColorType.Palette:
                    return bitsPerSample / 8;
//                case ColorType.GrayscaleWithAlpha:
//                    return 2 * bitsPerSample / 8;
                case ColorType.RgbWithAlpha:
                    return 4 * bitsPerSample / 8;
                default:
                    throw new Exception("Unknown color type.");
            }
        }
        #endregion
    }
}