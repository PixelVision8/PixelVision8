The background color is used to fill the screen when clearing the display. You can use this method to read or update the background color at runtime. When calling `BackgroundColor()` without an argument, it returns the current background color offset as an `int`. You can pass in an optional `int `to shift the background color by calling `BackgroundColor(offset),` where `offset` is any valid between 0 and 255.

Passing in a value such as `-1`, or one that is out of range will default to the first system color unless the `ColorChip`’s` debugColor` property is set to true. In debug color mode, any out of bounds color IDs will display Magenta (`#ff00ff`), which is the engine's default transparent color.

## Usage

```csharp
BackgroundColor ( id )
```

## Arguments

| Name | Value | Description                                                  |
| ---- | ----- | ------------------------------------------------------------ |
| id   | int   | This argument is optional\. Supply an int to offset the default background color value by. |


## Returns

| Value | Description                                                  |
| ----- | ------------------------------------------------------------ |
| int   | This method returns the current background color offset\. If no color exists, it returns 0 which is the default background color offset. |


## Example

In this example, we will display the default background color on the display, then change it, and redraw the new value below it. Running this code will output the following:

![image](Images/BackgroundColorOutput.png)

## Lua

```lua
function Init()

  -- Example Title
  DrawText("BackgroundColor()", 1, 1, DrawMode.Tile, "large", 15)
  DrawText("Lua Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4)

  -- Get the current background color
  local defaultColor = BackgroundColor()

  -- Draw the default background color ID to the display
  DrawText("Default Color " .. defaultColor, 1, 4, DrawMode.Tile, "large", 15)

  -- Here we are manually changing the background color
  local newColor = BackgroundColor(2)

  -- Draw the new color ID to the display
  DrawText("New Color " .. newColor, 1, 6, DrawMode.Tile, "large", 15)

end

function Draw()

  -- Redraw the display
  RedrawDisplay()
  
end
```



## C#

```csharp
namespace PixelVision8.Player
{
    public class BackgroundExample : GameChip
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

      public override void Draw()
      {

          //Redraw the display
          RedrawDisplay();

      }

    }
}
```

