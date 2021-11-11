The `DrawRect()` API allows you to display a rectangle with a fill color on the screen. Since this API uses `DrawPixels()`, rectangles can be drawn to the tilemap cache or sprite layers. You can also use `DrawRect()` to quickly clear the screen or debug objects on the screen such as bounding boxes for collision detection. It’s important to note that the rectangle’s pixel data is created every time the draw request is called, so there could be a performance hit if `DrawRect()` is used too much on each frame.

## Usage

```csharp
DrawRect ( x, y, width, height, color, drawMode )
```

## Arguments

| Name     | Value    | Description                                                                                                                                                                                                                                                                           |
|----------|----------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| x        | int      | The x position where to display the rectangle’s pixel data\. The display's horizontal 0 position is on the far left\-hand side\. When using DrawMode\.TilemapCache, the pixel data is drawn into the tilemap's cache instead of directly on the display when using DrawMode\.Sprite\. |
| y        | int      | The Y position where to display the rectangle’s pixel data\. The display's vertical 0 position is on the top\. When using DrawMode\.TilemapCache, the pixel data is drawn into the tilemap's cache instead of directly on the display when using DrawMode\.Sprite\.                   |
| width    | int      | The width of the pixel data to use when rendering the rectangle to the display\.                                                                                                                                                                                                      |
| height   | int      | The height of the pixel data to use when rendering the rectangle to the display\.                                                                                                                                                                                                     |
| color    | int      | The color to use when filling the rectangle’s pixel data\.                                                                                                                                                                                                                            |
| drawMode | DrawMode | This argument accepts the DrawMode enum\. You can use Sprite, SpriteBelow, and TilemapCache to change where the rectangle’s pixel data is drawn to\. By default, this value is DrawMode\.Sprite\.                                                                                     |

## Draw Modes

The `DrawPixels()` API supports the following draw modes:

| DrawMode     | Layer ID | Supported |
|--------------|----------|-----------|
| TilemapCache | \-1      | Yes       |
| Background   | 0        | No        |
| SpriteBelow  | 1        | Yes       |
| Tile         | 2        | No        |
| Sprite       | 3        | Yes       |
| UI           | 4        | Yes       |
| SpriteAbove  | 5        | Yes       |


Attempting to use an unsupported draw mode will cancel the draw request.

## Example

In this example, we draw three rectangles to the display using different draw modes. Running this code will output the following:

![image alt text](images/DrawRectOutput.png)

## Lua

```lua
function Init()

  -- Example Title
  DrawText("DrawRect()", 1, 1, DrawMode.Tile, "large", 15)
  DrawText("Lua Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4)
  
  -- Draw a 100 x 100 pixel rect to the display
  DrawRect(16, 40, 100, 100, 5, DrawMode.TilemapCache)

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Draw a rect to the sprite layer
  DrawRect(12, 36, 25, 25, 14, DrawMode.Sprite)

  -- Draw a rect to the sprite below layer
  DrawRect(100, 124, 25, 25, 15, DrawMode.SpriteBelow)

end
```



## C#

```csharp
namespace PixelVision8.Player
{
    class DrawRectExample : GameChip
    {
        public override void Init()
        {
        
            // Example Title
            DrawText("DrawRect()", 1, 1, DrawMode.Tile, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);
            
            // Draw a 100 x 100 pixel rect to the display
            DrawRect(16, 40, 100, 100, 5, DrawMode.TilemapCache);

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();

            // Draw a rect to the sprite layer
            DrawRect(12, 36, 25, 25, 14, DrawMode.Sprite);

            // Draw a rect to the sprite below layer
            DrawRect(100, 124, 25, 25, 15, DrawMode.SpriteBelow);

        }

    }
}
```

