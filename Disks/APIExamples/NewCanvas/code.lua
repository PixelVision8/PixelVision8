--[[
  Pixel Vision 8 - NewCanvas Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

-- Create a new canvas the size of the display
local canvas = NewCanvas(Display().x, Display().y)

-- This will be our solid pattern value
local solidPattern = {5}

-- This will be our checkered pattern value
local checkeredPattern = {
  5, 0, 5, 0,
  0, 5, 0, 5,
  5, 0, 5, 0,
  0, 5, 0, 5
}

function Init()

  -- Set the canvas stroke to a 1x1 pixel brush
  canvas:SetStroke({6}, 1, 1)

  -- Draw the line label to the canvas
  canvas:DrawText("Line", 8, 8, "large", 15)

  -- Draw a line between the two points
  canvas:DrawLine(8, 24, 80, 32)

  -- Change the spacing between the line's x pixels to 2 for a dotted line
  canvas:LinePattern(2, 0)

  -- Draw a line between the two points
  canvas:DrawLine(93, 24, 165, 32)

  -- Reset the space between the line's pixels back to 1
  canvas:LinePattern(1, 0)

  -- Draw the circle label to the canvas
  canvas:DrawText("Circle", 8, 40, "large", 15)

  -- Circle 1
  canvas:DrawCircle(8, 80, 64, 80)

  -- Circle 2
  canvas:SetPattern(solidPattern, 1, 1)
  canvas:DrawCircle(74, 80, 130, 80, true)

  -- Circle 3
  canvas:SetPattern(checkeredPattern, 4, 4)
  canvas:DrawCircle(140, 80, 196, 80, true)

  -- Draw the square label to the canvas
  canvas:DrawText("Square", 8, 120, "large", 15)

  -- Square 1
  canvas:DrawSquare(8, 136, 64, 192)

  -- Square 2
  canvas:SetPattern(solidPattern, 1, 1)
  canvas:DrawSquare(76, 136, 132, 192, true)

  -- Square 3
  canvas:SetPattern(checkeredPattern, 4, 4)
  canvas:DrawSquare(142, 136, 198, 192, true)

  -- Draw the canvas to the display
  canvas:DrawPixels()

end

function Draw()
  -- Redraw the display
  RedrawDisplay()
end
