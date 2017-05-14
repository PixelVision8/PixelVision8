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
    ///     The <see cref="IEngineChips" /> internal represents
    ///     all of the core chips the engine needs to run.
    /// </summary>
    public interface IEngineChips
    {
        /// <summary>
        ///     The Chip Manager class is responsible for registering and
        ///     deactivating chips. This property offers direct access to it.
        /// </summary>
        ChipManager chipManager { get; set; }

        /// <summary>
        ///     The color chips stores colors for the engine. It's used by several
        ///     other systems to properly convert pixel data into color data for the
        ///     display. This property offers direct access to it.
        /// </summary>
        ColorChip colorChip { get; set; }

        /// <summary>
        ///     The Color Map Chip allows you to remap colors from artwork imported
        ///     into the engine at run time. This property offers direct access to
        ///     it.
        /// </summary>
        ColorMapChip colorMapChip { get; set; }

        /// <summary>
        ///     The controller chip handles all input to the engine. This property
        ///     offers direct access to it.
        /// </summary>
        ControllerChip controllerChip { get; set; }

        /// <summary>
        ///     The Display Chip handles all rendering for the engine. This property
        ///     offers direct access to it.
        /// </summary>
        DisplayChip displayChip { get; set; }

        /// <summary>
        ///     The Sound Chip stores and manages playback of sound effects in the
        ///     game engine. This property offers direct access to it.
        /// </summary>
        SoundChip soundChip { get; set; }

        /// <summary>
        ///     The Sprite Chip stores all the sprite as pixel data for the engine.
        ///     This property offers direct access to it.
        /// </summary>
        SpriteChip spriteChip { get; set; }

        /// <summary>
        ///     The Tile Map Chip stores references to Sprites as tile map data
        ///     making it easy to create levels from sprites and handle collision
        ///     detection via flags. This property offers direct access to it.
        /// </summary>
        TilemapChip tilemapChip { get; set; }

        /// <summary>
        ///     This property offers direct access to the current game loaded into
        ///     the engine's memory.
        /// </summary>
        GameChip currentGame { get; set; }

        /// <summary>
        ///     The API bridge is the connection between both the engine and the
        ///     chips. It's used by the game to simply access to each system of the
        ///     engine. This property offers direct access to it.
        /// </summary>
        APIBridge apiBridge { get; set; }

        /// <summary>
        ///     The Fonts Chip is responsible for rendering text to the display.
        ///     This property offers direct access to it.
        /// </summary>
        FontChip fontChip { get; set; }

        /// <summary>
        ///     The music chip represents a sequencer that can be used to play back
        ///     sound data for songs.
        /// </summary>
        MusicChip musicChip { get; set; }
    }
}