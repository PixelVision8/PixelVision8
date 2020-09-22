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
-- Christer Kaitila - @McFunkypants
-- Pedro Medeiros - @saint11
-- Shawn Rakowski - @shwany
--

function EditorUI:CreateIconButton(rect, spriteName, label, toolTip, bgColor)

  -- TODO Create custom button states?

  -- Use the same data as the button
  local data = self:CreateButton(rect, spriteName, toolTip)

  data.name = "Icon" .. data.name

  -- Add the selected property to make this a toggle button
  data.selected = false
  data.open = false
  data.redrawBackground = true
  -- Enable the button's doubleClick property
  data.doubleClick = true
  data.open = false

  data.tilePixelArgs = {nil, data.rect.x, data.rect.y, 48, 48, DrawMode.TilemapCache, 0}

  data.onClick = function(tmpData)

    if(self.currentButtonDown == tmpData.name) then
      -- Toggle the button's action
      self:ToggleIconButton(tmpData)

    end

  end

  data.onRedraw = function(tmpData)
    self:RedrawIconButton(tmpData)
  end

  local bgColor = bgColor or BackgroundColor()

  data.bgDrawArgs = {data.rect.x, data.rect.y, data.rect.w, data.rect.h, bgColor, DrawMode.TilemapCache}


  self:CreateIconButtonStates(data, spriteName, label)

  -- Modify the hit rect to the new rect position
  data.hitRect = {x = data.rect.x + 12, y = data.rect.y, w = data.rect.w, h = data.rect.h}

  -- TODO the size isn't correct since the icon sits inside of the 48x40 pixel area

  return data

end

