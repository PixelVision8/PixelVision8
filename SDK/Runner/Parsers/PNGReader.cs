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

using Microsoft.Xna.Framework;
using PixelVision8.Player;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoGame.Framework.Utilities;

namespace PixelVision8.Runner
{
    public class PNGReader : IImageParser
    {
        public string MaskHex { get; private set; } = "#FF00FF";

        protected List<Color> _colorPalette;
        protected Color[] _colors;
        protected int bitsPerSample;
        protected int bytesPerPixel;
        protected int bytesPerSample;
        protected int bytesPerScanline;
        protected List<PngChunk> chunks;
        protected ColorType colorType;
        protected List<PngChunk> dataChunks;
        protected string maskHex;
        protected Stream memoryStream;
        protected Palette palette;
        public int[] pixels;

        public PNGReader(byte[] bytes = null)
        {
            if (bytes != null) ReadBytes(bytes);
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public Color[] ColorPixels => _colors;
        public List<Color> ColorPalette => _colorPalette;

        public virtual string FileName { get; set; } = "untitled.png";

        private void Reset()
        {
            chunks = new List<PngChunk>();
            dataChunks = new List<PngChunk>();
            _colorPalette = new List<Color>();
        }

        public void ReadBytes(byte[] bytes)
        {
            Reset();
            // this.bytes = bytes;

            //            this.inputStream = inputStream;

            memoryStream = new MemoryStream(bytes);

            if (!IsImage()) throw new Exception("File does not have PNG signature.");

            ReadHeader();
        }

        public virtual void ReadStream(string sourcePath, string maskHex)
        {
            if (!IsImage()) throw new Exception("File does not have PNG signature.");

            this.maskHex = maskHex;

            UnpackDataChunks();
        }

        public void ReadHeader()
        {
            memoryStream.Position = 8L;
            while (memoryStream.Position != memoryStream.Length)
            {
                var numArray1 = new byte[4];
                memoryStream.Read(numArray1, 0, 4);
                var num = numArray1.ToUInt();
                memoryStream.Position -= 4L;
                var numArray2 = new byte[12 + (int) num];
                memoryStream.Read(numArray2, 0, 12 + (int) num);
                ProcessChunk(numArray2);
            }
        }

        public bool IsImage()
        {
            // If we don't have byte data, return false
            // if (bytes == null) return false;

            memoryStream.Position = 0L;
            var buffer = new byte[8];
            memoryStream.Read(buffer, 0, 8);
            var num = buffer.SequenceEqual(HeaderChunk.PngSignature) ? 1 : 0;
            memoryStream.Position = 0L;
            return num != 0;
        }

        private void ProcessChunk(byte[] chunkBytes)
        {
            var chunkTypeString = PngChunk.GetChunkTypeString(chunkBytes.Skip(4).Take(4).ToArray());
            if (!(chunkTypeString == "IHDR"))
            {
                if (!(chunkTypeString == "PLTE"))
                {
                    if (!(chunkTypeString == "tRNS"))
                    {
                        if (!(chunkTypeString == "IDAT")) return;

                        var dataChunk = new DataChunk();
                        dataChunk.Decode(chunkBytes);
                        dataChunks.Add(dataChunk);
                    }
                    else
                    {
                        var transparencyChunk = new TransparencyChunk();
                        transparencyChunk.Decode(chunkBytes);
                        palette.AddAlphaToColors(transparencyChunk.PaletteTransparencies);
                    }
                }
                else
                {
                    var paletteChunk = new PaletteChunk();
                    paletteChunk.Decode(chunkBytes);
                    palette = paletteChunk.Palette;
                    chunks.Add(paletteChunk);
                }
            }
            else
            {
                var headerChunk = new HeaderChunk();
                headerChunk.Decode(chunkBytes);
                Width = (int) headerChunk.Width;
                Height = (int) headerChunk.Height;

                bitsPerSample = headerChunk.BitDepth;
                colorType = headerChunk.ColorType;
                chunks.Add(headerChunk);
            }
        }

        private void UnpackDataChunks()
        {
            var byteList = new List<byte>();
            foreach (var dataChunk in dataChunks)
                if (dataChunk.Type == "IDAT")
                    byteList.AddRange(dataChunk.Data);

            var memoryStream1 = new MemoryStream(byteList.ToArray());
            var memoryStream2 = new MemoryStream();
            try
            {
                // TODO need to use this on iOS
                //                using (DeflateStream zlibStream = new DeflateStream(memoryStream1, System.IO.Compression.CompressionMode.Decompress))
                using (var zlibStream = new ZlibStream(memoryStream1, CompressionMode.Decompress))
                {
                    zlibStream.CopyTo(memoryStream2);
                }
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
            bytesPerScanline = bytesPerPixel * Width + 1;
            var length = pixelData.Length / bytesPerScanline;
            if (pixelData.Length % bytesPerScanline != 0)
                throw new Exception(
                    "Malformed pixel data - total length of pixel data not multiple of ((bytesPerPixel * width) + 1)");

            var numArray = new byte[length][];
            for (var index1 = 0; index1 < length; ++index1)
            {
                numArray[index1] = new byte[bytesPerScanline];
                for (var index2 = 0; index2 < bytesPerScanline; ++index2)
                    numArray[index1][index2] = pixelData[index1 * bytesPerScanline + index2];
            }

            return numArray;
        }

        private void DecodePixelData(byte[][] pixelData)
        {
            //            data = new Color[_width * _height];
            _colors = new Color[Width * Height];
            pixels = new int[Width * Height];

            var previousScanline = new byte[bytesPerScanline];
            for (var y = 0; y < Height; ++y)
            {
                var scanline = pixelData[y];
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
                    for (var index1 = 0; index1 < Width; ++index1)
                    {
                        var index2 = 1 + index1 * bytesPerPixel;
                        var r = defilteredScanline[index2]; // / (float)byte.MaxValue;
                        var g = defilteredScanline[index2 + bytesPerSample]; // / (float)byte.MaxValue;
                        var b = defilteredScanline[index2 + 2 * bytesPerSample]; // / (float)byte.MaxValue;
                        //                        data[y * _width + index1] = new Color(r, g, b);


                        //                        var tmpColor = new Color(r, g, b);
                        //                        var colorVector = new Vector3((float) r / (float) byte.MaxValue, (float) g / (float) byte.MaxValue, (float) b / (float) byte.MaxValue);


                        var color = new Color(r, g, b);
                        _colors[y * Width + index1] = color;

                        pixels[y * Width + index1] = IndexColor(color);
                    }

                    break;
                case ColorType.Palette:
                    for (var index = 0; index < Width; ++index)
                    {
                        //                        Color color = palette[defilteredScanline[index + 1]];
                        //                        data[y * _width + index] = color;

                        var colorData = palette[defilteredScanline[index + 1]];
                        _colors[y * Width + index] = colorData;

                        pixels[y * Width + index] = IndexColor(colorData);

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
                    for (var index1 = 0; index1 < Width; ++index1)
                    {
                        var index2 = 1 + index1 * bytesPerPixel;
                        var r = defilteredScanline[index2]; // / (float)byte.MaxValue;
                        var g = defilteredScanline[index2 + bytesPerSample]; // / (float)byte.MaxValue;
                        var b = defilteredScanline[index2 + 2 * bytesPerSample]; // / (float)byte.MaxValue;
                        var alpha = defilteredScanline[index2 + 3 * bytesPerSample]; // / (float)byte.MaxValue;
                        //                        data[y * _width + index1] = new Color(r, g, b, alpha);
                        //                        pixels[y * _width + index1] = new ColorData(r, g, b){a=alpha};

                        //                        var tmpColor = new Color(r, g, b);
                        //                        var colorVector = tmpColor.ToVector4();
                        //                        

                        var color = alpha < 1 ? DisplayTarget.HexToColor(maskHex) : new Color(r, g, b);

                        //                        var color = new ColorData(r, g, b);
                        _colors[y * Width + index1] = color;
                        pixels[y * Width + index1] = IndexColor(color);
                        //                        IndexColor(color);
                    }

                    break;
            }
        }

        private int IndexColor(Color color)
        {
            var id = ColorPalette.IndexOf(color);
            // Add color to unique color
            if (id == -1)
            {
                id = ColorPalette.Count;
                ColorPalette.Add(color);

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

        public void Dispose()
        {
            _colorPalette.Clear();
            _colors = null;
            chunks.Clear();
            dataChunks.Clear();
            memoryStream.Dispose();
            palette = null;
            pixels = null;
        }
    }
}