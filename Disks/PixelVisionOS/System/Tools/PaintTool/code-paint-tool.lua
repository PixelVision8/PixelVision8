--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- Create table to store the workspace tool logic
PaintTool = {}
PaintTool.__index = PaintTool

LoadScript( "code-drop-down-menu" )
LoadScript( "code-canvas-panel" )
LoadScript( "code-picker-panel" )
LoadScript( "code-picker-modal" )
LoadScript( "code-toolbar" )
LoadScript( "code-index-sprites" )
LoadScript( "code-index-colors" )
LoadScript( "code-index-flags" )
LoadScript( "code-selection" )

function CreateTool()
  return PaintTool:Init()
end

function PaintTool:Init()

    -- Create a new table for the instance with default properties
    local _paintTool = {
        toolName = "Paint Tool",
        runnerName = SystemName(),
        rootPath = ReadMetadata("RootPath", "/"),
        rootDirectory = ReadMetadata("directory", nil),
        targetFile = ReadMetadata("file", nil),
        invalid = true,
        image = nil,
    }

    -- Create a global reference of the new workspace tool
    setmetatable(_paintTool, PaintTool)

    pixelVisionOS:ResetUndoHistory(_paintTool)

    -- Get the target file
    
    if(_paintTool.targetFile ~= nil) then

        _paintTool.targetFilePath = NewWorkspacePath(_paintTool.targetFile)
    
        _paintTool.extension = _paintTool.targetFilePath.GetExtension()

        local pathSplit = string.split(_paintTool.targetFile, "/")
        
        -- Update title with file path
        _paintTool.toolTitle = pathSplit[#pathSplit - 1] .. "/" .. pathSplit[#pathSplit]
        
        _paintTool:LoadSuccess()
                
    else
    
      pixelVisionOS:LoadError(_paintTool.toolName)
      
    end
    
    -- Return the draw tool data
    return _paintTool

end

function PaintTool:LoadSuccess()

    -- Load the image
    self.image = ReadImage(NewWorkspacePath(self.targetFile))

    -- TODO need to copy out colors from the image  
    local imageColors = self.image.colors

    -- Get the last color index for the offset

    -- TODO need to limit the number of colors the tool has to reduce the color offset
    self.colorOffset = 17
    self.maskColor = self.colorOffset - 1 

    Color(self.maskColor, "FF00FF")

    -- Add the image colors to the color chip
    for i = 1, #imageColors do
        Color(self.colorOffset + i - 1 , imageColors[i])
    end

    -- TODO display message if there are more colors than the tool can show
    

    -- The image is loaded at this point
    self.toolLoaded = true

    -- Configure the menu
    self:CreateDropDownMenu()
  
    self:CreateToolbar()

    self:CreateCanvasPanel()

    self:CreatePickerPanel()

    -- TODO this needs to read from the previous session
    self:OnSelectTool("pointer")

    -- Reset Tool Validation
    self:ResetDataValidation()
    
end

function DumpColors(start)

  start = start or 1

  local tmpColors = {}

  for i = start, TotalColors() do
    table.insert(tmpColors, Color(i-1))
  end

  print("Colors", dump(tmpColors))
  

end

function PaintTool:InvalidateData()

  -- Only everything if it needs to be
  if(self.invalid == true)then
    return
  end

  pixelVisionOS:ChangeTitle(self.toolTitle .."*", "toolbariconfile")

  -- pixelVisionOS:EnableMenuItem(SaveShortcut, true)

    self.invalid = true

end

function PaintTool:ResetDataValidation()

  -- Only everything if it needs to be
  if(self.invalid == false)then
    return
  end

  pixelVisionOS:ChangeTitle(self.toolTitle, "toolbariconfile")
    self.invalid = false

  -- Reset the input field's text validation
  -- editorUI:TextEditorResetTextValidation(self.inputAreaData)

  -- pixelVisionOS:EnableMenuItem(SaveShortcut, false)

end

function PaintTool:OnSave()

  -- local success = SaveTextToFile(self.targetFile, editorUI:TextEditorExport(self.inputAreaData), false)

  -- if(success == true) then
  --   pixelVisionOS:DisplayMessage("Saving '" .. self.targetFile .. "'.", 5 )
  --   self:ResetDataValidation()
  -- else
  --   pixelVisionOS:DisplayMessage("Unable to save '" .. self.targetFile .. "'.", 5 )
  -- end

end

function PaintTool:Shutdown()

  -- -- Save the current session ID
  -- WriteSaveData("sessionID", SessionID())

  -- if(targetFile ~= nil) then

  --   WriteSaveData("targetFile", self.targetFile)

  --   local state = editorUI:TextEditorGetState(self.inputAreaData)

  --   local stateString = tostring(state.cx) .. "," .. tostring(state.cy)

  --   WriteSaveData("cursor", stateString)

  --   WriteSaveData("scroll", tostring(self.inputAreaData.vx) .. "," .. tostring(self.inputAreaData.vy))

  -- end

end