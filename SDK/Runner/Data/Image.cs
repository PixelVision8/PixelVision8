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
    
    private PixelData pixelData;
    public int Width => pixelData.Width;

    public int Height => pixelData.Height;
    public int Columns => Width / _spriteSize.X;
    public int Rows => Height / _spriteSize.Y;
    
    public string[] Colors;
    
    public int TotalSprites => Columns * Rows;
    
    private Point _spriteSize;
    private List<int> _colorIDs;
    private int _colorID;
    private Point _pos;
    private int[] _tmpPixelData;

    public Image(int width, int height, string[] colors, int[] pixels = null, Point? spriteSize = null)// : base(width, height)
    {
        pixelData = new PixelData(256, 256);
            
        this.Colors = colors;
        _spriteSize = spriteSize ?? new Point(8, 8);

        _colorIDs = new List<int>();
        _colorID = 0;
        _pos = Point.Zero;
        _tmpPixelData = null;
        
        Resize(width, height);
        
        if (pixels != null)
        {
            PixelDataUtil.SetPixels(pixels, pixelData);
        }
       
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
        _tmpPixelData = PixelDataUtil.GetPixels(pixelData, _pos.X * 8, _pos.Y * 8, _spriteSize.X, _spriteSize.Y);
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
    
    public void SetPixels(int[] newPixelData) => PixelDataUtil.SetPixels(newPixelData, pixelData);

    public int[] GetPixels() => PixelDataUtil.GetPixels(pixelData);
    
    public void Resize(int newWidth, int newHeight) => PixelDataUtil.Resize(ref pixelData, newWidth, newHeight);
    

}