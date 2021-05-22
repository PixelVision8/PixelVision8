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

    public class ToggleGroup : EntityManager
    {
        public bool SingleSelection;
        public List<int> Selections = new List<int>();
        public Action<Button> OnPress { get; set; }
        public Action<Button> OnRelease { get; set; }

        public ToggleGroup(UIBuilder uiBuilder, string name = "", int x = 0, int y = 0, bool singleSelection = true, List<Entity> entities = null, bool autoResize = true) : base(uiBuilder, name, x, y, entities, autoResize)
        {
            SingleSelection = singleSelection;
        }

        public override void Add(IEntity entity)
        {

            // Attempt to cast the entity as a button
            var btn = entity as Button;

            // Make sure we only accept buttons
            if(btn == null)
                return;
            
            base.Add(entity);

            // Remap the button action
            btn.OnRelease = (Button target)=> SelectToggleButton(FindEntityId(target.Name));

            btn.OnPress = (Button target)=> PressToggleButton(FindEntityId(target.Name));
            
        }

        protected virtual int FindEntityId(string name)
        {
            for (int i = 0; i < _total; i++)
            {
                if(_entities[i].Name == name)
                    return i;
            }

            return -1;
        }

        public void PressToggleButton(int id, bool trigger = true)
        {
            
            if(id < -1 || id >= _total)
                return;

            var btn = _entities[id] as Button;

            if(btn != null)
            {
               
                if(trigger && OnPress != null)
                {
                    OnPress(btn);
                }

            }

        }

        public void SelectToggleButton(int id, bool trigger = true)
        {
            
            if(id < -1 || id >= _total)
                return;

            ClearSelections();
            
            var btn = _entities[id] as Button;

            if(btn != null)
            {
                btn.Select(true, SingleSelection);

                Selections.Add(id);

                if(trigger && OnRelease != null)
                {
                    OnRelease(btn);
                }

            }
            
        }

        public void ClearSelections()
        {
            for (int i = 0; i < _total; i++)
            {
                // Get the next button
                var btn = _entities[i] as Button;

                // If the button is not null and selected, trigget the selection
                if(btn != null)
                    btn.Select(false);
            }

            Selections.Clear();
        }


    }

}