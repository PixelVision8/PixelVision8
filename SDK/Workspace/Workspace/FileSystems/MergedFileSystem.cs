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
    public class MergedFileSystem : IFileSystem
    {
        public MergedFileSystem(IEnumerable<IFileSystem> fileSystems)
        {
            FileSystems = fileSystems.ToArray();
        }

        public MergedFileSystem(params IFileSystem[] fileSystems)
        {
            FileSystems = fileSystems.ToArray();
        }

        public IEnumerable<IFileSystem> FileSystems { get; set; }

        public void Dispose()
        {
            foreach (var fs in FileSystems) fs.Dispose();
        }

        public ICollection<WorkspacePath> GetEntities(WorkspacePath path)
        {
            var entities = new SortedList<WorkspacePath, WorkspacePath>();
            foreach (var fs in FileSystems.Where(fs => fs.Exists(path)))
            foreach (var entity in fs.GetEntities(path))
                if (!entities.ContainsKey(entity))
                    entities.Add(entity, entity);

            return entities.Values;
        }

        public bool Exists(WorkspacePath path)
        {
            return FileSystems.Any(fs => fs.Exists(path));
        }

        public Stream CreateFile(WorkspacePath path)
        {
            var fs = GetFirst(path) ?? FileSystems.First();
            return fs.CreateFile(path);
        }

        public Stream OpenFile(WorkspacePath path, FileAccess access)
        {
            var fs = GetFirst(path);
            if (fs == null) throw new FileNotFoundException();

            return fs.OpenFile(path, access);
        }

        public void CreateDirectory(WorkspacePath path)
        {
            if (Exists(path)) throw new ArgumentException("The specified directory already exists.");

            var fs = GetFirst(path.ParentPath);
            if (fs == null) throw new ArgumentException("The directory-parent does not exist.");

            fs.CreateDirectory(path);
        }

        public void Delete(WorkspacePath path)
        {
            foreach (var fs in FileSystems.Where(fs => fs.Exists(path))) fs.Delete(path);
        }

        public IFileSystem GetFirst(WorkspacePath path)
        {
            return FileSystems.FirstOrDefault(fs => fs.Exists(path));
        }
    }
}