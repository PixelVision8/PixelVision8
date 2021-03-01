function PixelVisionOS:CreateInputArea(rect, text, toolTip, font, colorOffset, spacing)

    local data = editorUI:CreateInputArea(rect, text, toolTip, font, colorOffset, spacing)

    self:AddInputActions(data)

    data.keymap["ctrl-k"] = function(targetData)
        if targetData.incsearch == true then
            self:TextEditorSearchTextAndNavigate(targetData, targetData.cy)
        end
    end

    data.keymap["shift-ctrl-f"] = function(targetData)
        self:TextEditorSearchPreviousFunction(targetData)
    end

    data.keymap["ctrl-f"] = function(targetData)
        self:TextEditorSearchNextFunction(targetData)
    end

    return data

end

function PixelVisionOS:CreateInputField(rect, text, toolTip, pattern, font, colorOffset, spacing)

    local data = editorUI:CreateInputField(rect, text, toolTip, pattern, font, colorOffset, spacing)

    self:AddInputActions(data)

    return data

end

function PixelVisionOS:CodeEditor(rect, text, toolTip, language)
    
    local data = self:CreateInputArea(rect, text, toolTip)

    -- TODO create highlighter

    return data

end

function PixelVisionOS:AddInputActions(data)

    data.keymap["ctrl-x"] = function(targetData)
        -- if targetData.readonly then _systemMessage("The file is readonly !", 1, 9, 4) return end
        self:TextEditorCutText(targetData)
    end

    data.keymap["ctrl-c"] = function(targetData) 
        self:TextEditorCopyText(targetData) 
    end

    data.keymap["ctrl-v"] = function(targetData)
    
        self:TextEditorPasteText(targetData)
    end

    data.keymap["ctrl-z"] = function(targetData)
        -- if targetData.readonly then _systemMessage("The file is readonly !", 1, 9, 4) return end
        self:TextEditorUndo(targetData)
    end
  
    data.keymap["shift-ctrl-z"] = function(targetData)
        -- if targetData.readonly then _systemMessage("The file is readonly !", 1, 9, 4) return end
        self:TextEditorRedo(targetData)
    end
  
    data.keymap["ctrl-y"] = function(targetData)
        -- if targetData.readonly then _systemMessage("The file is readonly !", 1, 9, 4) return end
        self:TextEditorRedo(targetData)
    end

end

--Copy selection text (Only if selecting)
function PixelVisionOS:TextEditorCopyText(data)
local sxs, sys, sxe, sye = editorUI:TextEditorGetOrderedSelect(data)
if sxs then --If there are any selection
    local clipbuffer = {}
    for lnum = sys, sye do
    local line = data.buffer[lnum]

    if lnum == sys and lnum == sye then --Single line selection
        line = line:sub(sxs, sxe)
    elseif lnum == sys then
        line = line:sub(sxs, - 1)
    elseif lnum == sye then
        line = line:sub(1, sxe)
    end

    table.insert(clipbuffer, line)
    end

    local clipdata = table.concat(clipbuffer, "\n")

    self:TextEditorClipboard(clipdata)
end
end

--Cut selection text
function PixelVisionOS:TextEditorCutText(data)
    if data.sxs then
        self:TextEditorCopyText(data)
        editorUI:TextEditorDeleteSelection(data)
    end
end

    -- Paste the text from the clipboard
function PixelVisionOS:TextEditorPasteText(data)
    editorUI:TextEditorBeginUndoable(data)
    if data.sxs then editorUI:TextEditorDeleteSelection(data) end
    local text = self:TextEditorClipboard()

    if(text == nil or text == "") then
        return
    end

    text = text:gsub("\t", " ") -- tabs mess up the layout, replace them with spaces
    local firstLine = true
    for line in string.gmatch(text.."\n", "([^\r\n]*)\r?\n") do
        if not firstLine then
        editorUI:TextEditorInsertNewLine(data) data.cx = 1
        else
        firstLine = false
        end
        editorUI:TextEditorTextInput(data, line)
    end
    if editorUI:TextEditorCheckPosition(data) then editorUI:TextEditorDrawBuffer(data) else editorUI:TextEditorDrawLine(data) end

    -- self:TextEditorDrawLineNum(data)
    editorUI:TextEditorEndUndoable(data)
    editorUI:TextEditorInvalidateText(data)
end

function PixelVisionOS:TextEditorClipboard(value)

    -- print("TextEditorClipboard", value)
    -- TODO this should be tied to the OS scope
    if(value ~= nil) then

        SetClipboardText(value)

        -- self.codeEditorClipboardValue = value
    end

    return GetClipboardText()

end

function PixelVisionOS:TextEditorSearchTextAndNavigate(data, from_line)
    for i, t in ipairs(data.buffer)
    do
      if from_line ~= nil and i > from_line then
        if string.find(t, data.searchtxt) then
          data.cy = i
          data.vy = i
          editorUI:TextEditorCheckPosition(data)
          editorUI:TextEditorInvalidateBuffer(data)
  
          break
        end
      end
    end
  
  end

function PixelVisionOS:TextEditorSearchPreviousFunction(data)
    
    local highermatch = -1
    for i, t in ipairs(data.buffer)
    do
      if i < data.cy then
        if string.find(t, "function ") then
          highermatch = i
        end
      end
    end
  
    if highermatch > - 1 then
      data.cy = highermatch
      data.vy = highermatch
      editorUI:TextEditorCheckPosition(data)
      -- Force the buffer to redraw
      editorUI:TextEditorInvalidateBuffer(data)
    end
  
end

function PixelVisionOS:TextEditorSearchNextFunction(data)
    for i, t in ipairs(data.buffer)
    do
      if i > data.cy then
        if string.find(t, "function ") then
          data.cy = i
          editorUI:TextEditorCheckPosition(data)
          -- Force the buffer to redraw
          editorUI:TextEditorInvalidateBuffer(data)
  
          break
        end
      end
    end
  end

  -- Perform an undo. This will pop one entry off the undo
-- stack and restore the editor's contents & cursor state.
function PixelVisionOS:TextEditorUndo(data)
    if #data.undoStack == 0 then
      -- beep?
      return
    end
    -- pull one entry from the undo stack
    local text, state = unpack(table.remove(data.undoStack))
  
    -- push a new entry onto the redo stack
    table.insert(data.redoStack, {
      editorUI:TextEditorExport(data),
      editorUI:TextEditorGetState(data)
    })
  
    -- restore the editor contents
    editorUI:TextEditorImport(data, text)
    -- restore the cursor state
    editorUI:TextEditorSetState(data, state)
  end
  
  -- Perform a redo. This will pop one entry off the redo
  -- stack and restore the editor's contents & cursor state.
  function PixelVisionOS:TextEditorRedo(data)
    if #data.redoStack == 0 then
      -- beep?
      return
    end
    -- pull one entry from the redo stack
    local text, state = unpack(table.remove(data.redoStack))
    -- push a new entry onto the undo stack
    table.insert(data.undoStack, {
      editorUI:TextEditorExport(data),
      editorUI:TextEditorGetState(data)
    })
    -- restore the editor contents
    editorUI:TextEditorImport(data, text)
    -- restore the cursor state
    editorUI:TextEditorSetState(data, state)
  end