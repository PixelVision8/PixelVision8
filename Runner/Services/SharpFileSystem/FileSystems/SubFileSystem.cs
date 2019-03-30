using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpFileSystem.Collections;

namespace SharpFileSystem.FileSystems
{
    public class SubFileSystem: IFileSystem
    {
        public IFileSystem FileSystem { get; private set; }
        public FileSystemPath Root { get; private set; }

        public SubFileSystem(IFileSystem fileSystem, FileSystemPath root)
        {
            FileSystem = fileSystem;
            Root = root;
        }

        protected FileSystemPath AppendRoot(FileSystemPath path)
        {
            return Root.AppendPath(path);
        }

        protected FileSystemPath RemoveRoot(FileSystemPath path)
        {
            return path.RemoveParent(Root);
        }

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
    }
}
