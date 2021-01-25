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
using System.IO;
using PixelVision8.Player;

namespace PixelVision8.Runner.Data
{
    public class ShaderDisplayTarget : DisplayTarget
    {
        private bool _useCRT;
        private Effect crtShader;
        private Texture2D _colorPalette;
        private readonly int paletteWidth = 256;
        public bool StretchScreen { get; set; }
        public bool CropScreen { get; set; } = true;
        
        public ShaderDisplayTarget(GraphicsDeviceManager graphicManager, int width, int height) : base(graphicManager, width, height)
        {
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
                    crtShader = new Effect(GraphicManager.GraphicsDevice,
                        reader.ReadBytes((int)reader.BaseStream.Length));
                }

                useCRT = true;
            }
        }
        
        protected override void CalculateDisplayOffset()
        {
            base.CalculateDisplayOffset();

            if (CropScreen && !Fullscreen)
            {
                displayWidth = Math.Min(displayWidth, (int) (VisibleRect.Width * Scale.X));
                displayHeight = Math.Min(displayHeight, (int) (VisibleRect.Height * Scale.Y));
                offset.X = 0;
                offset.Y = 0;
            }
        }
        
        protected override void CalculateDisplayScale()
        {
            base.CalculateDisplayScale();

            if (!StretchScreen)
            {
                // To preserve the aspect ratio,
                // use the smaller scale factor.
                _scale.X = Math.Min(Scale.X, Scale.Y);
                _scale.Y = Scale.X;
            }
        }

        public override void ResetResolution(IPlayerChips engine)
        {
            
            

            base.ResetResolution(engine);
            
            // if (renderTexture == null || renderTexture.Width != gameWidth || renderTexture.Height != gameHeight)
            // {
            //     renderTexture = new Texture2D(GraphicManager.GraphicsDevice, gameWidth, gameHeight);

            crtShader?.Parameters["textureSize"].SetValue(new Vector2(renderTexture.Width, renderTexture.Height));
            crtShader?.Parameters["videoSize"].SetValue(new Vector2(renderTexture.Width, renderTexture.Height));

            // }

        }


        public override void RebuildColorPalette(ColorChip colorChip)
        {

            var invalid = colorChip.invalid;
            
            base.RebuildColorPalette(colorChip);

            if (invalid)
            {
                _colorPalette = new Texture2D(GraphicManager.GraphicsDevice, paletteWidth, (int)Math.Ceiling(CachedColors.Length / (double)paletteWidth));

                // We need at least 256 colors for the shader to work so pad the array
                if (CachedColors.Length < 256)
                {
                    Array.Resize(ref CachedColors, 256);
                }
                
                _colorPalette.SetData(CachedColors);
            }
            
        }

        public override void Render(IPlayerChips engine)//int[] pixels, int backgroundColor)
        {

            if (crtShader == null)
            {
                base.Render(engine);
            }
            else
            {

                RebuildColorPalette(engine.ColorChip);
                
                renderTexture.SetData(engine.DisplayChip.Pixels);
                SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp);
                crtShader.CurrentTechnique.Passes[0].Apply();
                GraphicManager.GraphicsDevice.Textures[1] = _colorPalette;
                GraphicManager.GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
                SpriteBatch.Draw(renderTexture, offset, VisibleRect, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 1f);
                SpriteBatch.End();
            }

        }

    }
}