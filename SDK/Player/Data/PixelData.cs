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

namespace PixelVision8.Player
{
    public interface IPixelData<T> where T : IComparable
    {
        T this[in int i] { get; set; }

        public T[] Pixels { get; }
        
        public int Width { get; }
        public int Height { get; }

        public void Resize(int width, int height);

        int Total { get; }
    }

    public class PixelData<T> : IPixelData<T> where T : IComparable
    {
        private T[] _pixels = new T[1];

        public PixelData(int width = 1, int height = 1, T[] pixels = null)
        {

            Resize(width, height);

            if (pixels != null && pixels.Length == _pixels.Length) Array.Copy(pixels, _pixels, _pixels.Length);
        }

        public T[] Pixels => _pixels;

        public int Width { get; private set; } = 1;
        public int Height { get; private set; } = 1;
        public int Total { get; private set; } = 1;

        public T this[in int i]
        {
            get => _pixels[i];
            set => _pixels[i] = value;
        }

        public void Resize(int width, int height)
        {
            Width = Math.Max(width, 1);

            Height = Math.Max(height, 1);

            Total = Width * Height;

            _pixels = new T[Total];

            // Array.Resize(ref _pixels, Total);
        }

        public void SetPixels(T[] pixels, int? width = null, int? height = null)
        {
            // Create a new width and height by reading the new dimensions and making sure they are greater than 0
            var newWidth = Math.Max(width ?? Width, 1);
            var newHeight = Math.Max(height ?? Height, 1);

            if (newWidth * newHeight != pixels.Length)
                return;

            Total = pixels.Length;

            if (_pixels.Length != Total)
                Array.Resize(ref _pixels, Total);

            Array.Copy(pixels, _pixels, Total);

            Width = newWidth;
            Height = newHeight;
        }

        public override string ToString()
        {
            return "{Width:" + Width + " Height:" + Height + " Total Pixels:" + Total + "}";
        }
    }
}