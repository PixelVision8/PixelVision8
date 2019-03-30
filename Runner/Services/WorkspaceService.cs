using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using PixelVision8.Engine;
using PixelVision8.Engine.Services;
using PixelVision8.Engine.Utils;
using PixelVision8.Runner.Data;
using PixelVision8.Runner.Utils;
using SharpFileSystem;
using SharpFileSystem.FileSystems;
using SharpFileSystem.IO;
using File = System.IO.File;

namespace PixelVision8.Runner.Services
{

    public class WorkspaceService : AbstractService
    {

        public FileSystemMounter fileSystem;
        protected FileSystemMounter diskMounter = new FileSystemMounter();
        protected FileSystemPath userBiosPath => FileSystemPath.Parse("/Storage/user-bios.json");
        protected List<string> archiveExtensions;
        protected List<string> fileExtensions = new List<string>()
        {
            "png",
            "lua",
            "json",
            "txt"
        };
        
        protected List<string> requiredFiles = new List<string>()
        {
            "data.json",
            "info.json"
        };

        private int _totalDisks = 4;
        
        public int totalDisks
        {
            // TODO need to read this from the bios
            get { return _totalDisks; }
            set { _totalDisks = value; }
        }

        protected DiskDriveService diskDrives;
        
        protected BiosService bios;
        protected DesktopRunner runner;
        
        /// <summary>
        ///     This class manages all of the logic Pixel Vision 8 needs to create and manage the workspace.
        /// </summary>
        public WorkspaceService(BiosService bios, DesktopRunner runner)
        {

            this.bios = bios;
            this.runner = runner;
            
        }

        /// <summary>
        ///     This mounts the file system from a collection of File System Paths and File System instances.
        /// </summary>
        /// <param name="fileSystems"></param>
        public void MountFileSystems(Dictionary<FileSystemPath, IFileSystem> fileSystems)
        {
            
            // Create a new File System
            fileSystem = new FileSystemMounter(fileSystems);
            
            // Create System Mount for the built in OS system folder.
//            var system = new SubFileSystem(fileSystem, FileSystemPath.Root.AppendDirectory("PixelVisionOS").AppendDirectory("System"));
            
            // Create a path to the system folder.
//            var systemPath = FileSystemPath.Root.AppendDirectory("System");

            // Create a new mount point
//            var mount = new KeyValuePair<FileSystemPath, IFileSystem>(systemPath, system);
            
            // Add the system folder to the mount point
//            fileSystem.Mounts.Add(mount);
            
            
            // Create a mount point for disks

            diskMounter = new FileSystemMounter();

            // Add the disk mount point
            fileSystem.Mounts.Add(new KeyValuePair<FileSystemPath, IFileSystem>(FileSystemPath.Root.AppendDirectory("Disks"), diskMounter));
            
            
            // Create a disk drive service to mange the disks
            diskDrives = new DiskDriveService(diskMounter, totalDisks);
            

            EntityMovers.Registration.AddLast(typeof(IFileSystem), typeof(IFileSystem), new StandardEntityMover());
            EntityCopiers.Registration.AddLast(typeof(IFileSystem), typeof(IFileSystem), new StandardEntityCopier());
            
            // Load bios from the user's storage folder
            LoadBios(new[] { userBiosPath });

        }

        public void MountWorkspace(string name)
        {

            var filePath = FileSystemPath.Root.AppendDirectory("User");
    
            // Make sure that the user directory exits
            if (fileSystem.Exists(filePath))
            {
                filePath = filePath.AppendDirectory(name);

                // If the filesystem doesn't exit, we want to create it
                if (!fileSystem.Exists(filePath))
                {
                    fileSystem.CreateDirectory(filePath);
                }
            }
            
            var workspaceDisk = new SubFileSystem(fileSystem, filePath);

            fileSystem.Mounts.Add(new KeyValuePair<FileSystemPath, IFileSystem>(FileSystemPath.Root.AppendDirectory("Workspace"), workspaceDisk));

//            filePath = filePath.AppendDirectory("System");
//
//            if (fileSystem.Exists(filePath))
//            {
//                
//                var osDisk = new SubFileSystem(fileSystem, filePath);
//
//                
//                fileSystem.Mounts.Add(new KeyValuePair<FileSystemPath, IFileSystem>(FileSystemPath.Root.AppendDirectory("PixelVisionOS"), osDisk));
//            }
            
//            var contents = fileSystem.GetEntities(FileSystemPath.Root.AppendDirectory("Workspace"));
//            
//            Console.WriteLine("Workspace Contents " + contents.Count);

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

                return text;//file.ReadAllText();
            }
            
