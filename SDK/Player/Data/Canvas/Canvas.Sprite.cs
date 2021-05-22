
using System;

namespace PixelVision8.Player
{
    public sealed partial class Canvas
    {
        private readonly Point spriteSize;
        private readonly GameChip gameChip;

        // TODO need to draw pixel data to the display and route the sprite and text through it

        private int _total;

        public Canvas(int width, int height, GameChip gameChip)
        {
            this.gameChip = gameChip;
            spriteSize = gameChip.SpriteSize();

            Configure(width, height);
        }

        public void DrawSprite(int id, int x, int y, bool flipH = false, bool flipV = false, int colorOffset = 0)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest;

            newRequest.Action = DrawPixelDataAction;

            // Copy over the pixel
            newRequest.PixelData.SetPixels(gameChip.Sprite(id), spriteSize.X, spriteSize.X);
            newRequest.X = x;
            newRequest.Y = y;
            newRequest.Bounds.X = 0;
            newRequest.Bounds.Y = 0;
            newRequest.Bounds.Width = spriteSize.X;
            newRequest.Bounds.Height = spriteSize.Y;
            newRequest.FlipH = flipH;
            newRequest.FlipV = flipV;
            newRequest.ColorOffset = colorOffset;

            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }

        public void DrawMetaSprite(int id, int x, int y, bool flipH = false, bool flipV = false, int colorOffset = 0)
        {
            if(gameChip == null)
                return;

            var metaSprite = gameChip.MetaSprite(id);

            var total = metaSprite.Sprites.Count;

            // Loop through each of the sprites
            for (var i = 0; i < total; i++)
            {
                var spriteData = metaSprite.Sprites[i];

                DrawSprite(
                    spriteData.Id,
                    spriteData.X + x,
                    spriteData.Y + y,
                    spriteData.FlipH,
                    spriteData.FlipV,
                    spriteData.ColorOffset + colorOffset
                );

            }
        }

        public void DrawMetaSprite(string name, int x, int y, bool flipH = false, bool flipV = false, int colorOffset = 0)
        {
            if(gameChip == null)
                return;
                
            DrawMetaSprite(gameChip.FindMetaSpriteId(name), x, y, flipH, flipV, colorOffset);
        }
        

    }
}