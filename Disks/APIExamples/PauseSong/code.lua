--[[
  Pixel Vision 8 - PauseSong Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

function Init()

  -- Draw the song data label
  DrawText("Song Data:", 1, 1, DrawMode.Tile, "large", 15)

  -- Start playing the song on a loop
  PlaySong(0, true)

end

function Update(timeDelta)

  -- Test if the left mouse button was released to pause the song
  if(MouseButton(0, InputState.Released)) then
    PauseSong()
  end

end

function Draw()

  -- Redraw display
  RedrawDisplay()

  -- Reset the next row value so we know where to draw the first line of text
  local nextRow = 2

  -- Display the song's metadata
  for key, value in next, SongData() do

    -- Draw the key value pair from the song data table
    DrawText(key .. ":", 8, nextRow * 8, DrawMode.Sprite, "large", 6)
    DrawText(value, 16 + (#key * 8), nextRow * 8, DrawMode.Sprite, "large", 14)

    -- Increment the row by 1 for the next loop
    nextRow = nextRow + 1

  end

end
