using System;
using System.Collections.Generic;
using MiniJSON;

namespace PixelVision8.Runner.Services
{
    public class BiosService
    {

        public BiosService()
        {
            // Create a new dictionary to store bios settings
            biosData = new Dictionary<string, object>();
            
        }
        
//        #region Bios APIs
        
        private Dictionary<string, object> biosData;

        public void ParseBiosText(string json)
        {
            try
            {
                var data = Json.Deserialize(json) as Dictionary<string, object>;

                foreach (var pair in data)
                {
                    if (biosData.ContainsKey(pair.Key))
                    {
                        biosData[pair.Key] = pair.Value;
                    }
                    else
                    {
                        biosData.Add(pair.Key, pair.Value);
                    }
                }
                
            }
            catch
            {
//                Console.WriteLine("Bios Error:\n"+e.Message);
                // TODO ignore the bios file if it can't be read
            }
            
        }

        public Dictionary<string, object> userBiosChanges;

        /// <summary>
        ///     Modifies the bios in memory. Changes are saved to the current bios and are stored in a userBiosChanges var to make
        ///     saving changes easier when shutting down the workspace
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void UpdateBiosData(string key, object value)
        {
            // Update internal bios value
            if (biosData.ContainsKey(key))
                biosData[key] = value;
            else
                biosData.Add(key, value);

            if (userBiosChanges == null)
            {
                // Create a new dictionary to store any changes to the bios
                userBiosChanges = new Dictionary<string, object>();
            }
            
            // Update the user bios and save it
            if (userBiosChanges.ContainsKey(key))
                userBiosChanges[key] = value;
            else
                userBiosChanges.Add(key, value);
        }

        public object ReadBiosData(string key, object defaultValue, bool autoSave = false)
        {
            // Check to see if the key is in the bios
            if (!biosData.ContainsKey(key))
            {
                // If the key doesn't exist, test if we should save the default value
                if (autoSave)
                    UpdateBiosData(key, defaultValue);

                // return the default value
                return defaultValue;
            }

            // If the key exists, use the key's value
            return biosData[key];
        }
        
    }
}