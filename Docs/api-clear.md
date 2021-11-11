Clearing the display removes all of the existing pixel data, replacing it with the default background color. By calling` Clear()`, it automatically clears the entire display. The `Clear()` can only be used once at the beginning of the the draw phase no matter how many times you use it or where you put it in your code.

## Usage

```csharp
Clear ()
```

## Example

In this example, we are going to use a simple timer to clear the entire screen. Running this code will output the following:

![image alt text](Images/ClearOutput.png)

## Lua

```lua
local display = Display()

-- Create a delay and time value
local delay = 2000
local time = 0

-- This flag will toggle between a full or partial clear
local clearFlag = false

-- Store random integers for drawing a random character to the screen
local char, x, y, colorID = 0

function Update(timeDelta)

  -- Increase the time value base on the timeDelta between the last frame
  time = time + timeDelta

  -- Text to see if time is greater than the delay
  if(time > delay) then

    -- Toggle the clear flag
    clearFlag = true

    -- Reset the timer
    time = 0

  end

end

function Draw()

  -- Test the clear flag and do a full or partial clear based on the value
  if(clearFlag == true) then
    
    Clear()

    clearFlag= false
  end

  -- Perform the next block of code 10 times
  for i = 1, 10 do

    -- Assign random values to each of these variable
    char = math.random(32, 126)
    x = math.random(0, display.x)
    y = math.random(0, display.y)
    colorID = math.random(1, 15)

    -- Draw a random character at a random position on the screen with a random color
    DrawText( string.char(char), x, y, DrawMode.Sprite, "large", colorID)

  end

  -- Example Title
  DrawText("Clear()", 8, 8, DrawMode.Sprite, "large", 15)
  DrawText("Lua Example", 8, 16, DrawMode.Sprite, "medium", 15, -4)

end
```



## C#

```csharp
using System;

namespace PixelVision8.Player
{
    class ClearExample : GameChip
    {
        // Create a new random generator
        Random random = new Random();

        // We'll store the display's boundaries here
        private Point display;

        // Create a delay and time value
        private int delay = 2000;
        private int time = 0;

        // This flag will toggle between a full or partial clear
        private bool clearFlag = false;

        // Store random integers for drawing a random character to the screen
        private int charID, x, y, colorID;

        public override void Init()
        {
            // Save the display's boundaries when the game starts up
            display = Display();
        }

        public override void Update(int timeDelta)
        {
            // Increase the time value base on the timeDelta between the last frame
            time = time + timeDelta;

            // Text to see if time is greater than the delay
            if (time > delay)
            {

                // Toggle the clear flag
                clearFlag = true;

                // Reset the timer
                time = 0;

            }
        }

        public override void Draw()
        {
            // Test the clear flag and do a full or partial clear based on the value
            if (clearFlag == true)
            {
                Clear();

                clearFlag = false;

            }
            
            // Perform the next block of code 10 times
            for (int i = 0; i < 10; i++)
            {

                // Assign random values to each of these variable
                charID = random.Next(32, 126);
                x = random.Next(0, display.X);
                y = random.Next(0, display.Y);
                colorID = random.Next(1, 15);

                // Draw a random charIDacter at a random position on the screen with a random color
                DrawText(Convert.ToChar(charID).ToString(), x, y, DrawMode.Sprite, "large", colorID);

            }

            // Example Title
            DrawText("Clear()", 8, 8, DrawMode.Sprite, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.Sprite, "medium", 15, -4);
        }
    }
}
```

