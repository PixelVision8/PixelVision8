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
using ICSharpCode.SharpZipLib.Zip;
using PixelVision8.Runner.Services;
using PixelVision8.Runner.Workspace;

namespace PixelVision8.Runner.Exporters
{
    public class ZipExporter : AbstractExporter
    {
        protected List<KeyValuePair<WorkspacePath, WorkspacePath>> SourceFiles;
        protected MemoryStream ZipFs;
        protected int CurrentFile;
        protected ZipOutputStream Archive;
        protected byte[] Buffer;
        protected int compressionLevel;


        public ZipExporter(string fileName, IFileLoadHelper fileLoadHelper, Dictionary<WorkspacePath, WorkspacePath> srcFiles, int compressionLevel = 4) : base(fileName)
        {
            this.SourceFiles = srcFiles.ToList();

            this.FileLoadHelper = fileLoadHelper;

            // TODO convert dictionary into an array

            ZipFs = new MemoryStream();

            this.compressionLevel = compressionLevel;
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
            Archive = new ZipOutputStream(ZipFs);
            Archive.SetLevel(compressionLevel);
            Buffer = new byte[4096];

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

                    // Using GetFileName makes the result compatible with XP
                    // as the resulting path is not absolute.
                    var entry = new ZipEntry(destFile.Path.Substring(1))
                    {
                        // Could also use the last write time or similar for the file.
                        DateTime = DateTime.Now
                    };
                    
                    Archive.PutNextEntry(entry);

                    using (var fs = new MemoryStream(FileLoadHelper.ReadAllBytes(srcFile.Path)))
                    {
                        // Using a fixed size buffer here makes no noticeable difference for output
                        // but keeps a lid on memory usage.
                        int sourceBytes;

                        do
                        {
                            sourceBytes = fs.Read(Buffer, 0, Buffer.Length);
                            Archive.Write(Buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }

                    Archive.CloseEntry();
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

            Archive.Finish();
            

            ZipFs.Seek(0, SeekOrigin.Begin);

            bytes = ZipFs.ToArray();

            Archive.Close();
            ZipFs.Close();

            StepCompleted();
        }

        public override void Dispose()
        {
            base.Dispose();
            
            Archive.Dispose();
            ZipFs.Dispose();
        }
    }

}