﻿//   
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

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Utils;

namespace PixelVision8.Runner.Data
{
    public class DisplayTargetLite : IDisplayTarget
    {
        public Vector2 offset;
        public Texture2D renderTexture;
        protected readonly GraphicsDeviceManager GraphicManager;
        protected readonly SpriteBatch SpriteBatch;
        protected Color[] CachedColors;
        protected Rectangle VisibleRect;
        private readonly int _monitorHeight;
        private readonly int _monitorWidth;
        private int _monitorScale = 1;
        private int _totalPixels;
        private Color[] _pixelData = new Color[0];
        private int _colorID;
        private int _i;
        
        // TODO there is some conflict with this and the scale getter
        protected Vector2 _scale = new Vector2(1, 1);
        
        public bool stretchScreen { get; set; }
        public bool cropScreen { get; set; } = true;
        public bool fullscreen { get; set; } = false;
        public Vector2 Scale => _scale;
        
        public DisplayTargetLite(GraphicsDeviceManager graphicManager, int width, int height)
        {
            this.GraphicManager = graphicManager;

            this.GraphicManager.HardwareModeSwitch = false;

            SpriteBatch = new SpriteBatch(graphicManager.GraphicsDevice);

            _monitorWidth = MathHelper.Clamp(width, 64, 640);
            _monitorHeight = MathHelper.Clamp(height, 64, 480);
        }

        public void Render(int[] pixels, byte defaultColor = 0)
        {
            throw new NotImplementedException();
        }

        public int monitorScale
        {
            get => _monitorScale;
            set
            {
                var maxWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                var maxHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

                var fits = false;

                while (fits == false)
                {
                    var newWidth = _monitorWidth * value;
                    var newHeight = _monitorHeight * value;

                    if (newWidth < maxWidth && newHeight < maxHeight)
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

        public virtual void ResetResolution(int gameWidth, int gameHeight, int overScanX = 0, int overScanY = 0)
        {
            if (renderTexture == null || renderTexture.Width != gameWidth || renderTexture.Height != gameHeight)
            {
                renderTexture = new Texture2D(GraphicManager.GraphicsDevice, gameWidth, gameHeight);
            }

            // Calculate the game's resolution
            VisibleRect.Width = renderTexture.Width - overScanX;
            VisibleRect.Height = renderTexture.Height - overScanY;

            var tmpMonitorScale = fullscreen ? 1 : monitorScale;

            // Calculate the monitor's resolution
            var displayWidth = fullscreen
                ? GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width
                : _monitorWidth *
                  tmpMonitorScale;
            var displayHeight = fullscreen
                ? GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
                : _monitorHeight * tmpMonitorScale;
            
            // Calculate the game scale
            _scale.X = (float) displayWidth / VisibleRect.Width;
            _scale.Y = (float) displayHeight / VisibleRect.Height;

            if (!stretchScreen)
            {
                // To preserve the aspect ratio,
                // use the smaller scale factor.
                _scale.X = Math.Min(Scale.X, Scale.Y);
                _scale.Y = Scale.X;
            }

            offset.X = (displayWidth - VisibleRect.Width * Scale.X) * .5f;
            offset.Y = (displayHeight - VisibleRect.Height * Scale.Y) * .5f;

            if (cropScreen && !fullscreen)
            {
                displayWidth = Math.Min(displayWidth, (int) (VisibleRect.Width * Scale.X));
                displayHeight = Math.Min(displayHeight, (int) (VisibleRect.Height * Scale.Y));
                offset.X = 0;
                offset.Y = 0;
            }

            // Apply changes
            GraphicManager.IsFullScreen = fullscreen;

            if (GraphicManager.PreferredBackBufferWidth != displayWidth ||
                GraphicManager.PreferredBackBufferHeight != displayHeight)
            {
                GraphicManager.PreferredBackBufferWidth = displayWidth;
                GraphicManager.PreferredBackBufferHeight = displayHeight;
                GraphicManager.ApplyChanges();
            }

            _totalPixels = gameWidth * gameHeight;
            if (_pixelData.Length != _totalPixels)
            {
                Array.Resize(ref _pixelData, _totalPixels);
            }
        }

        public virtual void RebuildColorPalette(ColorChip colorChip)
        {
            CachedColors = ColorUtils.ConvertColors(colorChip.hexColors, colorChip.maskColor, colorChip.debugMode,
                colorChip.backgroundColor);
        }
        
        public virtual void Render(int[] pixels, int defaultColor)
        {

            // We can only update the display if the pixel lengths match up
            if (pixels.Length != _totalPixels)
                return;

            SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp);
            
            for (_i = 0; _i < _totalPixels; _i++)
            {
                _colorID = pixels[_i];
                _pixelData[_i] = CachedColors[_colorID < 0 ? defaultColor : _colorID];
            }
            
            renderTexture.SetData(_pixelData);
            SpriteBatch.Draw(renderTexture, offset, VisibleRect, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 1f);
            SpriteBatch.End();
            
        }

    }
}