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

using System.Collections.Generic;
using PixelVision8.Engine;
using PixelVision8.Engine.Services;

namespace PixelVision8.Runner
{
    public interface IRunner
    {
        IEngine ActiveEngine { get; }
        void ProcessFiles(IEngine tmpEngine, string[] files, bool displayProgress = false);
        void DisplayWarning(string message);
        int Volume(int? value = null);
        bool Mute(bool? value = null);
        int Scale(int? scale = null);
        bool Fullscreen(bool? value = null);
        bool StretchScreen(bool? value = null);
        bool CropScreen(bool? value = null);
        // void DebugLayers(bool value);
        // void ToggleLayers(int value);
        // void ResetGame();
        IServiceLocator ServiceManager { get;}

    }
}