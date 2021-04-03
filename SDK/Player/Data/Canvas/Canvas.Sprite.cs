
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

            // We need at least 1 pixel to save the sprite ID
            // if (newRequest.PixelData.Width != spriteSize.X || newRequest.PixelData.Height != spriteSize.Y)
            // {
            //     Utilities.Resize(newRequest.PixelData, spriteSize.X, spriteSize.X);
            // }

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

        // public void DrawSprites(int[] ids, int x, int y, int width, bool flipH = false, bool flipV = false,
        //     int colorOffset = 0)
        // {
        //     _total = ids.Length;

        //     var startX = x;
        //     var startY = y;

        //     var paddingW = spriteSize.X;
        //     var paddingH = spriteSize.Y;

        //     // TODO need to offset the bounds based on the scroll position before testing against it

        //     for (var i = 0; i < _total; i++)
        //     {
        //         // Set the sprite id
        //         var id = ids[i];

        //         // TODO should also test that the sprite is not greater than the total sprites (from a cached value)
        //         // Test to see if the sprite is within range
        //         if (id > -1)
        //         {
        //             x = (int)Math.Floor((double)i % width) * paddingW + startX;
        //             y = (int)Math.Floor((double)i / width) * paddingH + startY;

        //             DrawSprite(id, x, y, flipH, flipV, colorOffset);
        //         }
        //     }
        // }

        public void DrawMetaSprite(int id, int x, int y, bool flipH = false, bool flipV = false,
            int colorOffset = 0)
        {

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
        

    }
}