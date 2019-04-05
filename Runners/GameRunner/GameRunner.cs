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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Runner.Services;
using SharpFileSystem;
using SharpFileSystem.FileSystems;
using Color = Microsoft.Xna.Framework.Color;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace PixelVision8.Runner
{
    public class GameRunner : DesktopRunner
    {
        public GameRunner(string rootPath, string autoRunPath = null) : base(rootPath, autoRunPath)
        {
        }
    }
}