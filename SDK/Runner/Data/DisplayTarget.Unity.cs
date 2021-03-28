using System;
using UnityEngine;
using UnityEngine.UI;
using PixelVision8.Player;

// using Microsoft.Xna.Framework;
// using Microsoft.Xna.Framework.Graphics;

namespace PixelVision8.Runner
{
    
    
    public partial class DisplayTarget
    {
        
        
        private static int RealWidth = 256;//> GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        private static int RealHeight = 240;//> GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        
        private int _defaultColor;
        private Vector2 _offset;
        private Texture2D _renderTexture;
        // private GraphicsDeviceManager _graphicManager;
        // private SpriteBatch _spriteBatch;
        private Color[] _cachedColors;
        private Color[] _pixelData = new Color[0];
        private Rect _renderRect;
        
        public Vector2 Scale = new Vector2(1, 1);
        private RawImage _displayTarget;

        public DisplayTarget(int width, int height, RawImage displayTarget)
        {
            _displayWidth = Utilities.Clamp(width, 64, 640);
            _displayHeight = Utilities.Clamp(height, 64, 480);
            
            ResetResolution(_displayWidth, _displayHeight);

            // Before we set up the PixelVisionEngine we'll want to configure the renderTexture. 
            // We'll create a new 256 x 240 Texture2D instance and set it as the displayTarget.texture.
            _renderTexture = new Texture2D(width, height, TextureFormat.ARGB32, false) {filterMode = FilterMode.Point};

            _displayTarget = displayTarget;
            _displayTarget.texture = _renderTexture;

        }

        // public GraphicsDeviceManager GraphicsManager
        // {
        //     set
        //     {
        //         _graphicManager = value;

        //         _graphicManager.HardwareModeSwitch = false;

        //         _spriteBatch = new SpriteBatch(_graphicManager.GraphicsDevice);
        //     }
        // }
        
        public void RebuildColorPalette(string[] hexColors, int bgColorId = 0, string maskColor = "#FF00FF", bool debugMode = false)
        {
            
            _cachedColors = ColorUtils.ConvertColors(
                hexColors, 
                maskColor, 
                debugMode,
                bgColorId
            );

            _defaultColor = bgColorId;

            // Debug.Log("Rebuild Palette" + _cachedColors[0] + " vs " + hexColors[0]);

        }
        
        private void Apply()
        {
            // Apply changes
            // _graphicManager.IsFullScreen = Fullscreen;

            // if (_graphicManager.PreferredBackBufferWidth != _visibleWidth ||
            //     _graphicManager.PreferredBackBufferHeight != _visibleHeight)
            // {
            //     _graphicManager.PreferredBackBufferWidth = _visibleWidth;
            //     _graphicManager.PreferredBackBufferHeight = _visibleHeight;
            //     _graphicManager.ApplyChanges();
            // }
            
            // if (_renderTexture == null || _renderTexture.Width != _gameWidth || _renderTexture.Height != _gameHeight)
            // {
            //     _renderTexture = new Texture2D(_graphicManager.GraphicsDevice, _gameWidth, _gameHeight);
                
            //     _totalPixels = _renderTexture.Width * _renderTexture.Height;

            //     if (_pixelData.Length != _totalPixels)
            //     {
            //         Array.Resize(ref _pixelData, _totalPixels);
            //     }
                
            //     // Calculate the game's resolution
            //     _renderRect.Width = _renderTexture.Width;
            //     _renderRect.Height = _renderTexture.Height;
                
            // }

            Screen.fullScreen = Fullscreen;

            // The first thing we'll do to update the displayTarget recalculate the correct aspect ratio. Here we get a reference 
            // to the AspectRatioFitter component then set the aspectRatio property to the value of the width divided by the height. 
            var fitter = _displayTarget.GetComponent<AspectRatioFitter>();
            fitter.aspectRatio = (float) _gameWidth / _gameHeight;

            // Next we need to update the CanvasScaler's referenceResolution value.
            var canvas = _displayTarget.canvas;
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.referenceResolution = new Vector2(_gameWidth, _gameHeight);

            // TODO only resize if needed
            // Now we can resize the redenerTexture to also match the new resolution.
            _renderTexture.Resize(_gameWidth, _gameHeight);
            
            // At this point, the Unity-specific UI is correctly configured. The CanvasScaler and AspectRetioFitter will ensure that 
            // the Texture we use to show the DisplayChip's pixel data will always maintain it's aspect ratio no matter what the game's 
            // real resolution is.

            // Now it's time to resize our cahcedPixels array. We calculate the total number of pixels by multiplying the width by the 
            // height. We'll use this array to make sure we have enough pixels to correctly render the DisplayChip's own pixel data.
            // var totalPixels = _renderTexture.width * _renderTexture.height;
            // Array.Resize(ref cachedPixels, totalPixels);

            _totalPixels = _renderTexture.width * _renderTexture.height;

            if (_pixelData.Length != _totalPixels)
            {
                Array.Resize(ref _pixelData, _totalPixels);
            }

            // var pixels = _renderTexture.GetPixels();
            // for (int i = 0; i < _totalPixels; i++)
            // {
            //     pixels[i] = new Color(_cachedColors[1].r, _cachedColors[1].g, _cachedColors[1].b);
            // }
            // _renderTexture.SetPixels(pixels);
            // _renderTexture.Apply();

            // var offsetY = 1 - 1;
            _displayTarget.uvRect = new UnityEngine.Rect(0, 0, 1, -1);

            
            // _offset.x = _offsetX;
            // _offset.y = _offsetY;
            
            // Scale.x = _scaleX;
            // Scale.y = _scaleY;
            
        }
        
        public void ConfigureDisplay()
        {
            CalculateResolution();

            CalculateDisplayScale();

            CalculateDisplayOffset();

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

            for (var i = 0; i < _totalPixels; i++)
            {
                _pixelData[i] = _cachedColors[pixels[i] < 0 ? _defaultColor : pixels[i]];
            }

            // // At this point, we have all the color data we need to update the renderTexture. We'll set the cachedPixels on the renderTexture and call 
            // // Apply() to re-render the Texture.
            _renderTexture.SetPixels(_pixelData);
            _renderTexture.Apply();

        }
    }
}