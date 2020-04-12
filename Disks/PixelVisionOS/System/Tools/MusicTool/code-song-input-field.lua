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

function EditorUI:CreateSongInputField(rect, text, toolTip, pattern, font, forceDraw)

  -- If no hight is provided, simply make the height one row high
  if(rect.h == nil) then
    rect.h = self.spriteSize.y
  end

  local data = self:CreateInputField(rect, nil, toolTip, pattern, font, forceDraw)

  -- Create a unique name by removing the InputArea string from the data's name
  data.name = "SongInputField" .. data.name:sub(10, - 1)

  -- data.multiline = data.tiles.h > 1
  data.index = -1
  -- data.nextField = nil
  -- data.previousField = nil
  -- data.clearValue = ""
  -- data.clearOnEnter = false
  -- data.allowEmptyString = false
  -- data.forceCase = nil -- Accepts nil, upper and lower
  -- override key callbacks for input fields

  data.doubleClick = true
  data.doubleClickTime = 0
  data.doubleClickDelay = .45
  data.doubleClickActive = false

  data.onEdit = function(targetData, value)
    print("Editing song field", value)
    --   local select = false
    --   print("Edit sound field", value)
    --   if(value == false and targetData.editing == true) then
    --     print("Reselect", targetData.name)
    --   end
    --
    --   self:OnEditTextInputField(targetData, value)
    --
    --
    --
  end

  -- data.onTab = function(data)
  --
  --   -- TODO need to test to see if shift key is down to go back
  --
  --   -- Move over to the next input field
  --   if(data.nextField ~= nil) then
  --     self:EditTextEditor(data.previousField, true)
  --   end
  --
  -- end

  -- data.onUpArrow = function(data)
  --   self:InputAreaMoveCursorTo(data, 0, 0 )
  -- end
  --
  -- data.onDownArrow = function(data)
  --   self:InputAreaMoveCursorTo(data, data.width, 0 )
  -- end

  -- data.onReturn = function(data)
  --   self:EditTextEditor(data, false)
  -- end
  --
  -- data.onInsertChar = function(data, char)
  --   self:InputFieldInsertChar(data, char)
  -- end
  --
  -- -- We want to route the default text through ChangeInputField
  -- if(text ~= nil) then
  --   self:ChangeInputField(data, text)
  -- end

  return data

end

