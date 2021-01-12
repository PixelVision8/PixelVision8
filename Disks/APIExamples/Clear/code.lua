--[[
  Pixel Vision 8 - Clear Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

local display = Display()

-- Create a delay and time value
local delay = 2000
local time = 0

-- This flag will toggle between a full or partial clear
local clearFlag = false

-- Store random integers for drawing a random character to the screen
local char, x, y, colorID = 0

function Update(timeDelta)

  -- Increase the time value base on the timeDelta between the last frame
  time = time + timeDelta

  -- Text to see if time is greater than the delay
  if(time > delay) then

    -- Toggle the clear flag
    clearFlag = true

    -- Reset the timer
    time = 0

  end

end

function Draw()

  -- Test the clear flag and do a full or partial clear based on the value
  if(clearFlag == true) then
    
    Clear()

    clearFlag= false
  end

  -- Perform the next block of code 10 times
  for i = 1, 10 do

    -- Assign random values to each of these variable
    char = math.random(32, 126)
    x = math.random(0, display.x)
    y = math.random(0, display.y)
    colorID = math.random(1, 15)

    -- Draw a random character at a random position on the screen with a random color
    DrawText( string.char(char), x, y, DrawMode.Sprite, "large", colorID)

  end

end
