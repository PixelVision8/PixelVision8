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

using System.Collections.Generic;
using PixelVisionSDK.Chips;

namespace PixelVisionSDK
{

    /// <summary>
    ///     The <see cref="IEngine" /> internal represents the
    ///     core API for the Engine class. Implement this
    ///     internal to create custom engine classes.
    /// </summary>
    public interface IEngine : IEngineChips, IGameLoop
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        string GetMetaData(string key, string defaultValue = "");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetMetaData(string key, string value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="ignoreKeys"></param>
        void DumpMetaData(Dictionary<string, string> target, string[] ignoreKeys);

    }

}