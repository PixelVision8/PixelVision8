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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using SharpFileSystem;
using SharpFileSystem.IO;

namespace PixelVision8.Runner.Services
{
    public class WorkspaceServicePlus : WorkspaceService
    {
        public WorkspaceServicePlus(BiosService bios, DesktopRunner runner) : base(bios, runner)
        {
        }

        // Exports the active song in the music chip
        public void ExportSong(string path, MusicChip musicChip, SoundChip soundChip)
        {
            var filePath = FileSystemPath.Parse(path);

            if (fileSystem.Exists(filePath))
            {
                filePath = filePath.AppendDirectory("Loops");

                if (!fileSystem.Exists(filePath)) fileSystem.CreateDirectory(filePath);

                try
                {
                    var exportService = locator.GetService(typeof(ExportService).FullName) as ExportService;


                    // TODO exporting sprites doesn't work
                    if (exportService != null)
                    {
                        exportService.ExportSong(filePath.Path, musicChip, soundChip);
//
                        exportService.StartExport();
                    }
                }
                catch (Exception e)
                {
                    // TODO this needs to go through the error system?
                    Console.WriteLine(e);
                    throw;
                }

                // TODO saving song doesn't work
//                runner.exportService.ExportSong(filePath.Path, musicChip, soundChip);
//
//                runner.StartExport();
            }
        }

        public bool VaildateSpriteBuilderFolder(FileSystemPath rootPath)
        {
            // TODO need to make sure this is in the current game directory and uses the filesyem path
            rootPath = rootPath.AppendDirectory("SpriteBuilder");//(string) ReadBiosData("SpriteBuilderDir", "SpriteBuilder"));

            return fileSystem.Exists(rootPath);
        }

        public int GenerateSprites(string path, PixelVisionEngine targetGame)
        {
            var count = 0;

            var filePath = FileSystemPath.Parse(path);

            var srcPath = filePath.AppendDirectory("SpriteBuilder");

            var fileData = new Dictionary<string, byte[]>();

            if (fileSystem.Exists(srcPath))
            {
                // Get all the files in the folder
                var files = from file in fileSystem.GetEntities(srcPath)
                    where file.GetExtension() == ".png"
                    select file;

                foreach (var file in files)
                {
                    var name = file.EntityName.Substring(0, file.EntityName.Length - file.GetExtension().Length);

                    var bytes = fileSystem.OpenFile(file, FileAccess.Read).ReadAllBytes();

                    if (fileData.ContainsKey(name))
                        fileData[name] = bytes;
                    else
                        fileData.Add(name, bytes);

                    count++;
//                    Console.WriteLine("Parse File " + name);
                }

                try
                {
                    var exportService = locator.GetService(typeof(ExportService).FullName) as ExportService;


                    // TODO exporting sprites doesn't work
                    if (exportService != null)
                    {
                        exportService.ExportSpriteBuilder(path + "sb-sprites.lua", targetGame, fileData);
//
                        exportService.StartExport();
                    }
                }
                catch (Exception e)
                {
                    // TODO this needs to go through the error system?
                    Console.WriteLine(e);
                    throw;
                }
            }

            return count;
        }
    }
}