--[[
	Pixel Vision 8 - Debug Tool
	Copyright (C) 2016, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- Load in the editor framework script to access tool components
LoadScript("sb-sprites")
LoadScript("pixel-vision-os-v2")

-- Create an global instance of the Pixel Vision OS
_G["pixelVisionOS"] = PixelVisionOS:Init()

local toolName = "Text Editor"

-- local fileSize = "000k"
local invalid = true

local rootDirectory = nil
local showLines = false
local lineWidth = 0
local totalLines = 0
local codeMode = false

function Init()

  BackgroundColor(5)

  -- Disable the back key in this tool
  EnableBackKey(false)

  EnableAutoRun(false)

  rootDirectory = ReadMetadata("directory", nil)

  -- Get the target file
  targetFile = ReadMetadata("file", nil)

  if(targetFile ~= nil) then

    targetFilePath = NewWorkspacePath(targetFile)

    codeMode = targetFilePath.GetExtension() == ".lua"

    local pathSplit = string.split(targetFile, "/")

    -- Update title with file path
    toolTitle = pathSplit[#pathSplit - 1] .. "/" .. pathSplit[#pathSplit]

    local menuOptions =
    {
      -- About ID 1
      {name = "About", action = function() pixelVisionOS:ShowAboutModal(toolName) end, toolTip = "Learn about PV8."},
      {divider = true},
      {name = "New", action = OnNewSound, enabled = false, key = Keys.N, toolTip = "Create a new text file."}, -- Reset all the values
      {name = "Save", action = OnSave, enabled = false, key = Keys.S, toolTip = "Save changes made to the text file."}, -- Reset all the values
      {name = "Revert", action = nil, enabled = false, toolTip = "Revert the text file to its previous state."}, -- Reset all the values
      {divider = true},
      {name = "Cut", action = OnCopyColor, enabled = false, key = Keys.X, toolTip = "Cut the currently selected text."}, -- Reset all the values
      {name = "Copy", action = OnCopyColor, enabled = false, key = Keys.C, toolTip = "Copy the currently selected text."}, -- Reset all the values
      {name = "Paste", action = OnPasteColor, enabled = false, key = Keys.V, toolTip = "Paste the last copied text."}, -- Reset all the values

    }

    if(codeMode == true) then

      table.insert(menuOptions, {divider = true})
      table.insert(menuOptions, {name = "Toggle Lines", action = ToggleLineNumbers, key = Keys.L, toolTip = "Toggle the line numbers for the editor."})

      if(PathExists(NewWorkspacePath(rootDirectory).AppendFile("code.lua"))) then
        table.insert(menuOptions, {name = "Run Game", action = OnRunGame, key = Keys.R, toolTip = "Run the code for this game."})
      end
    end

    -- Add the last part of the menu options
    table.insert(menuOptions, {divider = true})
    table.insert(menuOptions, {name = "Quit", key = Keys.Q, action = OnQuit, toolTip = "Quit the current game."}) -- Quit the current game

    pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")


    vSliderData = editorUI:CreateSlider({x = 235, y = 20, w = 10, h = 193}, "vsliderhandle", "Scroll text vertically.")
    vSliderData.onAction = OnVerticalScroll

    hSliderData = editorUI:CreateSlider({ x = 4, y = 211, w = 233, h = 10}, "hsliderhandle", "Scroll text horizontally.", true)
    hSliderData.onAction = OnHorizontalScroll

    -- local lineWidth = 0
    --
    -- lineInputArea = editorUI:CreateInputArea({x = 8, y = 24, w = lineWidth, h = 184}, nil, "Click to edit the text.")
    -- editorUI:Enable(lineInputArea, false)
    -- lineInputArea.editable = false

    -- Add an extra 8 pixels between the line numbers
    -- lineWidth = lineWidth + 8

    -- Create input area
    inputAreaData = editorUI:CreateInputArea({x = 8, y = 24, w = 224, h = 184}, nil, "Click to edit the text.")
    inputAreaData.wrap = false
    inputAreaData.editable = true
    inputAreaData.autoDeselect = false
    inputAreaData.colorize = codeMode

    -- Prepare the input area for scrolling
    inputAreaData.scrollValue = {x = 0, y = 0}

    -- inputAreaData.colorOffset = 32
    inputAreaData.onAction = function(text)
      -- print("input area updated")
    end

    -- TODO need to read the toggle line state from the bios

    showLines = ReadBiosData("ShowLinesInTextEditor") == "True" and true or false

    RefreshEditor()

    ResetDataValidation()
    -- pixelVisionOS:DisplayMessage(toolName .. ": This tool allows you to edit text files.", 5)
    -- pixelVisionOS:ChangeTitle(toolTitle, "toolbariconfile")

  else

    pixelVisionOS:ChangeTitle(toolName, "toolbaricontool")

    pixelVisionOS:ShowMessageModal(toolName .. " Error", "The tool could not load without a reference to a file to edit.", 160, false,
      function()
        QuitCurrentTool()
      end
    )
  end

end

function OnRunGame()


  local parentPath = targetFilePath.ParentPath

  if(invalid == true) then

    pixelVisionOS:ShowMessageModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before running the game?", 160, true,
      function()
        if(pixelVisionOS.messageModal.selectionValue == true) then
          -- Save changes
          OnSave()

        end

        -- TODO should check that this is a game directory or that this file is at least a code.lua file
        LoadGame(parentPath.Path)
      end
    )

  else
    -- Quit the tool
    LoadGame(parentPath.Path)
  end


end

function OnQuit()

  if(invalid == true) then

    pixelVisionOS:ShowMessageModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before you quit?", 160, true,
      function()
        if(pixelVisionOS.messageModal.selectionValue == true) then
          -- Save changes
          OnSave()

        end

        -- Quit the tool
        QuitCurrentTool()
      end
    )

  else
    -- Quit the tool
    QuitCurrentTool()
  end

end

function ToggleLineNumbers()

  if(codeMode == false) then
    return
  end

  -- TODO need to save this value to the bios

  showLines = not showLines

  WriteBiosData("ShowLinesInTextEditor", showLines == true and "True" or "False")

  InvalidateLineNumbers()

  -- inputAreaData.rect.x = 8 + lineWidth
  -- inputAreaData.width = 224 - lineWidth
  -- TODO this needs to shift the text area over and display the line numbers. Should be part of the tool, not the component

end

function CalculateLineGutter()

  -- if(totalLines == #inputAreaData.buffer) then
  --   return
  -- end

  -- Update total
  totalLines = #inputAreaData.buffer

  lineWidth = showLines == true and ((#tostring(totalLines) + 1) * 8) or 0

  -- Only resize the input field if the size doesn't match
  local newWidth = 224 - lineWidth

  if(inputAreaData.rect.w ~= newWidth) then

    editorUI:ResizeTexdtEditor(inputAreaData, newWidth, inputAreaData.rect.h, 8 + lineWidth, inputAreaData.rect.y)
  end

end

function InvalidateData()

  -- Only everything if it needs to be
  if(invalid == true)then
    return
  end

  pixelVisionOS:ChangeTitle(toolTitle .."*", "toolbariconfile")

  pixelVisionOS:EnableMenuItem(4, true)

  invalid = true

end

function ResetDataValidation()

  -- Only everything if it needs to be
  if(invalid == false)then
    return
  end

  pixelVisionOS:ChangeTitle(toolTitle, "toolbariconfile")
  invalid = false

  -- Reset the input field's text validation
  editorUI:TextEditorResetTextValidation(inputAreaData)

  pixelVisionOS:EnableMenuItem(4, false)

end

function OnClear()
  editorUI:ChangeInputArea(inputAreaData, "")
  editorUI:InputAreaInvalidateText(inputAreaData)
end

function RefreshEditor()

  -- print("Load Text File", targetFile)
  local tmpText = ReadTextFile(targetFile)

  -- fileSize = GetFileSizeAsString(targetFile)

  editorUI:ChangeInputArea(inputAreaData, tmpText)

  ResetDataValidation()

  InvalidateLineNumbers()

  if(SessionID() == ReadSaveData("sessionID", "") and targetFile == ReadSaveData("targetFile", "")) then
    local cursorPosString = ReadSaveData("cursor", "0,0")


    local tmpCursor = editorUI:TextEditorGetState(inputAreaData)

    local map = {
      "cx",
      "cy",
      "sxs",
      "sys",
      "sxe",
      "sye",
    }

    local counter = 1
    for word in string.gmatch(cursorPosString, '([^,]+)') do

      tmpCursor[map[counter]] = tonumber(word)
      counter = counter + 1

    end

    editorUI:TextEditorSetState(inputAreaData, tmpCursor)

    -- Restore last scroll position
    local scrollPosString = ReadSaveData("scroll", "0,0")

    map = {"vx", "vy"}

    counter = 1
    for word in string.gmatch(scrollPosString, '([^,]+)') do

      inputAreaData[map[counter]] = tonumber(word)
      counter = counter + 1

    end


  end

  editorUI:EditTextEditor(inputAreaData, true, false)

  --
  -- inputAreaData.inFocus = true
  -- editorUI:SetFocus(inputAreaData, 3)

end

function OnSave()

  local success = SaveTextToFile(targetFile, editorUI:TextEditorExport(inputAreaData), false)

  if(success == true) then
    pixelVisionOS:DisplayMessage("Saving '" .. targetFile .. "'.", 5 )
    ResetDataValidation()
  else
    pixelVisionOS:DisplayMessage("Unable to save '" .. targetFile .. "'.", 5 )
  end

end

function OnHorizontalScroll(value)

  local charPos = math.ceil(((inputAreaData.maxLineWidth + 1) - (inputAreaData.tiles.w)) * value) + 1

  if(inputAreaData.vx ~= charPos) then
    inputAreaData.vx = charPos
    editorUI:TextEditorInvalidateBuffer(inputAreaData)
  end

end

function OnVerticalScroll(value)

  local line = math.ceil((#inputAreaData.buffer - (inputAreaData.tiles.h - 1)) * value)
  if(inputAreaData.vy ~= line) then
    inputAreaData.vy = Clamp(line, 1, #inputAreaData.buffer)

    editorUI:TextEditorInvalidateBuffer(inputAreaData)
  end

  InvalidateLineNumbers()

end

function DrawLineNumbers()

  if(codeMode == false) then
    return
  end

  -- Make sure the gutter is the correct size
  CalculateLineGutter()

  -- Only draw the line numbers if show lines is true
  if(showLines ~= true) then
    return
  end

  local offset = inputAreaData.vy - 1
  local totalLines = inputAreaData.tiles.h
  local padWidth = (lineWidth / 8) - 1
  for i = 1, inputAreaData.tiles.h do

    DrawText(string.lpad(tostring(i + offset), padWidth, "0") .. " ", 1, 2 + i, DrawMode.Tile, "input", 44)

  end

end

function Update(timeDelta)

  -- Convert timeDelta to a float
  timeDelta = timeDelta / 1000
  
  -- This needs to be the first call to make sure all of the editor UI is updated first
  pixelVisionOS:Update(timeDelta)

  if(inputAreaData ~= nil and inputAreaData.inFocus == true and pixelVisionOS:IsModalActive()) then
    editorUI:ClearFocus(inputAreaData)
  end
  -- Only update the tool's UI when the modal isn't active
  if(pixelVisionOS:IsModalActive() == false and targetFile ~= nil and pixelVisionOS.titleBar.menu.showMenu == false) then

    

    

    
    -- hSliderData

    -- print("Scroll", MouseWheel())

    -- Check to see if we should show the horizontal slider
    local showVSlider = #inputAreaData.buffer > inputAreaData.tiles.h

    -- Check for mouse wheel scrolling
    local wheelDir = MouseWheel()
    
    -- Test if we need to show or hide the slider
    if(vSliderData.enabled ~= showVSlider) then
      
      editorUI:Enable(vSliderData, showVSlider)
    end

    if(wheelDir.Y ~= 0) then
    
      local scrollValue = Clamp(wheelDir.y, -1, 1) * -5
      
      if(Key(Keys.LeftControl) == true or Key( Keys.RightControl)) then
        OnHorizontalScroll((Clamp(hSliderData.value * 100 + scrollValue, 0, 100)/100))
      else
        OnVerticalScroll((Clamp(vSliderData.value * 100 + scrollValue, 0, 100)/100))
      end
    
    elseif(wheelDir.X ~= 0) then

      OnHorizontalScroll((Clamp(hSliderData.value * 100 + (Clamp(wheelDir.y, -1, 1) * -5), 0, 100)/100))

    end

    if(vSliderData.enabled == true) then
      inputAreaData.scrollValue.y = (inputAreaData.vy - 1) / (#inputAreaData.buffer - inputAreaData.tiles.h)

      if(vSliderData.value ~= inputAreaData.scrollValue.y) then

        -- print("scroll", wheelDir, , inputAreaData.scrollValue.y)
        InvalidateLineNumbers()

        -- inputAreaData.scrollValue.y = inputAreaData.scrollValue.y + Clamp(wheelDir.y, -1, 1)/100

        editorUI:ChangeSlider(vSliderData, inputAreaData.scrollValue.y , false)
      end

    end

    -- Update the slider
    editorUI:UpdateSlider(vSliderData)

    -- Check to see if we should show the vertical slider
    local showHSlider = inputAreaData.maxLineWidth > inputAreaData.tiles.w

    -- Test if we need to show or hide the slider
    if(hSliderData.enabled ~= showHSlider) then
      editorUI:Enable(hSliderData, showHSlider)
    end

    if(hSliderData.enabled == true) then
      inputAreaData.scrollValue.x = (inputAreaData.vx - 1) / ((inputAreaData.maxLineWidth + 1) - inputAreaData.tiles.w)

    if(hSliderData.value ~= inputAreaData.scrollValue.x or wheelDir.x ~= 0) then

      -- OnHorizontalScroll(Clamp(hSliderData.value ))
        -- print(inputAreaData.vx, inputAreaData.maxLineWidth, inputAreaData.tiles.w)
        -- print("inputAreaData.scrollValue.x", inputAreaData.scrollValue.x)

        editorUI:ChangeSlider(hSliderData, inputAreaData.scrollValue.x + Clamp(wheelDir.x, -1, 1), false)
      end

    end

    -- Update the slider
    editorUI:UpdateSlider(hSliderData)

    -- Reset focus back to the text editor
    if(hSliderData.inFocus == false and vSliderData.inFocus == false and inputAreaData.inFocus == false and pixelVisionOS.titleBar.menu.showMenu == false) then
      editorUI:EditTextEditor(inputAreaData, true, false)
    end

    editorUI:UpdateInputArea(inputAreaData)

    -- TODO need a better way to check if the text has been changed in the editor
    if(inputAreaData.invalidText == true) then
      InvalidateData()
      InvalidateLineNumbers()
    end

  end

end

function InvalidateLineNumbers()
  lineNumbersInvalid = true
end

function ResetLineNumberInvalidation()
  lineNumbersInvalid = false
end

function Draw()


  RedrawDisplay()

  -- The ui should be the last thing to update after your own custom draw calls
  pixelVisionOS:Draw()

  if(lineNumbersInvalid == true) then
    DrawLineNumbers()

    ResetLineNumberInvalidation()
  end

end

function Shutdown()

  -- Save the current session ID
  WriteSaveData("sessionID", SessionID())

  if(targetFile ~= nil) then
    WriteSaveData("targetFile", targetFile)

    local state = editorUI:TextEditorGetState(inputAreaData)

    local stateString = tostring(state.cx) .. "," .. tostring(state.cy)

    -- if(state.sxs ~= nil and codeMode) then
    --   stateString = stateString .. "," .. tostring(state.sxs) .. "," .. tostring(state.sys) .. "," .. tostring(state.sxe) .. "," .. tostring(state.sye)
    -- end

    WriteSaveData("cursor", stateString)

    WriteSaveData("scroll", tostring(inputAreaData.vx) .. "," .. tostring(inputAreaData.vy))

  end

end
