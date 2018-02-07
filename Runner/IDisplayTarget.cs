
using PixelVisionSDK;

namespace PixelVisionRunner
{
    public interface IDisplayTarget
    {
        void ResetResolution(int width, int height);
        void Render();
        void CacheColors();
//        bool fullScreen { get; }
    }
}
