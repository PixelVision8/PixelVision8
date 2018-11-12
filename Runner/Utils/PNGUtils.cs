
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PixelVisionSDK;

namespace PixelVisionRunner.Utils
{


    public class Palette
    {
        public IList<IColor> colors;

        private string maskHex;
//        public List<IColor> uniqueColors = new List<IColor>();

        internal Palette(string maskHex = "#FF00FF")
        {
            colors = new List<IColor>();
        }

        internal IColor this[int index]
        {
            get
            {
                return colors[index];
            }
        }

        internal void AddColor(IColor color)
        {
//            if (colors.IndexOf(color) == -1)
//            {
                colors.Add(color);


//            var colorData = new ColorData(color.R, color.G, color.B);

//            if (uniqueColors.IndexOf(colorData) == -1)
//            {
//                uniqueColors.Add(colorData);
//                
////                Console.WriteLine("New Color " + colorData.ToHex());
//            }

//                Console.WriteLine("New Color " + color.ToHex());
//            }
        }

        internal void AddAlphaToColorAtIndex(int colorIndex, byte alpha)
        {
//            var oldColor = colors[colorIndex];

            // TODO this could be cleaner
            colors[colorIndex] = new ColorData(maskHex);
        }

        internal void AddAlphaToColors(IList<byte> alphas)
        {
            for (int i = 0; i < alphas.Count; i++)
            {
                AddAlphaToColorAtIndex(i, alphas[i]);
            }
        }
    }

    #region Chunks

    public class PngChunk
    {
        internal PngChunk()
        {
            Data = new byte[0];
        }

        /// <summary>
        /// Length of Data field
        /// </summary>
        internal uint Length
        {
            get;
            set;
        }

        internal string Type
        {
            get;
            set;
        }

        private bool Ancillary
        {
            get;
            set;
        }

        private bool Private
        {
            get;
            set;
        }

        private bool Reserved
        {
            get;
            set;
        }

        private bool SafeToCopy
        {
            get;
            set;
        }

        internal byte[] Data
        {
            get;
            set;
        }

        /// <summary>
        /// CRC of both Type and Data fields, but not Length field
        /// </summary>
        internal uint Crc
        {
            get;
            set;
        }

        internal virtual void Decode(byte[] chunkBytes)
        {
            var chunkBytesList = chunkBytes.ToList();

            Length = chunkBytesList.GetRange(0, 4).ToArray().ToUInt();
            DecodeType(chunkBytesList.GetRange(4, 4).ToArray());
            Data = chunkBytesList.GetRange(8, (int)Length).ToArray();
            Crc = chunkBytesList.GetRange((int)(8 + Length), 4).ToArray().ToUInt();

            if (CrcCheck() == false)
            {
                throw new Exception("CRC check failed.");
            }
        }

        internal virtual byte[] Encode()
        {
            var result = new List<byte>();

            uint dataLength = (uint)Data.Length;
            uint dataCrc = PngCrc.Calculate(InputToCrcCheck());

            result.AddRange(dataLength.ToByteArray());
            result.AddRange(GetChunkTypeBytes(Type));
            result.AddRange(Data);
            result.AddRange(dataCrc.ToByteArray());

            return result.ToArray();
        }

        private void DecodeType(byte[] typeBytes)
        {
            Type = GetChunkTypeString(typeBytes);

            var bitArray = new BitArray(typeBytes);

            Ancillary = bitArray[4];
            Private = bitArray[9];
            Reserved = bitArray[14];
            SafeToCopy = bitArray[19];
        }

        private bool CrcCheck()
        {
            var crcInputBytes = InputToCrcCheck();

            return (PngCrc.Calculate(crcInputBytes) == Crc);
        }

        private byte[] InputToCrcCheck()
        {
            byte[] chunkTypeBytes = GetChunkTypeBytes(Type);
            return chunkTypeBytes.Concat(Data).ToArray();
        }

        internal static string GetChunkTypeString(byte[] chunkTypeBytes)
        {
            return Encoding.UTF8.GetString(chunkTypeBytes, 0, chunkTypeBytes.Length);
        }

        private static byte[] GetChunkTypeBytes(string chunkTypeString)
        {
            return Encoding.UTF8.GetBytes(chunkTypeString);
        }
    }

    internal class HeaderChunk : PngChunk
    {
        private static byte[] pngSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };
        
        internal HeaderChunk()
        {
            Type = "IHDR";
        }

        internal uint Width
        {
            get;
            set;
        }

        internal uint Height
        {
            get;
            set;
        }

        internal byte BitDepth
        {
            get;
            set;
        }

        internal ColorType ColorType
        {
            get;
            set;
        }

        internal byte CompressionMethod
        {
            get;
            set;
        }

        internal byte FilterMethod
        {
            get;
            set;
        }

        internal byte InterlaceMethod
        {
            get;
            set;
        }

        internal static byte[] PngSignature
        {
            get { return pngSignature; }
        }

