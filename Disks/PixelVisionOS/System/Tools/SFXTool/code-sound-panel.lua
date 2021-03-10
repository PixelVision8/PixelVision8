function SFXTool:CreateSoundPanel()
    self.soundData = {}
    self.currentChannelType = -1

    self.validWaves = {
        "!\"", -- any
        "%&", -- square
        ":;", -- saw tooth
        -- "'(", -- sine (Not enabled by default)
        ")*", -- noise
        "<=", -- triangle
        "./" -- wave
    }

    self.waveTypeIDs = {
        -1,
        0,
        1,
        -- 2,
        3,
        4,
        5
    }

    -- Get the total number of songs
    self.totalSounds = gameEditor:TotalSounds()
    self.totalChannels = gameEditor:TotalChannels()

    self.soundIDStepper = editorUI:CreateNumberStepper({x = 32, y = 16}, 16, 0, 0, self.totalSounds - 1, "top", "Sound ID value.")
    self.soundIDStepper.onInputAction = function(value) self:OnChangeSoundID(value) end

    self.songNameFieldData = editorUI:CreateInputField({x = 88, y = 24, w = 80}, "Untitled", "Change the label of the selected sound.", "name")
    self.songNameFieldData.onAction = function(value) self:OnChangeName(value) end

    self.channelIDStepper = editorUI:CreateNumberStepper({x = 176, y = 16}, 8, -1, 0, gameEditor:TotalChannels()- 1, "top", "The channel sound effects will be previewed on.")
    self.channelIDStepper.onInputAction = function(value) self:OnChangeChannelID(value) end

    self.waveInputField = editorUI:CreateInputField({x = 224, y = 24, w = 16}, self.validWaves[1], "Wave type.", nil, nil)
    self.waveInputField.editable = false

    self.playButton = editorUI:CreateButton({x = 8, y = 16}, "playbutton", "Play the current sound.")
    self.playButton.onAction = function(value) self:OnPlaySound() end

    pixelVisionOS:RegisterUI({name = "UpdateSoundPanel"}, "UpdateSoundPanel", self)

end

function SFXTool:UpdateSoundPanel()

    editorUI:UpdateStepper(self.soundIDStepper)
    editorUI:UpdateStepper(self.channelIDStepper)

    editorUI:UpdateInputField(self.waveInputField)
    editorUI:UpdateInputField(self.songNameFieldData)

    editorUI:UpdateButton(self.playButton)

end

function SFXTool:CurrentSoundID()
    return tonumber(self.soundIDStepper.inputField.text)
end


function SFXTool:OnChangeSoundID(text)

    -- convert the text value to a number
    local value = tonumber(text)
  
    -- Load the sound into the editor
    self:LoadSound(value, true, false)
  
  end

function SFXTool:LoadSound(value)
  -- print("Load Sound Clear", clearHistory)
  self.currentID = value

  local data = gameEditor:Sound(value)

  if(self.originalSounds[value] == nil) then
    -- Make a copy of the sound
    self.originalSounds[value] = data
  end

  -- Load the current sounds string data so we can edit it
  self.soundData = {}

  local tmpValue = ""

  for i = 1, #data do
    local c = data:sub(i, i)

    if(c == ",") then

      table.insert(self.soundData, tmpValue)
      tmpValue = ""

    else
      tmpValue = tmpValue .. c

    end

  end

  -- Always add the last value since it doesn't end in a comma
  table.insert(self.soundData, tmpValue)

  self:Refresh()

  local label = gameEditor:SoundLabel(value)

  editorUI:ChangeInputField(self.songNameFieldData, label, false)
  editorUI:ChangeNumberStepperValue(self.soundIDStepper, self.currentID, false, true)

  -- if(clearHistory == true) then
  --   -- Reset the undo history so it's ready for the tool
  --   -- pixelVisionOS:ResetUndoHistory()
  --   -- UpdateHistoryButtons()
  --   -- TODO Undo logic here too that needs to be restored
  -- end

  -- if(updateHistory ~= false) then
  --     self:UpdateHistory(data)
  -- end

  -- TODO need to refresh the editor panels
end

  function SFXTool:OnChangeChannelID(value)

    self.currentChannelType = gameEditor:ChannelType(tonumber(value))
  
    local validWaveID = table.indexOf(self.waveTypeIDs, self.currentChannelType)
  
    editorUI:ChangeInputField(self.waveInputField, self.validWaves[validWaveID])
  
    self:UpdatePlayButton()
  
  end

  function SFXTool:OnPlaySound()

    if(self.playButton.enabled == false) then
      return
    end

    local channel = tonumber(self.channelIDStepper.inputField.text)
    -- print("Play Sound")
    gameEditor:StopSound(channel)
    gameEditor:PlaySound(self:CurrentSoundID(), channel)
  end

  function SFXTool:UpdatePlayButton()

    local enablePlay = true
    local isWav = gameEditor:IsWav(self.currentID)
  
    if(isWav) then --currentChannelType > - 1 or currentChannelType < 5) then
      enablePlay = (self.currentChannelType < 0 or self.currentChannelType > 4)
    elseif(self.currentChannelType == 5) then
      enablePlay = false
    end
  
    -- Enable play button
    editorUI:Enable(self.playButton, enablePlay)
  
  end

  function SFXTool:OnChangeName(value)

    local id = self:CurrentSoundID()
  
    local label = gameEditor:SoundLabel(id, value)
  
    self:Refresh()
  
    self:InvalidateData()
  
  end

  function SFXTool:OnStopSound()

    gameEditor:StopSound()
  
  end