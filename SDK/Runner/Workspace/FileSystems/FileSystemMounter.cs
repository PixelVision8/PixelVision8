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

namespace PixelVision8.Runner.Workspace
{
    public class FileSystemMounter : IFileSystem
    {
        public FileSystemMounter(IEnumerable<KeyValuePair<FileSystemPath, IFileSystem>> mounts)
        {
            Mounts = new SortedList<FileSystemPath, IFileSystem>(
                new InverseComparer<FileSystemPath>(Comparer<FileSystemPath>.Default));
            foreach (var mount in mounts)
                Mounts.Add(mount);
        }

        public FileSystemMounter(params KeyValuePair<FileSystemPath, IFileSystem>[] mounts)
            : this((IEnumerable<KeyValuePair<FileSystemPath, IFileSystem>>) mounts)
        {
        }

        public ICollection<KeyValuePair<FileSystemPath, IFileSystem>> Mounts { get; }

        public void Dispose()
        {
            foreach (var mount in Mounts.Select(p => p.Value))
                mount.Dispose();
        }

        public ICollection<FileSystemPath> GetEntities(FileSystemPath path)
        {
            var pair = Get(path);
            var entities = pair.Value.GetEntities(path.IsRoot ? path : path.RemoveParent(pair.Key));
            return new EnumerableCollection<FileSystemPath>(entities.Select(p => pair.Key.AppendPath(p)),
                entities.Count);
        }

        public bool Exists(FileSystemPath path)
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

        public Stream CreateFile(FileSystemPath path)
        {
            var pair = Get(path);
            return pair.Value.CreateFile(path.RemoveParent(pair.Key));
        }

        public Stream OpenFile(FileSystemPath path, FileAccess access)
        {
            var pair = Get(path);
            return pair.Value.OpenFile(path.RemoveParent(pair.Key), access);
        }

        public void CreateDirectory(FileSystemPath path)
        {
            var pair = Get(path);
            pair.Value.CreateDirectory(path.RemoveParent(pair.Key));
        }

        public void Delete(FileSystemPath path)
        {
            var pair = Get(path);
            pair.Value.Delete(path.RemoveParent(pair.Key));
        }

        protected KeyValuePair<FileSystemPath, IFileSystem> Get(FileSystemPath path)
        {
            return Mounts.First(pair => pair.Key == path || pair.Key.IsParentOf(path));
        }
    }
}