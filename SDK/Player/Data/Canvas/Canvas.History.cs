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

namespace PixelVision8.Player
{

    public sealed partial class Canvas
    {

        private List<int[]> states = new List<int[]>();

        private int stateId = 0;

        public int SaveState()
        {
            
            states.Add(GetPixels());
            Console.WriteLine("Save State " + states.Count);

            return states.Count - 1;
        }

        public void RestoreState(int id)
        {
            // var lastIndex = states.Count - 1;

            // if(lastIndex < 0)
            //     return;

            SetPixels(states[id]);

            // states.RemoveAt(id);
            
            Console.WriteLine("Restore State " +  id + " " + states.Count);
        }

        public void Undo()
        {
            stateId --;
        }

        public void Redo()
        {

        }

    }

}