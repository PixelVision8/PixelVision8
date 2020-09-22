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
using System.IO.Compression;
using System.Linq;
using Microsoft.Xna.Framework;
// using ICSharpCode.SharpZipLib.Zip;
// using ICSharpCode.SharpZipLib.Zip;
using PixelVision8.Runner.Services;
using PixelVision8.Runner.Workspace;

namespace PixelVision8.Runner.Exporters
{
    public class ZipExporter : AbstractExporter
    {
        protected List<KeyValuePair<WorkspacePath, WorkspacePath>> SourceFiles;
        protected MemoryStream ZipFs;
        protected int CurrentFile;
        protected ZipArchive Archive;
        protected CompressionLevel compressionLevel;


        public ZipExporter(string fileName, IFileLoadHelper fileLoadHelper, Dictionary<WorkspacePath, WorkspacePath> srcFiles, int compressionLevel = 0) : base(fileName)
        {
            SourceFiles = srcFiles.ToList();

            FileLoadHelper = fileLoadHelper;

            ZipFs = new MemoryStream();

            this.compressionLevel = (CompressionLevel)MathHelper.Clamp(compressionLevel, 0, 2);
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            steps.Add(CreateZip);

            for (int i = 0; i < SourceFiles.Count; i++)
            {
                steps.Add(AddFile);
            }

            steps.Add(CloseZip);
        }

        public override void LoadSourceData()
        {
            //base.LoadSourceData();
        }

        public void CreateZip()
        {
            Archive = new ZipArchive(ZipFs, ZipArchiveMode.Create, true);
            
            StepCompleted();
        }

        public void AddFile()
        {
            try
            {
                var file = SourceFiles[CurrentFile];

                var srcFile = file.Key;
                var destFile = file.Value;

                // We can only save files
                if (srcFile.IsFile && !srcFile.EntityName.StartsWith(".") && destFile.IsFile)
                {
                    ZipArchiveEntry archiveEntry = Archive.CreateEntry(destFile.Path.Substring(1), compressionLevel);
                    using (Stream entryStream = archiveEntry.Open())
                    {
                        var stream = new MemoryStream(FileLoadHelper.ReadAllBytes(srcFile.Path));
                        stream.CopyTo(entryStream);
                    }
                    
                }

                CurrentFile++;

                StepCompleted();
            }
            catch (Exception e)
            {
                Response["error"] = e.Message;
                Response["success"] = false;

                // Finish running the exporter since there was an error
                currentStep = totalSteps;
            }
        }

        public void CloseZip()
        {

            Response.Add("fileSize", ZipFs.Length / 1024);
            Response["success"] = true;

            // Archive.Finish();
            
            Archive.Dispose();
            
            ZipFs.Seek(0, SeekOrigin.Begin);

            bytes = ZipFs.ToArray();

            // Archive.Close();
            ZipFs.Close();

            StepCompleted();
        }

        public override void Dispose()
        {
            base.Dispose();
            
            
            ZipFs.Dispose();
        }
    }

}