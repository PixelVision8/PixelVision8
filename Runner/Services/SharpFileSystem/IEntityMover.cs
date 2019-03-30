namespace SharpFileSystem
{
    public interface IEntityMover
    {
        void Move(IFileSystem source, FileSystemPath sourcePath, IFileSystem destination, FileSystemPath destinationPath);
    }
}
