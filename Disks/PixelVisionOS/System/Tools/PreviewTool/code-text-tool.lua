--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- Create table to store the workspace tool logic
TextTool = {}
TextTool.__index = TextTool

LoadScript("code-text-drop-down-menu")
LoadScript("code-text-editor-panel")

function CreateTool()
  return TextTool:Init()
end

function TextTool:Init()

    -- Create a new table for the instance with default properties
    local _textTool = {
        toolName = "Preview Tool",
        runnerName = SystemName(),
        rootPath = ReadMetadata("RootPath", "/"),
        rootDirectory = ReadMetadata("directory", nil),
        targetFile = ReadMetadata("file", nil),
        invalid = true,
        showLines = false,
        lineWidth = 0,
        totalLines = 0,
        codeMode = false
        
    }

    -- Create a global reference of the new workspace tool
    setmetatable(_textTool, TextTool)


    -- Get the target file
    
    if(_textTool.targetFile ~= nil) then

        _textTool.targetFilePath = NewWorkspacePath(_textTool.targetFile)
    
        _textTool.extension = _textTool.targetFilePath.GetExtension()

        _textTool.codeMode = _textTool.extension == ".lua" or _textTool.extension == ".cs"
       
        local pathSplit = string.split(_textTool.targetFile, "/")
        
        -- Update title with file path
        _textTool.toolTitle = pathSplit[#pathSplit - 1] .. "/" .. pathSplit[#pathSplit]
        
        _textTool:LoadSuccess()
                
    else
    
        pixelVisionOS:ChangeTitle(_textTool.toolName, "toolbaricontool")
        
        pixelVisionOS:ShowMessageModal(_textTool.toolName .. " Error", "The tool could not load without a reference to a file to edit.", 160, false,
          function()
            QuitCurrentTool()
          end
        )
        
    end
    
    -- Return the draw tool data
    return _textTool

end

function TextTool:LoadSuccess()
    
    self:CreateDropDownMenu()

    self.showLines = ReadBiosData("ShowLinesInTextEditor") == "True" and true or false

    self:CreateEditorPanel()

    self:ResetDataValidation()
    
end

function TextTool:InvalidateData()

  -- Only everything if it needs to be
  if(self.invalid == true)then
    return
  end

  pixelVisionOS:ChangeTitle(self.toolTitle .."*", "toolbariconfile")

  pixelVisionOS:EnableMenuItem(SaveShortcut, true)

    self.invalid = true

end

function TextTool:ResetDataValidation()

  -- Only everything if it needs to be
  if(self.invalid == false)then
    return
  end

  pixelVisionOS:ChangeTitle(self.toolTitle, "toolbariconfile")
    self.invalid = false

  -- Reset the input field's text validation
  editorUI:TextEditorResetTextValidation(self.inputAreaData)

  pixelVisionOS:EnableMenuItem(SaveShortcut, false)

end

function TextTool:OnSave()

  local success = SaveTextToFile(self.targetFile, editorUI:TextEditorExport(self.inputAreaData), false)

  if(success == true) then
    pixelVisionOS:DisplayMessage("Saving '" .. self.targetFile .. "'.", 5 )
    self:ResetDataValidation()
  else
    pixelVisionOS:DisplayMessage("Unable to save '" .. self.targetFile .. "'.", 5 )
  end

end

function TextTool:Draw()

  if(self.lineNumbersInvalid == true) then
      self:DrawLineNumbers()

      self:ResetLineNumberInvalidation()
  end

end

function TextTool:Shutdown()

  -- Save the current session ID
  WriteSaveData("sessionID", SessionID())

  if(targetFile ~= nil) then

    WriteSaveData("targetFile", self.targetFile)

    local state = editorUI:TextEditorGetState(self.inputAreaData)

    local stateString = tostring(state.cx) .. "," .. tostring(state.cy)

    WriteSaveData("cursor", stateString)

    WriteSaveData("scroll", tostring(self.inputAreaData.vx) .. "," .. tostring(self.inputAreaData.vy))

  end

end