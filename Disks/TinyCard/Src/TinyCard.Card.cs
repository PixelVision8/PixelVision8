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
using System.Text;

namespace PixelVision8.Player
{

    public partial class Card : EntityManager
    {
        
        private PaintCanvas _canvas;
        private List<TinyCardButton> _buttons = new List<TinyCardButton>();
        private List<TinyCardField> _fields;
        
 
        public PaintCanvas Canvas => _canvas;

        public List<TinyCardButton> Buttons  => _buttons;

        public List<TinyCardField> Fields => _fields;

        public bool CanDelete = true;

        private int _id;
        private bool _forceCanvasInvalidation;

        public int Id => _id;

        private ISelect _currentUI;

        public Card(UIBuilder uiBuilder, int id, string name = "Untitled") : base(uiBuilder, name)
        {
            _id = id;

            // Offset below the menu
            Y = 9;

            // Set to the full width of the screen
            Width = _gameChip.Display().X;

            // Set to full height of the screen minus the nav offset
            Height = _gameChip.Display().Y - Y;

            // Create a new canvas for the background and offset by the height of the nav
            _canvas = uiBuilder.CreatePaintCanvas(Width, Height, y: 1, autoManage: false);
            _canvas.Name = name + _canvas.Name;

            _canvas.OnPress = new Action<PaintCanvas>(OnCanvasPress);

            // _canvas.Clear(2);

            Add(_canvas);

        }

        private void OnCanvasPress(PaintCanvas canvas)
        {
            if(canvas.Tool != Tools.Pen)
                return;

            // TODO need to look at the tilemap cache color not the canvas
            var pixelColor = canvas.SampleCurrentPixel();

            canvas.BrushColor = pixelColor == 0 ? 1 : 0;

        }

        public override void Update(int timeDelta)
        {

            if(_uiBuilder.CollisionManager.MouseDown && _currentUI != null && _uiBuilder.InFocusUI != _currentUI)
            {
                _currentUI.Select(false, false);
                _currentUI = null;
            }

            base.Update(timeDelta);
        }

        public void Load()
        {
            _canvas.InvalidateCanvas();
        }

        public void Close()
        {
            // _gameChip.DrawRect( X, Y, Width, Height, 1 );
        }

        public override void Clear()
        {
            
            base.Clear();

            Canvas.Clear();
            Buttons.Clear();
            Fields.Clear();

            // Re-add the canvas back to the display
            Add(_canvas);

        }

        public string Serialize()
        {
            var sb = new StringBuilder();

            // Loop through all of the elements and save them out

            return sb.ToString();
        }

        public void Deserialize(string value)
        {
            Clear();
            // Convert to json and rebuild all the elements

        }

        

        // Route all drawing APIs to the canvas

    }

}