using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelVision8.Runner
{
    public partial class DisplayTarget
    {
        private static int RealWidth => GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        private static int RealHeight => GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        
        private readonly int _paletteWidth = 256;
        private Vector2 _offset;
        private Texture2D _renderTexture;
        private GraphicsDeviceManager _graphicManager;
        private SpriteBatch _spriteBatch;
        private Color[] _cachedColors;
        private Color[] _pixelData = new Color[0];
        private Rectangle _renderRect;
        private bool _useCrt;
        private Effect _crtShader;
        private Texture2D _colorPalette;
        
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
        
        
        public bool CropScreen { get; set; } = true;

        public bool UseCrt
        {
            get => _useCrt;
            set
            {
                if (_crtShader == null) return;

                _useCrt = value;

                _crtShader?.Parameters["crtOn"].SetValue(value ? 1f : 0f);
                _crtShader?.Parameters["warp"].SetValue(value ? new Vector2(0.008f, 0.01f) : Vector2.Zero);
            }
        }

        public float Brightness
        {
            get => _crtShader?.Parameters["brightboost"]?.GetValueSingle() ?? 0;
            set => _crtShader?.Parameters["brightboost"]?.SetValue(MathHelper.Clamp(value, .255f, 1.5f));
        }

        public float Sharpness
        {
            get => _crtShader?.Parameters["hardPix"]?.GetValueSingle() ?? 0;
            set => _crtShader?.Parameters["hardPix"]?.SetValue(value);
        }

        public bool HasShader()
        {
            return _crtShader != null;
        }

        public Stream ShaderPath
        {
            set
            {
                using (var reader = new BinaryReader(value))
                {
                    _crtShader = new Effect(_graphicManager.GraphicsDevice,
                        reader.ReadBytes((int) reader.BaseStream.Length));
                }

                UseCrt = true;
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

            _colorPalette = new Texture2D(_graphicManager.GraphicsDevice, _paletteWidth,
                (int) Math.Ceiling(_cachedColors.Length / (double) _paletteWidth));

            // We need at least 256 colors for the shader to work so pad the array
            if (_cachedColors.Length < 256)
            {
                Array.Resize(ref _cachedColors, 256);
            }

            _colorPalette.SetData(_cachedColors);
            
        }
        
        private void UpdateShader()
        {
            _crtShader?.Parameters["textureSize"].SetValue(new Vector2(_renderTexture.Width, _renderTexture.Height));
            _crtShader?.Parameters["videoSize"].SetValue(new Vector2(_renderTexture.Width, _renderTexture.Height));
        }

        private void CalculateCrop()
        {
            if (CropScreen && !Fullscreen)
            {
                _visibleWidth = Math.Min(_visibleWidth, (int) (_gameWidth * _scaleX));
                _visibleHeight = Math.Min(_visibleHeight, (int) (_gameHeight * _scaleY));
                _offsetX = 0;
                _offsetY = 0;
            }
        }

        public void ConfigureDisplay()
        {
            CalculateResolution();

            CalculateDisplayScale();

            CalculateDisplayOffset();
                
            CalculateCrop();

            Apply();
                
            UpdateShader();

            ResetValidation();
        }
        
        public void Render(int[] pixels)
        {
            
            if (Invalid)
                ConfigureDisplay();
            
            _renderTexture.SetData(pixels);
            _spriteBatch.Begin();
            _crtShader.CurrentTechnique.Passes[0].Apply();
            _graphicManager.GraphicsDevice.Textures[1] = _colorPalette;
            _graphicManager.GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
            _spriteBatch.Draw(_renderTexture, _offset, _renderRect, Color.White, Vector2.Zero, Scale);
            _spriteBatch.End();
            
        }
    }
}