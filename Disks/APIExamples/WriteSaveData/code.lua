--[[
  Pixel Vision 8 - WriteSaveData Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

function Init()

  -- Draw the last opneded text
  DrawText("Last Opened", 1, 1, DrawMode.Tile, "large", 15)

  -- Draw the saved data to the display
  DrawText(ReadSaveData("LastOpened", "Never"), 1, 2, DrawMode.Tile, "large", 14)

end

function Draw()
  -- Redraw the display
  RedrawDisplay()
end

-- When the game shuts down, it will automatically save the timestamp
function Shutdown()

  -- Write timestamp to the saves.json file.
  WriteSaveData("LastOpened", os.date('%Y-%m-%d %H:%M:%S', ts))

end
