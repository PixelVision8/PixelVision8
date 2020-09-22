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

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Utils;

namespace PixelVision8.Runner.Data
{
    public class DisplayTarget : IDisplayTarget
    {
        private readonly int _monitorHeight;
        private readonly int _monitorWidth;
        private readonly GraphicsDeviceManager graphicManager;
        public readonly SpriteBatch spriteBatch;
        private int _monitorScale = 1;
        private bool _useCRT;
        public bool cropScreen = true;
        private Effect crtShader;
        public bool fullscreen = false;
        public Vector2 offset;
        public Texture2D renderTexture;
        public Vector2 scale = new Vector2(1, 1);
        // private Effect shaderEffect;
        public bool stretchScreen;
        private Rectangle visibleRect;

        // TODO think we just need to pass in the active game and not the entire runner?
        public DisplayTarget(GraphicsDeviceManager graphicManager, int width, int height)
        {
            this.graphicManager = graphicManager;

            this.graphicManager.HardwareModeSwitch = false;

            spriteBatch = new SpriteBatch(graphicManager.GraphicsDevice);

            _monitorWidth = MathHelper.Clamp(width, 64, 640);
            _monitorHeight = MathHelper.Clamp(height, 64, 480);
        }

        public bool useCRT
        {
            get
            {
                return _useCRT;
            }
            set
            {
                if (crtShader == null) return;

                _useCRT = value;

                crtShader?.Parameters["crtOn"].SetValue(value ? 1f : 0f);
                crtShader?.Parameters["warp"].SetValue(value ? new Vector2(0.008f, 0.01f) : Vector2.Zero);

            }
        }

        public float brightness
        {
            get => crtShader?.Parameters["brightboost"]?.GetValueSingle() ?? 0;
            set => crtShader?.Parameters["brightboost"]?.SetValue(MathHelper.Clamp(value, .255f, 1.5f));
        }

        public float sharpness
        {
            get => crtShader?.Parameters["hardPix"]?.GetValueSingle() ?? 0;
            set => crtShader?.Parameters["hardPix"]?.SetValue(value);
        }

        public bool HasShader()
        {
            return crtShader != null;
        }

        public Stream shaderPath
        {
            set
            {
                
                using (var reader = new BinaryReader(value))
                {
                    crtShader = new Effect(graphicManager.GraphicsDevice,
                        reader.ReadBytes((int) reader.BaseStream.Length));
                }

                useCRT = true;
            }
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

        public void ResetResolution(int gameWidth, int gameHeight, int overScanX = 0, int overScanY = 0)
        {
            if (renderTexture == null || renderTexture.Width != gameWidth || renderTexture.Height != gameHeight)
            {
                renderTexture = new Texture2D(graphicManager.GraphicsDevice, gameWidth, gameHeight);

                crtShader?.Parameters["textureSize"].SetValue(new Vector2(gameWidth, gameHeight));
                crtShader?.Parameters["videoSize"].SetValue(new Vector2(gameWidth, gameHeight));
                
            }

            // Calculate the game's resolution
            visibleRect.Width = renderTexture.Width - overScanX;
            visibleRect.Height = renderTexture.Height - overScanY;

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
            // TODO need to figure out scale
            scale.X = (float) displayWidth / visibleRect.Width;
            scale.Y = (float) displayHeight / visibleRect.Height;

            if (!stretchScreen)
            {
                // To preserve the aspect ratio,
                // use the smaller scale factor.
                scale.X = Math.Min(scale.X, scale.Y);
                scale.Y = scale.X;
            }

            offset.X = (displayWidth - visibleRect.Width * scale.X) * .5f;
            offset.Y = (displayHeight - visibleRect.Height * scale.Y) * .5f;

            if (cropScreen && !fullscreen)
            {
                displayWidth = Math.Min(displayWidth, (int) (visibleRect.Width * scale.X));
                displayHeight = Math.Min(displayHeight, (int) (visibleRect.Height * scale.Y));
                offset.X = 0;
                offset.Y = 0;
            }

            // Apply changes
            graphicManager.IsFullScreen = fullscreen;

            if (graphicManager.PreferredBackBufferWidth != displayWidth ||
                graphicManager.PreferredBackBufferHeight != displayHeight)
            {
                graphicManager.PreferredBackBufferWidth = displayWidth;
                graphicManager.PreferredBackBufferHeight = displayHeight;
                graphicManager.ApplyChanges();
            }

        }

        private Texture2D _colorPalette;
        private readonly int paletteWidth = 256;
        public void RebuildColorPalette(ColorChip colorChip)
        {

            var colors = ColorUtils.ConvertColors(colorChip.hexColors, colorChip.maskColor, colorChip.debugMode,
                colorChip.backgroundColor);

            var width = paletteWidth;
            var height = (int)Math.Ceiling(colors.Length / (double)width);

            _colorPalette = new Texture2D(graphicManager.GraphicsDevice, width, height);

            var fullPalette = new Color[_colorPalette.Width];
            for (int i = 0; i < fullPalette.Length; i++) { fullPalette[i] = i < colors.Length ? colors[i] : colors[0]; }

            _colorPalette.SetData(colors);

            colorChip.ResetValidation();

            // Set palette total
            // crtShader.Parameters["maskColor"].SetValue(Color.Magenta.ToVector4());

        }

        public void Render(int[] pixels)
        {

            renderTexture.SetData(pixels);
            spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp);
            crtShader.CurrentTechnique.Passes[0].Apply();
            graphicManager.GraphicsDevice.Textures[1] = _colorPalette;
            graphicManager.GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
            spriteBatch.Draw(renderTexture, offset, visibleRect, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
            spriteBatch.End();

        }

    }
}