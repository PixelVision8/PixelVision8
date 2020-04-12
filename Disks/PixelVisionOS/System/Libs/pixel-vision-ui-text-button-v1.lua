--
-- Copyright (c) 2017, Jesse Freeman. All rights reserved.
--
-- Licensed under the Microsoft Public License (MS-PL) License.
-- See LICENSE file in the project root for full license information.
--
-- Contributors
-- --------------------------------------------------------
-- This is the official list of Pixel Vision 8 contributors:
--
-- Jesse Freeman - @JesseFreeman
-- Christina-Antoinette Neofotistou - @CastPixel
-- Christer Kaitila - @McFunkypants
-- Pedro Medeiros - @saint11
-- Shawn Rakowski - @shwany
--

function EditorUI:CreateTextButton(rect, text, toolTip, colorOffset)

  -- TODO need to do the color offset table test here
  colorOffset = colorOffset or 25

  -- TODO create the name of the button
  local tmpSpriteName = text .. "TextButton"

  -- TODO need to generate out the sprite for the button
  local buttonSprites = self:BuildTextButton(text)

  local metaSprite = RegisterMetaSprite(tmpSpriteName)

  -- TODO merge all sprites into a single sprite

  local width = #buttonSprites
  local total = width * 2

  local spriteData = {spriteIDs = {}, width = width - 1}

  local row = 1

  for i = 1, total do

    local pos = CalculatePosition( i - 1, width )

    if(pos.x < width) then

      metaSprite.AddSprite(buttonSprites[pos.x + 1].spriteIDs[pos.y + 1], pos.x * 4, pos.y * 8, false, false, colorOffset)

      -- print("index", pos.x + 1, pos.y + 1)
      -- table.insert(spriteData.spriteIDs, buttonSprites[pos.x + 1].spriteIDs[pos.y + 1])
    end

  end

  DrawMetaSprite( tmpSpriteName, 9, 32, false, false, DrawMode.TilemapCache)

  -- print("button", dump(spriteData))

  -- _G[tmpSpriteName] = spriteData

  -- print(tmpSpriteName, text, width, dump(buttonSprites))
  -- Create the button's default data
  local data = self:CreatePaletteButton(rect, tmpSpriteName, toolTip, 0, 16)

  -- data.drawAPI = "DrawMetaSprite"

  return data

  --
  -- data.doubleClick = false
  -- data.doubleClickTime = 0
  -- data.doubleClickDelay = .45
  -- data.doubleClickActive = false
  --
  -- -- By default, we don't want buttons to redraw the background
  -- data.redrawBackground = true
  -- data.bgColorOverride = 0
  --
  -- -- Customize the default name by adding Button to it
  -- data.name = "TextButton" .. data.name
  --
  -- -- Internal CallBacks (These can be re-mapped when needed)
  --
  -- -- On click
  -- data.onClick = function(tmpData)
  --
  --   -- Only trigger the click action when the last pressed button name matches
  --   if(self.currentButtonDown == tmpData.name) then
  --     self:ClickButton(tmpData, true, tmpData.doubleClickActive and tmpData.doubleClickTime < tmpData.doubleClickDelay)
  --
  --     tmpData.doubleClickTime = 0
  --     tmpData.doubleClickActive = true
  --     tmpData.doubleClick = true
  --   end
  -- end
  --
  -- -- On First Press (Called when the button)
  -- data.onFirstPress = function(tmpData)
  --
  --   -- Save the name of the button that was just pressed
  --   self.currentButtonDown = tmpData.name
  --
  --   self:PressButton(tmpData, true)
  -- end
  --
  -- -- On Redraw
  -- data.onRedraw = function(tmpData)
  --   self:RedrawTextButton(tmpData)
  -- end
  --
  -- local sprites = self:BuildTextButton(text)
  --
  -- -- Default color starts at 48
  -- data.colorOffset = colorOffset or self.theme.textButton
  -- local totalColors = 16
  --
  -- if(sprites ~= nil) then
  --
  --   -- Update the UI tile width and height
  --   data.tiles.width = #sprites / 2
  --   data.tiles.height = 2
  --
  --   -- Update the rect width and height with the new sprite size
  --   data.rect.width = data.tiles.width * self.spriteSize.x
  --   data.rect.height = data.tiles.height * self.spriteSize.y
  --
  --   -- Create a table to store the sprite state data
  --   data.cachedSpriteData = {}
  --
  --   -- Get all of the possible button state labels
  --   local states = self.buttonStates
  --
  --   -- Loop through all of the states and create data for each one
  --   for i = 1, #states do
  --
  --     -- Get the current state
  --     local state = states[i]
  --
  --     -- Create the button state data and calculate the offset
  --     data.cachedSpriteData[state] = {
  --       sprites = sprites,
  --       width = data.tiles.width,
  --       colorOffset = (type(data.colorOffset) == "table" and data.colorOffset[state] ~= nil) and data.colorOffset[state] or data.colorOffset + (totalColors * (i - 1))
  --     }
  --   end
  --
  --   spriteData = data.cachedSpriteData.up or data.cachedSpriteData.disabled
  --
  -- end
  --
  -- return data

end

function EditorUI:BuildTextButton(text)

  -- Default button sprites
  local sprites = {
    textbuttonleft,
    textbuttonright
  }

  local total = #text

  for i = 1, total do

    local spriteData = nil
    local char = string.sub(text, i, i)

    if(char == " ") then
      spriteData = textbuttonmiddle
    else
      spriteData = _G["textbutton" .. char:lower()]
    end

    if(spriteData ~= nil) then
      table.insert(sprites, #sprites, spriteData)
    end
  end

  return sprites

end

function EditorUI:DrawTextButton(data, sprites, drawMode, colorOffset, shiftX)

  local x = data.rect.x
  local y = data.rect.y

  shiftX = shiftX or 0

  local total = #sprites

  if(#sprites % 2 > 0) then
    x = x + shiftX
  end

  for i = 1, total do
    local spriteData = sprites[i]

    self:NewDraw("DrawSprites", {spriteData.spriteIDs, x + ((i - 1) * 4), y, spriteData.width, false, false, drawMode, colorOffset})

  end

end
