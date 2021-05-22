// TODO need a dedicate card scene that supports moving between cards, saveing UI/script data, and exporting/importing

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

using System.Collections.Generic;
using System.Text;

namespace PixelVision8.Player
{

    public class TinyCardField : TextEdtor
    {
        public TinyCardField(UIBuilder uiBuilder, Rectangle rect, string name = "", string font = "large", int spacing = 0, string tooltip = "") : base(uiBuilder, rect, name, font, spacing, tooltip)
        {
        }
    }
}