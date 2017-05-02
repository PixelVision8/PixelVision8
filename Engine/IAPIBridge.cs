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

using PixelVisionSDK.Chips;
using PixelVisionSDK.Services;

namespace PixelVisionSDK
{
    /// <summary>
    ///     This class combines the Pixel Vision APIs with some additonal
    ///     helper APIs to create a common interface to allow games to talk
    ///     directly to the Pixel Vision Engine.
    ///     chips.
    /// </summary>
    public interface IAPIBridge : IPixelVisionAPI
    {
        /// <summary>
        ///     Returns a reference to the current game instance.
        /// </summary>
        IService GetService(string id);

        // <summary>
        ///     Offers access to the underlying service manager to expose internal
        ///     service APIs to any class referencing the APIBridge.
        /// </summary>
        /// <param name="id">Name of the service.</param>
        /// <returns>Returns an IService instance associated with the supplied ID.</returns>
        GameChip currentGame { get; }
    }
}