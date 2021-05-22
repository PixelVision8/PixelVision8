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
using System.Linq;

namespace PixelVision8.Player
{

    public enum MouseStates
    {
        Up,
        Pressed,
        Dragging,
        Released
        
    }
    public enum Tools
    {
        Pointer,
        Pen,
        Brush,
        Eraser,
        Line,
        Ellipse,
        Rectangle,
        Fill,
        Selection,
        Eyedropper,
        Text,
        Hand
    }

    public enum SelectionStates
    {
        Cancled,
        Resize,
        New,
        NewMove,
        Move,
        None
    }
    public class PaintCanvas : UIEntity
    {


        public Action<PaintCanvas> OnPress;
        public Action<PaintCanvas> OnRelease;

        private Canvas _tmpLayerCanvas;
        private int[] _selectedPixelData;
        private int _defaultPenWidth = 1;
        private int _defaultEraserWidth = 4;
        private Canvas _backgroundLayerCanvas;
        private Canvas _currentCanvasLayer;
        private Rectangle _boundaryRect = new Rectangle();
        private bool _displayInvalid = true;
        private int _gridSize = 8;
        private bool _snapToGrid = false;
        private Point _mousePos = new Point();

        // Scale
        private Rectangle _scaledViewport = new Rectangle();
        private float[] _scaleValues = new float[]{.5f, 1, 2, 4, 8};

        public int SampleCurrentPixel() => (_startPos.HasValue == false) ? - 1 : _imageLayerCanvas.ReadPixelAt(_startPos.Value.X, _startPos.Value.Y);
        
        private int _scaleMode = 1;
        private Cursors _focusCursor = Cursors.Hand;
        private Canvas _overCanvas = new Canvas(8, 8);
        private Canvas _imageLayerCanvas;
        private int emptyColorId = -1;
        private int[] _emptyPixelData = Enumerable.Repeat(-1, 64).ToArray();

        public bool Fill => _fill;

        private bool _fill = false;
        private Cursors _currentCursorID = Cursors.Pointer;

        public int BrushColor
        {
            get => _brushColor;
            set
            {
                _brushColor = value;
                ResetCanvasStroke();
                RebuildBrushPreview();
            }
        }
        private int _brushColor = 0;
        private int _brushColorOffset = 0;
        private Canvas _brushCanvas = new Canvas(8, 8);
        private Rectangle _brushMaskRect = new Rectangle(0, 0, 8, 8);
        private int overColor = -1;
        private int _defaultStrokeWidth = 1;
        private int selectionTime = 0;
        private int selectionDelay = 200;
        private int selectionShift = 0;
        private Tools _tool;
        public Tools Tool => _tool;
        private bool _drawTmpLayer;
        public int _scale = 1;
        public int ColorOffsset = 0;
        private Rectangle? _selectRect;
        private bool _clearArea;
        private bool _fillRect;
        private int _fillRectColor;
        private bool _firstPress;
        private MouseStates _mouseState = MouseStates.Up;
        private Cursors _defaultCursorID;
        private Point? _startPos;
        private bool _mergerTmpLayer;
        private SelectionStates _selectionState;
        private Point _moveOffset;
        private bool _showBrushPreview;
        private int _emptyColorId = -1;
        private int _lastPenX = -1;
        private int _lastPenY = -1;
        private int _colorOffset;
        private int _maskColor = 1;
        private bool _canvasInvalid;
        private bool _selectionInvalid;
        private bool _undoValid;
        private bool _invalidBrush;

        public bool UseGrid;
        private PixelData _defaultPattern =  new PixelData(1, 1, new int[]{0});

        public void ChangeStrokeWidth(int value)
        {
            _defaultStrokeWidth = Utilities.Clamp(value, 1, 4);
            ResetCanvasStroke();
        }

        public void ToggleGrid(bool? value = null)
        {
            _snapToGrid = value.HasValue ? value.Value : !_snapToGrid;

            Console.WriteLine("Snap {0}", _snapToGrid);
        }

        public void ToggleFill(bool? value = null)
        {
            _fill = value.HasValue ? value.Value : !_fill;
        }

        public int EmptyColorId
        {
            get => _emptyColorId;
            set 
            {
                _emptyColorId = value;
                for (int i = 0; i < _emptyPixelData.Length; i++)
                {
                    _emptyPixelData[i] = value;
                }
            }
        }

