--[[
  Pixel Vision 8 - PaletteOffset Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

-- Set up a time and delay
local time = 0
local delay = 800

-- This will be the direction value for the transition
local dir = 1

-- Total number of palettes for transition
local max = 4

-- Current palette ID
local paletteID = 0

function Init()

  -- Example Title
  DrawText("PaletteOffset()", 8, 8, DrawMode.TilemapCache, "large", 15)
  DrawText("Lua Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4)
  
  -- Draw the text for the palette and color ID
  DrawText("Palette   (Color ID    )", 1, 4, DrawMode.Tile, "large", 15)

end

function Update(timeDelta)

  -- Increase the time on each frame and test if it is greater than the delay
  time = time + timeDelta
  if(time > delay) then

    -- Update the palette ID based on the direction
    paletteID = paletteID + dir

    -- Test if the palette ID is too small or too large and reverse the direction
    if(paletteID >= max) then
      dir = -1
    elseif(paletteID <= 0) then
      dir = 1
    end

    -- Reset the time value
    time = 0

  end
end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Draw the sprite block using the current palette offset value
  DrawMetaSprite("lock", 8, 48, false, false, DrawMode.Sprite, PaletteOffset(paletteID))

  -- Draw the current palette number and color offset value
  DrawText(paletteID, 72, 32, DrawMode.Sprite, "large", 14)
  DrawText(PaletteOffset(paletteID), 168, 32, DrawMode.Sprite, "large", 14)

end
