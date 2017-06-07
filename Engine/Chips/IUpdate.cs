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

namespace PixelVisionSDK.Chips
{

    /// <summary>
    ///     This internal is for classes that need to be part of
    ///     the engine's Draw system.
    /// </summary>
    public interface IUpdate
    {

        /// <summary>
        ///     This Update() method is called as part of the engine's life-cycle.
        ///     Use this method for business logic, calculations and detecting
        ///     input.
        /// </summary>
        /// <param name="timeDelta"></param>
        void Update(float timeDelta);

    }

}