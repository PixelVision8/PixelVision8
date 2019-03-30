using System.IO;
using System.Collections.Generic;
using System;
namespace SharpFileSystem
{
public interface IFileSystem: IDisposable
{
    ICollection<FileSystemPath> GetEntities(FileSystemPath path);
    bool Exists(FileSystemPath path);
    Stream CreateFile(FileSystemPath path);
    Stream OpenFile(FileSystemPath path, FileAccess access);
    void CreateDirectory(FileSystemPath path);
    void Delete(FileSystemPath path);
}
}
