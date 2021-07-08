--[[
  Pixel Vision 8 - SplitLines Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

-- Message to display on the screen
local message = "PIXEL VISION 8\n\nVisit 'pixelvision8.com' to learn more about creating games from scratch."

function Init()

  -- Example Title
  DrawText("SplitLines()", 8, 8, DrawMode.TilemapCache, "large", 15)
  DrawText("Lua Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4)
  
  -- To convert the message into lines of text we need to wrap it then split it
  local wrap = WordWrap(message, (Display().x / 8) - 2)
  local lines = SplitLines(wrap)

  -- Loop through each line of text and draw it to the display
  for i = 1, #lines do
    DrawText(lines[i], 1, i + 4, DrawMode.Tile, "large", 15)
  end

end

function Draw()
  -- Redraw the display
  RedrawDisplay()
end
