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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Runner.Data;
using PixelVision8.Runner.Utils;
using SharpFileSystem;
using SharpFileSystem.FileSystems;
using SharpFileSystem.IO;
using File = System.IO.File;

namespace PixelVision8.Runner.Services
{
    public class WorkspaceServicePlus : WorkspaceService
    {
        public DiskDriveService diskDrives;
        protected FileSystemMounter diskMounter = new FileSystemMounter();
        
        public WorkspaceServicePlus(PixelVision8Runner runner, KeyValuePair<FileSystemPath, IFileSystem> mountPoint) : base(mountPoint)
        {
        }
        
        public void MountWorkspace(string name)
        {
            
            var osFileSystem = new MergedFileSystem();

            osFileSystem.FileSystems = osFileSystem.FileSystems.Concat(new[] { new SubFileSystem(fileSystem,
                FileSystemPath.Root.AppendDirectory("App").AppendDirectory("PixelVisionOS")) });
                
            // Mount the PixelVisionOS directory
            AddMount(new KeyValuePair<FileSystemPath, IFileSystem>(FileSystemPath.Root.AppendDirectory("PixelVisionOS"), osFileSystem));
                
            
            var filePath = FileSystemPath.Root.AppendDirectory("User");

            // Make sure that the user directory exits
            if (fileSystem.Exists(filePath))
            {
                filePath = filePath.AppendDirectory(name);

                // If the filesystem doesn't exit, we want to create it
                if (!fileSystem.Exists(filePath)) fileSystem.CreateDirectory(filePath);
            }

            var workspaceDisk = new SubFileSystem(fileSystem, filePath);

            fileSystem.Mounts.Add(
                new KeyValuePair<FileSystemPath, IFileSystem>(FileSystemPath.Root.AppendDirectory("Workspace"),
                    workspaceDisk));
            
            // Create a path to the workspace system folder
            var path = FileSystemPath.Root.AppendDirectory("Workspace").AppendDirectory("System");

            try
            {
                // Look to see if the workspace system folder exists
                if (Exists(path))
                {
//                    Console.WriteLine("Found Workspace system folder");
                    
                    // Add the workspace system folder to the os file system
                    osFileSystem.FileSystems = osFileSystem.FileSystems.Concat(
                        new[] { 
                            new SubFileSystem(fileSystem, path) 
                        }
                    );
                }
                
            }
            catch 
            {
                Console.WriteLine("No system folder");
            }
            
            

        }

        // Exports the active song in the music chip
        public void ExportSong(string path, MusicChip musicChip, SoundChip soundChip)
        {
            var filePath = FileSystemPath.Parse(path);

            if (fileSystem.Exists(filePath))
            {
                filePath = filePath.AppendDirectory("Loops");

                if (!fileSystem.Exists(filePath)) fileSystem.CreateDirectory(filePath);

                try
                {
                    var exportService = locator.GetService(typeof(ExportService).FullName) as ExportService;


                    // TODO exporting sprites doesn't work
                    if (exportService != null)
                    {
                        exportService.ExportSong(filePath.Path, musicChip, soundChip);
//
                        exportService.StartExport();
                    }
                }
                catch (Exception e)
                {
                    // TODO this needs to go through the error system?
                    Console.WriteLine(e);
                    throw;
                }

                // TODO saving song doesn't work
//                runner.exportService.ExportSong(filePath.Path, musicChip, soundChip);
//
//                runner.StartExport();
            }
        }

        public override void MountFileSystems(Dictionary<FileSystemPath, IFileSystem> fileSystems)
        {
            base.MountFileSystems(fileSystems);
            
            // Create a mount point for disks

            diskMounter = new FileSystemMounter();

            // Add the disk mount point
            fileSystem.Mounts.Add(
                new KeyValuePair<FileSystemPath, IFileSystem>(FileSystemPath.Root.AppendDirectory("Disks"),
                    diskMounter));


            // Create a disk drive service to mange the disks
            diskDrives = new DiskDriveService(diskMounter, totalDisks);
        }
        
        public Dictionary<string, string> DiskPaths()
        {
            var pathRefs = new Dictionary<string, string>();

            var mounts = diskDrives.disks; //disks.Mounts as SortedList<FileSystemPath, IFileSystem>;

            for (var i = 0; i < mounts.Length; i++)
            {
                var path = mounts[i];
                pathRefs.Add(path.EntityName, "/Disks" + path.Path);
            }

            return pathRefs;
        }

