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
using PixelVision8.Workspace;
using System.Collections.Generic;
using PixelVision8.Runner.Exporters;

namespace PixelVision8.Editor
{
    public class DiskExporter : ZipExporter
    {
        private long maxFileSize;

        public DiskExporter(string fileName, IFileLoader fileLoadHelper,
            Dictionary<WorkspacePath, WorkspacePath> srcFiles, long maxFileSize = 512, int compressionLevel = 0) : base(
            fileName, fileLoadHelper, srcFiles, compressionLevel)
        {
            this.maxFileSize = maxFileSize;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            Steps.Add(ValidateSize);
            ;
        }

        private void ValidateSize()
        {
            if ((bool) Response["success"])
            {
                // TODO need to add lib files to zip (but they are going to have different source and destination paths)

                // Copy the file to the right location
                var fileSize = (long) Response["fileSize"];

                if (fileSize > maxFileSize)
                {
                    // Change the response message to reflect that the file is to big to save
                    Response["message"] =
                        "The game is too big to compile. You'll need to increase the game's size to create a new build with the current files.";

                    // Set the success back to false
                    Response["success"] = false;

                    Bytes = null;
                }
            }

            StepCompleted();
        }
    }
}