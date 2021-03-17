--[[
  Pixel Vision 8 - AddScript Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

-- Create Lua code as a string
local textFile =
[===[
function test()
  DrawText("Hello World", 1, 8, DrawMode.Tile, "large", 15)
end
]===]

-- Register the text file as a script
AddScript("textFile", textFile)

function Init()

  -- Example Title
  DrawText("AddScript()", 1, 1, DrawMode.Tile, "large", 15)
  DrawText("Lua Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4)

  -- Call the text method
  test()
end

function Draw()
  -- Redraw the display
  RedrawDisplay()
end
