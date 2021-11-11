//   
// Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by Pixel Vision 8 are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
//
// Based on SharpFileSystem by Softwarebakery Copyright (c) 2013 under
// MIT license at https://github.com/bobvanderlinden/sharpfilesystem.
// Modified for PixelVision8 by Jesse Freeman
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

using System.IO;

namespace PixelVision8.Workspace
{
    public static class StreamExtensions
    {
        public const int DefaultBufferSize = 4096;

        public static string ReadAllText(this Stream s)
        {
            using (var reader = new StreamReader(s))
            {
                return reader.ReadToEnd();
            }
        }

        public static void StreamTo(this Stream s, Stream destination, int bufferSize)
        {
            var b = new byte[bufferSize];
            int readBytes;
            while ((readBytes = s.Read(b, 0, bufferSize)) > 0) destination.Write(b, 0, readBytes);
        }

        public static void StreamTo(this Stream s, Stream destination)
        {
            StreamTo(s, destination, DefaultBufferSize);
        }

        public static byte[] Read(this Stream s, int count)
        {
            var buffer = new byte[count];
            s.Read(buffer, 0, count);
            return buffer;
        }

        public static void Write(this Stream s, byte[] buffer)
        {
            s.Write(buffer, 0, buffer.Length);
        }

        public static void ReadAll(this Stream s)
        {
            s.StreamTo(EmptyStream.Instance);
        }

        public static byte[] ReadAllBytes(this Stream s)
        {
            using (var ms = new MemoryStream())
            {
                StreamTo(s, ms);
                return ms.ToArray();
            }
        }
    }
}