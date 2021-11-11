Converts an index value into a point with an `X` and `Y` value to help when working with 1D arrays that represent 2D data.

## Usage

```csharp
CalculatePosition ( index, width )
```

## Arguments

| Name  | Value | Description                                  |
|-------|-------|----------------------------------------------|
| index | int   | The position of the 1D array\.               |
| width | int   | The width of the data if it was a 2D array\. |

## Returns

| Value | Description                                                                    |
|-------|--------------------------------------------------------------------------------|
| int   | Returns a vector representing the X and Y position of an index in a 1D array\. |


## Example

In this example, we will treat a 1D as a 2D array and convert an index into a `X`, `Y` position. Running this code will output the following:

![image alt text](Images/CalculatePositionOutput.png)

## Lua

```lua
-- A 1D array of example values
local exampleGrid = {
  "A", "B", "C",
  "D", "E", "F",
  "G", "H", "I",
}

function Init()

  -- Example Title
  DrawText("CalculatePosition()", 8, 8, DrawMode.TilemapCache, "large", 15)
  DrawText("Lua Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4)

  local index = 4

  -- Calculate the center index based on a grid with 3 columns
  local position = CalculatePosition(index, 3)

  -- Draw the index and value to the display
  DrawText("Position " .. position.x .. ",".. position.y .. " at Index " .. index .. " is " .. exampleGrid[index], 1, 4, DrawMode.Tile, "large", 15)

end

function Draw()
  -- Redraw the display
  RedrawDisplay()
end
```



## C#

```c#
namespace PixelVision8.Player
{
    class CalculatePositionExample : GameChip
    {
        // A 1D array of example values
        private string[] exampleGrid =
        {
            "A", "B", "C",
            "D", "E", "F",
            "G", "H", "I",
        };

        public override void Init()
        {

            // Example Title
            DrawText("CalculatePosition()", 8, 8, DrawMode.TilemapCache, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);

            var index = 4;

            // Calculate the center index based on a grid with 3 columns
            var position = CalculatePosition(index, 3);

            // Draw the index and value to the display
            DrawText("Position " + position.X + "," + position.Y + " at Index " + index + " is " + exampleGrid[index], 1, 4, DrawMode.Tile, "large", 15);

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();
        }

    }
}
```

