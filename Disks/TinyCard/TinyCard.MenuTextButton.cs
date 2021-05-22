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

    public class MenuTextButton : TextButton
    {
        public MenuTextButton(UIBuilder editorUI, string text, int x = 0, int y = 0, string tooltip = "", string spriteName = "", Rectangle padding = default, string font = "medium", int spacing = -4, Alignment iconAlignment = Alignment.Left, bool hitRectFill = true, Dictionary<InteractiveStates, int[]> stateColors = null) : base(editorUI, text, x, y, tooltip, spriteName, padding, font, spacing, iconAlignment, hitRectFill, stateColors)
        {
            // Create a menu text button style
            var menuStyle = new TextButtonStyle()
            {
                Padding = new Rectangle(3, 0, 5, 1),
                // StateColors = new Dictionary<InteractiveStates, int[]>
                // {
                //     {InteractiveStates.Up, new int[]{0, 1}},
                //     {InteractiveStates.Down, new int[]{1, 0}},
                // }
                StateColors = new Dictionary<InteractiveStates, int[]>
                {
                    {InteractiveStates.Disabled, new int[]{0, 1}},
                    {InteractiveStates.Up, new int[]{0, 1}},
                    {InteractiveStates.Over, new int[]{3, 1}},
                    {InteractiveStates.Down, new int[]{1, 0}},
                    {InteractiveStates.SelectedUp, new int[]{1, 0}},
                    {InteractiveStates.SelectedOver, new int[]{1, 3}},
                }
                
            };

            // Apply the menu style
            menuStyle.ApplyStyle(this);
            
        }

        public override void Update(int timeDelta)
        {
            // if(Enabled == false)
            // {

            //     if(InFocus)
            //     {
            //         OnLoseFocus();
            //     }

            //     return;
            // }

            // if(_collisionManager.MouseDown && InFocus == false)
            // {
            //     // Invalidate();
            //     return;
            // }

            CurrentState = InteractiveStates.Up;

            if(_collisionManager.MouseInRect(HitRect, _rect.X, _rect.Y)   || (InFocus && _collisionManager.MouseDown))
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
    }

    // TODO need to overwrite update logic to support custom menu logic

}