using System.Collections.Generic;
using PixelVisionSDK;

namespace PixelVisionRunner
{
    public interface IRunner
    {
        IEngine activeEngine { get; }
        void ProcessFiles(IEngine tmpEngine, Dictionary<string, byte[]> files, bool displayProgress = false);
        void DisplayWarning(string message);
        int Volume(int? value = null);
        bool Mute(bool? value = null);
        int Scale(int? scale = null);
        bool Fullscreen(bool? value = null);
        bool StretchScreen(bool? value = null);
        bool CropScreen(bool? value = null);
        void DebugLayers(bool value);
        void ToggleLayers(int value);
        void ResetGame();
    }
}