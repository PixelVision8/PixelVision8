using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using PixelVision8.Engine.Utils;
using PixelVision8.Runner.Parsers;
using PixelVision8.Runner.Utils;
using PixelVisionRunner.Utils;


namespace PixelVision8.Runner.Importers
{
    public class PNGReader : IImageParser
    {
        protected Stream memoryStream;
        private int _width;
        private int _height;
        protected int bitsPerSample;
        protected int bytesPerSample;
        protected int bytesPerPixel;
        protected int bytesPerScanline;
        protected List<PngChunk> chunks;
        protected List<PngChunk> dataChunks;
        protected ColorType colorType;
        protected Palette palette;
        protected Color[] _colors;
        protected List<Color> _colorPalette;
        public int[] pixels;
        protected string maskHex;
        protected byte[] bytes;
        
        public int width => _width;

        public int height => _height;

        public Color[] colorPixels => _colors;
        public List<Color> colorPalette => _colorPalette;

        public PNGReader(byte[] bytes = null, string maskHex = "#FF00FF")
        {
            this.maskHex = maskHex;
            
            chunks = new List<PngChunk>();
            dataChunks = new List<PngChunk>();
            _colorPalette = new List<Color>();
            
            if (bytes != null)
            {
                ReadBytes(bytes);
            }
        }

        public void ReadBytes(byte[] bytes)
        {
            this.bytes = bytes;
            
//            this.inputStream = inputStream;
            
            memoryStream = new MemoryStream(bytes);
                
            if (!IsImage())
                throw new Exception("File does not have PNG signature.");
                
            ReadHeader();
        }

        public void ReadStream()
        {
            if (!IsImage())
                throw new Exception("File does not have PNG signature.");
                
            UnpackDataChunks();
            
        }

        public void ReadHeader()
        {
            memoryStream.Position = 8L;
            while (memoryStream.Position != memoryStream.Length)
            {
                byte[] numArray1 = new byte[4];
                memoryStream.Read(numArray1, 0, 4);
                uint num = numArray1.ToUInt();
                memoryStream.Position -= 4L;
                byte[] numArray2 = new byte[12 + (int) num];
                memoryStream.Read(numArray2, 0, 12 + (int) num);
                ProcessChunk(numArray2);
            }
        }

        public bool IsImage()
        {
            // If we don't have byte data, return false
            if (bytes == null)
                return false;
            
            memoryStream.Position = 0L;
            byte[] buffer = new byte[8];
            memoryStream.Read(buffer, 0, 8);
            int num = buffer.SequenceEqual(HeaderChunk.PngSignature) ? 1 : 0;
            memoryStream.Position = 0L;
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
                _width = (int) headerChunk.Width;
                _height = (int) headerChunk.Height;
                
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
                // TODO need to use this on iOS
//                using (DeflateStream zlibStream = new DeflateStream(memoryStream1, System.IO.Compression.CompressionMode.Decompress))
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
            bytesPerScanline = bytesPerPixel * _width + 1;
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
//            data = new Color[_width * _height];
            _colors = new Color[_width * _height];
            pixels = new int[_width * _height];
            
            byte[] previousScanline = new byte[bytesPerScanline];
            for (int y = 0; y < _height; ++y)
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
//                    for (int index1 = 0; index1 < _width; ++index1)
//                    {
//                        int index2 = 1 + index1 * bytesPerPixel;
//                        byte num = defilteredScanline[index2];
//                        data[y * _width + index1] = new Color(num, num, num);
//                    }
//                    break;
                case ColorType.Rgb:
                    for (int index1 = 0; index1 < _width; ++index1)
                    {
                        int index2 = 1 + index1 * bytesPerPixel;
                        byte r = defilteredScanline[index2];// / (float)byte.MaxValue;
                        byte g = defilteredScanline[index2 + bytesPerSample];// / (float)byte.MaxValue;
                        byte b = defilteredScanline[index2 + 2 * bytesPerSample];// / (float)byte.MaxValue;
//                        data[y * _width + index1] = new Color(r, g, b);


//                        var tmpColor = new Color(r, g, b);
//                        var colorVector = new Vector3((float) r / (float) byte.MaxValue, (float) g / (float) byte.MaxValue, (float) b / (float) byte.MaxValue);

                        
                        var color = new Color(r, g, b);
                        _colors[y * _width + index1] = color;

                        pixels[y * _width + index1] = IndexColor(color);



                    }
                    break;
                case ColorType.Palette:
                    for (int index = 0; index < _width; ++index)
                    {
//                        Color color = palette[defilteredScanline[index + 1]];
//                        data[y * _width + index] = color;
                        
                        var colorData = palette[defilteredScanline[index + 1]];
                        _colors[y * _width + index] = colorData;
                        
                        pixels[y * _width + index] = IndexColor(colorData);
                        
//                        IndexColor(colorData);
                    }
                    break;
//                case ColorType.GrayscaleWithAlpha:
//                    for (int index1 = 0; index1 < _width; ++index1)
//                    {
//                        int index2 = 1 + index1 * bytesPerPixel;
//                        byte num = defilteredScanline[index2];
//                        byte alpha = defilteredScanline[index2 + bytesPerSample];
//                        data[y * _width + index1] = new Color(num, num, num, alpha);
//                        pixels[y * _width + index1] = new ColorData(num, num, num, alpha);
//                    }
//                    break;
                case ColorType.RgbWithAlpha:
                    for (int index1 = 0; index1 < _width; ++index1)
                    {
                        int index2 = 1 + index1 * bytesPerPixel;
                        byte r = defilteredScanline[index2];// / (float)byte.MaxValue;
                        byte g = (defilteredScanline[index2 + bytesPerSample]);// / (float)byte.MaxValue;
                        byte b = (defilteredScanline[index2 + 2 * bytesPerSample]);// / (float)byte.MaxValue;
                        byte alpha = (defilteredScanline[index2 + 3 * bytesPerSample]);// / (float)byte.MaxValue;
//                        data[y * _width + index1] = new Color(r, g, b, alpha);
//                        pixels[y * _width + index1] = new ColorData(r, g, b){a=alpha};
                        
//                        var tmpColor = new Color(r, g, b);
//                        var colorVector = tmpColor.ToVector4();
//                        
                        
                        var color = alpha < 1 ? ColorUtils.HexToColor(maskHex) : new Color(r, g, b);
                        
//                        var color = new ColorData(r, g, b);
                        _colors[y * _width + index1] = color;
                        pixels[y * _width + index1] = IndexColor(color);
//                        IndexColor(color);
                        
                    }
                    break;
            }
        }

        private int IndexColor(Color color)
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
    }
}