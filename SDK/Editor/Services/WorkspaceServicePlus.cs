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

using PixelVision8.Player;
using PixelVision8.Runner.Exporters;
using PixelVision8.Runner;
using PixelVision8.Workspace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PixelVision8.Editor
{
    public class WorkspaceServicePlus : WorkspaceService
    {
        //        private bool disksInvalid = true;
        private readonly List<WorkspacePath> _disks = new List<WorkspacePath>();

        public WorkspaceServicePlus(KeyValuePair<WorkspacePath, IFileSystem> mountPoint) : base(mountPoint)
        {
        }

        //        private SortedList<WorkspacePath, IFileSystem> DiskMount => Mounts as SortedList<WorkspacePath, IFileSystem>;
        public int TotalDisks => _disks.Count;
        public int MaxDisks { get; set; } = 2;

        public WorkspacePath[] Disks => _disks.ToArray();

        public void MountWorkspace(string name)
        {
            var filePath = WorkspacePath.Root.AppendDirectory("User");

            // Make sure that the user directory exits
            if (Exists(filePath))
            {
                filePath = filePath.AppendDirectory(name);

                // If the filesystem doesn't exit, we want to create it
                if (!Exists(filePath)) CreateDirectory(filePath);
            }

            var workspaceDisk = new SubFileSystem(this, filePath);

            Mounts.Add(
                new KeyValuePair<WorkspacePath, IFileSystem>(WorkspacePath.Root.AppendDirectory("Workspace"),
                    workspaceDisk));
        }

        public void RebuildWorkspace()
        {
            var osPath = WorkspacePath.Root.AppendDirectory("PixelVisionOS");

            if (Exists(osPath)) Mounts.Remove(Get(osPath));

            var systemPaths = new List<IFileSystem>
            {
                new SubFileSystem(this,
                    WorkspacePath.Root.AppendDirectory("App").AppendDirectory("PixelVisionOS"))
            };

            // Create a path to the workspace system folder
            var path = WorkspacePath.Root.AppendDirectory("Workspace").AppendDirectory("System");

            // Look to see if the workspace system folder exists
            if (Exists(path))
                // Add the workspace system folder to the os file system
                systemPaths.Add(new SubFileSystem(this, path));

            // Mount the PixelVisionOS directory
            AddMount(new KeyValuePair<WorkspacePath, IFileSystem>(osPath, new MergedFileSystem(systemPaths)));
        }

        // Exports the active song in the music chip
        public void ExportSong(string path, MusicChip musicChip, SoundChip soundChip, int id)
        {
            var currentSong = musicChip.songs[id];

            var selectedPatterns = new int[currentSong.end];

            Array.Copy(currentSong.patterns, selectedPatterns, selectedPatterns.Length);

            var filePath = WorkspacePath.Parse(path);

            if (Exists(filePath))
            {
                filePath = filePath.AppendDirectory("Wavs").AppendDirectory("Songs");

                if (!Exists(filePath)) CreateDirectory(filePath);

                try
                {
                    filePath = UniqueFilePath(filePath.AppendFile("song " + id + " - " + currentSong.name + ".wav"));

                    Console.WriteLine("Export song to " + filePath);


                    // TODO exporting sprites doesn't work
                    if (locator.GetService(typeof(GameDataExportService).FullName) is GameDataExportService
                        exportService)
                    {
                        exportService.ExportSong(filePath.Path, musicChip, soundChip, selectedPatterns);
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

        public void ExportPattern(WorkspacePath filePath, MusicChip musicChip, SoundChip soundChip, int id)
        {
            var selectedPatterns = new int[id];

            if (!Exists(filePath)) CreateDirectory(filePath);

            try
            {
                filePath = UniqueFilePath(filePath.AppendFile("pattern+" + id + ".wav"));


                // TODO exporting sprites doesn't work
                if (locator.GetService(typeof(GameDataExportService).FullName) is GameDataExportService exportService)
                {
                    exportService.ExportSong(filePath.Path, musicChip, soundChip, selectedPatterns);
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

            if (disk == null) return null;

            // Test to see if the disk is a valid game
            if (ValidateGameInDir(disk) == false &&
                disk.Exists(WorkspacePath.Root.AppendFile("info.json")) == false)
                return null;

            // Update the root path to just be the name of the entity
            var rootPath = WorkspacePath.Root.AppendDirectory("Disks").AppendDirectory(entityName);

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
                    var newDiskPath = WorkspacePath.Parse($"/Disks/{diskName}{tmpPath}");

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

                // Move the new disk to the top of the list
                var diskPaths = new List<string>();

                // Add the remaining disks
                foreach (var disk in Disks) diskPaths.Add(DiskPhysicalRoot(disk));

                // TODO this shouldn't eject the disk  that is about to be loaded since it  could force a zip file to be rewritten before it is loaded
                // Remove the old disks
                EjectAll();

                // Mount all the disks
                foreach (var oldPath in diskPaths) MountDisk(oldPath);

                return diskPath.Path;
            }

            return null;
        }

        public string DiskPhysicalRoot(WorkspacePath disk)
        {
            var physicalPath = "";

            if (Exists(disk))
            {
                if (Get(disk).Value is PhysicalFileSystem fileSystem)
                    physicalPath = fileSystem.PhysicalRoot;
                else if (Get(disk).Value is ZipFileSystem system) physicalPath = system.PhysicalRoot;
            }

            return physicalPath;
        }


        public void SaveActiveDisk()
        {
            // Create a new mount point for the current game
            var rootPath = WorkspacePath.Root.AppendDirectory("Game");

            // Save the active disk if it is a zip file system
            if (currentDisk is ZipFileSystem disk) SaveDisk(rootPath);

            // Make sure we don't have a disk with the same name
            if (Exists(rootPath)) Mounts.Remove(Get(rootPath));
        }

        public override void ShutdownSystem()
        {
            foreach (var disk in Disks) SaveDisk(disk);

            base.ShutdownSystem();
        }

        // Make sure you can only eject a disk by forcing the path to be in the disk mounts scope
        public void EjectDisk(WorkspacePath filePath)
        {
            RemoveDisk(filePath);

            // What happens when there are no disks
            if (TotalDisks > 1)
                try
                {
                    // Get the next disk name
                    var diskName = Disks.First().Path.Replace("/", "");

                    // Attempt to run the fist disk
                    AutoRunGameFromDisk(diskName);
                }
                catch
                {
                    // ignored
                }
        }

        public int GenerateSprites(string path, PixelVision targetGame)
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
                    // TODO exporting sprites doesn't work
                    if (locator.GetService(typeof(GameDataExportService).FullName) is GameDataExportService
                        exportService)
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

        public void AddDisk(WorkspacePath path, IFileSystem disk)
        {
            // If we are out of open disks, remove the last one
            if (TotalDisks == MaxDisks) RemoveDisk(Disks.Last());

            // Attempt to remove the disk if it is already inserted
            RemoveDisk(path);

            // Add the new disk to the disk mount
            Mounts.Add(new KeyValuePair<WorkspacePath, IFileSystem>(path, disk));

            if (!_disks.Contains(path)) _disks.Add(path);
            //            InvalidateDisks();
        }

        public void RemoveDisk(WorkspacePath path)
        {
            if (Exists(path))
            {
                // Check to see if this is a zip
                SaveDisk(path);

                // Remove disk from the mount point
                Mounts.Remove(Get(path));

                if (_disks.Contains(path)) _disks.Remove(path);
                //                
                //                InvalidateDisks();
            }
        }

        public void SaveDisk(WorkspacePath path)
        {
            var diskExporter = new ZipDiskExporter(path.Path, this);
            diskExporter.CalculateSteps();

            while (diskExporter.Completed == false)
            {
                diskExporter.NextStep();
            }
        }

        public Dictionary<string, object> CreateZipFile(WorkspacePath path,
            Dictionary<WorkspacePath, WorkspacePath> files)
        {
            var fileHelper = new WorkspaceFileLoadHelper(this);
            var zipExporter = new ZipExporter(path.Path, fileHelper, files);
            zipExporter.CalculateSteps();

            while (zipExporter.Completed == false)
            {
                zipExporter.NextStep();
            }

            try
            {
                if ((bool) zipExporter.Response["success"])
                {
                    var zipPath = WorkspacePath.Parse(zipExporter.fileName);

                    SaveExporterFiles(new Dictionary<string, byte[]>() {{zipExporter.fileName, zipExporter.Bytes}});
                }
            }
            catch (Exception e)
            {
                // Change the success to false
                zipExporter.Response["success"] = false;
                zipExporter.Response["message"] = e.Message;
            }


            return zipExporter.Response;
        }

        public void EjectAll()
        {
            foreach (var path in Disks) RemoveDisk(path);
        }

        public override bool Exists(WorkspacePath path)
        {
            // Manually return true if the path is Disks since it's not a real mount point
            if (path == WorkspacePath.Root.AppendDirectory("Disks")) return MaxDisks > 0;

            return base.Exists(path);
        }

        public override List<WorkspacePath> SharedLibDirectories()
        {
            // Create paths to the System/Libs and Workspace/Libs folder
            var paths = base.SharedLibDirectories();

            // Add disks
            for (int i = 0; i < TotalDisks; i++)
            {
                var tmpPath = Disks[i].AppendDirectory("System").AppendDirectory("Libs");

                if (Exists(tmpPath))
                    paths.Insert(0, tmpPath);
            }

            return paths;
        }
    }
}