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

namespace PixelVision8.Runner.Workspace
{
    public class FileSystemEntity : IEquatable<FileSystemEntity>
    {
        public FileSystemEntity(IFileSystem fileSystem, FileSystemPath path)
        {
            FileSystem = fileSystem;
            Path = path;
        }

        public IFileSystem FileSystem { get; }
        public FileSystemPath Path { get; }
        public string Name => Path.EntityName;

        bool IEquatable<FileSystemEntity>.Equals(FileSystemEntity other)
        {
            return FileSystem.Equals(other.FileSystem) && Path.Equals(other.Path);
        }

        public override bool Equals(object obj)
        {
            var other = obj as FileSystemEntity;
            return other != null && ((IEquatable<FileSystemEntity>) this).Equals(other);
        }

        public override int GetHashCode()
        {
            return FileSystem.GetHashCode() ^ Path.GetHashCode();
        }

        public static FileSystemEntity Create(IFileSystem fileSystem, FileSystemPath path)
        {
            if (path.IsFile)
                return new File(fileSystem, path);
            return new Directory(fileSystem, path);
        }
    }
}