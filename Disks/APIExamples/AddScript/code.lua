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
  DrawText("Hello World", 1, 1, DrawMode.Tile, "large", 15)
end
]===]

-- Register the text file as a script
AddScript("textFile", textFile)

function Init()
  -- Call the text method
  test()
end

function Draw()
  -- Redraw the display
  RedrawDisplay()
end