        internal void ChangePattern(int[] currentPattern, int width, int height)
        {
            _defaultPattern.Resize(width, height);
            _defaultPattern.SetPixels(currentPattern);
        }

        protected override string GenerateUniqueName()
        {
            return "PaintCanvas" + base.GenerateUniqueName();
        }

        public override int Width { 
            get => _imageLayerCanvas.Width; 
            set { 
                if(_imageLayerCanvas != null) 
                    Resize(value, Height); 
            }
        }

        public override int Height { 
            get => _imageLayerCanvas.Height; 
            set { 
                if(_imageLayerCanvas != null) 
                    Resize(Height, value); 
            }
        }

        public PaintCanvas(UIBuilder uiBuilder, int width, int height, int x = 0, int y = 0, string name = "", string tooltip = "", DrawMode drawMode = DrawMode.TilemapCache) : base(uiBuilder:uiBuilder, rect:new Rectangle(x, y, width, height), name: name, tooltip: tooltip, autoSize: false, drawMode: drawMode, rebuildSpriteCache: false)
        {

            _imageLayerCanvas = new Canvas(1, 1, uiBuilder.GameChip);
            _tmpLayerCanvas = new Canvas(1, 1, uiBuilder.GameChip);
            _backgroundLayerCanvas = new Canvas(1, 1, uiBuilder.GameChip);

            DefaultFocusCursor = Cursors.Pen;

            // Use resize to force the hit rect to be calculated correctly
            Resize(width, height);

            EmptyColorId = -1;

            // TODO may not need this?
            _overCanvas.SetStroke(0, 1);

            ChangeScale(1);
        
        }

        public override void Update(int timeDelta)
        {
            
            // TODO make sure this is correctly enabled/disabled
            if(Enabled == false)
            {

                if(InFocus)
                {
                    OnLoseFocus();
                }

                return;
            }

            CurrentState = InteractiveStates.Up;

            if(_collisionManager.MouseInRect(HitRect, _rect.X, _rect.Y))
            {

                // Modify the state based on the state of the mouse
                CurrentState = InteractiveStates.Over;

                OnFocus();

                // Reset the state if it needs to be used during draw
                CurrentState = InteractiveStates.Up;

                _mousePos.X = (int)Math.Floor((_uiBuilder.CollisionManager.MouseX - X)/ (float)_scale) + _scaledViewport.X;
                _mousePos.Y = (int)Math.Floor((_uiBuilder.CollisionManager.MouseY - Y)/ (float)_scale) + _scaledViewport.Y;
                
                if(_snapToGrid)
                {
                    _mousePos.X = SnapToGrid(_mousePos.X);
                    _mousePos.Y = SnapToGrid(_mousePos.Y);
                }
                
                if(_selectRect.HasValue)
                {
                    // If the mouse is inside of the select rect, change the icon
                    if(_selectRect.Value.Contains(_mousePos))// && _selectionState != "resize") then
                    {
                        // Change the cursor to the drag hand 
                        // TODO need to add this cursor to the mouse class
                        _currentCursorID = Cursors.HandDrag;
                    }
                    
                }
                else
                {
                    // Reset the cursor when you leave the selection area
                    _currentCursorID = _defaultCursorID;
                }

                if(InFocus == true){

                    // -- Check to see if the button is pressed and has an onAction callback
                    if(_uiBuilder.CollisionManager.MouseReleased == true)
                    {

                        // Click the button
                        OnClick();
                        _firstPress = true;
                
                    }
                    else if(_uiBuilder.CollisionManager.MouseDown)
                    {
                
                        _mouseState = MouseStates.Dragging;

                        if(_firstPress) // TODO should make this just true and remove not equal false
                        {

                            // Save start position
                            _startPos = new Point(_mousePos.X, _mousePos.Y);

                            ResetCanvasStroke();
                
                            TriggerOnFirstPress();
                        
                
                            _mouseState = MouseStates.Pressed;
                
                            // Change the flag so we don't trigger first press again
                            _firstPress = false;

                        }

                        DrawOnCanvas(_mousePos);
                
                    }

                }
                
            }
            else if(InFocus)
            {
                OnLoseFocus();
            }

            if(_uiBuilder.CollisionManager.MouseReleased && _mouseState == MouseStates.Dragging)
            {
                OnClick();
                _firstPress = true;
            }

        }

