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
// 

using PixelVisionSDK.Chips;

namespace PixelVisionSDK
{

    public interface IControllerChip : IUpdate
    {

        int totalControllers { get; }
        bool ButtonDown(int buttonID, int controller = 0);
        bool ButtonReleased(int buttonID, int controller = 0);
        bool MouseButtonDown(int id = 0);
        bool MouseButtonUp(int id = 0);
        bool GetMouseButton(int id = 0);
        Vector MousePos();
        int[] ReadControllerKeys(int controllerID = 0);
        void UpdateControllerKey(int controllerID, Buttons button, int key);

    }

}