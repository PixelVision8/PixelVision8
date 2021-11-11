using System;

namespace PixelVision8.Player
{
    public sealed partial class Canvas
    {
        public void DrawText(string text, int x, int y, string font = "default", int colorOffset = 0, int spacing = 0, bool flipH = false, bool flipV = false)
        {

            DrawMetaSpriteRef(gameChip.ConvertTextToSprites(text, font, spacing), x, y, flipH, flipV, colorOffset, gameChip.Char);

        }
    }
}