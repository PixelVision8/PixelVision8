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

        -- _imageTool.codeMode = _imageTool.extension == ".lua" or _imageTool.extension == ".cs"
       
        local pathSplit = string.split(_imageTool.targetFile, "/")
        
        -- Update title with file path
        _imageTool.toolTitle = pathSplit[#pathSplit - 1] .. "/" .. pathSplit[#pathSplit]
        
        _imageTool:LoadSuccess()
                
    else
    
        pixelVisionOS:ChangeTitle(_imageTool.toolName, "toolbaricontool")
        
        pixelVisionOS:ShowMessageModal(_imageTool.toolName .. " Error", "The tool could not load without a reference to a file to edit.", 160, false,
          function()
            QuitCurrentTool()
          end
        )
        
    end
    
    -- Return the draw tool data
    return _imageTool

end

function ImageTool:LoadSuccess()

    -- Load the image
    self.image = ReadImage(NewWorkspacePath(self.targetFile))

    -- TODO need to copy out colors from the image  
    local imageColors = self.image.colors
    local totalColors = math.max(8, #imageColors)
    
    -- Get the last color index for the offset
    local colorOffset = TotalColors()

    -- Double the color memory
    ResizeColorMemory(512)

    -- Create the canvas to display the image color palette
    self.colorMemoryCanvas = NewCanvas(8, totalColors / 8)
    local pixels = {}

    -- Add the image colors to the color chip
    for i = 1, totalColors do
        Color(colorOffset + (i-1), imageColors[i])

        local index = i + 255
        table.insert(pixels, index)
    end

        -- TODO each pixel should be set above
    self.colorMemoryCanvas:SetPixels(pixels)
    
    -- Get the image pixels
    local pixelData = self.image.GetPixels()

    -- TODO we may not need to do this if we just offset the canvas when drawing?
    -- Shift colors
    for i = 1, #pixelData do
        pixelData[i] = pixelData[i] + colorOffset
    end

    -- Create a new canvas
    self.imageCanvas = NewCanvas(self.image.width, self.image.height)

    -- Copy the modified image pixel data over to the new canvas
    self.imageCanvas.SetPixels(pixelData)

    -- The image is loaded at this point
    self.toolLoaded = true
    
    -- Create tool title from path
    -- local pathSplit = string.split(targetFile, "/")
    -- self.toolTitle = pathSplit[#pathSplit - 1] .. "/" .. pathSplit[#pathSplit]


    -- -- Configure the menu
    self:CreateDropDownMenu()

    self:CreateEditorPanel()

    -- CreateViewport()

    -- Reset Tool Validation
    self:ResetDataValidation()
    
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

-- function ImageTool:Draw()

--   -- if(self.lineNumbersInvalid == true) then
--   --     self:DrawLineNumbers()

--   --     self:ResetLineNumberInvalidation()
--   -- end

-- end

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