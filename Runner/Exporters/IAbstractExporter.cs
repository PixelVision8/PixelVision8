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

using PixelVision8.Runner.Parsers;

namespace PixelVision8.Runner.Exporters
{
    public interface IAbstractExporter : IAbstractParser
    {
        byte[] bytes { get; }
        string fileName { get; }
    }
}