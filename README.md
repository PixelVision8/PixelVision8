# Pixel Vision 8 v2.0
![Pixel Vision 8](https://github.com/PixelVision8/PixelVision8/workflows/Pixel%20Vision%208/badge.svg)

Pixel Vision 8's core philosophy is to teach retro game development with streamlined workflows. PV8 is also a platform that standardizes 8-bit fantasy console limitations built on top of a fork of MonoGame, called [MonoVision](https://github.com/PixelVision8/MonoVision). This engine provides a standard set of APIs to create authentic-looking 8-bit games with modern programming languages and workflows. It also allows you to define specific limitations for the resolution, colors, number of sprites on screen, and more. 

There are several different ways to build and run Pixel Vision 8, depending on your choice of language, skill level, and goals. You can always get the latest compiled version on [itch.io](https://pixelvision8.itch.io/pv8) or check out the incremental builds on [github]([https://githu](https://github.com/PixelVision8/PixelVision8/releases/latest)).

The platform is incredibly modular and built on a "chip" system that supports swapping out core functionality with new chips. This allows you to customize it to your own needs. While there are tools and a dedicated OS to help make games, nothing stops you from using your external tools and workflows. In fact, the best way to make Pixel Vision 8 games is with the tools and workflow you are most comfortable with already!

## Disclaimer

[I have been working on Pixel Vision 8 for the past 6+ years](https://jessefreeman.hashnode.dev/the-dark-side-of-supporting-an-open-source-project) and it's still a work in process. I'm doing my best to keep [the docs](https://github.com/PixelVision8/PixelVision8/wiki), [code base](https://github.com/PixelVision8/PixelVision8/tree/master), and [examples](https://github.com/PixelVision8/Examples) as up to date as possible, but to help speed up development, consider all the code in this repo experimental. Please join the [Discord](https://discord.gg/pixelvision8) or subscribe to the new [Tutorial Site](https://hashnode.com/@pixelvision8) to learn more about getting started.

## Quick Start Guide

I've tried my best to make compiling Pixel Vision 8 from the source as easy as possible. While you can learn more about this in the [docs](https://github.com/PixelVision8/PixelVision8/wiki), here is the quickest way to build PV8 from scratch:

> Before you get started, you are going to want to install [.Net 6](https://dotnet.microsoft.com/download/dotnet/6.0), [NodeJS](https://nodejs.org/en/download/), and an IDE like [Visual Studio Code](https://code.visualstudio.com/Download).

1. Clone the main repo `> git clone https://github.com/PixelVision8/PixelVision8.git`
2. Install the NodeJS dependencies `> npm install`
3. Run the default Gulp action `> gulp`
4. Launch the `.dll` manually `dotnet Projects/PixelVision8/bin/Debug/net5.0/Pixel\ Vision\ 8.dll`

If you want to build Pixel Vision 8 executables, you can use the Gulp action `> gulp package`. This will create a new `Releases/Final/` folder, and inside, you'll zip files for Windows, Mac, and Linux. I call the task via a custom GitHub Action to build and upload the PV8 releases to this repo.

Finally, you can use Visual Studio Code to debug a build by running one of the custom tasks included in the `.vscode` folder.

## Runners

A runner is a wrapper for Pixel Vision 8's core code which allows it to run on different platforms. When you compile PV8 from the default Gulp action, it creates a `DesktopRunner` that includes all of the "bells and whistles." You may not need that or want to code a PV8 game closer to the metal. You can find the following runners in the `Project` folder.

1. Pixel Vision 8 Runner - Includes the Lua and Roslyn engines plus a virtual file system, bios, and other hooks to edit games.
2. CSharp Runner - For building pure C# games without the virtual file system, reloading, or boot tools.
3. Roslyn Runner - This is a hybrid CSharp Runner that allows you to reload C# games without recompiling.
4. Lua Runner - For building pure Lua games which support hot reloading but no virtual file system or boot tools.
5. Unity - This is an experimental runner designed to work inside of Unity.

If you are working on a Lua or C# game and would like to distribute it without all of the extra Pixel Vision 8 features, you can look at each of the Runner's `.csproj` file and replace the path to the `Empty Disk` with your own game:

```<Game>../../Build/Templates/EmptyDisk</Game>```

Then when you compile the Runner, the game files will be copied into the game's `Content` folder and are automatically loaded when the Runner starts. Pay special attention to how the CSharp Runner is structured because you will need to include all of your game's C# code in addition to copying over the game file for it to work.

## Games

A lot of people ask where they can find example games. You can check out some of my games like [Space Station 8](https://github.com/PixelVision8/SpaceStation8) and [Terminal](https://github.com/PixelVision8/Terminal) in their repos. I've also created a new [Game Repo](https://github.com/PixelVision8/Games) that people can contribute to by submitting a pull request.

## OS

Past versions of Pixel Vision 8 came bundled with an OS and tools installed by default. Suppose you are building PV8 from scratch or using one of the increment releases posted to GitHub. In that case, you can manually boot into the OS by downloading it from the [Pixel Vision OS repo](https://github.com/PixelVision8/OS) and dragging it onto the window. PV8 will remember the last disk it loaded, so you shouldn't have to keep manually booting into the OS unless the system loses a reference to the disk's path or another disk forces the OS disk to eject.

## Credits

Pixel Vision 8 was created by Jesse Freeman ([@jessefreeman](http://twitter.com/jessefreeman)) in collaboration with Christina-Antoinette Neofotistou ([@CastPixel](http://twitter.com/CastPixel)) for art, and Christer Kaitila ([@McFunkypants](http://twitter.com/McFunkypants)) for music. 

With additional contributions by the following people:

* Pedro Medeiros
* Shawn Rakowski
* Drake Williams
* Matt Hughson
* Dean Ellis

And special thanks to Jan Rochat, Ethan Shaughnessy, and our other sponsors.

## License

Pixel Vision 8 is Licensed under the [Microsoft Public License (MS-PL) License](https://opensource.org/licenses/MS-PL). See the [LICENSE file](https://github.com/PixelVision8/PixelVision8/blob/master/LICENSE.txt) in the project root for complete license information.

Pixel Vision 8 is Copyright (c) 2017-2021 Jesse Freeman. All rights reserved.
