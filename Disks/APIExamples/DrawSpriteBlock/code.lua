--[[
  Pixel Vision 8 - DrawSpriteBlock Example
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

  -- Draw sprite block moving horizontally and hide when it goes offscreen
  DrawSpriteBlock(168, nextPos, 8, 2, 2)

  -- Draw flipped sprite block moving vertically but render when offscreen
  DrawSpriteBlock(168, 36, nextPos, 2, 2, true, false, DrawMode.Sprite, 0, false)

  -- Draw the x,y position of each sprite
  DrawText("("..math.floor(nextPos)..",8)", nextPos + 20, 8, DrawMode.Sprite, "large", 15)
  DrawText("(36,"..math.floor(nextPos)..")", 56, nextPos, DrawMode.Sprite, "large", 15)

end
