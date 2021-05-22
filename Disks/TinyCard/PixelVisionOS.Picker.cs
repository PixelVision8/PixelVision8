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

namespace PixelVision8.Player
{

    public class Picker : UIEntity
    {

        public Point PickerOffset = new Point(-1, -1);

        public Action<Picker> OnSelection { get; set; }

        public int _currentSelection;

        public int CurrentSelection => _currentSelection;

        protected int _gridSize = 8;
        protected int _columns = 0;
        protected int _rows = 0;

        protected int _metaSpriteId = -1;

        protected int _selectionSpriteId = -1;
        protected int _selectionOverSpriteId = -1;

        private int _pickerTotal;
        public int Total
        {
            get => _pickerTotal;
            set => _pickerTotal = Utilities.Clamp(value, 0, _columns * _rows);
        }

        public Picker(UIBuilder uiBuilder, int x = 0, int y = 0, int gridSize = 8, string spriteName = "", string name = "", string tooltip = "", string selectSpriteName = "picker-selection", bool autoSize = true, DrawMode drawMode = DrawMode.Sprite, bool rebuildSpriteCache = true) : base(uiBuilder: uiBuilder, rect: new Rectangle(x, y, 0, 0), spriteName: spriteName, name: name, tooltip: tooltip, autoSize:false, drawMode:DrawMode.TilemapCache, rebuildSpriteCache: false)
        {

            // Save the grid size which we use to find the item
            _gridSize = gridSize;

            _selectionSpriteId = _gameChip.FindMetaSpriteId(selectSpriteName);
            _selectionOverSpriteId = _gameChip.FindMetaSpriteId(selectSpriteName + "-over");

            if(_selectionOverSpriteId == -1)
                _selectionOverSpriteId = _selectionSpriteId;

            // Manually rebuild the sprite cache
            RebuildMetaSpriteCache();

            // Update the hit rect to match the new width and height
            _hitRect.Width = Width;
            _hitRect.Height = Height;

        }

        protected override void CalculateStates()
        {
            // TODO get the size of the meta sprite and change the entity size to match

            _metaSpriteId = _gameChip.FindMetaSpriteId( SpriteName );

            if(_metaSpriteId == -1)
                return;

            var metaSprite = _gameChip.MetaSprite(_metaSpriteId);

            // TODO need to round this up to match the grid
            Width = metaSprite.Width;
            Height = metaSprite.Height;

            _columns = Width/_gridSize;
            _rows = Height/_gridSize;

            _pickerTotal = _columns * _rows;

            base.CalculateStates();

        }

        protected override void DrawStates()
        {
            if(_metaSpriteId == -1)
                return;

            var canvas = new Canvas(Width, Height, _gameChip);

            canvas.DrawMetaSprite(_metaSpriteId, 0, 0);

            _cachedPixelData = new PixelData(Width, Height, canvas.Pixels);

        }

        public override void Update(int timeDelta)
        {
            base.Update(timeDelta);

            if(_inFocus)
            {
                var tmpC = (int)Math.Floor((_collisionManager.MouseX  - X) / (float)_gridSize);
                var tmpR = (int)Math.Floor((_collisionManager.MouseY  - Y)/ (float)_gridSize);
                
                var tmpId = Utilities.CalculateIndex
                (
                    tmpC,
                    tmpR,
                    _columns
                );

                if(_collisionManager.MouseReleased)
                {
                    // ReleaseButton();
                    Select(tmpId);
                }
                
                _gameChip.DrawMetaSprite(_selectionOverSpriteId, tmpC * _gridSize + X + PickerOffset.X, tmpR * _gridSize + Y + PickerOffset.Y, false, false, DrawMode.UI);
            }
        }

        public override void Draw()
        {
            base.Draw();

            var pos = Utilities.CalculatePosition(_currentSelection, _columns);

            _gameChip.DrawMetaSprite(_selectionOverSpriteId, (pos.X * _gridSize) + X + PickerOffset.X, (pos.Y * _gridSize) + Y + PickerOffset.Y, false, false, DrawMode.UI);

        }

        public void Select(int id)
        {
            
            _currentSelection = id;

            if(OnSelection != null)
            {
                OnSelection(this);
            }

        }

    }

}