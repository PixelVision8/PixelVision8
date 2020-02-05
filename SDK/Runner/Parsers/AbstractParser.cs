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
using PixelVision8.Runner.Services;

namespace PixelVision8.Runner.Parsers
{
    public abstract class AbstractParser : IAbstractParser
    {
        protected List<Action> steps = new List<Action>();

        public IFileLoadHelper FileLoadHelper;

        public int currentStep { get; protected set; }

        public string SourcePath;

        public virtual byte[] bytes { get; set; }

        public int totalSteps => steps.Count;

        public bool completed => currentStep >= totalSteps;

        public virtual void CalculateSteps()
        {
            currentStep = 0;

            // First step will always be to get the data needed to parse
            if(!string.IsNullOrEmpty(SourcePath))
                steps.Add(LoadSourceData);
        }

        public virtual void LoadSourceData()
        {
            if (FileLoadHelper != null)
            {
                bytes = FileLoadHelper.ReadAllBytes(SourcePath);
            }

            StepCompleted();
        }

        public virtual void NextStep()
        {
            if (completed) return;

            steps[currentStep]();
        }

        public virtual void StepCompleted()
        {
            currentStep++;
        }

        public virtual void Dispose()
        {
            bytes = null;
            FileLoadHelper = null;
            steps.Clear();
        }
    }
}