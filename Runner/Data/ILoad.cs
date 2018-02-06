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

using System.Collections.Generic;

namespace PixelVisionSDK
{
    /// <summary>
    ///     The ILoad interface allows a standard
    ///     API when loading de-serialized data into a class. It works on the concept
    ///     of passing in already de-serialized generic objects into the DeserializeData()
    ///     method to parse and set within the class.
    /// </summary>
    public interface ILoad
    {

        /// <summary>
        ///     The DeserializeData method allows you to pass in a
        ///     Dictionary with a string as the key and a generic object for the
        ///     value. This can be manually parsed to convert each key/value pair
        ///     into data used to configure the class that
        ///     implements this interface.
        /// </summary>
        /// <param name="data">
        ///     A Dictionary with a string as the key and a generic object as the
        ///     value.
        /// </param>
        void DeserializeData(Dictionary<string, object> data);
    }
}