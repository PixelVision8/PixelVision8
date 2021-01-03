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

namespace PixelVision8.Runner.Utils
{
    public interface IFileLoadHelper
    {
        string GetFileName(string path);
        byte[] ReadAllBytes(string path);

        bool Exists(string path);
    }

    public class FileLoadHelper : IFileLoadHelper
    {
        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public byte[] ReadAllBytes(string file)
        {

            // TODO this should be a service
            return File.ReadAllBytes(file);
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }
    }

}