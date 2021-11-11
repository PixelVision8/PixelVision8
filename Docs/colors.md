The default game template included a `colors.png` file. This file represents all of the colors available at runtime and contains one color per pixel. The Pixel Vision 8 reads through each color and loads it into the `ColorChip` when a game is loaded into memory. These colors represent the available system colors and their corresponding IDs in memory. If no `color.png` file is provided, Pixel Vision 8’s default color palette will be used.

## System Colors

The color importer scans the PNG’s color data a pixel at a time, starting in the upper-left corner and ending in the bottom right. As it does this, each color is saved into memory with a unique ID until reaching the total color limit of 256 system colors.

To create your `color.png` file by hand, simply make a single line of pixels or lay them out in a grid. Here is an example of how the color importer traverses a grid of pixels inside of a small 4x4 pixel `color.png` file:

![image alt text](images/ImportingSystemColors.png)

Any colors over the `maxColors `value in the `data.json` file will be ignored. The order of the colors in the PNG will match how they are stored in memory. Once you know their ID, you can reference them as needed in your game.

One important thing to note about a game’s colors is that they are used to parse the sprite and tilemap PNG files. If a color does not exist in the system before importing these graphics files, it may be missing when the sprite data is saved to memory. Missing color data also impacts how sprites load, their ID in memory and if they even show up. It is critical that you always provide a complete set of system colors used by your game's artwork for it to load and display as expected.
