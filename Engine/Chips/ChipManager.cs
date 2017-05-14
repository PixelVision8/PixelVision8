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

using System;
using System.Collections.Generic;
using System.Linq;
using PixelVisionSDK.Services;

namespace PixelVisionSDK.Chips
{
    /// <summary>
    ///     The <see cref="ChipManager" /> is responsible for managing all of the
    ///     chips in the engine. It allows the engine to create chips from a string
    ///     of their full class name, important for deserialization, as well as
    ///     managing the life-cycle of chips as they are created and destroy in the
    ///     engine.
    /// </summary>
    public class ChipManager : IServiceLocator
    {
        protected Dictionary<string, IService> _services = new Dictionary<string, IService>();

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
            var chipNames = chips.Keys.ToList();

            foreach (var chipName in chipNames)
                chips[chipName].Init();
//            foreach (var chip in chips)
//            {
//                chip.Value.Init();
//            }
        }

        /// <summary>
        ///     Loops through all chips that implement the IUpdate interface
        ///     and calls the Update method on them.
        /// </summary>
        /// <param name="timeDelta"></param>
        public void Update(float timeDelta)
        {
            foreach (var chip in updateChips)
                chip.Update(timeDelta);
        }

        /// <summary>
        ///     Loops through all chips that implement the IDraw interface
        ///     and calls the Draw() method on them.
        /// </summary>
        public void Draw()
        {
            foreach (var chip in drawChips)
                chip.Draw();
        }

        /// <summary>
        ///     Loops through all chips and calls reset on them.
        /// </summary>
        public void Reset()
        {
            foreach (var chip in chips)
                chip.Value.Reset();
        }

        public Dictionary<string, IService> services
        {
            get { return _services; }
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="service"></param>
        public void AddService(string id, IService service)
        {
            // Add the service to the managed list
            if (services.ContainsKey(id))
                services[id] = service;
            else
                services.Add(id, service);

            // Add a reference of the service locator
            service.RegisterService(this);
        }

        public IService GetService(string id)
        {
            if (services.ContainsKey(id))
                return services[id];

            throw new ApplicationException("The requested service '" + id + "' is not registered");
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
            var type = Type.GetType(id);

            AbstractChip chipInstance = null;

            try
            {
                chipInstance = Activator.CreateInstance(type) as AbstractChip;
                ActivateChip(id, chipInstance);
            }
            catch (Exception)
            {
                //throw new Exception("Chip '" + id + "' could not be created.");
            }

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
        ///     A Dictionary with a string key and generic object value. Used to pass
        ///     in new properties to the chip.
        /// </param>
        public virtual void UpdateChip(string id, Dictionary<string, object> data)
        {
            var chip = GetChip(id);

            if (chip == null)
                return;

            chip.Activate(engine);
        }

        /// <summary>
        ///     This method flags all <see cref="chips" /> to be Deactivated. Used
        ///     when the ChipManager is being shut down or the <see cref="engine" />
        ///     is being restarted.
        /// </summary>
        public void DeactivateChips()
        {
            foreach (var item in chips)
                DeactivateChip(item.Key, item.Value);
        }

        /// <summary>
        ///     This method calls the Activate method on a chip
        ///     if it exists in the manager. This method can be used to register
        ///     chips as well as register them with unique IDs. Chips
        ///     registered without class names will not be restored correctly
        ///     through the serialization API.
        /// </summary>
        /// <param name="id">
        ///     The name of the chip. This should be the fully qualified class name
        ///     if you want to automatically create the instance if one doesn't
        ///     exist. The supplied chip will be registered to
        ///     this id.
        /// </param>
        /// <param name="chip">
        ///     Instance to the chip that needs to be activated.
        /// </param>
        public void ActivateChip(string id, AbstractChip chip, bool autoActivate = true)
        {
            if (HasChip(id))
            {
                //TODO do we need to disable the old chip first
                chips[id] = chip;
            }
            else
            {
                //TODO fixed bug here but need to make sure we don't need to do this above
                chips.Add(id, chip);

                if (chip is IUpdate)
                    updateChips.Add(chip as IUpdate);

                if (chip is IDraw)
                    drawChips.Add(chip as IDraw);
            }

            if (autoActivate)
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
                chips.Remove(item.Key);
        }
    }
}