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
        private PixelData stroke = new PixelData();

        public void SetStroke(int color, int size = 1)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest;

            newRequest.Action = SetStrokeAction;

            if (newRequest.PixelData.Width != size || newRequest.PixelData.Height != size)
            {
                Utilities.Resize(newRequest.PixelData, size, size);
            }

            var newPixels = new int[size * size];
            for (int i = 0; i < newPixels.Length; i++)
            {
                newPixels[i] = color;
            }

            newRequest.PixelData.SetPixels(newPixels);

            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }

        [DrawAction]
        private void SetStrokeAction(CanvasDrawRequest request)
        {
            // if (stroke.Width != request.PixelData.Width || pattern.Height != request.PixelData.Height)
            //     Utilities.Resize(stroke, request.PixelData.Width, request.PixelData.Height);

            stroke.SetPixels(request.PixelData.Pixels, request.PixelData.Width, request.PixelData.Height);
        }

        private void SetStrokePixel(int x, int y) =>
            Utilities.SetPixels(stroke.Pixels, x, y, stroke.Width, stroke.Height, currentTexture);
    }
}