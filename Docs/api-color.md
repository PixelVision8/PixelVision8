The `Color()` API allows you to read and update color values in the `ColorChip`. This API has two modes that require a color ID to work. By calling the method with just an ID, like `Color(0)`, it returns a HEX string for that color. If you supply an additional HEX string value, like `Color(0, "#FFFF00")`, you can change the color with the given ID. 

While you can use this method to modify color values directly, you should avoid doing this at run time since the `DisplayChip` must re-cache the new HEX value. It’s best to set up all the colors you need ahead of time in the `data.json` file.

## Usage

```csharp
Color ( int id, string value )
```

## Arguments

| Name  | Value  | Description                                                                                          |
|-------|--------|------------------------------------------------------------------------------------------------------|
| id    | int    | The ID of the color you want to access\.                                                             |
| value | string | This argument is optional\. It accepts a hex as a string and updates the supplied color ID's value\. |


## Returns

| Value  | Description                                                                                                                                                                                    |
|--------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| string | This method returns a hex string for the supplied color ID\. If the color has not been set or is out of range, it returns magenta \(\#FF00FF\) which is the default transparent system color\. |


## Example

In this example, we are going to read the second color (ID `1`), draw a rectangle with it, then change the color every few milliseconds. Running this code will output the following:

![image alt text](Images/ColorOutput.png)

## Lua

```lua
-- Create a delay and set the time to that value so it triggers right away
local delay = 500
local time = delay

-- Create an array of colors and an index value to point to the currently selected color
local colorIndex = 1
local colors = {"#000000", "#ffffff"}

function Init()

  -- Example Title
  DrawText("Color()", 8, 8, DrawMode.TilemapCache, "large", 15)
  DrawText("Lua Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4)

  -- Draw a rect with the second color
  DrawRect(8, 48, 32, 32, 1, DrawMode.TilemapCache)

end

function Update(timeDelta)

  -- Increase the time value base on the timeDelta between the last frame
  time = time + timeDelta

  -- Text to see if time is greater than the delay
  if(time > delay) then

    -- Increase the color index by 1 and reset if it's greater than the color array
    colorIndex = colorIndex + 1
    if(colorIndex > #colors) then
      colorIndex = 1
    end

    -- Update the second color value from the array
    Color(1, colors[colorIndex])

    -- Reset the timer
    time = 0

  end

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Draw a label showing the 2nd colors current HEX value
  DrawText("Color 1 is " .. Color(1), 8, 32, DrawMode.Sprite, "large", 15)

end
```




## C#

```csharp
namespace PixelVision8.Player
{
    class ColorExample : GameChip
    {

        // Create a delay and set the time to that value so it triggers right away
        private int delay = 500;
        private int time;

        // Create an array of colors and an index value to point to the currently selected color
        private int colorIndex = 1;
        private string[] colors = { "#000000", "#ffffff" };

        public override void Init()
        {

            // Example Title
            DrawText("Color()", 8, 8, DrawMode.TilemapCache, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);

            // Set the time to the delay to force this run on the first frame
            time = delay;

            // Draw a rect with the second color
            DrawRect(8, 48, 32, 32, 1, DrawMode.TilemapCache);

        }

        public override void Update(int timeDelta)
        {
            // Increase the time value base on the timeDelta between the last frame
            time = time + timeDelta;

            // Text to see if time is greater than the delay
            if (time > delay)
            {

                // Increase the color index by 1 and reset if it's greater than the color array
                colorIndex = Repeat(colorIndex + 1, colors.Length);

                // Update the second color value from the array
                Color(1, colors[colorIndex]);

                // Reset the timer
                time = 0;

            }

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();

            // Draw a label showing the 2nd colors current HEX value
            DrawText("Color 1 is " + Color(1), 8, 32, DrawMode.Sprite, "large", 15);

        }
    }
}
```

