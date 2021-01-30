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
using PixelVision8.Runner.Workspace;
using System;
using System.IO;

namespace PixelVision8.Runner
{
    public class WorkspaceFileLoadHelper : IFileLoader
    {
        protected WorkspaceService WorkspaceService;

        public WorkspaceFileLoadHelper(WorkspaceService workspaceService)
        {
            WorkspaceService = workspaceService;
        }

        public string GetFileName(string path)
        {
            return WorkspacePath.Parse(path).EntityName;
        }

        public byte[] ReadAllBytes(string file)
        {
            var path = WorkspacePath.Parse(file);

            using (var memoryStream = new MemoryStream())
            {
                using (var fileStream = WorkspaceService.OpenFile(path, FileAccess.Read))
                {
                    fileStream.CopyTo(memoryStream);
                    fileStream.Close();
                }

                return memoryStream.ToArray();
            }
        }

        public bool Exists(string path)
        {
            return WorkspaceService.Exists(WorkspacePath.Parse(path));
        }
    }
}