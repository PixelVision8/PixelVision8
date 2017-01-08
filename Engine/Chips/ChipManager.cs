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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PixelVisionSDK.Engine.Chips.Game;
using PixelVisionSDK.Engine.Chips.IO.File;

namespace PixelVisionSDK.Engine.Chips
{

    /// <summary>
    ///     The <see cref="ChipManager" /> is responsible for managing all of the
    ///     chips in the engine. It allows the engine to create chips from a string
    ///     of their full class name, important for deserialization, as well as
    ///     managing the life-cycle of chips as they are created and destroy in the
    ///     engine.
    /// </summary>
    public class ChipManager : IGameLoop, ISave, ILoad
    {

        protected Dictionary<string, AbstractChip> chips = new Dictionary<string, AbstractChip>();
        protected List<IDraw> drawChips = new List<IDraw>();
        protected PixelVisionEngine engine;
        protected List<IUpdate> updateChips = new List<IUpdate>();

        /// <summary>
        ///     The ChipManager constructor requires a reference to the engine for
        ///     correctly activating its chips.
        /// </summary>
        /// <param name="engine">A reference to the engine.</param>
        public ChipManager(PixelVisionEngine engine)
        {
            this.engine = engine;
        }

        /// <summary>
        ///     Reference to current time delta between the last render frame. This
        ///     is automatically passed into the ChipManager's Update() method but
        ///     is exposed publically to the APIBridge.
        /// </summary>
        /// <value>Float</value>
        public float timeDelta { get; private set; }

        /// <summary>
        ///     Loops through all chips and calls Init() method on them.
        /// </summary>
        public void Init()
        {
            foreach (var chip in chips)
            {
                chip.Value.Init();
            }
        }

        /// <summary>
        ///     Loops through all chips that implement the IUpdate interface
        ///     and calls the Update method on them.
        /// </summary>
        /// <param name="timeDelta"></param>
        public void Update(float timeDelta)
        {
            foreach (var chip in updateChips)
            {
                chip.Update(timeDelta);
            }
        }

        /// <summary>
        ///     Loops through all chips that implement the IDraw interface
        ///     and calls the Draw() method on them.
        /// </summary>
        public void Draw()
        {
            foreach (var chip in drawChips)
            {
                chip.Draw();
            }
        }

        /// <summary>
        ///     Loops throgh all chips and calls reset on them.
        /// </summary>
        public void Reset()
        {
            foreach (var chip in chips)
            {
                chip.Value.Reset();
            }
        }

        /// <summary>
        ///     This method reads a Dictionary's properties and applies them to any
        ///     <see cref="chips" /> registered in the manager.
        /// </summary>
        /// <param name="data">
        ///     A Dictionary with a string for the key and a generic object for the
        ///     value.
        /// </param>
        public void DeserializeData(Dictionary<string, object> data)
        {
            // Disable any active chips
            DeactivateChips();

            foreach (var entry in data)
            {
                var chipName = entry.Key;
                var chipData = entry.Value as Dictionary<string, object>;

                UpdateChip(chipName, chipData);
            }

            // Removed any active chips not reserialized
            RemoveInactiveChips();
        }

        /// <summary>
        ///     This method serializes all of the chips in the
        ///     manager. Each chip is added to a json object allowing the
        ///     ChipManager to be easily reserialized at run-time.
        /// </summary>
        /// <returns name="String">
        ///     Returns a JSON string containing all of the chips in
        ///     the manager.
        /// </returns>
        public string SerializeData()
        {
            // Create a new string builder instance
            var sb = new StringBuilder();

            // Start the json string
            sb.Append("{");

            CustomSerializedData(sb);

            sb.Append("}");

            return sb.ToString();
        }

        /// <summary>
        ///     Loops through each of the <see cref="chips" /> flagged for export and
        ///     adds them to the StringBuilder instance.
        /// </summary>
        /// <param name="sb">
        ///     Reference of a StringBuilder used by the SerializeData() method.
        /// </param>
        public void CustomSerializedData(StringBuilder sb)
        {
            // Select the chips that are flagged to be exported
            var chipsToSave = chips.Where(c => c.Value.export).ToArray();

            // Get total number of chips to export
            var total = chipsToSave.Length;

            // Loop through all the chips to export
            for (var i = 0; i < total; i++)
            {
                var item = chipsToSave.ElementAt(i);
                sb.Append("\"" + item.Key + "\":");
                sb.Append(item.Value.SerializeData());
                if (i < total - 1)
                    sb.Append(",");
            }
        }

