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

namespace PixelVision8.Runner.Data
{
    public class DisplayTarget : IDisplayTarget
    {
        private readonly int _monitorHeight = 640;
        private readonly int _monitorWidth = 640;
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
        private Effect shaderEffect;
        public bool stretchScreen;
        private int totalPixels;
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
                if (crtShader == null || shaderEffect == null) return false;

                return _useCRT;
            }
            set
            {
                if (crtShader == null) return;

                _useCRT = value;

                shaderEffect = _useCRT ? crtShader : null;
            }
        }

        public float brightness
        {
            get => shaderEffect?.Parameters["brightboost"]?.GetValueSingle() ?? 0;
            set => shaderEffect?.Parameters["brightboost"]?.SetValue(MathHelper.Clamp(value, .5f, 1.5f));
        }

        public float sharpness
        {
            get => shaderEffect?.Parameters["hardPix"]?.GetValueSingle() ?? 0;
            set => shaderEffect?.Parameters["hardPix"]?.SetValue(value);
        }

        public bool HasShader()
        {
            return crtShader != null;
        }

        public Stream shaderPath
        {
            set
            {
                //                Effect tmpEffect;

                using (var reader = new BinaryReader(value))
                {
                    crtShader = new Effect(graphicManager.GraphicsDevice,
                        reader.ReadBytes((int) reader.BaseStream.Length));
                }

                // Sharpness
                crtShader.Parameters["hardPix"]?.SetValue(-10.0f); // -3.0f (4 - 6)

                // Brightness
                crtShader.Parameters["brightboost"]?.SetValue(1f); // 1.0f (.5 - 1.5)


                crtShader.Parameters["hardScan"]?.SetValue(-6.0f); // -8.0f
                crtShader.Parameters["warpX"]?.SetValue(0.008f); // 0.031f
                crtShader.Parameters["warpY"]?.SetValue(0.01f); // 0.041f
                crtShader.Parameters["shape"]?.SetValue(2f); // 2.0f



                // crtShader.Parameters["hardScan"]?.SetValue(-8.0f);
                // crtShader.Parameters["hardPix"]?.SetValue(-3.0f);
                // crtShader.Parameters["warpX"]?.SetValue(0.031f);
                // crtShader.Parameters["warpY"]?.SetValue(0.041f);
                // crtShader.Parameters["maskDark"]?.SetValue(0.5f);
                // crtShader.Parameters["maskLight"]?.SetValue(1.5f);
                // crtShader.Parameters["scaleInLinearGamma"]?.SetValue(1.0f);
                // crtShader.Parameters["shadowMask"]?.SetValue(3.0f);
                // crtShader.Parameters["brightboost"]?.SetValue(1.0f);
                // crtShader.Parameters["hardBloomScan"]?.SetValue(-1.5f);
                // crtShader.Parameters["hardBloomPix"]?.SetValue(-2.0f);
                // crtShader.Parameters["bloomAmount"]?.SetValue(0.15f);
                // crtShader.Parameters["shape"]?.SetValue(2.0f);

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

                shaderEffect?.Parameters["textureSize"].SetValue(new Vector2(gameWidth, gameHeight));
                shaderEffect?.Parameters["videoSize"].SetValue(new Vector2(gameWidth, gameHeight));
                shaderEffect?.Parameters["outputSize"].SetValue(new Vector2(graphicManager.PreferredBackBufferWidth,
                    graphicManager.PreferredBackBufferHeight));

                // Set the new number of pixels
                totalPixels = renderTexture.Width * renderTexture.Height;
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

            //            Console.WriteLine("Reset Res Fullscreen " + fullscreen + " "+displayWidth+"x"+displayHeight);


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

        public void Render(Color[] pixels)
        {
            // TODO didn't have to check length before service refactoring
            if (totalPixels != pixels.Length) return;

            renderTexture.SetData(pixels);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp, effect: useCRT ? shaderEffect : null);

            spriteBatch.Draw(renderTexture, offset, visibleRect, Color.White, 0f, Vector2.Zero, scale,
                SpriteEffects.None, 1f);

            spriteBatch.End();
        }

        //        public void CaptureScreenshot()
        //        {
        //            var gd = graphicManager.GraphicsDevice;
        //            
        //            Color[] colors = new Color[gd.Viewport.Width * gd.Viewport.Height];
        //
        //            gd.GetBackBufferData<Color>(colors);
        //            
        //            
        //            
        //        }
    }
}