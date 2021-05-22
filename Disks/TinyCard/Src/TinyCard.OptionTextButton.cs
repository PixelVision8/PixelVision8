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
using Microsoft.Xna.Framework.Input;

namespace PixelVision8.Player
{

    public class OptionTextButton : TextButton
    {
        private static TextButtonStyle _defaultStyle = new TextButtonStyle()
        {
            Padding = new Rectangle(2, 1, 2, 2),
            StateColors = new Dictionary<InteractiveStates, int[]>
            {
                {InteractiveStates.Disabled, new int[]{0, 1, 0, 1}},
                {InteractiveStates.Up, new int[]{0, 1, 0, 1}},
                {InteractiveStates.Over, new int[]{1, 0, 0, 1}},
            }
        };

        public Shortcut? Shortcut = null;

        public bool isDivder = false;

        public OptionTextButton(
            UIBuilder editorUI, 
            string text, 
            string tooltip = "",
            Shortcut? shortcut = null, 
            int width = 0,
            bool isDivider = false) 
        : base(editorUI: editorUI, text: text, tooltip: tooltip)
        {

            if(shortcut.HasValue)
                Shortcut = shortcut.Value;

            Width = (int)Math.Ceiling(width/8f) * 8 + 2;

            if(text.ToLower() == "divider" || isDivder)
            {
                isDivder = true;

                Height = 4;
                Enable(false);
            }
            
            _defaultStyle.ApplyStyle(this);
            
        }

        public override void Update(int timeDelta)
        {
            if(Enabled == false)
            {

                if(InFocus)
                {
                    OnLoseFocus();
                }

                return;
            }

            CurrentState = InteractiveStates.Up;

            if(_collisionManager.MouseInRect(HitRect, _rect.X, _rect.Y))
            {

                // Modify the state based on the state of the mouse
                CurrentState = _collisionManager.MouseDown || _collisionManager.MouseReleased ? InteractiveStates.Down : InteractiveStates.Over;

                OnFocus();

                // Reset the state if it needs to be used during draw
                CurrentState = InteractiveStates.Up;
            }
            else if(InFocus)
            {
                OnLoseFocus();
            }
        }

        protected override void DrawState(Canvas canvas, InteractiveStates state, int x, int y, int colorOffset = 0)
        {
            if(isDivder)
            {
                canvas.SetStroke(1, 1);
                canvas.SetPattern(
                    new int[]{
                        1, 1, 1,
                        1, 0, 1,
                        1, 1, 1
                    }, 3, 3);

                canvas.DrawRectangle(x + 3, y + 1, Width - 4, Height -1 , true);
            }
            else
            {
            
                base.DrawState(canvas, state, x, y, colorOffset);

                if(Shortcut.HasValue && state != InteractiveStates.Disabled)
                {

                    var bgColor = StateColors[state][2];
                    var textColor = StateColors[state][3];

                    // TODO need to use the shortcut's modify and key plus add that to the tooltip text
                    var label = "";

                    if(Shortcut.Value.Shift)
                        label += ")";

                    if(Shortcut.Value.Control)
                        label += "!";

                    // TODO cleanup some characters
                    switch(Shortcut.Value.Key)
                    {
                        case Keys.NumPad1:
                            label += "1";
                        break;
                        case Keys.NumPad2:
                            label += "2";
                        break;
                        case Keys.Up:
                            label += "#";
                        break;
                        case Keys.Down:
                            label += "$";
                        break;
                        case Keys.Right:
                            label += "&";
                        break;
                        case Keys.Left:
                            label += "%";
                        break;
                        case Keys.Home:
                            label += "HOME";
                        break;
                        case Keys.End:
                            label += "END";
                        break;
                        default:
                            label += Shortcut.Value.Key.ToString();
                        break;

                    }

                    var boxWidth = (label.Length * 5) + 1;

                    if(label.Length == 1)
                        boxWidth -= 1;
                    else if(label.Length == 3)
                        boxWidth += 1;
                    
                    var offsetX = x + _textBounds.X + Width - boxWidth - 9;
                    var offsetY = y + _textBounds.Y;

                    canvas.SetStroke(-1, -1);
                    canvas.SetPattern(new int[]{bgColor}, 1, 1);

                    
                        canvas.DrawRectangle(offsetX - 2 , y + 1, boxWidth + 4, 9, true);
                    
                    
                        canvas.DrawText(label.ToUpper(), offsetX, offsetY, "menu", textColor, -1);

                }

            }

            if(state == InteractiveStates.Disabled && isDivder == false)
            {
                
                canvas.SetStroke(-1, 0);
                canvas.SetPattern(
                    new int[]{
                        1, 1,
                        -1, 1,
                        
                    }, 2, 2);

                canvas.DrawRectangle(x, y, Width, Height , true);
            }

        }
    }

}