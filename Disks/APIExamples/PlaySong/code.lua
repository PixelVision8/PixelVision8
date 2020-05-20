--[[
  Pixel Vision 8 - PlaySong Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

local isPlaying = 0

function Update(timeDelta)

  -- Get the playing value
  isPlaying = SongData().playing

  -- Test if the left mouse button was released and if isPlaying equals 0
  if(MouseButton(0, InputState.Released) and isPlaying == 0) then

    -- Play the first song without looping
    PlaySong(0, false)

  end

end

function Draw()

  -- Redraw display
  RedrawDisplay()

  -- Reset the next row value so we know where to draw the first line of text
  local nextRow = 2

  if(isPlaying == 1) then

    -- Draw the song data label
    DrawText("Song Data:", 8, 8, DrawMode.Sprite, "large", 15)

    -- Display the song's meta data
    for key, value in next, SongData() do

      -- Draw the key value pair from the song data table
      DrawText(key .. ":", 8, nextRow * 8, DrawMode.Sprite, "large", 6)
      DrawText(value, 16 + (#key * 8), nextRow * 8, DrawMode.Sprite, "large", 14)

      -- Increment the row by 1 for the next loop
      nextRow = nextRow + 1

    end
  else

    -- Draw the instructions label
    DrawText("Click To Play Song", 8, 8, DrawMode.Sprite, "large", 15)

  end

end
