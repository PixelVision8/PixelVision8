--[[
  Pixel Vision 8 - DrawRect Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

function Init()

  -- Example Title
  DrawText("DrawRect()", 1, 1, DrawMode.Tile, "large", 15)
  DrawText("Lua Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4)
  
  -- Draw a 100 x 100 pixel rect to the display
  DrawRect(16, 40, 100, 100, 5, DrawMode.TilemapCache)

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Draw a rect to the sprite layer
  DrawRect(12, 36, 25, 25, 14, DrawMode.Sprite)

  -- Draw a rect to the sprite below layer
  DrawRect(100, 124, 25, 25, 15, DrawMode.SpriteBelow)

end
