// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using MonoGame.OpenGL;
using ExtTextureFilterAnisotropic = MonoGame.OpenGL.TextureParameterName;

namespace Microsoft.Xna.Framework.Graphics
{
  public partial class SamplerState
  {
      private readonly float[] _openGLBorderColor = new float[4];

        internal const TextureParameterName TextureParameterNameTextureMaxAnisotropy = (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt;
        internal const TextureParameterName TextureParameterNameTextureMaxLevel = TextureParameterName.TextureMaxLevel;

        internal void Activate(GraphicsDevice device, TextureTarget target, bool useMipmaps = false)
        {
            if (GraphicsDevice == null)
            {
                // We're now bound to a device... no one should
                // be changing the state of this object now!
                GraphicsDevice = device;
            }
            Debug.Assert(GraphicsDevice == device, "The state was created for a different device!");

				if (GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                {
                    GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                    GraphicsExtensions.CheckGLError();
                }
        GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest));
                GraphicsExtensions.CheckGLError();
        GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GraphicsExtensions.CheckGLError();
        
      // Set up texture addressing.
      GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)GetWrapMode(AddressU));
            GraphicsExtensions.CheckGLError();
            GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)GetWrapMode(AddressV));
            GraphicsExtensions.CheckGLError();
#if !GLES
            // Border color is not supported by glTexParameter in OpenGL ES 2.0
            _openGLBorderColor[0] = BorderColor.R / 255.0f;
            _openGLBorderColor[1] = BorderColor.G / 255.0f;
            _openGLBorderColor[2] = BorderColor.B / 255.0f;
            _openGLBorderColor[3] = BorderColor.A / 255.0f;
            GL.TexParameter(target, TextureParameterName.TextureBorderColor, _openGLBorderColor);
            GraphicsExtensions.CheckGLError();
            // LOD bias is not supported by glTexParameter in OpenGL ES 2.0
            GL.TexParameter(target, TextureParameterName.TextureLodBias, MipMapLevelOfDetailBias);
            GraphicsExtensions.CheckGLError();
            // Comparison samplers are not supported in OpenGL ES 2.0 (without an extension, anyway)
            switch (FilterMode)
            {
                case TextureFilterMode.Comparison:
                    GL.TexParameter(target, TextureParameterName.TextureCompareMode, (int) TextureCompareMode.CompareRefToTexture);
                    GraphicsExtensions.CheckGLError();
                    GL.TexParameter(target, TextureParameterName.TextureCompareFunc, (int) ComparisonFunction.GetDepthFunction());
                    GraphicsExtensions.CheckGLError();
                    break;
                case TextureFilterMode.Default:
                    GL.TexParameter(target, TextureParameterName.TextureCompareMode, (int) TextureCompareMode.None);
                    GraphicsExtensions.CheckGLError();
                    break;
                default:
                    throw new InvalidOperationException("Invalid filter mode!");
            }
#endif
            if (GraphicsDevice.GraphicsCapabilities.SupportsTextureMaxLevel)
            {
                if (this.MaxMipLevel > 0)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterNameTextureMaxLevel, this.MaxMipLevel);
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterNameTextureMaxLevel, 1000);
                }
                GraphicsExtensions.CheckGLError();
            }
        }

    private int GetWrapMode(TextureAddressMode textureAddressMode)
    {
      switch(textureAddressMode)
      {
      case TextureAddressMode.Clamp:
        return (int)TextureWrapMode.ClampToEdge;
      default:
        throw new ArgumentException("No support for " + textureAddressMode);
      }
    }
  }
}

