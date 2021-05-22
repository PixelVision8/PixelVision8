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

    // Wrapper for all modals that can open, close and manage UI states
    public class ModalPanel : EntityManager
    {
        private bool _isOpen;
        public bool IsOpen => _isOpen;
        public Action OnOpen;
        public Action OnClose;
        private bool _opening;

        public ModalPanel(UIBuilder uiBuilder, string name, Rectangle rect, List<Entity> entities = null ): base(uiBuilder: uiBuilder, name: name, x: rect.X, y: rect.Y, entities: entities) 
        {
            Width = rect.Width;
            Height = rect.Height;
        }

        public virtual void Open(bool trigger = true)
        {
            
            if(_isOpen)
                return;

            _uiBuilder.OpenModal(this, false);

            _isOpen = true;

            Pause = false;

            // Invalidate all entities
            Invalidate();

            if(OnOpen != null && trigger)
                OnOpen();

            _opening = true;
        }

        public virtual void Close(bool trigger = true)
        {

            if(_isOpen == false)
                return;

            _uiBuilder.CloseModal(Name, false);

            _isOpen = false;

            Pause = true;

            if(OnClose != null && trigger)
                OnClose();
        }

        public override void Update(int timeDelta)
        {
            
            if(_opening == false && _isOpen && _uiBuilder.CollisionManager.MouseDown && !Rect.Contains(_uiBuilder.CollisionManager.MouseX, _uiBuilder.CollisionManager.MouseY))
            {
                Close();
                return;
            }

            base.Update(timeDelta);

            // Test to see if the menu is still opening and the mouse hasn't been released (still on trigger that opened it)
            if(_opening && _uiBuilder.CollisionManager.MouseReleased)
                _opening = false;

        }

        public override void Draw()
        {
            if(Invalid)
            {
                DrawBackground();
            }

            base.Draw();
        }

        public virtual void DrawBackground()
        {
            Console.WriteLine("{0} Redraw background {1} {2}", Name, Width, Height);
        }

    }

}