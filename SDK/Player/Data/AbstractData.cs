//   
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

namespace PixelVision8.Player
{
    /// <summary>
    ///     The AbstractData class represents a standard foundation for all
    ///     data objects in the engine. It implements the ISave, ILoad and
    ///     IInvalidate interfaces and provides as standard API for serializing
    ///     the data it contains via the CustomSerializeData() method.
    /// </summary>
    public abstract class AbstractData
    {
        /// <summary>
        ///     The invalid flag allows you to quickly see if data has been changed
        ///     on the data instance. This is used in conjunction with the
        ///     Invalidate() and ResetValidation() methods. Use this flag in classes
        ///     that have the potential to be expensive to update and need to be delayed
        ///     before refreshing their data.
        /// </summary>
        /// <value>Boolean</value>
        public bool Invalid {get; set;}

        /// <summary>
        ///     This method allows a clean way to set the invalid property to true
        ///     signaling a change in the underlying data. This method could be overridden
        ///     to provide additional logic when the AbstractData is invalidated.
        /// </summary>
        public virtual void Invalidate()
        {
            Invalid = true;
        }

        public virtual void ResetValidation() //Should be faster without calling 2 extra functions per tile. Adds 1-2FPS debugging.
        {
            Invalid = false;
        }
    }
}