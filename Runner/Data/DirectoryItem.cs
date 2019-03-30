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

using System.Linq;
using SharpFileSystem;

namespace PixelVision8.Runner.Data
{
    public class DirectoryItem
    {

        public string fullName;
        public string name;
        public string path;
        public string parentPath;
        public bool selected;
        public string type = "none";
        public string ext = "";
        public bool isDirectory;
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