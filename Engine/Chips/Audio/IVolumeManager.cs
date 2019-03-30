namespace PixelVision8.Engine.Chips
{
    public interface IVolumeManager
    {
        int Volume(int? value = null);
        bool Mute(bool? value = null);
    }
}