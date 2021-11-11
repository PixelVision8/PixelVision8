The `Tile()` API allows you to get the current sprite, color offset and flag values associated with a given tile ID. You can optionally supply your own values if you want to update the tile. Changing a tile's sprite ID or color offset will force the tilemap to redraw the layer cache on the next frame. If you are drawing raw pixel data into the tilemap cache in the same position, it will be overwritten with the new tile's pixel data.

## Usage

```csharp
Tile ( column, row, spriteID, colorOffset, flag, flipH, flipV )
```

## Arguments

| Name        | Value | Description                                                                                    |
|-------------|-------|------------------------------------------------------------------------------------------------|
| column      | int   | The X position of the tile in the tilemap\. The 0 position is on the far left of the tilemap\. |
| row         | int   | The Y position of the tile in the tilemap\. The 0 position is on the top of the tilemap\.      |
| spriteID    | int   | An optional sprite ID to use for the tile\.                                                    |
| colorOffset | int   | An optional value to shift the color IDs in the tile’s sprite data\.                           |
| flag        | int   | An optional int value between \-1 and 15 used for collision detection\.                        |
| flipH       | bool  | Optional flag for horizontally flipping the tile\. This is not currently implemented\.         |
| flipV       | bool  | Optional flag for vertically flipping the tile\. This is not currently implemented\.           |

## Returns

| Value    | Description                                                                                       |
|----------|---------------------------------------------------------------------------------------------------|
| TileData | Returns a TileData object containing the spriteID, colorOffset, and flag for an individual tile\. |

## Tile Data

The TileData object contains all of the values that make up a single tile. You can use this to learn more about what flags and values are set:

| Property    | Value | Description                                                        |
|-------------|-------|--------------------------------------------------------------------|
| index       | int   | The ID of the tile in the tilemap\.                                |
| spriteID    | int   | The sprite ID to display for the tile\.                            |
| colorOffset | int   | The color offset to be used when drawing the tile\.                |
| flag        | int   | The flag value of the tile\.                                       |
| flipH       | bool  | A flag to flip the tile horizontally when drawing to the display\. |
| flipV       | bool  | A flag to flip the tile vertically when drawing to the display\.   |

## Example

In this example, we are going to read all of the tiles and see which ones have sprites assigned to them. We’ll be using the following Tilemap:

![image alt text](images/Tile_image_0.png)

Next, we’ll set up 4 palettes to apply to each of these tiles:

![image alt text](Images/Tile_image_1.png)

![image alt text](images/Tile_image_2.png)

![image alt text](images/Tile_image_3.png)

![image alt text](images/Tile_image_4.png)

So  the colors.png file should look like this:

![image alt text](images/Tile_image_5.png)

Finally, we’ll change each tile’s `colorOffset` value over several frames to give the impression that the background is fading up and down. Running this code will output the following:

![image alt text](images/TileOutput.png)

## Lua

```lua
-- Set up a time and delay
local time = 0
local delay = 800

-- This will be the direction value for the transition
local dir = 1

-- Total number of palettes for transition
local max = 3

-- Current palette ID
local paletteID = 0

-- Store  the tilemap dimensions
local mapSize = TilemapSize()
local totalTiles = mapSize.x * mapSize.y

function Init()
  
  -- Example Title
  DrawText("Tile()", 32, 16, DrawMode.TilemapCache, "large", 15)
  DrawText("Lua Example", 32, 24, DrawMode.TilemapCache, "medium", 15, -4)

end

function Update(timeDelta)

  -- Increase the time on each frame and test if it is greater than the delay
  time = time + timeDelta
  if(time > delay) then

    -- Update the palette ID based on the direction
    paletteID = paletteID + dir

    -- Test if the palette ID is too small or too large and reverse the direction
    if(paletteID >= max) then
      dir = -1
    elseif(paletteID <= 0) then
      dir = 1
    end

    -- Reset the time value
    time = 0

    -- Loop through all of the tiles in the tilemap
    for i = 1, totalTiles do

      -- Convert the loop index to a column,row position
      local pos = CalculatePosition((i - 1), mapSize.x)

      -- Get the TileData based on the new position
      local tmpTile = Tile(pos.x, pos.y)

      -- Check to see if the tile has a sprite
      if(tmpTile.SpriteId > - 1) then

        -- Update the tile by reassigning the same spriteID but calculating a new color offset
        Tile(pos.x, pos.y, tmpTile.SpriteId, PaletteOffset(paletteID))

      end
    end
  end
end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Draw the text for the palette and color ID
  DrawText("Palette " .. paletteID, 32, 40, DrawMode.Sprite, "large", 15)

end
```



## C#

```c#
namespace PixelVision8.Player
{
    class TileExample : GameChip
    {
        // Set up a time and delay
        private int time;
        private int delay = 800;

        // This will be the direction value for the transition
        private int dir = 1;

        // Total number of palettes for transition
        private int max = 3;

        // Current palette ID
        private int paletteID;

        // Store the tilemap dimensions
        private Point mapSize;
        private int totalTiles;

        public override void Init()
        {
            
            // Example Title
            DrawText("Tile()", 32, 16, DrawMode.TilemapCache, "large", 15);
            DrawText("C Sharp Example", 32, 24, DrawMode.TilemapCache, "medium", 15, -4);

            // Set the tilemap dimensions
            mapSize = TilemapSize();
            totalTiles = mapSize.X * mapSize.Y;

        }

        public override void Update(int timeDelta)
        {
            // Increase the time on each frame and test if it is greater than the delay
            time = time + timeDelta;

            if (time > delay)
            {

                // Update the palette ID based on the direction
                paletteID += dir;

                // Test if the palette ID is too small or too large and reverse the direction
                if (paletteID >= max)
                {
                    dir = -1;
                }
                else if (paletteID <= 0)
                {
                    dir = 1;
                }

                // Reset the time value
                time = 0;

                // Loop through all of the tiles in the tilemap
                for (int i = 0; i < totalTiles; i++)
                {

                    // Convert the loop index to a column,row position
                    var pos = CalculatePosition((i), mapSize.X);

                    // Get the TileData based on the new position
                    var tmpTile = Tile(pos.X, pos.Y);

                    // Check to see if the tile has a sprite
                    if (tmpTile.SpriteId > -1)
                    {

                        // Update the tile by reassigning the same spriteID but calculating a new color offset
                        Tile(pos.X, pos.Y, tmpTile.SpriteId, PaletteOffset(paletteID));

                    }
                }
            }
        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();

            // Draw the text for the palette and color ID
            DrawText("Palette " + paletteID, 32, 40, DrawMode.Sprite, "large", 15);

        }
    }
}
```

One thing to note is that while this demo shows how to modify individual tiles at run-time, it’s important to point out that this approach may have bad performance impacts. If you need to modify large groups of tiles, try indexing them in the tilemap during startup and only iterate over tiles you know you need to update.

