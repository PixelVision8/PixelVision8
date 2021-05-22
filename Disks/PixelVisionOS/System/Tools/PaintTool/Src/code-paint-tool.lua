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
LoadScript( "code-color-editor-modal" )

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
        }
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
      
      _paintTool:LoadSuccess();
                
    else
    
      pixelVisionOS:LoadError(_paintTool.toolName)
      
    end
    
    -- Return the draw tool data
    return _paintTool

end

-- function PaintTool:ImportGameColors(self.colorsPath)
  
--   _paintTool:LoadSuccess()

-- end

function PaintTool:LoadSuccess()

  self.colorsPath = ""

  -- Load the image
  self.image = ReadImage(NewWorkspacePath(self.targetFile))

  local defaultColors = false

  -- Get a reference to the image's colors 
  local imageColors = self.image.colors

  if(#imageColors <= 1 and imageColors[1] == "#FF00FF") then

    imageColors = self.defaultColors
    table.insert(imageColors, 1, "#FF00FF")

    self.image.Clear()

    self.colorsPath = "none"

    defaultColors = true

  end

  -- Set the color offset after the tool's default colors + 1 for the mask
  self.colorOffset = 17

  -- Set the mask color to the color offset - 1
  self.maskColor = self.colorOffset - 1

  -- Save the default bg color so we don't have to keep calling it the loop below
  local defaultMaskColor = MaskColor()

  -- Set the mask to the tool's default mask color
  Color(self.maskColor,defaultMaskColor)

  -- Save the total colors so we don't have to keep recalculating this later on
  self.totalColors = 256

  self:ImportColors(nil, imageColors)

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

  
  if(string.ends(self.targetFile, "colors.png") == false) then

    local currentPath = NewWorkspacePath(self.rootDirectory)

    
    local messagePath = ""

    if(currentPath.EntityName == "Sprites" and pixelVisionOS:ValidateGameInDir(currentPath.ParentPath)) then

      print("Sprite inside of a Sprites folder")
      
      if(PathExists(currentPath.ParentPath.AppendFile("colors.png"))) then
        
        self.colorsPath = currentPath.ParentPath.AppendFile("colors.png").Path

        messagePath = currentPath.ParentPath.EntityName

      else
        messagePath = "PixelVision8"
        self.colorsPath = "default"

      end
    
    elseif(pixelVisionOS:ValidateGameInDir(currentPath)) then
    
      print("Sprite inside of a game folder")

      if(PathExists(currentPath.AppendFile("colors.png"))) then
        messagePath = currentPath.EntityName
        self.colorsPath = currentPath.AppendFile("colors.png").Path

      else
        messagePath = "PixelVision8"
        self.colorsPath = "default"

      end
    
    end

    if(defaultColors and self.colorsPath == "default") then
      self.colorsPath = ""
    end

    if(self.colorsPath ~= "") then

      self.importColorMessage = "The " .. self.toolName .. " has detected another color option:\n\n'" .. messagePath .. "/colors.png'\n\nDo you want to use these colors instead and replace the sprite colors?"

    end

  end

end

function PaintTool:OnImportColors(path)
  
  local buttons =
  {
    {
      name = "modalyesbutton",
      action = function(target)
        self:ImportColors(path)
        target.onParentClose()
      end,
      key = Keys.Enter,
      tooltip = "Press 'enter' to save"
    },
    {
      name = "modalnobutton",
      action = function(target)
        target.onParentClose()
      end,
      key = Keys.N,
      tooltip = "Press 'n' to not save"
    }
  }
  
  pixelVisionOS:ShowMessageModal("Import Colors", self.importColorMessage, 160, buttons)


end

function PaintTool:ImportColors(path, colors)

  if(path ~= nil and path ~= "") then
    
    if(path ~= "default") then
      
      local filePath = NewWorkspacePath(path)

      if(PathExists(filePath)) then

        local colorsimage = ReadImage(filePath)

        -- Get a reference to the image's colors 
        colors = colorsimage.colors

      end

    end
    
  end

  if(colors == nil) then
    colors = self.defaultColors
  end
  
  -- TODO need a way to remap colors
  -- We need to save the default tool colors to use when showing the color editor modal
  self.cachedToolColors = {}
  
  local tmpColorIndex, tmpRefIndex = 0, 0

  -- Loop through all of the system colors
  for i = 1, self.totalColors do

    -- Calculate the real color index
    tmpColorIndex = i-1

    -- Get the color accounting for Lua's 1 based arrays
    self.cachedToolColors[i] = Color(tmpColorIndex)

    -- Test to see if the color is after the colorOffset
    if(i > self.colorOffset) then
      
      -- Map the index back to the image's colors
      tmpRefIndex = i - self.colorOffset

      -- Set the new color from the image or to the default mask
      Color(tmpColorIndex, tmpRefIndex <= #colors and colors[tmpRefIndex] or MaskColor())

    end
   
  end

  self:InvalidateColors()

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