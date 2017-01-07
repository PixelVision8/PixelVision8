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
// Jesse Freeman
// 

namespace PixelVisionSDK.Engine.Chips.Game
{
    /// <summary>
    /// </summary>
    public interface IGameLoop : IUpdate, IDraw
    {
        /// <summary>
        ///     The Init() method for the game's live-cycle. This is the first
        ///     method called on a game when it is run.
        /// </summary>
        void Init();

        /// <summary>
        ///     This is called to when the game loads up after Init() and when the
        ///     engine is reset. Use this to store configuration logic for the game
        ///     you want to execute when a game is started/restarted.
        /// </summary>
        void Reset();
    }
}