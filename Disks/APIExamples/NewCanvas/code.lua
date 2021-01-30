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
  canvas:SetStroke(6, 1)

  -- Draw the line label to the canvas
  canvas:DrawText("Line Lua", 8, 8, "large", 15)

  -- Draw a line between the two points
  canvas:DrawLine(8, 24, 80, 24)

  -- Draw a line between the two points
  canvas:DrawLine(93, 24, 165, 32)

  -- Draw the Ellipse label to the canvas
  canvas:DrawText("Ellipse", 8, 40, "large", 15)

  -- Ellipse 1
  canvas:DrawEllipse(8, 52, 64, 64)

  -- Ellipse 2
  canvas:SetPattern(solidPattern, 1, 1)
  canvas:DrawEllipse(79, 52, 64, 64, true)

  -- Ellipse 3
  canvas:SetPattern(checkeredPattern, 4, 4)
  canvas:DrawEllipse(150, 52, 64, 64, true)

  -- Draw the Rectangle label to the canvas
  canvas:DrawText("Rectangle", 8, 120, "large", 15)

  -- Rectangle 1
  canvas:DrawRectangle(8, 136, 64, 64)

  -- Rectangle 2
  canvas:SetPattern(solidPattern, 1, 1)
  canvas:DrawRectangle(79, 136, 64, 64, true)

  -- Rectangle 3
  canvas:SetPattern(checkeredPattern, 4, 4)
  canvas:DrawRectangle(150, 136, 64, 64, true)

  -- Draw the canvas to the display
  canvas:DrawPixels()

end

function Draw()
  -- Redraw the display
  RedrawDisplay()
end