            // Always return an empty string if no file was found
            return "";
        }

        public bool SaveTextToFile(FileSystemPath filePath, string text, bool autoCreate = false)
        {
//            var filePath = FileSystemPath.Parse(path);

            Stream file = null;
            
            if (fileSystem.Exists(filePath))
            {
                fileSystem.Delete(filePath);
            }
//            else if (autoCreate)
//            {
//                
//            }
            
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
        
        
        
        #region Bios APIs

        public void LoadBios(FileSystemPath[] paths)
        {

            for (int i = 0; i < paths.Length; i++)
            {
                var path = paths[i];

                if (fileSystem.Exists(path))
                {
                    var json = ReadTextFromFile(path);

                    ParseBiosText(json);
                }
            }

            ConfigureWorkspaceSettings();
        }

        public void ParseBiosText(string json)
        {
            bios.ParseBiosText(json);

        }

        protected void ConfigureWorkspaceSettings()
        {
            
            archiveExtensions = ((string)ReadBiosData("ArchiveExtensions", "zip,pv8,pvt,pvs,pva")).Split(',').ToList();//new List<string> {"zip", "pv8", "pvt", "pvs", "pva"});
            fileExtensions = ((string)ReadBiosData("FileExtensions", "png,lua,json,txt")).Split(',').ToList();//new List<string> {"png", "lua", "json", "txt"};
//            gameFolders = ((string)ReadBiosData("GameFolders", "Games,Systems,Tools")).Split(',').ToList();//new List<string> {"zip", "pv8", "pvt", "pvs", "pva"});
            
            requiredFiles = ((string)ReadBiosData("RequiredFiles", "data.json,info.json")).Split(',').ToList();//new List<string> {"zip", "pv8", "pvt", "pvs", "pva"});

            
        }

        /// <summary>
        ///     Modifies the bios in memory. Changes are saved to the current bios and are stored in a userBiosChanges var to make
        ///     saving changes easier when shutting down the workspace
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void UpdateBiosData(string key, object value)
        {
            bios.UpdateBiosData(key, value);
        }

        /// <summary>
        ///     This saves any changes made to the bios from the user's session
        /// </summary>
        public void SaveBiosChanges()
        {
            
            // TODO need to update this
//            var path = FileSystemPath.Parse(userBiosPath);
            
            // Look for changes
            if (bios.userBiosChanges != null)
            {
                
                // Get the path to where the user's bios should be saved
//                var path = FileSystemPath.Parse("/User/").AppendFile("user-bios.json");

                if (!fileSystem.Exists(userBiosPath))
                {
                    var newBios = fileSystem.CreateFile(userBiosPath);
                    newBios.Close();
                }
                
                // Create a user data dictionary
                var userData = new Dictionary<string, object>();
                
                // Load the raw data for ther user's bio
                var json = ReadTextFromFile(userBiosPath);
                
                // If the json file isn't empty, deserialize it
                if (json != "")
                {
                    userData = Json.Deserialize(json) as Dictionary<string, object>;
                }
                
                // Loop through each of the items in the uerBiosChanges dictionary
                foreach (var pair in bios.userBiosChanges)
                {
                    // Set the changed values over any existing values from the json
                    if (userData.ContainsKey(pair.Key))
                    {
                        userData[pair.Key] = pair.Value;
                    }
                    else
                    {
                        userData.Add(pair.Key, pair.Value);
                    }
                }

                
                // Save the new bios data back to the user's bios file.
                SaveTextToFile(userBiosPath, Json.Serialize(userData), true);
                
            }
                
        }

        public object ReadBiosData(string key, object defaultValue, bool autoSave = false)
        {
            return bios.ReadBiosData(key, defaultValue, autoSave);
        }
        
        public void WriteBiosData(string key, object value)
        {
            bios.UpdateBiosData(key, value);
        }

        protected IFileSystem currentDisk;
        
        public bool LoadGame(string path, IEngine targetEngine, IRunner runner, bool showPreloader)
        {
            var filePath = FileSystemPath.Parse(path); //FileSystemPath.Root.AppendPath(fullPath);
            var exits = fileSystem.Exists(filePath);

            if (exits)
            {
                try
                {

                    // Found disk to load
                    if (filePath.IsDirectory)
                    {
                        currentDisk = new SubFileSystem(fileSystem, filePath);
                    }
                    else if (filePath.IsFile)
                    {
                        if (archiveExtensions.IndexOf(filePath.Path.Split('.').Last()) > -1)
                        {
                            using (var stream = fileSystem.OpenFile(filePath, FileAccess.ReadWrite))
                            {
                                if (stream is FileStream)
                                {
                                    currentDisk = ZipFileSystem.Open((FileStream)stream);

                                }
                                else
                                {
                                    currentDisk = ZipFileSystem.Open(stream, path);
                                }
                            
                                stream.Close();
                            }
                            
                        }
                    }

                    // We need to get a list of the current mounts
                    var mounts = fileSystem.Mounts as SortedList<FileSystemPath, IFileSystem>;
            
                    // Create a new mount point for the current game
                    var rootPath = FileSystemPath.Root.AppendDirectory("Game");
                
                    // Make sure we don't have a disk with the same name
                    if (mounts.ContainsKey(rootPath))
                    {
                        mounts.Remove(rootPath);
                    }
                    
                    mounts.Add(rootPath, currentDisk);
                    
                    var files = ConvertDiskFilesToBytes(currentDisk);
                    
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
                            {
                                
                                using (var memoryStream = new MemoryStream())
                                {
                                    using(var file = fileSystem.OpenFile(saveFile, FileAccess.Read))
                                    {
                                        file.CopyTo(memoryStream);
                                        file.Close();
                                    }

                                    var fileName = saveFile.EntityName;
                                    var data = memoryStream.ToArray();
                                    
                                    if (files.ContainsKey(fileName))
                                    {
                                        files[fileName] = data;
                                    }
                                    else
                                    {
                                        files.Add(fileName, data);
                                    }
                                    
                                    memoryStream.Close();
                                    
                                }
                                
                                
                            }
                        }
                            
                            
                            
//                        }
                        
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    // Read and Run the disk
                    runner.ProcessFiles(targetEngine, files, showPreloader);
    
                    return true;
                    
                }
                catch
                {
    //		        // TODO need to have a clearer messgae, like not a mount point or can't load from X because of Y
//                    Console.WriteLine("System Error: Could not load from path " + filePath.Path);
                }
            }
            
            return false;
            
        }

