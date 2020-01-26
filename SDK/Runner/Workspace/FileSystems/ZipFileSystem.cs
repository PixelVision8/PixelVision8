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
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

//using System.IO.Compression;

// TODO need to remove dependency on System.IO.File

namespace PixelVision8.Runner.Workspace
{
    public class ZipFileSystem : MemoryFileSystem
    {
        public string PhysicalRoot;

        private ZipFileSystem(ZipFile zf, string extractPath)
        {
            //            var entities = zf.GetEnumerator();

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

        public void Save()
        {
            if (PhysicalRoot == null) return;


            // TODO need to save the contents of the memory system back to a zip file

            var disk = this;

            var fileNameZip = disk.PhysicalRoot;

            // Move the original file so we keep it safe
            if (File.Exists(fileNameZip)) File.Move(fileNameZip, fileNameZip + ".bak");

            var files = disk.GetEntitiesRecursive(WorkspacePath.Root);

            //            using (var fileStream = new FileStream(fileNameZip, FileMode.Create))
            //            {
            using (var archive = new ZipOutputStream(new FileStream(fileNameZip, FileMode.Create)))
            {
                // Define the compression level
                // 0 - store only to 9 - means best compression
                archive.SetLevel(0);

                var buffer = new byte[4096];
                try
                {
                    foreach (var file in files)
                        // We can only save files
                        if (file.IsFile && !file.EntityName.StartsWith("."))
                        {
                            var tmpPath = file.Path.Substring(1);

                            // Using GetFileName makes the result compatible with XP
                            // as the resulting path is not absolute.
                            var entry = new ZipEntry(tmpPath)
                            {
                                // Could also use the last write time or similar for the file.
                                DateTime = DateTime.Now
                            };
                            archive.PutNextEntry(entry);

                            using (var fs = OpenFile(file, FileAccess.Read))
                            {
                                // Using a fixed size buffer here makes no noticeable difference for output
                                // but keeps a lid on memory usage.
                                int sourceBytes;

                                do
                                {
                                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                    archive.Write(buffer, 0, sourceBytes);
                                } while (sourceBytes > 0);
                            }

                            archive.CloseEntry();
                        }

                    // Finish is important to ensure trailing information for a Zip file is appended.  Without this
                    // the created file would be invalid.
                    archive.Finish();

                    // Close is important to wrap things up and unlock the file.
                    archive.Close();

                    File.Delete(fileNameZip + ".bak");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Archive Error: " + e);

                    if (File.Exists(fileNameZip + ".bak")) File.Move(fileNameZip + ".bak", fileNameZip);
                }
            }
        }
    }
}