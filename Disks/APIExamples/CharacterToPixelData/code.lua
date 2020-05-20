--[[
  Pixel Vision 8 - CharacterToPixelData Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

local pixelData = nil

function Init()

  -- Get the raw pixel data for the A character
  pixelData = CharacterToPixelData("A", "large")

  -- Loop through all of the pixels
  for i = 1, #pixelData do

    -- Test to see if the pixel is set to the color ID 0
    if(pixelData[i] == 0) then

      -- Change the color ID to 14
      pixelData[i] = 14

    end

  end

end

function Draw()

  -- Redraw display
  RedrawDisplay()

  -- Use the normal DrawText() API to display the A
  DrawText("A", 8, 8, DrawMode.Sprite, "large", 15)

  -- Draw the pixel data to the display next to the A
  DrawPixels(pixelData, 16, 8, 8, 8)

end