        public void OnClick()
        {
            // TODO add logic here for on click

            if(OnRelease != null)
                OnRelease(this);

            Console.WriteLine("on Click");

            CanvasRelease();

            if(_undoValid == true)
            {

                // _uiBuilder.BegineUndo(this);

                // _uiBuilder.EndUndo(this);

            }

        }

        public void TriggerOnFirstPress()
        {
            StoreUndoSnapshot();

            if(_uiBuilder.InFocusUI.Name != Name)
                return;

            if(OnPress != null)
                OnPress(this);

            // ResetCanvasStroke();

            if(_tool == Tools.Fill)
            {
                
                // TODO need to update this to match the real pattern
                _imageLayerCanvas.SetPattern(_defaultPattern.Pixels, _defaultPattern.Width, _defaultPattern.Height);

                _imageLayerCanvas.FloodFill(_startPos.Value.X, _startPos.Value.Y);

                _mergerTmpLayer = false;

                InvalidateCanvas();

                InvalidateUndo();

            }

        }
    
        private void CanvasRelease()
        {

            if(_selectRect.HasValue && _selectionState == SelectionStates.Resize)
            {

                ClampSelectionToBounds();

                if(_selectRect.Value.Width == 0 || _selectRect.Value.Height == 0)
                {

                    _selectedPixelData = null;

                    CancelCanvasSelection();

                }
                else
                {

                    if(_selectedPixelData == null)
                    {

                        _selectedPixelData = CutPixels();

                    }

                }

            // TODO enable menus

            }


            _selectionState = SelectionStates.None;
            

            _startPos = null;
            _lastPenX = -1;
            _lastPenY = -1;

            _mouseState = _mouseState == MouseStates.Released ? MouseStates.Up : MouseStates.Released;

            InvalidateCanvas();

        }

