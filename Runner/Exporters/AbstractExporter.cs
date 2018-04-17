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

using PixelVisionRunner.Parsers;

namespace PixelVisionRunner.Exporters
{
    public class AbstractExporter : AbstractParser, IAbstractExporter
    {

        public string fileName { get; protected set; }

        public byte[] bytes { get; protected set; }

        public AbstractExporter(string fileName)
        {
            this.fileName = fileName;
        }
    }
}