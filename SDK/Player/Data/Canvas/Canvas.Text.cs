using PixelVision8.Player;

namespace PixelVision8.Player
{
    public sealed partial class Canvas
    {
        public void DrawText(string text, int x, int y, string font = "default", int colorOffset = 0, int spacing = 0)
        {
            // This only works when the canvas has a reference to the gameChip
            if (gameChip == null) return;

            var total = text.Length;
            var nextX = x;

            for (var i = 0; i < total; i++)
            {
                var getRequest = NextRequest();

                if (getRequest == null)
                    return;

                var newRequest = getRequest;

                newRequest.Action = DrawPixelDataAction;

                // We need at least 1 pixel to save the sprite ID
                if (newRequest.PixelData.Width != spriteSize.X || newRequest.PixelData.Height != spriteSize.Y)
                {
                    Utilities.Resize(newRequest.PixelData, spriteSize.X, spriteSize.X);
                }

                // Copy over the pixel
                newRequest.PixelData.SetPixels(gameChip.FontChar(text[i], font));

                newRequest.X = nextX;
                newRequest.Y = y;
                newRequest.Bounds.X = 0;
                newRequest.Bounds.Y = 0;
                newRequest.Bounds.Width = spriteSize.X;
                newRequest.Bounds.Height = spriteSize.Y;
                newRequest.FlipH = false;
                newRequest.FlipV = false;
                newRequest.ColorOffset = colorOffset;

                // Save the changes to the request
                requestPool[currentRequest] = newRequest;

                nextX += spriteSize.X + spacing;
            }
        }
    }
}