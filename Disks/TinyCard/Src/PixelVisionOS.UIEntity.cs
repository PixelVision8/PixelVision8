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
using System.Text.RegularExpressions;

namespace PixelVision8.Player
{

    public partial class UIEntity : Entity, IInteractive
    {

        public PixelData CachedPixelData => _cachedPixelData;

        protected PixelData _cachedPixelData;
        
        public bool Enabled => _enabled;
        protected bool _enabled = true;

        public bool InFocus => _inFocus;
        protected bool _inFocus = false;

        public string Tooltip {get; set;}

        public int ColorOffset = 0;

        protected CollisionManager _collisionManager => _uiBuilder.CollisionManager;

        protected List<int> _cachedStateOffsets = new List<int>();

        public InteractiveStates CurrentState {get; set;} = InteractiveStates.Up;

        protected Rectangle _hitRect = new Rectangle();

        public Rectangle HitRect => _hitRect;

        protected List<int> _stateSpriteIds = new List<int>();

        public Cursors DefaultFocusCursor = Cursors.Hand;

        public virtual void Enable(bool value, params object[] args)
        {
            _enabled = value;
            Invalidate();
        }

        protected override string GenerateUniqueName()
        {
            return "UI" + base.GenerateUniqueName();
        }

        public UIEntity
        (
            UIBuilder uiBuilder, 
            Rectangle rect, 
            string spriteName = "",
            string name = "",
            string tooltip = "", 
            bool autoSize = true, 
            DrawMode drawMode = DrawMode.Sprite,
            bool rebuildSpriteCache = true
        ) : base(uiBuilder, rect, name: name, spriteName: spriteName, autoSize: autoSize, drawMode:drawMode)
        {
            
            Tooltip = tooltip;
                        
            if(rebuildSpriteCache)
                RebuildMetaSpriteCache();

            // Build the default hit rect off of the button's size
            _hitRect.Width = Rect.Width;
            _hitRect.Height = Rect.Height;

        }

        public override void AutoSizeFromSpriteName(string spriteName, ref Rectangle rect)
        {

            // TODO maybe tie this into rebuild meta sprite cache?
            if(_gameChip.FindMetaSpriteId(spriteName + "-up") != -1)
            {
                spriteName += "-up";
            }
            else if(_gameChip.FindMetaSpriteId(spriteName + "-disabled") != -1)
            {
                spriteName += "-disabled";
            }
            
            base.AutoSizeFromSpriteName(spriteName, ref rect);

        }

        public virtual string StateSpriteName(InteractiveStates state) 
        {
            return SpriteName + "-" + Regex.Replace(state.ToString(), "(?!^)([A-Z])", "-$1").ToLower();
        }

        public virtual void RebuildMetaSpriteCache(string spriteName = null)
        {
            if (spriteName != null)
            {
                SpriteName = spriteName;
            }

            if(spriteName == "")
                return;

            CalculateStates();
            DrawStates();
        }

        
        protected virtual void CalculateStates()
        {
            
            _cachedStateOffsets.Clear();
            _stateSpriteIds.Clear();

            // The default rect position (Empty)
            foreach (InteractiveStates state in Enum.GetValues(typeof(InteractiveStates)))
            {
                CalculateState(state);
            } 

            CleanupStateCalculations();
            
        }

        protected virtual void CalculateState(InteractiveStates state)
        {
            
            var stateSprite = StateSpriteName(state);

            _stateSpriteIds.Add(_gameChip.FindMetaSpriteId(stateSprite));
            

            int lastOffset = _cachedStateOffsets.Count() == 0 ? 0 : _cachedStateOffsets.Last();


            var id = _stateSpriteIds.Last();
            
            if(id > -1)
            {
                lastOffset ++;
            }

            _cachedStateOffsets.Add(lastOffset);

        }

        protected virtual void CleanupStateCalculations()
        {

            // Remap disabled to up if no disabled exists
            if(_stateSpriteIds[(int)InteractiveStates.Disabled] == -1)
            {
                var upRect = _cachedStateOffsets[(int)InteractiveStates.Up];

                _cachedStateOffsets[(int)InteractiveStates.Disabled] = upRect;
            }

        }

        protected virtual void DrawStates()
        {

            // Create new pixel data based on canvas size
            _cachedPixelData = new PixelData((_cachedStateOffsets.Last() + 1) * Width, Height);
            
            // Create canvas
            var tmpCanvas = new Canvas(_cachedPixelData.Width, _cachedPixelData.Height, _gameChip);
            
            // Loop back over the sprite data and copy to the canvas
            for (int i = 0; i < _stateSpriteIds.Count; i++)
            {

                if(_stateSpriteIds[i] > -1)
                {

                    var state = (InteractiveStates)Enum.ToObject(typeof(InteractiveStates) , i);

                    DrawState(tmpCanvas, state, _cachedStateOffsets[(int)state] * Width, 0);
                    
                }

            }
            
            // Copy canvas pixels over to pixel data
            _cachedPixelData.SetPixels(tmpCanvas.GetPixels());

        }

        protected virtual void DrawState(Canvas canvas, InteractiveStates state, int x, int y, int colorOffset = 0)
        {
            
            canvas.DrawMetaSprite(
                _stateSpriteIds[(int)state], 
                x, 
                y,
                false,
                false,
                colorOffset
            );

        }

        public virtual void Update(int timeDelta)
        {
            if(Enabled == false)
            {

                if(InFocus)
                {
                    OnLoseFocus();
                }

                return;
            }

            if(_collisionManager.MouseDown && InFocus == false)
            {
                // Invalidate();
                return;
            }

            CurrentState = InteractiveStates.Up;

            if(_collisionManager.MouseInRect(HitRect, _rect.X, _rect.Y) || (InFocus && _collisionManager.MouseDown))
            {

                // Modify the state based on the state of the mouse
                CurrentState = _collisionManager.MouseDown || _collisionManager.MouseReleased ? InteractiveStates.Down : InteractiveStates.Over;

                OnFocus();

                // Reset the state if it needs to be used during draw
                CurrentState = InteractiveStates.Up;
            }
            else if(InFocus)
            {
                OnLoseFocus();
            }

        }

        public override void Draw()
        {
            if(Debug)
            {
                _gameChip.DrawRect(X, Y, HitRect.Width, HitRect.Height, 3);

                return;
            }

            if(CachedPixelData == null || Invalid == false)
                return;
            
            DisplayState(DrawMode);

            if((DrawMode == DrawMode.TilemapCache || DrawMode == DrawMode.Tile) )
            {
                ResetValidation();
            }

        }

        public virtual void DisplayState(DrawMode drawMode)
        {
            if(_cachedPixelData == null)
                return;
            
            _gameChip.DrawPixels
            (
                Utilities.GetPixels(_cachedPixelData, _cachedStateOffsets[(int)CurrentState] * Width, 0, Width, Height),
                Rect.X,
                Rect.Y,
                Width,
                Height,
                false,
                false,
                drawMode,
                ColorOffset
            );
            
        }


        protected virtual void OnFocus()
        {
            
            // Let the UI Builder know this has focus
            _uiBuilder.SetFocus(this, DefaultFocusCursor);
            
            _inFocus = true;
            
            // TODO this needs to be deferred to the draw stage
            // Only render over when in tile or tilemap cache mode
            if(DrawMode == DrawMode.Tile || DrawMode == DrawMode.TilemapCache)
                DisplayState(DrawMode.Sprite);
            
        }

        protected virtual void OnLoseFocus()
        {
            _uiBuilder.ClearFocus(this);
            
            _inFocus = false;

        }
        
    }

}