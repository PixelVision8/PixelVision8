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
using System.Reflection;

namespace PixelVision8.Player
{
    
    public class LayoutAttribute : Attribute {
        public string Type;
        
        public LayoutAttribute(string type)
        {
            Type = type;
        }
    }

    public partial class EntityLayout : ILayout
    {
        public int Padding = 0;

        protected MethodInfo _layoutMethod;

        protected EntityLayout(string type)
        {
            
            var methods = GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(LayoutAttribute), false).Length > 0)
                .ToArray();

            for (int i = 0; i < methods.Length; i++)
            {
                
                Type thisType = this.GetType();
                MethodInfo theMethod = thisType.GetMethod(methods[i].Name);

                // Get the File Parser attribute
                var attributes = theMethod.GetCustomAttribute(typeof(LayoutAttribute)) as LayoutAttribute;

                if(attributes.Type == type)
                {
                    // Cache the method info on the attribute instance
                    _layoutMethod = theMethod;
                    return;
                }
                
            }
        }

        public void Apply(EntityManager entityManager, int offsetX = 0, int offsetY = 0)
        {
            if(_layoutMethod != null)
            {
                _layoutMethod.Invoke(this, new object[] {entityManager, offsetX, offsetY});
            }
        }
        

    }

}