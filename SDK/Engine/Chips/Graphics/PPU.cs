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

namespace PixelVision8.Engine.Chips
{

    // http://wiki.nesdev.com/w/index.php/PPU_OAM
    public class OAMMemory
    {

    }

    // http://wiki.nesdev.com/w/index.php/PPU_attribute_tables
    public class AttributeTable
    {

    }

    // http://wiki.nesdev.com/w/index.php/PPU_nametables
    // http://wiki.nesdev.com/w/index.php/Mirroring

    public class Nametable
    {

    }


    public class PPU : AbstractChip, IDraw
    {
        public int[] Pixels = new int[0];
        public int TotalPixels;
        SpriteChip SpriteChip => engine.SpriteChip;
        private ColorChip ColorChip => engine.ColorChip;

        /// <summary>
        ///     Returns the display's <see cref="Width" />
        /// </summary>
        public int Width { get; protected set; } = 256;

        /// <summary>
        ///     Returns the display's <see cref="Height" />
        /// </summary>
        public int Height { get; protected set; } = 240;

        public List<DrawRequest> OAMEntries = new List<DrawRequest>();

        public void Draw()
        {
            // cachedColors = engine.ColorChip.colors;


            var bgColor = engine.ColorChip.backgroundColor;

            var col = 0;
            var row = 0;
            var w = Width;
            var spriteCount = 0;
            var spriteOverflowFlag = false;
            int pixel = 0;
            int palette = 0;
            int bg_pixel = 0;
            int fg_pixel = 0;
            int fg_palette = 0;
            int bg_palette = 0;
            bool fg_priority = false;

            int nOAMEntry = 0;
            int sprite_count = 0;
            bool sprite_size = false;
            var pixels = new int[64];


            var pixelID = 0;
            var pixelsPerRow = Width;
            var scanlines = Height;

            // index = x + y * width;

            for (int i = 0; i < scanlines; i++)
            {

                // http://wiki.nesdev.com/w/index.php/PPU_sprite_priority

                // Check sprite count on scan line
                while (nOAMEntry < OAMEntries.Count && sprite_count < 9)
                {
                    // Note the conversion to signed numbers here
                    var diff = (row - OAMEntries[nOAMEntry].y);

                    // If the difference is positive then the scanline is at least at the
                    // same height as the sprite, so check if it resides in the sprite vertically
                    // depending on the current "sprite height mode"
                    // FLAGGED

                    if (diff >= 0 && diff < (sprite_size ? 16 : 8) && sprite_count < 8)
                    {
                        // Sprite is visible, so copy the attribute entry over to our
                        // scanline sprite cache. Ive added < 8 here to guard the array
                        // being written to.
                        if (sprite_count < 8)
                        {
                            var entry = OAMEntries[nOAMEntry];

                            SpriteChip.ReadSpriteAt(entry.id, ref pixels);

                            fg_pixel = 1;
                            // memcpy(&spriteScanline[sprite_count], &OAM[nOAMEntry], sizeof(sObjectAttributeEntry));
                        }
                        sprite_count++;
                    }
                    nOAMEntry++;
                }

                // if (sprite_count > 0)
                // {
                //     Debug.WriteLine("Line " + row + " Count " + sprite_count);
                // }

                /// Campsite the pixel color
                if (bg_pixel == 0 && fg_pixel == 0)
                {
                    // The background pixel is transparent
                    // The foreground pixel is transparent
                    // No winner, draw "background" colour
                    pixel = bgColor;
                    palette = bgColor;
                }
                else if (bg_pixel == 0 && fg_pixel > 0)
                {
                    // The background pixel is transparent
                    // The foreground pixel is visible
                    // Foreground wins!
                    pixel = fg_pixel;
                    palette = fg_palette;
                }
                else if (bg_pixel > 0 && fg_pixel == 0)
                {
                    // The background pixel is visible
                    // The foreground pixel is transparent
                    // Background wins!
                    pixel = bg_pixel;
                    palette = bg_palette;
                }
                else if (bg_pixel > 0 && fg_pixel > 0)
                {
                    // The background pixel is visible
                    // The foreground pixel is visible
                    // Hmmm...
                    if (fg_priority)
                    {
                        // Foreground cheats its way to victory!
                        pixel = fg_pixel;
                        palette = fg_palette;
                    }
                    else
                    {
                        // Background is considered more important!
                        pixel = bg_pixel;
                        palette = bg_palette;
                    }

                }

                // Now we have a final pixel colour, and a palette for this cycle
                // of the current scanline. Let's at long last, draw that ^&%*er :P
                Pixels[i] = GetColourFromPaletteRam(palette, pixel);

                col++;

                // Reached the end of a scan line
                if (col >= w)
                {
                    sprite_count = 0;
                    nOAMEntry = 0;
                    spriteOverflowFlag = false;
                    col = 0;
                    fg_pixel = 0;
                    bg_pixel = 0;
                }




            }

            // Loop through all draw requests
            // for (_layer = 0; _layer < DrawRequestPixelDataLayers.Length; _layer++)
            // {
            //     // TODO need to add back in support for turning layers on and off
            //
            //     _drawRequests = DrawRequestPixelDataLayers[_layer];
            //     _totalDR = _drawRequests.Count;
            //     for (_i = 0; _i < _totalDR; _i++)
            //     {
            //         _drawRequest = _drawRequests[_i];
            //
            //         CopyDrawRequestPixelData(_drawRequest.isRectangle ? null : _drawRequest.pixelData, _drawRequest.x, _drawRequest.y, _drawRequest.width, _drawRequest.height,
            //             _drawRequest.flipH, _drawRequest.flipV, _drawRequest.colorOffset);
            //     }
            // }
            //
            // // Sort sprite draw calls
            // SpriteDrawRequests.Sort((x, y) => x.priority.CompareTo(y.priority));
            //
            // for (int i = 0; i < OAMEntries.Count; i++)
            // {
            //     var request = OAMEntries[i];
            //     var tmpPixelData = new int[64];
            //     SpriteChip.ReadSpriteAt(request.id, ref tmpPixelData);
            //     
            //     CopyDrawRequestPixelData(tmpPixelData, request.x, request.y, 8, 8, request.flipH, request.flipV, request.colorOffset);
            //
            //     SpriteDrawRequestPool.Push(request);
            // }

            OAMEntries.Clear();
            //
            // // Reset Draw Requests after they have been processed
            // ResetDrawCalls();

            clearFlag = false;
        }

