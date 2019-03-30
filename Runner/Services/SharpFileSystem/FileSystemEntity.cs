using System;

namespace SharpFileSystem
{
    public class FileSystemEntity: IEquatable<FileSystemEntity>
    {
        public IFileSystem FileSystem { get; private set; }
        public FileSystemPath Path { get; private set; }
        public string Name { get { return Path.EntityName; } }

        public FileSystemEntity(IFileSystem fileSystem, FileSystemPath path)
        {
            FileSystem = fileSystem;
            Path = path;
        }

        public override bool Equals(object obj)
        {
            var other = obj as FileSystemEntity;
            return (other != null) && ((IEquatable<FileSystemEntity>) this).Equals(other);
        }

        public override int GetHashCode()
        {
            return FileSystem.GetHashCode() ^ Path.GetHashCode();
        }

        bool IEquatable<FileSystemEntity>.Equals(FileSystemEntity other)
        {
            return FileSystem.Equals(other.FileSystem) && Path.Equals(other.Path);
        }

        public static FileSystemEntity Create(IFileSystem fileSystem, FileSystemPath path)
        {
            if (path.IsFile)
                return new File(fileSystem, path);
            else
                return new Directory(fileSystem, path);
        }
    }
}
