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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using PixelVision8.Runner.Workspace;

namespace PixelVision8.Runner.Workspace
{
    public class SubFileSystem : IFileSystem
    {
        public SubFileSystem(IFileSystem fileSystem, FileSystemPath root)
        {
            FileSystem = fileSystem;
            Root = root;
        }

        public IFileSystem FileSystem { get; }
        public FileSystemPath Root { get; }

        public void Dispose()
        {
            FileSystem.Dispose();
        }

        public ICollection<FileSystemPath> GetEntities(FileSystemPath path)
        {
            var paths = FileSystem.GetEntities(AppendRoot(path));
            return new EnumerableCollection<FileSystemPath>(paths.Select(p => RemoveRoot(p)), paths.Count);
        }

        public bool Exists(FileSystemPath path)
        {
            return FileSystem.Exists(AppendRoot(path));
        }

        public Stream CreateFile(FileSystemPath path)
        {
            return FileSystem.CreateFile(AppendRoot(path));
        }

        public Stream OpenFile(FileSystemPath path, FileAccess access)
        {
            return FileSystem.OpenFile(AppendRoot(path), access);
        }

        public void CreateDirectory(FileSystemPath path)
        {
            FileSystem.CreateDirectory(AppendRoot(path));
        }

        public void Delete(FileSystemPath path)
        {
            FileSystem.Delete(AppendRoot(path));
        }

        protected FileSystemPath AppendRoot(FileSystemPath path)
        {
            return Root.AppendPath(path);
        }

        protected FileSystemPath RemoveRoot(FileSystemPath path)
        {
            return path.RemoveParent(Root);
        }
    }
}