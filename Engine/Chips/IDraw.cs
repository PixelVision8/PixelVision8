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

namespace PixelVision8.Engine.Chips
{

    /// <summary>
    ///     This internal is for classes that need to be part of
    ///     the engine's Draw system.
    /// </summary>
    public interface IDraw
    {

        /// <summary>
        ///     This Draw() method is called as part of the engine's life-cycle. Use
        ///     this method for rendering logic. It is called once per frame.
        /// </summary>
        void Draw();

    }

}