        public void IncludeLibDirectoryFiles(Dictionary<string, byte[]> files)
        {
            
            // Create paths to the System/Libs and Workspace/Libs folder
            var paths = new List<FileSystemPath>
            {
                // Look in the system folder
                FileSystemPath.Root.AppendDirectory("PixelVisionOS")
                    .AppendDirectory((string) ReadBiosData("LibsDir", "Libs")),
                        
                // Look in the workspace folder
                FileSystemPath.Root.AppendDirectory("Workspace")
                    .AppendDirectory((string) ReadBiosData("LibsDir", "Libs")),
                        
            };
            
            var diskPaths = diskDrives.disks;

            var totalDisks = diskPaths.Length - 1;

            // Loop backwards through disks
            for (int i = totalDisks; i >= 0; i--)
            {
                var diskPath = FileSystemPath.Root.AppendDirectory("Disks").AppendPath(diskPaths[i].AppendDirectory("Libs"));

                paths.Add(diskPath);
            }

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
                    using(var stream = ZipFileSystem.Open(fileSystem.OpenFile(path, FileAccess.Read) as FileStream))
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
                    };

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

        public void ShutdownSystem()
        {
            // make sure we have the current list of disks in the bios
            UpdateDiskInBios();
            SaveActiveDisks();
            SaveBiosChanges();
            ClearTempDirectory();

        }

