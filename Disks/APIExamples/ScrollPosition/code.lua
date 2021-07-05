--[[
  Pixel Vision 8 - ScrollPosition Example
  Copyright (C) 2017, Pixel Vision 8 (http:--pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https:--www.pixelvision8.com/getting-started
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
  DrawMetaSprite("life-bar", 224, 16, false, false, DrawMode.Sprite)

  -- Draw the exit offscreen and it will become visible when the maps scrolls to the }
  DrawMetaSprite("exit", 432 - nextPos, 88 + 32, false, false, DrawMode.Sprite)

  -- Example Title
  DrawText("ScrollPosition()", 8, 16, DrawMode.Sprite, "large", 15)
  DrawText("Lua Example", 8, 24, DrawMode.Sprite, "medium", 15, -4)

end
