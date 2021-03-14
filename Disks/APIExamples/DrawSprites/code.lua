--[[
  Pixel Vision 8 - DrawSprites Example
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

-- A group of sprite IDs for the DrawSprites() API
local spriteGroup = 
{
  -1, 33, 34, - 1,
  48, 49, 50, 51,
  64, 65, 66, 67,
  -1, 81, 82, - 1
}

function Update(timeDelta)

  -- Calculate the next position
  nextPos = nextPos + (speed * (timeDelta / 100))

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Draw sprite group moving horizontally and hide when it goes offscreen
  DrawSprites(spriteGroup, nextPos, 8, 4)

  -- Draw flipped sprite group moving vertically but render when offscreen
  DrawSprites(spriteGroup, 36, nextPos, 4, true, false, DrawMode.Sprite, 0, false)

  -- Show the total number of sprites
  DrawText("Sprites ".. ReadTotalSprites(), 144 + 24, 224, DrawMode.Sprite, "large", 15)

  -- Draw the x,y position of each sprite
  DrawText("("..math.floor(nextPos)..",8)", nextPos + 32, 20, DrawMode.Sprite, "large", 15)
  DrawText("(36,"..math.floor(nextPos)..")", 66, nextPos + 12, DrawMode.Sprite, "large", 15)

end
