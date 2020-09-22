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
  local sizeA = Display(false)

  -- Get the visible size of the display
  local sizeB = Display()

  -- Draw the two sizes to the display
  DrawText("Full Display Size " .. sizeA.x .. "x" ..sizeB.y, 1, 1, DrawMode.Tile, "large", 15)
  DrawText("Visible Display Size " .. sizeB.x .. "x" ..sizeB.y, 1, 2, DrawMode.Tile, "large", 15)

  -- Set the canvas stroke to white
  canvas:SetStroke({15}, 1, 1)

  -- Set the fill color to 5 and draw the full size square
  canvas:SetPattern({5}, 1, 1)
  canvas:DrawSquare(8, 32, sizeA.x / 2 + 8, sizeA.y / 2 + 32, true)

  -- Set the fill color to 0 and draw the visible size square
  canvas:SetPattern({0}, 1, 1)
  canvas:DrawSquare(8, 32, sizeB.x / 2 + 8, sizeB.y / 2 + 32, true)

end

function Draw()
  -- Redraw the display
  RedrawDisplay()

  -- Draw the canvas to the display
  canvas:DrawPixels()
end
