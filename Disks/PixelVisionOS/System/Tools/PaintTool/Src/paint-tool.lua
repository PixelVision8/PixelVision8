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

LoadScript("drop-down-menu" )
LoadScript("canvas-panel" )
LoadScript("picker-panel" )
LoadScript("picker-modal" )
LoadScript("toolbar" )
LoadScript("index-sprites" )
LoadScript("index-colors" )
LoadScript("index-flags" )
LoadScript("selection" )
LoadScript("color-editor-modal" )
LoadScript("import-colors" )

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
        defaultColors = {
          "#2D1B2E",
          "#218A91",
          "#3CC2FA",
          "#9AF6FD",
          "#4A247C",
          "#574B67",
          "#937AC5",
          "#8AE25D",
          "#8E2B45",
          "#F04156",
          "#F272CE",
          "#D3C0A8",
          "#C5754A",
          "#F2A759",
          "#F7DB53",
          "#F9F4EA"
        },
        colorsPath = "",
        -- Set the color offset after the tool's default colors + 1 for the mask
        colorOffset = 17,
        -- Save the default bg color so we don't have to keep calling it the loop below
        defaultMaskColor = MaskColor(),
        -- Save the total colors so we don't have to keep recalculating this later on
        totalColors = 256
    }

    DumpColors()

    -- Set the mask color to the color offset - 1
    _paintTool.maskColor = _paintTool.colorOffset - 1

    -- Set the mask to the tool's default mask color
    Color(_paintTool.maskColor, _paintTool.defaultMaskColor)

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
      
      _paintTool:LoadImage()
      _paintTool:ConfigureTool()
                
    else
    
      pixelVisionOS:LoadError(_paintTool.toolName)
      
    end
    
    -- Return the draw tool data
    return _paintTool

end


function PaintTool:LoadImage(defaultColors)
  
  -- Load the image
  self.image = ReadImage(NewWorkspacePath(self.targetFile), self.defaultMaskColor, defaultColors)

  self.useDefaultColors = false

  -- Get a reference to the image's colors 
  local imageColors = self.image.colors

  if(#imageColors <= 1 and imageColors[1] == "#FF00FF") then

    imageColors = self.defaultColors

    self.image.Clear()

    self.colorsPath = "none"

    self.useDefaultColors = true

  end

  self:ImportColors(imageColors)

end

function PaintTool:ConfigureTool()

  -- Create the drop down menu
  self:CreateDropDownMenu()

  -- Create the tool bar
  self:CreateToolbar()

  -- Create the canvas panel
  self:CreateCanvasPanel()

  -- Create the picker panel
  self:CreatePickerPanel()

  -- TODO this needs to read from the previous session
  self:OnSelectTool("pointer")

  -- Reset Tool Validation
  self:ResetDataValidation()

  self:ColorCheck()

end

function DumpColors(start)

  start = start or 1

  local tmpColors = {}

  for i = start, TotalColors() do
    table.insert(tmpColors, Color(i-1))
  end

  -- print("Colors", dump(tmpColors))

  return tmpColors

end

function PaintTool:InvalidateData()

  -- Only everything if it needs to be
  if(self.invalid == true)then
    return
  end

  pixelVisionOS:ChangeTitle(self.toolTitle .."*", "toolbariconfile")

  pixelVisionOS:EnableMenuItemByName("Save", true)

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

  pixelVisionOS:EnableMenuItemByName("Save", false)

end

function PaintTool:OnSave()

  local image = NewImage(self.imageLayerCanvas.Width, self.imageLayerCanvas.Height, self.imageLayerCanvas.GetPixels(), self:UniqueColors())

  SaveImage(
    NewWorkspacePath(self.targetFile),
    image,
    MaskColor()
  )

  self:ResetDataValidation()

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