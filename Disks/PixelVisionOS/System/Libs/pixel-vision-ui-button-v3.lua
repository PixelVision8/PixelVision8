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

function EditorUI:CreateButton(rect, spriteName, toolTip, forceDraw)

  -- Create the button's default data
  local data = self:CreateData(rect, spriteName, toolTip, forceDraw)

  data.doubleClick = false
  data.doubleClickTime = 0
  data.doubleClickDelay = .45
  data.doubleClickActive = false

  data.buttonCursor = 2

  -- By default, we don't want buttons to redraw the background
  data.redrawBackground = false
  data.bgColorOverride = nil

  -- Customize the default name by adding Button to it
  data.name = "Button" .. data.name

  -- Internal CallBacks (These can be re-mapped when needed)

  -- On click
  data.onClick = function(tmpData)

    -- Only trigger the click action when the last pressed button name matches
    if(self.inFocusUI ~= nil and self.inFocusUI.name == tmpData.name) then
      self:ClickButton(tmpData, true, tmpData.doubleClickActive and tmpData.doubleClickTime < tmpData.doubleClickDelay)

      -- self.inFocusUI = nil
      tmpData.doubleClickTime = 0
      tmpData.doubleClickActive = true
      tmpData.doubleClick = true
    end
    
  end

  -- On First Press (Called when the button)
  data.onFirstPress = function(tmpData)

    -- Save the name of the button that was just pressed
    -- self.inFocusUI = tmpData

    self:PressButton(tmpData, true)
  end

  -- On Redraw
  data.onRedraw = function(tmpData)
    self:RedrawButton(tmpData)
  end

  -- Make sure the button correctly sizes itself based on the cached sprite data
  self:UpdateButtonSizeFromCache(data)

  return data

end

function EditorUI:UpdateButtonSizeFromCache(data)
  local spriteData = -1

  -- Get the default sprite data for the button
  if(data.cachedSpriteData ~= nil) then

    spriteData = FindMetaSpriteId(data.cachedSpriteData.up or data.cachedSpriteData.disabled)

  end

  -- Calculate rect and hit rect
  if(spriteData > -1) then

    local tmpMetaSprite = MetaSprite(spriteData)

    -- Update the UI tile width and height
    data.tiles.w = tmpMetaSprite.Bounds.Width/ SpriteSize().X
    data.tiles.h = tmpMetaSprite.Bounds.Height / SpriteSize().Y

    -- Update the rect width and height with the new sprite size
    data.rect.w = data.tiles.w * self.spriteSize.x
    data.rect.h = data.tiles.h * self.spriteSize.y

    -- Cache the tile draw arguments for rendering
    data.spriteDrawArgs = {spriteData, 0, 0, false, false, DrawMode.Sprite, 0, false, false}
    data.tileDrawArgs = {spriteData, data.rect.x, data.rect.y, false, false, DrawMode.TilemapCache, 0}
    data.bgDrawArgs = {data.rect.x, data.rect.y, data.rect.w, data.rect.h, BackgroundColor(), DrawMode.TilemapCache}

  end

end

function EditorUI:UpdateButton(data, hitRect)

  -- Make sure we have data to work with and the component isn't disabled, if not return out of the update method
  if(data == nil) then
    return
  end

  -- If the button has data but it's not enabled exit out of the update
  if(data.enabled == false) then

    -- If the button is disabled but still in focus we need to remove focus
    if(data.inFocus == true) then
      self:ClearFocus(data)
    end

    -- See if the button needs to be redrawn.
    data.onRedraw(data)
    -- Shouldn't update the button if its disabled
    return

  end

  -- Make sure we don't detect a collision if the mouse is down but not over this button
  if(self.collisionManager.mouseDown and data.inFocus == false) then
    -- See if the button needs to be redrawn.
    -- self:RedrawButton(data)
    data.onRedraw(data)
    return
  end

  -- If the hit rect hasn't been overridden, then use the buttons own hit rect
  if(hitRect == nil) then
    hitRect = data.hitRect or data.rect
  end

  -- Ready to test finer collision if needed
  if(self.collisionManager:MouseInRect(hitRect) == true or (data.inFocus == true and self.collisionManager.mouseDown)) then

    if(data.doubleClick == true) then

      -- If the button wasn't in focus before, reset the timer since it's about to get focus
      if(data.inFocus == false) then
        data.doubleClickTime = 0
        data.doubleClickActive = false
      end

      data.doubleClickTime = data.doubleClickTime + self.timeDelta
      if(data.doubleClickActive and data.doubleClickTime > data.doubleClickDelay) then
        data.doubleClickActive = false
      end
    end

    -- print(data.name, data.buttonCursor)
    -- If we are in the collision area, set the focus
    self:SetFocus(data, data.buttonCursor)

    -- calculate the correct button over state
    self.tmpState = self.collisionManager.mouseDown and "down" or "over"

    if(data.selected == true) then
      self.tmpState = "selected" .. self.tmpState
    end

    local spriteData = data.cachedSpriteData ~= nil and data.cachedSpriteData[self.tmpState] or nil

    if(spriteData ~= nil and data.spriteDrawArgs ~= nil) then

      -- Sprite Data
      data.spriteDrawArgs[1] = FindMetaSpriteId(spriteData)--.spriteIDs

      -- X pos
      data.spriteDrawArgs[2] = data.rect.x

      -- Y pos
      data.spriteDrawArgs[3] = data.rect.y

      -- Color Offset
      data.spriteDrawArgs[8] = spriteData.colorOffset or 0

      self:NewDraw("DrawMetaSprite", data.spriteDrawArgs)

    end

    -- Check to see if the button is pressed and has an onAction callback
    if(self.collisionManager.mouseReleased == true) then

      -- Click the button
      data.onClick(data)
      data.firstPress = true

    elseif(self.collisionManager.mouseDown) then

      if(data.firstPress ~= false) then

        -- Call the onPress method for the button
        data.onFirstPress(data)

        -- Change the flag so we don't trigger first press again
        data.firstPress = false
      end
    end

  else

    -- On Release Outside
    if(data.inFocus == true) then
      data.firstPress = true
      -- If we are not in the button's rect, clear the focus
      self:ClearFocus(data)
    end

  end

  data.onRedraw(data)

