using PixelVision8.Workspace;
using System;
using System.Collections.Generic;
using System.IO;

namespace SharpFileSystem.FileSystems
{
    public class ReadOnlyFileSystem : IFileSystem
    {
        public IFileSystem FileSystem { get; private set; }

        public ReadOnlyFileSystem(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        public void Dispose()
        {
            FileSystem.Dispose();
        }

        public ICollection<WorkspacePath> GetEntities(WorkspacePath path)
        {
            return FileSystem.GetEntities(path);
        }

        public bool Exists(WorkspacePath path)
        {
            return FileSystem.Exists(path);
        }

        public Stream OpenFile(WorkspacePath path, FileAccess access)
        {
            if (access != FileAccess.Read)
                throw new InvalidOperationException("This is a read-only filesystem.");
            return FileSystem.OpenFile(path, access);
        }

        public Stream CreateFile(WorkspacePath path)
        {
            throw new InvalidOperationException("This is a read-only filesystem.");
        }

        public void CreateDirectory(WorkspacePath path)
        {
            throw new InvalidOperationException("This is a read-only filesystem.");
        }

        public void Delete(WorkspacePath path)
        {
            throw new InvalidOperationException("This is a read-only filesystem.");
        }
    }
}