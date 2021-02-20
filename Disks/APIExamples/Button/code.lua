--[[
  Pixel Vision 8 - Button Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

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
  DrawText("Button()", 1, 1, DrawMode.Tile, "large", 15);
  DrawText("Lua Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);
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
  DrawText(message:sub(0, #message), 8, 24, DrawMode.Sprite, "medium", 14, - 4)

end
