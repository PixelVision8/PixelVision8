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
using PixelVision8.Engine;
using PixelVision8.Engine.Utils;

namespace PixelVision8.Runner.Data
{
    public class DisplayTarget : IDisplayTarget
    {
        private readonly int _monitorHeight = 640;

//        private readonly GraphicsDeviceManager _graphicsDeviceManager;

        private readonly int _monitorWidth = 640;
        private int _totalPixels;

        private bool _useCRT;
        private Effect crtShader;

        private readonly GraphicsDeviceManager graphicManager;
        public Vector2 offset;
        private Texture2D renderTexture;

//        private int _monitorScale = 1;
//        
//        public int monitorScale
//        {
//            get { return _monitorScale; }
//            set { _monitorScale = value.Clamp(1, 6); }
//        }

        public Vector2 scale = new Vector2(1, 1);

        private Effect shaderEffect;

        private readonly SpriteBatch spriteBatch;
//        

        private int totalPixels;
//        {
//            get { return _totalPixels; }
//
//            set
//            {
//                if (cachedPixels.Length != value)
//                {
//                    // Now it's time to resize our cahcedPixels array. We calculate the total number of pixels by multiplying the width by the 
//                    // height. We'll use this array to make sure we have enough pixels to correctly render the DisplayChip's own pixel data.
//                    Array.Resize(ref cachedPixels, value);
//                }
//
//                _totalPixels = value;
//
//            }
//        }

//        private int bgColorID;
//        private int[] pixelData;
//        private int colorRef;
//        private Color[] colorsData;
//        private int i;
//        private Color colorData;
//        public Color[] cachedColors = new Color[0];
//        protected Color[] cachedPixels = new Color[0];
//        protected int totalCachedColors;
        private Rectangle visibleRect;

        // TODO think we just need to pass in the active game and not the entire runner?
        public DisplayTarget(GraphicsDeviceManager graphicManager, int width, int height, bool fullscreen = false)
        {
            this.graphicManager = graphicManager;

            this.graphicManager.HardwareModeSwitch = false;

            spriteBatch = new SpriteBatch(graphicManager.GraphicsDevice);

            _monitorWidth = MathHelper.Clamp(width, 64, 640);
            _monitorHeight = MathHelper.Clamp(height, 64, 480);
//            monitorScale = scale;
        }

