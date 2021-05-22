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

namespace PixelVision8.Player
{

    public partial class UIBuilder
    {

        Dictionary<string, ILayout> _cachedLayouts = new Dictionary<string, ILayout>();
        
        public ToggleGroup CreateToggleGroup(string name = "", int x = 0, int y = 0, bool singleSelection = true, List<Entity> entities = null, bool autoManage = true)
        {

            var toggleGroup = new ToggleGroup(this, name, x, y, singleSelection, entities);

            if(autoManage)
                AddUI(toggleGroup);

            return toggleGroup;
        }

        public ToggleGroup CreateVerticalToggleGroup(string name = "", int x = 0, int y = 0, bool singleSelection = true, List<Entity> entities = null, bool autoManage = true)
        {

            var toggleGroup = CreateToggleGroup(name, x, y, singleSelection, entities, autoManage);

            toggleGroup.Layout =  EntityLayout.Vertical;

            return toggleGroup;

        }

        public ToggleGroup CreateHorizontalToggleGroup(string name = "", int x = 0, int y = 0, bool singleSelection = true, List<Entity> entities = null, bool autoManage = true)
        {

            var toggleGroup = CreateToggleGroup(name, x, y, singleSelection, entities, autoManage);

            toggleGroup.Layout =  EntityLayout.Horizontal;

            return toggleGroup;

        }

        public ToggleGroup CreateGridToggleGroup(string name = "", int x = 0, int y = 0, int columns = 1, bool singleSelection = true, List<Entity> entities = null, bool autoManage = true)
        {

            var toggleGroup = CreateToggleGroup(name, x, y, singleSelection, entities, autoManage);
            
            var layout = EntityLayout.Grid;
            layout.Columns = columns;

            toggleGroup.Layout =  layout;

            return toggleGroup;

        }

        public TextButton CheckBoxButton(string text, int x = 0, int y = 0, string tooltip = "", bool autoManage = false)
        {

            var textButton = IconTextButton(text, "checkbox", x, y, tooltip, applyStyle: false);

            // Make sure default style exists first
            var style = TextButtonStyles.CheckBox;

            if(_cachedTextButtonStyles.ContainsKey(style) == false)
            {
                
                var colors = new Dictionary<InteractiveStates, int[]>
                {
                    {InteractiveStates.Up, new int[]{0, 1}},
                    {InteractiveStates.SelectedUp, new int[]{0, 1}},
                };

                _cachedTextButtonStyles[style] = new TextButtonStyle()
                {
                    Padding = new Rectangle(2, 0, 2, 0),
                    StateColors = colors,
                    HitRectFill = false
                };
            }

            _cachedTextButtonStyles[style].ApplyStyle(textButton);

            if(autoManage)
                AddUI(textButton);

            return textButton;
        }

    }

}