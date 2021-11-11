The `DrawPixels()` API allows you to push raw pixel data directly to the display. While `DrawPixels()` is used under the hood by all of the drawing APIs, you lose some performance by calling it directly, especially in Lua. Since this bypasses the sprite counter, there is no limit to the number of draw calls you can make to any of the sprite layers. While this may be advantageous in some situations, you also lose the built-in sprite pixel data caching and draw call limitation imposed by the `SpriteChip`.

You can also use `DrawPixels()` to make changes to the tilemap cache layer. Since `DrawPixels()` doesn’t have a width or height restriction, you can use this API to draw irregularly shaped data directly to the tilemap cache without using any sprites. On the flip side, there is no way to use `DrawPixels()` on the tile layer, so using it will cancel the draw call. It is important to note that when you change a tile's sprite ID or color offset, the tilemap redraws it back to the cache, overwriting any pixel data that was previously there.

## Usage

```csharp
DrawPixels ( pixelData, x, y, width, height, flipH, flipV, drawMode, colorOffset )
```

## Arguments

| Name        | Value    | Description                                                                                                                                                                                                                                                                   |
|-------------|----------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| pixelData   | int\[\]  | The pixelData argument accepts an int array representing references to color IDs\. The pixelData array length needs to be the same size as the supplied width and height, or it will throw an error\.                                                                         |
| x           | int      | The x position where to display the new pixel data\. The display's horizontal 0 position is on the far left\-hand side\. When using DrawMode\.TilemapCache, the pixel data is drawn into the tilemap's cache instead of directly on the display when using DrawMode\.Sprite\. |
| y           | int      | The Y position where to display the new pixel data\. The display's vertical 0 position is on the top\. When using DrawMode\.TilemapCache, the pixel data is drawn into the tilemap's cache instead of directly on the display when using DrawMode\.Sprite\.                   |
| width       | int      | The width of the pixel data to use when rendering to the display\.                                                                                                                                                                                                            |
| height      | int      | The height of the pixel data to use when rendering to the display\.                                                                                                                                                                                                           |
| flipH       | bool     | This is an optional argument which accepts a bool\. The default value is set to false but passing in true flips the pixel data horizontally\.                                                                                                                                 |
| flipV       | bool     | This is an optional argument which accepts a bool\. The default value is set to false but passing in true flips the pixel data vertically\.                                                                                                                                   |
| drawMode    | DrawMode | This argument accepts the DrawMode enum\. You can use Sprite, SpriteBelow, and TilemapCache to change where the pixel data is drawn to\. By default, this value is DrawMode\.Sprite\.                                                                                         |
| colorOffset | int      | This optional argument accepts an int that offsets all the color IDs in the pixel data array\. This value is added to each int, in the pixel data array, allowing you to simulate palette shifting\.                                                                          |

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

In this example, we will take an array of color IDs and push them to the display using the DrawPixels() API. There are 3 draw mode examples showing off how to use this API in different situations. Running this code will output the following:

![image alt text](images/DrawPixelsOutput.png)

## Lua

```lua
local pixelData = {
  -1, - 1, - 1, 0, 0, 0, 0, - 1,
  -1, - 1, 0, 0, 14, 14, 0, - 1,
  -1, 0, 0, 14, 14, 14, 0, - 1,
  -1, 0, 14, 14, 14, 13, 0, - 1,
  -1, 0, 0, 0, 14, 13, 0, - 1,
  -1, - 1, - 1, 0, 14, 13, 0, - 1,
  -1, - 1, - 1, 0, 13, 13, 0, - 1,
  -1, - 1, - 1, 0, 0, 0, 0, - 1,
}

function Init()

  -- Example Title
  DrawText("DrawPixels()", 1, 1, DrawMode.Tile, "large", 15)
  DrawText("Lua Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4)

  -- Draw the sprite data to the tilemap cache
  DrawText("Tilemap Cache", 1, 4, DrawMode.Tile, "large", 15)
  DrawPixels(pixelData, 8, 40, 8, 8, false, false, DrawMode.TilemapCache)
  DrawPixels(pixelData, 16, 40, 8, 8, true, false, DrawMode.TilemapCache)
  DrawPixels(pixelData, 24, 40, 8, 8, false, true, DrawMode.TilemapCache)
  DrawPixels(pixelData, 32, 40, 8, 8, true, true, DrawMode.TilemapCache)

  -- Shift the pixel data color IDs by 1
  DrawPixels(pixelData, 40, 40, 8, 8, false, false, DrawMode.TilemapCache, 1)

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Label for the sprite layer examples
  DrawText("Sprite", 1, 7, DrawMode.Tile, "large", 15)

  -- You can simplify the call if you are not flipping the pixel data
  DrawPixels(pixelData, 8, 64, 8, 8)

  -- Fliping the pixel data on the sprite layer, which is used by default when not provided
  DrawPixels(pixelData, 16, 64, 8, 8, true, false)
  DrawPixels(pixelData, 24, 64, 8, 8, false, true)
  DrawPixels(pixelData, 32, 64, 8, 8, true, true)

  -- Shift the pixel data color IDs over by 1 requires passing in the draw mode
  DrawPixels(pixelData, 40, 64, 8, 8, false, false, DrawMode.Sprite, 1)

  -- Draw pixel data to the sprite below layer
  DrawText("Sprite Below", 1, 10, DrawMode.Tile, "large", 15)
  DrawPixels(pixelData, 8, 88, 8, 8, false, false, DrawMode.SpriteBelow)
  DrawPixels(pixelData, 16, 88, 8, 8, true, false, DrawMode.SpriteBelow)
  DrawPixels(pixelData, 24, 88, 8, 8, false, true, DrawMode.SpriteBelow)
  DrawPixels(pixelData, 32, 88, 8, 8, true, true, DrawMode.SpriteBelow)
  DrawPixels(pixelData, 40, 88, 8, 8, false, false, DrawMode.SpriteBelow, 1)

  -- Display the total sprites used during this frame
  DrawText("Total Sprites " .. ReadTotalSprites(), 8, 104, DrawMode.Sprite, "large", 14)

end
```



