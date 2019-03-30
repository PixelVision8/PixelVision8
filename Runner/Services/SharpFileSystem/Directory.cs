using System;

namespace SharpFileSystem
{
    public class Directory : FileSystemEntity, IEquatable<Directory>
    {
        public Directory(IFileSystem fileSystem, FileSystemPath path) : base(fileSystem, path)
        {
            if (!path.IsDirectory)
                throw new ArgumentException("The specified path is no directory.", "path");
        }

        public bool Equals(Directory other)
        {
            return ((IEquatable<FileSystemEntity>)this).Equals(other);
        }
    }
}
