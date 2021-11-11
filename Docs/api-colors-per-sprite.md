Pixel Vision 8 sprites have limits around how many colors they can display at once. This is called **CPS** which stands for Colors Per Sprite. The `ColorsPerSprite()` API returns this value from the `SpriteChip`. Since this value is read-only, it is best to get a reference to it when the game starts up and store it in a variable your game can access.

## Usage

```csharp
ColorsPerSprite ( )
```

## Returns

| Value | Description                             |
|-------|-----------------------------------------|
| int   | The total colors each sprite can have\. |


## Example

In this example, we display the color per sprite value. Running this code will output the following:

![image alt text](Images/ColorsPerSpriteOutput.png)

## Lua

```lua
function Init()

  -- Change the background color
  BackgroundColor( 6 )

  -- Example Title
  DrawText("ColorsPerSprite()", 8, 8, DrawMode.TilemapCache, "large", 15)
  DrawText("Lua Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4)

  -- Draw the cps value to the display
  DrawText("Colors Per Sprite = " .. ColorsPerSprite(), 8, 32, DrawMode.TilemapCache, "large", 15)

end

function Draw()

  -- Clear the display
  RedrawDisplay()

  -- Draw meta sprite to the display
  DrawMetaSprite( "reaper-boy", 8, 48)

end
```



## C#

```csharp
namespace PixelVision8.Player
{
    class ColorsPerSpriteExample : GameChip
    {
        public override void Init()
        {

            // Change the background color
            BackgroundColor( 6 );

            // Example Title
            DrawText("ColorsPerSprite()", 8, 8, DrawMode.TilemapCache, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);

            // Draw the cps value to the display
            DrawText("Colors Per Sprite = " + ColorsPerSprite(), 8, 32, DrawMode.TilemapCache, "large", 15);

        }

        public override void Draw()
        {
            // Clear the display
            RedrawDisplay();

            // Draw meta sprite to the display
            DrawMetaSprite( "reaper-boy", 8, 48);
        }
    }
}
```

