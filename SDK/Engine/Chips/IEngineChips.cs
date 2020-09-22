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

using PixelVision8.Engine.Services;

namespace PixelVision8.Engine.Chips
{
    /// <summary>
    ///     The <see cref="IEngineChips" /> internal represents
    ///     all of the core chips the engine needs to run.
    /// </summary>
    public interface IEngineChips
    {
        /// <summary>
        ///     The color chips stores colors for the engine. It's used by several
        ///     other systems to properly convert pixel data into color data for the
        ///     display. This property offers direct access to it.
        /// </summary>
        ColorChip ColorChip { get; set; }

        /// <summary>
        ///     The controller chip handles all input to the engine. This property
        ///     offers direct access to it.
        /// </summary>
        IControllerChip ControllerChip { get; set; }

        /// <summary>
        ///     The Display Chip handles all rendering for the engine. This property
        ///     offers direct access to it.
        /// </summary>
        DisplayChip DisplayChip { get; set; }

        /// <summary>
        ///     The Sound Chip stores and manages playback of sound effects in the
        ///     game engine. This property offers direct access to it.
        /// </summary>
        SoundChip SoundChip { get; set; }

        /// <summary>
        ///     The Sprite Chip stores all the sprite as pixel data for the engine.
        ///     This property offers direct access to it.
        /// </summary>
        SpriteChip SpriteChip { get; set; }

        /// <summary>
        ///     The Tile Map Chip stores references to Sprites as tile map data
        ///     making it easy to create levels from sprites and handle collision
        ///     detection via flags. This property offers direct access to it.
        /// </summary>
        TilemapChip TilemapChip { get; set; }

        /// <summary>
        ///     This property offers direct access to the current game loaded into
        ///     the engine's memory.
        /// </summary>
        GameChip GameChip { get; set; }

        /// <summary>
        ///     The Fonts Chip is responsible for rendering text to the display.
        ///     This property offers direct access to it.
        /// </summary>
        FontChip FontChip { get; set; }

        /// <summary>
        ///     The music chip represents a sequencer that can be used to play back
        ///     sound data for songs.
        /// </summary>
        MusicChip MusicChip { get; set; }

        AbstractChip GetChip(string id, bool activeOnCreate = true);

        void AddService(string id, IService service);

        IService GetService(string id);

        bool HasChip(string id);

        void ActivateChip(string id, AbstractChip chip, bool autoActivate = true);

        void DeactivateChip(string id, AbstractChip chip);

        void RemoveInactiveChips();
    }
}