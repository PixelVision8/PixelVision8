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
    public class FileSystemMounter : IFileSystem
    {
        public FileSystemMounter(IEnumerable<KeyValuePair<WorkspacePath, IFileSystem>> mounts)
        {
            Mounts = new SortedList<WorkspacePath, IFileSystem>(
                new InverseComparer<WorkspacePath>(Comparer<WorkspacePath>.Default));
            foreach (var mount in mounts) Mounts.Add(mount);
        }

        public FileSystemMounter(params KeyValuePair<WorkspacePath, IFileSystem>[] mounts)
            : this((IEnumerable<KeyValuePair<WorkspacePath, IFileSystem>>) mounts)
        {
        }

        public ICollection<KeyValuePair<WorkspacePath, IFileSystem>> Mounts { get; }

        public void Dispose()
        {
            foreach (var mount in Mounts.Select(p => p.Value)) mount.Dispose();
        }

        public ICollection<WorkspacePath> GetEntities(WorkspacePath path)
        {
            var pair = Get(path);
            var entities = pair.Value.GetEntities(path.IsRoot ? path : path.RemoveParent(pair.Key));
            return new EnumerableCollection<WorkspacePath>(entities.Select(p => pair.Key.AppendPath(p)),
                entities.Count);
        }

        public virtual bool Exists(WorkspacePath path)
        {
            try
            {
                var pair = Get(path);
                return pair.Value.Exists(path.RemoveParent(pair.Key));
            }
            catch
            {
                return false;
            }
        }

        public Stream CreateFile(WorkspacePath path)
        {
            var pair = Get(path);
            return pair.Value.CreateFile(path.RemoveParent(pair.Key));
        }

        public Stream OpenFile(WorkspacePath path, FileAccess access)
        {
            var pair = Get(path);
            return pair.Value.OpenFile(path.RemoveParent(pair.Key), access);
        }

        public void CreateDirectory(WorkspacePath path)
        {
            var pair = Get(path);
            pair.Value.CreateDirectory(path.RemoveParent(pair.Key));
        }

        public void Delete(WorkspacePath path)
        {
            var pair = Get(path);
            pair.Value.Delete(path.RemoveParent(pair.Key));
        }

        public KeyValuePair<WorkspacePath, IFileSystem> Get(WorkspacePath path)
        {
            return Mounts.First(pair => pair.Key == path || pair.Key.IsParentOf(path));
        }
    }
}