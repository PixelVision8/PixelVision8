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

using PixelVision8.Runner.Services;

namespace PixelVision8.Runner.Importers
{
    public class PNGFileReader : PNGReader
    {
        public string FilePath;
        public IFileLoadHelper FileLoadHelper;

        public PNGFileReader(string filePath, IFileLoadHelper fileLoadHelper, string maskHex = "#FF00FF"):base(null, maskHex)
        {
            FilePath = filePath;
            FileLoadHelper = fileLoadHelper;
        }

        public override void ReadStream()
        {
            if (!string.IsNullOrEmpty(FilePath))
            {
                ReadBytes(FileLoadHelper.ReadAllBytes(FilePath));
            }
        
            base.ReadStream();
        }
        
        public override string FileName
        {
            get => FileLoadHelper.GetFileName(FilePath);
            set
            {
                // Do nothing
            }
        }
    }
}