        public void DrawOnCanvas(Point mousePos)
        {
            
            // -- TODO some tools should snap to grid when holding down shift

            

            

            // -- print("self.startPos", self.startPos, "mousePos", mousePos)

            // -- Get the start position for a new drawing
            if(_startPos.HasValue)
            {
                var startPos = _startPos.Value;

                // Test for the data.tool and perform a draw action
                if(_tool == Tools.Pen || _tool == Tools.Eraser || _tool == Tools.Brush)
                {

                    // var  targetCanvas = self.pickerMode == FlagMode and self.flagLayerCanvas or _imageLayerCanvas

                    if(_lastPenX != startPos.X || _lastPenY != startPos.Y) {

                    // Console.WriteLine("Draw Line");

            //         if(self.pickerMode == ColorMode) then

                        _imageLayerCanvas.DrawLine(startPos.X, startPos.Y, mousePos.X, mousePos.Y);
                        
                        // Console.WriteLine("Draw {0},{1}", startPos.X, startPos.Y);
            //         else
                
            //             targetCanvas:MergePixels(self.startPos.x, self.startPos.y, self.brushCanvas.Width, self.brushCanvas.Height, self.brushCanvas:GetPixels(), false, false, 0, false)

            //         end

                        _lastPenX = startPos.X;
                        _lastPenY = startPos.Y;

                        InvalidateCanvas();

                    }
                    
                    _startPos = new Point(mousePos.X, mousePos.Y);

                    // Don't merge tmp layer since we are not using it
                    _mergerTmpLayer = false;

                    // Invalidate the canvas and the selection
                    InvalidateUndo();

                }
                else if(_tool == Tools.Line)
                {

                    // Clear tmp layer before drawing the line on this frame
                    _tmpLayerCanvas.Clear();
                    
                    // Console.WriteLine("Clear Tmp Layer - Line");
                    
                    // Draw the line on the tmp canvas
                    _tmpLayerCanvas.DrawLine(startPos.X, startPos.Y, mousePos.X, mousePos.Y);

                    // Need to merge tmp layer after drawing
                    _mergerTmpLayer = true;

                    // Invalidate the canvas
                    InvalidateCanvas();

                    // Invalidate the canvas and the selection
                    InvalidateUndo();
                }
                else if(_tool == Tools.Rectangle)
                {
            //     

                    // Invalidate the canvas and the selection
                    InvalidateUndo();

                    _tmpLayerCanvas.Clear();
            //         -- print("Clear Tmp Layer - Rectangle")
                    ResetCanvasStroke();

                    // TODO this is fixed in the canvas
                    _tmpLayerCanvas.DrawRectangle
                    (
                        Math.Min(startPos.X, mousePos.X), 
                        Math.Min(startPos.Y, mousePos.Y),
                        Math.Abs(mousePos.X - startPos.X)+ 1,
                        Math.Abs(mousePos.Y - startPos.Y) + 1, 
                        _fill
                    );

                    // Need to merge tmp layer after drawing
                    _mergerTmpLayer = true;

                    InvalidateCanvas();
                }
                else if(_tool == Tools.Ellipse)
                {

                    // Invalidate the canvas and the selection
                    InvalidateUndo();

                    _tmpLayerCanvas.Clear();

                    ResetCanvasStroke();

                    // TODO this is fixed in the canvas
                    _tmpLayerCanvas.DrawEllipse(
                        Math.Min(_startPos.Value.X, mousePos.X),
                        Math.Min(_startPos.Value.Y, mousePos.Y),
                        Math.Abs(mousePos.X - _startPos.Value.X)+ 1,
                        Math.Abs(mousePos.Y - _startPos.Value.Y) + 1,
                        _fill
                    );

                    // Need to merge tmp layer after drawing
                    _mergerTmpLayer = true;

                    InvalidateCanvas();
                }
                else if(_tool == Tools.Selection)
                {

                    if(_mouseState == MouseStates.Pressed)
                    {

                        if(_selectRect.HasValue == false)
                        {

                            _selectionState = SelectionStates.New;

                            _selectRect = new Rectangle(startPos.X, startPos.Y, 0, 0);

                        }
                        else
                        {
                            if(_selectRect.Value.Contains(mousePos) == true)
                            {
                                _selectionState = SelectionStates.NewMove;

                                _moveOffset = new Point(_selectRect.Value.X - mousePos.X, _selectRect.Value.Y - mousePos.Y);

                            }
                            else
                            {
                                // Cancel the selection
                                CancelCanvasSelection();
                            
                            }

                        }
                    }
                    else if(_mouseState == MouseStates.Dragging)
                    {

                        if(_selectRect.HasValue)
                        {

                            InvalidateUndo();

                            var selection = _selectRect.Value;
                        
                            if(_selectionState == SelectionStates.New || _selectionState == SelectionStates.Resize)
                            {

                                _selectionState = SelectionStates.Resize;
                                
            //             -- print("resize", data.selectRect, mousePos, , )

                                selection.X = Math.Min(_startPos.Value.X, mousePos.X);
                                selection.Y = Math.Min(_startPos.Value.Y, mousePos.Y);
                                selection.Width = Utilities.Clamp(Math.Abs(mousePos.X - _startPos.Value.X), 0, _imageLayerCanvas.Width);
                                selection.Height = Utilities.Clamp(Math.Abs(mousePos.Y - _startPos.Value.Y), 0, _imageLayerCanvas.Height);
                            }
                            else
                            {
                                _uiBuilder.CursorID = Cursors.Hand; // TODO may need to fix this was set to 2 in lua?

                                selection.X = mousePos.X + _moveOffset.X;// --  data.selectionSize.X
                                selection.Y = mousePos.Y + _moveOffset.Y;// --  data.selectionSize.Y

                                _selectionState = SelectionStates.Move;
                            }
            

            //                 }
                        }
                    

                    }

                    // No need to merge tmp layer since the selection tool will do this manually
                    _mergerTmpLayer = false;

            //     -- print("Selection", dump(selection))

                }
                else if(_tool == Tools.Eyedropper)
                {
                    
            //         -- Need to merge tmp layer after drawing
            //         self.mergerTmpLayer = false

            //         -- Check to see if we are in color mode
            //         if(self.pickerMode == ColorMode) then

            //         -- Save reference to the previous color
            //         local oldColor = self.overColor
                    
            //         -- Save the new color under the mouse 
            //         self.overColor = _imageLayerCanvas:ReadPixelAt(mousePos.x, mousePos.y)


            //         if(oldColor ~= self.overColor) then

            //             -- Select the color and offset by 1 to account for the mask color in the first position
            //             self:OnPickerSelection(self.overColor + 1)

            //         end
            //         -- print("self.overColor", self.overColor)
            //     -- if(self.overColor > pixelVisionOS.colorsPerSprite) then
            //     --     self.overColor = -1
            //         end

            //     -- TODO should we display this value or that it is out of bounds?

                }

            }



        }

        

