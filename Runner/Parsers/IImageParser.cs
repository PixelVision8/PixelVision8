using System.Collections.Generic;

namespace PixelVisionRunner.Parsers
{
    public interface IImageParser
    {
        bool IsImage();
        void ReadHeader();
        int width { get; }
        int height { get; }
        IColor[] colorPixels { get;}
        List<IColor> colorPalette { get; }
        void ReadStream();
        void ReadBytes(byte[] bytes);
    }
    
    
}