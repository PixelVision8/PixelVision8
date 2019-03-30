using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpFileSystem.FileSystems
{
    public class ReadOnlyFileSystem: IFileSystem
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

        public ICollection<FileSystemPath> GetEntities(FileSystemPath path)
        {
            return FileSystem.GetEntities(path);
        }

        public bool Exists(FileSystemPath path)
        {
            return FileSystem.Exists(path);
        }

        public Stream OpenFile(FileSystemPath path, FileAccess access)
        {
            if (access != FileAccess.Read)
                throw new InvalidOperationException("This is a read-only filesystem.");
            return FileSystem.OpenFile(path, access);
        }

        public Stream CreateFile(FileSystemPath path)
        {
            throw new InvalidOperationException("This is a read-only filesystem.");
        }

        public void CreateDirectory(FileSystemPath path)
        {
            throw new InvalidOperationException("This is a read-only filesystem.");
        }

        public void Delete(FileSystemPath path)
        {
            throw new InvalidOperationException("This is a read-only filesystem.");
        }
    }
}
