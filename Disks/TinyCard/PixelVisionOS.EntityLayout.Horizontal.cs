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
        public static EntityLayout Horizontal
        {
            get
            {
                return new EntityLayout("horizontal");
            }
        }

        [LayoutAttribute ("horizontal")]
        public void HorizontalLayout(EntityManager entityManager, int offsetX = 0, int offsetY = 0)
        {
            var entities = entityManager.Entities;
            
            var total = entities.Count;
            
            var nextX = offsetX;
            var nextY = offsetY;

            // Reset width and height
            entityManager.Width = 0;
            entityManager.Height = 0;

            for (int i = 0; i < total; i++)
            {
                var entity = entities[i];

                entity.X = nextX;
                entity.Y = nextY;   

                entityManager.Width += entity.Width;
                entityManager.Height = Math.Max(entityManager.Height, entity.Height);

                nextX += entity.Width + Padding;
            }

        }

    }
}