        public void Clear(int colorRef = -1, int x = 0, int y = 0, int? width = null, int? height = null) => _imageLayerCanvas.Clear(colorRef, x, y, width, height);

        public void Resize(int width, int height)
        {

            // TODO the viewport and layers are hardcoded here to the canvas size
            _hitRect.Width = width;
            _hitRect.Height = height;
            
            _imageLayerCanvas.Resize(width, height);
            _tmpLayerCanvas.Resize(width, height);
            _backgroundLayerCanvas.Resize(width, height);
            
            // HitRect = new Rectangle(0, 0, Width, Height);
            
        }

        public void ChangeScale(int value)
        {

            _scale = value;

            var imageWidth = (int)Math.Floor(_imageLayerCanvas.Width * (float)_scale);
            var imageHeight = (int)Math.Floor(_imageLayerCanvas.Height * (float)_scale);

            var viewWidth = (int)Math.Floor(HitRect.Width / (float)_scale);
            var viewHeight = (int)Math.Floor(HitRect.Height / (float)_scale);

            _scaledViewport.Width = Utilities.Clamp(viewWidth, 1, Math.Max(imageWidth, _imageLayerCanvas.Width));
            _scaledViewport.Height = Utilities.Clamp(viewHeight, 1, Math.Max(imageHeight, _imageLayerCanvas.Height));

            // Calculate the boundary for scrolling
            _boundaryRect.Width = _imageLayerCanvas.Width - _scaledViewport.Width;
            _boundaryRect.Height = _imageLayerCanvas.Height - _scaledViewport.Height;
            
            _gridSize = 8 * _scale;

            InvalidateCanvas();

        }

        public int[] CutPixels(bool clearArea = true)
        {

            if(_selectRect.HasValue == false)
            {
                return null;
            }

            var selection = _selectRect.Value;

            _clearArea = clearArea;

            return _imageLayerCanvas.GetPixels(selection.X, selection.Y, selection.Width, selection.Height);
        }

        public void FillCanvasSelection(int colorId)
        {

            if(_selectRect.HasValue == false)
            {
                return;
            }

            _fillRect = true;
            _fillRectColor = colorId;    


        }

        public void InvalidateBackground()
        {
            // TODO This is probably not needed
        }

        public void InvalidateCanvas()
        {
            _displayInvalid = true;
        }

        public void ResetCanvasValidation()
        {
            _displayInvalid = false;
        }

        public void ChangeTool(Tools tool)
        {
            if(_selectRect.HasValue)
                CancelCanvasSelection();

            _showBrushPreview = tool == Tools.Pen || tool == Tools.Eraser || tool == Tools.Brush;

            _tool = tool;
            
            // Reset to default value
            _drawTmpLayer = false;

            // _drawTmpLayer = tool == Tools.Selection || ; // TODO don't remember what this was for "or self.toolButtons[3].selected == true" 

            if(tool == Tools.Pointer)
            {
                _currentCursorID = Cursors.Pointer;
            }
            else if(tool == Tools.Hand)
            {
                _currentCursorID = Cursors.Hand;
            }
            else if(tool == Tools.Pen)
            {
                _currentCursorID = Cursors.Pen;
            }
            else if(tool == Tools.Eraser)
            {
                _currentCursorID = Cursors.Eraser;
            }
            else if(tool == Tools.Text)
            {
                _currentCursorID = Cursors.Text;
            }
            else
            {
                _currentCursorID = Cursors.Cross;
                // These tools use the tmp layer for previewing
                _drawTmpLayer = true;
            }

            DefaultFocusCursor = _currentCursorID;

            InvalidateBrushPreview();

        }

        public void CancelCanvasSelection()
        {

            _selectionState = SelectionStates.Cancled;

            InvalidateUndo();

            InvalidateCanvas();

            // TODO need to fire an event so the parent can disable the menu items

        }

