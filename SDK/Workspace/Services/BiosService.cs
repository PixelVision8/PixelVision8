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

using PixelVision8.Runner;
using System.Collections.Generic;

namespace PixelVision8.Runner
{
    public class BiosService
    {
        //        #region Bios APIs

        private readonly Dictionary<string, string> biosData;

        public Dictionary<string, string> userBiosChanges;

        public BiosService()
        {
            // Create a new dictionary to store bios settings
            biosData = new Dictionary<string, string>();
        }

        public void ParseBiosText(string json)
        {
            try
            {
                var data = Json.Deserialize(json) as Dictionary<string, object>;

                foreach (var pair in data)
                    if (biosData.ContainsKey(pair.Key))
                        biosData[pair.Key] = pair.Value.ToString();
                    else
                        biosData.Add(pair.Key, pair.Value.ToString());
            }
            catch
            {
                //                Console.WriteLine("Bios Error:\n"+e.Message);
                // TODO ignore the bios file if it can't be read
            }
        }

        /// <summary>
        ///     Modifies the bios in memory. Changes are saved to the current bios and are stored in a userBiosChanges var to make
        ///     saving changes easier when shutting down the workspace
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void UpdateBiosData(string key, string value)
        {
            // Update internal bios value
            if (biosData.ContainsKey(key))
                biosData[key] = value;
            else
                biosData.Add(key, value);

            if (userBiosChanges == null) userBiosChanges = new Dictionary<string, string>();

            // Update the user bios and save it
            if (userBiosChanges.ContainsKey(key))
                userBiosChanges[key] = value;
            else
                userBiosChanges.Add(key, value);
        }

        public string ReadBiosData(string key, string defaultValue = "", bool autoSave = false)
        {
            // Check to see if the key is in the bios
            if (!biosData.ContainsKey(key))
            {
                // If the key doesn't exist, test if we should save the default value
                if (autoSave) UpdateBiosData(key, defaultValue);

                // return the default value
                return defaultValue;
            }

            // If the key exists, use the key's value
            return biosData[key];
        }

        public void ClearUserChanges()
        {
            userBiosChanges.Clear();
        }

        public void Clear(bool clearUserChanges = true)
        {
            biosData.Clear();
            ClearUserChanges();
        }
    }
}