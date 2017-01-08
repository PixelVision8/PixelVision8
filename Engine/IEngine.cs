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
// 

using PixelVisionSDK.Engine.Chips;
using PixelVisionSDK.Engine.Chips.Game;
using PixelVisionSDK.Engine.Chips.IO.File;

namespace PixelVisionSDK.Engine
{

    /// <summary>
    ///     The <see cref="IEngine" /> internal represents the
    ///     core API for the Engine class. Implement this
    ///     internal to create custom engine classes.
    /// </summary>
    public interface IEngine : IEngineChips, IGameLoop, ISave, ILoad
    {

        /// <summary>
        ///     A flag for if the engine is <see cref="running" />
        /// </summary>
        bool running { get; }

        string name { get; set; }

        /// <summary>
        ///     This method loads a <paramref name="game" /> into memory
        /// </summary>
        /// <param name="game"></param>
        void LoadGame(GameChip game);

        /// <summary>
        ///     Run the game in memory
        /// </summary>
        void RunGame();

        /// <summary>
        /// </summary>
        void Shutdown();

    }

}