--[[
  Pixel Vision 8 - CalculateDistance Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

local pointA = NewPoint(8, 8)
local pointB = NewPoint(248, 232)
local canvas = NewCanvas(256, 240)
local distance = 0

function Init()

  -- Set the canvas stroke to a white 1x1 pixel brush
  canvas:SetStroke(15, 1)

end

function Update(timeDelta)

  -- Update position B with the MousePosition
  pointB = MousePosition()

  -- Calculate the distance between pointA and pointB
  distance = CalculateDistance(pointA.x, pointA.y, pointB.x, pointB.x)

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Clear the canvas with the background color
  canvas:Clear(0)

  -- Draw 2 circles around each point 
  canvas:DrawEllipse(pointA.x - 4, pointA.y - 4, 10, 10)
  canvas:DrawEllipse(pointB.x - 4, pointB.y - 4, 10, 10)

  -- Draw a line between the two points
  canvas:DrawLine(pointA.x, pointA.y, pointB.x, pointB.y)

  -- Draw the distance value above pointB
  canvas:DrawText(tostring(distance), pointB.x, pointB.y - 12, "small", 15, - 4)

  -- Draw the canvas to the display
  canvas:DrawPixels()

end
