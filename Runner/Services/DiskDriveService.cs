using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using PixelVision8.Runner.Data;
using SharpFileSystem;
using SharpFileSystem.FileSystems;

namespace Desktop.Services
{
    public class DiskDriveService
    {

        public int totalDisks = 5;

        private SortedList<FileSystemPath, IFileSystem> diskMount;
        private List<FileSystemPath> diskNames = new List<FileSystemPath>();

        public int total
        {
            get { return diskNames.Count; }
        }
        
        public DiskDriveService(FileSystemMounter diskMount, int totalDisks)
        {
            this.diskMount = diskMount.Mounts as SortedList<FileSystemPath, IFileSystem>;
            
            this.totalDisks = totalDisks;
        }

        public void AddDisk(FileSystemPath path, IFileSystem disk)
        {
            // If for some reason we don't have a mount point we need to exit out of this method
            if (diskMount == null)
                return;
                
            // If we are out of open disks, remove the last one
            if (total == totalDisks )
            {
                RemoveDisk(diskNames.Last());
            }

            // Attempt to remove the disk if it is already inserted
            RemoveDisk(path);
                
            // Add the new disk to the disk mount
            diskMount.Add(path, disk);
            
            // Add the disk path to the list of names
            diskNames.Add(path);
        }

        public void RemoveDisk(FileSystemPath path)
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

        public void SaveDisk(FileSystemPath path)
        {
            
//            Console.WriteLine("Attempting to save disk " + path);
            
            if (diskMount.ContainsKey(path))
            {

//                Console.WriteLine("Found disk " + path);
                
                var mount = diskMount[path];
                if (mount is ZipFileSystem)
                {
                    ((ZipFileSystem) mount).Save();
                }

            }
            
        }

        public FileSystemPath[] disks => diskNames.ToArray();

        public void EjectAll()
        {
//            var names = disks;
            
            foreach (var path in disks)
            {
                RemoveDisk(path);
            }
        }

        public string[] physicalPaths
        {
            get
            {
                var paths = new string[totalDisks];

                for (int i = 0; i < totalDisks; i++)
                {
                    var tmpPath = "none";

                    if (i < total)
                    {
                        var key = disks[i];
    
                        if (diskMount.ContainsKey(key))
                        {
                            var disk = diskMount[key];

                            if (disk is PhysicalFileSystem src )
                            {
                                tmpPath = src.PhysicalRoot;
                            }else if (disk is ZipFileSystem zipDisk)
                            {
                                tmpPath = zipDisk.srcPath;
                            }
                        }
                    }

                    paths[i] = tmpPath;
                }

                return paths;
            }
        }
    }
}