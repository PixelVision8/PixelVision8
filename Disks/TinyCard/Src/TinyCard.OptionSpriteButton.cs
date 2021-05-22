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

namespace PixelVision8.Player
{

    public partial class EntityLayout
    {

        public const string ICON_GRID = "iconGrid";

        public static EntityLayout IconGrid
        {
            get
            {
                return new EntityLayout(EntityLayout.ICON_GRID);
            }
        }

        [LayoutAttribute (EntityLayout.ICON_GRID)]
        public void IconGridLayout(EntityManager entityManager, int offsetX = 0, int offsetY = 0)
        {
            var entities = entityManager.Entities;
            
            // Get the total
            var total = entities.Count;

            var nextX = 0;
            var nextY = 0;
            var col = 0;
            var row = 0;

            var maxHeight = 0;

            for (int i = 0; i < total; i++)
            {
                col ++;

                var entity = entities[i];

                var pos = new Point(i % Columns, i / Columns);
                
                entity.X = nextX + offsetX;
                entity.Y = nextY + offsetY;

                nextX += entity.Width;
                
                maxHeight = Math.Max(maxHeight, entity.Height);

                if(col >= Columns)
                {
                    col = 0;
                    row ++;
                    
                    nextY += maxHeight;

                    entityManager.Width = Math.Max(entityManager.Width, nextX);
                    entityManager.Height = Math.Max(entityManager.Height, nextY);

                    nextX = 0;
                    maxHeight = 0;
                }
            }

        }

    }

    public class OptionSpriteButton : Button
    {
        public OptionSpriteButton(UIBuilder editorUI, string name, int x = 0, int y = 0, int width = 0, int height = 0, string spriteName = "", string tooltip = "", bool autoSize = true, DrawMode drawMode = DrawMode.TilemapCache, bool rebuildSpriteCache = true) : base(editorUI, name, x, y, width, height, spriteName, tooltip, autoSize, drawMode, rebuildSpriteCache)
        {

            if(name.ToLower() == "divider")
            {

                Width = 16;
                Height = 4;

                _cachedStateOffsets[(int)InteractiveStates.Disabled] = 0;

                // Create new pixel data based on canvas size
                _cachedPixelData = new PixelData(Width, Height);
                
                // Create canvas
                var tmpCanvas = new Canvas(_cachedPixelData.Width, _cachedPixelData.Height, _gameChip);
                tmpCanvas.SetStroke(1, 1);
                tmpCanvas.SetPattern(
                    new int[]
                    {
                        1, 1, 1,
                        1, 0, 1,
                        1, 1, 1
                    }, 3, 3
                );

                tmpCanvas.DrawRectangle(x, y, Width - 1, 3, true);

                tmpCanvas.SetStroke(0, 1);
                // tmpCanvas.SetPattern(new int[]{0}, 1, 1);
                tmpCanvas.DrawLine(0, 3, Width, 3);
                
                Enable(false);

                // Copy canvas pixels over to pixel data
                _cachedPixelData.SetPixels(tmpCanvas.GetPixels());
                
                name = GenerateUniqueName();
            }
        }

        public override void Update(int timeDelta)
        {
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
                CurrentState = InteractiveStates.Down;

                OnFocus();

                // Reset the state if it needs to be used during draw
                CurrentState = InteractiveStates.Up;
            }
            else if(InFocus)
            {
                OnLoseFocus();
            }
        }

    }

}