using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelVisionRunner.Exporters
{
    public interface IImageExporter
    {
        void Write(int width, int height, Stream outputStream, IColor[] colors);
    }
}