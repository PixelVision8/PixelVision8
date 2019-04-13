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
using System.Linq;
using System.Text;
using PixelVision8.Engine.Services;
using PixelVision8.Runner.Workspace;

namespace PixelVision8.Runner.Services
{
    public class WorkspaceService : AbstractService
    {

        #region Default paths
    
        public FileSystemPath TmpFileSystemPath { get; set; } = FileSystemPath.Root.AppendDirectory("Tmp");
        

        #endregion
        
        public List<string> archiveExtensions;

        public List<string> fileExtensions = new List<string>
        {
            "png",
            "lua",
            "json",
            "txt"
        };

        protected FileSystemMounter fileSystem;
        protected FileSystemPath logFilePath;
        protected LogService logService;
        protected IFileSystem currentDisk;
        public FileSystemPath osLibPath;
        public FileSystemPath workspaceLibPath;
        
        public List<string> requiredFiles = new List<string>
        {
            "data.json",
            "info.json"
        };

        /// <summary>
        ///     This class manages all of the logic Pixel Vision 8 needs to create and manage the workspace.
        /// </summary>
        public WorkspaceService(KeyValuePair<FileSystemPath, IFileSystem> mountPoint)
        {
            fileSystem = new FileSystemMounter(mountPoint);
//            EntityMovers.Registration.AddLast(typeof(IFileSystem), typeof(IFileSystem), new StandardEntityMover());
//            EntityCopiers.Registration.AddLast(typeof(IFileSystem), typeof(IFileSystem), new StandardEntityCopier());
        }

        

        /// <summary>
        ///     This mounts the file system from a collection of File System Paths and File System instances.
        /// </summary>
        /// <param name="fileSystems"></param>
        public virtual void MountFileSystems(Dictionary<FileSystemPath, IFileSystem> fileSystems)
        {
            // Create a new File System
            foreach (var mountPoint in fileSystems)
            {
                fileSystem.Mounts.Add(new KeyValuePair<FileSystemPath, IFileSystem>(mountPoint.Key, mountPoint.Value));
            }
        }
        
        public bool ValidateGameInDir(FileSystemPath filePath)
        {
            if (!fileSystem.Exists(filePath))
                return false;

            var flag = 0;

            foreach (var file in requiredFiles)
                if (fileSystem.Exists(filePath.AppendFile(file)))
                    flag++;

            return flag == requiredFiles.Count;
        }

        public bool ValidateGameInDir(IFileSystem target)
        {
            var flag = 0;

            try
            {
                foreach (var file in requiredFiles)
                    if (target.Exists(FileSystemPath.Root.AppendFile(file)))
                        flag++;
            }
            catch
            {
                // Ignore errors if files are not found
            }


            return flag == requiredFiles.Count;
        }

        public string FindValidSavePath(string gamePath)
        {
            var savePath = "/";

            var filePath = FileSystemPath.Parse(gamePath);


            var parentFilePath = filePath.ParentPath;

            var writeAccess = WriteAccess(parentFilePath);

            if (writeAccess)
            {
                savePath = "/Game/";

                if (filePath.IsFile) savePath += filePath.EntityName;
            }
            else
            {
                savePath = "/Tmp" + gamePath;
            }

//            Console.WriteLine("Save Path " + savePath);

            return savePath;
        }

        public void SaveExporterFiles(Dictionary<string, byte[]> files)
        {
            // Save all the files to the disk
            foreach (var file in files)
                try
                {
                    // Anything that is not in the "/Workspace/" root is routed to save into the tmp directory
                    var path = FileSystemPath.Parse(file.Key);

//                    Console.WriteLine("Save Exported file " + file.Key + " to " + path);

                    Stream stream;


                    if (fileSystem.Exists(path))
                    {
                        stream = fileSystem.OpenFile(path, FileAccess.ReadWrite);
                    }
                    else
                    {
                        // Make sure we have the correct directory structure
                        if (path.IsFile)
                        {
                            // Get the parent path
                            var parent = path.ParentPath;

                            if (!fileSystem.Exists(parent)) fileSystem.CreateDirectoryRecursive(parent);
                        }

                        // Create a new file
                        // Create a new file
                        stream = fileSystem.CreateFile(path);
                    }

                    // TODO need to write to the file
                    if (file.Value != null)
                    {
                        // Clear the file contents before writing to it
                        stream.SetLength(0);

                        // Write the byte data to it
                        stream.Write(file.Value);

                        stream.Close();
                        // TODO make sure we dispose of the stream?
                        stream.Dispose();
                    }
                }
                catch
                {
//                    Console.WriteLine("Couldn't save " + file.Key + "\n" + e.Message);
                }

        }