function EditorUI:UpdateSongInputField(data, dt)

  local overrideFocus = (data.inFocus == true and self.collisionManager.mouseDown)

  local doubleClickDelay = .45

  -- TODO this should be only happen when in focus
  local cx = self.collisionManager.mousePos.c - data.tiles.c
  local cy = self.collisionManager.mousePos.r - data.tiles.r

  -- Ready to test finer collision if needed
  if(self.collisionManager:MouseInRect(data.rect) == true or overrideFocus) then

    if(data.enabled == true) then


      -- If the button wasn't in focus before, reset the timer since it's about to get focus
      if(data.inFocus == false) then
        -- print(data.name, "Reset focus")
        data.doubleClickTime = 0
        data.doubleClickActive = false
      end

      data.doubleClickTime = data.doubleClickTime + self.timeDelta


      if(data.doubleClickActive and data.doubleClickTime > doubleClickDelay) then
        data.doubleClickActive = false
      end


      if(data.inFocus == false) then
        -- Set focus
        self:SetFocus(data, data.editing == true and 3 or 2)
      end

      self:TextEditorMouseMoved(data, cx, cy)

      if(self.collisionManager.mouseReleased == true and data.editing == false) then

        -- self:EditTextEditor(data, true)
        if(data.doubleClickActive and data.doubleClickTime < doubleClickDelay) then

          -- TODO should this be edited on double click?

          -- Make sure we are not already editing the input field
          -- if(data.editing == false) then
          --
          --   -- self:InputAreaMoveCursorToMousePos(data)
          --
          --   -- Enter edit mode
          --   self:EditTextEditor(data, true)
          --
          -- end

        else
          self:SelectSongInputField(data, true)

        end

        data.doubleClickTime = 0
        data.doubleClickActive = true

      end

    else

      -- If the mouse is not in the rect, clear the focus
      if(data.inFocus == true) then
        self:ClearFocus(data)
      end

    end

  else
    -- If the mouse isn't over the component clear the focus
    self:ClearFocus(data)

  end


  if(data.inFocus == true)then

    -- if(self.collisionManager.mouseDown == true) then
    --   self:TextEditorMousepressed(data, cx, cy, 0)
    --   -- end
    -- elseif(self.collisionManager.mouseReleased == true) then
    --   self:TextEditorMouseReleased(data)
    -- end

  elseif(data.editing == true and self.collisionManager.mouseDown == true) then
    print(data.name, "Stop editing")
    self:EditTextEditor(data, false)
  end

  if(data.editing == true) then
    --
    -- If the field has focus, capture the keyboard input
    if(self.editingField ~= nil and data.name == self.editingField.name and data.editing == true) then

      -- Clear key name flag
      local keyName = ""

      if(Key(Keys.LeftShift) or Key(Keys.RightShift)) then
        keyName = keyName .. "shift-"
      end

      if(Key(Keys.LeftControl) or Key(Keys.RightControl)) then
        keyName = keyName .. "ctrl-"
      end

      if(Key(Keys.Backspace)) then
        keyName = keyName .. "backspace"
      elseif(Key(Keys.Delete)) then
        keyName = keyName .. "delete"
      elseif(Key(Keys.Enter)) then
        keyName = keyName .. "return"
      elseif(Key(Keys.Home, InputState.Released)) then
        keyName = keyName .. "home"
      elseif(Key(Keys.End, InputState.Released)) then
        keyName = keyName .. "end"
        -- elseif(Key(Keys.PageUp, InputState.Released)) then
        --   keyName = keyName .. "pageup"
        -- elseif(Key(Keys.PageDown, InputState.Released)) then
        --   keyName = keyName .. "pagedown"
        -- elseif(Key(Keys.Tab, InputState.Released)) then
        --   keyName = keyName .. "tab"
      elseif(Key(Keys.Up)) then
        keyName = keyName .. "up"
      elseif(Key(Keys.Down)) then
        keyName = keyName .. "down"
      elseif(Key(Keys.Right)) then
        keyName = keyName .. "right"
      elseif(Key(Keys.Left)) then
        keyName = keyName .. "left"
      else
        --These keys should have an immediate trigger when released
        -- if(Key(Keys.I, InputState.Released)) then
        --   keyName = keyName .. "i"
        -- elseif(Key(Keys.K, InputState.Released)) then
        --   keyName = keyName .. "k"
        if(Key(Keys.X, InputState.Released)) then
          keyName = keyName .. "x"
        elseif(Key(Keys.C, InputState.Released)) then
          keyName = keyName .. "c"
        elseif(Key(Keys.V, InputState.Released)) then
          keyName = keyName .. "v"
        elseif(Key(Keys.A, InputState.Released)) then
          keyName = keyName .. "a"
        elseif(Key(Keys.Z, InputState.Released)) then
          keyName = keyName .. "z"
        elseif(Key(Keys.Y, InputState.Released)) then
          keyName = keyName .. "y"
          -- elseif(Key(Keys.F, InputState.Released)) then
          --   keyName = keyName .. "f"
        end
        data.lastKeyCounter = data.inputDelay + 1
      end

      -- Clear the current key if it was the same as the last frame's key
      if(data.lastKey ~= "" and data.lastKey == keyName) then

        data.lastKeyCounter = data.lastKeyCounter + dt

        if(data.lastKeyCounter < data.inputDelay) then
          -- Clear key flag
          keyName = ""

        else

          data.lastKey = ""
        end

      end

      -- Look to see if there is a key map
      if(data.keymap[keyName] ~= nil) then

        -- Trigger the key action mapping
        data.keymap[keyName](data)

        -- Save the last key action
        data.lastKey = keyName
        -- Reset the counter
        data.lastKeyCounter = 0
      end

      -- end

      -- We only want to insert text if there the ctrl key is not being pressed
      if(Key(Keys.LeftControl) == false and Key(Keys.RightControl) == false) then
        local lastInput = data.captureInput(data)

        if(lastInput ~= "") then
          self:TextEditorTextInput(data, lastInput)
        end
      end

      --Blink timer
      if not data.sxs then --If not selecting
        data.btimer = data.btimer + dt
        if data.btimer >= data.btime then
          data.btimer = 0--data.btimer % data.btime
          data.bflag = not data.bflag


          -- -- EditorUI:TextEditorDrawBlink(data)
          -- EditorUI:TextEditorDrawLine(data) --Redraw the current line
        end

        if(data.bflag == true) then
          self:TextEditorDrawBlink(data)
        end
        -- print("Blink", data.bflag)
      elseif data.sflag.y ~= 0 then -- if selecting with the mouse and scrolling up/down
        data.stimer = data.stimer + dt
        if data.stimer > data.stime then
          data.stimer = data.stimer % data.stime
          data.vy = data.vy + data.sflag.y
          if data.vy <= 0 then
            data.vy = 1
          elseif data.vy > #data.buffer then
            data.vy = #data.buffer
          end

          -- EditorUI:TextEditorDrawBuffer(data)
        end
      end
    end
  end

  -- Only redraw the line if the buffer isn't about to redraw
  if(data.invalidateBuffer == false) then
    self:TextEditorDrawLine(data)
  end

  -- Redraw the display
  self:TextEditorDrawBuffer(data)
