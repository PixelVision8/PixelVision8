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

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelVision8.Engine;
using PixelVision8.Engine.Services;
using PixelVision8.Runner.Exporters;
using SharpFileSystem;

namespace PixelVision8.Runner.Services
{
    public class ScreenshotService : AbstractService
    {
        private bool active;

//        private ITextureFactory textureFactory;
        private readonly WorkspaceService workspace;
        private PNGWriter imageExporter;

        public ScreenshotService(WorkspaceService workspace)
        {
            // TODO this needs to get teh workspace through the service
//            this.textureFactory = textureFactory;
            this.workspace = workspace;
            
            this.imageExporter = new PNGWriter();
        }

        private FileSystemPath screenshotDir
        {
            get
            {
                var fileSystem = workspace.fileSystem;
                try
                {
                    var directoryName = "Screenshots";//workspace.ReadBiosData("ScreenshotDir", "Screenshots") as string;

                    var path = FileSystemPath.Root.AppendDirectory("Tmp").AppendDirectory(directoryName);

                    try
                    {
                        if (fileSystem.Exists(FileSystemPath.Root.AppendDirectory("Workspace")))
                            path = FileSystemPath.Root.AppendDirectory("Workspace")
                                .AppendDirectory(directoryName);
                    }
                    catch
                    {
//                        Console.WriteLine("Screenshot Error: No workspace found.");
                    }

                    // Check to see if a screenshot directory exits
                    if (!fileSystem.Exists(path)) fileSystem.CreateDirectoryRecursive(path);

                    active = true;

                    return path;
                }
                catch
                {
//                    Console.WriteLine("Save Screenshot Error:\n"+e.Message);
                }

                return FileSystemPath.Root;
            }
        }

        public FileSystemPath GenerateScreenshotName()
        {
            return workspace.UniqueFilePath(screenshotDir.AppendFile("screenshot.png"));
        }

        public bool TakeScreenshot(IEngine engine)
        {
//            throw new NotImplementedException();

            var fileName = GenerateScreenshotName().Path;
            
            if (active == false)
                return active;
            
            try
            {
                var pixels = engine.displayChip.pixels;
    
                var displaySize = engine.gameChip.Display();
    
    
                var visibleWidth = displaySize.X;
                var visibleHeight = displaySize.Y;
                var width = engine.displayChip.width;
                
                
                // Need to crop the image
                var newPixels = new Color[visibleWidth * visibleHeight];
    
                var totalPixels = pixels.Length;
                var newTotalPixels = newPixels.Length;
                
                var index = 0;
                
                for (int i = 0; i < totalPixels; i++)
                {
    
                    var col = i % width;
                    if (col < visibleWidth && index < newTotalPixels)
                    {
                        newPixels[index] = pixels[i];
                        index++;
                    }
    
                }
            
                // We need to do this manually since the exporter could be active and we don't want to break it for a screenshot
                var tmpExporter = new ImageExporter(fileName, imageExporter, newPixels, visibleWidth, visibleHeight);
                tmpExporter.CalculateSteps();
    
                // Manually step through the exporter
                while (tmpExporter.completed == false)
                    tmpExporter.NextStep();

            
                workspace.SaveExporterFiles(new Dictionary<string, byte[]> {{tmpExporter.fileName, tmpExporter.bytes}});
                
                return true;
            }
            catch
            {
//                Console.WriteLine("Take Screenshot Error:\n"+e.Message);
                // TODO throw some kind of error?
                return false;
            }
        }
    }
}