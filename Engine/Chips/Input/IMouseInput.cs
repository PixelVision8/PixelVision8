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

using System;

namespace PixelVisionSDK
{
    public interface IMouseInput
    {
        /// <summary>
        ///     The current mouse position in pixel coordinates.
        /// </summary>
        /// <returns>Returns a vector with the current mouse's x and y position.</returns>
        Vector ReadMousePosition();

        /// <summary>
        ///     Determines if the mouse button is down.
        /// </summary>
        /// <param name="id">
        ///     The id of the mouse button. Its set to 0 by default. 0 is the left
        ///     mouse and 1 is the right.
        /// </param>
        /// <returns>
        /// </returns>
        bool ReadMouseButton(int button);

        /// <summary>
        ///     <para>Returns true during the frame the user pressed the given mouse button.</para>
        /// </summary>
        /// <param name="button"></param>
        bool ReadMouseButtonDown(int button);

        /// <summary>
        ///     Determines if the state of the mouse button.
        /// </summary>
        /// <param name="id">
        ///     The id of the mouse button. Its set to 0 by default. 0 is the left
        ///     mouse and 1 is the right.
        /// </param>
        /// <returns>
        /// </returns>
        bool ReadMouseButtonUp(int button);

        /// <summary>
        ///     Current x position of the mouse on the screen.
        /// </summary>
        int ReadMouseX();

        /// <summary>
        ///     Current y position of the mouse on the screen.
        /// </summary>
        int ReadMouseY();

        #region Deprecated

        [Obsolete]
        Vector mousePosition { get; }
        bool GetMouseButton(int button);
        bool GetMouseButtonDown(int button);
        bool GetMouseButtonUp(int button);

        #endregion
    }
}