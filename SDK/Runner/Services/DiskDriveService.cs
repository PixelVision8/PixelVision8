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

using System.Collections.Generic;
using System.Linq;
using PixelVision8.Runner.Data;
using SharpFileSystem;
using SharpFileSystem.FileSystems;

namespace PixelVision8.Runner.Services
{
    public class DiskDriveService
    {
        private readonly SortedList<FileSystemPath, IFileSystem> diskMount;
        private readonly List<FileSystemPath> diskNames = new List<FileSystemPath>();

        public int totalDisks = 5;

        public DiskDriveService(FileSystemMounter diskMount, int totalDisks)
        {
            this.diskMount = diskMount.Mounts as SortedList<FileSystemPath, IFileSystem>;

            this.totalDisks = totalDisks;
        }

        public int total => diskNames.Count;

        public FileSystemPath[] disks => diskNames.ToArray();

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

        public void AddDisk(FileSystemPath path, IFileSystem disk)
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