        public string MountDisk(string path)
        {
//            Console.WriteLine("Load File - " + path + " Auto Run " + autoRunEnabled);
//            try
//            {
                IFileSystem disk;

                string entityName;

                var attr = File.GetAttributes(path);

                if (attr.HasFlag(FileAttributes.Directory))
                    entityName = new DirectoryInfo(path).Name;
                else
                    entityName = Path.GetFileNameWithoutExtension(path);

                if (path.EndsWith(".pv8") || path.EndsWith(".zip"))
                    disk = ZipFileSystem.Open(path);
                else
                    disk = new PhysicalFileSystem(path);

                if (disk == null)
                    return null;

                // Test to see if the disk is a valid game
                if (ValidateGameInDir(disk) == false &&
                    disk.Exists(FileSystemPath.Root.AppendFile("info.json")) == false) return null;

                // Update the root path to just be the name of the entity
                var rootPath = FileSystemPath.Root.AppendDirectory(entityName);

                // Check to see if there is already a disk in slot 1, if so we want to eject it since only slot 1 can boot
//                if (diskDrives.total > 0)// && autoRunEnabled)
//                {
//                    // Clear the load history
////                    runner.loadHistory.Clear();
//
//                    // Remove all the other disks
////                    diskDrives.EjectAll();
//                }

                // Add the new disk
                diskDrives.AddDisk(rootPath, disk);

                // Return the disk name
                return entityName;
                
                // Only try to auto run a game if this is enabled in the runner
//                if (autoRunEnabled) AutoRunGameFromDisk(entityName);
//            }
//            catch
//            {
//                autoRunEnabled = true;
//                // TODO need to make sure we show a better error to explain why the disk couldn't load
////                runner.DisplayError(RunnerGame.ErrorCode.NoAutoRun);
//            }

            // Only update the bios when we need  to
//            if (updateBios) UpdateDiskInBios();
        }

        public string AutoRunGameFromDisk(string diskName)
        {
            var diskPath = FileSystemPath.Root.AppendDirectory("Disks")
                .AppendDirectory(diskName);


            var autoRunPath = diskPath.AppendFile("info.json");

            // Try to read the disk's info file and see if there is an auto run path
            try
            {
                // Only run a disk if there is an auto run file in there
                if (fileSystem.Exists(autoRunPath))
                {
                    var json = ReadTextFromFile(autoRunPath);

                    var autoRunData = Json.Deserialize(json) as Dictionary<string, object>;

                    var tmpPath = autoRunData["AutoRun"] as string;

                    // Get the auto run from the json file
                    var newDiskPath = FileSystemPath.Parse("/Disks/" + diskName + tmpPath);

                    // Change the disk path to the one in the auto-run file
                    if (fileSystem.Exists(newDiskPath)) diskPath = newDiskPath;
                }
            }
            catch
            {
                // ignored
            }

            // Always validate that the disk is a valid game before trying to load it.
            if (ValidateGameInDir(diskPath))
            {
                

                return diskPath.Path;
                // Load the disk path and play the game
//                runner.Load(diskPath.Path, RunnerGame.RunnerMode.Playing, metaData);
            }
            else
            {
                return null;
                // If the new auto run path can't be found, throw an error
//                runner.DisplayError(RunnerGame.ErrorCode.NoAutoRun);
            }
        }

        public void SaveActiveDisks()
        {
            var disks = diskDrives.disks;

            foreach (var disk in disks) diskDrives.SaveDisk(disk);
        }


        public void SaveActiveDisk()
        {
//            var disks = diskDrives.disks;
//
//            foreach (var disk in disks)
//            {
//                diskDrives.SaveDisk(disk);
//            }


            if (currentDisk is ZipFileSystem disk)
            {
//                Console.WriteLine("Workspace is ready to save active disk");

//                disk.Save();
                var fileNameZip = disk.srcPath;

                var files = currentDisk.GetEntitiesRecursive(FileSystemPath.Root);

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
                            catch
                            {
//                                Console.WriteLine("Archive Error: "+ e);
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

//                        Console.WriteLine("Save archive ");

                        File.Delete(fileNameZip + ".bak");
                    }
                    catch
                    {
                        if (File.Exists(fileNameZip + ".bak")) File.Move(fileNameZip + ".bak", fileNameZip);

//                        Console.WriteLine("Disk Save Error "+e);
                    }
                }
            }


            var mounts = fileSystem.Mounts as SortedList<FileSystemPath, IFileSystem>;

            // Create a new mount point for the current game
            var rootPath = FileSystemPath.Root.AppendDirectory("Game");

            // Make sure we don't have a disk with the same name
            if (mounts.ContainsKey(rootPath)) mounts.Remove(rootPath);
        }

//        public void EjectDisks()
//        {
//            diskDrives.EjectAll();
//            
//            runner.DisplayError(RunnerGame.ErrorCode.NoAutoRun);
//
//        }

        protected override List<FileSystemPath> GetLibDirectoryPaths()
        {
            var paths = base.GetLibDirectoryPaths();
            
            var diskPaths = diskDrives.disks;

            var totalDisks = diskPaths.Length - 1;

            // Loop backwards through disks
            for (var i = totalDisks; i >= 0; i--)
            {
                var diskPath = FileSystemPath.Root.AppendDirectory("Disks")
                    .AppendPath(diskPaths[i].AppendDirectory("System").AppendDirectory("Libs"));

                paths.Add(diskPath);
            }

            return paths;
        }

//        public string AutoRunFirstDisk()
//        {
//            if (diskDrives.total > 0)
//            {
//                var firstDisk = diskDrives.disks[0];
//                return firstDisk.EntityName;
//            }
//
//            return null;
//        }

