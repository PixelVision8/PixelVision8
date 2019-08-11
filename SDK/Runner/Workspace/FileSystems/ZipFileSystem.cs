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

using System;
using System.IO;
using System.IO.Compression;

// TODO need to remove dependency on System.IO.File

namespace PixelVision8.Runner.Workspace
{
    public class ZipFileSystem : MemoryFileSystem
    {
        public string srcPath;

        private ZipFileSystem(ZipArchive zipStorer, string extractPath)
        {
            
            var entries = zipStorer.Entries;


            // Look for the desired file
            foreach (var entry in entries)
            {
                var entryPath = entry.FullName;

                // Gets the full path to ensure that relative segments are removed.
                var destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                // are case-insensitive.
                if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                {

                    var filePath = WorkspacePath.Root.AppendPath(entryPath);

                    if (filePath.IsFile)
                        try
                        {
                            if (!Exists(filePath.ParentPath)) this.CreateDirectoryRecursive(filePath.ParentPath);

                            var stream = CreateFile(filePath);

                            var fileStream = entry.Open();

                            stream.Write(fileStream.ReadAllBytes());

                            stream.Close();
                        }
                        catch
                        {
                            // ignored
                        }
                }
            }

            zipStorer.Dispose();
        }

        public static ZipFileSystem Open(string path)
        {
            // TODO this may fail on other systems because of the use of File
            return Open( File.OpenRead(path));
        }

        public static ZipFileSystem Open(FileStream s)
        {
            var fileSystem = new ZipFileSystem(new ZipArchive(s, ZipArchiveMode.Read), Path.GetFullPath(s.Name));
            fileSystem.srcPath = Path.GetFullPath(s.Name);
            return fileSystem;
        }

        public static ZipFileSystem Open(Stream s, string name)
        {
            return new ZipFileSystem(new ZipArchive(s, ZipArchiveMode.Read), Path.GetFullPath(name));
        }

        public void Save()
        {
            if (srcPath == null)
                return;


            // TODO need to save the contents of the memory system back to a zip file

            var disk = this;

            var fileNameZip = disk.srcPath;

            var files = disk.GetEntitiesRecursive(WorkspacePath.Root);

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                        try
                        {
                            // We can only save files
                            if (file.IsFile && !file.EntityName.StartsWith("._"))
                            {
                                var tmpPath = file.Path.Substring(1);

                                var tmpFile = archive.CreateEntry(tmpPath);

                                using (var entryStream = tmpFile.Open())
                                {
                                    disk.OpenFile(file, FileAccess.ReadWrite).CopyTo(entryStream);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Archive Error: " + e);
                        }
                }

                try
                {
                    if (File.Exists(fileNameZip)) File.Move(fileNameZip, fileNameZip + ".bak");

                    using (var fileStream = new FileStream(fileNameZip, FileMode.Create))
                    {
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        memoryStream.CopyTo(fileStream);
                    }

                    // Make sure we close the stream
                    memoryStream.Close();

//                    Console.WriteLine("Save archive ");

                    File.Delete(fileNameZip + ".bak");
                }
                catch (Exception e)
                {
                    if (File.Exists(fileNameZip + ".bak")) File.Move(fileNameZip + ".bak", fileNameZip);

                    Console.WriteLine("Disk Save Error " + e);
                }
            }
        }

    }
}