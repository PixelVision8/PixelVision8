The main form of input for Pixel Vision 8 is the controller's buttons. You can get the current state of any button by calling the `Button()` method and supplying a button ID. There are optional parameters for specifying an `InputState` and the controller ID. When called, the `Button()` method returns a `bool` for the requested button and its state. 

The `InputState `enum contains options for testing the `Down `and `Released `states of the supplied button ID. By default, `Down `is automatically used which returns `true `when the key was pressed in the current frame. When using `Released`, the method returns `true `if the key is currently up but was down in the last frame.

## Usage

```csharp
Button ( button, state, controllerID )
```

## Arguments

| Name         | Value      | Description                                                                                                                    |
|--------------|------------|--------------------------------------------------------------------------------------------------------------------------------|
| button       | Buttons    | Accepts the Buttons enum or int for the button's ID\.                                                                          |
| state        | InputState | Optional InputState enum\. Returns down state by default\.                                                                     |
| controllerID | int        | An optional int representing a controller ID\. Player 1 is 0 and player 2 is 1\. leaving this empty will deafult to player 1\. |



## Returns

| Value | Description                                       |
|-------|---------------------------------------------------|
| bool  | Returns a bool based on the state of the button\. |



## Button Enums

Each controller has 8 buttons. A button has a specific ID or you can reference it by the button’s enum name. 

| Enum            | Value |
|-----------------|-------|
| Buttons\.Up     | 0     |
| Buttons\.Down   | 1     |
| Buttons\.Left   | 2     |
| Buttons\.Right  | 3     |
| Buttons\.A      | 4     |
| Buttons\.B      | 5     |
| Buttons\.Select | 6     |
| Buttons\.Start  | 7     |



## Input State Enums

There are two input states you can use to test a button’s current state:

| Enum                 | Value |
|----------------------|-------|
| InputState\.Down     | 0     |
| InputState\.Released | 1     |

## Example

In this example, we will loop through all of the buttons on controller 1 and display their names on the screen. Running this code will output the following:

![image alt text](Images/ButtonOutput.png)

## Lua

```lua
-- This array will store any buttons pressed during the current frame
local pressedButtons = {}

-- A list of all the buttons to check on each frame
local buttons = {
  Buttons.Up,
  Buttons.Down,
  Buttons.Left,
  Buttons.Right,
  Buttons.A,
  Buttons.B,
  Buttons.Select,
  Buttons.Start
}

function Init()

  -- Example Title
  DrawText("Button()", 1, 1, DrawMode.Tile, "large", 15);
  DrawText("Lua Example - Press a direction or action button", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);

end

function Update(timeDelta)

  -- Clear the pressedButtons array on each frame
  pressedButtons = {}

  -- Loop through all the buttons
  for i = 1, #buttons do

    -- Test if player 1's current button is down and save it to the pressedButtons array
    if(Button(buttons[i], InputState.Down, 0)) then
      table.insert(pressedButtons, tostring(buttons[i]))
    end
    
  end

end

function Draw()

  -- Clear the display
  RedrawDisplay()

  -- Convert the pressedButtons into a string and draw to the display
  local message = table.concat(pressedButtons, ", "):upper()

  -- DrawText("Buttons Down:", 8, 8, DrawMode.Sprite, "large", 15)
  DrawText(message:sub(0, #message), 8, 32, DrawMode.Sprite, "medium", 14, - 4)

end
```



## C#

```c#
using System.Collections.Generic;

namespace PixelVision8.Player
{
    class ButtonExample : GameChip
    {
        // This array will store any buttons pressed during the current frame
        private List<string> pressedButtons = new List<string>();

        // A list of all the buttons to check on each frame
        private Buttons[] buttons =
        {
            Buttons.Up,
            Buttons.Down,
            Buttons.Left,
            Buttons.Right,
            Buttons.A,
            Buttons.B,
            Buttons.Select,
            Buttons.Start
        };

        public override void Init()
        {
            // Example Title
            DrawText("Button()", 1, 1, DrawMode.Tile, "large", 15);
            DrawText("C Sharp Example - Press a direction or action button", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);
        }

        public override void Update(int timeDelta)
        {

            // Clear the pressedButtons array on each frame
            pressedButtons.Clear();

            // Loop through all the buttons
            for (int i = 0; i < buttons.Length; i++)
            {

                // Test if player 1's current button is down and save it to the pressedButtons array
                if (Button(buttons[i], InputState.Down, 0))
                {
                    pressedButtons.Add(buttons[i].ToString());
                }

            }
        }

        public override void Draw()
        {

            // Clear the display
            RedrawDisplay();

            // Convert the pressedButtons into a string and draw to the display
            var message = string.Join(", ", pressedButtons.ToArray()).ToUpper();
            
            // DrawText("Buttons Down:", 8, 8, DrawMode.Sprite, "large", 15);
            DrawText(message.Substring(0, message.Length), 8, 32, DrawMode.Sprite, "medium", 14, -4);

        }
    }
}
```