        int GetColourFromPaletteRam(int palette, int pixel)
        {
            // TODO need to look up the palette value
            return pixel;
        }


        private int _oldSize;

        /// <summary>
        ///     Creates a new draw by copying the supplied pixel data over
        ///     to the Display's TextureData.
        /// </summary>
        /// <param name="pixelData"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
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
        // public void NewDrawCall(int[] pixelData, int x, int y, int width, int height, int layer = 0, bool flipH = false,
        //     bool flipV = false, int colorOffset = 0)
        // {
        //     
        //     // Exit if we are drawing to a layer that doesn't exist
        //     if (layer >= layers)
        //         return;
        //     // {
        //     //     // This can happen as the old system wasn't very strict.
        //     //     // TODO: Handle "out of bounds" layer accesses properly!
        //     //     _oldSize = layers;
        //     //     Array.Resize(ref DrawRequestPixelDataLayers, layer + 1);
        //     //     for (_i = layers - 1; _i >= _oldSize; _i--) DrawRequestPixelDataLayers[_i] = new List<DrawRequestPixelData>();
        //     // }
        //
        //     draw = NextDrawRequestPixelData();
        //     draw.x = x;
        //     draw.y = y;
        //     draw.width = width;
        //     draw.height = height;
        //     draw.pixelData = pixelData;
        //     draw.flipH = flipH;
        //     draw.flipV = flipV;
        //     draw.colorOffset = colorOffset;
        //     DrawRequestPixelDataLayers[layer].Add(draw);
        // }

        public List<DrawRequest> SpriteDrawRequests = new List<DrawRequest>();

        public void DrawSprite(int id, int x, int y, bool flipH, bool flipV, byte priority, int colorOffset)
        {

            // TODO Each draw request should go into the OAM memory
            // TODO should the sprite chip store the draw calls?

            var request = NextSpriteDrawRequest();

            if (request.HasValue)
            {
                var drawCall = request.Value;
                drawCall.id = id;
                drawCall.x = x;
                drawCall.y = y;
                drawCall.flipH = flipH;
                drawCall.flipV = flipV;
                drawCall.priority = priority;
                drawCall.colorOffset = colorOffset;

                SpriteDrawRequests.Add(drawCall);

                // Used by scan line draw
                // OAMEntries.Add(drawCall);
            }

        }

