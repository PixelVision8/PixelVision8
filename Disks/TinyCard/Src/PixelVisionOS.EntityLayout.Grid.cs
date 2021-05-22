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

    public partial class EntityLayout
    {

        public const string GRID = "grid";

        public static EntityLayout Grid
        {
            get
            {
                return new EntityLayout(EntityLayout.GRID);
            }
        }

        public int Columns = 1;

        [LayoutAttribute (EntityLayout.GRID)]
        public void GridLayout(EntityManager entityManager, int offsetX = 0, int offsetY = 0)
        {
            var entities = entityManager.Entities;
            
            // Get the total
            var total = entities.Count;

            var maxWidth = 0;
            var maxHeight = 0;

            // Loop through all items to find max width and height
            for (int i = 0; i < total; i++)
            {
                maxWidth = Math.Max(maxWidth, entities[i].Width);
                maxHeight = Math.Max(maxHeight, entities[i].Height);
            }

            maxWidth += Padding;
            maxHeight += Padding;

            // Reset width and height
            entityManager.Width = 0;
            entityManager.Height = 0;

            for (int i = 0; i < total; i++)
            {
                var entity = entities[i];

                var pos = new Point(i % Columns, i / Columns);

                entity.X = pos.X * maxWidth + offsetX;
                entity.Y = pos.Y * maxHeight + offsetY;

                entityManager.Width = Math.Max(entityManager.Width, (pos.X * maxWidth) + maxWidth);
                entityManager.Height = Math.Max(entityManager.Height, (pos.Y * maxHeight) + maxHeight);

            }

        }

    }

}