function EditorUI:CreateIconButtonStates(data, spriteName, text)

  local size = NewPoint(48, 40)
  data.cachedPixelData = {}

  -- TODO need to resize the button to match the correct dimensions

  -- TODO need to create a new state for dragging with transparent background

  local states = {"up", "over", "openup", "selectedup", "disabled", "dragging"}--, "selectedup", "openup"}

  for i = 1, #states do

    local state = states[i]
    local canvas = NewCanvas(size.x, size.y)

    -- Change the sprite state to accommodate for the fact that there is no dragging sprite
    local spriteState = state == "dragging" and "up" or state

    if(state == "over") then
      -- print("OVER")
      state = "over"
      spriteState = "selectedup"
    end
    -- Clear the canvas to the default background color
    canvas:Clear(-1)

    -- Get the background color
    local bgColor = state ! = "dragging" and data.bgDrawArgs[5] or - 1

    -- Set the stroke and pattern to clear any previous icon this draws over
    canvas:SetStroke({bgColor}, 1, 1)
    canvas:SetPattern({bgColor}, 1, 1)

    local offset = 12

    -- Clear icon background
    canvas:DrawSquare(offset, 0, 24 + offset, 24, true)

    -- Create states
    if(spriteName == nil) then
      spriteName = "fileunknown"
    end

    local spriteData = _G[spriteName .. spriteState]
    -- print("state", state, spriteState, spriteName, states[i])
    if(spriteData ~= nil) then
      -- print("spriteName", spriteName, spriteState, spriteData ~= nil)

      local tmpX = math.floor((size.x - spriteData.width * 8) * .5)

      canvas:DrawSprites(spriteData.spriteIDs, tmpX, 0, spriteData.width)

      -- TODO need to manually split the text

      local lines = {""}
      local counter = 0
      local maxWidth = 11
      local maxChars = maxWidth * 2

      if(#text > maxChars) then
        text = text:sub(0, maxChars - 3) .. "..."
      end

      -- print("Total Chars", #text)
      for i = 1, #text do

        lines[#lines] = lines[#lines] .. text:sub(i, i):upper()

        if(i % maxWidth == 0 and i ~= #text)then
          table.insert(lines, "")
        end

      end

      -- Clear the area for the text
      canvas:DrawSquare(0, 24, size.x - 2, 24 + 14, true)

      for i = 1, #lines do

        -- Get the current line
        local line = lines[i]

        -- Calculate the centered text
        local x = (size.x - (#line * 4)) * .5

        -- Calculate the y position
        local y = ((i - 1) * 6) + 24

        local textColor = state == "up" and 0 or 15

        if(state == "dragging") then
          textColor = 15
        end

        if(state == "disabled") then
          textColor = 12
        end

        if(textColor == 15) then

          -- Set the background color to black since the text is white
          canvas:SetStroke({0}, 1, 1)
          canvas:SetPattern({0}, 1, 1)

          canvas:DrawSquare(x - 1, y + 1, (x ) + (#line * 4) - 1, (y) + 7, true)

        end
        -- Draw the text
        canvas:DrawText(line, x, y, "medium", textColor, - 4)

      end

      -- print("save", states[i])
      data.cachedPixelData[states[i]] = canvas

    end
  end

end

function EditorUI:UpdateIconButton(data, hitRect)

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
    -- self:RedrawButton(data)
    data.onRedraw(data)
    -- Shouldn't update the button if its disabled
    return

  end



  -- If the hit rect hasn't been overridden, then use the buttons own hit rect
  if(hitRect == nil) then
    hitRect = data.hitRect or data.rect
  end

  local overrideFocus = (data.inFocus == true and self.collisionManager.mouseDown)

  local collision = self.collisionManager:MouseInRect(hitRect)

  -- Make sure we don't detect a collision if the mouse is down but not over this button
  if(self.collisionManager.mouseDown and data.inFocus == false) then

    if(data.highlight == true and collision == false) then
      self:HighlightIconButton(data, false)
    end

    -- See if the button needs to be redrawn.
    data.onRedraw(data)
    return
  end

  -- Ready to test finer collision if needed
  if(collision or overrideFocus) then


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

    -- If we are in the collision area, set the focus
    self:SetFocus(data)

    self:Invalidate(data)

    -- calculate the correct button over state
    -- local state = self.collisionManager.mouseDown and "down" or "over"
    --
    -- if(data.selected == true) then
    --   state = "selected" .. state
    -- end
    --
    --
    --
    -- local spriteData = data.cachedSpriteData ~= nil and data.cachedSpriteData[state] or nil

    -- print(data.name, state, spriteData == nil, dump(data.cachedSpriteData))

    -- if(spriteData ~= nil and data.spriteDrawArgs ~= nil) then
    --
    --   -- Sprite Data
    --   data.spriteDrawArgs[1] = spriteData.spriteIDs
    --
    --   -- X pos
    --   data.spriteDrawArgs[2] = data.rect.x
    --
    --   -- Y pos
    --   data.spriteDrawArgs[3] = data.rect.y
    --
    --   -- Color Offset
    --   data.spriteDrawArgs[8] = spriteData.colorOffset or 0
    --
    --   -- self:NewDraw("DrawSprites", data.spriteDrawArgs)
    --
    -- end

    -- TODO need to make sure we only register a click when over the right button. If the mouse was down and rolls over then releases, that shouldn't trigger a click

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

    if(data.highlight) then
      self:HighlightIconButton(data, false)
      data.highlight = false
    end

    if(data.inFocus == true) then
      data.firstPress = true
      -- If we are not in the button's rect, clear the focus
      self:ClearFocus(data)

      self:Invalidate(data)

    end

  end

  -- else
  --
  --   -- If the mouse is not over the button, clear the focus for this button
  --   self:ClearFocus(data)
  --
  -- end

  -- Make sure we don't need to redraw the button.
  -- self:RedrawButton(data)
  data.onRedraw(data)

end

function EditorUI:RedrawIconButton(data)

  if(data == nil) then
    return
  end

  -- If the button changes state we need to redraw it to the tilemap
  if(data.invalid == true) then

    -- The default state is up
    local state = "up"

    -- If the button is selected, we will use the selected up state
    if(data.selected == true) then
      state = "selected" .. state
    end

    if(data.highlight) then
      state = "over"
    end
    -- if(state == "over") then
    --   state = "down"
    -- end

    -- print("State", state)

    -- Test to see if the button is disabled. If there is a disabled sprite data, we'll change the state to disabled. By default, always use the up state.
    if(data.enabled == false and data.cachedPixelData["disabled"] ~= nil and data.selected ~= true) then --_G[spriteName .. "disabled"] ~= nil) then
      state = "disabled"
    end

    if(data.open == true) then
      state = "openup"
    end

    -- print("Draw", data.name, state, data.cachedPixelData[state] ~= nil)

    -- Test to see if the sprite data exist before updating the tiles
    if(data.cachedPixelData ~= nil and data.cachedPixelData[state] ~= nil and data.tilePixelArgs ~= nil) then

      -- Update the tile draw arguments
      data.tilePixelArgs[1] = data.cachedPixelData[state]

      -- self:NewDraw("DrawPixels", data.cachedPixelData)

      data.cachedPixelData[state]:DrawPixels(data.rect.x, data.rect.y)
      -- canvas:DrawPixels(50, 50)

    end

    self:ResetValidation(data)

  end

end

function EditorUI:ToggleIconButton(data, value, callAction)

  -- Check to see if the icon was double clicked
  local doubleClick = data.doubleClickActive and data.doubleClickTime < data.doubleClickDelay

  -- Reset the double click flags
  data.doubleClickTime = 0
  data.doubleClickActive = true

  -- print("Double Click", doubleClick, data.selected)

  -- Call the button data's onAction method and pass the current selected state
  if(data.selected == false)then

    if(value == nil) then
      value = not data.selected
    end

    -- invert the selected value
    data.selected = true

    -- Invalidate the button so it redraws
    self:Invalidate(data)

    if(data.onAction ~= nil and callAction ~= false) then
      data.onAction(data.selected, doubleClick)
    end

  elseif(doubleClick == true and data.onTrigger ~= nil and callAction ~= false) then


    data.onTrigger()

  end


end

function EditorUI:OpenIconButton(data)
  data.open = true
  self:Invalidate(data)
end

function EditorUI:CloseIconButton(data)
  data.open = false
  self:Invalidate(data)
end

function EditorUI:CreateIconGroup(singleSelection)

  singleSelection = singleSelection == nil and true or singleSelection

  local data = self:CreateData()

  data.name = "IconButtonGroup" .. data.name

  -- flagID = flag,
  -- x = x,
  -- y = y,
  data.buttons = {}
  data.currentSelection = 0
  data.onAction = nil
  data.invalid = false
  data.hovered = 0
  data.singleSelection = singleSelection
  data.dragOverTime = 0
  data.dragOverDelay = .3
  -- }

  data.drawIconArgs = {nil, 0, 0, 48, 40, false, false, DrawMode.UI}

  return data

end

-- Helper method that created a toggle button and adds it to the group
function EditorUI:NewIconGroupButton(data, rect, spriteName, label, toolTip, bgColor)

  -- Create a new toggle group button
  local buttonData = self:CreateIconButton(rect, spriteName, label, toolTip, bgColor)

  -- Add the new button to the toggle group
  self:IconGroupAddButton(data, buttonData)

  -- Return the button data
  return buttonData

end

function EditorUI:IconGroupAddButton(data, buttonData, id)

  -- When adding a new button, force it to redraw
  --data.invalid = forceDraw or true



  -- TODO need to replace with table insert
  -- Need to figure out where to put the button, if no id exists, find the last position in the buttons table
  id = id or #data.buttons + 1

  -- save the button data
  table.insert(data.buttons, id, buttonData)

  self.collisionManager:EnableDragging(buttonData, .5, data.name)

  buttonData.id = id

  -- Attach a new onAction to the button so it works within the group
  buttonData.onAction = function()


    -- if(doubleClick == true) then
    --   self:TriggerIconButton(data, id)
    -- else
    -- TODO need to enable double click here
    self:SelectIconButton(data, id)
    -- end

    -- TODO restore icon if dragging is complete
    -- if(buttonData.dragging == true) then
    -- print("Redraw", buttonData.name, "enabled")

    -- Clear
    buttonData.dragging = false

    -- end

    -- End the drag
    if(data.onEndDrag ~= nil) then
      data.onEndDrag(data)
    end

  end

  buttonData.onTrigger = function()

    self:TriggerIconButton(data, id)
  end


  buttonData.onPress = function(value)

    -- TODO there should be a delay
    if(buttonData.onStartDrag ~= nil) then


      if(buttonData.selected == false) then
        self:SelectIconButton(data, id)
      end

      data.draggingSrc = buttonData.iconPath
      data.draggingPixelData = buttonData.cachedPixelData["dragging"]
      data.dragTarget = {path = buttonData.iconPath, pxielData = buttonData.cachedPixelData["dragging"]}

      buttonData.onStartDrag(buttonData)
    end

  end

  -- Invalidate the button so it redraws
  self:Invalidate(buttonData)

end

function EditorUI:TriggerIconButton(data, id)
  --
  -- print("Trigger Icon Button", id)

  if(data.onTrigger ~= nil) then
    data.onTrigger(id)
  end

end


function EditorUI:IconGroupRemoveButton(data, id)

  self:ToggleGroupRemoveButton(data, id)

end

function EditorUI:UpdateIconGroup(data)

  -- Exit the update if there is no is no data
  if(data == nil) then
    return
  end

  -- Set data for the total number of buttons for the loop
  local total = #data.buttons
  local btn = nil

  -- Loop through each of the buttons and update them
  for i = 1, total do

    btn = data.buttons[i]

    -- TODO not sure why this would ever be nil
    if(btn ~= nil) then
      if(btn.dragging == true) then

        -- Look to see what the icon is over and if we should trigger it
        -- if(source.dragging == true) then

        -- print("Collision Manager", source.name, "End Drag", "Targets", #self.dragTargets)

        -- Look for drop targets
        for i = 1, #self.collisionManager.dragTargets do

          local dest = self.collisionManager.dragTargets[i]

          -- Only find drop targets not equal to the source
          -- if(dest.name ~= source.name) then

          -- Look for a collision with the dest
          if(self.collisionManager:MouseInRect(dest.hitRect ~= nil and dest.hitRect or dest.rect)) then


            -- TODO there should be a timer before this is actually triggered
            if(dest.onOverDropTarget ~= nil) then
              -- print("Over icon")

              if(data.dragOverIconButton == nil or data.dragOverIconButton.name ~= dest.name) then


                data.dragOverIconButton = dest
                data.dragOverTime = 0
              end

              if(data.dragOverTime > - 1) then
                data.dragOverTime = data.dragOverTime + self.timeDelta
              end

              -- print("Over timer", data.dragOverTime)

              if(data.dragOverTime ~= -1 and data.dragOverTime >= .2) then

                -- data.dragOverTime = -1
                -- TODO need to figure out a way to trigger this item since it's over it

                -- print(source.name, "Drop On", dest.name)
                dest.onOverDropTarget(btn, dest)
                --
              end

              break

            end

          end
          -- end

        end

        if(self.collisionManager.mousePos.x > - 1 and self.collisionManager.mousePos.y > - 1) then

          local mouseOffset = {x = 24, y = 12}

          local clipSize = {x = 0, y = 0, w = 48, h = 40}

          -- TODO need to save this so it's not being called every time
          -- Calculate mask
          local displaySize = Display()

          displaySize.x = displaySize.x - 1
          -- TODO think this is a bug in the display size, it shouldn't need to subtract 9, just 1 like the width
          displaySize.y = displaySize.y - 9
          -- displaySize.width = displaySize.width - 1
          -- displaySize.y = displaySize.y - 9

          if((self.collisionManager.mousePos.x + (clipSize.w / 2)) > displaySize.x) then
            clipSize.w = clipSize.w - ((self.collisionManager.mousePos.x + (clipSize.w / 2)) - displaySize.x)
          elseif((self.collisionManager.mousePos.x - (clipSize.w / 2)) < 0) then

            local tmp = clipSize.w - ((self.collisionManager.mousePos.x + (clipSize.w / 2))) + 1

            clipSize.x = tmp
            clipSize.w = clipSize.w - tmp

            mouseOffset.x = mouseOffset.x - clipSize.x

          end

          if((self.collisionManager.mousePos.y + (clipSize.h / 2)) > displaySize.y) then
            clipSize.h = clipSize.h - ((self.collisionManager.mousePos.y + (clipSize.h / 2)) - displaySize.y)
          elseif((self.collisionManager.mousePos.y - (clipSize.h / 2)) < 4) then
            -- clipSize.h = 0

            local tmp = clipSize.h - ((self.collisionManager.mousePos.y + (clipSize.h / 2))) + 3

            clipSize.y = tmp
            clipSize.h = clipSize.h - tmp

            mouseOffset.y = mouseOffset.y - clipSize.y

          end

          data.drawIconArgs[1] = btn.cachedPixelData["dragging"]:SamplePixels(clipSize.x, clipSize.y, clipSize.w, clipSize.h)
          data.drawIconArgs[2] = self.collisionManager.mousePos.x - mouseOffset.x
          data.drawIconArgs[3] = self.collisionManager.mousePos.y - mouseOffset.y
          data.drawIconArgs[4] = clipSize.w
          data.drawIconArgs[5] = clipSize.h

          -- DrawPixels(btn.cachedPixelData["up"], 0,0)
          self:NewDraw("DrawPixels", data.drawIconArgs)
        end

        -- TODO need to see if we are over another icon button and temporarily select it
        -- self:ClearIconGroupSelections(data)
        -- -- Automatically select any button we are dragging
        -- self:SelectIconButton(data, btn.id, false)

        -- end

        -- print("Dragging", btn.name, btn.dragging)

      end

    end
    -- if(btn.dragging == true) then
    --   data.dragTarget = btn
    --   -- print(btn.name, btn.dragging)
    -- end
    self:UpdateIconButton(btn)

  end

end

function EditorUI:SelectIconButton(data, id, trigger)
  -- TODO need to make sure we handle multiple selections vs one at a time
  -- Get the new button to select
  local buttonData = data.buttons[id]
  --print("Select", id, #data.buttons)

  -- Make sure there is button data and the button is not disabled
  if(buttonData == nil or buttonData.enabled == false)then
    return
  end

  -- if the button is already selected, just ignore the request
  if(id == buttonData.selected) then
    return
  end

  if(data.singleSelection == true) then
    -- Make sure that the button is selected before we disable it
    buttonData.selected = true
    -- self:Enable(buttonData, false)

  end

  -- Now it's time to restore the last button.
  if(data.currentSelection > 0) then

    -- Get the old button data
    buttonData = data.buttons[data.currentSelection]

    -- Make sure there is button data first, incase there wasn't a previous selection
    if(buttonData ~= nil) then

      if(data.singleSelection == true) then
        -- Reset the button's selection value to the group's disable selection value
        buttonData.selected = false

        -- Enable the button since it is no longer selected
        self:Enable(buttonData, true)

      end

    end

  end

  -- Set the current selection ID
  data.currentSelection = id

  -- Trigger the action for the selection
  if(data.onAction ~= nil and trigger ~= false) then
    data.onAction(id, buttonData.selected)
  end

end

function EditorUI:IconGroupCurrentSelection(data)

  return self:ToggleGroupCurrentSelection(data)

end

-- TODO is anything using this?
function EditorUI:ToggleIconGroupSelections(data)

  self:ToggleGroupSelections(data)

end

function EditorUI:ClearIconGroupSelections(data)
  self:ClearGroupSelections(data)
end

function EditorUI:ClearIconGroup(data)

  self:ClearToggleGroup(data)

end

function EditorUI:HighlightIconButton(data, value)

  if(data.highlight ~= value) then
    data.highlight = value
    self:Invalidate(data)
  end
  -- data.onRedraw(data)

end
