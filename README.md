# Pixel Vision 8 Source Code

Pixel Vision 8's core philosophy is to teach retro game development with streamlined workflows. PV8 is also a platform that standardizes 8-bit fantasy console limitations built on top of the open-source C# game engine based on MonoGame. This engine simply provides a standard set of APIs to create authentic-looking 8-bit games with modern programming languages and workflows. It also allows you to define specific limitations that alter the following properties:

* Memory and game size
* Color palettes
* System colors
* Colors per sprite
* Number of sprites on screen
* Scrolling tilemap
* and more

There are different ways to build and run Pixel Vision 8 games depending on your choice of language and skill level. The most common way is to use the commercial build of the fantasy console itself which you can download from [pixelvision8.com](https://pixelvision8.com) and write your games in Lua. For more advanced developers familiar with C#, Pixel Vision 8 can be compiled in Visual Studio by visiting the [GitHub repo](https://github.com/PixelVision8) as well. There you will find the two components which make up Pixel Vision 8: a Runner and Pixel Vision OS.

The platform was designed to be incredibly modular. This allows you to customize it to your own needs. While there are tools and a dedicated OS to help make games with, there is nothing stopping you from using your own external tools and workflows.



## Making C# Games

This documentation covers how to build Pixel Vision 8 from its source code, manually install Pixel Vision OS, workflows for using Pixel Vision 8 and Visual Studio, as well as how to export stand-alone Lua PV8 games in dedicated Runners. You’ll also find all the information you need to better understand how Pixel Vision 8 works under the hood and the APIs used to make your own games in C#.

### Getting Started
Let’s take a look at how you can use Pixel Vision OS and Visual Studio to make a PV8 game with C#. When it comes to making Pixel Vision 8 games in C#,  you will need to adopt a hybrid workflow. This tutorial assumes you are using Visual Studio on a PC. You can also follow the same steps on Visual Studio for the Mac or Jetbrain’s Rider. To get started, you’ll want to download a zip copy of the Github repo instead of cloning.

<p style="text-align:center"><img src="https://raw.githubusercontent.com/wiki/PixelVision8/PixelVisionRunner/images/MakingCSharpGames_image_0.png" /></p>

### Opening the Project

Once it’s downloaded, unzip the file, move it to your Documents folder, and rename it to `PixelVision8Games`. You can work on multiple Pixel Vision 8 C# games in the same Visual Studio solution.

<p style="text-align:center"><img src="https://raw.githubusercontent.com/wiki/PixelVision8/PixelVisionRunner/images/MakingCSharpGames_image_1.png" /></p>

Next, you’ll want to open up the `MonoGameRunner.sln` file in Visual Studio. This solution contains two Runners, one for C# and the other for Lua. Remove the Lua project since we won’t be needing it.

<p style="text-align:center"><img src="https://raw.githubusercontent.com/wiki/PixelVision8/PixelVisionRunner/images/MakingCSharpGames_image_2.png" /></p>

### Creating a New Game Project

Now we’ll need to make a copy of the default C# project and import it back into our solution. On the file system, open up the `PixelVision8Games/Runners/` directory and make a copy of the `CSharpRunner` folder. Rename the folder to MyGame and the project file inside to `MyGame.Desktop.csproj`.

<p style="text-align:center"><img src="https://raw.githubusercontent.com/wiki/PixelVision8/PixelVisionRunner/images/MakingCSharpGames_image_3.png" /></p>

Before we import this in Visual Studio, we’ll need to make a few changes to the `MyGame.Desktop.csproj` file you just renamed.  Look for the `ProjectGuid` node. We need to give this a unique ID so we don’t get a conflict with other projects in the solution.

<p style="text-align:center"><img src="https://raw.githubusercontent.com/wiki/PixelVision8/PixelVisionRunner/images/MakingCSharpGames_image_4.png" /></p>

You can use an [online generator](https://guidgenerator.com/) to or make it up yourself. You can also use this opportunity to make changes to any of the project’s other values such as its name and version number. After you’ve made your changes, add the project to the solution by right-clicking on the `Runner` folder and selecting `Add > Existing Project` from the menu.

<p style="text-align:center"><img src="https://raw.githubusercontent.com/wiki/PixelVision8/PixelVisionRunner/images/MakingCSharpGames_image_5.png" /></p>

At this point, you should have a solution with the default C# project and the new one you created.

<p style="text-align:center"><img src="https://raw.githubusercontent.com/wiki/PixelVision8/PixelVisionRunner/images/MakingCSharpGames_image_6.png" /></p>

Currently, the MyGame project is referencing a default project in the `Disks` folder which is not part of the solution. If you go back into the `MyGame.Desktop` project file, you’ll see where it is being referenced by looking for a reference to `\Disks\PixelVision8System\`.

<p style="text-align:center"><img src="https://raw.githubusercontent.com/wiki/PixelVision8/PixelVisionRunner/images/MakingCSharpGames_image_7.png" /></p>

If you are using Visual Studio, you will need to unload the project first, or you can just find the file in the `Runners/CSharpRunner/CSharpRunner.Desktop.csproj` and edit outside of Visual Studio.

In order to make a new game, we need to copy the `PixelVision8System` folder then renaming it to `MyGame`. To make things easier, you should put all of your games in the `Disks` folder to keep it in the same project folder.

<p style="text-align:center"><img src="https://raw.githubusercontent.com/wiki/PixelVision8/PixelVisionRunner/images/MakingCSharpGames_image_8.png" /></p>

Once you’ve created the new project folder, change the path in the `MyGame.Desktop` file. You can also delete the `code.lua` file in the game folder since we won’t be using it in C#.

<p style="text-align:center"><img src="https://raw.githubusercontent.com/wiki/PixelVision8/PixelVisionRunner/images/MakingCSharpGames_image_9.png" /></p>

### Customize the C# Template
Now, when you return to Visual Studio and reload the solution, you’ll see all of the files from the `MyGame` folder, referenced in the project’s `Content` folder.

<p style="text-align:center"><img src="https://raw.githubusercontent.com/wiki/PixelVision8/PixelVisionRunner/images/MakingCSharpGames_image_10.png" /></p>

Let’s go ahead and rename the `ExampleGameChip.cs` file as well. You’ll want to go in and change the class name to `MyGameChip` by clicking on the file and selecting rename in the drop-down menu. This will automatically update all references to that class.

<p style="text-align:center"><img src="https://raw.githubusercontent.com/wiki/PixelVision8/PixelVisionRunner/images/MakingCSharpGames_image_11.png" /></p>

At this point, we should be able to run the game. But, before we do that, open up the `MyGameChip.cs` file and change the message to read "MY NEW GAME" instead of “EMPTY GAME”. When you compile your game, you’ll see the following.

<p style="text-align:center"><img src="https://raw.githubusercontent.com/wiki/PixelVision8/PixelVisionRunner/images/MakingCSharpGames_image_12.png" /></p>

### Using Pixel Vision OS

At this point, you should have everything you need to start building your new C# game in Visual Studio. If you don’t have a copy of Pixel Vision 8 installed yet, [get a copy from the site](https://www.pixelvision8.com/play) and [install Pixel Vision OS](https://www.pixelvision8.com/getting-started). We’ll be using the Pixel Vision OS tools to help us build our C# game.

<p style="text-align:center"><img src="https://raw.githubusercontent.com/wiki/PixelVision8/PixelVisionRunner/images/MakingCSharpGames_image_13.png" /></p>

The last thing we’ll cover is how to use Pixel Vision OS’s tool to make changes to your game files. Once you have Pixel Vision 8 booted up, you’ll want to drag the `MyGame` folder you created in the `Disks` directory onto the Pixel Vision 8 window. That should mount your game as a disk on the desktop. Open the disk and you’ll see all of the files inside of the folder from your computer.

<p style="text-align:center"><img src="https://raw.githubusercontent.com/wiki/PixelVision8/PixelVisionRunner/images/MakingCSharpGames_image_14.png" /></p>

Now you can use Pixel Vision OS’s built-in tools, including the Pro Tools if you have them installed, to edit your games files. With this workflow, you’ll do all of your coding in Visual Studio, and all of your configuration and game file editing in Pixel Vision 8. When you want to see any changes, you’ll need to rebuild the game in Visual Studio.

It’s important to note that if you are adding or deleting files outside of Visual Studio, you may need to unload and reload the project to see the changes in the `Contents` folder. Since we are using a pattern to match all files inside of the `Disks/MyGame/` directory and copy them over to the Contents directory during a build, Visual Studio sometimes loses track of the external changes.

When you are ready to make a new C# game, you can simply copy the default project and rename it just like we did before. This way, you can keep all of your C# PV8 games in one Visual Studio Solution.

## Credits

Pixel Vision 8 was created by Jesse Freeman ([@jessefreeman](http://twitter.com/jessefreeman)) in collaboration with Christina-Antoinette Neofotistou ([@CastPixel](http://twitter.com/CastPixel), Pedro Medeiros ([@saint11](http://twitter.com/saint11)) for art and Christer Kaitila ([@McFunkypants](http://twitter.com/McFunkypants)) for music. With additional coding contributions by Shawn Rakowski ([@shwany](http://twitter.com/shwany)).

## License

Licensed under the [Microsoft Public License (MS-PL) License](https://opensource.org/licenses/MS-PL).  See LICENSE file in the project root for full license information.

Pixel Vision 8 is Copyright (c) 2017 Jesse Freeman. All rights reserved.
