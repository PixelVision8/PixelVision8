using System.IO;
using Microsoft.Xna.Framework;

namespace PixelVisionRunner.Exporters
{
    public interface IImageExporter
    {
        void Write(int width, int height, Stream outputStream, Color[] colors);
    }
}