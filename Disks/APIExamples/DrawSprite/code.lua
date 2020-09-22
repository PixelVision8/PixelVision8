--[[
  Pixel Vision 8 - DrawSprite Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

local speed = 5
local nextPos = 0

function Update(timeDelta)

  -- Calculate the next position
  nextPos = nextPos + (speed * (timeDelta / 100))

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Draw sprite moving horizontally
  DrawSprite(376, nextPos, 8)

  -- Draw sprite moving vertically
  DrawSprite(377, 36, nextPos)

  -- Draw the x,y position of each sprite
  DrawText("("..math.floor(nextPos)..",8)", nextPos + 8, 8, DrawMode.Sprite, "large", 15)
  DrawText("(36,"..math.floor(nextPos)..")", 44, nextPos, DrawMode.Sprite, "large", 15)

end
