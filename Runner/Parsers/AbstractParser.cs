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

namespace PixelVision8.Runner.Parsers
{
    public abstract class AbstractParser : IAbstractParser
    {
        protected List<Action> steps = new List<Action>();

        public int currentStep { get; protected set; }

        public virtual byte[] bytes { get; set; }

        public int totalSteps => steps.Count;

        public bool completed => currentStep >= totalSteps;

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