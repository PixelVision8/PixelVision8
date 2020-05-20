--[[
  Pixel Vision 8 - TilemapSize Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

function Init()

  -- Get the tilemap size and sprite size
  local mapSize = TilemapSize()
  local tileSize = SpriteSize()

  -- Display the tilemap size in tiels and pixels to the screen
  DrawText("Tilemap Size:", 1, 1, DrawMode.Tile, "large", 15)
  DrawText("Tiles " .. mapSize.x .. " x " .. mapSize.y, 1, 2, DrawMode.Tile, "large", 15)
  DrawText("Total Tiles " .. (mapSize.x * mapSize.y), 1, 3, DrawMode.Tile, "large", 15)
  DrawText("Pixels " .. (mapSize.x * tileSize.x) .. " x " .. (mapSize.y * tileSize.y), 1, 4, DrawMode.Tile, "large", 15)

end

function Draw()
  -- Redraw the display
  RedrawDisplay()
end
