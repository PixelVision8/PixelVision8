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
using System.Linq;

namespace PixelVision8.Player
{

    public class TextButton : Button
    {

        protected Rectangle _padding = new Rectangle(4, 0, 4, 0);
        public Rectangle Padding 
        {
            get
            {
                return _padding;
            }
            set
            {
                _padding.X = value.X;
                _padding.Y = value.Y;
                _padding.Width = value.Width;
                _padding.Height = value.Height;
            }
        }
        public string Font {get; set;} = "medium";
        public int Spacing {get; set; }= -4;
        public Alignment IconAlignment {get; set; }= Alignment.Left;
        public bool HitRectFill {get; set; } = true;

        public Rectangle _textBounds = new Rectangle();

        protected Rectangle _iconRect = new Rectangle();
        // protected Rectangle _lastRect;
        protected int _lastSprite;

        protected Dictionary<InteractiveStates, int[]> _stateColors = new Dictionary<InteractiveStates, int[]>
        {
            {InteractiveStates.Disabled, new int[]{0, 1}},
            {InteractiveStates.Up, new int[]{0, 1}},
            {InteractiveStates.Down, new int[]{1, 0}},
            {InteractiveStates.SelectedUp, new int[]{1, 0}},
        };

        protected override string GenerateUniqueName()
        {
            return "Text" + base.GenerateUniqueName();
        }

        public Dictionary<InteractiveStates, int[]> StateColors
        {
            get
            {
                return _stateColors;
            }
            set{
                // Clone the state colors
                _stateColors = value.ToDictionary(
                    x => x.Key,
                    y => y.Value 
                );
            }
        }

        public string Text{get;set;}
 
        public TextButton(
            UIBuilder editorUI, 
            string text, 
            int x =0,
            int y =0,
            string tooltip = "", 
            string spriteName = "",
            Rectangle padding = new Rectangle(),
            string font = "medium",
            int spacing = -4,
            Alignment iconAlignment = Alignment.Left,
            bool hitRectFill = true,
            Dictionary<InteractiveStates, int[]> stateColors = null
        ) : base(editorUI, name: text, x: x, y: y, width: 0, height: 0, spriteName: spriteName, tooltip: tooltip, rebuildSpriteCache: false)
        {
            
            Padding = padding;
            Font = font;
            Spacing = spacing;
            IconAlignment = iconAlignment;
            HitRectFill = hitRectFill;
            
            if(stateColors != null)
                StateColors = stateColors;

            Text = text;

        }

        public void ApplyStyle(Rectangle? rect = null)
        {
            Console.WriteLine("{0} ApplyStyle", Name);
            
            if(rect.HasValue)
            {
                Width = rect.Value.Width;
                Height = rect.Value.Height;
            }
            else
            {
                Width = Math.Max(0, Width);
                Height = Math.Max(0, Height);
            }

            _lastSprite = -1;
            _iconRect.Width = 0;
            _iconRect.Height = 0;

            CalculateSize();
            CalculateStates();
            CalculatePadding();
            DrawStates();
            
        }

        public override void RebuildMetaSpriteCache(string spriteName = null)
        {
            
            if (spriteName != null)
            {
                SpriteName = spriteName;
            }

            ApplyStyle();
            
        }

        protected void CalculateSize()
        {
            _textBounds = new Rectangle(0, 0, Text.Length * (Constants.SpriteSize + Spacing), 8);
        }

        protected override void CalculateState(InteractiveStates state)
        {
            var defaultValue = StateColors.ContainsKey(state) ? -1 : -2;
                
            int lastOffset = _cachedStateOffsets.Count() == 0 ? 0 : _cachedStateOffsets.Last();
            
            if(defaultValue > -2)
            {

                if(SpriteName != string.Empty)
                {
                    var id = _gameChip.FindMetaSpriteId(StateSpriteName(state));
                
                    if(id > -1)
                    {
                        var metaSprite = _gameChip.MetaSprite(id);

                        _iconRect.Width = Math.Max(_iconRect.Width, metaSprite.Width);
                        _iconRect.Height = Math.Max(_iconRect.Height, metaSprite.Height);

                        defaultValue = id;
                    }
                    
                }

                lastOffset ++;
                
            }

            _stateSpriteIds.Add(defaultValue);

            _cachedStateOffsets.Add(lastOffset);
            
        }

        protected override void CleanupStateCalculations()
        {
            // TODO what should we do to clean up the states before we render them out
        }

