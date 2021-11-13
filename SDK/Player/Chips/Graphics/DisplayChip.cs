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

namespace PixelVision8.Player
{

    #region Display Chip Class

    public class DisplayChip : AbstractChip, IDraw
    {
        private readonly PixelData<int> _display = new PixelData<int>();
        private readonly List<DrawRequest> _drawCalls = new List<DrawRequest>();
        // private int _clearColor = -1;
        private bool _clearFlag = true;
        // private int[] _clearPixels = new int[0];
        private DrawRequest _drawRequest;
        private int _drawRequestCounter = -1;
        private DrawRequest[] _drawRequestPool = new DrawRequest[0];

        // This should be part of the chip's data
        private int _maxDrawRequests = 1024;
        private bool _nextDrawRequest;
        public int[] Pixels => _display.Pixels;

        public int MaxDrawRequests
        {
            set
            {
                if (value == 0)
                    return;

                _maxDrawRequests = value;
                if (_drawRequestPool.Length != _maxDrawRequests)
                {
                    Array.Resize(ref _drawRequestPool, _maxDrawRequests);
                    _drawCalls.Capacity = _maxDrawRequests;
                }
            }
        }

        /// <summary>
        ///     Returns the display's <see cref="Width" />
        /// </summary>
        public int Width => _display.Width;

        /// <summary>
        ///     Returns the display's <see cref="Height" />
        /// </summary>
        public int Height => _display.Height;

        /// <summary>
        /// </summary>
        public void Draw()
        {
            var drawCallCounter = _drawCalls.Count;

            if (_clearFlag)
            {
                Array.Clear(Pixels, 0, _display.Total);

                // Reset the clear flag for the next frame
                _clearFlag = false;
            }

            // Sort sprite draw calls
            _drawCalls.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            for (var i = 0; i < drawCallCounter; i++)
            {
                _drawRequest = _drawCalls[i];

                Utilities.MergePixels(
                    _drawRequest.PixelData, 
                    _drawRequest.SampleRect.X, 
                    _drawRequest.SampleRect.Y,
                    _drawRequest.SampleRect.Width, 
                    _drawRequest.SampleRect.Height, 
                    _display, 
                    _drawRequest.X,
                    _drawRequest.Y, 
                    _drawRequest.FlipH, 
                    _drawRequest.FlipV, 
                    _drawRequest.ColorOffset,
                    _drawRequest.MaskId
                );
                
            }

            // Reset Draw Requests after they have been processed
            ResetDrawCalls();
        }

        public void Clear() => _clearFlag = true;

        public void NewDrawCall(int[] pixels, int destX, int destY, int blockWidth, int blockHeight, byte layer = 0,
            bool flipH = false,
            bool flipV = false, int colorOffset = 0, int srcX = 0, int srcY = 0, int maskId = 0)
        {
            NextDrawRequest(destX, destY, layer, flipH, flipV, colorOffset, maskId);

            if (_nextDrawRequest)
            {
                _drawRequest.SampleFrom(pixels, srcX, srcY, blockWidth, blockHeight);
                _drawCalls.Add(_drawRequest);
            }
        }


        /// <summary>
        ///     Creates a new draw by copying the supplied pixel data over
        ///     to the Display's TextureData.
        /// </summary>
        /// <param name="pixelData"></param>
        /// <param name="destX"></param>
        /// <param name="destY"></param>
        /// <param name="blockWidth"></param>
        /// <param name="blockHeight"></param>
        /// <param name="layer"></param>
        /// <param name="flipH">
        ///     This is an optional argument which accepts a bool. The default value is set to false but passing in true flips
        ///     the pixel data horizontally.
        /// </param>
        /// <param name="flipV">
        ///     This is an optional argument which accepts a bool. The default value is set to false but passing in true flips
        ///     the pixel data vertically.
        /// </param>
        /// <param name="colorOffset"></param>
        /// <param name="layerOrder"></param>
        public void NewDrawCall(IDisplay src, int destX, int destY, int blockWidth, int blockHeight, byte layer = 0,
            bool flipH = false,
            bool flipV = false, int colorOffset = 0, int srcX = 0, int srcY = 0, int maskId = 0)
        {
            NextDrawRequest(destX, destY, layer, flipH, flipV, colorOffset, maskId);

            if (_nextDrawRequest)
            {
                // _drawRequest = request.Value;
                _drawRequest.SampleFrom(src, srcX, srcY, blockWidth, blockHeight);
                _drawCalls.Add(_drawRequest);
            }
        }

        /// <summary>
        ///     Changes the resolution of the display.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void ResetResolution(int width, int height)
        {
            _display.Resize(width, height);

            // Force the screen to clear after a resolution reset   
            _clearFlag = true;
        }

        /// <summary>
        ///     This configures the DisplayChip. It registers itself as the default
        ///     <see cref="DisplayChip" /> for the engine, gets a reference to the
        ///     engine's renderTarget, sets <see cref="autoClear" /> and
        ///     <see cref="wrapMode" /> to true and
        ///     finally resets the resolution to its default value
        ///     of 256 x 240.
        /// </summary>
        protected override void Configure()
        {
            //Debug.Log("Pixel Data Renderer: Configure ");
            Player.DisplayChip = this;

            ResetResolution(256, 240);

            MaxDrawRequests = 1024;
        }

        public void ResetDrawCalls()
        {
            _drawRequestCounter = -1;
            _drawCalls.Clear();
        }

        public void NextDrawRequest(int destX, int destY, byte layer = 0, bool flipH = false, bool flipV = false,
            int colorOffset = 0, int maskId = -1)
        {
            _drawRequestCounter++;

            _nextDrawRequest = _drawRequestCounter < _maxDrawRequests;

            if (_nextDrawRequest == false)
                return;

            _drawRequest = _drawRequestPool[_drawRequestCounter];

            _drawRequest.X = destX;
            _drawRequest.Y = destY;
            _drawRequest.Priority = layer;
            _drawRequest.FlipH = flipH;
            _drawRequest.FlipV = flipV;
            _drawRequest.ColorOffset = colorOffset;
            _drawRequest.MaskId = maskId;
        }
    }
    
    #endregion
    
    #region Modify PixelVision
    public partial class PixelVision
    {
        public DisplayChip DisplayChip { get; set; }
    }
    
    #endregion
}