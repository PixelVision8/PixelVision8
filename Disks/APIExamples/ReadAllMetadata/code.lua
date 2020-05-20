--[[
  Pixel Vision 8 - ReadAllMetadata Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

function Init()

  -- Set up the values we'll use to keep track of the metadata
  local nextRow = 3
  local maxRows = 29
  local counter = 0
  local metadata = ReadAllMetadata()

  -- Iterate over the game's metadata
  for key, value in next, metadata do

    -- We only display a key/value pair if there is space on the display
    if(nextRow < maxRows ) then

      -- Draw the key value pair from the game's metadata
      DrawText(key .. ":", 8, nextRow * 8, DrawMode.TilemapCache, "large", 6)
      DrawText(value, (2 + #key) * 8, nextRow * 8, DrawMode.TilemapCache, "large", 14)

      -- Increment the row by 1 for the next loop
      nextRow = nextRow + 1
    end

    -- Keep track of the total amount of metadata keys
    counter = counter + 1

  end

  -- Display the amount displayed and the total in the game's metadata
  DrawText("Showing " .. (nextRow - 3) .." of " .. counter .. " Metadata Keys", 8, 8, DrawMode.TilemapCache, "large", 15)

end

function Draw()
  -- Redraw the display
  RedrawDisplay()
end
