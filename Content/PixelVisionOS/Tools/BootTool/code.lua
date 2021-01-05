--[[
	Pixel Vision 8 - Boot Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

LoadScript("sb-sprites")
LoadScript("boot-text")

-- Get references to boot animation sprite data
local bootSprites = {
  logoframe01,
  logoframe02,
  logoframe03,
  logoframe04,
  logoframe05,
  logoframe06,
  logoframe07,
  logoframe08,
  logoframe09,
  logoframe10,
  logoframe11,
  logoframe12,
  logoframe13,
  logoframe14,
  logoframe15,
  logoframe15,
  logoframe15,
  logoframe15,
  logoframe15,
  logoframe15
}

-- Animation properties
local animDelay = .1;
local time = 0;
local frame = 1;
local done = false;
local nextScreenDelay = .3
local nextScreenTime = 0
local startDelay = .5
local ready = false
local bottomBorder = 232
local safeMode = false
local showPlugin = -1

function Init()

  playSounds = ReadBiosData("PlaySystemSounds", "True") == "True"

  if(EnableAutoRun ~= nil) then
    EnableAutoRun(false)
  end

  if(EnableBackKey ~= nil) then
    EnableBackKey(false)
  end

  -- Set the default background color
  BackgroundColor(5)

  local display = Display(false)

  -- We are going to render the message in a box as tiles. To do this, we need to wrap the
  -- text, then split it into lines and draw each line.
  local wrap = WordWrap(message, (display.x / 8) - 3)
  local lines = SplitLines(wrap)
  local total = #lines
  local startY = (240 + 16) / 8

  --176 448

  local runnerName = SystemName()
  local runnerVer = SystemVersion():upper()

  local labelWidth = (#runnerName * 8) + (#runnerVer * 4) + 16

  local startX = 256 - labelWidth

  DrawText(runnerName, startX, 225, DrawMode.TilemapCache, "large", 11)

  DrawText(runnerVer, startX + (#runnerName * 8) + 8, 225, DrawMode.TilemapCache, "small", 11, - 4)

  -- DrawText("Runner Version")

  -- We want to render the text from the bottom of the screen so we offset it and loop backwards.
  for i = 1, total do
    DrawText(lines[i], 1, startY + (i - 1), DrawMode.Tile, "large", 15)
  end

  -- Replace the tile with a logo and rest the color offset to 0 (since the font was set to 15)
  Tile(1, startY, logosmall.spriteIDs[1], 0)

end

function Update(timeDelta)

  -- Convert timeDelta to a float
  timeDelta = timeDelta / 1000

  -- Track time of animation
  time = time + timeDelta

  shortcutTime = time + timeDelta

  if(Key(Keys.Escape)) then

    -- Trigger BootDone
    BootDone(safeMode)
    return
  end
  -- if(shortcutTime > shortcutDelay) then
  --   checkShortcuts = false
  -- end
  --
  -- if(checkShortcuts == true) then

  -- end

  -- Test to see if we are ready to display the boot animation
  if(ready == false) then

    -- If the current time is less than the start delay exit the Update method
    if(time < startDelay) then
      return

    else
      -- If the time has passed, reset time for the next frame and change ready to true
      time = 0
      ready = true

      if(playSounds) then
        -- Play the boot song
        PlayPattern(0, false)
      end

    end

  end

  -- If animation is past delay
  if(time > animDelay) then

    -- Reset animation time value
    time = 0

    if(done == true)then

      local newScrollY = ScrollPosition().y

      if(newScrollY < bottomBorder) then

        newScrollY = newScrollY + math.floor(500 * timeDelta)

        if(newScrollY > bottomBorder) then
          newScrollY = bottomBorder
        end

        -- scroll background to new position
        ScrollPosition(0, newScrollY)

        -- Check that we are in boot mode
      else -- if(editorBridge.mode == 2) then

        nextScreenTime = nextScreenTime + timeDelta

        if(nextScreenTime > nextScreenDelay) then
          -- Trigger BootDone
          BootDone(safeMode)

        end

      end

    else

      -- Test to see if we are done
      if(frame < #bootSprites) then

        KeyPressCheck()

        -- Not done with animation, go to next frame
        frame = frame + 1
        local sprite = bootSprites[frame]
        DrawSprites(sprite.spriteIDs, 10, 12, sprite.width, false, false, DrawMode.Tile)
        -- UpdateTiles(, )

      elseif(done == false) then

        if(editors == nil) then
          editors = FindEditors()
          showPlugin = 0
        end

        if(showPlugin >= #editors or safeMode == true) then
          -- If frames are over total sprites, we are done
          done = true
        else
          showPlugin = showPlugin + 1
          local key = _G[editors[showPlugin] .. "icon"];

          DrawSpriteBlock(key.spriteIDs[1], ((showPlugin - 1) * 2) + 1, 24, key.width, key.width, false, false, DrawMode.Tile)

        end

      end
    end
  end
end

function Draw()

  -- Redraw the entire display
  RedrawDisplay()

  -- Draw top border
  DrawSprites(topborder.spriteIDs, 0, 0, topborder.width)

  -- Draw bottom border
  DrawSprites(bottomborder.spriteIDs, 0, 232, bottomborder.width)

  -- Mask off the bottom of the screen so you can see the scrolling
  DrawRect(0, 240, 256, 8, 0, DrawMode.UI)
end

function KeyPressCheck()

  if(invalid == true) then
    return
  end

  if(Key(Keys.F)) then
    Fullscreen(not Fullscreen())
    InvalidateKeys()
  else
    if(Key(Keys.D1)) then
      Scale(1)
      InvalidateKeys()
    elseif(Key(Keys.D2)) then
      Scale(2)
      InvalidateKeys()
    elseif(Key(Keys.D3)) then
      Scale(3)
      InvalidateKeys()
    elseif(Key(Keys.D4)) then
      Scale(4)
      InvalidateKeys()
    elseif(Key(Keys.LeftShift) or Key(Keys.RightShift)) then
      if(safeMode == false) then
        safeMode = true
        DrawText("SAFE MODE", 8, 225, DrawMode.TilemapCache, "small", 11, - 4)
        
        -- Reset the display
        Fullscreen(false)
        EnableCRT(false)
        Scale(1)

        --print("Safe mode")
      end
    end
  end

end

function InvalidateKeys()
  invalid = true
end

function FindEditors()

  local editors = {}

  -- If the file system isn't exposed, exit out of this since we can't check for any installed tool
  if(PathExists == nil) then
    return {}
  end

  local paths = 
  {
    NewWorkspacePath("/PixelVisionOS/Tools/"),
  }

  if(PathExists(paths[1]) == nil) then
    return
  end

  local total = #paths

  local tools = {}

  for i = 1, total do

    local path = paths[i];

    if (PathExists(path)) then

      local folders = GetEntities(path);
      local total = 0

      for i = 1, #folders do

        local folder = folders[i]

        if (folder.IsDirectory) then

          local tmpInfoPath = folder.AppendFile("info.json")

          if(PathExists(tmpInfoPath)) then

            local jsonData = ReadJson(tmpInfoPath )

            if (jsonData["editType"] ~= nil) then
              --     {
              local split = string.split(jsonData["editType"], ",")
              --
              local totalTypes = #split
              for j = 1, totalTypes do

                if(_G[split[j] .. "icon"] ~= nil) then
                  table.insert(tools, split[j])
                end

              end
            end
          end
        end
      end
    end

  end

  table.sort(tools)

  return tools
end

string.split = function(string, delimiter)
  if delimiter == nil then
    delimiter = "%s"
  end
  local t = {} ; i = 1
  for str in string.gmatch(string, "([^"..delimiter.."]+)") do
    t[i] = str
    i = i + 1
  end
  return t
end
