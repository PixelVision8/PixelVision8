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
        private readonly PixelData defaultLayer = new PixelData();
        private readonly PixelData tmpLayer = new PixelData();
        private PixelData currentTexture;

        private void ChangeTargetCanvas(int width, int height)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest;

            newRequest.Action = ChangeTargetCanvasAction;
            // newRequest.PixelData = textureData;
            newRequest.Bounds.Width = width;
            newRequest.Bounds.Height = height;

            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }

        [DrawAction]
        private void ChangeTargetCanvasAction(CanvasDrawRequest drawRequest)
        {
            currentTexture = tmpLayer;

            if (drawRequest.Bounds.Width != currentTexture.Width || drawRequest.Bounds.Height != currentTexture.Height)
                Utilities.Resize(currentTexture, drawRequest.Bounds.Width, drawRequest.Bounds.Height);
            else
                Utilities.Clear(currentTexture);
        }

        private void SaveTmpLayer(int x, int y, int blockWidth, int blockHeight)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest;

            newRequest.Action = SaveTmpLayerAction;
            newRequest.X = x;
            newRequest.Y = y;
            newRequest.Bounds.X = 0;
            newRequest.Bounds.Y = 0;
            newRequest.Bounds.Width = blockWidth;
            newRequest.Bounds.Height = blockHeight;

            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }

        // private Rectangle _tmpRect = Rectangle.Empty;
        [DrawAction]
        private void SaveTmpLayerAction(CanvasDrawRequest request)
        {
            Utilities.MergePixels(tmpLayer, request.Bounds.X, request.Bounds.Y, request.Bounds.Width,
                request.Bounds.Height, defaultLayer, request.X, request.Y);

            currentTexture = defaultLayer;
        }
    }
}