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

using System.Linq;
using SharpFileSystem;

namespace PixelVision8.Runner.Data
{
    public class DirectoryItem
    {
        public string ext = "";

        public string fullName;
        public bool isDirectory;
        public string name;
        public string parentPath;
        public string path;
        public bool selected;

        public string type = "none";
//        public bool isGameFile;

        public DirectoryItem(FileSystemPath filePath, bool gameFile = false)
        {
            path = filePath.Path;
            parentPath = filePath.ParentPath.Path;
//            isGameFile = gameFile;

            isDirectory = filePath.IsDirectory;

            fullName = filePath.EntityName;

            var split = fullName.Split('.');


            name = split.First();

            if (isDirectory)
            {
                type = "folder";
            }
            else
            {
                ext = filePath.GetExtension().Substring(1);

                // TODO this could be optimized better
                type = string.Join(".", split.Skip(1).ToArray().Select(o => o.ToString()).ToArray());
            }
        }
    }
}