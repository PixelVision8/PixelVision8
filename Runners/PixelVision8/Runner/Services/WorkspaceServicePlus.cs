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
using PixelVision8.Engine.Utils;
using PixelVision8.Runner.Utils;
using PixelVision8.Runner.Workspace;

namespace PixelVision8.Runner.Services
{
    public class WorkspaceServicePlus : WorkspaceService
    {
//        public DiskDriveService diskDrives;
        public string spriteBuilderFolderName = "SpriteBuilder";
        protected FileSystemMounter diskMounter = new FileSystemMounter();
        
        public WorkspaceServicePlus(KeyValuePair<WorkspacePath, IFileSystem> mountPoint) : base(mountPoint)
        {
        }

        public int totalDisks => 2;//Exists(WorkspacePath.Root.AppendDirectory("Workspace")) ? 3 : 3;

        public void CreateWorkspaceDrive(string name)
        {
            
        }
        
        public void MountWorkspace(string name)
        {

//            var osPath = FileSystemPath.Root.AppendDirectory("PixelVisionOS");
//            
//            var mounts = Mounts as SortedList<FileSystemPath, IFileSystem>;
//            
//            mounts.Remove()
//            
            var filePath = WorkspacePath.Root.AppendDirectory("User");

            // Make sure that the user directory exits
            if (Exists(filePath))
            {
                filePath = filePath.AppendDirectory(name);

                // If the filesystem doesn't exit, we want to create it
                if (!Exists(filePath)) 
                    CreateDirectory(filePath);
            }

            var workspaceDisk = new SubFileSystem(this, filePath);

            Mounts.Add(
                new KeyValuePair<WorkspacePath, IFileSystem>(WorkspacePath.Root.AppendDirectory("Workspace"),
                    workspaceDisk));


//            RebuildWorkspace();
        }

        public void RebuildWorkspace()
        {
            var osPath = WorkspacePath.Root.AppendDirectory("PixelVisionOS");
            
            var mounts = Mounts as SortedList<WorkspacePath, IFileSystem>;

            if (mounts.ContainsKey(osPath))
            {
                mounts.Remove(osPath);
            }
            
            var systemPaths = new List<IFileSystem>()
            {
                new SubFileSystem(this,
                    WorkspacePath.Root.AppendDirectory("App").AppendDirectory("PixelVisionOS"))
            };
            
            // Create a path to the workspace system folder
            var path = WorkspacePath.Root.AppendDirectory("Workspace").AppendDirectory("System");

                // Look to see if the workspace system folder exists
            if (Exists(path))
            {

                // Add the workspace system folder to the os file system
                systemPaths.Add(new SubFileSystem(this, path));

            }
 
            // Mount the PixelVisionOS directory
            AddMount(new KeyValuePair<WorkspacePath, IFileSystem>(osPath, new MergedFileSystem(systemPaths)));
            
        }

