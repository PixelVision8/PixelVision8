--[[
  Pixel Vision 8 - MousePosition Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

-- Store the current position of the mouse
local pos = NewPoint()

function Init()

  -- Example Title
  DrawText("MousePosition()", 8, 16, DrawMode.TilemapCache, "large", 15)
  DrawText("Lua Example", 8, 24, DrawMode.TilemapCache, "medium", 15, -4)

  -- Draw the text for where the position will be displayed
  DrawText("Mouse Position", 1, 5, DrawMode.Tile, "large", 15)

end

function Update(timeDelta)

  -- Update the mouse position
  pos = MousePosition()

  if(pos.X < 0 or pos.X > Display().X) then
    pos.X = -1;
  end

  if(pos.Y < 0 or pos.Y > Display().Y) then
    pos.Y = -1;
  end

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Test if the mouse is offscreen first
  if(pos.x == -1 or pos.y == -1) then

    -- Display that the mouse is offscreen
    DrawText("Offscreen", 128, 40, DrawMode.Sprite, "large", 14)

  else

    -- Draw a rectangle that follows the mouse on the screen
    DrawRect(pos.x, pos.y, 8, 8, 5, DrawMode.Sprite)

    -- Display the X and Y position of the mouse
    DrawText(pos.x .. "," ..pos.y, 128, 40, DrawMode.Sprite, "large", 14)

  end

end
