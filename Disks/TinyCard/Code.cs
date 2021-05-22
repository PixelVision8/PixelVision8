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

namespace PixelVision8.Player
{

    public class TinyCardGame : GameChip
    {
        // Store a reference to the UI Builder
        UIBuilder uiBuilder;

        private ModalPanel toolModal;

        public override void Init()
        {
            
            // TODO need to look into why this is not working correctly
            // Need to fix colors (Not displaying correctly when loading from colors.png)
            Color(0, "#2d1b2e");
            Color(1, "#f9f4ea");
            Color(2, "#ff0000");

            // Create a new UI Builder instance with a reference to the Game Chip
            uiBuilder = new UIBuilder(this);

            // Add the new scenes to the UI Builder
            // uiBuilder.AddScene(new SceneUITest(uiBuilder));
            uiBuilder.AddScene(new SceneStackEditor(uiBuilder));

            // Switch to the default scene
            uiBuilder.SwitchScene(SceneStackEditor.STACK_EDITOR);

        }

        public override void Update(int timeDelta)
        {
            uiBuilder.Update(timeDelta);

        }

        /// <summary>
        ///     Draw() is called once per frame after the Update() has completed. This is where all visual updates to
        ///     your game should take place such as clearing the display, drawing sprites, and pushing raw pixel data
        ///     into the display.
        /// </summary>
        public override void Draw()
        {

            //Redraw the display
            RedrawDisplay();

            uiBuilder.Draw();

        }

    }
}