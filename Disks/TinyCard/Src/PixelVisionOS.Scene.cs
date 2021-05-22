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

    public partial class Scene : EntityManager
    {

        // Flag to keep track of a scene's first time running incase there is special code that needs to execute
        protected bool _firstRun = true;

        // Flag to reset the first run flag when a scene is deactivated making sure it resets on next activate
        public bool AutoReset;

        public Scene(UIBuilder uiBuilder, string name, Rectangle rect = new Rectangle(), List<Entity> entities = null) : base(uiBuilder, name, rect.X, rect.Y, entities, autoResize: false)
        {
            // Set the default size of the scene to the screen's dimensions
            Width = Display().X;
            Height = Display().Y;
        }

        public virtual void Activate()
        {
            
            // Reset the scroll position to 0,0 when a scene is activated
            ScrollPosition(0, 0);

            // Invalidate the scene and any children entities
            Invalidate();

            // Set first run flag to false
            _firstRun = false;

        }

        public virtual void Deactivate()
        {
            
            // Set first run back to true if auto reset is true
            if(AutoReset)
                _firstRun = true;

        }

        public void Follow(Entity entity = null)
        {
            // TODO force the camera follow an entity
        }

        public void ScrollTo(int x, int y, int speed = 10, Action callback = null)
        {
            // TODO move the camera to a specific position and trigger a callback
        }

        public override void Update(int timeDelta)
        {
            
            
            base.Update(timeDelta);

            // TODO Update camera

        }
    }

}