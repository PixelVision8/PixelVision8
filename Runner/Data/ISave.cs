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

namespace PixelVisionSDK
{
    /// <summary>
    ///     The ISave internal provides the API for
    ///     serializing data into a string from a class that implements the
    ///     interface. It's broken up into two methods that work together. The first
    ///     method SerializeData() is used to set up the basic string wrapper and
    ///     the CustomSerializedData() method is used for adding custom values to
    ///     the string. These methods were designed to work with a JSON serialize.
    /// </summary>
    public interface ISave
    {
        bool ignore { get; }
        
        /// <summary>
        ///     Use this method to create a new StringBuilder instance and wrap any
        ///     custom serialized data by leveraging the CustomSerializedData()
        ///     method.
        /// </summary>
        /// <returns>
        /// </returns>
        string SerializeData();
    }
}