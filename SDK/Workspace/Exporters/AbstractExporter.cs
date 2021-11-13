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

using PixelVision8.Runner;
using System.Collections.Generic;

namespace PixelVision8.Runner.Exporters
{
    public class AbstractExporter : AbstractParser, IExporter
    {
        public AbstractExporter(string fileName)
        {
            this.fileName = fileName;

            Response = new Dictionary<string, object>
            {
                {"success", false},
                {"message", ""}
            };
        }

        public Dictionary<string, object> Response { get; }
        public string fileName { get; protected set; }
    }
}