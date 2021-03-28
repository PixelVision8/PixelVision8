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

using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;

namespace PixelVision8.Runner
{
    public class ZipFileHelper : IFileLoader
    {

        public string[] Files
        {
            get {return _files.Keys.ToArray();}
        }

        private Dictionary<string, byte[]> _files = new Dictionary<string, byte[]>();

        public ZipFileHelper(Stream stream)
        {

                var zip = ZipStorer.Open(stream, FileAccess.Read);

                var dir = zip.ReadCentralDir();

                // Look for the desired file
                foreach (var entry in dir)
                {
                    var fileBytes = new byte[0];
                    zip.ExtractFile(entry, out fileBytes);

                    _files.Add(entry.ToString(), fileBytes);
                }

                zip.Close();

        }

        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public byte[] ReadAllBytes(string file)
        {

            return _files[file];
            
        }

        public bool Exists(string path)
        {
            
            return _files.ContainsKey(path);
        }
    }
}