/**
Pixel Vision 8 - BackgroundColor() Example
Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
Created by Jesse Freeman (@jessefreeman)

Learn more about making Pixel Vision 8 games at
https://www.pixelvision8.com/getting-started
**/

using PixelVision8.Player;

namespace PixelVision8.Examples
{

    public class ExampleGameChip : GameChip
    {
        public override void Init()
      {

          // Example Title
          DrawText("BackgroundColor()", 1, 1, DrawMode.Tile, "large", 15);
          DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);

          //  Get the current BackgroundColor
          var defaultColor = BackgroundColor();

          // Draw the default background color ID to the display
          DrawText("Default Color " + defaultColor, 1, 4, DrawMode.Tile, "large", 15);

          //  Here we are manually changing the background color
          var newColor = BackgroundColor(2);

          //  Draw the new color ID to the display
          DrawText("New Color " + newColor, 1, 6, DrawMode.Tile, "large", 15);

      }

      /// <summary>
      ///     Draw() is called once per frame after the Update() has completed. This is where all visual updates to
      ///     your game should take place such as clearing the display, drawing sprites, and pushing raw pixel data
      ///     into the display.
      /// </summary>
      public override void Draw()
      {

          //Redraw the display
          RedrawDisplay();

      }

    }
}
