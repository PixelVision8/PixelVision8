//   
// Copyright (c) Jesse Freeman. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) License. 
// See LICENSE file in the project root for full license information. 
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman - @JesseFreeman
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany

using System;

namespace PixelVisionSDK.Chips
{

    /// <summary>
    ///     The color map chip is used to help import sprites and tile maps into the
    ///     engine if their colors don't match the system colors. When loading
    ///     artwork, if a color map is present, it will use the order of the colors
    ///     in the map as the index to remap to the system colors in the ColorChip.
    /// </summary>
    public class ColorMapChip : AbstractChip, IColorChip
    {

        protected string[] _colors = new string[256];

        protected string transparent = "#ff00ff";
        
        // This is ignored in this class
        public bool debugMode { get; set; }
        
        public ColorData[] colors
        {
            get
            {
                var t = total;
                var colorCache = new ColorData[t];

                for (var i = 0; i < t; i++)
                    colorCache[i] = new ColorData(_colors[i]);

                return colorCache;
            }
        }

        public string[] hexColors
        {
            get
            {
                var colors = new string[total];

                Array.Copy(_colors, colors, total);

                return colors;
            }
        }

        public int total
        {
            get { return _colors.Length; }
            set
            {
                Array.Resize(ref _colors, value);
                Clear();
            }
        }

        public void Clear()
        {
            var t = _colors.Length;
            for (var i = 0; i < t; i++)
                UpdateColorAt(i, transparent);
        }

        public string ReadColorAt(int index)
        {
            return index < 0 || index > _colors.Length - 1 ? transparent : _colors[index];
        }

        public int FindColorID(string color)
        {
            return Array.IndexOf(_colors, color);
        }

        public void UpdateColorAt(int index, string color)
        {
            if (index > _colors.Length)
                return;

            if (ColorData.ValidateColor(color))
                _colors[index] = color;
        }

        public int pages { get; set; }
        public int colorsPerPage { get; set; }
        public int supportedColors { get; private set; }
        public void RecalculateSupportedColors()
        {
            throw new NotImplementedException();
        }

        public void RebuildColorPages(int totalColors)
        {
            total = totalColors;
        }

        /// <summary>
        ///     This method registers the color map with the engine. It also marks
        ///     export to false so it is not serialized with the
        ///     other chips in the ChipManager.
        /// </summary>
        public override void Configure()
        {
            // This is a temporary chip so don't export it
            export = false;
            engine.colorMapChip = this;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            engine.colorMapChip = null;
        }

    }

}