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

using PixelVision8.Player;
using PixelVision8.Runner.Exporters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace PixelVision8.Runner
{
    public abstract class BaseExportService : AbstractService
    {
        protected readonly List<IExporter> exporters = new List<IExporter>();
        protected int currentParserID;
        protected int currentStep;
        protected bool exporting;
        protected BackgroundWorker exportWorker;
        public Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();
        protected Dictionary<string, object> message = new Dictionary<string, object>();
        protected int totalParsers => exporters.Count;
        public int totalSteps;
        public bool completed => currentParserID >= totalParsers;
        protected float percent => currentStep / (float) totalSteps;

        public virtual bool IsExporting()
        {
            return exporting;
        }

        // TODO need to make this work like loading does
        public virtual int ReadExportPercent()
        {
            return (int) (percent * 100);
        }

        public virtual Dictionary<string, object> ReadExportMessage()
        {
            return message;
        }

        public virtual void AddExporter(IExporter exporter)
        {
            // Calculate the steps for the exporter
            exporter.CalculateSteps();

            exporters.Add(exporter);

            totalSteps += exporter.TotalSteps;
        }

        public virtual void StartExport(bool useSteps = true)
        {
            Restart();

            if (useSteps == false)
            {
                ExportAll();
            }
            else
            {
                exportWorker = new BackgroundWorker
                {
                    // TODO need a way to of locking this.

                    WorkerSupportsCancellation = true,
                    WorkerReportsProgress = true
                };

                exportWorker.DoWork += WorkerExportSteps;
                //            bgw.ProgressChanged += WorkerLoaderProgressChanged;
                exportWorker.RunWorkerCompleted += WorkerExporterCompleted;

                //            bgw.WorkerReportsProgress = true;
                exportWorker.RunWorkerAsync();

                exporting = true;
            }
        }

        protected virtual void WorkerExportSteps(object sender, DoWorkEventArgs e)
        {
            var total = totalSteps; //some number (this is your variable to change)!!

            for (var i = 0; i <= total; i++) //some number (total)
            {
                try
                {
                    NextExporter();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    // throw;
                }

                Thread.Sleep(1);
                exportWorker.ReportProgress((int) (percent * 100), i);
            }
        }

        protected virtual void WorkerExporterCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (locator.GetService(typeof(WorkspaceService).FullName) is WorkspaceService workspaceService)
            {
                // Aggregate all Get all the messages
                foreach (var exporter in exporters)
                {
                    if (exporter.Bytes != null)
                    {
                        files.Add(exporter.fileName, exporter.Bytes);
                    }

                    foreach (var response in exporter.Response)
                    {
                        var tmpKey = exporter.GetType().Name + "_" + response.Key;
                        var tmpValue = response.Value;

                        if (message.ContainsKey(tmpKey))
                            message[tmpKey] = tmpValue;
                        else
                            message.Add(tmpKey, tmpValue);
                    }
                }

                workspaceService.SaveExporterFiles(files);

                files.Clear();
            }

            exporting = false;
        }

        #region Main APIs

        public virtual void ExportAll()
        {
            while (completed == false) NextExporter();

            WorkerExporterCompleted(null, null);
        }


        public virtual void NextExporter()
        {
            if (completed) return;

            var parser = exporters[currentParserID];

            parser.NextStep();

            currentStep++;

            if (parser.Completed)
            {
                currentParserID++;
            }
        }

        public virtual void Restart()
        {
            currentParserID = 0;
            currentStep = 0;
            message.Clear();
        }

        public virtual void Clear()
        {
            exporters.Clear();
            files.Clear();
            totalSteps = 0;
        }

        public virtual void Cancel()
        {
            currentStep = totalSteps;
        }

        #endregion
    }
}