--[[
  Pixel Vision 8 - Preloader Tool
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  Please do not copy and distribute verbatim copies
  of this license document, but modifications without
  distributing is allowed.
]]--

local currentAnimation = {
  "errorframe1",
  "errorframe2",
  "errorframe3",
  "errorframe4",
  "errorframe5",
  "errorframe6",
  "errorframe7",
  "errorframe8",
  "errorframe9",
  "errorframe10",
  "errorframe11",
  "errorframe12",
  "errorframe13",
  "errorframe14"
}

local animDelay = .07
local animTime = 0
local frame = 1

function Init()

  playSounds = ReadBiosData("PlaySystemSounds", "True") == "True"

  if(EnableAutoRun ~= nil) then
    -- TODO Should only enable this if there is a disk loading error?
    EnableAutoRun(true)

  end

  -- Set the background an rebuild the screen buffer
  BackgroundColor(tonumber(ReadBiosData("DefaultBackgroundColor", "5")))

  local message = ReadMetadata("errorMessage")

  local display = Display()

  -- We are going to render the message in a box as tiles. To do this, we need to wrap the
  -- text, then split it into lines and draw each line.
  local wrap = WordWrap(message, (display.x / 8) - 3)
  local lines = SplitLines(wrap)
  local total = #lines
  local startY = ((display.y / 8) - 1) - total

  -- We want to render the text from the bottom of the screen so we offset it and loop backwards.
  for i = total, 1, - 1 do
    DrawText(lines[i], 1, startY + (i - 1), DrawMode.Tile, "large", 15)
  end

  if(playSounds) then
    PlaySound(0)
  end
  --
end

function Update(timeDelta)

  -- Convert timeDelta to a float
  timeDelta = timeDelta / 1000

  animTime = animTime + timeDelta
  
  if(animTime > animDelay) then
    animTime = 0
    frame = frame + 1
    if(frame > #currentAnimation) then
      frame = 1
    end
  end

end

function Draw()

  RedrawDisplay()

  DrawMetaSprite(FindMetaSpriteId(currentAnimation[frame]), 104, 71)

end
