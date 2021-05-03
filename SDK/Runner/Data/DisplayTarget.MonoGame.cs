using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelVision8.Runner
{
    
    
    public partial class DisplayTarget
    {
        private static int RealWidth => GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        private static int RealHeight => GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        
        private int _defaultColor;
        private Vector2 _offset;
        private Texture2D _renderTexture;
        private GraphicsDeviceManager _graphicManager;
        private SpriteBatch _spriteBatch;
        private Color[] _cachedColors;
        private Color[] _pixelData = new Color[0];
        private Rectangle _renderRect;
        
        public Vector2 Scale = new Vector2(1, 1);

        public GraphicsDeviceManager GraphicsManager
        {
            set
            {
                _graphicManager = value;

                _graphicManager.HardwareModeSwitch = false;

                _spriteBatch = new SpriteBatch(_graphicManager.GraphicsDevice);
            }
        }
        
        public void RebuildColorPalette(string[] hexColors, int bgColorId = 0, string maskColor = "#FF00FF", bool debugMode = false)
        {
            
            _cachedColors = ColorUtils.ConvertColors(
                hexColors, 
                maskColor, 
                debugMode,
                bgColorId
            );

            _defaultColor = bgColorId;

        }
        
        private void Apply()
        {
            // Apply changes
            _graphicManager.IsFullScreen = Fullscreen;

            if (_graphicManager.PreferredBackBufferWidth != _visibleWidth ||
                _graphicManager.PreferredBackBufferHeight != _visibleHeight)
            {
                _graphicManager.PreferredBackBufferWidth = _visibleWidth;
                _graphicManager.PreferredBackBufferHeight = _visibleHeight;
                _graphicManager.ApplyChanges();
            }
            
            if (_renderTexture == null || _renderTexture.Width != _gameWidth || _renderTexture.Height != _gameHeight)
            {
                _renderTexture = new Texture2D(_graphicManager.GraphicsDevice, _gameWidth, _gameHeight);
                
                _totalPixels = _renderTexture.Width * _renderTexture.Height;

                if (_pixelData.Length != _totalPixels)
                {
                    Array.Resize(ref _pixelData, _totalPixels);
                }
                
                // Calculate the game's resolution
                _renderRect.Width = _renderTexture.Width;
                _renderRect.Height = _renderTexture.Height;
            }
            
            _offset.X = _offsetX;
            _offset.Y = _offsetY;
            
            Scale.X = _scaleX;
            Scale.Y = _scaleY;
            
        }
        
        public void ConfigureDisplay()
        {
            CalculateResolution();

            CalculateDisplayScale();

            CalculateDisplayOffset();
            
            CalculateCrop();

            Apply();

            ResetValidation();
        }
        
        public virtual void Render(int[] pixels)
        {
            if (Invalid)
                ConfigureDisplay();
            
            // We can only update the display if the pixel lengths match up
            if (pixels.Length != _totalPixels)
                return;

            _spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp);

            for (var i = 0; i < _totalPixels; i++)
            {
                _pixelData[i] = _cachedColors[pixels[i] < 0 ? _defaultColor : pixels[i]];
            }

            _renderTexture.SetData(_pixelData);
            _spriteBatch.Draw(_renderTexture, _offset, _renderRect, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 1f);
            _spriteBatch.End();
        }
    }
}