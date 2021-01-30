/*
 Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.

 Licensed under the Microsoft Public License (MS-PL) except for a few
 portions of the code. See LICENSE file in the project root for full
 license information. Third-party libraries used by Pixel Vision 8 are
 under their own licenses. Please refer to those libraries for details
 on the license they use.

 Contributors
 --------------------------------------------------------
 This is the official list of Pixel Vision 8 contributors:

 - Jesse Freeman - @jessefreeman
 - Christina-Antoinette Neofotistou @castpixel
 - Christer Kaitila - @mcfunkypants
 - Pedro Medeiros - @saint11
 - Shawn Rakowski - @shwany

*/

#region Imports

using PixelVision8.Player;
using System;
using Microsoft.Xna.Framework;

#endregion

// This is a Test
//
// to see how much I can put in a comment
namespace PixelVision8.Player
{
    public partial interface IPlayerChips
    {
        /// <summary>
        ///     This property offers direct access to the current game loaded into
        ///     the engine's memory.
        /// </summary>
        GameChip GameChip { get; set; }

        int SpriteCounter { get; set; }
    }

    public partial class PixelVision
    {
        public GameChip GameChip { get; set; }
    }

    /// <summary>
    ///     The GameChip represents the foundation of a game class
    ///     with all the logic it needs to work correctly in the PixelVisionEngine.
    ///
    ///     The AbstractChip class manages configuring the game when created via the
    ///     chip life-cycle. The engine manages the game's state, the game's own life-cycle and
    ///     serialization/deserialization of the game's data.
    /// </summary>
    public partial class GameChip : AbstractChip, IUpdate, IDraw
    {
        protected ColorChip ColorChip => Player.ColorChip;
        protected DisplayChip DisplayChip => Player.DisplayChip;

        #region Lifecycle

        public override void Configure()
        {
            // Set the engine's game to this instance
            Player.GameChip = this;
        }

        /// <summary>
        ///     Update() is called once per frame at the beginning of the game loop. This is where you should put all
        ///     non-visual game logic such as character position calculations, detecting input and performing updates to
        ///     your animation system. The time delta is provided on each frame so you can calculate the difference in
        ///     milliseconds since the last render took place.
        /// </summary>
        /// <label>
        ///     Runner
        /// </label>
        /// <param name="timeDelta">A float value representing the time in milliseconds since the last Draw() call was completed.</param>
        public virtual void Update(int timeDelta)
        {
            // Override this method and add your own update logic.
        }

        /// <summary>
        ///     Draw() is called once per frame after the Update() has completed. This is where all visual updates to
        ///     your game should take place such as clearing the display, drawing sprites, and pushing raw pixel data
        ///     into the display.
        /// </summary>
        /// <label>
        ///     Runner
        /// </label>
        public virtual void Draw()
        {
            // Override this method and add your own draw logic.
        }

        /// <summary>
        ///     Clearing the display removed all of the existing pixel data, replacing it with the default background
        ///     color. The Clear() method allows you specify what region of the display to clear. By simply calling
        ///     Clear(), with no arguments, it automatically clears the entire display. You can manually define an area
        ///     of the screen to clear by supplying option x, y, width and height arguments. When clearing a specific
        ///     area of the display, anything outside of the defined boundaries remains on the next draw phase. This is
        ///     useful for drawing a HUD but clearing the display below for a scrolling map and sprites. Clear can only
        ///     be used once during the draw phase.
        /// </summary>
        public void Clear()
        {
            DisplayChip.Clear(ColorChip.BackgroundColor);
        }

        public override void Deactivate()
        {
            base.Deactivate();
            Player.GameChip = null;
        }

        /// <summary>
        ///     Shutdown() is called when quitting a game or shutting down the Runner/Game Creator. This hook allows you
        ///     to perform any last minute changes to the game's data such as saving or removing any temp files that
        ///     will not be needed.
        /// </summary>
        public override void Shutdown()
        {
            // Put save logic here
        }
        
        #endregion
    }
}