--[[
  Pixel Vision 8 - DrawPixels Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

local pixelData = {
  -1, - 1, - 1, 0, 0, 0, 0, - 1,
  -1, - 1, 0, 0, 14, 14, 0, - 1,
  -1, 0, 0, 14, 14, 14, 0, - 1,
  -1, 0, 14, 14, 14, 13, 0, - 1,
  -1, 0, 0, 0, 14, 13, 0, - 1,
  -1, - 1, - 1, 0, 14, 13, 0, - 1,
  -1, - 1, - 1, 0, 13, 13, 0, - 1,
  -1, - 1, - 1, 0, 0, 0, 0, - 1,
}

function Init()

  -- Draw the sprite data to the tilemap cache
  DrawText("Tilemap Cache", 1, 3, DrawMode.Tile, "large", 15)
  DrawPixels(pixelData, 8, 32, 8, 8, false, false, DrawMode.TilemapCache)
  DrawPixels(pixelData, 16, 32, 8, 8, true, false, DrawMode.TilemapCache)
  DrawPixels(pixelData, 24, 32, 8, 8, false, true, DrawMode.TilemapCache)
  DrawPixels(pixelData, 32, 32, 8, 8, true, true, DrawMode.TilemapCache)

  -- Shfit the pixel data color IDs by 1
  DrawPixels(pixelData, 40, 32, 8, 8, false, false, DrawMode.TilemapCache, 1)

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Label for the sprite layer examples
  DrawText("Sprite", 1, 6, DrawMode.Tile, "large", 15)

  -- You can simplify the call if you are not flipping the pixel data
  DrawPixels(pixelData, 8, 56, 8, 8)

  -- Fliping the pixel data on the sprite layer, which is used by default when not provided
  DrawPixels(pixelData, 16, 56, 8, 8, true, false)
  DrawPixels(pixelData, 24, 56, 8, 8, false, true)
  DrawPixels(pixelData, 32, 56, 8, 8, true, true)

  -- Shift the pixel data color IDs over by 1 requires passing in the draw mode
  DrawPixels(pixelData, 40, 56, 8, 8, false, false, DrawMode.Sprite, 1)

  -- Draw pixel data to the sprite below layer
  DrawText("Sprite Below", 1, 9, DrawMode.Tile, "large", 15)
  DrawPixels(pixelData, 8, 80, 8, 8, false, false, DrawMode.SpriteBelow)
  DrawPixels(pixelData, 16, 80, 8, 8, true, false, DrawMode.SpriteBelow)
  DrawPixels(pixelData, 24, 80, 8, 8, false, true, DrawMode.SpriteBelow)
  DrawPixels(pixelData, 32, 80, 8, 8, true, true, DrawMode.SpriteBelow)
  DrawPixels(pixelData, 40, 80, 8, 8, false, false, DrawMode.SpriteBelow, 1)

  -- Display the total sprites used during this frame
  DrawText("Total Sprites " .. ReadTotalSprites(), 8, 8, DrawMode.Sprite, "large", 14)

end
