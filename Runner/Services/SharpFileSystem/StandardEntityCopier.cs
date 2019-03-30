using System;
using System.IO;

namespace SharpFileSystem
{
    public class StandardEntityCopier : IEntityCopier
    {
        public int BufferSize { get; set; }

        public StandardEntityCopier()
        {
            BufferSize = 65536;
        }

        public void Copy(IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath)
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
            }
            else
            {
                if (!destinationPath.IsRoot)
                    destination.CreateDirectory(destinationPath);
                foreach (var ep in source.GetEntities(sourcePath))
                {
                    var destinationEntityPath = ep.IsFile
                                                    ? destinationPath.AppendFile(ep.EntityName)
                                                    : destinationPath.AppendDirectory(ep.EntityName);
                    Copy(source, ep, destination, destinationEntityPath);
                }
            }
        }
    }
}
