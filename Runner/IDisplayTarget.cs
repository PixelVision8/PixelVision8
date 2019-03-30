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

using PixelVision8.Engine;

namespace PixelVision8.Runner
{
    public interface IDisplayTarget
    {
        void ResetResolution(IEngine activeEngine, int monitorScale, bool fullscreen, bool matchResolution, bool stretch);
        void Render(IEngine engine, bool ignoreTransparent = false);
//        void CacheColors(IEngine engine);
//        void ConvertMousePosition(Vector pos);
    }
}
