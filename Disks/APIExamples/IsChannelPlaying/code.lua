--[[
  Pixel Vision 8 - IsChannelPlaying Example
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at
  https://www.pixelvision8.com/getting-started
]]--

function Init()

  -- Example Title
  DrawText("IsChannelPlaying()", 8, 8, DrawMode.TilemapCache, "large", 15)
  DrawText("Lua Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4)
  
  -- Display the instructions
  DrawText("Press the 1 or 2 key", 1, 4, DrawMode.Tile, "large", 15)
  DrawText("Channel 0 is playing ", 1, 5, DrawMode.Tile, "large", 15)
  DrawText("Channel 1 is playing ", 1, 6, DrawMode.Tile, "large", 15)

end

function Update(timeDelta)

  -- Check for the 1 key to be pressed and play sound ID 0 on channel 0
  if(Key(Keys.D1, InputState.Released)) then
    PlaySound(0, 0)
  end

  -- Only play sound 1 if the channel is not currently playing a sound
  if(Key(Keys.D2, InputState.Released) and IsChannelPlaying(1) == false) then
    PlaySound(1, 1)
  end

end

-- The Draw() method is part of the game's life cycle. It is called after Update() and is where
-- all of our draw calls should go. We'll be using this to render sprites to the display.
function Draw()

  -- Redraw the display
  RedrawDisplay()

  -- Draw channel 0 and 1's current playing state to the display
  DrawText(tostring(IsChannelPlaying(0)), 176, 40, DrawMode.Sprite, "large", 14)
  DrawText(tostring(IsChannelPlaying(1)), 176, 48, DrawMode.Sprite, "large", 14)

end