        protected void CalculatePadding()
        {
            Console.WriteLine("CalculatePadding {0}", Name);
            
            switch(IconAlignment){
                
                case Alignment.Left:

                    // Move the icon over based on the padding
                    _iconRect.X = _padding.X;
                    _iconRect.Y = _padding.Y;

                    // Move the text to the right of the icon + padding
                    _textBounds.X =+ _iconRect.X + _iconRect.Width + _padding.X;

                    // Calculate the width based on the text bounds and padding
                    if(Width == 0)
                        Width = _textBounds.Right + _padding.Width;

                    // Calculate the height based on the padding and if the icon or text bounds are larger
                    if(Height == 0)
                        Height = _padding.Y + Math.Max(_iconRect.Height, _textBounds.Height) + _padding.Height;

                    // Center Text Horizontally
                    _textBounds.Y = (int)((Height * .5f) - (_textBounds.Height * .5f));

                break;
                case Alignment.Right:

                    // Move the text to the right of the icon + padding
                    _textBounds.X = _padding.X;

                    // Move the icon over based on the padding
                    _iconRect.X = _padding.X + _textBounds.Right;
                    _iconRect.Y = _padding.Y;

                    // Calculate the width based on the text bounds and padding
                    if(Width == 0)
                        Width = _textBounds.Right + _padding.Width + _iconRect.Width + _padding.Width;

                    // Calculate the height based on the padding and if the icon or text bounds are larger
                    if(Height == 0)
                        Height = _padding.Y + Math.Max(_iconRect.Height, _textBounds.Height) + _padding.Height;

                    // Center Text Horizontally
                    _textBounds.Y = (int)((Height * .5f) - (_textBounds.Height * .5f));

                break;
                case Alignment.Top:

                    // Move the icon over based on the padding
                    
                    _iconRect.Y = _padding.Y;

                    // Move the text to the right of the icon + padding
                    _textBounds.X =+ _padding.X;
                    _textBounds.Y =+ _padding.Y + _iconRect.Y + _iconRect.Height + _padding.Y;

                    // Calculate the width based on the text bounds and padding
                    if(Width == 0)
                        Width = _textBounds.Right + _padding.Width;

                    // Center button
                    _iconRect.X = (int)((Width * .5f) - (_iconRect.Width * .5f));

                    // Calculate the height based on the padding and if the icon or text bounds are larger
                    if(Height == 0)
                        Height = _textBounds.Bottom + _padding.Bottom;

                break;
                default:

                    
                    
                    // Calculate the width based on the text bounds and padding
                    if(Width == 0)
                    {
                        // Move the text to the right of the icon + padding
                        _textBounds.X =+ _padding.X;
                        Width = _textBounds.Right + _padding.Width;
                    }
                    else
                    {
                        Console.WriteLine("Center {0} {1} {2}", Width, _textBounds.Width, (int)((Width - _textBounds.Width) * .5f));
                        _textBounds.X = (int)((Width - _textBounds.Width) * .5f);
                    }
                        

                    // Calculate the height based on the padding and if the icon or text bounds are larger
                    if(Height == 0)
                    {
                        _textBounds.Y =+ _padding.Y + _padding.Y;
                        Height = _textBounds.Bottom + _padding.Bottom;
                    }
                    else
                    {
                        _textBounds.Y = (int)((Height - _textBounds.Height) * .5f);
                    }  

                break;
                
            }

            // Update the hit rect
            _hitRect = HitRectFill ? new Rectangle(0, 0, Width, Height) : _iconRect;
        }

        // TODO need to find a solution around reusing this entire logic simply for -2 instead of -1 in the base function
        protected override void DrawStates()
        {

            // Create new pixel data based on canvas size
            _cachedPixelData = new PixelData((_cachedStateOffsets.Last() + 1) * Width, Height);
            
            // Create canvas
            var tmpCanvas = new Canvas(_cachedPixelData.Width, _cachedPixelData.Height, _gameChip);
            
            // Loop back over the sprite data and copy to the canvas
            for (int i = 0; i < _stateSpriteIds.Count; i++)
            {

                if(_stateSpriteIds[i] > -2)
                {

                    var state = (InteractiveStates)Enum.ToObject(typeof(InteractiveStates) , i);

                    DrawState(tmpCanvas, state, _cachedStateOffsets[(int)state] * Width, 0);
                    
                }

            }
            
            // Copy canvas pixels over to pixel data
            _cachedPixelData.SetPixels(tmpCanvas.GetPixels());

        }

        protected override void DrawState(Canvas canvas, InteractiveStates state, int x, int y, int colorOffset = 0)
        {

            var stateId = (int)state;

            var stateSprite = _stateSpriteIds[stateId];
            
            // var offset = _cachedStateOffsets[stateId];
            var textColor = StateColors[state][0];
            var bgColor = StateColors[state][1];
            colorOffset += 0;//StateColors.Count > 1 ? StateColors[state][2] : 0;

            canvas.Clear(bgColor, x, y, Width, Height);

            // Use base draw state for drawing the icon sprite into the canvas
            if(_stateSpriteIds[stateId] > -1)
            {
                _lastSprite = _stateSpriteIds[stateId];
                base.DrawState(canvas, state, x + _iconRect.X, y + _iconRect.Y, colorOffset);
            }

            // Center text inside of padding
            canvas.DrawText(Text, x + _textBounds.X, y + _textBounds.Y, Font, textColor, Spacing);

        }

    }

}