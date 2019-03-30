//   
// Copyright (c) Jesse Freeman. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) License. 
// See LICENSE file in the project root for full license information. 
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman - @JesseFreeman
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany

using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;

namespace PixelVision8.Engine
{

    public interface IControllerChip : IUpdate
    {
        bool export { get; set; }
        bool ButtonReleased(Buttons buttonID, int controllerID = 0);
        bool ButtonDown(Buttons button, int controllerID = 0);
        Point ReadMousePosition();
        string ReadInputString();
        bool GetMouseButtonUp(int button);
        bool GetMouseButtonDown(int button);
        bool GetKeyUp(Keys key);
        bool GetKeyDown(Keys key);
        void MouseScale(float x, float y);
        void RegisterKeyInput();
        void RegisterControllers();
    }

}