        protected void ClearTempDirectory()
        {

            var tmpPath = FileSystemPath.Parse("/Tmp/");

            if (fileSystem.Exists(tmpPath))
            {
                foreach (var entities in fileSystem.GetEntities(tmpPath))
                {
                    fileSystem.Delete(entities);                    
                }
            }

        }

        public string DefaultBootTool()
        {
            // TODO need to see if there is a startup disk loaded
            return (string)ReadBiosData("BootTool", "/PixelVisionOS/Tools/BootTool/");
            
        }
        
//        public string DefaultTool()
//        {
//            // TODO need to see if there is a startup disk loaded
//            return (string)ReadBiosData("DefaultTool", "/");
//        }
        
        #endregion

        public Dictionary<string, object> ReadGameMetaData(FileSystemPath filePath)
        {
           
            Dictionary<string, object> data = new Dictionary<string, object>();

            if (fileSystem.Exists(filePath))
            {
                //            Debug.Log("ReadGameMeta " + path + " ~ "+fileName);

                // Step 1. Load the system snapshot
                var fileContents = ReadTextFromFile(filePath);//ReadTextFromFile(filePath);

                // parse the json data into a dictionary the engine can use
                data = Json.Deserialize(fileContents) as Dictionary<string, object>;
            }

            return data;
        }

        public bool WriteGameMetaData(FileSystemPath filePath, Dictionary<string, object> data)
        {
            var success = false;

            try
            {
                if (fileSystem.Exists(filePath))
                {
                    var text = Json.Serialize(data);
                    
                    SaveTextToFile(filePath, text, true);
                    
                    success = true;
                }
            }
            catch
            {
//                Console.WriteLine("Workspace Meta Data Error:\n"+e.Message);
            }

            return success;
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
        
        public Dictionary<string, string> DiskPaths()
        {

            var pathRefs = new Dictionary<string, string>();

            var mounts = diskDrives.disks;//disks.Mounts as SortedList<FileSystemPath, IFileSystem>;

            for (int i = 0; i < mounts.Length; i++)
            {
                var path = mounts[i];
                pathRefs.Add(path.EntityName, "/Disks" + path.Path);
            }
            
            return pathRefs;
        }
        
        public bool autoRunEnabled = true;

        public void MountDisk(string path, bool updateBios = true)
        {
//            Console.WriteLine("Load File - " + path + " Auto Run " + autoRunEnabled);
            try
            {
                IFileSystem disk;
                
                string entityName;
        
                FileAttributes attr = File.GetAttributes(path);
    
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    entityName = new DirectoryInfo(path).Name;
                }
                else
                {
                    entityName = Path.GetFileNameWithoutExtension(path);
                }
                
                if (path.EndsWith(".pv8") || path.EndsWith(".zip"))
                {
                    disk = ZipFileSystem.Open(path);
                }
                else
                {
                    disk = new PhysicalFileSystem(path);
                }
    
                if (disk == null)
                    return;

                // Test to see if the disk is a valid game
                if (ValidateGameInDir(disk) == false && disk.Exists(FileSystemPath.Root.AppendFile("info.json")) == false)
                {
                     return;
                }
                
                // Update the root path to just be the name of the entity
                var rootPath = FileSystemPath.Root.AppendDirectory(entityName);

                // Check to see if there is already a disk in slot 1, if so we want to eject it since only slot 1 can boot
                if (diskDrives.total > 0 && autoRunEnabled == true)
                {
                    // Clear the load history
                    runner.loadHistory.Clear();
                    
                    // Remove all the other disks
                    diskDrives.EjectAll();
                }
                
                // Add the new disk
                diskDrives.AddDisk(rootPath, disk);
                
                // Only try to auto run a game if this is enabled in the runner
                if (autoRunEnabled)
                {
                    AutoRunGameFromDisk(entityName);
                }
                else
                {
                    // If auto run id disabled, we can just restart the current game to display the loading animation
                    
                    // TODO do we need to force the disk load animation?
//                    runner.ResetGame();
                }

            }
            catch
            {
                autoRunEnabled = true;
                // TODO need to make sure we show a better error to explain why the disk couldn't load
                runner.DisplayError(RunnerGame.ErrorCode.NoAutoRun);
            }

            // Only update the bios when we need  to
            if (updateBios)
            {
                // Update the disk order in the bios
                UpdateDiskInBios();
            }
            
        }