        public string[] SplitFileName(FileSystemPath filePath)
        {
            var split = filePath.EntityName.Split('.').ToList();

            var results = new string[2];
            results[0] = split[0];

            split.RemoveAt(0);

            if (filePath.IsFile)
                results[1] = "." + string.Join(".", split);
            else
                results[1] = "";

            return results;
        }

        public FileSystemPath UniqueFilePath(FileSystemPath path)
        {
            if (!fileSystem.Exists(path)) return path;

            var fileSplit = SplitFileName(path);
            var name = fileSplit[0];

            var ix = 0;
            FileSystemPath filePath;

            do
            {
                ix++;
                filePath = path.ParentPath;

                if (path.IsDirectory)
                    filePath = filePath.AppendDirectory(string.Format("{0}{1}", name, ix));
                else
                    filePath = filePath.AppendFile(string.Format("{0}{1}{2}", name, ix, fileSplit[1]));

//                Console.WriteLine("Path " + filePath.Path);
            } while (fileSystem.Exists(filePath));

            return filePath;
        }

        public bool WriteAccess(FileSystemPath path)
        {
            var canWrite = false;

            try
            {
                // We need to make sure we have a directory to write to
                var filePath = path.IsDirectory ? path : path.ParentPath;

                // Make sure the directory exists first
                if (fileSystem.Exists(filePath))
                    try
                    {
                        // Create a unique folder path name
                        var uniqueFolderPath = filePath.AppendDirectory(DateTime.Now.ToString("yyyyMMddHHmmssfff"));

                        // Create the unique folder
                        fileSystem.CreateDirectory(uniqueFolderPath);

                        // If we don't throw an error (which is caught above) we have written to the directory
                        canWrite = true;

                        // Delete the folder we just created
                        fileSystem.Delete(uniqueFolderPath);
                    }
                    catch
                    {
//                        runner.DisplayWarning("'"+path+"' does not have write access");
                        // Can't write a file
                    }
            }
            catch
            {
//                Console.WriteLine("Workspace Write Error:\n"+e.Message);
//                Console.WriteLine(e);
//                throw;
            }


            return canWrite;
        }

        public void SetupLogFile(FileSystemPath filePath)
        {
            if (logService == null)
            {
                var total = 100;//MathHelper.Clamp(Convert.ToInt32((long) ReadBiosData("TotalLogItems", 100L, true)), 1, 500);

                logService = new LogService(total);
            }

            logFilePath = filePath;

            UpdateLog("Debug Log Created " + DateTime.Now.ToString("yyyyMMddHHmmssfff"));
        }

        public virtual void UpdateLog(string logString, LogType type = LogType.Log, string stackTrace = "")
        {
            if (logService == null) return;

            logService.UpdateLog(logString, type, stackTrace);

            SaveTextToFile(logFilePath, logService.ReadLog(), true);
        }

        public void ClearLog()
        {
            // Clear all the log entries
            logService.Clear();

            // Update the log file now that it is empty
            SaveTextToFile(logFilePath, logService.ReadLog(), true);
        }

        public List<string> ReadLogItems()
        {
            return logService.ReadLogItems();
        }

        #region File IO

        public string ReadTextFromFile(FileSystemPath filePath)
        {
//            var filePath = FileSystemPath.Parse(path);

            if (fileSystem.Exists(filePath))
            {
                var text = "";

                using (var file = fileSystem.OpenFile(filePath, FileAccess.Read))
                {
                    text = file.ReadAllText();
                    file.Close();
                    file.Dispose();
                }

                return text; //file.ReadAllText();
            }

            // Always return an empty string if no file was found
            return "";
        }

