using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PixelVision8.Runner.Parsers
{
    public interface IImageParser
    {
        bool IsImage();
        void ReadHeader();
        int width { get; }
        int height { get; }
        Color[] colorPixels { get;}
        List<Color> colorPalette { get; }
        void ReadStream();
        void ReadBytes(byte[] bytes);
    }
    
    
}