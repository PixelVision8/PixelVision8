namespace SharpFileSystem
{
    public interface IEntityCopier
    {
        void Copy(IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath);
    }
}
