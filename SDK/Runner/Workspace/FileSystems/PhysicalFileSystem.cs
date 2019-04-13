//   
// Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by Pixel Vision 8 are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
//
// Based on SharpFileSystem by Softwarebakery Copyright (c) 2013 under
// MIT license at https://github.com/bobvanderlinden/sharpfilesystem.
// Modified for PixelVision8 by Jesse Freeman
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
using PixelVision8.Runner.Workspace.Collections;

namespace PixelVision8.Runner.Workspace.FileSystems
{
    public class PhysicalFileSystem : IFileSystem
    {
        public ICollection<FileSystemPath> GetEntities(FileSystemPath path)
        {
            var physicalPath = GetPhysicalPath(path);
            var directories = System.IO.Directory.GetDirectories(physicalPath);
            var files = System.IO.Directory.GetFiles(physicalPath);
            var virtualDirectories =
                directories.Select(p => GetVirtualDirectoryPath(p));
            var virtualFiles =
                files.Select(p => GetVirtualFilePath(p));
            return new EnumerableCollection<FileSystemPath>(virtualDirectories.Concat(virtualFiles),
                directories.Length + files.Length);
        }

        public bool Exists(FileSystemPath path)
        {
            return path.IsFile
                ? System.IO.File.Exists(GetPhysicalPath(path))
                : System.IO.Directory.Exists(GetPhysicalPath(path));
        }

        public Stream CreateFile(FileSystemPath path)
        {
            if (!path.IsFile)
                throw new ArgumentException("The specified path is not a file.", "path");
            return System.IO.File.Create(GetPhysicalPath(path));
        }

        public Stream OpenFile(FileSystemPath path, FileAccess access)
        {
            if (!path.IsFile)
                throw new ArgumentException("The specified path is not a file.", "path");
            return System.IO.File.Open(GetPhysicalPath(path), FileMode.Open, access);
        }

        public void CreateDirectory(FileSystemPath path)
        {
            if (!path.IsDirectory)
                throw new ArgumentException("The specified path is not a directory.", "path");
            System.IO.Directory.CreateDirectory(GetPhysicalPath(path));
        }

        public void Delete(FileSystemPath path)
        {
            if (path.IsFile)
                System.IO.File.Delete(GetPhysicalPath(path));
            else
                System.IO.Directory.Delete(GetPhysicalPath(path), true);
        }

        public void Dispose()
        {
        }

        #region Internals

        public string PhysicalRoot { get; }

        public PhysicalFileSystem(string physicalRoot)
        {
            if (!Path.IsPathRooted(physicalRoot))
                physicalRoot = Path.GetFullPath(physicalRoot);
            if (physicalRoot[physicalRoot.Length - 1] != Path.DirectorySeparatorChar)
                physicalRoot = physicalRoot + Path.DirectorySeparatorChar;
            PhysicalRoot = physicalRoot;
        }

        public string GetPhysicalPath(FileSystemPath path)
        {
            return Path.Combine(PhysicalRoot,
                path.ToString().Remove(0, 1).Replace(FileSystemPath.DirectorySeparator, Path.DirectorySeparatorChar));
        }

        public FileSystemPath GetVirtualFilePath(string physicalPath)
        {
            if (!physicalPath.StartsWith(PhysicalRoot, StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException("The specified path is not member of the PhysicalRoot.", "physicalPath");
            var virtualPath = FileSystemPath.DirectorySeparator + physicalPath.Remove(0, PhysicalRoot.Length)
                                  .Replace(Path.DirectorySeparatorChar, FileSystemPath.DirectorySeparator);
            return FileSystemPath.Parse(virtualPath);
        }

        public FileSystemPath GetVirtualDirectoryPath(string physicalPath)
        {
            if (!physicalPath.StartsWith(PhysicalRoot, StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException("The specified path is not member of the PhysicalRoot.", "physicalPath");
            var virtualPath = FileSystemPath.DirectorySeparator + physicalPath.Remove(0, PhysicalRoot.Length)
                                  .Replace(Path.DirectorySeparatorChar, FileSystemPath.DirectorySeparator);
            if (virtualPath[virtualPath.Length - 1] != FileSystemPath.DirectorySeparator)
                virtualPath += FileSystemPath.DirectorySeparator;
            return FileSystemPath.Parse(virtualPath);
        }

        #endregion
    }
}