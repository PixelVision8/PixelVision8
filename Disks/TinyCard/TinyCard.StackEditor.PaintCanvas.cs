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

    public enum Modes
    {
        None,
        Pointer,
        Button,
        TextField,
        Paint
    }

    public partial class SceneStackEditor : Scene
    {

        private Modes _currentMode = Modes.None;
        private TinyCardPopupModal _brushModal;
        private TinyCardPopupModal _patternModal;

        [MenuAction]
        public void SelectPointerTool() => ChangeMode(Modes.Pointer);

        [MenuAction]
        public void SelectButtonTool() => ChangeMode(Modes.Button);

        [MenuAction]
        public void SelectTextFieldTool() => ChangeMode(Modes.TextField);

        private void ChangeMode(Modes mode)
        {
            if(_currentMode == mode)
                return;

            var lastMode = _currentMode;

            _currentMode = mode;

            // Enable the canvas if the mode is set to paint
            // _canvas.Enable(mode == Modes.Paint);
            
            CurrentCard.Canvas.Enable(mode == Modes.Paint);

            // Look to see if we just switched to the button mode
            // if(_currentMode == Modes.Button && lastMode != Modes.Button)
            // {
                
            // }

            CurrentCard.EditButtons(_currentMode == Modes.Button);
                        
            // Select the correct mode button
            if(mode != Modes.Paint)
                _navBar.SelectOption("TOOLS", "Select" + mode.ToString() + "Tool", true);

        }

        private void ChangeTool(Tools tool)
        {

            ChangeMode(Modes.Paint);

            CurrentCard.Canvas.ChangeTool(tool);
            // Route tool selection to the canvas
            // _canvas.ChangeTool(tool);

            _navBar.SelectOption("TOOLS", "Select" + tool.ToString() + "Tool", true);

            CurrentCard.EditButtons(false);

        }

        [MenuAction]
        public void SelectSelectionTool() => ChangeTool(Tools.Selection);
        
        [MenuAction]
        public void SelectPenTool() => ChangeTool(Tools.Pen);

        [MenuAction]
        public void SelectEraserTool() => ChangeTool(Tools.Eraser);

        [MenuAction]
        public void SelectTextTool() => ChangeTool(Tools.Text);

        [MenuAction]
        public void SelectBrushTool() => ChangeTool(Tools.Brush);

        [MenuAction]
        public void SelectLineTool() => ChangeTool(Tools.Line);

        [MenuAction]
        public void SelectRectangleTool() => ChangeTool(Tools.Rectangle);

        [MenuAction]
        public void SelectEllipseTool() => ChangeTool(Tools.Ellipse);

        [MenuAction]
        public void SelectFillTool() => ChangeTool(Tools.Fill);

        [MenuAction]
        public void ToggleFilled()
        {
            CurrentCard.Canvas.ToggleFill();

            // TODO need to go and toggle the ellipse and rectangle tool buttons to show filled
            // TODO need to change the text of the toggle menu option?
        }

        [MenuAction]
        public void ToggleGrid()
        {
            CurrentCard.Canvas.ToggleGrid();
        }




        [MenuAction]
        public void OpenBrushOptions()
        {
            if(_brushModal == null)
            {
                _brushModal = new TinyCardPopupModal(_uiBuilder, "BrushOptions");
            
                var picker = new Picker(_uiBuilder, 0 ,0, 8, "brush-picker", "select-brush")
                {
                    OnSelection = new Action<Picker>(OnSelectBrush)
                };

                _brushModal.Add(picker);

            }
            
            _brushModal.Open();
            
        }

        [MenuAction]
        public void OpenPatternPicker()
        {
            if(_patternModal == null)
            {
                _patternModal = new TinyCardPopupModal(_uiBuilder, "BrushOptions");
            
                var picker = new PatternPicker(_uiBuilder)
                {
                    OnSelection = new Action<Picker>(OnSelectPattern)
                };

                _patternModal.Add(picker);

            }
            
            _patternModal.Open();
            
        }

        private void OnSelectBrush(Picker picker)
        {
            
            // TODO change picker size
            _brushModal.Close();
            CurrentCard.Canvas.ChangeStrokeWidth(picker.CurrentSelection + 1);

        }

        private void OnSelectPattern(Picker picker)
        {
        
            var patternPicker = picker as PatternPicker;

            if(patternPicker == null)
                return;

            // TODO change picker size
            _patternModal.Close();

            Console.WriteLine("Pattern {0}", picker.CurrentSelection);

            CurrentCard.Canvas.ChangePattern(patternPicker.CurrentPattern, 8, 8);

        }


        [MenuAction]
        public void OpenPattenOptions()
        {
            Console.WriteLine("OpenPattenOptions");
        }

    }

}