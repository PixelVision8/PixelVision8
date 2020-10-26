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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelVision8.Engine;
using PixelVision8.Engine.Utils;

/// <summary>
///     Wrapper for texture data that includes Hex color data to rebuild colors when
///     exporting and cutting up sprites. 
/// </summary>
public struct Image
{

    private PixelData _pixelData;
    public PixelData PixelData => _pixelData;
    public int Width => _pixelData.Width;
    public int Height => _pixelData.Height;
    public int Columns => Width / _spriteSize.X;
    public int Rows => Height / _spriteSize.Y;
    
    public string[] Colors;
    
    public int TotalSprites => Columns * Rows;
    
    private Point _spriteSize;
    private List<int> _colorIDs;
    private int _colorID;
    private Point _pos;
    private int[] _tmpPixelData;

    public int SpriteWidth
    {
        get => _spriteSize.X;
        set => _spriteSize.X = value; // TODO this should be divisible by 8
    }

    public int SpriteHeight
    {
        get => _spriteSize.Y;
        set => _spriteSize.Y = value; // TODO this should be divisible by 8
    }

    public Image(int width, int height, int[] pixels = null, string[] colors = null)// : base(width, height)
    {
        _pixelData = new PixelData(width, height);

        if (pixels != null)
        {
            PixelDataUtil.SetPixels(pixels, _pixelData);
        }
        
        Colors = colors;
        
        _spriteSize = new Point(8, 8);
        _colorIDs = new List<int>();
        _colorID = 0;
        _pos = Point.Zero;
        _tmpPixelData = null;
        
    }

    /// <summary>
    ///     Get a single sprite from the Image.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cps">Total number of colors supported by the sprite.</param>
    /// <returns></returns>
    public int[] GetSpriteData(int id, int? cps = null)
    {
        _pos = MathUtil.CalculatePosition(id, Columns);

        // _pixelData = GetPixels(_pos.X * 8, _pos.Y * 8, _spriteSize.X, _spriteSize.Y);
        _tmpPixelData = PixelDataUtil.GetPixels(_pixelData, _pos.X * 8, _pos.Y * 8, _spriteSize.X, _spriteSize.Y);
        // If there is a CPS cap, we need to go through all the pixels and make sure they are in range.
        if (cps.HasValue)
        {

            _colorIDs.Clear();

            for (int i = 0; i < _tmpPixelData.Length; i++)
            {
                _colorID = _tmpPixelData[i];

                if (_colorID > -1 && _colorIDs.IndexOf(_colorID) == -1)
                {
                    if (_colorIDs.Count < cps.Value)
                    {
                        _colorIDs.Add(_colorID);
                    }
                    else
                    {
                        _tmpPixelData[i] = -1;
                    }
                }
            }
        }

        // Return the new sprite image
        return _tmpPixelData;
    }
    
    public void WriteSpriteData(int id, int[] pixels)
    {
        
        // The total sprite pixel size should be cached
        if(pixels.Length != _spriteSize.X * _spriteSize.Y)
            return;
        
        // Make sure we stay in bounds
        id = MathHelper.Clamp(id, 0, TotalSprites - 1);

        var pos = MathUtil.CalculatePosition(id, Columns);
        pos.X *= _spriteSize.X;
        pos.Y *= _spriteSize.Y;
        
        PixelDataUtil.SetPixels(PixelData, pos.X, pos.Y, _spriteSize.X, _spriteSize.Y, pixels);
        
    }

    public void SetPixels(int[] pixels) => PixelDataUtil.SetPixels(pixels, _pixelData);
    
    public void SetPixels(int x, int y, int blockWidth, int blockHeight, int[] pixels) => PixelDataUtil.SetPixels(PixelData, x, y, blockWidth, blockHeight, pixels);

    public int[] GetPixels() => PixelDataUtil.GetPixels(PixelData);
    public int[] GetPixels(int x, int y, int blockWidth, int blockHeight) => PixelDataUtil.GetPixels(PixelData, x, y, blockWidth, blockHeight);
    
    public void Resize(int newWidth, int newHeight) => PixelDataUtil.Resize(_pixelData, newWidth, newHeight);

    public void Clear(int color = -1) => PixelDataUtil.Clear(PixelData, color);
    

}