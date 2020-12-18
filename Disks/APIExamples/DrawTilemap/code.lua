--[[
  Pixel Vision 8 - DrawTilemap Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

local speed = 5
local nextPos = 0

function Update(timeDelta)

  -- Calculate the next position
  nextPos = nextPos + (speed * (timeDelta / 100))

  -- Update the scroll position
  ScrollPosition(nextPos)

end

function Draw()

  -- Clear the background
  Clear()

  -- Draw the actual tilemap starting below the top border and manually adjust the scroll offset values
  DrawTilemap(0, 16, 32, 28, nextPos, 16)

  -- Draw the tilemap top border over everything else and lock the x scroll value
  DrawTilemap(0, 0, 32, 2, 0)

  -- Display the scroll position
  DrawText("Scroll Position: " .. ScrollPosition().x..", "..ScrollPosition().y, 8, 16, DrawMode.Sprite, "large")

end
