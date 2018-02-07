
using PixelVisionSDK;

namespace PixelVisionRunner
{
    public interface IDisplayTarget
    {
        void ResetResolution(int width, int height, bool fullScreen);
        void Render();
        void CacheColors();
    }
}
