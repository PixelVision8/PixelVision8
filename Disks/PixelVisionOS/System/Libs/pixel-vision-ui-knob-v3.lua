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

function EditorUI:CreateKnob(rect, spriteName, toolTip)

  -- Create a generic component data object
  local data = self:CreateData(rect, spriteName, toolTip, forceDraw)

  data.rotation = 
  {
    "50",
    "60",
    "70",
    "80",
    "90",
    "100",
    "110",
    "120",
    "130",
    "140",
    "150",
    "160",
    "170",
    "180",
    "190",
    "200",
    "210",
    "220",
    "230",
    "240",
    "250",
    "260",
    "270",
    "280",
    "290",
    "300",
    "310",
    "320",
  }

  -- Add the name of the component type to the default data name value
  data.name = "Knob" .. data.name

  -- Configure extra data properties needed to run the slider component
  data.horizontal = true
  data.size = rect.w or rect.h
  data.value = 0
  data.handleX = 0
  data.handleY = 0
  data.handleSize = 1

  data.colorOffsetDisabled = 144

  data.colorOffsetUp = data.colorOffsetDisabled + 4
  data.colorOffsetOver = data.colorOffsetDisabled + 8
  

  -- This is applied to the top when horizontal and the left when vertical
  -- data.offset = offset or 0


  -- Calculate the handle's size based on the sprite
  -- local spriteData = _G[data.spriteName .. data.rotation[1]]

  -- local metaSpriteId = 

  -- if(spriteData ~= nil) then

  data.spriteDrawArgs = {FindMetaSpriteId(data.spriteName .. data.rotation[1]), data.rect.x, data.rect.y, false, false, DrawMode.TilemapCache, 0, false}

  -- end

  -- Need to account for the correct orientation
  data.handleCenter = data.handleSize / 2

  -- Create a custom hit rect for the knob
  data.hitRect = {x = data.rect.x + 2, y = data.rect.y + 2, w = 18, h = 18}

  -- Return the data
  return data

end

function EditorUI:UpdateKnob(data, hitRect)

  -- Make sure we have data to work with and the component isn't disabled, if not return out of the update method
  if(data == nil) then
    return
  end

  local size = data.size - data.handleSize

  if(data.enabled == true) then

    local overrideFocus = (data.inFocus == true and self.collisionManager.mouseDown)

    if(hitRect == nil) then
      hitRect = data.hitRect or data.rect
    end

    -- Ready to test finer collision if needed
    if(self.collisionManager:MouseInRect(hitRect) == true or overrideFocus) then

      if(self.inFocusUI == nil) then

        self:SetFocus(data)

      end

      -- Check to see if the mouse is down to update the handle position
      if(self.collisionManager.mouseDown == true and data.inFocus) then

        -- Need to calculate the new x position
        local newPos = self.collisionManager.mousePos.x - data.handleCenter

        -- Make sure the position is in range
        if(newPos > size + hitRect.x) then
          newPos = size + hitRect.x
        elseif(newPos < hitRect.x) then
          newPos = hitRect.x
        end

        -- Save the new position
        data.handleX = newPos

        -- Need to calculate the value
        local percent = math.ceil(((data.handleX - hitRect.x) / size) * 100) / 100

        self:ChangeKnob(data, percent)

      else
        self:DrawKnobSprite(data)
      end

    else

      -- If the mouse is not in the rect, clear the focus
      if(data.inFocus == true and self.collisionManager.mouseDown == false) then
        self:ClearFocus(data)
        self:DrawKnobSprite(data)
      end

    end

  end

  -- If the component has changes and the mouse isn't over it, update the handle
  if(data.invalid == true) then

    data.handleX = data.handleX + (data.value * size)
    data.handleY = data.handleY

    self:DrawKnobSprite(data)

    -- Clear the validation
    self:ResetValidation(data)

  end

  -- Return the slider data value
  return data.value

end

function EditorUI:CalculateKnobRotationID(data)
  local rotationID = math.floor(data.value * #data.rotation)

  if(rotationID < 1) then
    rotationID = 1
  elseif(rotationID > #data.rotation) then
    rotationID = #data.rotation
  end

  return rotationID

end

function EditorUI:DrawKnobSprite(data, mode)

  -- local metaSpriteId = FindMetaSpriteId(data.spriteName .. data.rotation[self:CalculateKnobRotationID(data)])--_G[data.spriteName .. data.rotation[self:CalculateKnobRotationID(data)]] -- data.enabled == true

  -- Make sure we have sprite data to render
  if(data.spriteDrawArgs ~= nil) then

    -- Sprite Data
    data.spriteDrawArgs[1] = FindMetaSpriteId(data.spriteName .. data.rotation[self:CalculateKnobRotationID(data)])

    -- Color Offset
    if(data.enabled) then
      data.spriteDrawArgs[7] = data.inFocus == true and data.colorOffsetOver or data.colorOffsetUp
    else
      data.spriteDrawArgs[7] = data.colorOffsetDisabled
    end

    self:NewDraw("DrawMetaSprite", data.spriteDrawArgs)

  end

end

function EditorUI:ChangeKnob(data, percent, trigger)

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