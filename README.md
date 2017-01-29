# Pixel Vision 8 SDK

Pixel Vision 8's SDK is open source, allowing anyone to build authentic 8-bit C# games with it. Developers can use the SDK in any game engine that supports C# such as [Unity](https://unity3d.com) or [MonoGame](https://github.com/MonoGame/MonoGame). Pixel Vision 8's SDK also integrates with lower-level rendering engines such as [OpenTK](https://github.com/opentk/opentk). Finally, there is [a collection of demo games](https://gitlab.com/PixelVision8/Demos), [extensive documentation](https://gitlab.com/PixelVision8/SDK/wikis/home), and examples for building both a [UnityRunner](https://gitlab.com/PixelVision8/UnityRunner) or a [MonoGame Runner](https://gitlab.com/PixelVision8/MonoGameRunner) which show off how to use the SDK for your games.

## What is Pixel Vision 8

PV8's core philosophy is to teach retro game development with streamlined workflows. It enables designing games around limited resolutions, colors, sprites, sound and memory. It is ideal for game jams, prototyping ideas or having fun. 

Pixel Vision 8 is also a platform that standardizes 8-bit fantasy console limitations. Developers can customize these restrictions to match actual legacy hardware or create something new. The challenge of working within these confines forces creativity and limits scope. Pixel Vision 8 creations are expressions of ingenuity that rise above their limitations. 

## Features

Pixel Vision 8’s SDK contains a collection of components called Chips. Each Chip shares an API and [base class](https://gitlab.com/PixelVision8/SDK/wikis/Docs/PixelVisionSDK/Engine/Chips/AbstractChip) to standardize their lifecycle. A central [ChipManager](https://gitlab.com/PixelVision8/SDK/wikis/Docs/PixelVisionSDK/Engine/Chips/ChipManager) handles activating, updating and rendering them. Here is a list of the standard chips included with the SDK:

* [ColorChip](https://gitlab.com/PixelVision8/SDK/wikis/Docs/PixelVisionSDK/Engine/Chips/Graphics/Colors/ColorChip) - Manages system colors

* Display - Renders pixel data.

* [Font](https://gitlab.com/PixelVision8/SDK/wikis/Docs/PixelVisionSDK/Engine/Chips/Graphics/Sprites/FontChip) - Renders sprite based fonts.

* [GameChip](https://gitlab.com/PixelVision8/SDK/wikis/Docs/PixelVisionSDK/Engine/Chips/Game/GameChip) - A game's base class.

* [MusicChip](https://gitlab.com/PixelVision8/SDK/wikis/Docs/PixelVisionSDK/Engine/Chips/Audio/MusicChip) - A sequencer for playing back music.

* [ScreenBufferChip](https://gitlab.com/PixelVision8/SDK/wikis/Docs/PixelVisionSDK/Engine/Chips/Graphics/Display/ScreenBufferChip) - Caches sprites for tilemaps and fonts.

* [SoundChip](https://gitlab.com/PixelVision8/SDK/wikis/Docs/PixelVisionSDK/Engine/Chips/Audio/SoundChip) - A sound effect manager.

* [SpriteChip](https://gitlab.com/PixelVision8/SDK/wikis/Docs/PixelVisionSDK/Engine/Chips/Graphics/Sprites/SpriteChip) - Manages sprites in memory.

* [TileMap](https://gitlab.com/PixelVision8/SDK/wikis/Docs/PixelVisionSDK/Engine/Chips/Graphics/Sprites/TileMapChip) - Drawing sprites into the ScreenBufferChip.

The engine is completely modular allowing you to extend these Chips to add new functionality or create your own from the base classes and interfaces provided.

## Documentation

Pixel Vision 8 SDK is cleanly architected and well commented. Along with a straightforward set of APIs, there is [extensive documentation](https://gitlab.com/PixelVision8/SDK/wikis/home) on how to get started, deep dives into the engine's code and how to build each of the demos from scratch. More documentation is on the way to cover how to extend the core system, create custom Chips and create runners for different platforms.

## Demo Resources

The [Demo project](https://gitlab.com/PixelVision8/Demos) contains examples of how to use the [Pixel Vision 8 SDK](https://gitlab.com/PixelVision8/SDK) APIs. Each demo highlights a specific aspect of the engine and shows off use cases for drawing sprites, capturing input, working with tilemaps and more. These demos are in both the [Unity](https://gitlab.com/PixelVision8/UnityRunner) and [MonoGame](https://gitlab.com/PixelVision8/MonoGameRunner) Runner example projects as [git submodules](https://git-scm.com/docs/git-submodule). In addition to the pure C# code examples inside of the Scripts folder, there are also Lua examples inside of the Resources/LuaScripts folder. Lua games can be loaded at runtime and offer an alternative way to write PV8 games. These games require a special Lua Bridge included in both Runner projects.

The last thing to mention is that you may need to configure the included artwork based on the platform you are using. The [Unity](https://gitlab.com/PixelVision8/UnityRunner/blob/master/README.md) and [MonoGame](https://gitlab.com/PixelVision8/MonoGameRunner/blob/master/README.md) Runner projects readme files explain how to do this.

## Credits

Pixel Vision 8 was created by Jesse Freeman ([@jessefreeman](http://twitter.com/jessefreeman)) in collaboration with Pedro Medeiros ([@saint11](http://twitter.com/saint11)) for art and Christer Kaitila ([@McFunkypants](http://twitter.com/McFunkypants)) for music. With additional coding contributions by Shawn Rakowski ([@shwany](http://twitter.com/shwany)).

## License

Licensed under the [Microsoft Public License (MS-PL) License](https://opensource.org/licenses/MS-PL).  See LICENSE file in the project root for full license information. 

Pixel Vision 8 is Copyright (c) 2017 Jesse Freeman. All rights reserved.

