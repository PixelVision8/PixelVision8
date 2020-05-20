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

using PixelVision8.Engine.Chips;

namespace PixelVisionRunner.Games
{
    public class EmptyTemplateDemoChip : GameChip
    {
        private readonly string message =
            "EMPTY GAME\n\n\nThis is an empty game template.\n\n\nVisit 'www.pixelvision8.com' to learn more about creating games from scratch.";

        public override void Init()
        {
            var display = Display();

            // We are going to render the message in a box as tiles. To do this, we need to wrap the
            // text, then split it into lines and draw each line.
            var wrap = WordWrap(message, display.X / 8 - 2);
            var lines = SplitLines(wrap);
            var total = lines.Length;
            var startY = display.Y / 8 - total;

            // We want to render the text from the bottom of the screen so we offset it and loop backwards.
            for (var i = 0; i < total; i++) 
                DrawText(lines[i], 1, startY + (i - 1), DrawMode.Tile, "large", 15);
        }

        /// <summary>
        ///     The Draw() method is part of the game's life cycle. It is called after Update() and is where all of our
        ///     draw calls should go. We'll be using this to render sprites to the display.
        /// </summary>
        public override void Draw()
        {
            
            //We can use the RedrawDisplay() method to clear the screen and redraw the tilemap in a single call.
            RedrawDisplay();
        }

    }
}