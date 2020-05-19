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
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace PixelVision8.Runner.Workspace
{
    public class ZipFileSystem : MemoryFileSystem
    {
        public string PhysicalRoot;

        private ZipFileSystem(ZipFile zf, string extractPath)
        {
            
            using (zf)
            {
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                        // Ignore directories
                        continue;

                    var entryFileName = zipEntry.Name;


                    var filePath = WorkspacePath.Root.AppendPath(entryFileName);

                    if (!filePath.Path.StartsWith("/__"))
                        try
                        {
                            if (!Exists(filePath.ParentPath)) this.CreateDirectoryRecursive(filePath.ParentPath);

                            // 4K is optimum
                            var buffer = new byte[4096];

                            using (var zipStream = zf.GetInputStream(zipEntry))
                            using (var fsOutput = CreateFile(filePath))
                            {
                                StreamUtils.Copy(zipStream, fsOutput, buffer);
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                }
            }
        }

        public static ZipFileSystem Open(string path)
        {
            // TODO this may fail on other systems because of the use of File
            return Open(File.OpenRead(path));
        }

        public static ZipFileSystem Open(FileStream s)
        {
            var fileSystem = new ZipFileSystem(new ZipFile(s), Path.GetFullPath(s.Name))
            {
                PhysicalRoot = Path.GetFullPath(s.Name)
            };
            return fileSystem;
        }

        public static ZipFileSystem Open(Stream s, string name)
        {
            return new ZipFileSystem(new ZipFile(s), Path.GetFullPath(name));
        }

    }
}