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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PixelVision8.Engine.Chips;
using Buttons = PixelVision8.Engine.Chips.Buttons;

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
        Point ReadMouseWheel();
        bool GetKeyUp(Keys key);
        bool GetKeyDown(Keys key);
        void MouseScale(float x, float y);
        void RegisterKeyInput();
        void RegisterControllers();
        void SetInputText(char character, Keys key);
        bool IsConnected(int id);
    }
}