--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- Create table to store the workspace tool logic
ImageTool = {}
ImageTool.__index = ImageTool

LoadScript("code-image-drop-down-menu")
LoadScript("code-image-preview-panel")

function CreateTool()
  return ImageTool:Init()
end

function ImageTool:Init()

    -- Create a new table for the instance with default properties
    local _imageTool = {
        toolName = "Preview Tool",
        runnerName = SystemName(),
        rootPath = ReadMetadata("RootPath", "/"),
        rootDirectory = ReadMetadata("directory", nil),
        targetFile = ReadMetadata("file", nil),
        invalid = true,
        image = nil,
        imageCanvas = nil,
        viewportRect = NewRect(0, 0, 224, 184),
        boundaryRect = NewRect(0,0,0,0),
        displayInvalid = true
    }

    -- Create a global reference of the new workspace tool
    setmetatable(_imageTool, ImageTool)

    -- Get the target file
    
    if(_imageTool.targetFile ~= nil) then

        _imageTool.targetFilePath = NewWorkspacePath(_imageTool.targetFile)
    
        _imageTool.extension = _imageTool.targetFilePath.GetExtension()

        local pathSplit = string.split(_imageTool.targetFile, "/")
        
        -- Update title with file path
        _imageTool.toolTitle = pathSplit[#pathSplit - 1] .. "/" .. pathSplit[#pathSplit]
        
        _imageTool:LoadSuccess()
                
    else
    
      pixelVisionOS:LoadError(_imageTool.toolName)
      
    end
    
    -- Return the draw tool data
    return _imageTool

end

function ImageTool:LoadSuccess()

    -- Load the image
    self.image = ReadImage(NewWorkspacePath(self.targetFile))

    -- TODO need to copy out colors from the image  
    local imageColors = self.image.colors

    -- Get the last color index for the offset
    self.colorOffset = 16
    self.maskColor = self.colorOffset

    Color(self.maskColor, "FF00FF")

    -- Add the image colors to the color chip
    for i = 1, #imageColors do
        Color(self.colorOffset - 1 + i , imageColors[i])
    end

    -- Get the image pixels
    local pixelData = self.image.GetPixels()

    -- Create a new canvas
    self.imageCanvas = NewCanvas(self.image.width, self.image.height)

    -- Copy the modified image pixel data over to the new canvas
    self.imageCanvas.SetPixels(pixelData)

    -- The image is loaded at this point
    self.toolLoaded = true

    -- Configure the menu
    self:CreateDropDownMenu()

    self:CreateEditorPanel()

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

function ImageTool:InvalidateData()

  -- Only everything if it needs to be
  if(self.invalid == true)then
    return
  end

  pixelVisionOS:ChangeTitle(self.toolTitle .."*", "toolbariconfile")

  -- pixelVisionOS:EnableMenuItem(SaveShortcut, true)

    self.invalid = true

end

function ImageTool:ResetDataValidation()

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

function ImageTool:OnSave()

  -- local success = SaveTextToFile(self.targetFile, editorUI:TextEditorExport(self.inputAreaData), false)

  -- if(success == true) then
  --   pixelVisionOS:DisplayMessage("Saving '" .. self.targetFile .. "'.", 5 )
  --   self:ResetDataValidation()
  -- else
  --   pixelVisionOS:DisplayMessage("Unable to save '" .. self.targetFile .. "'.", 5 )
  -- end

end

function ImageTool:Shutdown()

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