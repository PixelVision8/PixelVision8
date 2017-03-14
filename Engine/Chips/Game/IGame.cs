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

namespace PixelVisionSDK.Chips
{
    /// <summary>
    ///     The <see cref="IGame" /> internal contains the API
    ///     calls for a game in the PixelVisionEngine.
    /// </summary>
    public interface IGame
    {
        /// <summary>
        ///     Name of the game.
        /// </summary>
        string name { get; set; }

        /// <summary>
        ///     Description of the game.
        /// </summary>
        string description { get; set; }

        /// <summary>
        ///     Flag telling if the game is <see cref="ready" /> or not.
        /// </summary>
        bool ready { get; }

        /// <summary>
        ///     The <see cref="SaveData" /> method allows you to store a string
        ///     <paramref name="value" /> as part of the game's persistent data. You
        ///     can associate the string <paramref name="value" /> with a string
        ///     <paramref name="key" /> for looking up later.
        /// </summary>
        /// <param name="key">A String key for the value.</param>
        /// <param name="value">
        ///     A String to be saved in the game's memory.
        /// </param>
        void SaveData(string key, string value);

        /// <summary>
        ///     The <see cref="SaveData" /> method allows you to store a
        ///     int <paramref name="value" /> as part of the game's
        ///     persistent data. You can associate the int
        ///     <paramref name="value" /> with a string <paramref name="key" /> for
        ///     looking up later.
        /// </summary>
        /// <param name="key">A String key for the value.</param>
        /// <param name="value">
        ///     An int to be saved in the game's memory.
        /// </param>
        void SaveData(string key, int value);

        /// <summary>
        ///     The <see cref="SaveData" /> method allows you to store a
        ///     float <paramref name="value" /> as part of the
        ///     game's persistent data. You can associate the
        ///     float <paramref name="value" /> with a string
        ///     <paramref name="key" /> for looking up later.
        /// </summary>
        /// <param name="key">A String key for the value.</param>
        /// <param name="value">
        ///     A float to be saved in the game's memory.
        /// </param>
        void SaveData(string key, float value);

        /// <summary>
        ///     The <see cref="GetData" /> method allows you to look up a String by
        ///     its key.
        /// </summary>
        /// <param name="key">The key associated with the string value.</param>
        /// <param name="defaultValue">
        ///     The default value to return if the <paramref name="key" /> does not
        ///     exist in memory.
        /// </param>
        /// <returns>
        ///     Returns the value as a string, even if it's not.
        /// </returns>
        string GetData(string key, string defaultValue);

        /// <summary>
        ///     The <see cref="GetData" /> method allows you to look up an
        ///     int by its key.
        /// </summary>
        /// <param name="key">
        ///     The key associated with theint value.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value to return if the <paramref name="key" /> does not
        ///     exist in memory.
        /// </param>
        /// <returns>
        ///     Returns the value as an int. It will be cast as an
        ///     int if it is not.
        /// </returns>
        int GetData(string key, int defaultValue);

        /// <summary>
        ///     The <see cref="GetData" /> method allows you to look up a
        ///     float by its key.
        /// </summary>
        /// <param name="key">
        ///     The key associated with the float value.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value to return if the <paramref name="key" /> does not
        ///     exist in memory.
        /// </param>
        /// <returns>
        ///     Returns the value as a float. It will be cast as a
        ///     float if it is not.
        /// </returns>
        float GetData(string key, float defaultValue);
    }
}