        /// <summary>
        ///     This method tests to see if a chip exists in the manager.
        /// </summary>
        /// <param name="id">
        ///     Requires the string id used to register the chip. By default, this
        ///     will be the fully qualified name of the class.
        /// </param>
        /// <returns name="Boolean">
        ///     Returns true if the chip exists or
        ///     false if it does not.
        /// </returns>
        public bool HasChip(string id)
        {
            return chips.ContainsKey(id);
        }

        /// <summary>
        ///     Gets a reference to a chip in the manager. This method will attempt
        ///     to create the chip automatically. You can optionally chose to
        ///     activate it or not when a new instance is created. Use this method
        ///     for creating new <see cref="chips" /> at run time.
        /// </summary>
        /// <param name="id">
        ///     The name of the chip. This should be the fully qualified class name
        ///     if you want to automatically create the instance if one doesn't
        ///     exist.
        /// </param>
        /// <param name="activeOnCreate">
        ///     Optional value to have method automatically activate any
        ///     chips it creates. This is set to
        ///     true by default.
        /// </param>
        /// <returns name="AbstractChip">
        ///     Returns a reference to any chips associated with the supplied ID
        /// </returns>
        public AbstractChip GetChip(string id, bool activeOnCreate = true)
        {
            //Debug.Log("Chip Manager: Get Chip " + id);

            if (HasChip(id))
            {
                var chip = chips[id];

                if (activeOnCreate)
                    ActivateChip(id, chip);

                return chip;
            }

            // TODO create a chip
            var chipInstance = Activator.CreateInstance(Type.GetType(id)) as AbstractChip;

            ActivateChip(id, chipInstance);

            //Debug.Log("Chip Manager: Create new instance of " + id);

            return chipInstance;
        }

        /// <summary>
        ///     This method allows you to update the <paramref name="data" /> in the
        ///     chip by passing in new values to a chip's DeserializeData() method.
        ///     It also reactivates the chip as if it was created from scratch.
        /// </summary>
        /// <param name="id">
        ///     The name of the chip. This should be the fully qualified class name
        ///     if you want to automatically create the instance if one doesn't
        ///     exist.
        /// </param>
        /// <param name="data">
        ///     A Dictonary with a string key and generic object value. Used to pass
        ///     in new properties to the chip.
        /// </param>
        public void UpdateChip(string id, Dictionary<string, object> data)
        {
            var chip = GetChip(id);

            if (chip == null)
                return;

            chip.Activate(engine);
            chip.DeserializeData(data);
        }

        /// <summary>
        ///     This method flags all <see cref="chips" /> to be Deactivated. Used
        ///     when the ChipManager is being shut down or the <see cref="engine" />
        ///     is being restarted.
        /// </summary>
        public void DeactivateChips()
        {
            foreach (var item in chips)
            {
                DeactivateChip(item.Key, item.Value);
            }
        }

        /// <summary>
        ///     This method calls the Activate method on a <paramref name="chip" />
        ///     if it exists in the manager. This method can be used to register
        ///     <see cref="chips" /> as well as register them with unique IDs. Chips
        ///     registered without class names will not be restored correctly
        ///     through the serialization API.
        /// </summary>
        /// <param name="id">
        ///     The name of the chip. This should be the fully qualified class name
        ///     if you want to automatically create the instance if one doesn't
        ///     exist. The supplied <paramref name="chip" /> will be registered to
        ///     this id.
        /// </param>
        /// <param name="chip">
        ///     Instance to the chip that needs to be activated.
        /// </param>
        public void ActivateChip(string id, AbstractChip chip)
        {
            if (HasChip(id))
            {
                //TODO do we need to dissable the old chip first
                chips[id] = chip;
            }
            else
            {
                chips.Add(id, chip);
            }

            if (chip is IUpdate)
                updateChips.Add(chip as IUpdate);

            if (chip is IDraw)
                drawChips.Add(chip as IDraw);

            chip.Activate(engine);
        }

        /// <summary>
        ///     This deactivates a chip and safely removed it from the manager.
        /// </summary>
        /// <param name="id">Name of the chip.</param>
        /// <param name="chip">Reference to the chip.</param>
        public void DeactivateChip(string id, AbstractChip chip)
        {
            chip.Deactivate();

            if (chip is IUpdate)
                updateChips.Remove(chip as IUpdate);

            if (chip is IDraw)
                drawChips.Remove(chip as IDraw);

            chips.Remove(id);
        }

        /// <summary>
        ///     Removed any <see cref="chips" /> that are no longer active
        /// </summary>
        public void RemoveInactiveChips()
        {
            foreach (var item in chips.Where(c => c.Value.active == false).ToArray())
            {
                chips.Remove(item.Key);
            }
        }

    }

}