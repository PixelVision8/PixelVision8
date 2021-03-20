-- Create table to store the workspace tool logic
SFXTool = {}
SFXTool.__index = SFXTool

LoadScript("code-drop-down-menu")
LoadScript("code-instrument-panel")
LoadScript("code-sound-panel")
LoadScript("code-waveform-panel")
LoadScript("code-edit-panel")
LoadScript("pixel-vision-os-sound-progress-modal-v1")

function SFXTool:Init()

    -- Create a new table for the instance with default properties
    local _sfxTool = {
        toolName = "Sound Editor",
        runnerName = SystemName(),
        rootPath = ReadMetadata("RootPath", "/"),
        rootDirectory = ReadMetadata("directory", nil),
        targetFile = ReadMetadata("file", nil),
        invalid = true,
        success = false,
        playSound = false,
        originalSounds = {},
        canExport = ProgressModal ~= nil,
        currentID = 0,
        powerMetaSpriteId = FindMetaSpriteId("powerbuttonon")

    }
  
    pixelVisionOS:ResetUndoHistory(_sfxTool)
  
    -- Create a global reference of the new workspace tool
    setmetatable(_sfxTool, SFXTool)

    if(_sfxTool.rootDirectory ~= nil) then

        _sfxTool.targetFilePath = NewWorkspacePath(_sfxTool.targetFile)

        -- Load only the game data we really need
        _sfxTool.success = gameEditor.Load(_sfxTool.rootDirectory, {SaveFlags.System, SaveFlags.Sounds})

    end

    if(_sfxTool.success == true) then

        local pathSplit = string.split(_sfxTool.rootDirectory, "/")
    
        -- Update title with file path
        _sfxTool.toolTitle = pathSplit[#pathSplit] .. "/sounds.json"

        _sfxTool:CreateDropDownMenu()

        _sfxTool:CreateInstrumentPanel()

        _sfxTool:CreateSoundPanel()

        _sfxTool:CreateWaveformPanel()

        _sfxTool:CreateEditPanel()

        if(SessionID() == ReadSaveData("sessionID", "") and _sfxTool.rootDirectory == ReadSaveData("rootDirectory", "")) then
            _sfxTool.currentID = tonumber(ReadSaveData("currentID", "0"))
        end
      
        editorUI:ChangeNumberStepperValue(_sfxTool.channelIDStepper, 0)
    
        _sfxTool:LoadSound(_sfxTool.currentID, true, false)

    _sfxTool:ResetDataValidation()
    
    else

      pixelVisionOS:LoadError(_sfxTool.toolName)
    
    end

    -- Return the new instance of the editor ui
    return _sfxTool
  
end

function SFXTool:InvalidateData()

    -- Only everything if it needs to be
    if(self.invalid == true)then
      return
    end
  
    pixelVisionOS:ChangeTitle(self.toolTitle .."*", "toolbariconfile")
  
    pixelVisionOS:EnableMenuItem(self.SaveShortcut, true)
  
    self.invalid = true
  
  end
  
  function SFXTool:ResetDataValidation()
  
    -- Only everything if it needs to be
    if(self.invalid == false)then
      return
    end
  
    pixelVisionOS:ChangeTitle(self.toolTitle, "toolbariconfile")
    self.invalid = false
  
    pixelVisionOS:EnableMenuItem(self.SaveShortcut, false)
  
  end

  function SFXTool:Refresh()
    
    for i = 1, self.totalKnobs do
      
        local knob = self.knobData[i]
  
        local value = self.soundData[knob.propID] ~= "" and tonumber(self.soundData[knob.propID]) or 0
  
        local percent = ((knob.range - 1) + value) / knob.range
  
        self:UpdateKnobTooltip(knob, percent)
  
        editorUI:ChangeKnob(knob.knobUI, percent, false)
  
    end
  
    self:UpdateSoundTemplates()
  
    self:UpdateWaveButtons()
  
    self:UpdatePlayButton()
  
  end

function SFXTool:Draw()

    -- TODO this is not working
    if(gameEditor:IsChannelPlaying(0)) then
        DrawMetaSprite(self.powerMetaSpriteId, 16, 88)
      end

end
  

function SFXTool:Shutdown()

    -- Make sure all sounds are stopped before shutting down
    self:OnStopSound()
  
    -- Save the current session ID
    WriteSaveData("sessionID", SessionID())
  
    WriteSaveData("rootDirectory", self.rootDirectory)
  
    -- Make sure we don't save paths in the tmp directory
    WriteSaveData("currentID", self.currentID)
  
  end