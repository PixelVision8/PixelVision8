--[[
  Pixel Vision 8 - InputString Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

-- Store the text between frames
local inputText = ""

-- Cap on how much text will be displayed
local maxCharacters = 30

function Init()

  -- Display the instructions
  DrawText("Start Typing", 1, 1, DrawMode.Tile, "large", 15)

end

function Update(timeDelta)

  -- Check how long the input text is and clear it if when it gets too long
  if(#inputText > maxCharacters) then
    inputText = ""
  end

  -- Add the current frame's input to the previous frame's text
  inputText = inputText .. InputString()

end

function Draw()

  -- Redraw display
  RedrawDisplay()

  -- Display the text that has been entered
  DrawText(inputText, 8, 16, DrawMode.Sprite, "large", 14)

end