        internal override void Decode(byte[] chunkBytes)
        {
            base.Decode(chunkBytes);
            var chunkData = Data;

            Width = chunkData.Take(4).ToArray().ToUInt();
            Height = chunkData.Skip(4).Take(4).ToArray().ToUInt();
            BitDepth = chunkData.Skip(8).First();
            ColorType = (ColorType)chunkData.Skip(9).First();
            CompressionMethod = chunkData.Skip(10).First();
            FilterMethod = chunkData.Skip(11).First();
            InterlaceMethod = chunkData.Skip(12).First();

            if (BitDepth < 8)
            {
                throw new Exception(String.Format("Bit depth less than 8 bits per sample is unsupported.  Image bit depth is {0} bits per sample.", BitDepth));
            }
        }

        internal override byte[] Encode()
        {
            var chunkData = new List<byte>();

            chunkData.AddRange(Width.ToByteArray().ToList());
            chunkData.AddRange(Height.ToByteArray().ToList());
            chunkData.Add(BitDepth);
            chunkData.Add((byte)ColorType);
            chunkData.Add(CompressionMethod);
            chunkData.Add(FilterMethod);
            chunkData.Add(InterlaceMethod);

            Data = chunkData.ToArray();

            return base.Encode();
        }
    }

    internal class PaletteChunk : PngChunk
    {
        internal PaletteChunk()
        {
            Type = "PLTE";
            Palette = new Palette();
        }

        internal Palette Palette
        {
            get;
            set;
        }

        internal override void Decode(byte[] chunkBytes)
        {
            base.Decode(chunkBytes);
            var chunkData = Data;

            if (chunkData.Length % 3 != 0)
            {
                throw new Exception("Malformed palette chunk - length not multiple of 3.");
            }

            for (int i = 0; i < chunkData.Length / 3; i++)
            {
                float red = (chunkData.Skip(3 * i).Take(1).First())/(float)byte.MaxValue;
                float green = (chunkData.Skip((3 * i) + 1).Take(1).First())/(float)byte.MaxValue;
                float blue = (chunkData.Skip((3 * i) + 2).Take(1).First())/(float)byte.MaxValue;
                
                Palette.AddColor(new ColorData(red, green, blue));
            }
        }
    }

    internal class TransparencyChunk : PngChunk
    {
        internal TransparencyChunk()
        {
            Type = "tRNS";
            PaletteTransparencies = new List<byte>();
        }

        internal IList<byte> PaletteTransparencies
        {
            get;
            set;
        }

        internal override void Decode(byte[] chunkBytes)
        {
            base.Decode(chunkBytes);
            var chunkData = Data;

            PaletteTransparencies = chunkData.ToArray();
        }

        internal override byte[] Encode()
        {
            var chunkData = new List<byte>();


            Data = chunkData.ToArray();

            return base.Encode();
        }
    }

    internal class DataChunk : PngChunk
    {
        internal DataChunk()
        {
            Type = "IDAT";
        }
    }

    internal class EndChunk : PngChunk
    {
        internal EndChunk()
        {
            Type = "IEND";
        }
    }

    #endregion
    
    #region Enumerations

    public enum ColorType
    {
        Grayscale = 0,
        Rgb = 2,
        Palette = 3,
        GrayscaleWithAlpha = 4,
        RgbWithAlpha = 6
    }

    internal enum FilterType
    {
        None = 0,
        Sub = 1,
        Up = 2,
        Average = 3,
        Paeth = 4
    }

    #endregion

    #region Filters

    internal static class NoneFilter
    {
        internal static byte[] Decode(byte[] scanline)
        {
            return scanline;
        }

        internal static byte[] Encode(byte[] scanline)
        {
            var encodedScanline = new byte[scanline.Length + 1];

            encodedScanline[0] = (byte)FilterType.None;
            scanline.CopyTo(encodedScanline, 1);

            return encodedScanline;
        }
    }

    internal static class SubFilter
    {
        internal static byte[] Decode(byte[] scanline, int bytesPerPixel)
        {
            byte[] result = new byte[scanline.Length];

            for (int x = 1; x < scanline.Length; x++)
            {
                byte priorRawByte = (x - bytesPerPixel < 1) ? (byte)0 : result[x - bytesPerPixel];

                result[x] = (byte)((scanline[x] + priorRawByte) % 256);
            }

            return result;
        }

        internal static byte[] Encode(byte[] scanline, int bytesPerPixel)
        {
            var encodedScanline = new byte[scanline.Length + 1];

            encodedScanline[0] = (byte)FilterType.Sub;

            for (int x = 0; x < scanline.Length; x++)
            {
                byte priorRawByte = (x - bytesPerPixel < 0) ? (byte)0 : scanline[x - bytesPerPixel];

                encodedScanline[x + 1] = (byte)((scanline[x] - priorRawByte) % 256);
            }

            return encodedScanline;
        }
    }

    internal static class UpFilter
    {
        internal static byte[] Decode(byte[] scanline, byte[] previousScanline)
        {
            byte[] result = new byte[scanline.Length];

            for (int x = 1; x < scanline.Length; x++)
            {
                byte above = previousScanline[x];

                result[x] = (byte)((scanline[x] + above) % 256);
            }

            return result;
        }

        internal static byte[] Encode(byte[] scanline, byte[] previousScanline)
        {
            var encodedScanline = new byte[scanline.Length + 1];

            encodedScanline[0] = (byte)FilterType.Up;

            for (int x = 0; x < scanline.Length; x++)
            {
                byte above = previousScanline[x];

                encodedScanline[x + 1] = (byte)((scanline[x] - above) % 256);
            }

            return encodedScanline;
        }
    }

    internal static class AverageFilter
    {
        internal static byte[] Decode(byte[] scanline, byte[] previousScanline, int bytesPerPixel)
        {
            byte[] result = new byte[scanline.Length];

            for (int x = 1; x < scanline.Length; x++)
            {
                byte left = (x - bytesPerPixel < 1) ? (byte)0 : result[x - bytesPerPixel];
                byte above = previousScanline[x];

                result[x] = (byte)((scanline[x] + Average(left, above)) % 256);
            }

            return result;
        }

        internal static byte[] Encode(byte[] scanline, byte[] previousScanline, int bytesPerPixel)
        {
            var encodedScanline = new byte[scanline.Length + 1];

            encodedScanline[0] = (byte)FilterType.Average;

            for (int x = 0; x < scanline.Length; x++)
            {
                byte left = (x - bytesPerPixel < 0) ? (byte)0 : scanline[x - bytesPerPixel];
                byte above = previousScanline[x];

                encodedScanline[x + 1] = (byte)((scanline[x] - Average(left, above)) % 256);
            }

            return encodedScanline;
        }

        private static int Average(byte left, byte above)
        {
            return Convert.ToInt32(Math.Floor((left + above) / 2.0));
        }
    }

    internal static class PaethFilter
    {
        internal static byte[] Decode(byte[] scanline, byte[] previousScanline, int bytesPerPixel)
        {
            byte[] result = new byte[scanline.Length];

            for (int x = 1; x < scanline.Length; x++)
            {
                byte left = (x - bytesPerPixel < 1) ? (byte)0 : result[x - bytesPerPixel];
                byte above = previousScanline[x];
                byte upperLeft = (x - bytesPerPixel < 1) ? (byte)0 : previousScanline[x - bytesPerPixel];

                result[x] = (byte)((scanline[x] + PaethPredictor(left, above, upperLeft)) % 256);
            }

            return result;
        }

        internal static byte[] Encode(byte[] scanline, byte[] previousScanline, int bytesPerPixel)
        {
            var encodedScanline = new byte[scanline.Length + 1];

            encodedScanline[0] = (byte)FilterType.Paeth;

            for (int x = 0; x < scanline.Length; x++)
            {
                byte left = (x - bytesPerPixel < 0) ? (byte)0 : scanline[x - bytesPerPixel];
                byte above = previousScanline[x];
                byte upperLeft = (x - bytesPerPixel < 0) ? (byte)0 : previousScanline[x - bytesPerPixel];

                encodedScanline[x + 1] = (byte)((scanline[x] - PaethPredictor(left, above, upperLeft)) % 256);
            }

            return encodedScanline;
        }

        private static int PaethPredictor(int a, int b, int c)
        {
            int p = a + b - c;
            int pa = Math.Abs(p - a);
            int pb = Math.Abs(p - b);
            int pc = Math.Abs(p - c);

            if ((pa <= pb) && (pa <= pc))
            {
                return a;
            }

            if (pb <= pc)
            {
                return b;
            }

            return c;
        }
    }

    #endregion

    internal static class PngCrc
    {
        // table of CRCs of all 8-bit messages
        private static uint[] crcTable;

        static PngCrc()
        {
            BuildCrcTable();
        }

        /// <summary>
        /// Build CRC lookup table for performance (once-off)
        /// </summary>
        private static void BuildCrcTable()
        {
            crcTable = new uint[256];

            uint c, n, k;

            for (n = 0; n < 256; n++)
            {
                c = n;

                for (k = 0; k < 8; k++)
                {
                    if ((c & 1) > 0)
                    {
                        c = 0xedb88320 ^ (c >> 1);
                    }
                    else
                    {
                        c = c >> 1;
                    }
                }

                crcTable[n] = c;
            }
        }

        internal static uint Calculate(byte[] bytes)
        {
            uint c = 0xffffffff;

            int n;

            if (crcTable == null)
            {
                BuildCrcTable();
            }

            for (n = 0; n < bytes.Length; n++)
            {
                c = crcTable[(c ^ bytes[n]) & 0xff] ^ (c >> 8);
            }

            return c ^ 0xffffffff;
        }
    }

    internal static class Extensions
    {
        internal static uint ToUInt(this byte[] bytes)
        {
            byte[] input;

            if (BitConverter.IsLittleEndian)
            {
                input = ReverseByteArray(bytes);
            }
            else
            {
                input = bytes;
            }

            return BitConverter.ToUInt32(input, 0);
        }

        internal static byte[] ToByteArray(this uint integer)
        {
            byte[] output = BitConverter.GetBytes(integer);

            if (BitConverter.IsLittleEndian)
            {
                return ReverseByteArray(output);
            }

            return output;
        }

        private static byte[] ReverseByteArray(byte[] byteArray)
        {
            return byteArray.Reverse().ToArray();
        }
    }
}