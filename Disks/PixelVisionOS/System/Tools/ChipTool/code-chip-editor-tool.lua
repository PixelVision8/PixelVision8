--[[
	Pixel Vision 8 - Debug Tool
	Copyright (C) 2016, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- Create table to store the workspace tool logic
ChipEditorTool = {}
ChipEditorTool.__index = ChipEditorTool

LoadScript("code-drop-down-menu")
LoadScript("code-chip-picker")
LoadScript("code-chip-selector-panel")
LoadScript("code-settings-panel")
LoadScript("code-chip-templates")

function ChipEditorTool:Init()

  -- Create a new table for the instance with default properties
  local _chipEditorTool = {
  
      toolName = "Chip Editor",
      runnerName = SystemName(),
      rootPath = ReadMetadata("RootPath", "/"),
      rootDirectory = ReadMetadata("directory", nil),
      targetFile = ReadMetadata("file", nil),
      invalid = true,
      SaveShortcut = 5,
      editorMode = -1,
      inputFields = {},
      specsLocked = false,
      invalidateColors = false,
      validWaves = {
        "ab", -- any
        "ef", -- square
        "cd", -- saw tooth
        -- "gh", -- Sine (Not enabled by default)
        "ij", -- noise
        "{|", -- triangle
        "kl", -- wave
      },
      waveTypeIDs = {
        -1,
        0,
        1,
        -- 2,
        3,
        4,
        5
      },
      waveToolTips =
      {
        "Support for square wave form on this channel",
        "Support for saw tooth wave form on this channel",
        -- "Sine waves are not currently supported.",
        "Support for noise wave form on this channel",
        "Support for triangle wave form on this channel",
        "Support for wav sample files on this channel",
        "Support for any wave form on this channel.",
      },
      selectedLineDrawArgs = nil,
      chipPicker = nil,
      cancelSelectionRect = {
        x = 40,
        y = 16,
        w = 216,
        h = 152 + 8
      },
      chipSpriteNames = {
        "chipgpuempty",
        "chipcartempty",
        "chipsoundempty"
      }
  }

  
    
  -- Create a global reference of the new workspace tool
  setmetatable(_chipEditorTool, ChipEditorTool)
  
  rootDirectory = ReadMetadata("directory", nil)

  local success = false

  if(_chipEditorTool.rootDirectory ~= nil) then

    -- Load only the game data we really need
    success = gameEditor:Load(_chipEditorTool.rootDirectory, {SaveFlags.System, SaveFlags.Meta, SaveFlags.Colors})

  end

  -- If data loaded activate the tool
  if(success == true) then
    local pathSplit = string.split(_chipEditorTool.rootDirectory, "/")

    -- Update title with file path
    _chipEditorTool.toolTitle = pathSplit[#pathSplit] .. "/data.json"
    
    _chipEditorTool:CreateDropDownMenu()
    _chipEditorTool:CreateChipTemplates()
    
    _chipEditorTool:CreateChipSelectorPanel()
    _chipEditorTool:CreateSettingsPanel()

    _chipEditorTool.specsLocked = gameEditor:GameSpecsLocked()

    _chipEditorTool:UpdateFieldLock()
    _chipEditorTool:ResetDataValidation()

  else

    pixelVisionOS:LoadError(_chipEditorTool.toolName)

  end
  
  return _chipEditorTool

end

function ChipEditorTool:InvalidateData()

  -- Only everything if it needs to be
  if(self.invalid == true)then
    return
  end

  pixelVisionOS:ChangeTitle(self.toolTitle .."*", "toolbariconfile")

  pixelVisionOS:EnableMenuItem(self.SaveShortcut, true)

  self.invalid = true

end

function ChipEditorTool:ResetDataValidation()

  -- Only everything if it needs to be
  if(self.invalid == false)then
    return
  end

  pixelVisionOS:ChangeTitle(self.toolTitle, "toolbariconfile")
  self.invalid = false

  pixelVisionOS:EnableMenuItem(self.SaveShortcut, false)
  
end

-- The Draw() method is part of the game's life cycle. It is called after Update() and is where
-- all of our draw calls should go. We'll be using this to render sprites to the display.
function ChipEditorTool:Draw()

  if(pixelVisionOS:IsModalActive() == false) then
    if(self.editorMode ~= 0) then

      DrawMetaSprite(
        self.selectedLineDrawArgs[1],
        self.selectedLineDrawArgs[2],
        self.selectedLineDrawArgs[3]
      )

      editorUI:DrawChipPicker(self.chipPicker)

    end
  end

end
