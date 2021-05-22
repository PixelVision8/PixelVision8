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

using System.Collections.Generic;
using System.Linq;

namespace PixelVision8.Player
{

    public class TextButtonStyle
    {
        
        public Rectangle Padding = new Rectangle(2, 0, 2, 1);
        public string Font = "medium";
        public int Spacing = -4;
        // public Alignment IconAlignment = Alignment.Left;
        public string IconSpriteName = string.Empty;

        public bool HitRectFill = true;

        public Alignment IconAlignment = Alignment.Left;

        public Dictionary<InteractiveStates, int[]> StateColors = new Dictionary<InteractiveStates, int[]>
        {
            {InteractiveStates.Disabled, new int[]{0, 1}},
            {InteractiveStates.Up, new int[]{0, 1}},
            {InteractiveStates.Down, new int[]{1, 0}},
            {InteractiveStates.SelectedUp, new int[]{1, 0}},
        };
        
        public void ApplyStyle(TextButton textButton)
        {
            
            textButton.Padding = new Rectangle(Padding.X, Padding.Y, Padding.Width, Padding.Height);
            textButton.Font = Font;
            textButton.Spacing = Spacing;
            textButton.HitRectFill = HitRectFill;

            textButton.IconAlignment = IconAlignment;

            if(IconSpriteName != string.Empty)
                textButton.SpriteName = IconSpriteName;

            if(StateColors != null)
            {
                // Clone the state colors
                textButton.StateColors = StateColors.ToDictionary(
                    x => x.Key,
                    x => x.Value 
                );
            }

            textButton.ApplyStyle();
        }

    }

}