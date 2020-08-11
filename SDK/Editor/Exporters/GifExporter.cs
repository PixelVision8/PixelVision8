﻿//   
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
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;
using PixelVision8.Runner.Exporters;
using PixelVision8.Runner.Utils;

namespace PixelVision8.Runner
{
    public class GifExporter : AbstractExporter
    {

        private DisplayChip DisplayChip;
        private List<GifFrame> Frames = new List<GifFrame>();
        private Rectangle bounds;
        List<byte> byteList = new List<byte>();
        const string header = "GIF89a";
        // private Gif tmpGif;
        private ushort Width;
        private ushort Height;
        private List<Color> globalColorTable;
        private ApplicationExtension applicationExtension;
        private List<Color>[] colorTables;
        private Dictionary<int, List<Color>> distinctColors;
        private int scale = 1;
        private Dictionary<int, List<byte>> encoded;

        public bool ExportingFinished => byteList != null;

        public GifExporter(string fileName, IEngineChips engine) : base(fileName)
        {
            DisplayChip = engine.displayChip;
            bounds = DisplayChip.visibleBounds;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            steps.Add(StartGif);

            steps.Add(IndexColors);

            steps.Add(BuildColorTable);

            steps.Add(ConvertFrame);

        }

        public void AddFrame(float delay = 0)
        {

            var tmpFrame = new GifFrame();
            
            var tmpT2D = new Texture2D(DisplayChip.width, DisplayChip.height);

            tmpT2D.SetPixels32(DisplayChip.pixels);

            var pixels = tmpT2D.GetPixels(0, 0, bounds.Width, bounds.Height);

            tmpFrame.Texture = new Texture2D(bounds.Width, bounds.Height);
            tmpFrame.Texture.SetPixels32(pixels); // TODO need to crop this

            tmpFrame.Delay = delay;

            Frames.Add(tmpFrame);
        }
        
        public void StartGif()
        {
            
            Width = (ushort)(Frames[0].Texture.width);
            Height = (ushort)(Frames[0].Texture.height);
            globalColorTable = new List<Color>();
            applicationExtension = new ApplicationExtension();
            // byteList = new List<byte>();
            encoded = new Dictionary<int, List<byte>>();
            colorTables = new List<Color>[Frames.Count];
            distinctColors = new Dictionary<int, List<Color>>();

            currentStep++;
        }

        public void IndexColors()
        {
            for (var i = 0; i < Frames.Count; i++)
            {
                var frame = Frames[i];

                var distinct = frame.Texture.GetPixels32().Distinct().ToList();
                
                distinctColors.Add(i, distinct);
                
            }

            currentStep++;
        }

        public void BuildColorTable()
        {
            for (var i = 0; i < Frames.Count; i++)
            {
                var colors = distinctColors[i];
                var add = colors.Where(j => !globalColorTable.Contains(j)).ToList();

                if (globalColorTable.Count + add.Count <= 256)
                {
                    globalColorTable.AddRange(add);
                    colorTables[i] = globalColorTable;
                }
                else if (add.Count <= 256) // Introducing local color table
                {
                    colorTables[i] = colors;
                }
                else
                {
                    throw new Exception($"Frame #{i} contains more than 256 colors!");
                }
            }

            ReplaceTransparentColor(ref globalColorTable);

            currentStep++;
        }

        public void ConvertFrame()
        {
            for (var i = 0; i < Frames.Count; i++)
            {
                var index = i;//(int)context;
                var colorTable = colorTables[index];
                var localColorTableFlag = (byte)(colorTable == globalColorTable ? 0 : 1);
                var localColorTableSize = GetColorTableSize(colorTable);
                byte transparentColorFlag = 0, transparentColorIndex = 0;
                byte max;
                var colorIndexes = GetColorIndexes(Frames[index].Texture, scale, colorTable, localColorTableFlag, ref transparentColorFlag, ref transparentColorIndex, out max);
                var graphicControlExtension = new GraphicControlExtension(4, 0, (byte)Frames[index].DisposalMethod, 0, transparentColorFlag, (ushort)(100 * Frames[index].Delay), transparentColorIndex);
                var imageDescriptor = new ImageDescriptor(0, 0, Width, Height, localColorTableFlag, 0, 0, 0, localColorTableSize);
                var minCodeSize = LzwEncoder.GetMinCodeSize(max);
                var lzw = LzwEncoder.Encode(colorIndexes, minCodeSize);
                var tableBasedImageData = new TableBasedImageData(minCodeSize, lzw);
                var tmpBytes = new List<byte>();

                tmpBytes.AddRange(graphicControlExtension.GetBytes());
                tmpBytes.AddRange(imageDescriptor.GetBytes());

                if (localColorTableFlag == 1)
                {
                    tmpBytes.AddRange(ColorTableToBytes(colorTable, localColorTableSize));
                }

                tmpBytes.AddRange(tableBasedImageData.GetBytes());

                
                encoded.Add(index, tmpBytes);
                // encodeProgress.Progress++;

                if (encoded.Count == Frames.Count)
                {
                    var globalColorTableSize = GetColorTableSize(globalColorTable);
                    var logicalScreenDescriptor = new LogicalScreenDescriptor(Width, Height, 1, 7, 0, globalColorTableSize, 0, 0);
                    var binary = new List<byte>();

                    binary.AddRange(Encoding.UTF8.GetBytes(header));
                    binary.AddRange(logicalScreenDescriptor.GetBytes());
                    binary.AddRange(ColorTableToBytes(globalColorTable, globalColorTableSize));
                    binary.AddRange(applicationExtension.GetBytes());
                    binary.AddRange(encoded.OrderBy(j => j.Key).SelectMany(j => j.Value));
                    binary.Add(0x3B); // GIF Trailer.

                    bytes = binary.ToArray();
                    // encodeProgress.Completed = true;
                }
                
                // onProgress(encodeProgress);
                
            }

            currentStep++;
        }

