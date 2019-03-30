using System;

namespace SharpFileSystem
{
    public class File : FileSystemEntity, IEquatable<File>
    {
        public File(IFileSystem fileSystem, FileSystemPath path) : base(fileSystem, path)
        {
            if (!path.IsFile)
                throw new ArgumentException("The specified path is no file.", "path");
        }

        public bool Equals(File other)
        {
            return ((IEquatable<FileSystemEntity>)this).Equals(other);
        }
    }
}
