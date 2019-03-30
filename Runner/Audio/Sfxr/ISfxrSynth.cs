namespace PixelVisionRunner.Chips.Sfxr
{
    public interface ISfxrSynth
    {
        // this call is used generally by MonoGame
        byte[] GenerateWav(uint sampleRate = 44100, uint bitDepth = 16);
    }
}