        /// <summary>
        ///     Changes the resolution of the display.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void ResetResolution(int width, int height)
        {
            Width = width;
            Height = height;

            TotalPixels = Width * Height;

            Array.Resize(ref Pixels, TotalPixels);
        }

        /// <summary>
        ///     This configures the DisplayChip. It registers itself as the default
        ///     <see cref="DisplayChip" /> for the engine, gets a reference to the
        ///     engine's renderTarget, sets <see cref="autoClear" /> and
        ///     <see cref="wrapMode" /> to true and
        ///     finally resets the resolution to its default value
        ///     of 256 x 240.
        /// </summary>
        public override void Configure()
        {
            //Debug.Log("Pixel Data Renderer: Configure ");
            // engine.DisplayChip = this;

            ResetResolution(256, 240);

            // By default set the total layers to the DrawModes minus Tilemap Cache which isn't used for rendering
            // layers = Enum.GetNames(typeof(DrawMode)).Length - 1;

            // TODO should the display have the sprite limit and the game chip looks there first

            SpriteDrawRequestPool = new Stack<DrawRequest>();

            // var maxCalls = SpriteChip.maxSpriteCount > 0 ? SpriteChip.maxSpriteCount : maxDrawRequests;

            // for (int i = 0; i < maxCalls; i++)
            // {
            //     SpriteDrawRequestPool.Push(new DrawRequest());
            // }


        }

        public override void Deactivate()
        {
            base.Deactivate();
            engine.DisplayChip = null;
        }

        public void ResetDrawCalls()
        {
            // Reset all draw requests
            // for (var layer = DrawRequestPixelDataLayers.Length - 1; layer > -1; layer--)
            // {
            //     var drawRequests = DrawRequestPixelDataLayers[layer];
            //
            //     for (var i = drawRequests.Count - 1; i > -1; i--)
            //     {
            //         var request = drawRequests[i];
            //         DrawRequestPixelDataPool.Push(request.pixelData);
            //     }
            //
            //     drawRequests.Clear();
            // }
        }

        // public DrawRequestPixelData NextDrawRequestPixelData()
        // {
        //     var request = new DrawRequestPixelData();
        //
        //     if (DrawRequestPixelDataPool.Count > 0)
        //         request.pixelData = DrawRequestPixelDataPool.Pop();
        //     else
        //         request.pixelData = new int[0];
        //
        //     return request;
        // }

        protected Stack<DrawRequest> SpriteDrawRequestPool;

        public DrawRequest? NextSpriteDrawRequest()
        {

            if (SpriteDrawRequestPool.Count > 0)
                return SpriteDrawRequestPool.Pop();

            // var request = new DrawRequest();
            //
            // if (SpriteDrawRequestPool.Count > 0)
            //     request.pixelData = DrawRequestPixelDataPool.Pop();
            // else
            //     request.pixelData = new int[0];

            return null;
        }

        int _total;
        int _srcX;
        int _srcY;
        int _colorID;
        int i1;
        int _index;

        public void CopyDrawRequestPixelData(int[] pixelData, int x, int y, int width, int height, bool flipH = false,
            bool flipV = false, int colorOffset = 0)
        {
            // int total;
            // int srcX;
            // int srcY;
            // int colorID;
            // // int i;
            // int index;

            var tmpWidth = this.Width;
            var tmpHeight = this.Height;

            _total = width * height;

            for (i1 = 0; i1 < _total; i1++)
            {
                _colorID = pixelData?[i1] ?? 0;

                if (_colorID > -1)
                {
                    if (colorOffset > 0) _colorID += colorOffset;

                    _srcX = i1 % width;
                    _srcY = i1 / width;

                    if (flipH) _srcX = width - 1 - _srcX;

                    if (flipV) _srcY = height - 1 - _srcY;

                    _srcX += x;
                    _srcY += y;

                    // Make sure x & y are wrapped around the display
                    // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
                    _srcY = (_srcY % tmpHeight + tmpHeight) % tmpHeight;
                    _srcX = (_srcX % tmpWidth + tmpWidth) % tmpWidth;
                    // size is still == _width from the previous operation - let's reuse the local

                    // Find the index
                    _index = _srcX + tmpWidth * _srcY;

                    // Set the pixel
                    Pixels[_index] = _colorID;//cachedColors[_colorID];
                }
            }
        }

        private bool clearFlag = false;

        public void Clear()
        {
            clearFlag = true;
        }

    }
}