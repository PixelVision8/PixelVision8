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

using Microsoft.Xna.Framework;
using PixelVision8.Player;
using PixelVision8.Workspace;
using System.Collections.Generic;
using System.Linq;
using PixelVision8.Runner;

namespace PixelVision8.Editor
{
    public class ScreenshotService : AbstractService
    {
        private readonly PNGWriter imageExporter;

        //        private ITextureFactory textureFactory;
        private readonly WorkspaceService workspace;
        private bool active;

        public ScreenshotService(WorkspaceService workspace)
        {
            // TODO this needs to get teh workspace through the service
            //            this.textureFactory = textureFactory;
            this.workspace = workspace;

            imageExporter = new PNGWriter();
        }

        private WorkspacePath screenshotDir
        {
            get
            {
                //                var fileSystem = workspace.fileSystem;
                try
                {
                    var directoryName =
                        "Screenshots"; //workspace.ReadBiosData("ScreenshotDir", "Screenshots") as string;

                    var path = WorkspacePath.Root.AppendDirectory("Tmp").AppendDirectory(directoryName);

                    try
                    {
                        if (workspace.Exists(WorkspacePath.Root.AppendDirectory("Workspace")))
                            path = WorkspacePath.Root.AppendDirectory("Workspace")
                                .AppendDirectory(directoryName);
                    }
                    catch
                    {
                        //                        Console.WriteLine("Screenshot Error: No workspace found.");
                    }

                    // Check to see if a screenshot directory exits
                    if (!workspace.Exists(path)) workspace.CreateDirectoryRecursive(path);

                    active = true;

                    return path;
                }
                catch
                {
                    //                    Console.WriteLine("Save Screenshot Error:\n"+e.Message);
                }

                return WorkspacePath.Root;
            }
        }

        public WorkspacePath GenerateScreenshotName()
        {
            return workspace.UniqueFilePath(screenshotDir.AppendFile("screenshot.png"));
        }

        public bool TakeScreenshot(PixelVision engine)
        {
            //            throw new NotImplementedException();

            var fileName = GenerateScreenshotName().Path;

            if (active == false) return active;

            try
            {
                // var cachedColors = engine.ColorChip.colors;

                var cachedColors = ColorUtils.ConvertColors(engine.ColorChip.HexColors, "#FF00FF", true).Select(c=> new ColorData(c.R, c.G, c.B)).ToArray();

                var pixels = engine.DisplayChip.Pixels;

                var displaySize = engine.GameChip.Display();


                var visibleWidth = displaySize.X;
                var visibleHeight = displaySize.Y;
                var width = engine.DisplayChip.Width;


                // Need to crop the image
                var newPixels = new ColorData[visibleWidth * visibleHeight];

                var totalPixels = pixels.Length;
                var newTotalPixels = newPixels.Length;

                var index = 0;

                for (var i = 0; i < totalPixels; i++)
                {
                    var col = i % width;
                    if (col < visibleWidth && index < newTotalPixels)
                    {
                        newPixels[index] = cachedColors[pixels[i]];
                        index++;
                    }
                }

                // We need to do this manually since the exporter could be active and we don't want to break it for a screenshot
                var tmpExporter = new ImageExporter(fileName, imageExporter, newPixels, visibleWidth, visibleHeight);
                tmpExporter.CalculateSteps();

                // Manually step through the exporter
                while (tmpExporter.Completed == false) tmpExporter.NextStep();

                workspace.SaveExporterFiles(new Dictionary<string, byte[]> {{tmpExporter.fileName, tmpExporter.Bytes}});

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