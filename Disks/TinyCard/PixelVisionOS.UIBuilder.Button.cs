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

namespace PixelVision8.Player
{

    public partial class UIBuilder{
        
         public Button CreateButton(string name ="", int x = 0, int y = 0, int width = 0, int height = 0, string spriteName = "", string tooltip = "", bool autoManage = true)
        {
            var tmpButton = new Button(this, name, x, y, width, height, spriteName, tooltip);

            if(autoManage)
                AddUI(tmpButton);

            return tmpButton;
        }

        public Button CreateEmptyButton(int x = 0, int y = 0, int width = 0, int height = 0, string tooltip = "")
        {

            var button = CreateButton(x: x, y: y, width: width, height: height, tooltip: tooltip);
            button.Name = "Empty" + button.Name;
            return button;
        }

        public Button CreateSpriteButton(string spriteName, int x = 0, int y = 0,  string tooltip = "")
        {
            var button = CreateButton(spriteName: spriteName, x: x, y: y, tooltip: tooltip);
            button.Name = "Sprite" + button.Name;
            return button;
        }

    }
}