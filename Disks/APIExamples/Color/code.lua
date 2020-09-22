--[[
  Pixel Vision 8 - Color Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

-- Create a delay and set the time to that value so it triggers right away
local delay = 500
local time = delay

-- Create an array of colors and an index value to point to the currently selected color
local colorIndex = 1
local colors = {"#000000", "#ffffff"}

function Init()

  -- Draw a rect with the second color
  DrawRect(8, 24, 32, 32, 1, DrawMode.TilemapCache)

end

function Update(timeDelta)

  -- Increase the time value base on the timeDelta between the last frame
  time = time + timeDelta

  -- Text to see if time is greater than the delay
  if(time > delay) then

    -- Increase the color index by 1 and reset if it's greater than the color array
    colorIndex = colorIndex + 1
    if(colorIndex > #colors) then
      colorIndex = 1
    end

    -- Update the second color value from the array
    Color(1, colors[colorIndex])

    -- Reset the timer
    time = 0

  end

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Draw a label showing the 2nd colors current HEX value
  DrawText("Color 1 is " .. Color(1), 8, 8, DrawMode.Sprite, "large", 15)

end
