using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelVision8.Runner
{
    public partial class DisplayTarget
    {
        private bool _useCRT;
        private Effect crtShader;
        private Texture2D _colorPalette;
        private readonly int paletteWidth = 256;
        public bool CropScreen { get; set; } = true;

        public bool useCRT
        {
            get { return _useCRT; }
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
                    crtShader = new Effect(_graphicManager.GraphicsDevice,
                        reader.ReadBytes((int) reader.BaseStream.Length));
                }

                useCRT = true;
            }
        }

        public void RebuildColorPalette(string[] hexColors, int bgColorId = 0, string maskColor = "#FF00FF", bool debugMode = false)
        {
            CachedColors = ConvertColors(
                hexColors, 
                maskColor, 
                debugMode,
                bgColorId
            );

            _colorPalette = new Texture2D(_graphicManager.GraphicsDevice, paletteWidth,
                (int) Math.Ceiling(CachedColors.Length / (double) paletteWidth));

            // We need at least 256 colors for the shader to work so pad the array
            if (CachedColors.Length < 256)
            {
                Array.Resize(ref CachedColors, 256);
            }

            _colorPalette.SetData(CachedColors);
            
        }
        
        private void UpdateShader()
        {
            crtShader?.Parameters["textureSize"].SetValue(new Vector2(RenderTexture.Width, RenderTexture.Height));
            crtShader?.Parameters["videoSize"].SetValue(new Vector2(RenderTexture.Width, RenderTexture.Height));
        }

        private void CalculateCrop()
        {
            if (CropScreen && !Fullscreen)
            {
                DisplayWidth = Math.Min(DisplayWidth, (int) (GameWidth * Scale.X));
                DisplayHeight = Math.Min(DisplayHeight, (int) (GameHeight * Scale.Y));
                OffsetX = 0;
                OffsetY = 0;
            }
        }

        public void Render(int[] pixels)
        {
            
            if (Invalid)
            {
                CalculateResolution();

                CalculateDisplayScale();

                CalculateDisplayOffset();
                
                CalculateCrop();

                Apply();
                
                UpdateShader();

                ResetValidation();
            }
            
            RenderTexture.SetData(pixels);
            SpriteBatch.Begin();
            crtShader.CurrentTechnique.Passes[0].Apply();
            _graphicManager.GraphicsDevice.Textures[1] = _colorPalette;
            _graphicManager.GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
            SpriteBatch.Draw(RenderTexture, Offset, VisibleRect, Color.White, Vector2.Zero, Scale);
            SpriteBatch.End();
            
        }
    }
}