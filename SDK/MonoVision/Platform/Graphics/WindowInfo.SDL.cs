using System;

namespace MonoGame.OpenGL
{
    internal class WindowInfo
    {
        public IntPtr Handle { get; private set; }

        public WindowInfo(IntPtr handle)
        {
            Handle = handle;
        }
    }
}
