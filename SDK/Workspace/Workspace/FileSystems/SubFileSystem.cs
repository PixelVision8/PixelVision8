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

namespace PixelVision8.Workspace
{
    public class SubFileSystem : IFileSystem
    {
        public SubFileSystem(IFileSystem fileSystem, WorkspacePath root)
        {
            FileSystem = fileSystem;
            Root = root;
        }

        public IFileSystem FileSystem { get; }
        public WorkspacePath Root { get; }

        public void Dispose()
        {
            FileSystem.Dispose();
        }

        public ICollection<WorkspacePath> GetEntities(WorkspacePath path)
        {
            var paths = FileSystem.GetEntities(AppendRoot(path));
            return new EnumerableCollection<WorkspacePath>(paths.Select(p => RemoveRoot(p)), paths.Count);
        }

        public bool Exists(WorkspacePath path)
        {
            return FileSystem.Exists(AppendRoot(path));
        }

        public Stream CreateFile(WorkspacePath path)
        {
            return FileSystem.CreateFile(AppendRoot(path));
        }

        public Stream OpenFile(WorkspacePath path, FileAccess access)
        {
            return FileSystem.OpenFile(AppendRoot(path), access);
        }

        public void CreateDirectory(WorkspacePath path)
        {
            FileSystem.CreateDirectory(AppendRoot(path));
        }

        public void Delete(WorkspacePath path)
        {
            FileSystem.Delete(AppendRoot(path));
        }

        protected WorkspacePath AppendRoot(WorkspacePath path)
        {
            return Root.AppendPath(path);
        }

        protected WorkspacePath RemoveRoot(WorkspacePath path)
        {
            return path.RemoveParent(Root);
        }
    }
}