
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
            if(gameChip == null)
                return;

            DrawSpriteRef(id, x, y, flipH, flipV, colorOffset, gameChip.Sprite);
        }

        protected void DrawSpriteRef(int id, int x, int y, bool flipH = false, bool flipV = false, int colorOffset = 0, Func<int, int[], int[]> getPixelsCallback = null)
        {

            var getRequest = NextRequest();

            if (getRequest == null || getPixelsCallback == null)
                return;

            var newRequest = getRequest;

            newRequest.Action = DrawPixelDataAction;

            // Copy over the pixel
            newRequest.PixelData.SetPixels(getPixelsCallback(id, null), spriteSize.X, spriteSize.X);
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

            DrawMetaSprite(gameChip.MetaSprite(id), x, y, flipH, flipV, colorOffset);

        }
        
        public void DrawMetaSprite(string name, int x, int y, bool flipH = false, bool flipV = false, int colorOffset = 0)
        {
            if(gameChip == null)
                return;
                
            DrawMetaSprite(gameChip.FindMetaSpriteId(name), x, y, flipH, flipV, colorOffset);
        }

        public void DrawMetaSprite(SpriteCollection spriteCollection, int x, int y, bool flipH = false, bool flipV = false, int colorOffset = 0)
        {

            if(gameChip == null)
                return;
                
            DrawMetaSpriteRef(spriteCollection, x, y, flipH, flipV, colorOffset, gameChip.Sprite);

        }

        public void DrawMetaSpriteRef(SpriteCollection spriteCollection, int x, int y, bool flipH = false, bool flipV = false, int colorOffset = 0, Func<int, int[], int[]> drawCallback = null)
        {
            // Get the sprite data for the meta sprite
            var tmpSpritesData = spriteCollection.Sprites;
            var total = tmpSpritesData.Count;
                
            int startX, startY;
            bool tmpFlipH, tmpFlipV;
            
            // Loop through each of the sprites
            for (var i = 0; i < total; i++)
            {
                var _currentSpriteData = tmpSpritesData[i];

                if (_currentSpriteData.Id != -1)
                {
                    // Get sprite values
                    startX = _currentSpriteData.X;
                    startY = _currentSpriteData.Y;
                    tmpFlipH = _currentSpriteData.FlipH;
                    tmpFlipV = _currentSpriteData.FlipV;

                    if (flipH)
                    {
                        startX = spriteCollection.Bounds.Width - startX - Constants.SpriteSize;
                        tmpFlipH = !tmpFlipH;
                    }

                    if (flipV)
                    {
                        startY = spriteCollection.Bounds.Height - startY - Constants.SpriteSize;
                        tmpFlipV = !tmpFlipV;
                    }

                    startX += x;
                    startY += y;

                    DrawSpriteRef(
                        _currentSpriteData.Id,
                        startX,
                        startY,
                        tmpFlipH,
                        tmpFlipV,
                        _currentSpriteData.ColorOffset + colorOffset,
                        drawCallback
                    );
                }
            }
        }
        

    }
}