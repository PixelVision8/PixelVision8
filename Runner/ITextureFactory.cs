namespace PixelVisionRunner
{
    public interface ITextureFactory
    {
        ITexture2D NewTexture2D(int width, int height);
    }
}
