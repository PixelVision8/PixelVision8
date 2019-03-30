using System.IO;
using Microsoft.Xna.Framework;

namespace PixelVision8.Runner.Exporters
{
    public interface IImageExporter
    {
        void Write(int width, int height, Stream outputStream, Color[] colors);
    }
}