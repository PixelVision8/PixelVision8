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
using System.Linq;
using System.Reflection;

namespace PixelVision8.Player
{

    // The base data object that represents all UI elements
    public class UIComponentAttribute : Attribute {}

    public partial class UIComponent
    {
        protected UIBuilder _uiBuilder;
        protected GameChip _gameChip => _uiBuilder.GameChip;

        public UIComponent(UIBuilder uiBuilder)
        {
            
            _uiBuilder = uiBuilder;
        }
    }

    // TODO maybe this should be a chip?

    // The UI factory responsible for creating and updating UI elements
    public partial class UIBuilder : IUpdate, IDraw
    {

        protected List<IUpdate> Updatable = new List<IUpdate>();
        protected List<IDraw> Drawable = new List<IDraw>();

        public GameChip GameChip;
        protected int _timeDelta;

        public int TimeDelta => _timeDelta;

        public IInteractive InFocusUI { get; private set; }
        public IInteractive LastInFocusUI { get; private set; }

        public void RegisterComponent(object component)
        {

            if(component is IUpdate)
                Updatable.Add(component as IUpdate);

            if(component is IDraw)
                Drawable.Add(component as IDraw);

        }
        
        public UIBuilder(GameChip gameChip)
        {

            GameChip = gameChip;
            
            // Get all of the UI Components
            var methods = GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(UIComponentAttribute), false).Length > 0).ToArray();

            // Loop through all of the UI Components and update them
            for (int i = 0; i < methods.Length; i++)
            {
                
                // Get the method
                Type thisType = this.GetType();
                MethodInfo theMethod = thisType.GetMethod(methods[i].Name);

                // Call the method
                theMethod.Invoke(this, new object[]{});

            }

        }

        public void Update(int timeDelta)
        {

            _timeDelta = timeDelta;

            foreach (var item in Updatable)
            {
                item.Update(timeDelta);
            }

        }

        public void Draw()
        {
            foreach (var item in Drawable)
            {
                item.Draw();
            }
        }

        public void ClearFocus(IInteractive entity = null)
        {
            
            LastInFocusUI = InFocusUI;

            CursorID = Cursors.Pointer;
            
            if(entity!= null && entity.InFocus)
            {
                InFocusUI = null;
            }

        }

        public void SetFocus(IInteractive entity, Cursors overrideCursor = Cursors.Hand)
        {

            if(CollisionManager.MouseReleased == true)
                return;

            CursorID = overrideCursor;

            if(entity.InFocus == true)
                return;

            if(CollisionManager.MouseDown && InFocusUI != null)
            {
                if(InFocusUI.Name != entity.Name)
                    return;
            }

            InFocusUI = entity;
            
        }

    }
}