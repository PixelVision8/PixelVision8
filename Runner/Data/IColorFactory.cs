namespace PixelVisionRunner
{
    public interface IColorFactory
    {
        IColor magenta { get; }
        IColor clear { get; }
        IColor[] CreateArray(int length);
        IColor Create(float r, float g, float b);
    }
}
