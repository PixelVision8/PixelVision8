# Pixel Vision 8: SDK

[Pixel Vision 8](http://pixelvision8.com) is a *fantasy game console* for making authentic 8-bit games, art, and music

PV8's core philosophy is to teach retro game development with streamlined workflows. It enables designing games around limited resolutions, colors, sprites, sound and memory. It is ideal for game jams, prototyping ideas or having fun. 

Pixel Vision 8 is also a platform that standardizes 8-bit fantasy console limitations. Developers can customize these restrictions to match actual legacy hardware or create something new. The challenge of working within these confines forces creativity and limits scope. Pixel Vision 8 creations are expressions of ingenuity that rise above their limitations. 

## Getting Started

[Pixel Vision 8's SDK](https://gitlab.com/PixelVision8/SDK) is open source, allowing anyone to build authentic 8-bit C# games with it. Developers can use the SDK in any game engine that supports C# such as [Unity](https://unity3d.com) or [MonoGame](https://github.com/MonoGame/MonoGame). Pixel Vision 8's SDK also integrates with lower-level rendering engines such as [OpenTK](https://github.com/opentk/opentk). With that in mind, it will need a runner. The PV8 Runner is any code harness that bridges the core engine to a host platform. The Runner performs the following tasks:

* Facilitates displaying Bitmap Data for rendering the display as well as the assets importers.

* Provides an Application wrapper so that PV8 can run on a computer as an executable.

* Calls the engine's Init() on startup and the Update(), and Draw() methods during each frame.

* Feeds the engine input data such as the mouse, keyboard and controller data.

* Provides a wrapper for playing sounds.

* Allows loading and playing of games.

Runners are relatively easy to build. To help get you started, check out the [Unity Runner Example](https://gitlab.com/PixelVision8/UnityRunner).

## Demos

[Pixel Vision 8 Demos](https://gitlab.com/PixelVision8/Demos) help show off specific features of the SDK as well as how to get up and running on different platforms. Currently, there is a collection of Game examples help you better understand how the engine works:

* Controller

* Draw Sprites

* Draw Fonts

* Mouse

* Sprite Stress Test

In addition to the above demos, you can learn how to build your own Pixel Vision 8 Runner in Unity with this tutorial or download the sample project from its repo here.

## Features

Pixel Vision 8’s SDK contains a collection of components called Chips. Each Chip shares an API and base class to standardize their lifecycle. A central ChipManager handles activating, updating and rendering them. Here is a list of the standard chips included with the SDK:

* ColorChip - Manages system colors

* Display - Renders pixel data.

* Font - Renders sprite based fonts.

* GameChip - A game's base class.

* MusicChip - A sequencer for playing back music.

* ScreenBufferChip - Caches sprites for tilemaps and fonts.

* SoundChip - A sound effect manager.

* SpriteChip - Manages sprites in memory.

* TileMap - Drawing sprites into the ScreenBufferChip.

The engine is completely modular allowing you to extend these Chips to add new functionality or create your own from the base classes and interfaces provided.

## Documentation

Pixel Vision 8 SDK is cleanly architected and well commented. Along with a straightforward set of APIs, there is extensive documentation on how to get started, deep dives into the engine's code and how to build each of the demos from scratch. More documentation is on the way to cover how to extend the core system, create custom Chips and create runners for different platforms.

## Credits

Pixel Vision 8 was created by Jesse Freeman ([@jessefreeman](http://twitter.com/jessefreeman)) in collaboration with Pedro Medeiros ([@saint11](http://twitter.com/saint11)) for art and Christer Kaitila ([@McFunkypants](http://twitter.com/McFunkypants)) for music.