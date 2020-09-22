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

namespace PixelVision8.Runner.Workspace
{
    public class MemoryFileSystem : IFileSystem
    {
        private readonly IDictionary<WorkspacePath, ISet<WorkspacePath>> _directories =
            new Dictionary<WorkspacePath, ISet<WorkspacePath>>();

        private readonly IDictionary<WorkspacePath, MemoryFile> _files =
            new Dictionary<WorkspacePath, MemoryFile>();

        public MemoryFileSystem()
        {
            _directories.Add(WorkspacePath.Root, new HashSet<WorkspacePath>());
        }

        public ICollection<WorkspacePath> GetEntities(WorkspacePath path)
        {
            if (!path.IsDirectory) throw new ArgumentException("The specified path is no directory.", "path");

            if (!_directories.TryGetValue(path, out var subentities)) throw new DirectoryNotFoundException();

            return subentities;
        }

        public bool Exists(WorkspacePath path)
        {
            return path.IsDirectory ? _directories.ContainsKey(path) : _files.ContainsKey(path);
        }

        public Stream CreateFile(WorkspacePath path)
        {
            if (!path.IsFile) throw new ArgumentException("The specified path is no file.", "path");

            if (!_directories.ContainsKey(path.ParentPath)) throw new DirectoryNotFoundException();

            _directories[path.ParentPath].Add(path);
            return new MemoryFileStream(_files[path] = new MemoryFile());
        }

        public Stream OpenFile(WorkspacePath path, FileAccess access)
        {
            if (!path.IsFile) throw new ArgumentException("The specified path is no file.", "path");

            if (!_files.TryGetValue(path, out var file)) throw new FileNotFoundException();

            return new MemoryFileStream(file);
        }

        public void CreateDirectory(WorkspacePath path)
        {
            if (!path.IsDirectory) throw new ArgumentException("The specified path is no directory.", "path");

            if (_directories.ContainsKey(path))
                throw new ArgumentException("The specified directory-path already exists.", "path");

            if (!_directories.TryGetValue(path.ParentPath, out var subentities)) throw new DirectoryNotFoundException();

            subentities.Add(path);
            _directories[path] = new HashSet<WorkspacePath>();
        }

        public void Delete(WorkspacePath path)
        {
            if (path.IsRoot) throw new ArgumentException("The root cannot be deleted.");

            bool removed;
            if (path.IsDirectory)
                removed = _directories.Remove(path);
            else
                removed = _files.Remove(path);

            if (!removed) throw new ArgumentException("The specified path does not exist.");

            var parent = _directories[path.ParentPath];
            parent.Remove(path);
        }

        public void Dispose()
        {
        }

        public class MemoryFile
        {
            public MemoryFile()
                : this(new byte[0])
            {
            }

            public MemoryFile(byte[] content)
            {
                Content = content;
            }

            public byte[] Content { get; set; }
        }

        public class MemoryFileStream : Stream
        {
            private readonly MemoryFile _file;

            public MemoryFileStream(MemoryFile file)
            {
                _file = file;
            }

            public byte[] Content
            {
                get => _file.Content;
                set => _file.Content = value;
            }

            public override bool CanRead => true;

            public override bool CanSeek => true;

            public override bool CanWrite => true;

            public override long Length => _file.Content.Length;

            public override long Position { get; set; }

            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                if (origin == SeekOrigin.Begin) return Position = offset;

                if (origin == SeekOrigin.Current) return Position += offset;

                return Position = Length - offset;
            }

            public override void SetLength(long value)
            {
                var newLength = (int) value;
                var newContent = new byte[newLength];
                Buffer.BlockCopy(Content, 0, newContent, 0, Math.Min(newLength, (int) Length));
                Content = newContent;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                var mincount = Math.Min(count, Math.Abs((int) (Length - Position)));
                Buffer.BlockCopy(Content, (int) Position, buffer, offset, mincount);
                Position += mincount;
                return mincount;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (Length - Position < count) SetLength(Position + count);

                Buffer.BlockCopy(buffer, offset, Content, (int) Position, count);
                Position += count;
            }
        }
    }
}