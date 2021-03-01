LoadScript("code-highlighter")
LoadScript("lume")

function EditorUI:CreateTextEditor(rect, text, toolTip, font, colorOffset, spacing)

  local data = self:CreateData(rect, nil, toolTip)

  data.highlighterTheme = {
    text = 15, -- White
    selection = 0, -- Yellow,
    keyword = 14, -- 
    number = 6, -- 
    comment = 5, -- 
    string = 11, -- 
    api = 7, -- 
    callback = 9, -- 
    escape = 15, -- 
    disabled = 5, -- 
    selectionBackground = 11
  }

  -- Default language is lua
  -- data.language = "lua"
  
  data.lastCX = 0

  data.editable = true
  data.endOfLineOffset = 1
  data.viewPort = NewRect(data.rect.x, data.rect.y, data.rect.w, data.rect.h)
  data.cursorPos = {x = 0, y = 0, color = 0}
  data.inputDelay = .10
  data.flavorBack = 0
  data.theme = {
    bg = 0, --Background Color
    cursor = 0, --Cursor Color
    cursorBG = 15 --Cursor BG Color
  }
  data.cx, data.cy = 1, 1 --Cursor Position
  -- data.charSize.x = self.spriteSize.x
  -- data.charSize.y = self.spriteSize.y --The font character size

  data.vx, data.vy = 1, 1 --View postions

  data.mflag = false --Mouse flag

  data.btimer = 0 --The cursor blink timer
  data.btime = 0.5 --The cursor blink time
  data.bflag = true --The cursor is blinking atm ?

  data.stimer = 0 -- The scroll timer when the mouse is dragging up
  data.stime = 0.1 -- The speed of up scrolling when the mouse is dragging up
  data.sflag = {x = 0, y = 0} -- Vector for scroll. 0 for no scroll, 1 for scroll down, -1 for scroll up.

  data.undoStack = {} -- Keep a stack of undo info, each one is {data, state}
  data.redoStack = {} -- Keep a stack of redo info, each one is {data, state}

  data.colorize = false --Color lua syntax
  data.autoDeselect = true

  data.buffer = {}
  
  data.invalidateLine = true
  data.invalidateBuffer = true
  data.invalidText = true
  data.lastKeyCounter = 0
  data.tabChar = "  "
  data.lastKey = ""

  -- Set up the draw arguments

  data.drawMode = DrawMode.TilemapCache
  data.colorOffset = colorOffset or 15
  data.spacing = spacing or 0
  data.font = font or "large"
  data.charSize = NewPoint(8 - math.abs(data.spacing), 8)

  -- Create input callbacks. These can be overridden to add special functionality to each input field
  data.captureInput = function()
    return InputString()
  end

  data.keymap = {
    ["return"] = function(targetData)
      if targetData.sxs then self:TextEditorDeleteSelection(targetData) end
      self:TextEditorInsertNewLine(targetData)
    end,

    ["left"] = function(targetData)
      self:TextEditorDeselect(targetData)
      local flag = false
      targetData.cx = targetData.cx - 1
      if targetData.cx < 1 then
        if targetData.cy > 1 then
          targetData.cy = targetData.cy - 1
          targetData.cx = targetData.buffer[targetData.cy]:len() + 1
          flag = true
        end
      end
      self:TextEditorResetCursorBlink(targetData)
      self:TextEditorCheckPosition(targetData)-- or flag then
      -- self:TextEditorDrawLineNum(targetData)
    end,

    ["right"] = function(targetData)
      self:TextEditorDeselect(targetData)
      local flag = false
      targetData.cx = targetData.cx + 1
      if targetData.cx > targetData.buffer[targetData.cy]:len() + 1 then
        if targetData.buffer[targetData.cy + 1] then
          targetData.cy = targetData.cy + 1
          targetData.cx = 1
          flag = true
        end
      end
      self:TextEditorResetCursorBlink(targetData)
      self:TextEditorCheckPosition(targetData)-- or flag then
      -- self:TextEditorDrawLineNum(targetData)
    end,
    ["shift-up"] = function(targetData)
      --in case we want to reduce shift selection
      if targetData.cy == 1 then
        --we stay in buffer
        return
      end
      if targetData.sxs then
        --there is an existing selection to update
        targetData.cy = targetData.cy - 1
        self:TextEditorCheckPosition(targetData)
        targetData.sye = targetData.cy
        targetData.sxe = math.min(targetData.cx, #targetData.buffer[targetData.cy])
      else
        targetData.sxs = targetData.cx
        targetData.sys = targetData.cy
        targetData.cy = targetData.cy - 1
        self:TextEditorCheckPosition(targetData)
        targetData.sye = targetData.cy
        targetData.sxe = math.min(targetData.cx, #targetData.buffer[targetData.cy])
      end
      self:TextEditorInvalidateBuffer(targetData)
      -- self:TextEditorDrawLineNum(targetData)
    end,

    -- ["shift-ctrl-f"] = function(targetData)
    --   self:TextEditorSearchPreviousFunction(targetData)
    -- end,

    -- ["ctrl-f"] = function(targetData)
    --   self:TextEditorSearchNextFunction(targetData)
    -- end,

    ["shift-down"] = function(targetData)
      --last line check, we do not go further than buffer
      if #targetData.buffer == targetData.cy then
        return
      end

      if targetData.sxs then
        targetData.cy = targetData.cy + 1
        self:TextEditorCheckPosition(targetData)
        targetData.sye = targetData.cy
        targetData.sxe = math.min(targetData.cx, #targetData.buffer[targetData.cy])
      else
        targetData.sxs = targetData.cx
        targetData.sys = targetData.cy
        targetData.cy = targetData.cy + 1
        self:TextEditorCheckPosition(targetData)
        targetData.sye = targetData.cy
        targetData.sxe = math.min(targetData.cx, #targetData.buffer[targetData.cy])
      end
      self:TextEditorInvalidateBuffer(targetData)
      -- self:TextEditorDrawLineNum(targetData)
    end,
    ["shift-right"] = function(targetData)

      --last line check, we do not go further than buffer
      if #targetData.buffer == targetData.cy and targetData.cx == #targetData.buffer[targetData.cy] then
        return
      end
      local originalcx, originalcy = targetData.cx, targetData.cy
      targetData.cx = targetData.cx + 1

      if targetData.cx > targetData.buffer[targetData.cy]:len() + 1 then
        if targetData.buffer[targetData.cy + 1] then
          targetData.cy = targetData.cy + 1
          targetData.cx = 1
        end
      end
      self:TextEditorCheckPosition(targetData)

      if targetData.sxs then
        targetData.sye = targetData.cy
        targetData.sxe = math.min(targetData.cx, #targetData.buffer[targetData.cy])
      else
        targetData.sxs = originalcx
        targetData.sys = originalcy
        targetData.sye = targetData.cy
        targetData.sxe = math.min(targetData.cx, #targetData.buffer[targetData.cy])
      end

      self:TextEditorInvalidateBuffer(targetData)
      -- self:TextEditorDrawLineNum(targetData)
    end,

    ["shift-left"] = function(targetData)
      --last line check, we do not go further than buffer
      if 0 == targetData.cy and targetData.cx <= 1 then
        return
      end
      local originalcx, originalcy = targetData.cx, targetData.cy
      targetData.cx = targetData.cx - 1

      if targetData.cx < 1 then
        if targetData.cy > 1 then
          targetData.cy = targetData.cy - 1
          targetData.cx = targetData.buffer[targetData.cy]:len() + 1
        end
      end
      self:TextEditorCheckPosition(targetData)

      if targetData.sxs then
        targetData.sye = targetData.cy
        targetData.sxe = math.min(targetData.cx, #targetData.buffer[targetData.cy])
      else
        targetData.sxs = originalcx
        targetData.sys = originalcy
        targetData.sye = targetData.cy
        targetData.sxe = math.min(targetData.cx, #targetData.buffer[targetData.cy])
      end

      self:TextEditorInvalidateBuffer(targetData)
      -- self:TextEditorDrawLineNum(targetData)
    end,

    ["up"] = function(targetData)
      self:TextEditorDeselect(targetData)
      targetData.cy = targetData.cy - 1
      self:TextEditorResetCursorBlink(targetData)
      self:TextEditorCheckPosition(targetData)
      -- self:TextEditorDrawLineNum(targetData)
    end,

    ["down"] = function(targetData)
      self:TextEditorDeselect(targetData)
      targetData.cy = targetData.cy + 1
      self:TextEditorResetCursorBlink(targetData)
      self:TextEditorCheckPosition(targetData)
      -- self:TextEditorDrawLineNum(targetData)
    end,

    ["backspace"] = function(targetData)
      -- if targetData.readonly then _systemMessage("The file is readonly !", 1, 9, 4) return end
      if targetData.sxs then self:TextEditorDeleteSelection(targetData) return end
      if targetData.cx == 1 and targetData.cy == 1 then return end
      local lineChange
      targetData.cx, targetData.cy, lineChange = self:TextEditorDeleteCharAt(targetData, targetData.cx - 1, targetData.cy)
      self:TextEditorResetCursorBlink(targetData)
      self:TextEditorCheckPosition(targetData)-- or lineChange then self:TextEditorDrawBuffer(targetData) else self:TextEditorDrawLine(targetData) end
      -- self:TextEditorDrawLineNum(targetData)
    end,

    ["delete"] = function(targetData)
      -- if targetData.readonly then _systemMessage("The file is readonly !", 1, 9, 4) return end
      if targetData.sxs then self:TextEditorDeleteSelection(targetData) return end
      local lineChange
      targetData.cx, targetData.cy, lineChange = self:TextEditorDeleteCharAt(targetData, targetData.cx, targetData.cy)
      self:TextEditorResetCursorBlink(targetData)
      self:TextEditorCheckPosition(targetData)-- or lineChange then self:TextEditorDrawBuffer(targetData) else self:TextEditorDrawLine(targetData) end
      -- self:TextEditorDrawLineNum(targetData)
    end,

    ["home"] = function(targetData) self:TextEditorGotoLineStart(targetData) end,

    ["end"] = function(targetData) self:TextEditorGotoLineEnd(targetData) end,

    ["pageup"] = function(targetData)
      targetData.vy = targetData.vy - targetData.tiles.h
      targetData.cy = targetData.cy - targetData.tiles.h

      if targetData.vy < 1 then targetData.vy = 1 end
      if targetData.cy < 1 then targetData.cy = 1 end

      self:TextEditorResetCursorBlink(targetData)
      self:TextEditorInvalidateBuffer(targetData)
      -- self:TextEditorDrawBuffer(targetData)
      -- self:TextEditorDrawLineNum(targetData)
    end,

    ["pagedown"] = function(targetData)

      targetData.vy = targetData.vy + targetData.tiles.h
      targetData.cy = targetData.cy + targetData.tiles.h

      local bottom = #targetData.buffer - targetData.tiles.h + 1

      if targetData.vy > bottom then targetData.vy = bottom end
      if targetData.cy > bottom then targetData.cy = bottom end

      self:TextEditorResetCursorBlink(targetData)
      self:TextEditorInvalidateBuffer(targetData)
      -- self:TextEditorDrawBuffer(targetData)
      -- self:TextEditorDrawLineNum(targetData)
    end,

    ["tab"] = function(targetData)
      self:TextEditorTextInput(targetData, targetData.tabChar)
    end,

    -- ["ctrl-k"] = function(targetData)
    --   if targetData.incsearch == true then
    --     self:TextEditorSearchTextAndNavigate(targetData, targetData.cy)
    --   end
    -- end,
    -- ["ctrl-x"] = function(targetData)
    --   if targetData.readonly then _systemMessage("The file is readonly !", 1, 9, 4) return end
    --   self:TextEditorCutText(targetData)
    -- end,

    -- ["ctrl-c"] = function(targetData) self:TextEditorCopyText(targetData) end,

    -- ["ctrl-v"] = function(targetData)
    --   if targetData.readonly then _systemMessage("The file is readonly !", 1, 9, 4) return end
    --   self:TextEditorPasteText(targetData)
    -- end,

    ["ctrl-a"] = function(targetData) self:TextEditorSelectAll(targetData) end,

    -- ["ctrl-z"] = function(targetData)
    --   -- if targetData.readonly then _systemMessage("The file is readonly !", 1, 9, 4) return end
    --   self:TextEditorUndo(targetData)
    -- end,

    -- ["shift-ctrl-z"] = function(targetData)
    --   -- if targetData.readonly then _systemMessage("The file is readonly !", 1, 9, 4) return end
    --   self:TextEditorRedo(targetData)
    -- end,

    -- ["ctrl-y"] = function(targetData)
    --   -- if targetData.readonly then _systemMessage("The file is readonly !", 1, 9, 4) return end
    --   self:TextEditorRedo(targetData)
    -- end
  }

  return data

end


function EditorUI:TextEditorMoveCursor(data, x, y, color)

  if(x ~= nil) then
    data.cursorPos.x = x
  end

  if(y ~= nil) then
    -- Offset this by 1 since the buffer is not 0 based
    data.cursorPos.y = y - 1
  end

  if(color ~= nil) then
    data.cursorPos.color = color
  end

  return data.cursorPos

end

function EditorUI:TextEditorCursorColor(data, value)
  data.cursorPos.color = value
end

function EditorUI:TextEditorDrawCharactersAtCursor(data, text, x, y)

  x = x or (data.cursorPos.x * data.charSize.X)
  y = y or (data.cursorPos.y * data.charSize.Y)

  local length = #text
  local tmpChar = ""

  for i = 1, length do

    data.cursorPos.x = data.cursorPos.x + 1

    tmpChar = text:sub(i, i)
    
    if(x > (data.viewPort.width - data.charSize.x)) then
      return
    end

    
    if(data.cursorPos.color == data.highlighterTheme.selection) then
        
      DrawRect( 
        data.rect.x + x,
        data.rect.y + y, 
        data.charSize.x, 
        data.charSize.y, 
        data.highlighterTheme.selectionBackground, 
        DrawMode.TilemapCache 
      )
    end
    
    if(x >= 0 and tmpChar ~= " ") then

      DrawText( 
        tmpChar,
        data.rect.x + x,
        data.rect.y + y,
        DrawMode.TilemapCache,
        data.font,
        data.enabled == true and data.cursorPos.color or data.highlighterTheme.disabled,
        data.spacing
      )
      
    end

    -- Increase X to the next character
    x = x + data.charSize.X

  end

end


--A usefull print function with color support !
function EditorUI:TextEditorDrawColoredTextAtCursor(data, tbl)
  if type(tbl) == "string" then
    self:TextEditorCursorColor(data, data.highlighterTheme.text)
    self:TextEditorDrawCharactersAtCursor(data, tbl)--, false, true)
  else
    for i = 1, #tbl, 2 do
      local col = tbl[i]
      local txt = tbl[i + 1]
      self:TextEditorCursorColor(data, col)
      self:TextEditorDrawCharactersAtCursor(data, txt)--, false, true)--Disable auto newline
    end
  end
end

--Check the position of the cursor so the view includes it
function EditorUI:TextEditorCheckPosition(data)
  local flag = false --Flag if the whole buffer requires redrawing

  -- Clamp the y position between 1 and the length of the buffer
  data.cy = Clamp(data.cy, 1, #data.buffer)

  if data.cy > data.tiles.h + data.vy - 1 then --Passed the screen to the bottom
    data.vy = data.cy - (data.tiles.h - 1); flag = true
  elseif data.cy < data.vy then --Passed the screen to the top
    if data.cy < 1 then data.cy = 1 end
    data.vy = data.cy; flag = true
  end

  --X position checking--
  if data.buffer[data.cy]:len() < data.cx - 1 then data.cx = data.buffer[data.cy]:len() + data.endOfLineOffset end --Passed the end of the line !

  data.cx = Clamp(data.cx, 1, data.buffer[data.cy]:len() + data.endOfLineOffset)

  if data.cx > data.tiles.w + (data.vx - 1) then --Passed the screen to the right
    data.vx = data.cx - (data.tiles.w - 1); flag = true
  elseif data.cx < data.vx then --Passed the screen to the left
    if data.cx < 1 then data.cx = 1 end
    data.vx = data.cx; flag = true
  end

  if(flag) then
    self:TextEditorInvalidateBuffer(data)

  end

  return flag
end

-- Make the cursor visible and reset the blink timer
function EditorUI:TextEditorResetCursorBlink(data)
  data.btimer = 0
  data.bflag = true
end

--Draw the cursor blink
function EditorUI:TextEditorDrawBlink(data)

  -- Don't blink if there is a selection or the cursor is off the screen
  if data.sxs or data.cx > (data.vx + (data.tiles.w - 1)) then return end

  if data.cy - data.vy < 0 or data.cy - data.vy > data.tiles.h then return end
  if data.bflag then

    local char = data.buffer[data.cy]:sub(data.cx, data.cx)

    -- Need to replace empty characters with a space
    if(char == "" or char == "\r") then
      char = " "
    end
    
    local tmpX = data.rect.x + (data.cx - data.vx) * (data.charSize.x)
    local tmpY = data.rect.y + (data.cy - data.vy) * (data.charSize.y)

    DrawRect( 
      tmpX,
      tmpY,
      data.charSize.X,
      data.charSize.Y,
      data.theme.cursorBG,
      DrawMode.Sprite
    )
    
    DrawText( 
      char,
      tmpX,
      tmpY,
      DrawMode.SpriteAbove,
      data.font,
      data.theme.cursor,
      data.spacing
    )
    
  end

end

--Draw the code on the screen
function EditorUI:TextEditorDrawBuffer(data)

  if data.invalidateBuffer == false then return end

  self:TextEditorResetBufferValidation(data)

  local vbuffer = lume.slice(data.buffer, data.vy, data.vy + data.tiles.h - 1) --Visible buffer
  local cbuffer = (data.colorize and highlighter ~= nil) and highlighter:highlightLines(vbuffer, data.vy) or vbuffer

  DrawRect( 
    data.rect.x,
    data.rect.y,
    data.rect.w,
    data.rect.h,
    data.theme.bg,
    DrawMode.TilemapCache
   )

  for k, l in ipairs(cbuffer) do

    -- Draw the line first for the background
    self:TextEditorMoveCursor(data, - (data.vx - 2) - 1, k, - 1)
    self:TextEditorDrawColoredTextAtCursor(data, l)

    local sxs, sys, sxe, sye = self:TextEditorGetOrderedSelect(data)

    if sxs and data.vy + k - 1 >= sys and data.vy + k - 1 <= sye then --Selection
      self:TextEditorMoveCursor(data, - (data.vx - 2) - 1, k, data.highlighterTheme.selection)
      local linelen, skip = vbuffer[k]:len(), 0

      if data.vy + k - 1 == sys then --Selection start
        skip = sxs - 1
        self:TextEditorMoveCursor(data, skip - (data.vx - 2) - 1)
        linelen = linelen - skip
      end

      if data.vy + k - 1 == sye then --Selection end
        linelen = sxe - skip
      end

      if data.vy + k - 1 < sye then --Not the end of the selection
        linelen = linelen + 1
      end

      -- Highlight start
      local hs = data.vx + data.cursorPos.x

      local he = hs + linelen - 1

      local char = data.buffer[data.vy + k - 1]:sub(hs, he)

      -- print("highlight", hs, he)
      if(char == "") then
        char = " "
      end

      -- DrawRect( hs, data.cursorPos.y * 8, he, 8, 1, DrawMode.TilemapCache )
      self:TextEditorDrawCharactersAtCursor(data, char)

    end
  end
end

function EditorUI:TextEditorDrawLine(data)

  -- If the line hasn't been invalidated don't render it
  if data.invalidateLine == false then return end

  self:TextEditorResetLineValidation(data)

  -- If there is a selection we want to draw the buffer instead of the line
  if(data.sxs) then

    self:TextEditorInvalidateBuffer(data)

    return

  end

  -- get the line's new width to see if it's larger than the max counter
  data.maxLineWidth = math.max(#data.buffer[data.cy], data.maxLineWidth)

  -- Reset validation
  if data.cy - data.vy < 0 or data.cy - data.vy > data.tiles.h - 1 then return end
  local cline, colateral
  if (data.colorize and highlighter ~= nil) then
    cline, colateral = highlighter:highlightLine(data.buffer[data.cy], data.cy)
  end
  if not cline then cline = data.buffer[data.cy] end

  local y = ((data.cy - data.vy + 1) * (data.charSize.y))

  DrawRect( 
    data.rect.x,
    data.rect.y + y - data.charSize.Y,
    data.rect.w,
    data.charSize.y,
    data.theme.bg,
    DrawMode.TilemapCache
  )
  
  self:TextEditorMoveCursor(data, - (data.vx - 2) - 1, y / data.charSize.Y, data.theme.bg)
  if not colateral then
    self:TextEditorDrawColoredTextAtCursor(data, cline)
  else
    self:TextEditorInvalidateBuffer(data)
  end

end

function EditorUI:TextEditorInvalidateLine(data)
  data.invalidateLine = true
end

function EditorUI:TextEditorResetLineValidation(data)
  data.invalidateLine = false
end

function EditorUI:TextEditorInvalidateBuffer(data)
  data.invalidateBuffer = true
end

function EditorUI:TextEditorResetBufferValidation(data)
  data.invalidateBuffer = false
end

function EditorUI:TextEditorInvalidateText(data)
  data.invalidText = true
end

function EditorUI:TextEditorResetTextValidation(data)
  data.invalidText = false
end

--Clear the selection just incase
function EditorUI:TextEditorDeselect(data)

  if data.sxs then
    --print(data.name, "Deselect")
    data.sxs, data.sys, data.sxe, data.sye = nil, nil, nil, nil
    self:TextEditorInvalidateBuffer(data)
  end
end

function EditorUI:TextEditorGetOrderedSelect(data)
  if data.sxs then
    if data.sye < data.sys then
      return data.sxe, data.sye, data.sxs, data.sys
    elseif data.sye == data.sys and data.sxe < data.sxs then
      return data.sxe, data.sys, data.sxs, data.sye
    else
      return data.sxs, data.sys, data.sxe, data.sye
    end
  else
    return false
  end
end

-- function EditorUI:TextEditorDrawLineNum(data)

--   local linestr = "LINE "..tostring(data.cy).."/"..tostring(#data.buffer).."  CHAR "..tostring(data.cx - 1).."/"..tostring(data.buffer[data.cy]:len())

-- end

-- function EditorUI:TextEditorSearchNextFunction(data)
--   for i, t in ipairs(data.buffer)
--   do
--     if i > data.cy then
--       if string.find(t, "function ") then
--         data.cy = i
--         self:TextEditorCheckPosition(data)
--         -- Force the buffer to redraw
--         self:TextEditorInvalidateBuffer(data)

--         break
--       end
--     end
--   end
-- end

-- function EditorUI:TextEditorSearchPreviousFunction(data)
--   highermatch = -1
--   for i, t in ipairs(data.buffer)
--   do
--     if i < data.cy then
--       if string.find(t, "function ") then
--         highermatch = i
--       end
--     end
--   end

--   if highermatch > - 1 then
--     data.cy = highermatch
--     data.vy = highermatch
--     self:TextEditorCheckPosition(data)
--     -- Force the buffer to redraw
--     self:TextEditorInvalidateBuffer(data)
--   end

-- end

-- function EditorUI:TextEditorSearchTextAndNavigate(data, from_line)
--   for i, t in ipairs(data.buffer)
--   do
--     if from_line ~= nil and i > from_line then
--       if string.find(t, data.searchtxt) then
--         data.cy = i
--         data.vy = i
--         self:TextEditorCheckPosition(data)
--         self:TextEditorInvalidateBuffer(data)

--         break
--       end
--     end
--   end

-- end

function EditorUI:TextEditorTextInput(data, t)

  if data.incsearch then
    if data.searchtxt == nil then data.searchtxt = "" end
    data.searchtxt = data.searchtxt..t
    -- note on -1 : that way if search is on line , still works
    -- and also ok for ctrl k
    self:TextEditorSearchTextAndNavigate(data, data.cy - 1)
    self:TextEditorDrawIncSearchState(data)
  else
    self:TextEditorBeginUndoable(data)
    local delsel
    if data.sxs then self:TextEditorDeleteSelection(data); delsel = true end
    data.buffer[data.cy] = data.buffer[data.cy]:sub(0, data.cx - 1)..t..data.buffer[data.cy]:sub(data.cx, - 1)

    data.cx = data.cx + t:len()

    -- Update the line length
    data.maxLineWidth = math.max(#data.buffer[data.cy], data.maxLineWidth)

    self:TextEditorResetCursorBlink(data)
    self:TextEditorInvalidateLine(data)
    self:TextEditorCheckPosition(data)
    -- self:TextEditorDrawLineNum(data)
    self:TextEditorEndUndoable(data)
    self:TextEditorInvalidateText(data)
  end
end

function EditorUI:TextEditorGotoLineStart(data)
  self:TextEditorDeselect(data)
  data.cx = 1
  self:TextEditorResetCursorBlink(data)
  if self:TextEditorCheckPosition(data) then self:TextEditorDrawBuffer(data) else self:TextEditorDrawLine(data) end

end

function EditorUI:TextEditorGotoLineEnd(data)
  self:TextEditorDeselect(data)
  data.cx = data.buffer[data.cy]:len() + 1
  self:TextEditorResetCursorBlink(data)
  if self:TextEditorCheckPosition(data) then self:TextEditorDrawBuffer(data) else self:TextEditorDrawLine(data) end

end

function EditorUI:TextEditorInsertNewLine(data)
  self:TextEditorBeginUndoable(data)
  local newLine = data.buffer[data.cy]:sub(data.cx, - 1)
  data.buffer[data.cy] = data.buffer[data.cy]:sub(0, data.cx - 1)
  local snum = string.find(data.buffer[data.cy].."a", "%S") --Number of spaces
  snum = snum and snum - 1 or 0
  newLine = string.rep(" ", snum)..newLine
  data.cx, data.cy = snum + 1, data.cy + 1
  if data.cy > #data.buffer then
    table.insert(data.buffer, newLine)
  else
    data.buffer = lume.concat(lume.slice(data.buffer, 0, data.cy - 1), {newLine}, lume.slice(data.buffer, data.cy, - 1)) --Insert between 2 different lines
  end

  self:TextEditorInvalidateBuffer(data)

  self:TextEditorResetCursorBlink(data)
  self:TextEditorCheckPosition(data)
  -- self:TextEditorDrawBuffer(data)
  -- self:TextEditorDrawLineNum(data)
  self:TextEditorEndUndoable(data)
  self:TextEditorInvalidateText(data)
end

-- Delete the char from the given coordinates.
-- If out of bounds, it'll merge the line with the previous or next as it suits
-- Returns the coordinates of the deleted character, adjusted if lines were changed
-- and a boolean "true" if other lines changed and redrawing the Buffer is needed
function EditorUI:TextEditorDeleteCharAt(data, x, y)
  self:TextEditorBeginUndoable(data)

  local lineChange = false
  -- adjust "y" if out of bounds, just as failsafe
  if y < 1 then y = 1 elseif y > #data.buffer then y = #data.buffer end
  -- newline before the start of line == newline at end of previous line
  if y > 1 and x < 1 then
    y = y - 1
    x = data.buffer[y]:len() + 1
  end
  -- join with next line (delete newline) when deleting past the boundaries of the line
  if x > data.buffer[y]:len() and y < #data.buffer then
    data.buffer[y] = data.buffer[y]..data.buffer[y + 1]
    data.buffer = lume.concat(lume.slice(data.buffer, 0, y), lume.slice(data.buffer, y + 2, - 1))
    lineChange = true
  else
    data.buffer[y] = data.buffer[y]:sub(0, x - 1) .. data.buffer[y]:sub(x + 1, - 1)
  end

  if lineChange then self:TextEditorInvalidateBuffer(data) else self:TextEditorInvalidateLine(data) end

  self:TextEditorEndUndoable(data)
  self:TextEditorInvalidateText(data)
  return x, y, lineChange
end

--Will delete the current selection
function EditorUI:TextEditorDeleteSelection(data)

  if not data.sxs then return end --If not selection just return back.
  local sxs, sys, sxe, sye = self:TextEditorGetOrderedSelect(data)

  self:TextEditorBeginUndoable(data)
  local lnum, slength = sys, sye + 1
  while lnum < slength do
    if lnum == sys and lnum == sye then --Single line selection
      data.buffer[lnum] = data.buffer[lnum]:sub(1, sxs - 1) .. data.buffer[lnum]:sub(sxe + 1, - 1)
      lnum = lnum + 1
    elseif lnum == sys then
      data.buffer[lnum] = data.buffer[lnum]:sub(1, sxs - 1)
      lnum = lnum + 1
    elseif lnum == slength - 1 then
      data.buffer[lnum - 1] = data.buffer[lnum - 1] .. data.buffer[lnum]:sub(sxe + 1, - 1)
      data.buffer = lume.concat(lume.slice(data.buffer, 1, lnum - 1), lume.slice(data.buffer, lnum + 1, - 1))
      slength = slength - 1
    else --Middle line
      data.buffer = lume.concat(lume.slice(data.buffer, 1, lnum - 1), lume.slice(data.buffer, lnum + 1, - 1))
      slength = slength - 1
    end
  end
  data.cx, data.cy = sxs, sys
  self:TextEditorCheckPosition(data)
  self:TextEditorDeselect(data)
  self:TextEditorInvalidateBuffer(data)
  self:TextEditorEndUndoable(data)
  self:TextEditorInvalidateText(data)
end

-- --Copy selection text (Only if selecting)
-- function EditorUI:TextEditorCopyText(data)
--   local sxs, sys, sxe, sye = self:TextEditorGetOrderedSelect(data)
--   if sxs then --If there are any selection
--     local clipbuffer = {}
--     for lnum = sys, sye do
--       local line = data.buffer[lnum]

--       if lnum == sys and lnum == sye then --Single line selection
--         line = line:sub(sxs, sxe)
--       elseif lnum == sys then
--         line = line:sub(sxs, - 1)
--       elseif lnum == sye then
--         line = line:sub(1, sxe)
--       end

--       table.insert(clipbuffer, line)
--     end

--     local clipdata = table.concat(clipbuffer, "\n")

--     self:TextEditorClipboard(clipdata)
--   end
-- end

-- --Cut selection text
-- function EditorUI:TextEditorCutText(data)
--   if data.sxs then
--     self:TextEditorCopyText(data)
--     self:TextEditorDeleteSelection(data)
--   end
-- end

-- -- Paste the text from the clipboard
-- function EditorUI:TextEditorPasteText(data)
--   self:TextEditorBeginUndoable(data)
--   if data.sxs then self:TextEditorDeleteSelection(data) end
--   local text = self:TextEditorClipboard()
  
--   if(text == nil) then
--     return
--   end
  
--   text = text:gsub("\t", " ") -- tabs mess up the layout, replace them with spaces
--   local firstLine = true
--   for line in string.gmatch(text.."\n", "([^\r\n]*)\r?\n") do
--     if not firstLine then
--       self:TextEditorInsertNewLine(data) data.cx = 1
--     else
--       firstLine = false
--     end
--     self:TextEditorTextInput(data, line)
--   end
--   if self:TextEditorCheckPosition(data) then self:TextEditorDrawBuffer(data) else self:TextEditorDrawLine(data) end

--   -- self:TextEditorDrawLineNum(data)
--   self:TextEditorEndUndoable(data)
--   self:TextEditorInvalidateText(data)
-- end

--Select all text
function EditorUI:TextEditorSelectAll(data)
  data.sxs, data.sys = 1, 1
  data.sye = #data.buffer
  data.sxe = data.buffer[data.sye]:len()
  self:TextEditorInvalidateBuffer(data)
end

-- Call :TextEditorBeginUndoable(data) right before doing any modification to the
-- text in the editor. It will capture the current state of the editor's
-- contents (data) and the state of the cursor, selection, etc. (state)
-- so it can be restored later.
-- NOTE: Make sure to balance each call to :TextEditorBeginUndoable(data) with a call
-- to :TextEditorEndUndoable(data). They can nest fine, just don't forget one.
function EditorUI:TextEditorBeginUndoable(data)
  if data.currentUndo then
    -- we have already stashed the data & state, just track how deep we are
    data.currentUndo.count = data.currentUndo.count + 1
  else
    -- make a new in-progress undo
    data.currentUndo = {
      count = 1, -- here is where we track nested begin/endUndoable calls
      data = self:TextEditorExport(data),
      state = self:TextEditorGetState(data)
    }
  end
end

-- Call :TextEditorEndUndoable(data) after each modification to the text in the editor.
function EditorUI:TextEditorEndUndoable(data)
  -- We might be inside several nested calls to begin/endUndoable
  data.currentUndo.count = data.currentUndo.count - 1
  -- If this was the last of the nesting
  if data.currentUndo.count == 0 then
    -- then push the undo onto the undo stack.
    table.insert(data.undoStack, {
      data.currentUndo.data,
      data.currentUndo.state
    })
    -- clear the redo stack
    data.redoStack = {}
    data.currentUndo = nil
  end
end

-- -- Perform an undo. This will pop one entry off the undo
-- -- stack and restore the editor's contents & cursor state.
-- function EditorUI:TextEditorUndo(data)
--   if #data.undoStack == 0 then
--     -- beep?
--     return
--   end
--   -- pull one entry from the undo stack
--   local text, state = unpack(table.remove(data.undoStack))

--   -- push a new entry onto the redo stack
--   table.insert(data.redoStack, {
--     self:TextEditorExport(data),
--     self:TextEditorGetState(data)
--   })

--   -- restore the editor contents
--   self:TextEditorImport(data, text)
--   -- restore the cursor state
--   self:TextEditorSetState(data, state)
-- end

-- -- Perform a redo. This will pop one entry off the redo
-- -- stack and restore the editor's contents & cursor state.
-- function EditorUI:TextEditorRedo(data)
--   if #data.redoStack == 0 then
--     -- beep?
--     return
--   end
--   -- pull one entry from the redo stack
--   local text, state = unpack(table.remove(data.redoStack))
--   -- push a new entry onto the undo stack
--   table.insert(data.undoStack, {
--     self:TextEditorExport(data),
--     self:TextEditorGetState(data)
--   })
--   -- restore the editor contents
--   self:TextEditorImport(data, text)
--   -- restore the cursor state
--   self:TextEditorSetState(data, state)
-- end

-- Get the state of the cursor, selection, etc.
-- This is used for the undo/redo feature.
function EditorUI:TextEditorGetState(data)
  return {
    cx = data.cx,
    cy = data.cy,
    sxs = data.sxs,
    sys = data.sys,
    sxe = data.sxe,
    sye = data.sye,
  }
end

-- Set the state of the cursor, selection, etc.
-- This is used for the undo/redo feature.
function EditorUI:TextEditorSetState(data, state)
  data.cx = state.cx
  data.cy = state.cy
  data.sxs = state.sxs
  data.sys = state.sys
  data.sxe = state.sxe
  data.sye = state.sye

  -- TODO need to invalidate line and buffer

  self:TextEditorCheckPosition(data)
  self:TextEditorDrawBuffer(data)
  -- self:TextEditorDrawLineNum(data)
end

-- Last used key, this should be set to the last keymap used from the data.keymap table

function EditorUI:TextEditorMousePressed(data, cx, cy)--, istouch)

  -- local cx, cy = self:TextEditorWhereInGrid(x, y, data.charGrid)
  if (not data.mflag and data.inFocus) then

    cx = data.vx + (cx)
    cy = data.vy + (cy)

    data.mflag = true

    data.cx = cx
    data.cy = cy
    --print("cursor", cx, cy, data.cx, data.cy, data.vx, data.vy)
    if data.sxs then data.sxs, data.sys, data.sxe, data.sye = false, false, false, false end --End selection

    self:TextEditorCheckPosition(data)


  end

end

function EditorUI:TextEditorMouseMoved(data, cx, cy)--, dx, dy, it)

  if not data.mflag then return end

  if(cx < 0 or cy < 0) then
    data.sflag.x = 0
    data.sflag.y = 0
    return
  end

  -- Adjust for the view scroll
  local cx2 = data.vx + (cx)
  local cy2 = data.vy + (cy)

  if(data.cx ~= cx2 or data.cy ~= cy2) then

    -- Save the cursor position
    data.cx = cx2
    data.cy = cy2

    -- Make sure cursor is in range
    self:TextEditorCheckPosition(data)

    -- Disable blink flag
    data.bflag = false --Disable blinking

    -- Start selection
    if not data.sxs then --Start the selection
      data.sxs, data.sys = data.cx, data.cy
      data.sxe, data.sye = data.cx, data.cy
      -- Note: the ordered selection is given by EditorUI:TextEditorGetOrderedSelect(data)
      -- This is used to avoid extra overhead.
    else

      data.sxe, data.sye = data.cx, data.cy

      if cx > data.tiles.w - 2 then
        data.sflag.x = 1
      elseif cx < 1 then
        data.sflag.x = -1
      else
        data.sflag.x = 0
      end

      -- TODO need to fix scroll calculation here
      if cy > data.tiles.h - 2 then
        data.sflag.y = 1
      elseif cy < 1 then
        data.sflag.y = -1
      else
        data.sflag.y = 0
      end

      self:TextEditorInvalidateBuffer(data)

    end




  end

  self:TextEditorCheckPosition(data)

end
-- end

function EditorUI:TextEditorMouseReleased(data)--, b, it)

  data.mflag = false
  data.sflag.x = 0
  data.sflag.y = 0

  if(data.sxs == data.sxe and data.sys == data.sye) then
    self:TextEditorDeselect(data)
  end

  self:TextEditorInvalidateBuffer(data)
  -- TODO need to figure out if there is only one character selected and disable the selection

end

function EditorUI:TextEditorWheelMoved(x, y)
  data.vy = math.floor(data.vy - y)
  if data.vy > #data.buffer then data.vy = #data.buffer end
  if data.vy < 1 then data.vy = 1 end
  data.vx = math.floor(data.vx + x)
  if data.vx < 1 then data.vx = 1 end
  self:TextEditorDrawBuffer(data)
end

function EditorUI:TextEditorUpdate(data, dt)

  -- Make sure we don't detect a collision if the mouse is down but not over this button
  if(self.collisionManager.mouseDown and data.inFocus == false) then

    -- If we lose focus while the mouse is down but still in edit mode we need to clear that
    if(data.editing == true) then
      self:EditTextEditor(data, false)
    end

    -- Force the display to redraw if it has been changed externally
    self:TextEditorDrawBuffer(data)

    return

  end

  local overrideFocus = (data.inFocus == true and self.collisionManager.mouseDown)

  -- TODO this should be only happen when in focus
  local cx = math.floor(self.collisionManager.mousePos.x/data.charSize.x) - data.tiles.c
  local cy = self.collisionManager.mousePos.r - data.tiles.r

  -- print(self.collisionManager.mousePos.x, data.charSize.x , (self.collisionManager.mousePos.x/data.charSize.x) * data.charSize.X)

  -- print("Inside Text", (editorUI.inFocusUI ~= nil and editorUI.inFocusUI.name or nil))
  -- Ready to test finer collision if needed
  if((self.collisionManager:MouseInRect(data.rect) == true or overrideFocus)) then

    if(data.enabled == true and data.editable == true) then

      if(data.inFocus == false) then
        -- print("in focus")
        -- Set focus
        self:SetFocus(data, 3)
      end



      if(self.collisionManager.mouseReleased == true and data.editing == false) then

        self:EditTextEditor(data, true)

      end

      self:TextEditorMouseMoved(data, cx, cy)

    else

      -- If the mouse is not in the rect, clear the focus
      if(data.inFocus == true) then
        self:ClearFocus(data)
      end

    end

  elseif(data.inFocus == true) then
    -- If the mouse isn't over the component clear the focus
    self:ClearFocus(data)

  end


  if(data.inFocus == true)then

    if(self.collisionManager.mouseDown == true) then
      self:TextEditorMousePressed(data, cx, cy, 0)
      -- end
    elseif(self.collisionManager.mouseReleased == true) then
      self:TextEditorMouseReleased(data)
    end

  elseif(data.editing == true and self.collisionManager.mouseDown == true) then
    --print(data.name, "Stop editing")
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

      if(Key(Keys.Back)) then
        keyName = keyName .. "backspace"
      elseif(Key(Keys.Delete)) then
        keyName = keyName .. "delete"
      elseif(Key(Keys.Enter)) then
        keyName = keyName .. "return"
      elseif(Key(Keys.Home, InputState.Released)) then
        keyName = keyName .. "home"
      elseif(Key(Keys.End, InputState.Released)) then
        keyName = keyName .. "end"
      elseif(Key(Keys.PageUp, InputState.Released)) then
        keyName = keyName .. "pageup"
      elseif(Key(Keys.PageDown, InputState.Released)) then
        keyName = keyName .. "pagedown"
      elseif(Key(Keys.Tab, InputState.Released)) then
        keyName = keyName .. "tab"
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
        if(Key(Keys.I, InputState.Released)) then
          keyName = keyName .. "i"
        elseif(Key(Keys.K, InputState.Released)) then
          keyName = keyName .. "k"
        elseif(Key(Keys.X, InputState.Released)) then
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
        elseif(Key(Keys.F, InputState.Released)) then
          keyName = keyName .. "f"
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

        end

        if(data.bflag == true) then
          self:TextEditorDrawBlink(data)
        end

      elseif data.sflag.x ~= 0 or data.sflag.y ~= 0 then -- if selecting with the mouse and scrolling up/down
        data.stimer = data.stimer + dt
        if data.stimer > data.stime then
          data.stimer = data.stimer % data.stime

          data.vx = Clamp(data.vx + data.sflag.x, 1, data.maxLineWidth - data.tiles.w)

          data.vy = Clamp(data.vy + data.sflag.y, 1, #data.buffer - data.tiles.h)

          -- EditorUI:TextEditorDrawBuffer(data)
        end
      end
    end
  end

  -- Only redraw the line if the buffer isn't about to redraw
  if(data.invalidateBuffer == false) then
    self:TextEditorDrawLine(data)
  end

  -- TODO this is sort of hacky, see if there is a better way to do this?
  -- Clear the flag just in case mouse is up but flag is true
  if(data.mflag == true and self.collisionManager.mouseDown == false) then
    data.mflag = false
    data.sflag.x = 0
    data.sflag.y = 0
  end

  -- Redraw the display
  self:TextEditorDrawBuffer(data)
end

function EditorUI:TextEditorMagicLines(s)
  if s:sub(-1) ~= "\n" then s = s.."\n" end
  return s:gmatch("([^\n]*)\n?") -- "([^\n]*)\n?"
end

function EditorUI:TextEditorImport(data, text)

  if(highlighter ~= nil and data.language ~= nil) then
    highlighter:setSyntax(data.language)
    highlighter:setTheme(data.highlighterTheme)
  end
  
  data.maxLineWidth = 0

  -- Create a new buffer
  data.buffer = {}

  -- Loop through each line of the text
  for line in self:TextEditorMagicLines(tostring(text)) do

    -- Replace tabs
    line = line:gsub("\t", data.tabChar)

    data.maxLineWidth = math.max(#line, data.maxLineWidth)

    -- Flag to add the new line
    local addLine = true

    -- Check to see if the maximum lines is set
    if(data.maxLines ~= nil) then

      -- Set the add line flag based on the current lines in the buffer
      addLine = #data.buffer < data.maxLines
    end

    -- Add the line
    if(addLine) then
      table.insert(data.buffer, line)
    end
  end

  if not data.buffer[1] then data.buffer[1] = "" end
  self:TextEditorCheckPosition(data)

  self:TextEditorResetTextValidation(data)
  self:TextEditorInvalidateBuffer(data)
end

function EditorUI:TextEditorExport(data)
  return table.concat(data.buffer, "\n")
end

function EditorUI:EditTextEditor(data, value, callAction)

  if(data.enabled == false or value == data.editing)then
    return
  end

  if(data.onEdit ~= nil) then
    data.onEdit(data, value)
  end

  -- Need to make sure we are not currently editing another field
  if(value == true) then

    -- Look to see if a field is being edited
    if(self.editingField ~= nil) then

      -- Exit field's edit mode
      self:EditTextEditor(self.editingField, false)
      return

    end

    -- Set new field to edit mode
    self.editingField = data

  else
    self.editingField = nil
  end

  -- change the edit mode to the new value
  data.editing = value

  -- Make sure the field deselects when exiting edit mode
  if(data.editing == false) then
    if(data.autoDeselect == true) then
      self:TextEditorDeselect(data)
    end
    data.mflag = false
    -- TODO need to call action here, should this be a lose focus event?

    if(data.onAction ~= nil and callAction ~= false) then
      data.onAction(self:TextEditorExport(data))
    end

  end

  -- Force the text field to redraw itself
  self:TextEditorInvalidateBuffer(data)

end

function EditorUI:ResizeTexdtEditor(data, width, height, x, y)

  if(data.rect.x == x and data.rect.y == y and data.rect.w == width and data.rect.h == height) then
    return
  end

  -- Update the rect value
  data.rect.x = x
  data.rect.y = y
  data.rect.w = width
  data.rect.h = height
  data.tiles.c = math.floor(data.rect.x / data.charSize.x)
  data.tiles.r = math.floor(data.rect.y / data.charSize.y)
  data.tiles.w = math.ceil(data.rect.w / data.charSize.x)
  data.tiles.h = math.ceil(data.rect.h / data.charSize.y)
  data.viewPort = NewRect(data.rect.x, data.rect.y, data.rect.w, data.rect.h)
 
  self:TextEditorInvalidateBuffer(data)

end
