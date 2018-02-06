namespace PixelVisionRunner
{
    public interface ITexture2D
    {
        int width { get; }
        int height { get; }
        IColor[] GetPixels();
        IColor[] GetPixels(int x, int y, int blockWidth, int blockHeight);
        IColor GetPixel(int x, int y);
        IColor32[] GetPixels32();
        void Resize(int width, int height);
        void SetPixels(int x, int y, int width, int height, IColor[] pixelData);
        void UsePointFiltering(); // set filterPoint in unity
        void LoadImage(byte[] data);
    }
}
