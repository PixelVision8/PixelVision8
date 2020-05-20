--[[
  Pixel Vision 8 - MaxSpriteCount Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

-- Store random integers for drawing a random character to the screen
local char, x, y, colorID = 0

-- Reference for the size of the display
local display = Display()

-- Total number of sprites to draw each frame
local sprites = 500

function Init()

  -- Display the text for the maximum and total number of sprites
  DrawText("Maximum Sprites " .. MaxSpriteCount(), 1, 1, DrawMode.Tile, "large", 15)
  DrawText("Total Sprites ", 1, 2, DrawMode.Tile, "large", 15)

end

function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Perform the next block of code 10 times
  for i = 1, sprites do

    -- Assign random values to each of these variable
    char = math.random(32, 126)
    x = math.random(0, display.x)
    y = math.random(32, display.y)
    colorID = math.random(1, 15)

    -- Draw a random character at a random position on the screen with a random color
    DrawText( string.char(char), x, y, DrawMode.Sprite, "large", colorID)

  end

  -- Draw the total number sprite on the display
  DrawText(ReadTotalSprites(), 15, 2, DrawMode.Tile, "large", 15)

end
