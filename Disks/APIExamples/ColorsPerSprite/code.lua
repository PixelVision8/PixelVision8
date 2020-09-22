--[[
  Pixel Vision 8 - ColorsPerSprite Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

-- Get the colors per sprite value
local cps = ColorsPerSprite()

function Draw()

  -- Clear the display
  Clear()

  -- Draw the cps value to the display
  DrawText("Colors Per Sprite = "..cps, 8, 8, DrawMode.Sprite, "large", 15)

end
