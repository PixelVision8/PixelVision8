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

using System;
using System.Collections.Generic;

namespace PixelVision8.Player
{

    public class PatternPicker : Picker
    {
        private List<SpriteData> _patterns;

        public PatternPicker(UIBuilder uiBuilder, int x = 0, int y = 0, int gridSize = 8, string spriteName = "patterns", string name = "", string tooltip = "", string selectSpriteName = "picker-selection", bool autoSize = true, DrawMode drawMode = DrawMode.Sprite, bool rebuildSpriteCache = true) : base(uiBuilder, x, y, gridSize, spriteName, name, tooltip, selectSpriteName, autoSize, drawMode, rebuildSpriteCache)
        {
            
        }

        public int[] CurrentPattern => _gameChip.Sprite(_patterns[_currentSelection].Id);

        protected override void DrawStates()
        {
            
            if(_metaSpriteId == -1)
                return;

            var metaSprite = _gameChip.MetaSprite(_metaSpriteId);
            _patterns = metaSprite.Sprites;

            var canvas = new Canvas(Width * 2 + 1, Height * 2 + 1);

            for (int i = 0; i < _patterns.Count; i++)
            {
                var patternPixels = _gameChip.Sprite(_patterns[i].Id);
                // TODO loop through patterns

                var pos = Utilities.CalculatePosition(i, _columns);

                canvas.SetPattern(patternPixels, 8 , 8);

                canvas.DrawRectangle((pos.X * 16), (pos.Y * 16), 17, 17, true);

            }

            // Scale up the grid
            _gridSize = 16;
            _columns = Width/_gridSize;


            Console.WriteLine("New Grid {0}", _gridSize);


            Width = canvas.Width;
            Height = canvas.Height;

            _cachedPixelData = new PixelData(canvas.Width, canvas.Height, canvas.Pixels);

        }


    }

}