end

-- function EditorUI:UpdateSongInputFieldOld(data)
--
--   -- Exit if there is no data to render the input field
--   if(data == nil) then
--     return
--   end
--
--   -- If the input field is disabled we need to see if it should be redrawn and then exit
--   if(data.enabled == false) then
--     self:DrawInputArea(data)
--     return
--   end
--
--   -- Do the first test to see if we are in the right area to detect a collision
--   -- if(self.collisionManager.hovered == data.flagID) then
--
--   local overrideFocus = (data.inFocus == true and self.collisionManager.mouseDown)
--
--   -- print("Inside Text", data.name)
--   -- Ready to test finer collision if needed
--   if(self.collisionManager:MouseInRect(data.rect) == true or overrideFocus) then
--
--     if(data.doubleClick == true) then
--
--       -- If the button wasn't in focus before, reset the timer since it's about to get focus
--       if(data.inFocus == false) then
--         -- print(data.name, "Reset focus")
--         data.doubleClickTime = 0
--         data.doubleClickActive = false
--       end
--
--       data.doubleClickTime = data.doubleClickTime + self.timeDelta
--       if(data.doubleClickActive and data.doubleClickTime > data.doubleClickDelay) then
--         data.doubleClickActive = false
--       end
--
--       -- print(data.name, "data.doubleClickTime", data.doubleClickTime)
--     end
--
--     self:SetFocus(data, data.editing == true and 3 or 2)
--
--     if(self.collisionManager.mouseReleased == true) then
--
--       -- Check for double click before editing
--       if(data.doubleClickActive and data.doubleClickTime < data.doubleClickDelay) then
--
--         -- Make sure we are not already editing the input field
--         if(data.editing == false) then
--
--           self:InputAreaMoveCursorToMousePos(data)
--
--           -- Enter edit mode
--           self:EditTextEditor(data, true)
--
--         end
--
--       else
--         self:SelectSongInputField(data, true)
--
--       end
--
--       data.doubleClickTime = 0
--       data.doubleClickActive = true
--
--     end
--
--   else
--
--     -- If the mouse is not in the rect, clear the focus
--     if(data.inFocus == true) then
--       self:ClearFocus(data)
--     end
--
--   end
--
--   -- else
--   --   -- If the mouse isn't over the component clear the focus
--   --   -- self:ClearFocus(data)
--   --
--   -- end
--
--   if(data.editing == true) then
--
--     if(self.collisionManager.mouseReleased == true ) then
--
--       if(data.inFocus == false)then
--         self:EditTextEditor(data, false)
--       else
--         -- Update the mouse cursor
--         self:InputAreaMoveCursorToMousePos(data)
--       end
--
--     else
--
--       local lastInput = data.captureInput()
--
--       if(lastInput ~= "") then
--
--         self:InputAreaOnInput(data, lastInput)
--
--       end
--
--       self:InputAreaKeyCapture(data)
--
--       -- if we are in edit mode, we need to update the cursor blink time
--       data.blinkTime = data.blinkTime + self.timeDelta
--
--       if(data.blinkTime > data.blinkDelay) then
--         data.blinkTime = 0
--         data.blink = not data.blink
--
--       end
--
--     end
--
--   end
--
--   if(data.editing == true) then
--     data.colorOffset = 30
--   end
--
--   self:DrawInputArea(data)
--
-- end

function EditorUI:SelectSongInputField(data, value, triggerAction)

  -- print(data.name, "selected", value)
  data.selected = value

  -- data.colorOffset = data.selected and 30 or 28

  -- self:TextEditorCursorColor(data, 0)

  -- self:TextEditorInvalidateBuffer(data)

  if(value == true) then
    self:TextEditorSelectAll(data)
  else
    self:TextEditorDeselect(data)
  end

  if(data.onSelected ~= nil and value == true and triggerAction ~= false) then
    data.onSelected(data.index)
  end
end
