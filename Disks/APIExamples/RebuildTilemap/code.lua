--[[
  Pixel Vision 8 - RebuildTilemap Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

function Init()

  -- Add text to the tilemap
  DrawText("Click to rebuild the tilemap", 1, 1, DrawMode.Tile, "large", 15)
  DrawText("Text on the tilemap cache", 8, 16, DrawMode.TilemapCache, "large", 5)

end

function Update(timeDelta)

  -- Detect if the mouse button was released and trigger the tilemap to rebuild
  if(MouseButton(0, InputState.Released)) then
    RebuildTilemap()
  end

end

function Draw()
  -- Redraw the display
  RedrawDisplay()
end
