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
    public class SceneUITest : Scene
    {
        private bool bgInvalidated;
        private Button currentToolButton;
        private ModalPanel _toolPanel;
        private TextButton textButton;

        public SceneUITest(UIBuilder uiBuilder) : base(uiBuilder, "SceneUITest")
        {
            AutoReset = false;
            
        }

        public override void Activate()
        {

            if(_firstRun)
            {
                // There is no tilemap so use this canvas to store the background
                var bgCanvas = new Canvas(_gameChip.Display().X, _gameChip.Display().Y, _gameChip);
                // bgCanvas.Clear(1);
                
                // Set the canvas stroke
                bgCanvas.SetStroke(0, 1);
                
                // Create top left and right corners
                bgCanvas.SetPixels(0, 0, 2, 2, new[]{0,0,0,1});
                bgCanvas.SetPixels(_gameChip.Display().X - 2, 0, 2, 2, new[]{0,0,1,0});


                var pattern = _gameChip.Sprite(_gameChip.MetaSprite("patterns").Sprites[6].Id);

                bgCanvas.SetPattern(pattern, 8, 8);
                bgCanvas.DrawRectangle(8, 100, 30, 30, true);

                pattern = _gameChip.Sprite(_gameChip.MetaSprite("patterns").Sprites[20].Id);

                var emptyButton = _uiBuilder.CreateEmptyButton(50, 100, 30, 30);

                bgCanvas.SetPattern(pattern, 8, 8);
                // bgCanvas.DrawRectangle(emptyButton.X, emptyButton.Y, emptyButton.Width, emptyButton.Height, true);
                
                // Draw nav bar
                bgCanvas.DrawLine(0, 9, _gameChip.Display().X, 9);

                bgCanvas.DrawPixels();

                _toolPanel = new ModalPanel(_uiBuilder, "ToolPanel", new Rectangle());
                _toolPanel.X = 10;

                // Debug color
                _gameChip.Color(3, "#FF0000");
                
                

                InvalidateBackground();

                _uiBuilder.CursorID = Cursors.Pointer;

                // Test making a tool modal

                var toolNames = new string[]
                {
                    "tool-pointer",
                    "tool-button",
                    "tool-textfield",

                    "tool-selection",
                    "tool-pen",
                    "tool-eraser",

                    "tool-text",
                    "tool-brush",
                    "tool-line",

                    "tool-box",
                    "tool-circle",
                    "tool-fill",
                    
                };

                var toolButtons = new List<Button>();
                
                for (int i = 0; i < toolNames.Length; i++)
                {
                    var pos = _gameChip.CalculatePosition(i, 3);

                    pos.X = (pos.X * 16);
                    pos.Y = (pos.Y * 16) + 10;

                    var btn = _uiBuilder.CreateButton(spriteName: toolNames[i], x: pos.X, y:pos.Y, tooltip: "Tool " + i, autoManage: false);
                    btn.OnRelease = new Action<Button>(OnToolRelease);

                    _toolPanel.Add(btn);

                }

                _toolPanel.X = 2;

                var sprite = _uiBuilder.CreateSprite("slider-vertical", 200, 28);

                var spriteButton = _uiBuilder.CreateSpriteButton("tool-textfield", 80, 16);

                textButton = _uiBuilder.CreateTextButton("Tiny Card", 3);
                textButton.OnRelease = new Action<Button>(OnOpenMenu);


                var leftIconButton = _uiBuilder.IconTextButton("Left Icon Text Button", "tool-box", 80, 46);
                var rightIconButton = _uiBuilder.IconTextButton("Right Icon Text Button", "tool-box", 80, 46 + 24, "", Alignment.Right);
                var topIconButton = _uiBuilder.IconTextButton("Top Icon Text Button", "tool-box", 80, 46 + 24 + 20, "", Alignment.Top);

                var toggleGroup = _uiBuilder.CreateVerticalToggleGroup("Check Box Group", 80, 130, true);

                toggleGroup.Add(_uiBuilder.CheckBoxButton(text: "Option 1", autoManage: false));
                toggleGroup.Add(_uiBuilder.CheckBoxButton(text: "Option 2", autoManage: false));
                toggleGroup.Add(_uiBuilder.CheckBoxButton(text: "Option 3", autoManage: false));

                
            }

            base.Activate();
            
            
        }

        public void OnOpenMenu(Button button)
        {
            var textButton = button as TextButton;

            if(textButton == null)
                return;

            button.Select(true);

            _toolPanel.Open();

        }

        public void OnToolRelease(Button button)
        {
            if(currentToolButton != null)
                currentToolButton.Select(false);

            currentToolButton = button;
            currentToolButton.Select(true);

            textButton.Select(false);
            textButton.Enable(true);


            _toolPanel.Close();
        }

        public void ButtonReleaseTest(Button button)
        {

            // _uiBuilder.OpenModal();
        }

        public void ButtonPressTest(Button button)
        {
        }

        public void InvalidateBackground()
        {
            bgInvalidated = true;
        }

        public void ResetBackgroundValidation()
        {
            bgInvalidated = false;
        }

    }


}