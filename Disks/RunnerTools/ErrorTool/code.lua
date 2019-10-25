--[[
	Pixel Vision 8 - Preloader Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

LoadScript("sb-sprites")

local currentAnimation = {
  errorframe1,
  errorframe2,
  errorframe3,
  errorframe4,
  errorframe5,
  errorframe6,
  errorframe7,
  errorframe8,
  errorframe9,
  errorframe10,
  errorframe11,
  errorframe12,
  errorframe13,
  errorframe14
}

local animDelay = .07
local animTime = 0
local frame = 1
local loopAnimation = true

function Init()

  -- TODO Should only enable this if there is a disk loading error?
  EnableAutoRun(true)
  EnableBackKey(false)

  -- Set the background an rebuild the screen buffer
  BackgroundColor(5)

  local runnerName = SystemName()
  local runnerVer = SystemVersion() -- TODO we don't have a V char so use / instead

  local labelWidth = #runnerName + #runnerVer + 2

  local startX = math.floor((32 / 2) - (labelWidth / 2))

  DrawSprite(logosmall.spriteIDs[1], startX * 8 - 2, 8, false, false, DrawMode.TilemapCache)

  startX = startX + 1

  DrawText(runnerName, startX, 1, DrawMode.Tile, "large", 15)

  DrawText(runnerVer, startX + #runnerName + 1, 1, DrawMode.Tile, "large", 15)

  local message = ReadMetaData("errorMessage")

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

  --
end

function Update(timeDelta)

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

  local sprite = currentAnimation[frame]
  DrawSprites(sprite.spriteIDs, 104, 72, sprite.width, false, false, DrawMode.Sprite, 0)
end
