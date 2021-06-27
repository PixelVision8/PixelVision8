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

-- Use floats to store the subpixel position
local speed = 5
local nextPos = 0

-- Use this point to position the  sprites
local pos = NewPoint()

-- Track the animation frame and the total frames
local frame = 1
local totalFrames = 4

-- Track the delay between frames
local delay = 100
local time = 0

function Init()
  
  -- Example Title
  DrawText("DrawMetaSprite()", 1, 1, DrawMode.Tile, "large", 15)
  DrawText("Lua Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4)

end

function Update(timeDelta)

  -- Increment the time
  time = time + timeDelta

  -- Check to see if we should change the animation frame
  if(time > delay) then
  
      
      -- Reset the timer
      time = 0

      -- Increment the frame
      frame = frame + 1

      -- reset the frame counter when out of bounds
      if(frame > totalFrames) then
          frame = 1
      end

  end

  -- Calculate the next position
  nextPos = nextPos + (speed * (timeDelta / 100))

  -- Need to convert the nextPoint to an int, so we'll save it in a point
  pos.X = Repeat( nextPos, Display( ).X )

  pos.Y = nextPos

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Draw sprite group moving horizontally and hide when it goes offscreen
  DrawMetaSprite("ladybug-fly-" .. frame, pos.X, 24)

  -- Draw flipped sprite group moving vertically but render when offscreen
  DrawMetaSprite("ladybug-fly-" .. frame, 36, pos.Y, true, false, DrawMode.Sprite, 0)

  -- Show the total number of sprites
  DrawText("Sprites " .. ReadTotalSprites(), 144 + 24, 224, DrawMode.Sprite, "large", 15)

  -- Draw the x,y position of each sprite
  DrawText("(" .. pos.X .. ",8)", pos.X + 24, 32, DrawMode.Sprite, "large", 15)
  DrawText("(36," .. pos.Y .. ")", 60, pos.Y + 12, DrawMode.Sprite, "large", 15)

end
