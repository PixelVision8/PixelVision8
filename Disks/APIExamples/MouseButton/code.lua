--[[
  Pixel Vision 8 - MouseButton Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

-- This array will store any buttons pressed during the current frame
local pressedButtons = {}

-- A list of mouse buttons to check on each frame
local buttons = {"left", "right"}

function Update(timeDelta)

  -- Clear the pressedButtons array on each frame
  pressedButtons = {}

  -- Loop through all the buttons
  for i = 1, #buttons do

    -- Test if the current mouse button ID is down and saves it to the pressedButtons array
    if(MouseButton((i - 1), InputState.Down)) then
      table.insert(pressedButtons, buttons[i])
    end
  end

end

function Draw()

  -- Clear the display
  Clear()

  -- Convert the pressedButtons into a string and draw to the display
  local message = table.concat(pressedButtons, ", "):upper()
  DrawText("Mouse Buttons Down:", 8, 8, DrawMode.Sprite, "large", 15)
  DrawText(message:sub(0, #message), 8, 16, DrawMode.Sprite, "medium", 14, - 4)

end
