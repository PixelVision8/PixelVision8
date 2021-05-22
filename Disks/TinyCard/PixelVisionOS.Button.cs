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

namespace PixelVision8.Player
{

    public partial class Button : UIEntity
    {
        
        public Action<Button> OnPress { get; set; }
        public Action<Button> OnRelease { get; set; }
        public Action<Button> OnReleaseOutside { get; set; }
        public bool DoubleClickActive { get; set; }
        public bool DoubleClick { get; set; }
        public int DoubleClickTime { get; set; }
        public int DoubleClickDelay { get; set; }
        public Cursors ButtonCursor { get; set; } = Cursors.Hand;
        public bool RedrawBackground { get; set; } = false;
        public int BGColorOverride { get; set; }
        public bool Selected { get; set; }
        public bool FirstPressed { get; set; } = true;

        protected override string GenerateUniqueName()
        {
            return "Button" + base.GenerateUniqueName();
        }
        
        public Button(
            UIBuilder editorUI,
            string name,
            int x = 0,
            int y = 0,
            int width = 0,
            int height = 0,  
            string spriteName = "", 
            string tooltip = "", 
            bool autoSize = true, 
            DrawMode drawMode = DrawMode.TilemapCache,
            bool rebuildSpriteCache = true
        ):base(editorUI, new Rectangle(x, y, width ,height), spriteName, name, tooltip, autoSize, drawMode, rebuildSpriteCache)
        {

        }

        protected override void OnFocus()
        {

            // Modify the state based on if the button has been selected 
            if(Selected)
            {
                if(CurrentState == InteractiveStates.Up)
                    CurrentState = InteractiveStates.SelectedUp;

                if(CurrentState == InteractiveStates.Over)
                    CurrentState = InteractiveStates.SelectedOver;
            }

            base.OnFocus();

            if(_collisionManager.MouseReleased)
            {
                ReleaseButton();
            }
            else if(_collisionManager.MouseDown)
            {
                PressButton();
            }
        }

        protected override void OnLoseFocus()
        {
            base.OnLoseFocus();

            FirstPressed = true;

            ReleaseOutsideButton();

        }

        public override void Draw()
        {
            if(Invalid == false)
                return;

            // Make sure we always start with the up state
            CurrentState = InteractiveStates.Up;

            // Check to see if the button has been selected
            if(Selected)
            {
                if(CurrentState == InteractiveStates.Up)
                    CurrentState = InteractiveStates.SelectedUp;

                if(CurrentState == InteractiveStates.Down)
                    CurrentState = InteractiveStates.SelectedDown;
                
                if(CurrentState == InteractiveStates.Over)
                    CurrentState = InteractiveStates.SelectedOver;
            }

            if(Enabled == false && Selected == false)
            {
                CurrentState = InteractiveStates.Disabled;
            }

            base.Draw();       
        }

        public virtual void PressButton(bool callAction = true)
        {
            if(FirstPressed == false)
                return;

            if(OnPress != null && callAction)
                OnPress(this);

            // Set first press to false
            FirstPressed = false;
        }

        public virtual void ReleaseOutsideButton()
        {

            if(_collisionManager.MouseReleased && !FirstPressed && OnReleaseOutside != null)
            {
                OnReleaseOutside(this);
            }

        }

        public virtual void ReleaseButton(bool callAction = true)
        {

            // Only trigger if this is being released for the first time - stops multiple triggers
            if(FirstPressed == true)
            {
                return;
            }

            // Reset first pressed
            FirstPressed = true;

            // Reset double click timer
            DoubleClickTime = 0;
            DoubleClickActive = true;
            DoubleClick = true;

            // Look for a release callback
            if(OnRelease != null && callAction)
                OnRelease(this);
        }

        public virtual void Select(bool value, bool autoDisable = true)
        {
            
            if(Selected == value)
                return;

            Selected = value;

            Invalidate();

            if(autoDisable)
                Enable(!value);

        }

    }

}