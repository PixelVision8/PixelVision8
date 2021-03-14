--[[
  Pixel Vision 8 - Display Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

-- Create a canvas to visualize the screen sizes
local canvas = NewCanvas(256, 240)

function Init()

  -- Get the full size of the display
  local sizeA = Display()

  -- Draw the two sizes to the display
  DrawText("Full Display Size " .. sizeA.x .. "x" ..sizeA.y, 1, 1, DrawMode.Tile, "large", 15)

  -- Set the canvas stroke to white
  canvas:SetStroke(14, 2)

  canvas:DrawRectangle(0, 0, sizeA.x, sizeA.y)

end

function Draw()
  -- Redraw the display
  RedrawDisplay()

  -- Draw the canvas to the display
  canvas:DrawPixels()
end
