The `Clamp()` API limits a value between a minimum and maximum integer.

## Usage

```csharp
Clamp ( val, min, max )
```

## Arguments

| Name | Value | Description                   |
|------|-------|-------------------------------|
| val  | int   | The value to clamp\.          |
| min  | int   | The minimum value should be\. |
| max  | int   | The maximum value should be\. |

## Returns

| Value | Description                                   |
|-------|-----------------------------------------------|
| int   | Returns an int within the min and max range\. |

## Example

In this example, we will increase the number by 1 every frame and clamp it at 100. 

Running this code will output the following:

![image alt text](Images/ClampOutput.png)

## Lua

```lua
local counter = 0
local time = 0
local delay = 300

function Init()   

  -- Example Title
  DrawText("Clamp()", 8, 8, DrawMode.TilemapCache, "large", 15)
  DrawText("Lua Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4)

end


function Update(timeDelta)

  -- Add the time delay to the time
  time = time + timeDelta

  -- Check if time is greater than the delay
  if(time > delay) then

    -- Increase the counter by 1
    counter = Clamp(counter + 1, 0, 10)

    -- Reset the time
    time = 0

  end

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Draw the counter to the display
  DrawText("Counter " .. counter, 8, 32, DrawMode.Sprite, "large", 15)

end
```



## C#

```csharp
namespace PixelVision8.Player
{
    class ClampExample : GameChip
    {

        private int counter;
        private int time;
        private int delay = 300;

        public override void Init()
        {

            // Example Title
            DrawText("Clamp()", 8, 8, DrawMode.TilemapCache, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);

        }

        public override void Update(int timeDelta)
        {
            // Add the time delay to the time
            time = time + timeDelta;

            // Check if time is greater than the delay
            if (time > delay)
            {

                // Increase the counter by 1
                counter = Clamp(counter + 1, 0, 10);

                // Reset the time
                time = 0;

            }
        }

        public override void Draw()
        {

            // Redraw the display
            RedrawDisplay();

            // Draw the counter to the display
            DrawText("Counter " + counter, 8, 32, DrawMode.Sprite, "large", 15);

        }

    }
}
```

