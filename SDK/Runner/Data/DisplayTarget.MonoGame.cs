using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelVision8.Runner
{
    
    
    public partial class DisplayTarget
    {
        private int _defaultColor;
        
        public void RebuildColorPalette(string[] hexColors, int bgColorId = 0, string maskColor = "#FF00FF", bool debugMode = false)
        {
            
            CachedColors = ConvertColors(
                hexColors, 
                maskColor, 
                debugMode,
                bgColorId
            );

            _defaultColor = bgColorId;

        }
        
        public virtual void Render(int[] pixels)
        {
            if (Invalid)
            {
                
                CalculateResolution();

                CalculateDisplayScale();

                CalculateDisplayOffset();

                Apply();
                
                ResetValidation();
                
            }
            
            // We can only update the display if the pixel lengths match up
            if (pixels.Length != _totalPixels)
                return;

            SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp);

            for (_i = 0; _i < _totalPixels; _i++)
            {
                _colorId = pixels[_i];
                _pixelData[_i] = CachedColors[_colorId < 0 ? _defaultColor : _colorId];
            }

            RenderTexture.SetData(_pixelData);
            SpriteBatch.Draw(RenderTexture, Offset, VisibleRect, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 1f);
            SpriteBatch.End();
        }
    }
}