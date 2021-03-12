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

function EditorUI:CreateSlider(rect, spriteName, toolTip, horizontal, offset)

  -- Set up button states
  local spriteData = MetaSprite(FindMetaSpriteId(spriteName))-- _G[spriteName]

  -- Create a generic component data object
  local data = self:CreateData(rect, spriteName, toolTip, forceDraw)

  -- Add the name of the component type to the default data name value
  data.name = "Slider" .. data.name

  -- Configure extra data properties needed to run the slider component
  data.horizontal = horizontal
  data.size = horizontal and rect.w or rect.h
  data.value = 0
  data.handleX = 0
  data.handleY = 0
  data.handleSize = -1

  -- This is applied to the top when horizontal and the left when vertical
  data.offset = offset or 0

  -- If there is a sprite calculate the handle size
  if(data.cachedMetaSpriteIds["up"] > -1) then
    -- Determine if the slider is horizontal or vertical and use the correct sprite dimensions
    if(horizontal == true) then
      data.handleSize = MetaSprite(data.cachedMetaSpriteIds["up"]).Width
    else
      data.handleSize = MetaSprite(data.cachedMetaSpriteIds["up"]).Height
    end
      data.size = data.size - data.handleSize
    -- end

    -- data.spriteDrawArgs = {FindMetaSpriteId(spriteName), 0, 0, false, false, DrawMode.Sprite, 0, false}

  end
  -- Need to account for the correct orientation
  data.handleCenter = data.handleSize / 2

  -- Return the data
  return data

end

function EditorUI:UpdateSlider(data)

  -- Make sure we have data to work with and the component isn't disabled, if not return out of the update method
  if(data == nil or data.handleSize < 0) then
    return
  end
  

  -- local size = data.size - data.handleSize

  if(data.enabled == true) then


    local overrideFocus = (data.inFocus == true and self.collisionManager.mouseDown)

    -- Ready to test finer collision if needed
    if(self.collisionManager:MouseInRect(data.rect) == true or overrideFocus) then

      if(self.inFocusUI == nil) then

        self:SetFocus(data)

      end

      -- Check to see if the mouse is down to update the handle position
      if(self.collisionManager.mouseDown == true and data.inFocus) then

        -- Calculate the position
        self:UpdateSliderPosition(data)

      end

    else

      -- If the mouse is not in the rect, clear the focus
      if(data.inFocus == true and self.collisionManager.mouseDown == false) then
        self:ClearFocus(data)
      end

    end

  end

  if(data.invalid == true) then

    -- If the mouse isn't on the slider, make sure it's position is correct
    data.handleX = data.rect.x
    data.handleY = data.rect.y

    if(data.horizontal == true) then
      data.handleX = data.handleX + (data.value * data.size)
      data.handleY = data.handleY + data.offset
    else
      data.handleX = data.handleX + data.offset
      data.handleY = data.handleY + (data.value * data.size)

    end

    -- Clear the validation
    self:ResetValidation(data)

  end

  if(data.enabled == true) then

    DrawMetaSprite(data.cachedMetaSpriteIds[(data.inFocus and "over" or "up")], data.handleX, data.handleY)--, false, false, DrawMode.Sprite, 0, false)
      
  end

  -- Return the slider data value
  return data.value

end

function EditorUI:UpdateSliderPosition(data)

  if(data.handleSize < 0) then
    return
  end

  -- local size = data.size - data.handleSize
  local dir = data.horizontal and "x" or "y"
  local prop = "handle" .. string.upper(dir)

  -- Need to calculate the new x position
  local newPos = self.collisionManager.mousePos[dir] - data.handleCenter

  if(newPos > - 1) then

    -- Make sure the position is in range
    if(newPos > data.size + data.rect[dir]) then
      newPos = data.size + data.rect[dir]
    elseif(newPos < data.rect[dir]) then
      newPos = data.rect[dir]
    end

    -- Save the new position
    data[prop] = newPos

    -- Need to calculate the value
    local percent = math.ceil(((data[prop] - data.rect[dir]) / data.size) * 100) / 100

    self:ChangeSlider(data, percent)

  end

end

function EditorUI:ChangeSlider(data, percent, trigger)

  -- If there is no data or the value is the same as what's being passed in, don't update the component
  if(data == nil or data.value == percent) then
    return
  end

  -- Set the new value
  data.value = percent

  -- TODO shouldn't this be onUpdate?
  if(data.onAction ~= nil and trigger ~= false) then
    data.onAction(percent)
  end

  -- Invalidate the component's display
  self:Invalidate(data)

end
