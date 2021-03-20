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
    public static class FileSystemExtensions
    {
        //        public static Stream Open(this File file, FileAccess access)
        //        {
        //            return file.FileSystem.OpenFile(file.Path, access);
        //        }
        //
        //        public static void Delete(this FileSystemEntity entity)
        //        {
        //            entity.FileSystem.Delete(entity.Path);
        //        }

        //        public static ICollection<FileSystemPath> GetEntityPaths(this Directory directory)
        //        {
        //            return directory.FileSystem.GetEntities(directory.Path);
        //        }

        //        public static ICollection<FileSystemEntity> GetEntities(this Directory directory)
        //        {
        //            var paths = directory.GetEntityPaths();
        //            return new EnumerableCollection<FileSystemEntity>(
        //                paths.Select(p => FileSystemEntity.Create(directory.FileSystem, p)), paths.Count);
        //        }

        public static IEnumerable<WorkspacePath> GetEntitiesRecursive(this IFileSystem fileSystem, WorkspacePath path)
        {
            if (!path.IsDirectory) throw new ArgumentException("The specified path is not a directory.");

            foreach (var entity in fileSystem.GetEntities(path))
            {
                yield return entity;
                if (entity.IsDirectory)
                    foreach (var subentity in fileSystem.GetEntitiesRecursive(entity))
                        yield return subentity;
            }
        }

        public static void CreateDirectoryRecursive(this IFileSystem fileSystem, WorkspacePath path)
        {
            if (!path.IsDirectory) throw new ArgumentException("The specified path is not a directory.");

            var currentDirectoryPath = WorkspacePath.Root;
            foreach (var dirName in path.GetDirectorySegments())
            {
                currentDirectoryPath = currentDirectoryPath.AppendDirectory(dirName);
                if (!fileSystem.Exists(currentDirectoryPath)) fileSystem.CreateDirectory(currentDirectoryPath);
            }
        }

        #region Copy Extensions

        public static void Copy(this IFileSystem sourceFileSystem, WorkspacePath sourcePath,
            IFileSystem destinationFileSystem, WorkspacePath destinationPath)
        {
            bool isFile;
            if ((isFile = sourcePath.IsFile) != destinationPath.IsFile)
                throw new ArgumentException(
                    "The specified destination-path is of a different type than the source-path.");

            if (isFile)
            {
                using (var sourceStream = sourceFileSystem.OpenFile(sourcePath, FileAccess.Read))
                {
                    using (var destinationStream = destinationFileSystem.CreateFile(destinationPath))
                    {
                        var buffer = new byte[BufferSize];
                        int readBytes;
                        while ((readBytes = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                            destinationStream.Write(buffer, 0, readBytes);
                    }
                }
            }
            else
            {
                if (!destinationPath.IsRoot) destinationFileSystem.CreateDirectory(destinationPath);

                foreach (var ep in sourceFileSystem.GetEntities(sourcePath))
                {
                    var destinationEntityPath = ep.IsFile
                        ? destinationPath.AppendFile(ep.EntityName)
                        : destinationPath.AppendDirectory(ep.EntityName);
                    Copy(sourceFileSystem, ep, destinationFileSystem, destinationEntityPath);
                }
            }

            //            IEntityCopier copier;
            //            if (!EntityCopiers.Registration.TryGetSupported(sourceFileSystem.GetType(), destinationFileSystem.GetType(),
            //                out copier))
            //                throw new ArgumentException("The specified combination of file-systems is not supported.");
            //            copier.Copy(sourceFileSystem, sourcePath, destinationFileSystem, destinationPath);
        }

        //        public static void CopyTo(this FileSystemEntity entity, IFileSystem destinationFileSystem,
        //            FileSystemPath destinationPath)
        //        {
        //            entity.FileSystem.Copy(entity.Path, destinationFileSystem, destinationPath);
        //        }
        //
        //        public static void CopyTo(this Directory source, Directory destination)
        //        {
        //            source.FileSystem.Copy(source.Path, destination.FileSystem,
        //                destination.Path.AppendDirectory(source.Path.EntityName));
        //        }
        //
        //        public static void CopyTo(this File source, Directory destination)
        //        {
        //            source.FileSystem.Copy(source.Path, destination.FileSystem,
        //                destination.Path.AppendFile(source.Path.EntityName));
        //        }

        #endregion

        #region Move Extensions

        private static readonly int BufferSize = 65536;

        public static void Move(this IFileSystem sourceFileSystem, WorkspacePath sourcePath,
            IFileSystem destinationFileSystem, WorkspacePath destinationPath)
        {
            bool isFile;
            if ((isFile = sourcePath.IsFile) != destinationPath.IsFile)
                throw new ArgumentException(
                    "The specified destination-path is of a different type than the source-path.");

            if (isFile)
            {
                using (var sourceStream = sourceFileSystem.OpenFile(sourcePath, FileAccess.Read))
                {
                    using (var destinationStream = destinationFileSystem.CreateFile(destinationPath))
                    {
                        var buffer = new byte[BufferSize];
                        int readBytes;
                        while ((readBytes = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                            destinationStream.Write(buffer, 0, readBytes);
                    }
                }

                sourceFileSystem.Delete(sourcePath);
            }
            else
            {
                destinationFileSystem.CreateDirectory(destinationPath);
                foreach (var ep in sourceFileSystem.GetEntities(sourcePath).ToArray())
                {
                    var destinationEntityPath = ep.IsFile
                        ? destinationPath.AppendFile(ep.EntityName)
                        : destinationPath.AppendDirectory(ep.EntityName);
                    Move(sourceFileSystem, ep, destinationFileSystem, destinationEntityPath);
                }

                if (!sourcePath.IsRoot) sourceFileSystem.Delete(sourcePath);
            }

            //            IEntityMover mover;
            //            if (!EntityMovers.Registration.TryGetSupported(sourceFileSystem.GetType(), destinationFileSystem.GetType(),
            //                out mover))
            //                throw new ArgumentException("The specified combination of file-systems is not supported.");
            //            mover.Move(sourceFileSystem, sourcePath, destinationFileSystem, destinationPath);
        }

        //        public static void MoveTo(this FileSystemEntity entity, IFileSystem destinationFileSystem,
        //            FileSystemPath destinationPath)
        //        {
        //            entity.FileSystem.Move(entity.Path, destinationFileSystem, destinationPath);
        //        }

        //        public static void MoveTo(this Directory source, Directory destination)
        //        {
        //            source.FileSystem.Move(source.Path, destination.FileSystem,
        //                destination.Path.AppendDirectory(source.Path.EntityName));
        //        }

        //        public static void MoveTo(this File source, Directory destination)
        //        {
        //            source.FileSystem.Move(source.Path, destination.FileSystem,
        //                destination.Path.AppendFile(source.Path.EntityName));
        //        }

        #endregion
    }
}