end

function EditorUI:RedrawButton(data, stateOverride)

  if(data == nil) then
    return
  end

  -- If the button changes state we need to redraw it to the tilemap
  if(data.invalid == true or stateOverride ~= nil) then

    -- The default state is up
    self.tmpState = "up"

    if(stateOverride ~= nil) then
      self.tmpState = stateOverride
    else

      -- If the button is selected, we will use the selected up state
      if(data.selected == true) then
        self.tmpState = "selected" .. self.tmpState
      end

      -- Test to see if the button is disabled. If there is a disabled sprite data, we'll change the state to disabled. By default, always use the up state.
      if(data.enabled == false and data.cachedSpriteData["disabled"] ~= nil and data.selected ~= true) then --_G[spriteName .. "disabled"] ~= nil) then
        self.tmpState = "disabled"
      end

    end

    -- Test to see if the sprite data exist before updating the tiles
    if(data.cachedSpriteData ~= nil and data.cachedSpriteData[self.tmpState] ~= nil and data.tileDrawArgs ~= nil) then

      -- Update the tile draw arguments
      data.tileDrawArgs[1] = FindMetaSpriteId(data.cachedSpriteData[self.tmpState])--.spriteIDs

      -- Color offset
      data.tileDrawArgs[8] = data.cachedSpriteData[self.tmpState].colorOffset or 0

      if(data.redrawBackground == true) then

        -- Make sure we always have the current BG color
        data.bgDrawArgs[5] = data.bgColorOverride ~= nil and data.bgColorOverride or BackgroundColor()

        self:NewDraw("DrawRect", data.bgDrawArgs)

      end

      self:NewDraw("DrawMetaSprite", data.tileDrawArgs)

    end

    self:ResetValidation(data)

  end

end

function EditorUI:ClearButton(data, flag)

  -- We want to clear the flag if no value is supplied
  flag = flag or - 1

  if(data.cachedSpriteData == nil) then
    return
  end

  -- Get the cached empty sprite data
  -- self.tmpSpriteData = data.cachedSpriteData["empty"]

  -- make sure we have sprite data to draw
  if(self.tmpSpriteData ~= nil) then

    -- Update the tile draw arguments
    data.tileDrawArgs[1] = FindMetaSpriteId(data.cachedSpriteData["empty"])--.spriteIDs

    -- Color offset
    data.tileDrawArgs[8] = self.tmpSpriteData.colorOffset or 0

    self:NewDraw("DrawMetaSprite", data.tileDrawArgs)

  end

end

-- Use this to perform a click action on a button. It's used internally when a mouse click is detected.
function EditorUI:ClickButton(data, callAction, doubleClick)

  if(data.onAction ~= nil and callAction ~= false) then

    -- Trigger the onAction call back and pass in the double click value if the button is set up to use it
    data.onAction(doubleClick)

  end

end

function EditorUI:PressButton(data, callAction)

  if(data.onPress ~= nil and callAction ~= false) then

    -- Trigger the onPress
    data.onPress()

  end

end

function EditorUI:SelectButton(data, value)
  data.selected = value
  self:Invalidate(data)
end
