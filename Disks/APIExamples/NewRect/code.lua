--[[
  Pixel Vision 8 - NewRect Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

-- Create a rectangle
local rectA = NewRect(8, 8, 128, 128)

-- This will store the mouse position
local mousePos = NewPoint()

-- This will store the collision state
local collision = false

function Update(timeDelta)

  -- Get the mouse position
  mousePos = MousePosition()

  -- Test for the collision
  collision = rectA:Contains(mousePos)

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Draw rectA and change the color if there is a collision
  DrawRect(rectA.x, rectA.y, rectA.width, rectA.height, collision and 6 or 5)

  -- Draw the mouse cursor on the screen
  DrawRect(mousePos.x - 1, mousePos.y - 1, 2, 2, 15)

end
