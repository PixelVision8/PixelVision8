namespace PixelVision8.Runner
{
    public interface IFileLoader
    {
        string GetFileName(string path);
        byte[] ReadAllBytes(string path);

        bool Exists(string path);
    }
}