
using System;
using System.Collections.Generic;
using System.Linq;

namespace PixelVision8.Player
{
    public enum TextButtonStyles
    {
        Label,
        IconButton,
        CheckBox,
        Radio,
        Rectangle,
        Rounded,
        Transparent,
        TransparentSelected
    }
    
    public partial class UIBuilder
    {

        

        private Dictionary<TextButtonStyles, TextButtonStyle> _cachedTextButtonStyles = new Dictionary<TextButtonStyles, TextButtonStyle>();

        public TextButton CreateTextButton(string text, int x = 0, int y = 0, string  tooltip ="", bool applyStyle = true, bool autoManage = true)
        {
            var textButton = new TextButton(this, text: text.ToUpper(), x: x, y: y, tooltip: tooltip);

            if(applyStyle)
            {
                var style = TextButtonStyles.Label;

                if(_cachedTextButtonStyles.ContainsKey(style) == false)
                {
                    _cachedTextButtonStyles[style] = new TextButtonStyle();
                }
                
                _cachedTextButtonStyles[style].ApplyStyle(textButton);
            }            
            
            if(autoManage)
                AddUI(textButton);

            return textButton;
        }

        
        

        public TextButton IconTextButton(string text, string spriteName, int x =0, int y = 0, string tooltip = "", Alignment iconAlignment = Alignment.Left, bool applyStyle = true, bool autoManage = true)
        {

            // Create a default text button without any style
            var textButton = CreateTextButton(text: text.ToUpper(), x: x, y: y, tooltip: tooltip, applyStyle: false);
            
            // Add the sprite name
            textButton.SpriteName = spriteName;

            // Check to see if we should apply the default icon button style
            if(applyStyle)
            {
                // Make sure default style exists first
                var style = TextButtonStyles.IconButton;

                if(_cachedTextButtonStyles.ContainsKey(style) == false)
                {
                    // var colors = new Dictionary<InteractiveStates, int[]>
                    // {
                    //     {InteractiveStates.Up, new int[]{1, 0}},
                    //     {InteractiveStates.SelectedUp, new int[]{0, 1}},
                    // };
                    
                    _cachedTextButtonStyles[style] = new TextButtonStyle()
                    {
                        Padding = new Rectangle(2, 0, 2, 0),
                        // IconAlignment = Alignment.Left,
                        // StateColors = colors
                    };
                }

                
                // Override the icon alignment from the constructor
                textButton.IconAlignment = iconAlignment;

                _cachedTextButtonStyles[style].ApplyStyle(textButton);
            }
            
            if(autoManage)
                AddUI(textButton);

            return textButton;
        }

    }
}