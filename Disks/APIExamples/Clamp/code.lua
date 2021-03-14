--[[
  Pixel Vision 8 - Clamp Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

local counter = 0
local time = 0
local delay = 300

function Update(timeDelta)

  -- Add the time delay to the time
  time = time + timeDelta

  -- Check if time is greater than the delay
  if(time > delay) then

    -- Increase the counter by 1
    counter = Clamp(counter + 1, 0, 10)

    -- Reset the time
    time = 0

  end

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Draw the counter to the display
  DrawText("Counter " .. counter, 8, 8, DrawMode.Sprite, "large", 15)

end