        public override void ShutdownSystem()
        {
            // make sure we have the current list of disks in the bios
//            UpdateDiskInBios();
            SaveActiveDisks();
            
            base.ShutdownSystem();
        }

//        public void SaveDisksInMemory()
//        {
//            Console.WriteLine("Save disks in memory");
//
//            var paths = diskDrives.disks;
//
//            for (var i = 0; i < paths.Length; i++) diskDrives.SaveDisk(paths[i]);
//        }

        // Make sure you can only eject a disk by forcing the path to be in the disk mounts scope
        public void EjectDisk(FileSystemPath? filePath = null)
        {
            // Remove the disk if disks exists
            if (diskDrives.total > 0)
                try
                {
                    // Use the path that is supplied or get the first disk path
                    var path = filePath.HasValue ? filePath.Value : diskDrives.disks.First();

                    // Attempt to remove the disk
                    diskDrives.RemoveDisk(path);

                    // Update the bios when a disk is removed
//                    UpdateDiskInBios();
                }
                catch
                {
                    // ignored error when removing a disk that doesn't exist
                }

//            // Test to see if there is an OS image
//            try
//            {
//                if (fileSystem.Exists(FileSystemPath.Root.AppendDirectory("Workspace")))
//                {
//                    // Reset he current game
//
//                    // TODO Need to figure out how we should restart here
//                    runner.ResetGame();
//
//                    // Exit out of the function
//                    return;
//                }
//            }
//            catch
//            {
//                // ignored
//            }

            // What happens when there are no disks
            if (diskDrives.total > 0)
                try
                {
                    // Get the next disk name
                    var diskName = diskDrives.disks.First().Path.Replace("/", "");

                    // Clear the history
//                    runner.loadHistory.Clear();

                    // Attempt to run the fist disk
                    AutoRunGameFromDisk(diskName);

                    return;
                }
                catch
                {
                    // ignored
                }

            // TODO this is duplicated from the Pixel Vision 8 Runner
            // Look to see if we have the bios default tool in the OS folder
            try
            {
//                var biosAutoRun = FileSystemPath.Parse((string) ReadBiosData("AutoRun", ""));
//
//                if (fileSystem.Exists(biosAutoRun))
//                    if (ValidateGameInDir(biosAutoRun))
//                    {
//                        runner.Load(biosAutoRun.Path);
//                        return;
//                    }
            }
            catch
            {
            }

            // If ejecting a disk fails, display the disk error
//            runner.DisplayError(RunnerGame.ErrorCode.NoAutoRun);
        }

        // TODO this is hardcoded but should come from the bios
        
        public string spriteBuilderFolderName = "SpriteBuilder";
        
        public bool VaildateSpriteBuilderFolder(FileSystemPath rootPath)
        {
            // TODO need to make sure this is in the current game directory and uses the filesyem path
            rootPath = rootPath.AppendDirectory(spriteBuilderFolderName);//(string) ReadBiosData("SpriteBuilderDir", "SpriteBuilder"));

            return fileSystem.Exists(rootPath);
        }

        public int GenerateSprites(string path, PixelVisionEngine targetGame)
        {
            var count = 0;

            var filePath = FileSystemPath.Parse(path);

            var srcPath = filePath.AppendDirectory("SpriteBuilder");

            var fileData = new Dictionary<string, byte[]>();

            if (fileSystem.Exists(srcPath))
            {
                // Get all the files in the folder
                var files = from file in fileSystem.GetEntities(srcPath)
                    where file.GetExtension() == ".png"
                    select file;

                foreach (var file in files)
                {
                    var name = file.EntityName.Substring(0, file.EntityName.Length - file.GetExtension().Length);

                    var bytes = fileSystem.OpenFile(file, FileAccess.Read).ReadAllBytes();

                    if (fileData.ContainsKey(name))
                        fileData[name] = bytes;
                    else
                        fileData.Add(name, bytes);

                    count++;
//                    Console.WriteLine("Parse File " + name);
                }

                try
                {
                    var exportService = locator.GetService(typeof(ExportService).FullName) as ExportService;


                    // TODO exporting sprites doesn't work
                    if (exportService != null)
                    {
                        exportService.ExportSpriteBuilder(path + "sb-sprites.lua", targetGame, fileData);
//
                        exportService.StartExport();
                    }
                }
                catch (Exception e)
                {
                    // TODO this needs to go through the error system?
                    Console.WriteLine(e);
                    throw;
                }
            }

            return count;
        }
    }
}