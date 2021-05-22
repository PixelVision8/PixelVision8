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

        public static EntityLayout Vertical
        {
            get
            {
                return new EntityLayout("vertical");
            }
        }


        [LayoutAttribute ("vertical")]
        public void VerticalLayout(EntityManager entityManager, int offsetX = 0, int offsetY = 0)
        {
            var entities = entityManager.Entities;

            var total = entities.Count;
            
            var nextX = offsetX;
            var nextY = offsetY;

            entityManager.Width = 0;
            entityManager.Height = 0;

            for (int i = 0; i < total; i++)
            {
                var entity = entities[i];

                entity.X = nextX;
                entity.Y = nextY; 

                entityManager.Width = Math.Max(entityManager.Width, entity.Width);
                entityManager.Height += entity.Height;   

                nextY += entity.Height + Padding;
            }

        }

    }

}