        public bool useCRT
        {
            get
            {
                if (crtShader == null)
                    return false;

                return _useCRT;
            }
            set
            {
                if (crtShader == null)
                    return;

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

        public string shaderPath
        {
            set
            {
//                Effect tmpEffect;

                using (var reader = new BinaryReader(File.Open(value, FileMode.Open)))
                {
                    crtShader = new Effect(graphicManager.GraphicsDevice,
                        reader.ReadBytes((int) reader.BaseStream.Length));
                }

                // Sharpness
                crtShader.Parameters["hardPix"]?.SetValue(-10.0f); // -3.0f (4 - 6)

                // Brightness
                crtShader.Parameters["brightboost"]?.SetValue(1f); // 1.0f (.5 - 1.5)


                crtShader.Parameters["hardScan"]?.SetValue(-4.0f); // -8.0f
                crtShader.Parameters["warpX"]?.SetValue(0.008f); // 0.031f
                crtShader.Parameters["warpY"]?.SetValue(0.01f); // 0.041f
                crtShader.Parameters["shape"]?.SetValue(2f); // 2.0f

                useCRT = true;
                // Monitor brightness
//                shaderEffect.Parameters["maskDark"]?.SetValue(.5f); // 0.5f
//                
//                
//                shaderEffect.Parameters["maskLight"]?.SetValue(1.5f); // 1.5f ?
//                
//                shaderEffect.Parameters["scaleInLinearGamma"]?.SetValue(1.0f); // 1.0f
//                shaderEffect.Parameters["shadowMask"]?.SetValue(0.5f); // 3.0f ?
//                

//                shaderEffect.Parameters["hardBloomScan"]?.SetValue(-8.5f); //-1.5f
//                shaderEffect.Parameters["hardBloomPix"]?.SetValue(-8f); // -2.0f
//                shaderEffect.Parameters["bloomAmount"]?.SetValue(0f); // 0.15f
//                


//                shaderEffect = new Effect(graphicManager.GraphicsDevice, File.ReadAllBytes(value));
            }
        }
//        private GameWindow window;

        public void ResetResolution(IEngine activeEngine, int monitorScale, bool fullscreen, bool matchResolution,
            bool stretch)
        {
            var displayChip = activeEngine.displayChip;

            var gameWidth = displayChip.width;
            var gameHeight = displayChip.height;
            var overScanX = displayChip.overscanXPixels;
            var overScanY = displayChip.overscanYPixels;


            renderTexture?.Dispose();

            renderTexture = new Texture2D(graphicManager.GraphicsDevice, gameWidth, gameHeight);

            // Calculate the game's resolution
            visibleRect.Width = renderTexture.Width - overScanX;
            visibleRect.Height = renderTexture.Height - overScanY;

            var tmpMonitorScale = fullscreen ? 1 : MathHelper.Clamp(monitorScale, 1, 4);

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

            if (!stretch)
            {
                // To preserve the aspect ratio,
                // use the smaller scale factor.
                scale.X = Math.Min(scale.X, scale.Y);
                scale.Y = scale.X;
            }

            offset.X = (displayWidth - visibleRect.Width * scale.X) * .5f;
            offset.Y = (displayHeight - visibleRect.Height * scale.Y) * .5f;

            if (matchResolution && !fullscreen)
            {
                displayWidth = Math.Min(displayWidth, (int) (visibleRect.Width * scale.X));
                displayHeight = Math.Min(displayHeight, (int) (visibleRect.Height * scale.Y));
                offset.X = 0;
                offset.Y = 0;
            }

//            Console.WriteLine("Reset Res Fullscreen " + fullscreen + " "+displayWidth+"x"+displayHeight);

            // Apply changes
            graphicManager.IsFullScreen = fullscreen;
            graphicManager.PreferredBackBufferWidth = displayWidth;
            graphicManager.PreferredBackBufferHeight = displayHeight;
            graphicManager.ApplyChanges();


            shaderEffect?.Parameters["textureSize"].SetValue(new Vector2(gameWidth, gameHeight));
            shaderEffect?.Parameters["videoSize"].SetValue(new Vector2(gameWidth, gameHeight));
            shaderEffect?.Parameters["outputSize"].SetValue(new Vector2(graphicManager.PreferredBackBufferWidth,
                graphicManager.PreferredBackBufferHeight));

            // Update the controller to use the correct mouse scale
            activeEngine.controllerChip.MouseScale(scale.X, scale.Y);

            // Set the new number of pixels
            totalPixels = renderTexture.Width * renderTexture.Height;
        }

        public void Render(IEngine activeEngine, bool ignoreTransparent = false)
        {
            // TODO didn't have to check length before service refactoring
            if (activeEngine == null || totalPixels != activeEngine.displayChip.pixels.Length) return;

//            // The first part of rendering Pixel Vision 8's DisplayChip is to get all of the current pixel data during the current frame. Each 
//            // Integer in this Array contains an ID we can use to match up to the cached colors we created when setting up the Runner.
//            pixelData = activeEngine.displayChip.pixels; //.displayPixelData;
//
//            // Need to make sure we are using the latest colors.
//            if (activeEngine.colorChip.invalid)
//                CacheColors(activeEngine);
//
//            // Now it's time to loop through all of the DisplayChip's pixel data.
//            for (i = 0; i < totalPixels; i++)
//            {
//                // Here we get a reference to the color we are trying to look up from the pixelData array. Then we compare that ID to what we 
//                // have in the cachedPixels. If the color is out of range, we use the cachedTransparentColor. If the color exists in the cache we use that.
//                colorRef = pixelData[i];
//
//                // Replace transparent colors with bg for next pass
//                if (colorRef < 0 || (colorRef >= totalCachedColors))
//                {
//                    colorRef = ignoreTransparent ? -1 : bgColorID;
//                }
//
//                if (colorRef > -1)
//                {
//                    cachedPixels[i] = cachedColors[colorRef];
//                }
//
//                // As you can see, we are using a protected field called cachedPixels. When we call ResetResolution, we resize this array to make sure that 
//                // it matches the length of the DisplayChip's pixel data. By keeping a reference to this Array and updating each color instead of rebuilding 
//                // it, we can significantly increase the render performance of the Runner.
//            }

            renderTexture.SetData(activeEngine.displayChip.pixels);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp, effect: shaderEffect);

            spriteBatch.Draw(renderTexture, offset, visibleRect, Color.White, 0f, Vector2.Zero, scale,
                SpriteEffects.None, 1f);

            spriteBatch.End();
        }


        /// <summary>
        ///     To optimize the Runner, we need to save a reference to each color in the ColorChip as native Unity Colors. The
        ///     cached
        ///     colors will improve rendering performance later when we cover the DisplayChip's pixel data into a format the
        ///     Texture2D
        ///     can display.
        /// </summary>
//        public void CacheColors(IEngine activeEngine)
//        {
//            
//            // We also want to cache the ScreenBufferChip's background color. The background color is an ID that references one of the ColorChip's colors.
//            bgColorID = activeEngine.colorChip.backgroundColor;
//            
//            // The ColorChip can return an array of ColorData. ColorData is an internal data structure that Pixel Vision 8 uses to store 
//            // color information. It has properties for a Hex representation as well as RGB.
//            colorsData = activeEngine.colorChip.colors;
//
//            // To improve performance, we'll save a reference to the total cashed colors directly to the Runner's totalCachedColors field. 
//            // Also, we'll create a new array to store native Unity Color classes.
//            totalCachedColors = colorsData.Length;
//            
//            if (cachedColors.Length != totalCachedColors)
//                Array.Resize(ref cachedColors, totalCachedColors);
//
//            // Now it's time to loop through each of the colors and convert them from ColorData to Color instances. 
//            for (i = 0; i < totalCachedColors; i++)
//            {
//                // Converting ColorData to Unity Colors is relatively straight forward by simply passing the ColorData's RGB properties into 
//                // the Unity Color class's constructor and saving it  to the cachedColors array.
//                colorData = colorsData[i];
//                
//                // TODO is there a better way to do this without having to update all 256 colors?
//                cachedColors[i] = new Color(colorData.R, colorData.G, colorData.B);
//            }
//        }
    }
}