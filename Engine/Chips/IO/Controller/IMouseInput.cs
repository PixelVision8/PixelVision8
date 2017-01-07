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
// Jesse Freeman
// 

using PixelVisionSDK.Engine.Chips.Data;

namespace PixelVisionSDK.Engine.Chips.IO.Controller
{
    public interface IMouseInput
    {
        /// <summary>
        ///     <para>The current mouse position in pixel coordinates. (Read Only)</para>
        /// </summary>
        Vector mousePosition { get; }

        /// <summary>
        ///     Determines if the mouse button is down.
        /// </summary>
        /// <param name="id">
        ///     The id of the mouse button. Its set to 0 by default. 0 is the left
        ///     mouse and 1 is the right.
        /// </param>
        /// <returns>
        /// </returns>
        bool GetMouseButton(int button);

        /// <summary>
        ///     <para>Returns true during the frame the user pressed the given mouse button.</para>
        /// </summary>
        /// <param name="button"></param>
        bool GetMouseButtonDown(int button);

        /// <summary>
        ///     Determines if the state of the mouse button.
        /// </summary>
        /// <param name="id">
        ///     The id of the mouse button. Its set to 0 by default. 0 is the left
        ///     mouse and 1 is the right.
        /// </param>
        /// <returns>
        /// </returns>
        bool GetMouseButtonUp(int button);
    }
}