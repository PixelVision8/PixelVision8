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
using PixelVision8.Workspace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PixelVision8.Runner
{
    public class WorkspaceService : FileSystemMounter, IService
    {
        public List<string> archiveExtensions;
        protected IFileSystem currentDisk;

        public List<string> fileExtensions = new List<string>
        {
            "png",
            "lua",
            "json",
            "txt",
            "wav"
        };

        protected WorkspacePath logFilePath;
        // protected LogService logService;
        public WorkspacePath osLibPath;

        public List<string> requiredFiles = new List<string>
        {
            "data.json",
            "info.json"
        };

        #region Default paths

        public WorkspacePath TmpFileSystemPath { get; set; } = WorkspacePath.Root.AppendDirectory("Tmp");

        #endregion

        public IServiceLocator locator { get; set; }
        protected bool LogInvalidated = false;

        public WorkspaceService(KeyValuePair<WorkspacePath, IFileSystem> mountPoint) : base(mountPoint)
        {
        }

        /// <summary>
        ///     This method registers the service with the service locator.
        /// </summary>
        /// <param name="locator"></param>
        public virtual void RegisterService(IServiceLocator locator)
        {
            this.locator = locator;
        }


        /// <summary>
        ///     This mounts the file system from a collection of File System Paths and File System instances.
        /// </summary>
        /// <param name="fileSystems"></param>
        public virtual void MountFileSystems(Dictionary<WorkspacePath, IFileSystem> fileSystems)
        {
            // Create a new File System
            foreach (var mountPoint in fileSystems)
                AddMount(new KeyValuePair<WorkspacePath, IFileSystem>(mountPoint.Key, mountPoint.Value));
        }

        public bool ValidateGameInDir(WorkspacePath filePath)
        {
            if (!Exists(filePath)) return false;

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
                        StreamExtensions.Write(stream, file.Value);

                        stream.Close();
                        // TODO make sure we dispose of the stream?
                        stream.Dispose();
                    }
                }

                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
        }

        internal string GetPhysicalPath(WorkspacePath filePath)
        {
            string path = filePath.Path;

            var segments = filePath.GetDirectorySegments();

            if (segments[0] == "Game")
            {
                return GetPhysicalPath(((SubFileSystem) currentDisk).Root.AppendFile(filePath.EntityName));
            }
            else if (segments[0] == "Disks")
            {
                var diskRoot = WorkspacePath.Root.AppendDirectory("Disks").AppendDirectory(segments[1]);

                foreach (var keyValuePair in Mounts)
                {
                    if (keyValuePair.Key == diskRoot)
                    {
                        if (keyValuePair.Value is PhysicalFileSystem mount)
                        {
                            var realPath = mount.PhysicalRoot;

                            for (int i = 2; i < segments.Length; i++)
                            {
                                realPath = Path.Combine(realPath, segments[i]);
                            }

                            realPath = Path.Combine(realPath, filePath.EntityName);

                            return realPath;
                        }
                    }
                }

                // TODO need to find the mount point
            }
            else if (segments[0] == "Workspace")
            {
                var userRoot = WorkspacePath.Root.AppendDirectory("User");
                // string userSystemPath;
                foreach (var keyValuePair in Mounts)
                {
                    if (keyValuePair.Key == userRoot)
                    {
                        if (keyValuePair.Value is PhysicalFileSystem mount)
                        {
                            path = mount
                                .PhysicalRoot; //Path.Combine(mount.PhysicalRoot, segments[1], filePath.EntityName);
                            break;
                        }
                    }
                }

                foreach (var segment in segments)
                {
                    path = Path.Combine(path, segment);
                }

                path = Path.Combine(path, filePath.EntityName);
            }
            else if (segments[0] == "PixelVisionOS")
            {
                // Look through all of the share libraries to find the correct path
                foreach (var sharedLibDirectory in SharedLibDirectories())
                {
                    if (Exists(sharedLibDirectory.AppendFile(filePath.EntityName)))
                    {
                        path = sharedLibDirectory.AppendFile(filePath.EntityName).Path;
                        // TODO still need to find the correct system path to any of the shared lib folders
                        if (path.StartsWith("/Disks/"))
                        {
                            path = GetPhysicalPath(sharedLibDirectory.AppendFile(filePath.EntityName));
                        }
                    }
                }
            }

            return path;
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

            // TODO need to see if the last character is a number and start from there?

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
            } while (Exists(filePath));

            return filePath;
        }

        public void SetupLogFile(WorkspacePath filePath)
        {
            // if (logService == null)
            // {
            //     var
            //         total = 500; //MathHelper.Clamp(Convert.ToInt32((long) ReadBiosData("TotalLogItems", 100L, true)), 1, 500);

            //     logService = new LogService(total);
            // }

            logFilePath = filePath;

            UpdateLog("Debug Log Created " + DateTime.Now.ToString("yyyyMMddHHmmssfff"));
        }

        public virtual void UpdateLog(string logString, LogType type = LogType.Log, string stackTrace = "")
        {
            // if (logService == null) return;

            Log.Print(logString, type, stackTrace);

            LogInvalidated = true;
        }

        public void SaveLog()
        {
            if (LogInvalidated)
            {
                SaveTextToFile(logFilePath, Log.ReadLog(), true);
                LogInvalidated = false;
            }
        }

        public void ClearLog()
        {
            // Clear all the log entries
            Log.Clear();

            // Update the log file now that it is empty
            SaveTextToFile(logFilePath, Log.ReadLog(), true);
        }

        public List<string> ReadLogItems()
        {
            return Log.ReadLogItems();
        }

        public string[] LoadGame(string path)
        {
            var filePath = WorkspacePath.Parse(path);
            var exits = Exists(filePath);

            string[] files = null;

            if (exits)
            {
                // Found disk to load
                if (filePath.IsDirectory)
                    currentDisk = new SubFileSystem(this, filePath);
                else if (filePath.IsFile)
                {
                    // TODO need to figure out how to do this from a disk now without the currentDisk drive?
                    if (archiveExtensions.IndexOf(filePath.Path.Split('.').Last()) > -1)
                        using (var stream = OpenFile(filePath, FileAccess.ReadWrite))
                        {
                            if (stream is FileStream)
                                currentDisk = ZipFileSystem.Open((FileStream) stream);
                            else
                                currentDisk = ZipFileSystem.Open(stream, path);

                            stream.Close();
                        }
                }

                // We need to get a list of the current mounts

                if (Mounts is SortedList<WorkspacePath, IFileSystem> mounts)
                {
                    // Create a new mount point for the current game
                    var rootPath = WorkspacePath.Root.AppendDirectory("Game");

                    // Make sure we don't have a disk with the same name
                    if (mounts.ContainsKey(rootPath)) mounts.Remove(rootPath);

                    mounts.Add(rootPath, currentDisk);

                    // Filter out only the files we can use and convert this into a dictionary with the file name as the key and the path as the value
                    files = GetGameEntities(rootPath);

                    // Find any lose sprites
                    var spriteDir = rootPath.AppendDirectory("Sprites");

                    if(Exists(spriteDir))
                    {
                        var sprites = (from p in GetEntities(spriteDir) where p.EntityName.EndsWith(".png") select p.Path).ToArray();
                        
                        var total = files.Length;

                        Array.Resize(ref files, total + sprites.Length);

                        Array.Copy(sprites, 0, files, total, sprites.Length);
                        
                    }
                }
            }

            return files;
        }

        public virtual string[] GetGameEntities(WorkspacePath path) => (from p in GetEntities(path)
            where fileExtensions.Any(val => p.EntityName.EndsWith(val))
            select p.Path).ToArray();

        public virtual List<WorkspacePath> SharedLibDirectories()
        {
            // Create paths to the System/Libs and Workspace/Libs folder
            var paths = new List<WorkspacePath>
            {
                // Look in the system folder
                osLibPath
            };

            var workspaceLibsPath = WorkspacePath.Root.AppendDirectory("Workspace").AppendDirectory("System")
                .AppendDirectory("Libs");

            if (Exists(workspaceLibsPath))
                paths.Insert(0, workspaceLibsPath);

            return paths;
        }

        public virtual void ShutdownSystem()
        {
            //            var tmpPath = FileSystemPath.Parse("/Tmp/");

            if (Exists(TmpFileSystemPath))
                foreach (var entities in GetEntities(TmpFileSystemPath))
                    Delete(entities);
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
                    text = StreamExtensions.ReadAllText(file);
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
            if (Exists(filePath)) Delete(filePath);

            // TODO need to look into how to clear the bytes before writing to it?
            var file = CreateFile(filePath);

            if (file != null)
            {
                var bytes = Encoding.ASCII.GetBytes(text);
                StreamExtensions.Write(file, bytes);

                file.Close();

                return true;
            }

            return false;
        }

        #endregion

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
    }
}