        public void SelectAll()
        {

            if(_selectRect.HasValue && _selectedPixelData != null)
            {
                _imageLayerCanvas.MergePixels(_selectRect.Value.X, _selectRect.Value.Y, _selectRect.Value.Width, _selectRect.Value.Height, _selectedPixelData);
            }

            // TODO need to finish this

            InvalidateUndo();

            _selectionState = SelectionStates.Resize;

            _selectRect = new Rectangle(0, 0, _imageLayerCanvas.Width, _imageLayerCanvas.Height);

            _selectedPixelData = null;

            OnClick();

        }

        private void StoreUndoSnapshot()
        {

        }

        private void InvalidateUndo(bool canvas = true, bool selection = true)
        {
            _canvasInvalid = canvas;
            _selectionInvalid = selection;

            _undoValid = true;

        }

        private void ResetUndoValidation()
        {
            _undoValid = false;
        }

        private void ClampSelectionToBounds()
        {
            var selection = _selectRect.Value;

            selection.X = Math.Max(selection.X, 0);
            selection.Y = Math.Max(selection.Y, 0);

            // Clamp the Width
            if(selection.X + selection.Width > _imageLayerCanvas.Width)
            {
                selection.Width = selection.Width - ((selection.X + selection.Width) - _imageLayerCanvas.Width);
            }
            
            // Clamp the Height
            if(selection.Y + selection.Height > _imageLayerCanvas.Height)
            {
                selection.Height = selection.Height - ((selection.Y + selection.Height) - _imageLayerCanvas.Height);
            }

            _selectRect = selection;

        }

        private void ResetCanvasStroke()
        {
            
            if(_tool == Tools.Pen)
            {
                _imageLayerCanvas.SetStroke(_brushColor, _defaultPenWidth);
            }
            else if(_tool == Tools.Eraser)
            {
                _imageLayerCanvas.SetStroke(_emptyColorId, _defaultEraserWidth);
            }
            else
            {
                _imageLayerCanvas.SetStroke(_brushColor, _defaultStrokeWidth);
                _tmpLayerCanvas.SetStroke(_brushColor, _defaultStrokeWidth);
                _tmpLayerCanvas.SetPattern(_defaultPattern.Pixels, _defaultPattern.Width, _defaultPattern.Height);
            }
            
        }

        private int SnapToGrid(int value, int? grid = null)
        {
            if(Tool == Tools.Pen || Tool == Tools.Eraser || Tool == Tools.Eyedropper || Tool == Tools.Brush)
                return value;

            var tmpGrid = grid.HasValue ? grid.Value : Constants.SpriteSize;
            
            return (int)Math.Floor(value / (float)tmpGrid) * tmpGrid;

        }

        private void InvalidateBrushPreview()
        {
            _invalidBrush = true;
        }

        private void ResetBrushInvalidation()
        {
            _invalidBrush = false;
        }

        private void RebuildBrushPreview()
        {

            if(_invalidBrush == false)
                return;
            
            Console.WriteLine("Rebuild brush");

            // _brushColor = 2;
            // _brushColor = ;

            _brushCanvas.Clear();

            _brushCanvas.Clear(_tool == Tools.Eraser ? _emptyColorId : _brushColor, 0, 0, _defaultStrokeWidth, _defaultStrokeWidth);

            _brushColorOffset = _colorOffset;

            ResetBrushInvalidation();

            return;


            // TODO need to add pattern logic here

        }

