--[[
  Pixel Vision 8 - Sound Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

-- Stores the current wave type
local waveType = 0

-- Stores all the sound effect properties
local soundProps = {}

function Init()

  -- Add label text
  DrawText("Click to Play Sound", 1, 1, DrawMode.Tile, "large", 15)
  DrawText("WaveType", 1, 2, DrawMode.Tile, "large", 15)

  -- Read first sound effect
  local soundData = Sound(0)

  -- Create a temp value for the parser
  local tmpValue = ""

  -- Loop through all the of the characters in the soundData string
  for i = 1, #soundData do

    -- Get a single character from the soundData string
    local c = soundData:sub(i, i)

    -- If the character is a comma
    if(c == ",") then

      -- Add the current string of characters to the next table position
      table.insert(soundProps, tmpValue)

      -- Reset the tmpValue
      tmpValue = ""

    else

      -- Concatenate the current character with the previous ones in the tmpValue
      tmpValue = tmpValue .. c

    end

  end

  -- Always add the last value since it doesn't end in a comma
  table.insert(soundProps, tmpValue)

end

function Update(timeDelta)

  -- Test to see if the mouse button was released and the sound is not playing
  if(MouseButton(0, InputState.Released) and IsChannelPlaying(0) == false) then

    -- Update the waveType value
    waveType = Repeat(waveType + 1, 5)
    soundProps[1] = tostring(waveType)

    -- Save the new sound data
    Sound(0, table.concat(soundProps, ","))

    -- Play the first sound on channel 0
    PlaySound(0)

  end

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Draw the wavetype ID
  DrawText(waveType, 80, 16, DrawMode.Sprite, "large", 14)

end
