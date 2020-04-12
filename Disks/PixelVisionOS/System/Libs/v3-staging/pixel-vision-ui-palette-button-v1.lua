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
--
function EditorUI:CreatePaletteButton(rect, spriteName, toolTip, colorOffset, totalColors)

  totalColors = totalColors or 16

  -- Create the button's default data
  local data = self:CreateButton(rect, spriteName, toolTip, false)

  data.name = "Palette" .. data.name

  -- Find the default sprite
  local defaultSprite = _G[spriteName]

  -- If the default sprite exists set up the states
  if(defaultSprite ~= nil) then

    -- Try to figure out a start offset based on if the colorOffset is a table
    data.colorOffset = (type(colorOffset) == "table") and

    (colorOffset.disabled ~= nil) and colorOffset.disabled or

    (colorOffset.up ~= nil) and colorOffset.up or 0

    or colorOffset

    -- Get the sprites
    local sprites = defaultSprite.spriteIDs

    -- Update the UI tile width and height
    data.tiles.width = defaultSprite.width
    data.tiles.height = math.floor(#sprites / defaultSprite.width)

    -- Update the rect width and height with the new sprite size
    data.rect.width = data.tiles.width * self.spriteSize.x
    data.rect.height = data.tiles.height * self.spriteSize.y

    -- Create a table to store the sprite state data
    data.cachedSpriteData = {}

    -- Get all of the possible button state labels
    local states = self.buttonStates

    -- Loop through all of the states and create data for each one
    for i = 1, #states do

      -- Get the current state
      local state = states[i]

      -- Create the button state data and calculate the offset
      data.cachedSpriteData[state] = {
        spriteIDs = sprites,
        width = data.tiles.width,
        -- This assumes that there is an up state if a table is supplied for the color offset
        colorOffset = (type(colorOffset) == "table" and colorOffset[state] ~= nil) and colorOffset[state] or data.colorOffset + (totalColors * (i - 1))
      }
    end

    -- Rebuild the draw argument tables
    data.spriteDrawArgs = {sprites, 0, 0, defaultSprite.width, false, false, DrawMode.Sprite, 0, false, false}
    data.tileDrawArgs = {sprites, data.rect.x, data.rect.y, defaultSprite.width, false, false, DrawMode.TilemapCache, 0}

    -- Invalidate the button
    self:Invalidate(data)

  end

  return data
  --
end

function EditorUI:CreateTogglePaletteButton(rect, spriteName, toolTip, colorOffset, totalColors)

  local data = self:CreatePaletteButton(rect, spriteName, toolTip, colorOffset, totalColors)

  data.name = "Toggle" .. data.name

  -- Add the selected property to make this a toggle button
  data.selected = false

  data.onClick = function(tmpData)

    -- Only trigger the click action when the last pressed button name matches
    if(self.currentButtonDown == tmpData.name) then
      self:ToggleButton(tmpData)
    end

  end

  return data

end
