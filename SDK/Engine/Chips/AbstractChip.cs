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

using System;

namespace PixelVision8.Engine.Chips
{
    /// <summary>
    ///     The AbstractChip represents plug-in that adds new functionality to the
    ///     PixelVisionEngine. Simply extend this abstract class and override the
    ///     Config() method. Each Chip is responsible for registering itself with
    ///     the engine and managing its own state. Chips can also communicate to
    ///     each other via the reference of the ChipManager which is part of the
    ///     PixelVisionEngine instance supplied to the Config() method.
    /// </summary>
    /// <example>
    ///     // Create a new class that extends the abstract chip
    ///     public class CustomChip : AbstractChip
    ///     {
    ///     // You need to override the Config method with your own custom setup code
    ///     public override Config()
    ///     {
    ///     // Add custom logic here
    ///     }
    ///     }
    ///     // Create a new instance of your chip
    ///     var chip = new CustomChip();
    ///     // When ready, activate the chip by passing in a reference to an engine instance
    ///     chip.Activate(engine);
    /// </example>
    public class AbstractChip
    {
        protected PixelVisionEngine engine;

        /// <summary>
        ///     A flag for the ChipManager to determine if the chip
        ///     should be part of the serialization process.
        /// </summary>
        /// <value>bool</value>
        public bool export = true;

        /// <summary>
        ///     Determines if the chip is enabled or active by the
        ///     ChipManager. If marked as active it may be deleted
        ///     when the ChipManager performs its cleanup.
        /// </summary>
        public bool active { get; private set; }

        /// <summary>
        ///     Activate is the beginning of the chip's life cycle.
        ///     This allows the chip to gain a reference to the engine
        ///     itself. This allows chips to talk back to the engine
        ///     as well as to each other through the engine's exposed APIs.
        /// </summary>
        /// <param name="parent">A reference to the engine.</param>
        public virtual void Activate(PixelVisionEngine parent)
        {
            engine = parent;
            active = true;
            Configure();
        }

        /// <summary>
        ///     Configure is the second part of the chip's life-cycle.
        ///     It is called after Activate() and is designed to be overridden by
        ///     children classes so perform specific configuration tasks. This
        ///     method must be implemented in order for a chip to activate
        ///     correctly.
        /// </summary>
        public virtual void Configure()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     When called, this method sets the active field to
        ///     false. It's part of the chip's life-cycle and is called when
        ///     shutting down the ChipManager.
        /// </summary>
        public virtual void Deactivate()
        {
            active = false;
        }

        public virtual void Init()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void Shutdown()
        {
        }
    }
}