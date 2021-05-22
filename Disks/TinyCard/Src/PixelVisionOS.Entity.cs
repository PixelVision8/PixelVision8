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

    public partial class Entity : AbstractData, IEntity
    {

        public bool Debug {get; set;} 
        public int DebugColor {get; set;} 

        protected string _name = string.Empty;

        protected virtual string GenerateUniqueName()
        {
            return "Entity:" + Tiles.X + "," + Tiles.Y;
        }

        public string Name
        {
            get 
            { 
                return _name;
            } 
            set 
            {
                _name = value;
            }
        }

        public virtual int X
        {
            get { return _rect.X; }
            set 
            { 
                _rect.X = value; 
                _tiles.X = (int)Math.Floor(_rect.X / (float)Constants.SpriteSize);
            }
        }

        public virtual int Y
        {
            get { return _rect.Y; }
            set 
            { 
                _rect.Y = value; 
                _tiles.Y = (int)Math.Floor(_rect.Y / (float)Constants.SpriteSize);
            }
        }

        public virtual int Width
        {
            get { return _rect.Width; }
            set 
            { 
                _rect.Width = value; 
                _tiles.Width = (int)Math.Floor(_rect.Width / (float)Constants.SpriteSize); 
            }
        }

        public virtual int Height
        {
            get { return _rect.Height; }
            set 
            { 
                _rect.Height = value; 
                _tiles.Width = (int)Math.Floor(_rect.Height / (float)Constants.SpriteSize); 
            }
        }

        protected Rectangle _rect = new Rectangle();

        public Rectangle Rect => _rect;
        public string SpriteName {get; set;}
        
        private Rectangle _tiles = new Rectangle();

        public Rectangle Tiles => _tiles;

        protected UIBuilder _uiBuilder;
        protected GameChip _gameChip => _uiBuilder.GameChip;

        public DrawMode DrawMode { get; set; } = DrawMode.TilemapCache;
        

        public Entity(UIBuilder uiBuilder, Rectangle rect, string spriteName = "", bool autoSize = true, DrawMode drawMode = DrawMode.Sprite, string name = "")
        {

            _uiBuilder = uiBuilder;

            SpriteName = spriteName;

            if(autoSize && spriteName != string.Empty) 
                AutoSizeFromSpriteName(spriteName, ref rect);

            X = rect.X;
            Y = rect.Y;
            Width = rect.Width;
            Height = rect.Height;

            _name = name != "" ? name : GenerateUniqueName();

            DrawMode = drawMode;

            Invalidate();
            
        }

        public virtual void AutoSizeFromSpriteName(string spriteName, ref Rectangle rect)
        {
            var metaSprite = _gameChip.MetaSprite(spriteName);

            if(metaSprite != null)
            {
                rect.Width = metaSprite.Width;
                rect.Height = metaSprite.Height;
            }

        }

        public virtual void Draw()
        {
            if(Invalid == true)
            {

                _gameChip.DrawMetaSprite(SpriteName, Rect.X, Rect.Y, false, false, DrawMode);

                if(DrawMode == DrawMode.Tile || DrawMode == DrawMode.TilemapCache)
                    ResetValidation();
            }
        }

    }

    public partial class UIBuilder
    {

        public IEntity CreateSprite(string spriteName, int x = 0, int y = 0, bool autoManage = true)
        {
            var entity = CreateEntity(rect: new Rectangle(x, y, 0, 0), spriteName: spriteName, autoManage: autoManage);

            // Rename the sprite
            entity.Name = "Sprite" + entity.Name;

            return entity;
        }

        public IEntity CreateEntity(Rectangle rect = new Rectangle(), string spriteName = "", bool autoSize = true, bool autoManage = true, DrawMode drawMode = DrawMode.Sprite)
        {
            var entity = new Entity(this, rect, spriteName, autoSize, drawMode);

            if(autoManage)
                AddUI(entity);

            return entity;
        }
    }

    
}