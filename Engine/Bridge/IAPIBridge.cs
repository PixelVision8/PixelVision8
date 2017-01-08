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
// 

using PixelVisionSDK.Engine;
using PixelVisionSDK.Engine.Chips;

namespace PixelVisionSDK
{

    /// <summary>
    ///     This internal defines the APIs for the
    ///     <see cref="IAPIBridge" /> which allows games to talk to the engine's
    ///     chips.
    /// </summary>
    public interface IAPIBridge : IPixelVisionAPI
    {

        /// <summary>
        ///     A reference to the core <see cref="chips" /> in the engine.
        /// </summary>
        IEngineChips chips { get; set; }

    }

}