        public override void Draw()
        {

            // --  Render a preview of the brush if the mouse is inside of the canvas
            if(_showBrushPreview)
            {
                
                RebuildBrushPreview();
                // Console.WriteLine("Show Preview");

                // Adjust mouse position inside of the canvas with scale
                var tmpX = ((_mousePos.X - _scaledViewport.X) * _scale);
                var tmpY = ((_mousePos.Y - _scaledViewport.Y) * _scale);
                
                // Calculate scroll offset
                var scrollXOffset = (_scaledViewport.X % 8);
                var scrollYOffset = (_scaledViewport.Y % 8);
                
                // We don't need to clip or snap the brush to the grid in color mode
                // if(_pickerMode ~= ColorMode)
                // {

                //     // Adjust the tmp X and Y position to account for the scroll offset
                //     tmpX = tmpX + (scrollXOffset * _scale);
                //     tmpY = tmpY + (scrollYOffset * _scale);
                    
                //     // Snap tmp X and Y to the grid
                //     tmpX = SnapToGrid(tmpX, _scale * 8) - (scrollXOffset * _scale);
                //     tmpY = SnapToGrid(tmpY, _scale * 8) - (scrollYOffset * _scale);
                
                // }
            //     -- Calculate horizontal clipping
                if(tmpX < 0)
                {

                    // If the brush is too far left, shift the X, width, and tmpX position
                    _brushMaskRect.X = scrollXOffset;
                    _brushMaskRect.Width = _brushCanvas.Width - _brushMaskRect.X;
                    tmpX = tmpX + (scrollXOffset * _scale);
                }
                else if((tmpX/_scale) + 8 > _scaledViewport.Width)
                {    
                    // If the brush is too far right, reset the X and shift the width
                    _brushMaskRect.X = 0;
                    _brushMaskRect.Width = _brushCanvas.Width - (((tmpX/_scale) + 8) - _scaledViewport.Width);
                }
                else
                {
                    // Reset the X and Width
                    _brushMaskRect.X = 0;
                    _brushMaskRect.Width = _brushCanvas.Width;

                }

            //     -- Calculate vertical clipping
                if(tmpY < 0)
                {
                    // If the brush is too far up, shift the Y, height, and tmpY position
                    _brushMaskRect.Y = scrollYOffset;
                    _brushMaskRect.Height = _brushCanvas.Height - _brushMaskRect.Y;
                    tmpY = tmpY + (scrollYOffset * _scale);
                }
                else if((tmpY/_scale) + 8 > _scaledViewport.Height)
                {    
                    // If the brush is too far down, reset the Y and shift the height
                    _brushMaskRect.Y = 0;
                    _brushMaskRect.Height = _brushCanvas.Height - (((tmpY/_scale) + 8) - _scaledViewport.Height);
                }
                else
                {
                    // Reset the Y and height
                    _brushMaskRect.Y = 0;
                    _brushMaskRect.Height = _brushCanvas.Height;
                }

            
                // Clamp brush mask so we don't get an error if the entire brush is out of bounds
                _brushMaskRect.Width = Utilities.Clamp( _brushMaskRect.Width, 0, _brushCanvas.Width );
                _brushMaskRect.Height = Utilities.Clamp( _brushMaskRect.Height, 0, _brushCanvas.Height );
           
            
            // _brushCanvas.Clear(2);
            
            // Console.WriteLine("Brush preview {0}", _brushMaskRect);
                
                // TODO this is not drawing correctly
                // _gameChip.DrawRect( tmpX + X - 5, tmpY + Y, _brushMaskRect.Width, _brushMaskRect.Height, 2, DrawMode.UI );
                // _gameChip.DrawPixels(_brushCanvas.GetPixels(), tmpX + X-1, tmpY + 11, _brushMaskRect.Width, _brushMaskRect.Height, false, false, DrawMode.UI);
                // Draw the brush
                _brushCanvas.DrawPixels(
                    // Shift the tmpX and tmpY position over by the viewportRect's position
                    tmpX + X,
                    tmpY + Y,
                    DrawMode.UI,
                    _scale,
                    -1,
                    -1,
                    _brushColorOffset,
                    _brushMaskRect
                );

            }


            if(_displayInvalid == true || _selectRect.HasValue)
            {
                
                // Check if we need to fill the area below the selection
                if(_fillRect == true)
                {
                    
                    for (int i = 0; i < _selectedPixelData.Length; i++)
                    {
                        _selectedPixelData[i] = _fillRectColor;
                    }
                
                    _fillRect = false;

                }

                if(_selectRect.HasValue)
                {

                    var selection = _selectRect.Value;

                    // Check to see if the state has been set to canceled
                    if(_selectionState == SelectionStates.Cancled)
                    {
                    
                    
                        if(_selectedPixelData != null)
                        {
                    
                            //  Draw pixel data to the image
                            _imageLayerCanvas.MergePixels(selection.X, selection.Y, selection.Width, selection.Height, _selectedPixelData);
                        
                            // Clear the pixel data
                            _selectedPixelData = null;
                    
                        }
                    
                
                        // Clear the selection state
                        _selectionState = SelectionStates.None;
                
                        // Clear the selection rect
                        _selectRect = null;
                
                        // Force the display to clear the tmp layer canvas
                        _tmpLayerCanvas.Clear();
                        
                        // Invalidate the canvas and the selection
                        InvalidateUndo();

                    }
                    else
                    {
                
                        // Check for the clear flag
                        if(_clearArea == true)
                        {

                            // If the area wasn't being filled, clear it with the mask color
                            _imageLayerCanvas.Clear(-1, selection.X, selection.Y, selection.Width, selection.Height);

                            _clearArea = false;

                        }

                        // Increment selection timer
                        selectionTime = selectionTime + _uiBuilder.TimeDelta;

            //         -- print("self.selectionTime", self.selectionTime)
                        if(selectionTime > selectionDelay)
                        {
                            selectionShift = selectionShift == 0 ? 1 : 0;
                            selectionTime = 0;
                //         -- print("Change Shift")
                        }

            //         -- Clear the tmp layer
                        _tmpLayerCanvas.Clear();
            //         -- print("Clear Tmp Layer - Redraw selection")
                    if(_selectedPixelData == null)
                    {
            //         -- Check to see if the selection is ignoring transparency
            //         if(_selectionUsesMaskColor == true) then

            //             self.tmpLayerCanvas:Clear(self.maskColor, self.selectRect.X, self.selectRect.Y, self.selectRect.Width, self.selectRect.Height)
            // //             -- print("Clear Tmp Layer - Redraw Selection mask")
            //         end
                    
                        _tmpLayerCanvas.MergePixels(selection.X, selection.Y, selection.Width, selection.Height, _selectedPixelData);

                    }
                    
                    DrawSelectionRectangle(_tmpLayerCanvas, selection, selectionShift);

                }
       

            }
                
                // Redraw the background of the canvas
                _backgroundLayerCanvas.DrawPixels(X, Y, DrawMode.TilemapCache, _scale, -1, _maskColor, 0, _scaledViewport);
              
                if(_mergerTmpLayer == true && _mouseState == MouseStates.Released)
                {

                    // TODO we can optimize this by passing in a rect for the area to merge
                    _imageLayerCanvas.MergeCanvas(_tmpLayerCanvas, 0, true);

                    _mergerTmpLayer = false;
                
                }

                // Draw the pixel data in the upper left hand corner of the tool's window
                _imageLayerCanvas.DrawPixels(X, Y, DrawMode.TilemapCache, _scale, _maskColor, -1, _colorOffset, _scaledViewport);

            //     -- Only draw the temp layer when we need to
                if(_drawTmpLayer == true)
                {
                    _tmpLayerCanvas.DrawPixels(X, Y, DrawMode.TilemapCache, _scale, -1, emptyColorId, _colorOffset, _scaledViewport);
                }
                
                _displayInvalid = false;

            }

            // Reset the mouse state at the end of the draw cycle if it was released on this frame
            if(_mouseState == MouseStates.Released)
            {
                _mouseState = MouseStates.Up;
            }

        }

        private void DrawSelectionRectangle(Canvas canvas, Rectangle selection, int shift)
        {
            Console.WriteLine("Draw Selection {0}", selection );
        }

        // private void DrawSelectoinLine(canvas, x1, y1, x2, y2, shift, visibleBounds)
    
        //     local dx, sx = math.abs(x2-x1), x1<x2 and 1 or -1
        //     local dy, sy = math.abs(y2-y1), y1<y2 and 1 or -1
        //     local err = math.floor((dx>dy and dx or -dy)/2)
        //     local counter = 0

        //     shift = shift or 0

        //     while(true) do

        //         -- Make sure the pixel is inside of the canvas
        //         if(((((0 <= x1) and (x1 < canvas.Width)) and (0 <= y1)) and (y1 < canvas.Height))) then
        //             canvas:SetPixelAt(x1, y1, counter % 2 == shift and -17 or -2)
        //         end
                    
        //         -- increment the alternating pixel counter
        //         counter = counter + 1

        //         if (x1==x2 and y1==y2) then break end
        //         if (err > -dx) then
        //             err, x1 = err-dy, x1+sx
        //         end
        //         if (err < dy) then
        //             err, y1 = err+dx, y1+sy
        //         end

                
        //     end

        // end
    }

}