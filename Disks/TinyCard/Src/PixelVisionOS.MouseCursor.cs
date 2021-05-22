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

namespace PixelVision8.Player
{
    public enum Cursors
    {
        None,
        Pointer,
        Hand,
        HandDown,
        Text,
        Cross,
        Pen,
        Eraser,
        HandDrag
    }

    internal struct Cursor{

        public string SpriteName;
        public Point Offset;

        public Cursor(string spriteName, int x = 0, int y = 0)
        {
            SpriteName = spriteName;
            Offset = new Point(x, y);
        }

    }

    public partial class MouseCursor : UIComponent, IUpdate, IDraw
    {
        private int _refreshDelay = 300;
        private int _refreshTime = 0;
        private int _animationTime = 0;
        private int _animationDelay = 1000; // change animation on every frame

        public bool Lock;

        public Point Pos = new Point(-1, -1);

        public Cursors CursorId = Cursors.Pointer;

        private Cursors _cursorId = Cursors.None;

        private CollisionManager _collisionManager => _uiBuilder.CollisionManager;
        private bool _lockCursor;

        private Cursor _currentCursor;

        private Cursor[] _cursors = new Cursor[]
        {
            new Cursor("cursor-none", 0, 0),
            new Cursor("cursor-pointer", -1, -1),
            new Cursor("cursor-hand", -3, -1),
            new Cursor("cursor-hand-down", -3, 0),
            new Cursor("cursor-text", -3, -5),
            new Cursor("cursor-cross", -5, -5),
            new Cursor("cursor-pen", -1, -10),
            new Cursor("cursor-eraser", -1, -9)
        };

        public MouseCursor(UIBuilder uiBuilder):base(uiBuilder)
        {
            
        }

        public void Update(int timeDelta)
        {
            // throw new NotImplementedException();

            _refreshTime += timeDelta;

            if(_refreshTime > _refreshDelay)
            {
                _refreshTime = 0;

                // Update the cursor id
                SetCursor(CursorId);

            }
        }

        public void Draw()
        {
            
            Pos.X = _uiBuilder.CollisionManager.MouseX;
            Pos.Y = _uiBuilder.CollisionManager.MouseY;

            // Mouse out of bounds
            if(Pos.X < 0 || Pos.Y < 0)
                return;
            
            var cursorData = (_cursorId == Cursors.Hand && _collisionManager.MouseDown) ? _cursors[(int)Cursors.HandDown] : _currentCursor;

            _gameChip.DrawMetaSprite(
                cursorData.SpriteName,
                Pos.X + cursorData.Offset.X,
                Pos.Y + cursorData.Offset.Y,
                false,
                false,
                DrawMode.Mouse // TODO need to make this above everything else
            );

        }

        public void SetCursor(Cursors id, bool lockCursor = false)
        {

            // Update mouse lock
            if(lockCursor == false)
                _lockCursor = false;
            
            // Check if there is a different cursor or we are locked and return
            if(_cursorId == id || _lockCursor)
                return;

            _lockCursor = lockCursor;
            _cursorId = id;

            _animationTime = 0;
            _animationDelay = 1000;

            _currentCursor = _cursors[(int)_cursorId];

        }

    }
}