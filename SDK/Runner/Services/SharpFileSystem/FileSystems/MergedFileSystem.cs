using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpFileSystem.FileSystems
{
    public class MergedFileSystem : IFileSystem
    {
        public MergedFileSystem(IEnumerable<IFileSystem> fileSystems)
        {
            FileSystems = fileSystems.ToArray();
        }

        public MergedFileSystem(params IFileSystem[] fileSystems)
        {
            FileSystems = fileSystems.ToArray();
        }

        public IEnumerable<IFileSystem> FileSystems { get; set; }

        public void Dispose()
        {
            foreach (var fs in FileSystems)
                fs.Dispose();
        }

        public ICollection<FileSystemPath> GetEntities(FileSystemPath path)
        {
            var entities = new SortedList<FileSystemPath, FileSystemPath>();
            foreach (var fs in FileSystems.Where(fs => fs.Exists(path)))
            foreach (var entity in fs.GetEntities(path))
                if (!entities.ContainsKey(entity))
                    entities.Add(entity, entity);
            return entities.Values;
        }

        public bool Exists(FileSystemPath path)
        {
            return FileSystems.Any(fs => fs.Exists(path));
        }

        public Stream CreateFile(FileSystemPath path)
        {
            var fs = GetFirst(path) ?? FileSystems.First();
            return fs.CreateFile(path);
        }

        public Stream OpenFile(FileSystemPath path, FileAccess access)
        {
            var fs = GetFirst(path);
            if (fs == null)
                throw new FileNotFoundException();
            return fs.OpenFile(path, access);
        }

        public void CreateDirectory(FileSystemPath path)
        {
            if (Exists(path))
                throw new ArgumentException("The specified directory already exists.");
            var fs = GetFirst(path.ParentPath);
            if (fs == null)
                throw new ArgumentException("The directory-parent does not exist.");
            fs.CreateDirectory(path);
        }

        public void Delete(FileSystemPath path)
        {
            foreach (var fs in FileSystems.Where(fs => fs.Exists(path)))
                fs.Delete(path);
        }

        public IFileSystem GetFirst(FileSystemPath path)
        {
            return FileSystems.FirstOrDefault(fs => fs.Exists(path));
        }
    }
}