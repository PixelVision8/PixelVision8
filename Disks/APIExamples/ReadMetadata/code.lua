--[[
  Pixel Vision 8 - ReadMetadata Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

function Init()

  -- Display the name of the game from the game's metadata
  DrawText("Game Name:", 1, 1, DrawMode.Tile, "large", 15)
  DrawText(ReadMetadata("GameName", "Untitled"), 12, 1, DrawMode.Tile, "large", 14)

end

function Draw()
  -- Redraw the display
  RedrawDisplay()
end
