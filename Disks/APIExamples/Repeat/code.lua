--[[
  Pixel Vision 8 - Repeat Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

-- Store the counter value and max value
local counter = 0
local counterMax = 500

function Init()

  -- Example Title
  DrawText("Repeat()", 8, 8, DrawMode.TilemapCache, "large", 15)
  DrawText("Lua Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4)

end

function Update(timeDelta)

  -- Increase the counter by 1 every frame
  counter = Repeat(counter + 1, counterMax)

end

function Draw()

  -- Redraw display
  RedrawDisplay()

  -- Draw the counter value to the display
  DrawText("Counter " .. counter .. "/" .. counterMax, 8, 32, DrawMode.Sprite, "large", 15)

end
