--[[
  Pixel Vision 8 - UpdateTiles Example
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
local max = 3

-- Current palette ID
local paletteID = 0

-- Loop through all of the tiles and find the ones that have sprites
local mapSize = TilemapSize()
local totalTiles = mapSize.x * mapSize.y

-- Store all of the tiles that should be updated
local tileIDs = {}

function Init()

  -- Loop through all of the tiles in the tilemap
  for i = 1, totalTiles do

    -- The index is offset by 1 since the first tile starts at 0
    local index = (i - 1)

    -- Convert the loop index to a column,row position
    local pos = CalculatePosition(index, mapSize.x)

    -- Check to see if the tile has a sprite
    if(Tile(pos.x, pos.y).spriteID > - 1) then

      -- Save the tile index value
      table.insert(tileIDs, index)

    end
  end


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

    -- Update each tile's color offset
    UpdateTiles(tileIDs, PaletteOffset(paletteID))

  end
end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Draw the text for the palette and color ID
  DrawText("Palette " .. paletteID, 32, 16, DrawMode.Sprite, "large", 15)

end
