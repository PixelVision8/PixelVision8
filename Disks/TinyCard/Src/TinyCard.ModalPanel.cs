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

namespace PixelVision8.Player
{

    // Wrapper for all modals that can open, close and manage UI states
    public class TinyCardModalPanel : ModalPanel
    {
        protected Rectangle _offsets = new Rectangle(-1, 0, 2, 2);
        protected Canvas canvas;

        public TinyCardModalPanel(UIBuilder uiBuilder, string name, Rectangle? offsets = null, List<Entity> entities = null) : base(uiBuilder, name, new Rectangle(), entities)
        {
            canvas = new Canvas(1,1, _gameChip);
            if(offsets.HasValue)
                _offsets = offsets.Value;
        }

        public override void DrawBackground()
        {

            if(canvas.Width != (Width + _offsets.Width) || canvas.Height != (Height + _offsets.Height))
            {
                
                canvas.Resize((Width + _offsets.Width), (Height + _offsets.Height));
                // canvas.Clear(3);

                // Create white outline and fill background
                canvas.SetStroke(1, 1);
                canvas.SetPattern(new int[]{1}, 1, 1);
                canvas.DrawRectangle(0,0, canvas.Width, canvas.Height-1, true);
                canvas.DrawLine(1,canvas.Height-1,canvas.Width-1,canvas.Height-1);

                // Draw inner outline
                canvas.SetStroke(0, 1);
                canvas.DrawRectangle(1,0, canvas.Width-3, canvas.Height-2);

                // Draw shadow
                canvas.SetStroke(0, 1);
                canvas.DrawLine(2,canvas.Height-2,canvas.Width -2,canvas.Height-2);
                canvas.DrawLine(canvas.Width-2, 1, canvas.Width-2, canvas.Height- 2);

                CustomBackground();
                
            }
            
            canvas.DrawPixels(X + _offsets.X, Y + _offsets.Y);
            
        }

        protected virtual void CustomBackground()
        {
            // Override with any custom draw logic
        }

    }

}