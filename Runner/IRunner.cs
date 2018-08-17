using System.Collections.Generic;
using PixelVisionSDK;

namespace PixelVisionRunner
{
    public interface IRunner
    {
        IEngine activeEngine { get; }
        void ProcessFiles(IEngine tmpEngine, Dictionary<string, byte[]> files, bool displayProgress = false);
    }
}