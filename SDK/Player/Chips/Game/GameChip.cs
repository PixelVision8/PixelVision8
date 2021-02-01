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

// This is a Test
//
// to see how much I can put in a comment

using Microsoft.Xna.Framework;

namespace PixelVision8.Player
{
    
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
        private ColorChip ColorChip => Player.ColorChip;
        public DisplayChip DisplayChip => Player.DisplayChip;

        #region Lifecycle

        protected override void Configure()
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
            DisplayChip.Clear(Player.ColorChip.BackgroundColor);
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
        
        /// <summary>
        ///     The background color is used to fill the screen when clearing the display. You can use
        ///     this method to read or update the background color at any point during the GameChip's
        ///     draw phase. When calling BackgroundColor(), without an argument, it returns the current
        ///     background color int. You can pass in an optional int to update the background color by
        ///     calling BackgroundColor(0) where 0 is any valid ID in the ColorChip. Passing in a value
        ///     such as -1, or one that is out of range, defaults the background color to magenta (#ff00ff)
        ///     which is the engine's default transparent color.
        /// </summary>
        /// <param name="id">
        ///     This argument is optional. Supply an int to update the existing background color value.
        /// </param>
        /// <returns>
        ///     This method returns the current background color ID. If no color exists, it returns -1
        ///     which is magenta (#FF00FF).
        /// </returns>
        public int BackgroundColor(int? id = null)
        {
            if (id.HasValue) ColorChip.BackgroundColor = id.Value;

            return ColorChip.BackgroundColor;
        }

        /// <summary>
        ///     The Color() method allows you to read and update color values in the ColorChip. This
        ///     method has two modes which require a color ID to work. By calling the method with just
        ///     an ID, like Color(0), it returns a hex string for the given color at the supplied color
        ///     ID. By passing in a new hex string, like Color(0, "#FFFF00"), you can change the color
        ///     with the given ID. While you can use this method to modify color values directly, you
        ///     should avoid doing this at run time since the DisplayChip must parse and cache the new
        ///     hex value. If you just want to change a color to an existing value, use the ReplaceColor()
        ///     method.
        /// </summary>
        /// <param name="id">
        ///     The ID of the color you want to access.
        /// </param>
        /// <param name="value">
        ///     This argument is optional. It accepts a hex as a string and updates the supplied color ID's value.
        /// </param>
        /// <returns>
        ///     This method returns a hex string for the supplied color ID. If the color has not been set
        ///     or is out of range, it returns magenta (#FF00FF) which is the default transparent system color.
        /// </returns>
        public string Color(int id, string value = null)
        {
            if (value == null) return ColorChip.ReadColorAt(id);

            ColorChip.UpdateColorAt(id, value);

            return value;
        }

        /// <summary>
        ///     The TotalColors() method simply returns the total number of colors in the ColorChip. By default,
        ///     it returns only colors that have been set to value other than magenta (#FF00FF) which is the
        ///     default transparent value used by the engine. By calling TotalColors(false), it returns the total
        ///     available color slots in the ColorChip.
        /// </summary>
        /// <param name="ignoreEmpty">
        ///     This is an optional value that defaults to true. When set to true, the ColorChip returns the total
        ///     number of colors not set to magenta (#FF00FF). Set this value to false if you want to get all of
        ///     the available color slots in the ColorChip regardless if they are empty or not.
        /// </param>
        /// <returns>
        ///     This method returns the total number of colors in the color chip based on the ignoreEmpty argument's
        ///     value.
        /// </returns>
        public int TotalColors(bool ignoreEmpty = false)
        {
            return ignoreEmpty ? ColorChip.TotalUsedColors : ColorChip.Total;
        }

        /// <summary>
        ///     The ReplaceColor() method allows you to quickly change a color to an existing color without triggering
        ///     the DisplayChip to parse and cache a new hex value. Consider this an alternative to the Color() method.
        ///     It is useful for simulating palette swapping animation on sprites pointed to a fixed group of color IDs.
        ///     Simply call the ReplaceColor() method and supply a target color ID position, then the new color ID it
        ///     should point to. Since you are only changing the color's ID pointer, there is little to no performance
        ///     penalty during the GameChip's draw phase.
        /// </summary>
        /// <param name="index">The ID of the color you want to change.</param>
        /// <param name="id">The ID of the color you want to replace it with.</param>
        public void ReplaceColor(int index, int id)
        {
            ColorChip.UpdateColorAt(index, ColorChip.ReadColorAt(id));
        }
        
        /// <summary>
        ///     The display's size defines the visible area where pixel data exists on the screen. Calculating this is
        ///     important for knowing how to position sprites on the screen. The Display() method allows you to get
        ///     the resolution of the display at run time. By default, this will return the visble screen area based on
        ///     the overscan value set on the display chip. To calculate the exact overscan in pixels, you must subtract
        ///     the full size from the visible size. Simply supply false as an argument to get the full display dimensions.
        /// </summary>
        public Point Display()
        {
            return new Point(DisplayChip.Width, DisplayChip.Height);
        }
    }
}