using System;
using System.IO;
using System.Linq;

namespace SharpFileSystem
{
    public class StandardEntityMover : IEntityMover
    {
        public int BufferSize { get; set; }

        public StandardEntityMover()
        {
            BufferSize = 65536;
        }

        public void Move(IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath)
        {
            bool isFile;
            if ((isFile = sourcePath.IsFile) != destinationPath.IsFile)
                throw new ArgumentException("The specified destination-path is of a different type than the source-path.");

            if (isFile)
            {
                using (var sourceStream = source.OpenFile(sourcePath, FileAccess.Read))
                {
                    using (var destinationStream = destination.CreateFile(destinationPath))
                    {
                        byte[] buffer = new byte[BufferSize];
                        int readBytes;
                        while ((readBytes = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                            destinationStream.Write(buffer, 0, readBytes);
                    }
                }
                source.Delete(sourcePath);
            }
            else
            {
                destination.CreateDirectory(destinationPath);
                foreach (var ep in source.GetEntities(sourcePath).ToArray())
                {
                    var destinationEntityPath = ep.IsFile
                                                    ? destinationPath.AppendFile(ep.EntityName)
                                                    : destinationPath.AppendDirectory(ep.EntityName);
                    Move(source, ep, destination, destinationEntityPath);
                }
                if (!sourcePath.IsRoot)
                    source.Delete(sourcePath);
            }
        }
    }
}
