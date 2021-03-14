--[[
  Pixel Vision 8 - Sprite Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

local delay = 500
local time = delay

function Update(timeDelta)

  time = time + timeDelta

  if(time > delay) then

    -- Get the first sprite's pixel data
    local pixelData = Sprite(0)

    -- Loop through all of the pixels
    for i = 1, #pixelData do

      -- Set a random pixel color
      pixelData[i] = math.random(0, 15)

    end

    -- Save the pixel data back
    Sprite(0, pixelData)

    time = 0
  end

end

function Draw()
  -- Redraw the display
  RedrawDisplay()
end
