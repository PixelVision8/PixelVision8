using PixelVisionSDK;
using PixelVisionSDK.Chips;

namespace PixelVisionRunner
{
    public interface IInputFactory
    {
        ButtonState CreateButtonBinding(int playerIdx, Buttons button);
        IKeyInput CreateKeyInput();
        IMouseInput CreateMouseInput();
    }
}
