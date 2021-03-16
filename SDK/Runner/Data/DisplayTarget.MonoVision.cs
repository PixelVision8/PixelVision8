using Microsoft.Xna.Framework;
namespace PixelVision8.Runner
{
    public partial class DisplayTarget
    {
        public virtual void Render(int[] pixels, int defaultColor)
        {
            
            // We can only update the display if the pixel lengths match up
            if (pixels.Length != _totalPixels)
                return;

            SpriteBatch.Begin(); //SpriteSortMode.Immediate

            for (_i = 0; _i < _totalPixels; _i++)
            {
                _colorId = pixels[_i];
                _pixelData[_i] = CachedColors[_colorId < 0 ? defaultColor : _colorId];
            }

            RenderTexture.SetData(_pixelData);
            SpriteBatch.Draw(RenderTexture, Offset, VisibleRect, Color.White, Vector2.Zero, Scale);
            SpriteBatch.End();
        }
    }
}