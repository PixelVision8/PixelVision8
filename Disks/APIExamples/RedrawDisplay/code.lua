--[[
  Pixel Vision 8 - RedrawDisplay Example
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
  DrawText("RedrawDisplay()", 8, 8, DrawMode.TilemapCache, "large", 15)
  DrawText("Lua Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4)

  -- Draw the FPS label to the tilemap
  DrawText("FPS ", 1, 4, DrawMode.Tile, "large", 15)

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Draw the FPS value to the display on every frame
  DrawText(ReadFPS(), 40, 32, DrawMode.Sprite, "large", 14)

end