        public void AutoRunGameFromDisk(string diskName)
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
                    var newDiskPath = FileSystemPath.Parse("/Disks/" + diskName+ tmpPath);
                
                    // Change the disk path to the one in the auto-run file
                    if (fileSystem.Exists(newDiskPath))
                    {
                        diskPath = newDiskPath;
                    }
                }
            }
            catch
            {
                // ignored
            }

            // Always validate that the disk is a valid game before trying to load it.
            if (ValidateGameInDir(diskPath))
            {
                // Create new meta data for the game. We wan to display the disk insert animation.
                var metaData = new Dictionary<string, string>()
                {
                    {"showDiskAnimation", "true"} 
                };
                
                // Load the disk path and play the game
                runner.Load(diskPath.Path, RunnerGame.RunnerMode.Playing, metaData);
            }
            else
            {
                // If the new auto run path can't be found, throw an error
                runner.DisplayError(RunnerGame.ErrorCode.NoAutoRun);
            }

            
        }

        public void UpdateDiskInBios()
        {
            var paths = diskDrives.physicalPaths;

            for (int i = 0; i < paths.Length; i++)
            {
                WriteBiosData("Disk" + i, paths[i]);
            }

        }

        public void SaveDisksInMemory()
        {
            Console.WriteLine("Save disks in memory");
            
            var paths = diskDrives.disks;

            for (int i = 0; i < paths.Length; i++)
            {
                diskDrives.SaveDisk(paths[i]);
            }
        }
        
        // Make sure you can only eject a disk by forcing the path to be in the disk mounts scope
        public void EjectDisk(FileSystemPath? filePath = null)
        {

            // Remove the disk if disks exists
            if (diskDrives.total > 0)
            {
                try
                {
                    // Use the path that is supplied or get the first disk path
                    FileSystemPath path = filePath.HasValue ? filePath.Value : diskDrives.disks.First();

                    // Attempt to remove the disk
                    diskDrives.RemoveDisk(path);

                    // Update the bios when a disk is removed
                    UpdateDiskInBios();


                }
                catch
                {
                    // ignored error when removing a disk that doesn't exist
                }
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
            {
    
                // Try to load the first disk
                try
                {
                    // Get the next disk name
                    var diskName = diskDrives.disks.First().Path.Replace("/", "");
    
                    // Clear the history
                    runner.loadHistory.Clear();
    
                    // Attempt to run the fist disk
                    AutoRunGameFromDisk(diskName);
    
                    return;
                }
                catch
                {
                    // ignored
                }
            }
            
            // TODO this is duplicated from the Pixel Vision 8 Runner
            // Look to see if we have the bios default tool in the OS folder
            try
            {
                var biosAutoRun = FileSystemPath.Parse((string) ReadBiosData("AutoRun", ""));
                    
                if (fileSystem.Exists(biosAutoRun))
                {

                    if (ValidateGameInDir(biosAutoRun))
                    {
                        runner.Load(biosAutoRun.Path);
                        return;
                    }
                        
                }
                    
            }
            catch
            {
            }
            
            // If ejecting a disk fails, display the disk error
            runner.DisplayError(RunnerGame.ErrorCode.NoAutoRun);
            
        }

        public string FindValidSavePath(string gamePath)
        {
    
            string savePath = "/";
            
            var filePath = FileSystemPath.Parse(gamePath);

            
            var parentFilePath = filePath.ParentPath;

            var writeAccess = WriteAccess(parentFilePath);

            if (writeAccess)
            {
                savePath = "/Game/";

                if (filePath.IsFile)
                {
                    savePath += filePath.EntityName;
                }
           
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
            {
                
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

                            if (!fileSystem.Exists(parent))
                            {
                                // Create all the missing directories
                                fileSystem.CreateDirectoryRecursive(parent);
                            }
                            
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
            
//             If this is saving to a zip disk, re-write the file to the system
//            try
//            {
//                
//                // Get the first file path
//                var filePath = FileSystemPath.Parse(files.First().Key);
//                
//                // Test to see if this is a disk
//                if (filePath.Path.StartsWith("/Disks/"))
//                {
//                    var pathSegments = filePath.GetDirectorySegments();
//                    
//                    diskDrives.SaveDisk(FileSystemPath.Root.AppendDirectory(pathSegments[0]).AppendDirectory(pathSegments[1]));
//                    
//
////                    var disks = diskDrives.disks;
////                    var diskID = -1;
////                    
////                    for (int i = 0; i < disks.Length; i++)
////                    {
////                        if (filePath.Path.StartsWith(disks[i].Path))
////                        {
////                            diskID = i;
////                            break;
////                        }
////                    }
////
////                    if (diskID != -1)
////                    {
////                        Console.WriteLine("Saved disk " + diskID);
////                    }
//
//                }
//                // 
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//                throw;
//            }
            
        }

//        public bool IsZipDisk(string path)
//        {
//
//
//            return false;
//        }

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
            
            if (!fileSystem.Exists(path))
            {
                return path;
            }

            var fileSplit = SplitFileName(path);
            string name = fileSplit[0];
            
            int ix = 0;
            FileSystemPath filePath;
            
            do {
                ix++;
                filePath = path.ParentPath;
                
                if (path.IsDirectory)
                {
                    filePath = filePath.AppendDirectory(String.Format("{0}{1}", name, ix));
                }
                else
                {
                    
                    filePath = filePath.AppendFile(String.Format("{0}{1}{2}", name, ix, fileSplit[1]));
                }
                
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
                {
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
            }
            catch
            {
//                Console.WriteLine("Workspace Write Error:\n"+e.Message);
//                Console.WriteLine(e);
//                throw;
            }
            

            return canWrite;
            
        }

        public void SaveActiveDisks()
        {
            var disks = diskDrives.disks;

            foreach (var disk in disks)
            {
                diskDrives.SaveDisk(disk);
            }
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
                            catch
                            {
//                                Console.WriteLine("Archive Error: "+ e);
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
                        
//                        Console.WriteLine("Save archive ");
                        
                        File.Delete(fileNameZip+".bak");
                    }
                    catch
                    {
                        if (File.Exists(fileNameZip+".bak"))
                        {
                            File.Move(fileNameZip+".bak", fileNameZip);
                        }
                        
//                        Console.WriteLine("Disk Save Error "+e);
                        
                    }
                }
               
            }
                
                
            var mounts = fileSystem.Mounts as SortedList<FileSystemPath, IFileSystem>;
            
            // Create a new mount point for the current game
            var rootPath = FileSystemPath.Root.AppendDirectory("Game");
                
            // Make sure we don't have a disk with the same name
            if (mounts.ContainsKey(rootPath))
            {
                mounts.Remove(rootPath);
            }

        }

//        public void EjectDisks()
//        {
//            diskDrives.EjectAll();
//            
//            runner.DisplayError(RunnerGame.ErrorCode.NoAutoRun);
//
//        }

        public void AutoRunFirstDisk()
        {

            if (diskDrives.total > 0)
            {
                var firstDisk = diskDrives.disks[0];
                AutoRunGameFromDisk(firstDisk.EntityName);
            }
            
        }

//        protected FileStream logFile;
        
        protected FileSystemPath logFilePath;

        protected LogService logService;
        public string documentsPath;
        public string tmpPath;

        public void SetupLogFile(FileSystemPath filePath)
        {

            if (logService == null)
            {

                var total = Convert.ToInt32((long) ReadBiosData("TotalLogItems", 100L, true)).Clamp(1, 500);
                
                logService = new LogService(total);
                
            }

            logFilePath = filePath;

            UpdateLog("Debug Log Created " + DateTime.Now.ToString("yyyyMMddHHmmssfff"));
        }

        
        public virtual void UpdateLog(string logString, LogType type = LogType.Log, string stackTrace = "")
        {

            if (logService == null)
            {
                return;
            }
            
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
    }
}
