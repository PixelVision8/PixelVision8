--[[
  Pixel Vision 8 - ReplaceColor Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

-- Stores the value of the source color
local colorID = 0

-- Get the total colors and ignore any empty ones
local totalColors = TotalColors(true)

-- Set the target color ID to replace on the first empty color
local targetColorID = totalColors + 1

-- Keep track of time and delay
local delay = 300
local time = delay


function Init()

  -- Set the background to the targetColorID
  BackgroundColor(targetColorID)

end

function Update(timeDelta)

  -- Increase the time each frame and test if time is greater than the delay
  time = time + timeDelta
  if(time > delay) then

    -- Increase the color ID by 1 and repeat before reaching the last color
    colorID = Repeat(colorID + 1, totalColors - 1)

    -- Replace the target color with another color
    ReplaceColor(targetColorID, colorID)

    -- Reset time back to 0
    time = 0

  end

end

function Draw()

  -- Redraws the display
  RedrawDisplay()

  -- Draw the color value to the display
  DrawText("New Color " .. Color(targetColorID), 8, 8, DrawMode.Sprite, "large", 15)

end
