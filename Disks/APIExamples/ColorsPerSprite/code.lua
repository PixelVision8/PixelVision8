--[[
  Pixel Vision 8 - ColorsPerSprite Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

function Init()

  -- Change the background color
  BackgroundColor( 6 )

  -- Example Title
  DrawText("ColorsPerSprite()", 8, 8, DrawMode.TilemapCache, "large", 15)
  DrawText("Lua Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4)

  -- Draw the cps value to the display
  DrawText("Colors Per Sprite = " .. ColorsPerSprite(), 8, 32, DrawMode.TilemapCache, "large", 15)

end

function Draw()

  -- Clear the display
  RedrawDisplay()

  -- Draw meta sprite to the display
  DrawMetaSprite( "reaper-boy", 8, 48)

end