## C#

```csharp
namespace PixelVision8.Player
{
    class DrawPixelsExample : GameChip
    {
        int[] pixelData = {
            -1, -1, -1, 0, 0, 0, 0, -1,
            -1, -1, 0, 0, 14, 14, 0, -1,
            -1, 0, 0, 14, 14, 14, 0, -1,
            -1, 0, 14, 14, 14, 13, 0, -1,
            -1, 0, 0, 0, 14, 13, 0, -1,
            -1, -1, -1, 0, 14, 13, 0, -1,
            -1, -1, -1, 0, 13, 13, 0, -1,
            -1, -1, -1, 0, 0, 0, 0, -1,
        };

        public override void Init()
        {
            
            // Example Title
            DrawText("DrawPixels()", 1, 1, DrawMode.Tile, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);

            // Draw the sprite data to the tilemap cache
            DrawText("Tilemap Cache", 1, 4, DrawMode.Tile, "large", 15);
            DrawPixels(pixelData, 8, 40, 8, 8, false, false, DrawMode.TilemapCache);
            DrawPixels(pixelData, 16, 40, 8, 8, true, false, DrawMode.TilemapCache);
            DrawPixels(pixelData, 24, 40, 8, 8, false, true, DrawMode.TilemapCache);
            DrawPixels(pixelData, 32, 40, 8, 8, true, true, DrawMode.TilemapCache);

            // Shift the pixel data color IDs by 1
            DrawPixels(pixelData, 40, 40, 8, 8, false, false, DrawMode.TilemapCache, 1);

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();

            // Label for the sprite layer examples
            DrawText("Sprite", 1, 7, DrawMode.Tile, "large", 15);

            // You can simplify the call if you are not flipping the pixel data
            DrawPixels(pixelData, 8, 64, 8, 8);

            // Fliping the pixel data on the sprite layer, which is used by default when not provided
            DrawPixels(pixelData, 16, 64, 8, 8, true, false);
            DrawPixels(pixelData, 24, 64, 8, 8, false, true);
            DrawPixels(pixelData, 32, 64, 8, 8, true, true);

            // Shift the pixel data color IDs over by 1 requires passing in the draw mode
            DrawPixels(pixelData, 40, 64, 8, 8, false, false, DrawMode.Sprite, 1);

            // Draw pixel data to the sprite below layer
            DrawText("Sprite Below", 1, 10, DrawMode.Tile, "large", 15);
            DrawPixels(pixelData, 8, 88, 8, 8, false, false, DrawMode.SpriteBelow);
            DrawPixels(pixelData, 16, 88, 8, 8, true, false, DrawMode.SpriteBelow);
            DrawPixels(pixelData, 24, 88, 8, 8, false, true, DrawMode.SpriteBelow);
            DrawPixels(pixelData, 32, 88, 8, 8, true, true, DrawMode.SpriteBelow);
            DrawPixels(pixelData, 40, 88, 8, 8, false, false, DrawMode.SpriteBelow, 1);

            // Display the total sprites used during this frame
            DrawText("Total Sprites " + ReadTotalSprites(), 8, 104, DrawMode.Sprite, "large", 14);

        }

    }
}
```

