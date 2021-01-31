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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PixelVision8.Runner.Exporters
{
    public class ZipDiskExporter : AbstractExporter
    {
        protected ZipExporter zipExporter;
        private WorkspacePath diskPath;
        private string physicalPath;
        private string physicalBackupPath;
        private ZipFileSystem zipFileSystem;
        private WorkspaceService workspaceService;

        public ZipDiskExporter(string fileName, WorkspaceService workspaceService) : base(fileName)
        {
            diskPath = WorkspacePath.Parse(fileName);
            this.workspaceService = workspaceService;
            this.FileLoadHelper = new WorkspaceFileLoadHelper(this.workspaceService);
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            if (workspaceService.Exists(diskPath))
            {
                zipFileSystem = workspaceService.Get(diskPath).Value as ZipFileSystem;

                if (zipFileSystem == null || zipFileSystem.PhysicalRoot == null)
                {
                    return;
                }

                // Get the physical path
                physicalPath = zipFileSystem.PhysicalRoot;
                physicalBackupPath = zipFileSystem.PhysicalRoot + ".bak";

                Steps.Add(BackupZip);

                // Get all the files
                var srcFiles = zipFileSystem.GetEntitiesRecursive(WorkspacePath.Root).ToArray();

                // Convert files into the correct format: src/dest path
                var files = new Dictionary<WorkspacePath, WorkspacePath>();
                foreach (var file in srcFiles)
                {
                    files.Add(diskPath.AppendPath(file), file);
                }

                // Create the  zip exporter
                zipExporter = new ZipExporter(diskPath.Path, FileLoadHelper, files);

                // Calculate all the zip steps
                zipExporter.CalculateSteps();

                for (int i = 0; i < zipExporter.TotalSteps; i++)
                {
                    Steps.Add(NextZipStep);
                }

                // Save the disk
                Steps.Add(SaveDisk);

                Steps.Add(CheckForErrors);

                Steps.Add(Cleanup);
            }
        }

        private void NextZipStep()
        {
            zipExporter.NextStep();

            // Update response from zip exporter
            Response["success"] = zipExporter.Response["success"];
            Response["message"] = zipExporter.Response["message"];

            StepCompleted();
        }

        private void CheckForErrors()
        {
            // Need to check for an error
            if ((bool) Response["success"] == false)
            {
                // Restore the old zip
                if (File.Exists(physicalBackupPath))
                {
                    // Rename the old zip back to its original name
                    File.Move(physicalBackupPath, physicalPath);
                }
            }

            StepCompleted();
        }

        private void BackupZip()
        {
            // Move the original file so we keep it safe
            if (File.Exists(physicalPath)) File.Move(physicalPath, physicalBackupPath);

            StepCompleted();
        }

        private void Cleanup()
        {
            // Restore the old zip
            if (File.Exists(physicalBackupPath))
            {
                // Delete the failed zip
                if (File.Exists(physicalBackupPath))
                {
                    File.Delete(physicalBackupPath);
                }
            }

            StepCompleted();
        }

        private void SaveDisk()
        {
            try
            {
                if ((bool) zipExporter.Response["success"])
                {
                    using (var fs = new FileStream(physicalPath, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(zipExporter.Bytes, 0, zipExporter.Bytes.Length);
                    }
                }
            }
            catch (Exception e)
            {
                // Change the success to false
                Response["success"] = false;
                Response["message"] = e.Message;
            }

            StepCompleted();
        }

        public override void Dispose()
        {
            base.Dispose();
            zipExporter.Dispose();
        }
    }
}