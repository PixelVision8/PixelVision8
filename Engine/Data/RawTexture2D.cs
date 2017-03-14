//
// Copyright (c) Jesse Freeman. All rights reserved.
//
// Licensed under the Microsoft Public License (MS-PL) License.
// See LICENSE file in the project root for full license information.
//
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//
// Jesse Freeman - @JesseFreeman
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany
//

using PixelVisionSDK;

namespace Assets.PixelVisionSDK.Engine.Data
{
    /// <summary>
    ///     The RawTexture2D class represents a TextureData and a color index and allows you to
    ///     work with external images in a native Pixel Vision 8 format.
    /// </summary>
    public class RawTexture2D
    {
        protected string[] _colorIndex;

        protected ColorData[] colors;

        protected TextureData texturedata;

        public RawTexture2D(int width, int height, string[] colorIndex)
        {
            texturedata = new TextureData(width, height);
            _colorIndex = colorIndex;
        }

        public int width
        {
            get { return texturedata.width; }
        }

        public int height
        {
            get { return texturedata.height; }
        }

        public int totalColors
        {
            get { return colors.Length; }
        }

        public string[] colorIndex
        {
            get { return _colorIndex; }
        }

        public void SetPixels(int[] pixels)
        {
            texturedata.SetPixels(pixels);
        }

        public int[] GetPixels()
        {
            return texturedata.GetPixels();
        }

        public void GetPixels(int x, int y, int width, int height, int[] data)
        {
            texturedata.GetPixels(x, y, width, height, data);
        }
    }
}