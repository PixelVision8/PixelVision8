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

    public partial class CollisionManager : UIComponent, IUpdate
    {
        
        public Entity CurrentFocus;

        public Entity Active;
        public Entity Hovered;
        public Entity CurrentDragSource;

        // Read only getters for the mouse x,y and column,row position
        public int MouseX => _mousePos.X;
        public int MouseY => _mousePos.Y;
        public int MouseColumn => _mousePos.Width;
        public int MouseRow => _mousePos.Height;
        
        // Going to use a rect to store the mouse x, y, column, and row
        private Rectangle _mousePos = new Rectangle();

        public bool MouseDown;
        public bool MouseReleased;

        public Entity LastHovered;
        public Entity LastActive;

        // Read only getters for the last mouse x,y and column,row position
        public int LastMouseX => _lastMousePos.X;
        public int LastMouseY => _lastMousePos.Y;
        public int LastMouseColumn => _lastMousePos.Width;
        public int LastMouseRow => _lastMousePos.Height;
        
        // Going to use a rect to store the mouse x, y, column, and row
        private Rectangle _lastMousePos = new Rectangle();

        public bool LastMouseDown;
        public bool LastMouseReleased;

        public string Tooltip;
        public bool OverrideMessage;

        private List<Entity> DragTargets = new List<Entity>();

        public Point ScrollPosition => _gameChip.ScrollPosition();
        public bool IgnoreScrollPos = false;
        
        public CollisionManager(UIBuilder uiBuilder):base(uiBuilder)
        {
        }

        public void Update(int timeDelta)
        {
            
            // Save last mouse state values
            LastMouseDown = MouseDown;
            LastMouseReleased = MouseReleased;
            LastHovered = Hovered;
            LastActive = Active;
            _lastMousePos.X = _mousePos.X;
            _lastMousePos.Y = _mousePos.Y;
            _lastMousePos.Width = _mousePos.Width;
            _lastMousePos.Height = _mousePos.Height;

            // Get the current mouse position
            var mousePointer = _gameChip.MousePosition();

            // Save the current mouse state
            _mousePos.X = mousePointer.X;
            _mousePos.Y = mousePointer.Y;
            _mousePos.Width = (int)Math.Floor((float)_mousePos.X / Constants.SpriteSize);
            _mousePos.Height = (int)Math.Floor((float)_mousePos.Y / Constants.SpriteSize);
            MouseDown = _gameChip.MouseButton(0, InputState.Down);
            MouseReleased = _gameChip.MouseButton(0, InputState.Released);

            if(CurrentDragSource != null)
            {
                // TODO need to port over drag logic
            }
            
        }

        public bool MouseInRect(Rectangle hitRect, int offsetX = 0, int offsetY = 0) => hitRect.Contains(_mousePos.X - offsetX, _mousePos.Y - offsetY);

    }
}