        private static byte[] ColorTableToBytes(List<Color> colorTable, byte colorTableSize)
        {
            if (colorTable.Count > 256) throw new Exception("Color table size exceeds 256 size limit: " + colorTable.Count);

            var size = 1 << (colorTableSize + 1);
            var byteList = new byte[3 * size];

            for (var i = 0; i < colorTable.Count; i++)
            {
                byteList[3 * i] = colorTable[i].R;
                byteList[3 * i + 1] = colorTable[i].G;
                byteList[3 * i + 2] = colorTable[i].B;
            }

            return byteList;
        }
        private static void ReplaceTransparentColor(ref List<Color> colors)
        {
            for (var i = 0; i < colors.Count; i++)
            {
                if (colors[i].A == 0)
                {
                    colors.RemoveAll(j => j.A == 0);
                    colors.Insert(0, GetTransparentColor(colors));

                    return;
                }
            }
        }

        private static byte[] GetColorIndexes(Texture2D texture, int scale, List<Color> colorTable, byte localColorTableFlag, ref byte transparentColorFlag, ref byte transparentColorIndex, out byte max)
        {
            var indexes = new Dictionary<Color, int>();

            for (var i = 0; i < colorTable.Count; i++)
            {
                indexes.Add(colorTable[i], i);
            }

            var pixels = texture.GetPixels32();

            var colorIndexes = new byte[pixels.Length * scale * scale];

            max = 0;

            Action<int, int, byte> setScaledIndex = (x, y, index) =>
            {
                for (var dy = 0; dy < scale; dy++)
                {
                    for (var dx = 0; dx < scale; dx++)
                    {
                        colorIndexes[x * scale + dx + (y * scale + dy) * texture.width * scale] = index;
                    }
                }
            };

            for (var y = 0; y < texture.height; y++)
            {
                for (var x = 0; x < texture.width; x++)
                {
                    var pixel = pixels[x + y * texture.width];

                    if (pixel.A == 0)
                    {
                        if (transparentColorFlag == 0)
                        {
                            transparentColorFlag = 1;

                            if (localColorTableFlag == 1)
                            {
                                transparentColorIndex = (byte)indexes[pixel];
                                colorTable[transparentColorIndex] = GetTransparentColor(colorTable);
                            }
                        }

                        if (scale == 1)
                        {
                            colorIndexes[x + y * texture.width] = transparentColorIndex;
                        }
                        else
                        {
                            setScaledIndex(x, y, transparentColorIndex);
                        }

                        if (transparentColorIndex > max) max = transparentColorIndex;
                    }
                    else
                    {
                        var index = indexes[pixel];

                        if (index >= 0)
                        {
                            var i = (byte)index;

                            if (scale == 1)
                            {
                                colorIndexes[x + y * texture.width] = i;
                            }
                            else
                            {
                                setScaledIndex(x, y, i);
                            }

                            if (i > max) max = i;
                        }
                        else
                        {
                            throw new Exception("Color index not found: " + pixel);
                        }
                    }
                }
            }

            return colorIndexes;
        }

        private static Color GetTransparentColor(List<Color> colorTable)
        {
            for (byte r = 0; r < 0xFF; r++)
            {
                for (byte g = 0; g < 0xFF; g++)
                {
                    for (byte b = 0; b < 0xFF; b++)
                    {
                        var transparentColor = new Color(r, g, b, Convert.ToByte(1));

                        if (!colorTable.Contains(transparentColor))
                        {
                            return transparentColor;
                        }
                    }
                }
            }

            throw new Exception("Unable to resolve transparent color!");
        }

        private static byte GetColorTableSize(List<Color> colorTable)
        {
            byte size = 0;

            while (1 << (size + 1) < colorTable.Count)
            {
                size++;
            }

            return size;
        }

    }

}
