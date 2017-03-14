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
// 

namespace PixelVisionSDK.Chips
{
    /// <summary>
    ///     The Interface for the chip class. It handles Activating, configuring and
    ///     deactivating the chip as part of its life-cycle. This Interface also
    ///     implements the <see cref="ILoad" /> and <see cref="ISave" /> interfaces
    ///     for serialization.
    /// </summary>
    public interface IChip
    {
        /// <summary>
        ///     <see cref="Activate" /> is the beginning of the chip's life cycle.
        ///     This allows the chip to gain a reference to the engine itself. This
        ///     allows chips to talk back to the engine as well as to each other
        ///     through the engine's exposed APIs.
        /// </summary>
        /// <param name="parent">A reference to the engine.</param>
        void Activate(PixelVisionEngine parent);

        /// <summary>
        ///     <see cref="Configure" /> is the second part of the chip's life-cycle.
        ///     It is called after Activate() and is designed to be overridden by
        ///     children classes so perform specific configuration tasks. This
        ///     method must be implemented in order for a chip to activate
        ///     correctly.
        /// </summary>
        void Deactivate();

        /// <summary>
        ///     When called, this method sets the disabled field to false. It's part
        ///     of the chip's life-cycle and is called when shutting down the
        ///     ChipManager.
        /// </summary>
        void Configure();
    }
}