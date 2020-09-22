--[[
  Pixel Vision 8 - ScrollPosition Example
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

-- We need to know the width of the screen
local screenWidth = Display().x

-- We need total columns in the tilemap and multiply that by the sprite size to get the full width
local mapWidth = TilemapSize().x * SpriteSize().x

function Update(timeDelta)

  -- We need to text if the next position plus the screen width is less than the map's width
  if(nextPos + screenWidth < mapWidth) then

    -- Calculate the next position
    nextPos = nextPos + (speed * (timeDelta / 100))

    -- Update the scroll position
    ScrollPosition(nextPos)

  end

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Draw the life bar sprite block but ignore the scroll position so it stays fixed on the screen
  DrawSpriteBlock(300, 8, 8, 4, 2, false, false, DrawMode.Sprite, 0, false, false)

  -- Draw the exit offscreen and it will become visible when the maps scrolls to the end
  DrawSpriteBlock(104, 432, 88, 4, 4, false, false, DrawMode.Sprite)

end
