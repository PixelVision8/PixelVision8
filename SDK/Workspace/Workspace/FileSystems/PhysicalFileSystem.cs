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

namespace PixelVision8.Workspace
{
    public class PhysicalFileSystem : IFileSystem
    {
        public ICollection<WorkspacePath> GetEntities(WorkspacePath path)
        {
            var physicalPath = GetPhysicalPath(path);
            var directories = Directory.GetDirectories(physicalPath);
            var files = Directory.GetFiles(physicalPath);
            var virtualDirectories =
                directories.Select(p => GetVirtualDirectoryPath(p));
            var virtualFiles =
                files.Select(p => GetVirtualFilePath(p));
            return new EnumerableCollection<WorkspacePath>(virtualDirectories.Concat(virtualFiles),
                directories.Length + files.Length);
        }

        public bool Exists(WorkspacePath path)
        {
            return path.IsFile
                ? File.Exists(GetPhysicalPath(path))
                : Directory.Exists(GetPhysicalPath(path));
        }

        public Stream CreateFile(WorkspacePath path)
        {
            if (!path.IsFile) throw new ArgumentException("The specified path is not a file.", "path");

            return File.Create(GetPhysicalPath(path));
        }

        public Stream OpenFile(WorkspacePath path, FileAccess access)
        {
            if (!path.IsFile) throw new ArgumentException("The specified path is not a file.", "path");

            return File.Open(GetPhysicalPath(path), FileMode.Open, access);
        }

        public void CreateDirectory(WorkspacePath path)
        {
            if (!path.IsDirectory) throw new ArgumentException("The specified path is not a directory.", "path");

            Directory.CreateDirectory(GetPhysicalPath(path));
        }

        public void Delete(WorkspacePath path)
        {
            if (path.IsFile)
                File.Delete(GetPhysicalPath(path));
            else
                Directory.Delete(GetPhysicalPath(path), true);
        }

        public void Dispose()
        {
        }

        #region Internals

        public string PhysicalRoot { get; }

        public PhysicalFileSystem(string physicalRoot)
        {
            if (!Path.IsPathRooted(physicalRoot)) physicalRoot = Path.GetFullPath(physicalRoot);

            if (physicalRoot[physicalRoot.Length - 1] != Path.DirectorySeparatorChar)
                physicalRoot = physicalRoot + Path.DirectorySeparatorChar;

            PhysicalRoot = physicalRoot;
        }

        public string GetPhysicalPath(WorkspacePath path)
        {
            return Path.Combine(PhysicalRoot,
                path.ToString().Remove(0, 1).Replace(WorkspacePath.DirectorySeparator, Path.DirectorySeparatorChar));
        }

        public WorkspacePath GetVirtualFilePath(string physicalPath)
        {
            if (!physicalPath.StartsWith(PhysicalRoot, StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException("The specified path is not member of the PhysicalRoot.", "physicalPath");

            var virtualPath = WorkspacePath.DirectorySeparator + physicalPath.Remove(0, PhysicalRoot.Length)
                .Replace(Path.DirectorySeparatorChar, WorkspacePath.DirectorySeparator);
            return WorkspacePath.Parse(virtualPath);
        }

        public WorkspacePath GetVirtualDirectoryPath(string physicalPath)
        {
            if (!physicalPath.StartsWith(PhysicalRoot, StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException("The specified path is not member of the PhysicalRoot.", "physicalPath");

            var virtualPath = WorkspacePath.DirectorySeparator + physicalPath.Remove(0, PhysicalRoot.Length)
                .Replace(Path.DirectorySeparatorChar, WorkspacePath.DirectorySeparator);
            if (virtualPath[virtualPath.Length - 1] != WorkspacePath.DirectorySeparator)
                virtualPath += WorkspacePath.DirectorySeparator;

            return WorkspacePath.Parse(virtualPath);
        }

        #endregion
    }
}