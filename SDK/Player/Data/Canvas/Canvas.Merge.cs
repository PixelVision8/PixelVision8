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

namespace PixelVision8.Player
{
    public sealed partial class Canvas
    {
        private readonly PixelData<int> _tmpPixelData = new PixelData<int>();

        /// <summary>
        ///     Allows you to merge the pixel data of another canvas into this one without compleatly overwritting it.
        /// </summary>
        /// <param name="canvas"></param>
        public void MergeCanvas(Canvas canvas, int offsetX = 0, int offsetY = 0, int colorOffset = 0, int maskId = Constants.EmptyPixel) => MergePixels(offsetX, offsetY,
            canvas.Width, canvas.Height, canvas.GetPixels(), false, false, colorOffset, maskId);


        public void MergePixels(int x, int y, int blockWidth, int blockHeight, int[] pixels,
            bool flipH = false, bool flipV = false, int colorOffset = 0, int maskId = Constants.EmptyPixel)
        {
            // Flatten the canvas
            Draw();

            _tmpPixelData.SetPixels(pixels, blockWidth, blockHeight);

            Utilities.MergePixels(_tmpPixelData, 0, 0, blockWidth, blockHeight, defaultLayer, x, y, flipH, flipV, colorOffset, maskId);
        }
    }
}