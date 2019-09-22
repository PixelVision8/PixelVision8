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
    public class WorkspaceService : FileSystemMounter, IService
    {

        #region Default paths
    
        public WorkspacePath TmpFileSystemPath { get; set; } = WorkspacePath.Root.AppendDirectory("Tmp");
        

        #endregion
        
        public List<string> archiveExtensions;

        public List<string> fileExtensions = new List<string>
        {
            "png",
            "lua",
            "json",
            "txt",
            "wav"
        };

//        protected FileSystemMounter fileSystem;
        protected WorkspacePath logFilePath;
        protected LogService logService;
        protected IFileSystem currentDisk;
        public WorkspacePath osLibPath;
//        public WorkspacePath workspaceLibPath;
        
        public List<string> requiredFiles = new List<string>
        {
            "data.json",
            "info.json"
        };

        /// <summary>
        ///     This class manages all of the logic Pixel Vision 8 needs to create and manage the workspace.
        /// </summary>
        public WorkspaceService(KeyValuePair<WorkspacePath, IFileSystem> mountPoint) :base(mountPoint)
        {
//            fileSystem = new FileSystemMounter(mountPoint);
//            EntityMovers.Registration.AddLast(typeof(IFileSystem), typeof(IFileSystem), new StandardEntityMover());
//            EntityCopiers.Registration.AddLast(typeof(IFileSystem), typeof(IFileSystem), new StandardEntityCopier());
        }

        

        /// <summary>
        ///     This mounts the file system from a collection of File System Paths and File System instances.
        /// </summary>
        /// <param name="fileSystems"></param>
        public virtual void MountFileSystems(Dictionary<WorkspacePath, IFileSystem> fileSystems)
        {
            // Create a new File System
            foreach (var mountPoint in fileSystems)
            {
                AddMount(new KeyValuePair<WorkspacePath, IFileSystem>(mountPoint.Key, mountPoint.Value));
            }
        }
        
        public bool ValidateGameInDir(WorkspacePath filePath)
        {
            if (!Exists(filePath))
                return false;

            var flag = 0;

            foreach (var file in requiredFiles)
                if (Exists(filePath.AppendFile(file)))
                    flag++;

            return flag == requiredFiles.Count;
        }

        public bool ValidateGameInDir(IFileSystem target)
        {
            var flag = 0;

            try
            {
                foreach (var file in requiredFiles)
                    if (target.Exists(WorkspacePath.Root.AppendFile(file)))
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

            var filePath = WorkspacePath.Parse(gamePath);


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
            // TODO the dictionary string should be converted into a Workspace path
            
            // Save all the files to the disk
            foreach (var file in files)
                try
                {
                    // Anything that is not in the "/Workspace/" root is routed to save into the tmp directory
                    var path = WorkspacePath.Parse(file.Key);

//                    Console.WriteLine("Save Exported file " + file.Key + " to " + path);

                    Stream stream;


                    if (Exists(path))
                    {
                        stream = OpenFile(path, FileAccess.ReadWrite);
                    }
                    else
                    {
                        // Make sure we have the correct directory structure
                        if (path.IsFile)
                        {
                            // Get the parent path
                            var parent = path.ParentPath;

                            if (!Exists(parent)) this.CreateDirectoryRecursive(parent);
                        }

                        // Create a new file
                        // Create a new file
                        stream = CreateFile(path);
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

        public string[] SplitFileName(WorkspacePath filePath)
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

        public WorkspacePath UniqueFilePath(WorkspacePath path)
        {
            if (!Exists(path)) return path;

            var fileSplit = SplitFileName(path);
            var name = fileSplit[0];

            var ix = 0;
            WorkspacePath filePath;

            do
            {
                ix++;
                filePath = path.ParentPath;

                if (path.IsDirectory)
                    filePath = filePath.AppendDirectory(string.Format("{0}{1}", name, ix));
                else
                    filePath = filePath.AppendFile(string.Format("{0}{1}{2}", name, ix, fileSplit[1]));

//                Console.WriteLine("Path " + filePath.Path);
            } while (Exists(filePath));

            return filePath;
        }

        public bool WriteAccess(WorkspacePath path)
        {
            var canWrite = false;

            try
            {
                // We need to make sure we have a directory to write to
                var filePath = path.IsDirectory ? path : path.ParentPath;

                // Make sure the directory exists first
                if (Exists(filePath))
                    try
                    {
                        // Create a unique folder path name
                        var uniqueFolderPath = filePath.AppendDirectory(DateTime.Now.ToString("yyyyMMddHHmmssfff"));

                        // Create the unique folder
                        CreateDirectory(uniqueFolderPath);

                        // If we don't throw an error (which is caught above) we have written to the directory
                        canWrite = true;

                        // Delete the folder we just created
                        Delete(uniqueFolderPath);
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

        public void SetupLogFile(WorkspacePath filePath)
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

        public string ReadTextFromFile(WorkspacePath filePath)
        {
//            var filePath = FileSystemPath.Parse(path);

            if (Exists(filePath))
            {
                var text = "";

                using (var file = OpenFile(filePath, FileAccess.Read))
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

        public bool SaveTextToFile(WorkspacePath filePath, string text, bool autoCreate = false)
        {

            Stream file = null;

            if (Exists(filePath)) Delete(filePath);

            // TODO need to look into how to clear the bytes before writing to it?
            file = CreateFile(filePath);

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
            var filePath = WorkspacePath.Parse(path); //FileSystemPath.Root.AppendPath(fullPath);
            var exits = Exists(filePath);

            Dictionary<string, byte[]> files = null;
            
            if (exits)
                try
                {
                    // Found disk to load
                    if (filePath.IsDirectory)
                        currentDisk = new SubFileSystem(this, filePath);
                    else if (filePath.IsFile)
                        if (archiveExtensions.IndexOf(filePath.Path.Split('.').Last()) > -1)
                            using (var stream = OpenFile(filePath, FileAccess.ReadWrite))
                            {
                                if (stream is FileStream)
                                    currentDisk = ZipFileSystem.Open((FileStream) stream);
                                else
                                    currentDisk = ZipFileSystem.Open(stream, path);

                                stream.Close();
                            }

                    // We need to get a list of the current mounts
                    var mounts = Mounts as SortedList<WorkspacePath, IFileSystem>;

                    // Create a new mount point for the current game
                    var rootPath = WorkspacePath.Root.AppendDirectory("Game");

                    // Make sure we don't have a disk with the same name
                    if (mounts.ContainsKey(rootPath)) mounts.Remove(rootPath);

                    mounts.Add(rootPath, currentDisk);

                    files = ConvertDiskFilesToBytes(currentDisk);

                    IncludeLibDirectoryFiles(files);


                    try
                    {
                        // Convert the path to a system path
                        var tmpFilePath = WorkspacePath.Parse(path);


                        // TODO should we still try to load the saves file from a zip?

                        // If the path is a directory we are going to look for a save file in it
                        if (tmpFilePath.IsDirectory)
                        {
                            tmpFilePath = tmpFilePath.AppendFile("saves.json");

//                        if (WriteAccess(tmpFilePath) == false)
//                        {
                            // Check if save file is in tmp directory
                            var saveFile = WorkspacePath.Parse(FindValidSavePath(tmpFilePath.Path));

                            if (saveFile.Path != "/" && Exists(saveFile))
                                using (var memoryStream = new MemoryStream())
                                {
                                    using (var file = OpenFile(saveFile, FileAccess.Read))
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
            var paths = new List<WorkspacePath>
            {
                // Look in the system folder
                osLibPath
//                ,
//                workspaceLibPath
                // Look in the workspace folder
                
            };

            AddExtraFiles(files, paths);
        }

        public IFileSystem ReadDisk(WorkspacePath path)
        {
            IFileSystem disk = null;

            // Found disk to load
            if (path.IsDirectory)
                disk = new SubFileSystem(this, path);
            else if (path.IsFile)
                if (archiveExtensions.IndexOf(path.Path.Split('.').Last()) > -1)
                    using (var stream = ZipFileSystem.Open(OpenFile(path, FileAccess.Read) as FileStream))
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
            
            // Get the root files
            List<WorkspacePath> list = (from p in disk.GetEntities(WorkspacePath.Root)
                where fileExtensions.Any(val => p.EntityName.EndsWith(val))
                select p).ToList();
            
            // TODO this is an example of how we can have other files in folders and import them in.
            
            // Look for any wav files in the samples folder
            
            // Samples Directory
//            var samplesPath = WorkspacePath.Root.AppendDirectory("Samples"); // TODO this should probably not be hard coded
//            
//            // Check if the directory exists
//            if (disk.Exists(samplesPath))
//            {
//                // Get all the wav files in the samples folder
//                list = list.Concat(from p in disk.GetEntities(samplesPath) where p.EntityName.EndsWith("wav") select p).ToList();
//            }

            // Loop through all the files and convert them into binary data to be used by other parser

            foreach (var file in list)
                // TODO Track if file is critical
                using (var memoryStream = new MemoryStream())
                {
                    using (var fileStream = disk.OpenFile(file, FileAccess.Read))
                    {
                        fileStream.CopyTo(memoryStream);
                        fileStream.Close();
                    }
    
//                    Console.WriteLine("Add File " + file.Path.Substring(1));
                    
                    files.Add(file.Path.Substring(1), memoryStream.ToArray());
                }

            return files;
        }

        public void AddExtraFiles(Dictionary<string, byte[]> files, List<WorkspacePath> paths)
        {
            var libs = new Dictionary<string, byte[]>();

            foreach (var path in paths)
                try
                {
                    if (Exists(path))
                    {
                        var tmpFiles = GetEntities(path);
                        var luaFiles = from p in tmpFiles
                            where p.EntityName.EndsWith("lua")
                            select p;

//                        Print("Loading", luaFiles.Count(), "Libs from", path.Path);

                        foreach (var luaFile in luaFiles)
                            if (!files.ContainsKey(luaFile.EntityName))
                                using (var memoryStream = new MemoryStream())
                                {
                                    using (var fileStream = OpenFile(luaFile, FileAccess.Read))
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

            if (Exists(TmpFileSystemPath))
                foreach (var entities in GetEntities(TmpFileSystemPath))
                    Delete(entities);
        }

        #region FileSystem IO

        
        public void Copy(WorkspacePath src, WorkspacePath dest)
        {
            this.Copy(src, this, dest);
        }
        
        public void Move(WorkspacePath src, WorkspacePath dest)
        {
            this.Move(src, this, dest);
        }

        public void AddMount(KeyValuePair<WorkspacePath, IFileSystem> mount)
        {
            Mounts.Add(mount);
        }
        
        #endregion

        public IServiceLocator locator
        {
            get;
            set;
        }

        /// <summary>
        ///     This method registers the service with the service locator.
        /// </summary>
        /// <param name="locator"></param>
        public virtual void RegisterService(IServiceLocator locator)
        {
            this.locator = locator;
        }
    }
}