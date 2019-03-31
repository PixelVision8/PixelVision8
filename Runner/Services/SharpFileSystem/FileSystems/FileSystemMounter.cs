using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpFileSystem.Collections;

namespace SharpFileSystem.FileSystems
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