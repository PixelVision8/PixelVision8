
using PixelVisionSDK;

namespace PixelVisionRunner
{
    public interface IDisplayTarget
    {
        void ResetResolution(int width, int height, bool fullScreen, int overscanXPixels = 0, int overscanYPixels = 0);
        void Render(int[] pixelColors, int bgColor);
        void CacheColors(ColorData[] colorsData);
    }
}
