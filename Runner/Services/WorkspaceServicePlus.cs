using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameCreator.Services;
using MonoGameRunner;
using PixelVision8.Runner.Services;
using PixelVisionSDK;
using PixelVisionSDK.Chips;
using SharpFileSystem;
using SharpFileSystem.IO;

namespace PixelVisionPlus.Services
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

                if (!fileSystem.Exists(filePath))
                {
                    fileSystem.CreateDirectory(filePath);
                }
                
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
            rootPath = rootPath.AppendDirectory((string)ReadBiosData("SpriteBuilderDir", "SpriteBuilder"));

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
                var files = from file in fileSystem.GetEntities(srcPath) where file.GetExtension() == ".png" select file;

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