        public bool SaveTextToFile(FileSystemPath filePath, string text, bool autoCreate = false)
        {

            Stream file = null;

            if (fileSystem.Exists(filePath)) fileSystem.Delete(filePath);

            // TODO need to look into how to clear the bytes before writing to it?
            file = fileSystem.CreateFile(filePath);

            if (file != null)
            {
                var bytes = Encoding.ASCII.GetBytes(text);
                file.Write(bytes);

                file.Close();

                return true;
            }

            return false;
        }

        #endregion

        
        public Dictionary<string, byte[]> LoadGame(string path)
        {
            var filePath = FileSystemPath.Parse(path); //FileSystemPath.Root.AppendPath(fullPath);
            var exits = fileSystem.Exists(filePath);

            Dictionary<string, byte[]> files = null;
            
            if (exits)
                try
                {
                    // Found disk to load
                    if (filePath.IsDirectory)
                        currentDisk = new SubFileSystem(fileSystem, filePath);
                    else if (filePath.IsFile)
                        if (archiveExtensions.IndexOf(filePath.Path.Split('.').Last()) > -1)
                            using (var stream = fileSystem.OpenFile(filePath, FileAccess.ReadWrite))
                            {
                                if (stream is FileStream)
                                    currentDisk = ZipFileSystem.Open((FileStream) stream);
                                else
                                    currentDisk = ZipFileSystem.Open(stream, path);

                                stream.Close();
                            }

                    // We need to get a list of the current mounts
                    var mounts = fileSystem.Mounts as SortedList<FileSystemPath, IFileSystem>;

                    // Create a new mount point for the current game
                    var rootPath = FileSystemPath.Root.AppendDirectory("Game");

                    // Make sure we don't have a disk with the same name
                    if (mounts.ContainsKey(rootPath)) mounts.Remove(rootPath);

                    mounts.Add(rootPath, currentDisk);

                    files = ConvertDiskFilesToBytes(currentDisk);

                    IncludeLibDirectoryFiles(files);


                    try
                    {
                        // Convert the path to a system path
                        var tmpFilePath = FileSystemPath.Parse(path);


                        // TODO should we still try to load the saves file from a zip?

                        // If the path is a directory we are going to look for a save file in it
                        if (tmpFilePath.IsDirectory)
                        {
                            tmpFilePath = tmpFilePath.AppendFile("saves.json");

//                        if (WriteAccess(tmpFilePath) == false)
//                        {
                            // Check if save file is in tmp directory
                            var saveFile = FileSystemPath.Parse(FindValidSavePath(tmpFilePath.Path));

                            if (saveFile.Path != "/" && fileSystem.Exists(saveFile))
                                using (var memoryStream = new MemoryStream())
                                {
                                    using (var file = fileSystem.OpenFile(saveFile, FileAccess.Read))
                                    {
                                        file.CopyTo(memoryStream);
                                        file.Close();
                                    }

                                    var fileName = saveFile.EntityName;
                                    var data = memoryStream.ToArray();

                                    if (files.ContainsKey(fileName))
                                        files[fileName] = data;
                                    else
                                        files.Add(fileName, data);

                                    memoryStream.Close();
                                }
                        }


//                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    

//                    return true;
                }
                catch
                {
                    //		        // TODO need to have a clearer messgae, like not a mount point or can't load from X because of Y
//                    Console.WriteLine("System Error: Could not load from path " + filePath.Path);
                }

            return files;
        }

        public virtual void IncludeLibDirectoryFiles(Dictionary<string, byte[]> files)
        {
            // Create paths to the System/Libs and Workspace/Libs folder
            var paths = new List<FileSystemPath>
            {
                // Look in the system folder
                osLibPath
                ,
                workspaceLibPath
                // Look in the workspace folder
                
            };

            AddExtraFiles(files, paths);
        }

        public IFileSystem ReadDisk(FileSystemPath path)
        {
            IFileSystem disk = null;

            // Found disk to load
            if (path.IsDirectory)
                disk = new SubFileSystem(fileSystem, path);
            else if (path.IsFile)
                if (archiveExtensions.IndexOf(path.Path.Split('.').Last()) > -1)
                    using (var stream = ZipFileSystem.Open(fileSystem.OpenFile(path, FileAccess.Read) as FileStream))
                    {
                        disk = stream;

                        // TODO need to see how we can close the stream?
//                        stream.Close();
                    }

            return disk;
        }

        public Dictionary<string, byte[]> ConvertDiskFilesToBytes(IFileSystem disk)
        {
            var files = new Dictionary<string, byte[]>();
            var tmpFiles = disk.GetEntities(FileSystemPath.Root);

            var list = from p in tmpFiles
                where fileExtensions.Any(val => p.EntityName.EndsWith(val))
                select p;

            foreach (var file in list)
                // TODO Track if file is critical
                using (var memoryStream = new MemoryStream())
                {
                    using (var fileStream = disk.OpenFile(file, FileAccess.Read))
                    {
                        fileStream.CopyTo(memoryStream);
                        fileStream.Close();
                    }

                    ;

                    files.Add(file.EntityName, memoryStream.ToArray());
                }

            return files;
        }

        public void AddExtraFiles(Dictionary<string, byte[]> files, List<FileSystemPath> paths)
        {
            var libs = new Dictionary<string, byte[]>();

            foreach (var path in paths)
                try
                {
                    if (fileSystem.Exists(path))
                    {
                        var tmpFiles = fileSystem.GetEntities(path);
                        var luaFiles = from p in tmpFiles
                            where p.EntityName.EndsWith("lua")
                            select p;

//                        Print("Loading", luaFiles.Count(), "Libs from", path.Path);

                        foreach (var luaFile in luaFiles)
                            if (!files.ContainsKey(luaFile.EntityName))
                                using (var memoryStream = new MemoryStream())
                                {
                                    using (var fileStream = fileSystem.OpenFile(luaFile, FileAccess.Read))
                                    {
                                        fileStream.CopyTo(memoryStream);
                                        fileStream.Close();
                                    }

                                    if (libs.ContainsKey(luaFile.EntityName))
                                        libs[luaFile.EntityName] = memoryStream.ToArray();
                                    else
                                        libs.Add(luaFile.EntityName, memoryStream.ToArray());

//                                    Print("Adding Lua File", luaFile.EntityName);
                                }
                    }
                }
                catch
                {
//                    Console.WriteLine(e);
//                    throw;
                }


            // Add the libs to the file list

            libs.ToList().ForEach(x => files.Add(x.Key, x.Value));
        }

        
        
        public virtual void ShutdownSystem()
        {
//            var tmpPath = FileSystemPath.Parse("/Tmp/");

            if (fileSystem.Exists(TmpFileSystemPath))
                foreach (var entities in fileSystem.GetEntities(TmpFileSystemPath))
                    fileSystem.Delete(entities);
        }

        #region FileSystem IO

        // TODO All paths going into the Workspace should be string

        public ICollection<FileSystemPath> GetEntities(FileSystemPath path)
        {
            return fileSystem.GetEntities(path);
        }
        
        public bool Exists(FileSystemPath path)
        {
            return fileSystem.Exists(path);
        }

        public Stream CreateFile(FileSystemPath path)
        {
            return fileSystem.CreateFile(path);
        }

        public Stream OpenFile(FileSystemPath path, FileAccess access)
        {
            return fileSystem.OpenFile(path, access);
        }

        public void CreateDirectory(FileSystemPath path)
        {
            fileSystem.CreateDirectory(path);
        }

        public void Delete(FileSystemPath path)
        {
            fileSystem.Delete(path);
        }

        public void Copy(FileSystemPath src, FileSystemPath dest)
        {
            fileSystem.Copy(src, fileSystem, dest);
        }
        
        public void Move(FileSystemPath src, FileSystemPath dest)
        {
            fileSystem.Move(src, fileSystem, dest);
        }

        public void CreateDirectoryRecursive(FileSystemPath path)
        {
            fileSystem.CreateDirectoryRecursive(path);
        }

        public void AddMount(KeyValuePair<FileSystemPath, IFileSystem> mount)
        {
            fileSystem.Mounts.Add(mount);
        }
        
        #endregion

    }
}