        // Exports the active song in the music chip
        public void ExportSong(string path, MusicChip musicChip, SoundChip soundChip)
        {
            var filePath = WorkspacePath.Parse(path);

            if (Exists(filePath))
            {
                filePath = filePath.AppendDirectory("Patterns");

                if (!Exists(filePath)) CreateDirectory(filePath);

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

        public override void MountFileSystems(Dictionary<WorkspacePath, IFileSystem> fileSystems)
        {
            base.MountFileSystems(fileSystems);
            
            // Create a mount point for disks

            diskMounter = new FileSystemMounter();

            // Add the disk mount point
            Mounts.Add(
                new KeyValuePair<WorkspacePath, IFileSystem>(WorkspacePath.Root.AppendDirectory("Disks"),
                    diskMounter));


            // Create a disk drive service to mange the disks
//            diskDrives = new DiskDriveService(diskMounter, totalDisks);

            diskMount = diskMounter.Mounts as SortedList<WorkspacePath, IFileSystem>;
        }
        
        public Dictionary<string, string> DiskPaths()
        {
            var pathRefs = new Dictionary<string, string>();

            var mounts = disks; //disks.Mounts as SortedList<FileSystemPath, IFileSystem>;

            for (var i = 0; i < mounts.Length; i++)
            {
                var path = mounts[i];
                pathRefs.Add(path.EntityName, "/Disks" + path.Path);
            }

            return pathRefs;
        }

        public string MountDisk(string path)
        {

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
                    disk.Exists(WorkspacePath.Root.AppendFile("info.json")) == false) return null;

                // Update the root path to just be the name of the entity
                var rootPath = WorkspacePath.Root.AppendDirectory(entityName);

                // Add the new disk
                AddDisk(rootPath, disk);

                // Return the disk name
                return entityName;
                
        }

        public string AutoRunGameFromDisk(string diskName)
        {
            var diskPath = WorkspacePath.Root.AppendDirectory("Disks")
                .AppendDirectory(diskName);


            var autoRunPath = diskPath.AppendFile("info.json");

            // Try to read the disk's info file and see if there is an auto run path
            try
            {
                // Only run a disk if there is an auto run file in there
                if (Exists(autoRunPath))
                {
                    var json = ReadTextFromFile(autoRunPath);

                    var autoRunData = Json.Deserialize(json) as Dictionary<string, object>;

                    var tmpPath = autoRunData["AutoRun"] as string;

                    // Get the auto run from the json file
                    var newDiskPath = WorkspacePath.Parse("/Disks/" + diskName + tmpPath);

                    // Change the disk path to the one in the auto-run file
                    if (Exists(newDiskPath)) diskPath = newDiskPath;
                }
            }
            catch
            {
                // ignored
            }

            // Always validate that the disk is a valid game before trying to load it.
            if (ValidateGameInDir(diskPath))
            {
                
                // TODO need to make sure the auto run disk is at the top of the list

                var diskPaths = new List<string>();
                
                foreach (var disk in diskMount)
                {
                    var name = disk.Key.EntityName;

                    var physicalPath = ((PhysicalFileSystem) disk.Value).PhysicalRoot;
                    
                    if (name == diskName)
                    {
                        diskPaths.Insert(0, physicalPath);
                    }
                    else
                    {
                        diskPaths.Add(physicalPath);
                    }
                    
                }
                
                EjectAll();

                foreach (var oldPath in diskPaths)
                {
                    MountDisk(oldPath);
                }
                
//                SortedList<WorkspacePath, IFileSystem> 
//                
//                    diskMount
                    
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


        public void SaveActiveDisk()
        {

            if (currentDisk is ZipFileSystem disk)
            {
//                Console.WriteLine("Workspace is ready to save active disk");

//                disk.Save();
                var fileNameZip = disk.srcPath;

                var files = currentDisk.GetEntitiesRecursive(WorkspacePath.Root);

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


            var mounts = Mounts as SortedList<WorkspacePath, IFileSystem>;

            // Create a new mount point for the current game
            var rootPath = WorkspacePath.Root.AppendDirectory("Game");

            // Make sure we don't have a disk with the same name
            if (mounts.ContainsKey(rootPath)) mounts.Remove(rootPath);
        }

        public override void IncludeLibDirectoryFiles(Dictionary<string, byte[]> files)
        {
            base.IncludeLibDirectoryFiles(files);
                
            var paths = new List<WorkspacePath>();
            
            var diskPaths = disks;

            var totalDisks = diskPaths.Length - 1;

            // Loop backwards through disks
            for (var i = totalDisks; i >= 0; i--)
            {
                var diskPath = WorkspacePath.Root.AppendDirectory("Disks")
                    .AppendPath(diskPaths[i].AppendDirectory("System").AppendDirectory("Libs"));

                paths.Add(diskPath);
            }

            AddExtraFiles(files, paths);
            
        }

        public override void ShutdownSystem()
        {
            // make sure we have the current list of disks in the bios
//            UpdateDiskInBios();
//            var disks = disks;

            foreach (var disk in disks) SaveDisk(disk);

            base.ShutdownSystem();
        }

        // Make sure you can only eject a disk by forcing the path to be in the disk mounts scope
        public void EjectDisk(WorkspacePath? filePath = null)
        {
            // Remove the disk if disks exists
            if (total > 0)
                try
                {
                    // Use the path that is supplied or get the first disk path
                    var path = filePath.HasValue ? filePath.Value : disks.First();

                    // Attempt to remove the disk
                    RemoveDisk(path);

                    // Update the bios when a disk is removed
//                    UpdateDiskInBios();
                }
                catch
                {
                    // ignored error when removing a disk that doesn't exist
                }

            // What happens when there are no disks
            if (total > 0)
                try
                {
                    // Get the next disk name
                    var diskName = disks.First().Path.Replace("/", "");

                    // Clear the history
//                    runner.loadHistory.Clear();

                    // Attempt to run the fist disk
                    AutoRunGameFromDisk(diskName);

                }
                catch
                {
                    // ignored
                }
        }

        // TODO this is hardcoded but should come from the bios
        
        
        public bool ValidateSpriteBuilderFolder(WorkspacePath rootPath)
        {
            // TODO need to make sure this is in the current game directory and uses the filesyem path
            rootPath = rootPath.AppendDirectory(spriteBuilderFolderName);//(string) ReadBiosData("SpriteBuilderDir", "SpriteBuilder"));

            return Exists(rootPath);
        }

        public int GenerateSprites(string path, PixelVisionEngine targetGame)
        {
            var count = 0;

            var filePath = WorkspacePath.Parse(path);

            var srcPath = filePath.AppendDirectory("SpriteBuilder");

            var fileData = new Dictionary<string, byte[]>();

            if (Exists(srcPath))
            {
                // Get all the files in the folder
                var files = from file in GetEntities(srcPath)
                    where file.GetExtension() == ".png"
                    select file;

                foreach (var file in files)
                {
                    var name = file.EntityName.Substring(0, file.EntityName.Length - file.GetExtension().Length);

                    var bytes = OpenFile(file, FileAccess.Read).ReadAllBytes();

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
        
        private SortedList<WorkspacePath, IFileSystem> diskMount;
        private List<WorkspacePath> diskNames = new List<WorkspacePath>();

//        public int totalDisks = 5;

//        public DiskDriveService(FileSystemMounter diskMount, int totalDisks)
//        {
//            this.diskMount = diskMount.Mounts as SortedList<FileSystemPath, IFileSystem>;
//
//            this.totalDisks = totalDisks;
//        }

        public int total => diskNames.Count;

        public WorkspacePath[] disks => diskNames.ToArray();

        public string[] physicalPaths
        {
            get
            {
                var paths = new string[totalDisks];

                for (var i = 0; i < totalDisks; i++)
                {
                    var tmpPath = "none";

                    if (i < total)
                    {
                        var key = disks[i];

                        if (diskMount.ContainsKey(key))
                        {
                            var disk = diskMount[key];

                            if (disk is PhysicalFileSystem src)
                                tmpPath = src.PhysicalRoot;
                            else if (disk is ZipFileSystem zipDisk) tmpPath = zipDisk.srcPath;
                        }
                    }

                    paths[i] = tmpPath;
                }

                return paths;
            }
        }

        public void AddDisk(WorkspacePath path, IFileSystem disk)
        {
            // If for some reason we don't have a mount point we need to exit out of this method
            if (diskMount == null)
                return;

            // If we are out of open disks, remove the last one
            if (total == totalDisks) RemoveDisk(diskNames.Last());

            // Attempt to remove the disk if it is already inserted
            RemoveDisk(path);

            // Add the new disk to the disk mount
            diskMount.Add(path, disk);

            // Add the disk path to the list of names
            diskNames.Add(path);
        }

        public void RemoveDisk(WorkspacePath path)
        {
            if (diskMount.ContainsKey(path))
            {
                // Check to see if this is a zip
                SaveDisk(path);

                // Remove disk from the mount point
                diskMount.Remove(path);

                // Remove the disk from the list of name
                diskNames.Remove(path);
            }
        }

        public void SaveDisk(WorkspacePath path)
        {
//            Console.WriteLine("Attempting to save disk " + path);

            if (diskMount.ContainsKey(path))
            {
//                Console.WriteLine("Found disk " + path);

                var mount = diskMount[path];
                if (mount is ZipFileSystem) ((ZipFileSystem) mount).Save();
            }
        }

        public void EjectAll()
        {
//            var names = disks;

            foreach (var path in disks) RemoveDisk(path);
        }

        
    }
}