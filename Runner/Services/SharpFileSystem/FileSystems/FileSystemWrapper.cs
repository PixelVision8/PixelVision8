using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpFileSystem.FileSystems
{
    public class FileSystemWrapper : IFileSystem
    {
        public IFileSystem Parent { get; private set; }

        public FileSystemWrapper(IFileSystem parent)
        {
            Parent = parent;
        }

        public ICollection<FileSystemPath> GetEntities(FileSystemPath path)
        {
            return Parent.GetEntities(path);
        }

        public bool Exists(FileSystemPath path)
        {
            return Parent.Exists(path);
        }

        public Stream CreateFile(FileSystemPath path)
        {
            return Parent.CreateFile(path);
        }

        public Stream OpenFile(FileSystemPath path, FileAccess access)
        {
            return Parent.OpenFile(path, access);
        }

        public void CreateDirectory(FileSystemPath path)
        {
            Parent.CreateDirectory(path);
        }

        public void Delete(FileSystemPath path)
        {
            Parent.Delete(path);
        }

        public void Dispose()
        {
            Parent.Dispose();
        }
    }
}
