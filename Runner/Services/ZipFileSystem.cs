


using System;
using System.IO;
using System.IO.Compression;
using SharpFileSystem;
using SharpFileSystem.FileSystems;
using SharpFileSystem.IO;
using File = System.IO.File;

namespace PixelVision8.Runner.Data
{
    
    public class ZipFileSystem : MemoryFileSystem
    {
        public string srcPath;
        
        public static ZipFileSystem Open(string path)
        {
            // TODO this may fail on other systems because of the use of File
                return Open((FileStream) System.IO.File.OpenRead(path));
        }
        
        public static ZipFileSystem Open(FileStream s)
        {
            
            var fileSystem = new ZipFileSystem(new ZipArchive(s, ZipArchiveMode.Read), Path.GetFullPath(s.Name));
            fileSystem.srcPath = Path.GetFullPath(s.Name);
            return fileSystem;
//            ZipFile.Open(s.Name);

//            Console.WriteLine("Reading "+ s.Name);
//            
//            return new ZipFileSystem(Path.GetFileNameWithoutExtension(s.Name), ZipStorer.Open(s, FileAccess.ReadWrite));
        }
        
        public static ZipFileSystem Open(Stream s,  string name)
        {
            
            return new ZipFileSystem(new ZipArchive(s, ZipArchiveMode.Read), Path.GetFullPath(name));
//            ZipFile.Open(s.Name);
            
//            Console.WriteLine("Reading "+ s.Name);
//            
//            return new ZipFileSystem(Path.GetFileNameWithoutExtension(s.Name), ZipStorer.Open(s, FileAccess.ReadWrite));
        }
        
        private ZipFileSystem(ZipArchive zipStorer, string extractPath)
        {
            
            // TODO need to save the path to the zip file so it can updated
            
//            if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
//                extractPath += Path.DirectorySeparatorChar;

            
            
            // Opne up the zip
            // Read the central directory collection
            var entries = zipStorer.Entries;//zipStorer.ReadCentralDir();
            
//            extractPath = ;
            
            // Look for the desired file
            foreach (ZipArchiveEntry entry in entries)
            {

                var entryPath = entry.FullName;
//                Console.WriteLine("Zip File " + entryPath);
                
                // Gets the full path to ensure that relative segments are removed.
                string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                // are case-insensitive.
                if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                {

//                if (entryPath.StartsWith(fileName + Path.DirectorySeparatorChar))
//                {
//                    entryPath = entryPath.Substring(fileName.Length + 1);
//                }

                    var filePath = FileSystemPath.Root.AppendPath(entryPath);

                    if (filePath.IsFile)
                    {

//                    Console.WriteLine("Zip File - " + filePath + " in " + filePath.ParentPath);

                        // Try to create teh file in memeory
                        try
                        {
                            if (!Exists(filePath.ParentPath))
                            {
                                this.CreateDirectoryRecursive(filePath.ParentPath);
                            }

                            var stream = CreateFile(filePath);

//                        entry.ExtractToFile();
                            var fileStream = entry.Open(); //zipStorer.GetEntry(, stream);

                            stream.Write(fileStream.ReadAllBytes());

                            stream.Close();
                        }
                        catch
                        {
//                            Console.WriteLine("Couldn't create " + filePath.Path + "\n" + e.Message);
//                        throw;
                        }

                    }
                }

            }
            
            zipStorer.Dispose();
           
        }

        public void Save()
        {

            if (srcPath == null)
                return;
            
            
            // TODO need to save the contents of the memory system back to a zip file
            
            Console.WriteLine("Saving Zip Disk back to " + srcPath);

//            if (File.Exists(srcPath))
//            {
//                // TODO rename the existing zip so we don't overwrite 
//            }
//
//            var allFiles = SharpFileSystem
//            

//            var allFiles = this.GetEntitiesRecursive(FileSystemPath.Root);
//            SharpFileSystem.GetEntitiesRecursive(this, FileSystemPath.Root);
//            var files = GetEntities(FileSystemPath.Root);
//
//            
//            
//            var currentDisk = this;
//            
//            if (currentDisk is ZipFileSystem disk)
//            {
//                Console.WriteLine("Workspace is ready to save active disk");


            var disk = this;
            
                var fileNameZip = disk.srcPath;
                
                var files = disk.GetEntitiesRecursive(FileSystemPath.Root);

                using (var memoryStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {

                        foreach (var file in files)
                        {

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
                                Console.WriteLine("Archive Error: "+ e);
                            }

                        }
                        
                    }

                    try
                    {
                        
                        if (File.Exists(fileNameZip))
                        {
                            File.Move(fileNameZip, fileNameZip+".bak");
                        }
                        
                        using (var fileStream = new FileStream(fileNameZip, FileMode.Create))
                        {
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            memoryStream.CopyTo(fileStream);
                        }
                        
                        // Make sure we close the stream
                        memoryStream.Close();
                        
                        Console.WriteLine("Save archive ");
                        
                        File.Delete(fileNameZip+".bak");
                    }
                    catch (Exception e)
                    {
                        if (File.Exists(fileNameZip+".bak"))
                        {
                            File.Move(fileNameZip+".bak", fileNameZip);
                        }
                        
                        Console.WriteLine("Disk Save Error "+e);
                        
                    }
                }
               
                }
                
//            var mounts = fileSystem.Mounts as SortedList<FileSystemPath, IFileSystem>;
//            
//            // Create a new mount point for the current game
//            var rootPath = FileSystemPath.Root.AppendDirectory("Game");
//                
//            // Make sure we don't have a disk with the same name
//            if (mounts.ContainsKey(rootPath))
//            {
//                mounts.Remove(rootPath);
//            }

//        }
        
    }
}