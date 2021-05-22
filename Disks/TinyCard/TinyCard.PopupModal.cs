//   
// Copyright (c) Jesse Freeman, Tiny Card. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by TinyCard are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Tiny Card contributors:
//  
// Jesse Freeman - @JesseFreeman
//

using System;
using System.Collections.Generic;

//   
// Copyright (c) Jesse Freeman, Tiny Card. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by TinyCard are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Tiny Card contributors:
//  
// Jesse Freeman - @JesseFreeman
//

namespace PixelVision8.Player
{

    // Wrapper for all modals that can open, close and manage UI states
    public class TinyCardPopupModal : TinyCardModalPanel
    {
        
        public TinyCardPopupModal(UIBuilder uiBuilder, string name, Rectangle? offsets = null, List<Entity> entities = null) : base(uiBuilder, name, offsets.HasValue ? offsets.Value : new Rectangle(-8, -8, 18, 18), entities)
        {
            
        }

        protected override void CustomBackground()
        {

            canvas.SetStroke(0, 2);
            canvas.DrawRectangle(3,2, canvas.Width-7, canvas.Height-6);
            
        }

        public override void Open(bool trigger = true)
        {
            base.Open(trigger);

            // Center
            X = (int)((_gameChip.Display().X - Width) * .5f);
            Y = (int)((_gameChip.Display().Y - Height) * .5f);
        }

    }

}