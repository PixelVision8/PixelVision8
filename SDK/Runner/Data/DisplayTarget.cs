//   
// Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by Pixel Vision 8 are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman - @JesseFreeman
// Christina-Antoinette Neofotistou @CastPixel
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany
//

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Globalization;
using PixelVision8.Player;

namespace PixelVision8.Runner
{
    public class DisplayTarget
    {
    
        public static Color HexToColor(string hex)
        {
            HexToRgb(hex, out var r, out var g, out var b);

            return new Color(r, g, b);
        }
        
        /// <summary>
        ///     Static method for converting a HEX color into an RGB value.
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static void HexToRgb(string hex, out byte r, out byte g, out byte b)
        {
            if (hex == null) hex = "FF00FF";

            if (hex[0] == '#') hex = hex.Substring(1);

            r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber); // / (float) byte.MaxValue;
            g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber); // / (float) byte.MaxValue;
            b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber); // / (float) byte.MaxValue;
        }
        
        /// <summary>
        ///     The display target is in charge of converting system colors into MonoGame colors. This utility method
        ///     can be used by any external runner class to correctly convert hex colors into Colors.
        /// </summary>
        /// <param name="hexColors"></param>
        /// <param name="maskColor"></param>
        /// <param name="debugMode"></param>
        /// <param name="backgroundColor"></param>
        /// <returns></returns>
        public static Color[] ConvertColors(string[] hexColors, string maskColor = "#FF00FF", bool debugMode = false, int backgroundColor = 0)
         {
             var t = hexColors.Length;
             var colors = new Color[t];

             for (var i = 0; i < t; i++)
             {
                 var colorHex = hexColors[i];

                 if (colorHex == maskColor && debugMode == false) colorHex = hexColors[backgroundColor];

                 var color = HexToColor(colorHex);
                 colors[i] = color;
             }

             return colors;
         }

        protected Vector2 Offset;
        protected Texture2D RenderTexture;
        protected readonly GraphicsDeviceManager GraphicManager;
        protected readonly SpriteBatch SpriteBatch;
        protected Color[] CachedColors;
        protected Rectangle VisibleRect;
        private readonly int _monitorHeight;
        private readonly int _monitorWidth;
        private int _monitorScale = 1;
        private int _totalPixels;
        private Color[] _pixelData = new Color[0];
        private int _colorId;
        private int _i;

        public Vector2 Scale = new Vector2(1, 1);
        protected int DisplayWidth;
        protected int DisplayHeight;

        public bool StretchScreen { get; set; }
        public bool Fullscreen { get; set; }

        public DisplayTarget(GraphicsDeviceManager graphicManager, int width, int height)
        {
            GraphicManager = graphicManager;

            GraphicManager.HardwareModeSwitch = false;

            SpriteBatch = new SpriteBatch(graphicManager.GraphicsDevice);

            _monitorWidth = MathHelper.Clamp(width, 64, 640);
            _monitorHeight = MathHelper.Clamp(height, 64, 480);
            
            ResetResolution(_monitorWidth, _monitorHeight);
        }

        public int MonitorScale
        {
            get => _monitorScale;
            set
            {
                var fits = false;

                while (fits == false)
                {
                    var newWidth = _monitorWidth * value;
                    var newHeight = _monitorHeight * value;

                    if (newWidth < GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width &&
                        newHeight < GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                    {
                        fits = true;
                        _monitorScale = value;
                    }
                    else
                    {
                        value--;
                    }
                }
            }
        }

        public virtual void ResetResolution(int gameWidth, int gameHeight)
        {
            
            if (RenderTexture == null || RenderTexture.Width != gameWidth || RenderTexture.Height != gameHeight)
            {
                RenderTexture = new Texture2D(GraphicManager.GraphicsDevice, gameWidth, gameHeight);
            }

            // Calculate the game's resolution
            VisibleRect.Width = RenderTexture.Width;
            VisibleRect.Height = RenderTexture.Height;

            var tmpMonitorScale = Fullscreen ? 1 : MonitorScale;

            // Calculate the monitor's resolution
            DisplayWidth = Fullscreen
                ? GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width
                : _monitorWidth *
                  tmpMonitorScale;
            DisplayHeight = Fullscreen
                ? GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
                : _monitorHeight * tmpMonitorScale;

            CalculateDisplayScale();

            CalculateDisplayOffset();

            // Apply changes
            GraphicManager.IsFullScreen = Fullscreen;

            if (GraphicManager.PreferredBackBufferWidth != DisplayWidth ||
                GraphicManager.PreferredBackBufferHeight != DisplayHeight)
            {
                GraphicManager.PreferredBackBufferWidth = DisplayWidth;
                GraphicManager.PreferredBackBufferHeight = DisplayHeight;
                GraphicManager.ApplyChanges();
            }

            _totalPixels = gameWidth * gameHeight;
            
            if (_pixelData.Length != _totalPixels)
            {
                Array.Resize(ref _pixelData, _totalPixels);
            }
        }

        protected virtual void CalculateDisplayOffset()
        {
            Offset.X = (DisplayWidth - VisibleRect.Width * Scale.X) * .5f;
            Offset.Y = (DisplayHeight - VisibleRect.Height * Scale.Y) * .5f;
        }

        protected virtual void CalculateDisplayScale()
        {
            // Calculate the game scale
            Scale.X = (float) DisplayWidth / VisibleRect.Width;
            Scale.Y = (float) DisplayHeight / VisibleRect.Height;

            if (!StretchScreen)
            {
                // To preserve the aspect ratio,
                // use the smaller scale factor.
                Scale.X = Math.Min(Scale.X, Scale.Y);
                Scale.Y = Scale.X;
            }
        }

        public virtual void RebuildColorPalette(string[] hexColors, int bgColorId = 0, string maskColor = "#FF00FF", bool debugMode = false)
        {
            
            CachedColors = ConvertColors(
                hexColors, 
                maskColor, 
                debugMode,
                bgColorId
            );
            
        }

        public virtual void Render(int[] pixels, int defaultColor)
        {
            
            // We can only update the display if the pixel lengths match up
            if (pixels.Length != _totalPixels)
                return;

            SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp);

            for (_i = 0; _i < _totalPixels; _i++)
            {
                _colorId = pixels[_i];
                _pixelData[_i] = CachedColors[_colorId < 0 ? defaultColor : _colorId];
            }

            RenderTexture.SetData(_pixelData);
            SpriteBatch.Draw(RenderTexture, Offset, VisibleRect, Color.White, 0f, Vector2.Zero, Scale,
                SpriteEffects.None, 1f);
            SpriteBatch.End();
        }
    }
}