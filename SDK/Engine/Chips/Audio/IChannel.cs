namespace PixelVision8.Engine
{
    public interface IChannel
    {
        bool playing { get; }
        void Play(string settings, float? frequency = null);
    }
}