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

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using PixelVision8.Runner.Gif;

namespace PixelVision8.Runner
{
    class GifReader : AbstractParser
    {
        private static readonly Color EmptyColor = new Color();
        private static int Width;
        private static int Height;
        private static Color[] state;
        private static bool filled;
        private static List<GifFrame> frames;
        GifParser parser;
        private Dictionary<ImageDescriptor, byte[]> decoded;
        private int frameCount;

        public GifReader(byte[] bytes) : base()
        {
            this.Bytes = bytes;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            Steps.Add(CreateGif);
            Steps.Add(DecodeGif);
            Steps.Add(FinishDecoding);
        }

        public void CreateGif()
        {
            parser = new GifParser(Bytes);
        }

        public void DecodeGif()
        {
            decoded = new Dictionary<ImageDescriptor, byte[]>();
            frameCount = parser.Blocks.Count(i => i is ImageDescriptor);
            // var decodeProgress = new DecodeProgress { FrameCount = frameCount };

            for (var i = 0; i < parser.Blocks.Count; i++)
            {
                var imageDescriptor = parser.Blocks[i] as ImageDescriptor;

                if (imageDescriptor == null) continue;

                var data = (TableBasedImageData) parser.Blocks[i + 1 + imageDescriptor.LocalColorTableFlag];

                // ThreadPool.QueueUserWorkItem(context =>
                // {
                //     if (decodeProgress.Completed || decodeProgress.Exception != null) return;
                //
                byte[] colorIndexes;

                // try
                // {
                colorIndexes = LzwDecoder.Decode(data.ImageData, data.LzwMinimumCodeSize);
                // }
                // catch (Exception e)
                // {
                //     decodeProgress.Exception = e;
                //     decodeProgress.Completed = true;
                //     onProgress(decodeProgress);
                //     return;
                // }

                // lock (decoded)
                // {
                decoded.Add(imageDescriptor, colorIndexes);
                // decodeProgress.Progress++;

                // if (decoded.Count == frameCount)
                // {
                //     try
                //     {
                //         decodeProgress.Gif = CompleteDecode(parser, decoded);
                //         decodeProgress.Completed = true;
                //     }
                //     catch (Exception e)
                //     {
                //         decodeProgress.Exception = e;
                //         decodeProgress.Completed = true;
                //     }
                // }
                //
                // onProgress(decodeProgress);
                // }
                // });
            }
        }


        private void FinishDecoding()
        {
            // var globalColorTable = parser.LogicalScreenDescriptor.GlobalColorTableFlag == 1 ? GetUnityColors(parser.GlobalColorTable) : null;
            //var backgroundColor = globalColorTable?[parser.LogicalScreenDescriptor.BackgroundColorIndex] ?? EmptyColor;
            GraphicControlExtension gcExtension = null;
            Width = parser.LogicalScreenDescriptor.LogicalScreenWidth;
            Height = parser.LogicalScreenDescriptor.LogicalScreenHeight;
            state = new Color[Width * Height];
            filled = false;
            frames = new List<GifFrame>();

            for (var j = 0; j < parser.Blocks.Count; j++)
            {
                if (parser.Blocks[j] is GraphicControlExtension)
                {
                    gcExtension = (GraphicControlExtension) parser.Blocks[j];
                }
                else if (parser.Blocks[j] is ImageDescriptor)
                {
                    var imageDescriptor = (ImageDescriptor) parser.Blocks[j];

                    if (imageDescriptor.InterlaceFlag == 1)
                        throw new NotSupportedException("Interlacing is not supported!");

                    // var colorTable = imageDescriptor.LocalColorTableFlag == 1 ? GetUnityColors((ColorTable) parser.Blocks[j + 1]) : globalColorTable;
                    var colorIndexes = decoded[imageDescriptor];
                    var frame = DecodeFrame(gcExtension, imageDescriptor, colorIndexes, filled, Width, Height, state);

                    frames.Add(frame);

                    //if (frames.Count == 1 && globalColorTable != null)
                    //{
                    //	if (gcExtension == null || gcExtension.TransparentColorFlag == 0 || gcExtension.TransparentColorIndex != parser.LogicalScreenDescriptor.BackgroundColorIndex)
                    //	{
                    //		backgroundColor = globalColorTable[parser.LogicalScreenDescriptor.BackgroundColorIndex];
                    //	}
                    //}

                    switch (frame.DisposalMethod)
                    {
                        case DisposalMethod.NoDisposalSpecified:
                        case DisposalMethod.DoNotDispose:
                            break;
                        case DisposalMethod.RestoreToBackgroundColor:
                            for (var i = 0; i < state.Length; i++)
                            {
                                state[i] = EmptyColor;
                            }

                            filled = true;
                            break;
                        case DisposalMethod.RestoreToPrevious
                            : // 'state' was already copied before decoding current frame
                            filled = false;
                            break;
                        default:
                            throw new NotSupportedException($"Unknown disposal method: {frame.DisposalMethod}!");
                    }
                }
            }

            // return new Gif(frames);
        }

        private static GifFrame DecodeFrame(GraphicControlExtension extension, ImageDescriptor descriptor,
            byte[] colorIndexes, bool filled, int Width, int Height, Color[] state)
        {
            var frame = new GifFrame();
            var pixels = state;
            var transparentIndex = -1;

            if (extension != null)
            {
                frame.Delay = extension.DelayTime / 100f;
                frame.DisposalMethod = (DisposalMethod) extension.DisposalMethod;

                if (frame.DisposalMethod == DisposalMethod.RestoreToPrevious)
                {
                    pixels = state.ToArray();
                }

                if (extension.TransparentColorFlag == 1)
                {
                    transparentIndex = extension.TransparentColorIndex;
                }
            }

            for (var y = 0; y < descriptor.ImageHeight; y++)
            {
                for (var x = 0; x < descriptor.ImageWidth; x++)
                {
                    var colorIndex = colorIndexes[x + y * descriptor.ImageWidth];
                    var transparent = colorIndex == transparentIndex;

                    if (transparent && !filled) continue;

                    var color = EmptyColor; //transparent ? EmptyColor : colorTable[colorIndex];
                    var fx = x + descriptor.ImageLeftPosition;
                    var fy = Height - y - 1 - descriptor.ImageTopPosition; // Y-flip

                    pixels[fx + fy * Width] = pixels[fx + fy * Width] = color;
                }
            }

            frame.Texture = new Texture2D(Width, Height);
            frame.Texture.SetPixels32(pixels);
            // frame.Texture.Apply();

            return frame;
        }
    }
}