--[[
  Pixel Vision 8 - Flag Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

-- This point will store the current tile's position
local tilePosition = NewPoint()

-- This will store the current flag ID
local flagID = -1

function Init()
    -- Example Title
    DrawText("Flag()", 8, 16, DrawMode.TilemapCache, "large", 15)
    DrawText("Lua Example", 8, 24, DrawMode.TilemapCache, "medium", 15, -4)
end

function Update(timeDelta)

  -- Get the current mouse position
  tilePosition = MousePosition()

  -- Check to see if the mouse is out of bounds
  if(tilePosition.X < 0 or tilePosition.X > Display().X or tilePosition.Y < 0 or tilePosition.Y >= Display().Y) then

      -- Set all of the values to -1
      tilePosition.X = -1;
      tilePosition.Y = -1;
      flagID = -1;

      -- Return before the position and flag are calculated
      return

  end

  -- Convert the mouse position to the tilemap's correct column and row
  tilePosition.x = math.floor(tilePosition.x / 8)
  tilePosition.y = math.floor(tilePosition.y / 8)

  -- Get the flag value of the current tile
  flagID = Flag(tilePosition.x, tilePosition.y)

end

function Draw()

  -- Redraws the display
  RedrawDisplay()

  -- Display the tile and flag text on the screen
  DrawText("Tile " .. tilePosition.x .. ",".. tilePosition.y, 8, 40, DrawMode.Sprite, "large", 15)
  DrawText("Flag " .. flagID, 8, 48, DrawMode.Sprite, "large", 15)

  -- Draw a rect to represent which tile the mouse is over and set the color to match the flag ID plus 1
  DrawRect(tilePosition.x * 8, tilePosition.y * 8, 8, 8, flagID + 1, DrawMode.Sprite)

end
