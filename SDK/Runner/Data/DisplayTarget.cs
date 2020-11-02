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
using PixelVision8.Engine.Chips;
using System;
using System.IO;

namespace PixelVision8.Runner.Data
{
    public class DisplayTarget : DisplayTargetLite
    {
        private bool _useCRT;
        private Effect crtShader;
        private Texture2D _colorPalette;
        private readonly int paletteWidth = 256;

        public DisplayTarget(GraphicsDeviceManager graphicManager, int width, int height) : base(graphicManager, width, height)
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

        public override void ResetResolution(int gameWidth, int gameHeight, int overScanX = 0, int overScanY = 0)
        {
            if (renderTexture == null || renderTexture.Width != gameWidth || renderTexture.Height != gameHeight)
            {
                renderTexture = new Texture2D(GraphicManager.GraphicsDevice, gameWidth, gameHeight);

                crtShader?.Parameters["textureSize"].SetValue(new Vector2(gameWidth, gameHeight));
                crtShader?.Parameters["videoSize"].SetValue(new Vector2(gameWidth, gameHeight));

            }

            base.ResetResolution(gameWidth, gameHeight, overScanX, overScanY);

        }


        public override void RebuildColorPalette(ColorChip colorChip)
        {

            base.RebuildColorPalette(colorChip);

            // TODO do we need to recreate these two things each time?
            _colorPalette = new Texture2D(GraphicManager.GraphicsDevice, paletteWidth, (int)Math.Ceiling(CachedColors.Length / (double)paletteWidth));
            _colorPalette.SetData(CachedColors);

            colorChip.ResetValidation();

        }

        public override void Render(int[] pixels, int backgroundColor)
        {

            if (crtShader == null)
            {
                base.Render(pixels, backgroundColor);
            }
            else
            {
                renderTexture.SetData(pixels);
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