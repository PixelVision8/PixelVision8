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

using PixelVisionSDK;

namespace PixelVisionRunner
{
    public interface ITexture2D
    {
        int width { get; }
        int height { get; }
        IColor[] GetPixels();
        IColor[] GetPixels(int x, int y, int blockWidth, int blockHeight);
        IColor GetPixel(int x, int y);
//        IColor32[] GetPixels32();
        void Resize(int width, int height);
        void SetPixels(int x, int y, int width, int height, IColor[] pixelData);
        void SetPixels(IColor[] colorData);
        void LoadImage(byte[] data);
        byte[] EncodeToPNG();
        void LoadTextureData(TextureData textureData, ColorData[] colors, string transColor = "#ff00ff");
        void Apply();
//        void FlipTexture();
    }
}
