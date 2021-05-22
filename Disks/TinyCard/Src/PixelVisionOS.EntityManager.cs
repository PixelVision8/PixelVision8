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

namespace PixelVision8.Player
{

    public class EntityManager : Entity, IUpdate
    {

        // TODO to speed this up, UI should be registered in an update and draw list

        protected List<IEntity> _entities = new List<IEntity>();

        public List<IEntity> Entities => _entities;

        protected int _total;

        public int Total => _total;

        public ILayout Layout;

        public bool Pause { get; set; }

        public override int X
        {
            get => _rect.X;
            set
            {
                var oldX = _rect.X;

                _rect.X = value;

                if(Layout != null)
                {
                    Layout.Apply(this, _rect.X, _rect.Y);
                }
                else
                {
                    for (int i = 0; i < _total; i++)
                    {
                        _entities[i].X = _entities[i].X - oldX + _rect.X;
                    }
                }
            }
        }

        public override int Y
        {
            get => _rect.Y;
            set
            {
                var oldY = _rect.Y;

                _rect.Y = value;

                if(Layout != null)
                {
                    Layout.Apply(this, _rect.X, _rect.Y);
                }
                else
                {
                    for (int i = 0; i < _total; i++)
                    {
                        _entities[i].Y = _entities[i].Y - oldY + _rect.Y;
                    }
                }
                
            }
        }

        protected bool _autoResize;

        public EntityManager(UIBuilder uiBuilder, string name = "", int x = 0, int y = 0,List<Entity> entities = null, bool autoResize = true) : base(uiBuilder: uiBuilder, rect: new Rectangle(), autoSize: false, name: name)
        {

            _autoResize = autoResize;

            // Add all of the entities
            if(entities != null)
            {
                for (int i = 0; i < entities.Count; i++)
                {
                    Add(entities[i]);
                }
            }

            X = x;
            Y = y;

        }
        
        public IEntity this[in int i]
        {
            get => _entities[i];
            set => _entities[i] = value;
        }

        public virtual void Update(int timeDelta)
        {
            if(Pause)
                return;

            for (int i = 0; i < _total; i++)
            {
                var tmpEntity = _entities[i];

                if(tmpEntity is IUpdate)
                {
                    ((IUpdate)tmpEntity).Update(timeDelta);
                }

            }
            
        }

        public override void Draw()
        {
            if(Pause)
                return;
                
            
            for (int i = 0; i < _total; i++)
            {
                var tmpEntity = _entities[i];

                if(tmpEntity is IDraw)
                {
                    ((IDraw)tmpEntity).Draw();
                }

            }

            // Reset the invalidation at the end of the frame
            if(Invalid)
                ResetValidation();
        }

        public virtual void Add(IEntity entity)
        {
            if(entity is IUpdate || entity is IDraw)
            {
                _entities.Add(entity);
                _total = _entities.Count;
            }

            if(Layout != null)
            {
                Layout.Apply(this, _rect.X, _rect.Y);
            }
            else
            {
                entity.X += X;
                entity.Y += Y;

                CalculateSize();
            }

            Invalidate();
            
        }

        public virtual IEntity Remove(string name)
        {

            IEntity tmpEntity = null;

            // TODO this can be done quicker with linq
            var removeId = -1;

            for (int i = 0; i < _total; i++)
            {
                
                if(_entities[i].Name == name){
                    tmpEntity = _entities[i];
                    removeId = i;
                    break;

                }

            }

            if(removeId == -1)
                return tmpEntity;

            _entities.RemoveAt(removeId);

            _total = _entities.Count;

            if(Layout != null)
            {
                Layout.Apply(this, _rect.X, _rect.Y);
            }
            else
            {
                CalculateSize();
            }

            Invalidate();

            return tmpEntity;
        }

        protected virtual void CalculateSize()
        {
            if(_autoResize == false || Invalid == false)
                return;

            Width = 0;
            Height = 0;

            foreach (var entity in _entities)
            {
                Width = Math.Max(Width, entity.Rect.Right);
                Height = Math.Max(Height, entity.Rect.Bottom);
            }

            Width -= X;
            Height -= Y;

        }

        public virtual void Clear()
        {
            while(_total > 0)
            {
                Remove(_entities[0].Name);
            }
            
        }

        public override void Invalidate()
        {

            base.Invalidate();

            for (int i = 0; i < _total; i++)
            {
                _entities[i].Invalidate();
            }

        }

        public override void ResetValidation()
        {
            base.ResetValidation();

        }
        
    }

}