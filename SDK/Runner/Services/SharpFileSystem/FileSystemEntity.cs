using System;

namespace SharpFileSystem
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