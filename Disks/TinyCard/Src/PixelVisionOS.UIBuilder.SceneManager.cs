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
    public partial class UIBuilder
    {

        private Dictionary<string, Scene> _scenes = new Dictionary<string, Scene>();

        public Scene ActiveScene;

        public Scene CreateScene(string name, Rectangle rect, List<Entity> entities = null)
        {
            var scene = new Scene(this, name, rect, entities);

            scene.Width = rect.Width == 0 ? GameChip.Display().X : 0;
            scene.Height = rect.Height == 0 ? GameChip.Display().Y : 0;

            AddScene(scene);

            return scene;
        }

        public void AddScene(Scene scene)
        {
            if(_scenes.ContainsKey(scene.Name))
            {
                _scenes[scene.Name] = scene;
            }
            else
            {
                _scenes.Add(scene.Name, scene);
            }
            // TODO need to make sure there isn't a scene with the same name first

            // Add scene
        }

        public bool RemoveScene(string name)
        {
            if(_scenes.ContainsKey(name))
            {
                _scenes.Remove(name);
                return true;
            }

            return false;
            // Look for a scene with the same name
        }

        public void SwitchScene(string name)
        {
            
            if(_scenes.ContainsKey(name))
            {

                if(ActiveScene != null)
                {
                    ActiveScene.Deactivate();
                    RemoveUI(ActiveScene.Name);
                }
            
                var scene = _scenes[name];
                scene.Activate();

                AddUI(scene);

            }

        }

        public void CameraFollow(Entity entity = null)
        {
            // TODO add logic for the camera to follow an entity in a scene
        }

        public void ScrollTo(int x, int y, Action callback = null)
        {
            // TODO stop the camera from following

            // Scroll to a point in the scene

            // Trigger callback when point is reached
        }

    }


}