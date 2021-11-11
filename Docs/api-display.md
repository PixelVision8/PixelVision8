The display's size defines the visible area where pixel data exists on the screen. Calculating this is important for knowing how to position sprites on the screen. The `Display()` method allows you to get the resolution of the display at run time. By default, this will return the visible screen area based on the overscan value set on the `DisplayChip`. To calculate the exact overscan in pixels, you must subtract the full size from the visible size. Simply supply false as an argument to get the full display dimensions.

## Usage

```csharp
Display ()
```


## Returns

| Value | Description                                                           |
|-------|-----------------------------------------------------------------------|
| Point | Returns a point with X representing Width and Y representing Height\. |


## Example

In this example, we get the full size of the display and the visible size. The visible size represents the full size minus the right and bottom overscan values. Running this code will output the following:

![image alt text](Images/DisplayOutput.png)

## Lua

```lua
-- Create a canvas to visualize the screen sizes
local canvas = NewCanvas(256, 240)

function Init()

  -- Example Title
  DrawText("Display()", 8, 8, DrawMode.TilemapCache, "large", 15)
  DrawText("Lua Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4)
  
  -- Get the full size of the display
  local sizeA = Display()

  -- Draw the two sizes to the display
  DrawText("Full Display Size " .. sizeA.x .. "x" ..sizeA.y, 1, 4, DrawMode.Tile, "large", 15)

  -- Set the canvas stroke to white
  canvas:SetStroke(14, 2)

  canvas:DrawRectangle(0, 0, sizeA.x, sizeA.y)

end

function Draw()
  -- Redraw the display
  RedrawDisplay()

  -- Draw the canvas to the display
  canvas:DrawPixels()
end
```



## C#

```csharp
namespace PixelVision8.Player
{
    class DisplayExample : GameChip
    {

        // Save a reference to the canvas
        private Canvas canvas;

        public override void Init()
        {
            // Example Title
            DrawText("Display()", 8, 8, DrawMode.TilemapCache, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);

            // Create a canvas to visualize the screen sizes
            canvas = new Canvas(256, 240, this);

            // Get the full size of the display
            var sizeA = Display();

            // Draw the two sizes to the display
            DrawText("Full Display Size " + sizeA.X + "x" + sizeA.Y, 1, 4, DrawMode.Tile, "large", 15);
            
            // Set the canvas stroke to white
            canvas.SetStroke(14, 2);

            // Set the fill color to 5 and draw the full size square
            canvas.DrawRectangle(0, 0, sizeA.X, sizeA.Y);

            // Draw the canvas to the display
            canvas.DrawPixels();
            
        }
        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();
            
        }
        
    }
}
```

