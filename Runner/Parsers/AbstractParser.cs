//   
// Copyright (c) Jesse Freeman. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) License. 
// See LICENSE file in the project root for full license information. 
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman - @JesseFreeman
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany

using System;
using System.Collections.Generic;

namespace PixelVisionRunner.Parsers
{
    public abstract class AbstractParser : IAbstractParser
    {
        protected List<Action> steps = new List<Action>();

        public int currentStep { get; protected set; }

        public int totalSteps
        {
            get { return steps.Count; }
        }

        public bool completed
        {
            get { return currentStep >= totalSteps; }
        }

        public virtual void CalculateSteps()
        {
            currentStep = 0;
        }

        public virtual void NextStep()
        {
            if (completed)
                return;

            steps[currentStep]();
        }
    }
}