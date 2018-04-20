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

using PixelVisionRunner;

namespace PixelVisionSDK.Chips
{

    /// <summary>
    ///     The <see cref="IColorChip" /> internal represents the
    ///     main APIs for the PixelVisionEngine's color system.
    /// </summary>
    public interface IColorChip
    {

        /// <summary>
        ///     An array of the supported <see cref="colors" /> in the chip.
        /// </summary>
        IColor[] colors { get; }

        string[] hexColors { get; }

        /// <summary>
        ///     The <see cref="total" /> number of <see cref="colors" /> in the chip.
        ///     Changing this value will clear the <see cref="colors" /> and resize
        ///     the <see cref="colors" /> array.
        /// </summary>
        int total { get; }

        /// <summary>
        ///     Clears all of the <see cref="colors" /> in the chip.
        /// </summary>
        void Clear();

        /// <summary>
        ///     Reads a color at a specific index. The internal
        ///     classes use color references to find colors, these indexes match up
        ///     to the values in the <see cref="colors" /> array.
        /// </summary>
        /// <param name="index">
        ///     The index of a color in the <see cref="colors" /> array
        /// </param>
        /// <returns>
        ///     Returns a Unity color class
        /// </returns>
        string ReadColorAt(int index);

        /// <summary>
        ///     This method looks up a color's index based on the
        ///     <paramref name="color" /> passed in.
        /// </summary>
        /// <param name="color">
        ///     A Unity <see cref="Color" /> Class to look for inside of the
        ///     <see cref="colors" /> array.
        /// </param>
        /// <returns>
        ///     Returns an index of the <paramref name="color" /> in the
        ///     <see cref="colors" /> array or -1 if it was not found.
        /// </returns>
        int FindColorID(string color);

        /// <summary>
        ///     Updates a <paramref name="color" /> at a specific index. If the
        ///     <paramref name="index" /> is out of range of the <see cref="colors" />
        ///     array it will be ignored.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="color"></param>
        void UpdateColorAt(int index, string color);


        int pages { get; set; }
        int colorsPerPage { get; set; }
        int supportedColors { get; }
//        void RecalculateSupportedColors();
        void RebuildColorPages(int totalColors);
        bool debugMode { get; set; }
    }

}