--[[
  Pixel Vision 8 - New Template Script
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)  
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(), 
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at https://www.gitbook.com/@pixelvision8
]]--

LoadScript("TMaze.lua")


-- This this is an empty game, we will the following text. We combined two sets of fonts into
-- the default.font.png. Use uppercase for larger characters and lowercase for a smaller one.
local message = "Random Maze\n\n\nThis is an example of a Maze generator. Press UP to generate new and wait ... ;)"

-- The Init() method is part of the game's lifecycle and called a game starts. We are going to
-- use this method to configure background color, ScreenBufferChip and draw a text box.
function Init()

  -- Here we are manually changing the background color
  BackgroundColor(0)

  local display = Display()

  -- We are going to render the message in a box as tiles. To do this, we
  -- need to wrap the text, then split it into lines and draw each line.
  local wrap = WordWrap(message, (display.x / 8) - 2)
  local lines = SplitLines(wrap)
  local total = #lines
  local startY = ((display.y / 8) - 1) - total

  -- We want to render the text from the bottom of the screen so we offset
  -- it and loop backwards.
  for i = total, 1, - 1 do
    DrawText(lines[i], 1, startY + (i - 1), DrawMode.Tile, "large", 15)
  end
  
  map = TMaze:New(20, 20)
  map:Generate();

end

-- The Update() method is part of the game's life cycle. The engine calls Update() on every frame 
-- before the Draw() method. It accepts one argument, timeDelta, which is the difference in 
-- milliseconds since the last frame.
function Update(timeDelta)

  -- TODO add your own update logic here
  if Button(Buttons.Up, InputState.Released, 0) then
    map:Generate();
  end

  -- cave:Update(timeDelta)


end

-- The Draw() method is part of the game's life cycle. It is called after Update() and is where 
-- all of our draw calls should go. We'll be using this to render sprites to the display.
function Draw()

  -- We can use the RedrawDisplay() method to clear the screen and redraw the tilemap in a 
  -- single call.
  RedrawDisplay()
  
  -- Draw generated cellular automata map
  map:Preview(1,1);

  
  
end

