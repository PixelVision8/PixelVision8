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

using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace PixelVision8.Player
{
    /// <summary>
    ///     The GameChip represents the foundation of a game class
    ///     with all the logic it needs to work correctly in the PixelVisionEngine.
    ///     The AbstractChip class manages configuring the game when created via the
    ///     chip life-cycle. The engine manages the game's state, the game's own life-cycle and
    ///     serialization/deserialization of the game's data.
    /// </summary>
    public partial class GameChip
    {
        protected int _saveSlots;
        public Dictionary<string, string> savedData = new Dictionary<string, string>();

        #region GameChip Properties

        /// <summary>
        ///     Used to limit the amount of data the game can save.
        /// </summary>
        public int SaveSlots
        {
            get => _saveSlots;
            set
            {
                value = MathHelper.Clamp(value, 2, 16);
                _saveSlots = value;

                // resize dictionary?
                for (var i = savedData.Count - 1; i >= 0; i--)
                {
                    var item = savedData.ElementAt(i);
                    if (i > value) savedData.Remove(item.Key);
                }
            }
        }

        #endregion

        #region File IO

        /// <summary>
        ///     Allows you to save string data to the game file itself. This data persistent even after restarting a game.
        /// </summary>
        /// <param name="key">
        ///     A string to use as the key for the data.
        /// </param>
        /// <param name="value">
        ///     A string representing the data to be saved.
        /// </param>
        public void WriteSaveData(string key, string value)
        {
            if (savedData.Count > SaveSlots) return;

            if (savedData.ContainsKey(key))
            {
                savedData[key] = value;
                return;
            }

            savedData.Add(key, value);
        }

        /// <summary>
        ///     Allows you to read saved data by supplying a key. If no matching key exists, "undefined" is returned.
        /// </summary>
        /// <param name="key">
        ///     The string key used to find the data.
        /// </param>
        /// <param name="defaultValue">
        ///     The optional string to use if data does not exist.
        /// </param>
        /// <returns>
        ///     Returns string data associated with the supplied key.
        /// </returns>
        public string ReadSaveData(string key, string defaultValue = "undefined")
        {
            if (!savedData.ContainsKey(key)) WriteSaveData(key, defaultValue);

            return savedData[key];
        }

        #endregion

        /// <summary>
        ///     Reads the meta data that was passed into the game when it was loaded.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string ReadMetadata(string key, string defaultValue = "undefined")
        {
            return ((PixelVision) Player).GetMetadata(key, defaultValue);
        }

        /// <summary>
        ///     Writes meta data back into the game which can be read if the game reloads.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void WriteMetadata(string key, string value)
        {
            ((PixelVision) Player).SetMetadata(key, value);
        }

        public Dictionary<string, string> ReadAllMetadata()
        {
            var tmpMetadata = new Dictionary<string, string>();

            ((PixelVision) Player).ReadAllMetadata(tmpMetadata);

            return tmpMetadata;
        }
    }
}