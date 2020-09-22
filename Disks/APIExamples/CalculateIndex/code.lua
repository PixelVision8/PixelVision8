--[[
  Pixel Vision 8 - CalculateIndex Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

-- A 1D array of example values
local exampleGrid = {
  "A", "B", "C",
  "D", "E", "F",
  "G", "H", "I",
}

function Init()

  -- Calculate the center index based on a grid with 3 columns
  local index = CalculateIndex(1, 1, 3)

  -- Draw the index and value to the display
  DrawText("Position 1,1 is Index " .. index .. " is " .. exampleGrid[index], 1, 1, DrawMode.Tile, "large", 15)

end

function Draw()
  -- Redraw the display
  RedrawDisplay()
end
