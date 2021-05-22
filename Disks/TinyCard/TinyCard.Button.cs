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
    public interface IButtonEditor
    {
        bool ButtonEditMode
        {
            get;
        }

        UIBuilder UIBuilder {get;}

        GameChip GameChip {get;}

    }

    public partial class Card : IButtonEditor
    {
        private bool _buttonEditMode;

        public bool ButtonEditMode => _buttonEditMode;

        public UIBuilder UIBuilder => _uiBuilder;

        public GameChip GameChip => _gameChip;

        

        private Dictionary<TextButtonStyles, TextButtonStyle> _cachedTextButtonStyles = new Dictionary<TextButtonStyles, TextButtonStyle>();


        public void NewButton()
        {
            var btn = new TinyCardButton(this, "New Button", (int)((_gameChip.Display().X - 48) * .5f), (int)((_gameChip.Display().Y - 8) * .5f))
            {
                OnPress = new Action<Button>(OnButtonPress),
                OnRelease = new Action<Button>(OnButtonRelease),
                OnReleaseOutside = new Action<Button>(OnButtonReleaseOutside)
            };

            btn.Name = "Button" + _buttons.Count;

            Add(btn);

        }
        
        public void EditButtons(bool value = true)
        {
            _buttonEditMode = value;

            if(value == false)
            {
                _currentUI = null;
            }

        }

        public void OnButtonPress(Button button)
        {

            var tinyCardButton = button as TinyCardButton;

            if(tinyCardButton == null)
                return;

            tinyCardButton.DragOffset.X = _uiBuilder.CollisionManager.MouseX - tinyCardButton.X;
            tinyCardButton.DragOffset.Y = _uiBuilder.CollisionManager.MouseY - tinyCardButton.Y;

            if(_currentUI != tinyCardButton)
            {
                _currentUI = tinyCardButton;
                _currentUI.Select(true, false);
            }
            
    
            Console.WriteLine("OnButtonPress {0} edit {1} - {2} {3} {4}", button.Name, ButtonEditMode, tinyCardButton.X, _uiBuilder.CollisionManager.MouseX, button.Width);

            // button.Select(true, false);
        }

        public void OnButtonRelease(Button button)
        {
            Console.WriteLine("OnButtonRelease {0} edit {1}", button.Name, ButtonEditMode);

            // button.Select(false, false);

        }

        public void OnButtonReleaseOutside(Button button)
        {
            Console.WriteLine("OnButtonReleaseOutside {0} edit {1}", button.Name, ButtonEditMode);

            button.Select(false, false);

        }
        

    }

    public enum EditModes
    {
        None,
        Move,
        Resize, 
        Selected
    }

    public interface ISelect
    {
        bool Selected { get; }
        void Select(bool value, bool autoDisable = true);
    }

    public class TinyCardButton : TextButton, ISelect
    {
        private static Dictionary<TextButtonStyles, TextButtonStyle> _cachedTextButtonStyles = new Dictionary<TextButtonStyles, TextButtonStyle>()
        {
            {
                TextButtonStyles.Label, 
                new TextButtonStyle()
                {
                    IconAlignment = Alignment.Center
                }
            },
            {
                TextButtonStyles.IconButton, 
                new TextButtonStyle()
                {
                    Padding = new Rectangle(2, 0, 2, 0),
                    IconAlignment = Alignment.Top
                }
            },
            {
                TextButtonStyles.CheckBox, 
                new TextButtonStyle()
                {
                    IconSpriteName = "checkbox",
                    Padding = new Rectangle(2, 0, 2, 0),
                    StateColors = new Dictionary<InteractiveStates, int[]>
                    {
                        {InteractiveStates.Up, new int[]{0, 1}},
                        {InteractiveStates.SelectedUp, new int[]{0, 1}},
                    },
                    HitRectFill = false,
                    IconAlignment = Alignment.Left
                }
            },
        };

        private int[][] _cornerPixels = new int[][]
        {
            // Top left
            new int[]{
                1, 1,
                1, 0
            },
            // Top Right
            new int[]{
                1, 1,
                0, 1
            },
            // Bottom Right
            new int[]{
                0, 1,
                1, 1
            },
            // Bottom Left
            new int[]{
                1, 0,
                1, 1
            },
        };

        private TextButtonStyles _style;

        private bool Edit => _cardEditor.ButtonEditMode;

        private bool _invalidateStyle;
        private IButtonEditor _cardEditor;
        private Canvas _outlineCanvas;

        private bool _border = true;

        public Point DragOffset = new Point();

        public bool IsCheckbox
        {
            get =>  _style == TextButtonStyles.CheckBox;
            set
            {
                _style = TextButtonStyles.CheckBox;
            }
        }

        private Rectangle _resizeHandle = new Rectangle(0, 0, 4, 4);

        private Point _anchorResizePoint = new Point();

        public EditModes EditMode = EditModes.None;
        private bool _rounded = true;

        public bool ShowBorder
        {

            set
            {
                _border = value;
                InvalidateStyle();
            }

        }
        
        public TinyCardButton(IButtonEditor cardEditor, string text, int x = 0, int y = 0) : base(cardEditor.UIBuilder, text, x, y)
        {

            _cardEditor = cardEditor;
            
            _outlineCanvas = new Canvas(1, 1, cardEditor.GameChip);
            _outlineCanvas.SetStroke(0, 1);

            // Change default render mode to sprite
            DrawMode = DrawMode.Sprite;

            // Apply deafult style
            Style = TextButtonStyles.Label;
            
        }

        public void InvalidateStyle()
        {
            _invalidateStyle = true;
        }

        public void ResetStyleValidation()
        {
            _invalidateStyle = false;
        }

        public override void PressButton(bool callAction = true)
        {
            
            // Only trigger if this is being released for the first time - stops multiple triggers
            if(FirstPressed == false)
            {
                return;
            }

            EditMode = _resizeHandle.Contains(_collisionManager.MouseX, _collisionManager.MouseY) ? EditModes.Resize : EditModes.Move;

            base.PressButton(callAction);

        }

        public override void ReleaseButton(bool callAction = true)
        {
            base.ReleaseButton(callAction);

            EditMode = Selected ? EditModes.Selected : EditModes.None;
        }

        public override void ReleaseOutsideButton()
        {
            base.ReleaseOutsideButton();

            EditMode = Selected ? EditModes.Selected : EditModes.None;

        }

        public override void Update(int timeDelta)
        {

            // Check if any changes to the style happened
            if(_invalidateStyle == true)
            {
                _cachedTextButtonStyles[_style].ApplyStyle(this);
                ResetStyleValidation();
            }

            if(Selected && Edit)
            {

                if(EditMode == EditModes.Resize)
                {
                    
                    Width = Math.Max(8,(_collisionManager.MouseX - X));
                    Height = Math.Max(8, (_collisionManager.MouseY - Y));

                    RebuildMetaSpriteCache();
                    InvalidateStyle();

                }
                else if(EditMode == EditModes.Move)
                {
                    X = Utilities.Clamp(_collisionManager.MouseX - DragOffset.X, 0, _gameChip.Display().X - Width);
                    Y = Utilities.Clamp(_collisionManager.MouseY - DragOffset.Y, 9, _gameChip.Display().Y - Height);

                    UpdateHandlePosition();
                }
                
            }
            
            base.Update(timeDelta);

        }

        protected override void CalculateStates()
        {

            base.CalculateStates();
            
            _outlineCanvas.Resize(Width, Height);
            _outlineCanvas.DrawRectangle(0, 0, Width, Height);

            UpdateHandlePosition();
        }

        private void UpdateHandlePosition()
        {
            _resizeHandle.X = X + Width - 4;
            _resizeHandle.Y = Y + Height - 4;
        }

        protected override void DrawState(Canvas canvas, InteractiveStates state, int x, int y, int colorOffset = 0)
        {

            base.DrawState(canvas, state, x, y, colorOffset);

            if(_border == true)
            {
                canvas.SetStroke(0, 1);
                canvas.DrawRectangle(x, y, Width, Height);

                if(_rounded)
                {
                    canvas.SetPixels(x, y, 2, 2, _cornerPixels[0]);
                    canvas.SetPixels(x + Width - 2, y, 2, 2, _cornerPixels[1]);
                    canvas.SetPixels(x + Width - 2, Height - 2, 2, 2, _cornerPixels[2]);
                    canvas.SetPixels(x, Height - 2, 2, 2, _cornerPixels[3]);
                }
                
            }

        }

        public override void DisplayState(DrawMode drawMode)
        {

            if(Edit)
            {
                // Console.WriteLine("Button edit mode");

                CurrentState = InteractiveStates.Up;
            }

            base.DisplayState(drawMode);
        }

        public override void Draw()
        {
            base.Draw();
            
            // if(Selected && Edit)
            //     _gameChip.DrawRect( _resizeHandle.X, _resizeHandle.Y, _resizeHandle.Width, _resizeHandle.Height, 2, DrawMode.SpriteAbove );
            
        }


        public TextButtonStyles Style
        {
            set
            {
                if(_cachedTextButtonStyles.ContainsKey(value) == false)
                {
                    return;
                }
                
                _style = value;

                InvalidateStyle();
                
            }
        }
        
    }

}