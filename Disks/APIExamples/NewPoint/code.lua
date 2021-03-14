--[[
  Pixel Vision 8 - NewPoint Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

-- Create a new point
local pos = NewPoint()

function Update(timeDelta)

  -- Increase the position by one and have it reset back to 0 if it gets bigger than the display's boundaries
  pos.x = Repeat(pos.x + 1, Display().x)
  pos.y = Repeat(pos.y + 1, Display().y)

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Draw the X and Y value of the position
  DrawText("Position " .. pos.x .. "," .. pos.y, 8, 8, DrawMode.Sprite, "large", 15)

end
