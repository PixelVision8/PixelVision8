function MusicTool:CreateTrackerPanel()

    self.midiTable = 
    {
    "C", -- 24
    "C#", -- 25
    "D", -- 26
    "D#", -- 27
    "E", -- 28
    "F", -- 29
    "F#", -- 30
    "G", -- 31
    "G#", -- 32
    "A", -- 33
    "A#", -- 34
    "B", -- 35
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

    self.validNotes = {
        "",
        "C",
        "C#",
        "D",
        "D#",
        "E",
        "F",
        "F#",
        "G",
        "G#",
        "A",
        "A#",
        "B",
      }

    self.validWaves = {
        "!\"", -- any
        "%&", -- square
        ":;", -- saw tooth
        -- "'(", -- sine (Not enabled by default)
        ")*", -- noise
        "<=", -- triangle
        "./" -- wave
      }

      -- Pattern input field
    self.patternIDStepper = editorUI:CreateNumberStepper({x = 8, y = 88}, 16, 0, 0, gameEditor:TotalLoops() - 1, "top", "Change the length of the pattern.")

    self.patternIDStepper.onInputAction = function(value) self:OnLoopID(value) end

    -- Need to manually register each component to disable
    table.insert(self.disableWhenPlaying, self.patternIDStepper.backButton)
    table.insert(self.disableWhenPlaying, self.patternIDStepper.inputField)
    table.insert(self.disableWhenPlaying, self.patternIDStepper.nextButton)

    self.muteToggleButton = editorUI:CreateToggleButton({x = 8, y = 168}, "checkbox", "Mute the track while playing.")

    table.insert(self.disableWhenPlaying, self.muteToggleButton)

    self.muteToggleButton.onAction = function(value)

      gameEditor:MuteTrack(self.currentTrack, not value)

      -- Force the track to redraw
      self:DrawTrack(self.currentTrack, true)

    end

    self.waveInputField = editorUI:CreateInputField({x = 72, y = 168, w = 16}, self.validWaves[1], "Wave type.")

    table.insert(self.disableWhenPlaying, self.waveInputField)

    self.waveInputField.editable = false

    self.trackStepper = editorUI:CreateNumberStepper({x = 24, y = 152 + 8}, 8, 0, 0, gameEditor:TotalChannels() - 1, "top", "Change the currently selected track.")
    self.trackStepper.onInputAction = function(value) self:SelectTrack(tonumber(value)) end

    table.insert(self.disableWhenPlaying, self.trackStepper.backButton)
    table.insert(self.disableWhenPlaying, self.trackStepper.inputField)
    table.insert(self.disableWhenPlaying, self.trackStepper.nextButton)

    self.octaveStepper = editorUI:CreateNumberStepper({x = 208, y = 152 + 8}, 8, 0, self.octaveRange.x, self.octaveRange.y, "top", "Change the length of the pattern.")

    self.octaveStepper.onInputAction = function(value) self:UpdateNoteFromFields(value) end

    table.insert(self.disableWhenPlaying, self.octaveStepper.backButton)
    table.insert(self.disableWhenPlaying, self.octaveStepper.inputField)
    table.insert(self.disableWhenPlaying, self.octaveStepper.nextButton)

    self.tempoStepper = editorUI:CreateNumberStepper({x = 120, y = 88}, 24, 0, 1, 480, "top", "Change the length of the pattern.")
    self.tempoStepper.onInputAction = function(value) self:OnTempoChange(value) end

    table.insert(self.disableWhenPlaying, self.tempoStepper.backButton)
    table.insert(self.disableWhenPlaying, self.tempoStepper.inputField)
    table.insert(self.disableWhenPlaying, self.tempoStepper.nextButton)

    self.incrementStepper = editorUI:CreateNumberStepper({x = 200, y = 88}, 8, 1, 0, 8, "top", "Change the length of the pattern.")

    table.insert(self.disableWhenPlaying, self.incrementStepper.backButton)
    table.insert(self.disableWhenPlaying, self.incrementStepper.inputField)
    table.insert(self.disableWhenPlaying, self.incrementStepper.nextButton)

    

    self.instrumentStepper = editorUI:CreateNumberStepper({x = 96, y = 152 + 8}, 16, 0, 0, gameEditor:TotalSounds(), "top", "Change the length of the pattern.")
    self.instrumentStepper.onInputAction = function(value) self:OnSFXChange(value) end

    table.insert(self.disableWhenPlaying, self.instrumentStepper.backButton)
    table.insert(self.disableWhenPlaying, self.instrumentStepper.inputField)
    table.insert(self.disableWhenPlaying, self.instrumentStepper.nextButton)


    self.noteStepper = editorUI:CreateStringStepper({x = 152, y = 152 + 8}, 16, "", self.validNotes, "top", "Enter the note to play.")

    self.noteStepper.inputField.forceCase = "upper"
    self.noteStepper.inputField.allowEmptyString = true

    self.noteStepper.onInputAction = function(value) self:UpdateNoteFromFields(value) end

    table.insert(self.disableWhenPlaying, self.noteStepper.backButton)
    table.insert(self.disableWhenPlaying, self.noteStepper.inputField)
    table.insert(self.disableWhenPlaying, self.noteStepper.nextButton)

    self.pickerData = editorUI:CreatePicker({x = 0, y = 112, w = 256, h = (8 * self.totalTracks)}, 8, 8, self.totalBeats * self.totalTracks, "notepicker", 0, "Select a track and beat to preview a note value.")
    self.pickerData.overShift = 0
    table.insert(self.disableWhenPlaying, self.pickerData)

    self.pickerData.borderOffset = 0

    self.pickerData.onAction = function(value) self:OnPickerSelection(value) end

    self.patternLengthStepper = editorUI:CreateNumberStepper({x = 64, y = 88}, 16, 32, 4, gameEditor:NotesPerTrack(), "top", "Change the length of the pattern.")

    editorUI:EnableStepper(self.patternLengthStepper, false)

    self.patternLengthStepper.stepperValue = 4
    self.patternLengthStepper.inputField.onValidate = function (value)

      local validValues = {4, 8, 12, 16, 20, 24, 28, 32}

      value = tonumber(value)

      for i = 1, #validValues do

        if(validValues[i] == value) then
          return value
        end

      end

      return self.patternLengthStepper.inputField.defaultValue

    end

    self.patternLengthStepper.onInputAction = function(value)
      gameEditor:NotesPerTrack(tonumber(value))
    end

    pixelVisionOS:RegisterUI({name = "UpdateTrackerPanelLoop"}, "UpdateTrackerPanelPanel", self)
  
  end
  
  function MusicTool:UpdateTrackerPanelPanel()
  
    editorUI:UpdateButton(self.muteToggleButton)
    editorUI:UpdateInputField(self.waveInputField)
    editorUI:RefreshNumberStepper(self.trackStepper)
    editorUI:UpdateStepper(self.octaveStepper)
    editorUI:UpdateStepper(self.patternIDStepper)
    editorUI:UpdateStepper(self.patternLengthStepper)
    editorUI:UpdateStepper(self.tempoStepper)
    editorUI:UpdateStepper(self.incrementStepper)
    editorUI:UpdateStepper(self.instrumentStepper)
    editorUI:UpdateStepper(self.noteStepper)
    editorUI:UpdateStepper(self.trackStepper)

    editorUI:UpdatePicker(self.pickerData)

    -- Need to update the game editor's music chip so it plays
    if(gameEditor:SongPlaying() == true) then

        -- Call update on the Game Editor's music chip to move the sequencer
        gameEditor:UpdateSequencer(editorUI.timeDelta * 1000)
  
        local songData = gameEditor:ReadSongData()
  
        local currentPattern = songData["pattern"]
  
        if((self.currentSelectedSong - 1) ~= currentPattern and self.playMode ~= 3) then
          self:OnSelectSongField(currentPattern - self.songScrollOffset)
          editorUI.refreshTime = 0
        end
  
        -- TODO need to put this cap of under 32 in to make sure the array didn't go out of bounds
        if(editorUI.refreshTime == 0 and songData["note"] < 32) then
          -- print("note", songData["note"])
          self:SelectBeat(songData["note"])
        end
    end

    -- Need to update the game editor's music chip so it plays
    if(gameEditor:SongPlaying() == false) then

      local newPos = NewPoint(0, 0)

      if(editorUI.editingInputField == false) then
      -- Offset the new position by the direction button
      if(Key(Keys.Up, InputState.Released)) then
          newPos.y = -1
      elseif(Key(Keys.Right, InputState.Released)) then
          newPos.x = 1
      elseif(Key(Keys.Down, InputState.Released)) then
          newPos.y = 1
      elseif(Key(Keys.Left, InputState.Released)) then
          newPos.x = -1
      end
      end
      -- Test to see if the new position has changed
      if(newPos.x ~= 0 or newPos.y ~= 0) then

      self:SelectBeat(self.currentBeat + newPos.x)
      self:SelectTrack(self.currentTrack + newPos.y)

      end

    end
    
  end

  -- Select the track
function MusicTool:SelectTrack(id)

    -- Only update if the current track is not the same as the new ID
    if(self.currentTrack ~= id) then
  
      -- Make sure the track ID is never greater than the total number of tracks
      id = Repeat(id, self.totalTracks)
  
      if(self.currentTrack > - 1 and self.currentTrack < 5) then
        -- Redraw the current track so it doesn't look selected
        self:DrawTrack(self.currentTrack, false)
      end
  
      local currentChannelType = gameEditor:ChannelType(id)
  
      local validWaveID = table.indexOf(self.waveTypeIDs, currentChannelType)
  
      editorUI:ChangeInputField(self.waveInputField, self.validWaves[validWaveID])
  
  
      -- local type = gameEditor:ChannelType(id) + 2
      --
      -- editorUI:ChangeStringStepperValue(waveStepper, validWaves[type], false, true)
  
      -- Save the new track value
      self.currentTrack = id
  
      -- Redraw the track
      self:DrawTrack(self.currentTrack, true)
  
      editorUI:ToggleButton(self.muteToggleButton, not gameEditor:MuteTrack(self.currentTrack), false)
  
      -- We need to save the current beat then clear the current beat so the beat updates correctly on the new track
      local beat = self.currentBeat
      self.currentBeat = nil
  
      -- Select the current beat but now it is on the new track
      self:SelectBeat(beat)
  
      -- TODO need to move the update logic into it's own block of code
      -- editorUI:ChangeInputField(trackInputData, tostring(self.currentTrack), false)
  
      editorUI:ChangeNumberStepperValue(self.trackStepper, id, false)
  
      editorUI:ChangeNumberStepperValue(self.instrumentStepper, gameEditor:TrackInstrument(self.currentTrack), false)
  
    end
  
  end
  
  function MusicTool:DrawAllTracks()
  
    -- Get the current number of tracks from the input field
    -- local total = tonumber(self.totalTracksInputData.text) - 1
    for i = 0, self.totalChannels - 1 do
      self:DrawTrack(i, i == self.currentTrack)
      -- TODO need to see if the track is disabled
    end
  end
  
  -- Draw the track to the display
  function MusicTool:DrawTrack(track, selected)
  
    local muted = gameEditor:MuteTrack(track)
    local hasNote = false
    local disabled = track >= self.totalTracks
    for i = 0, self.totalBeats - 1 do
  
      self:DrawNote(track, i, selected, disabled, muted)
    end
  end
  
  -- Test to see if a note exists
  function HasNote(track, beat)
    return (gameEditor:Note(track, beat) > 0)
  end
  
  -- Draw the note to the display
  function MusicTool:DrawNote(track, beat, selected, disabled, muted)
    local bar = beat % 4 == 3 and 1 or 0
    local hasNote = HasNote(track, beat)
  
    local spriteName = "trackbar"..bar.."disabled"
  
    if(disabled == false) then
  
      spriteName = GenerateNoteSpriteName(muted == true and 4 or track, bar, hasNote, selected)
  
    end
  
    local spriteId = FindMetaSpriteId(spriteName)
  
    if(spriteId > -1)then
      DrawMetaSprite(spriteId, beat, track + self.pickerData.tiles.r, false, false, DrawMode.Tile)
    end
  
  end
  
  -- This generates the correct sprite to use for a note based on the naming convention
  function GenerateNoteSpriteName(track, bar, note, selected)
    local name = "track"
  
    if(note == true) then
      name = name .. (track + 1)
    end
  
    name = name .. "bar" .. tostring(bar)
    name = name .. (note == true and "note" or "none")
    name = name .. (selected == true and "selected" or "notselected")
  
    return name
  end
  
  -- Select a beat
  function MusicTool:SelectBeat(id)
  
    self.currentBeat = gameEditor:CurrentBeat(id)
  
    self.currentNote = gameEditor:Note(self.currentTrack, self.currentBeat)
  
    pixelVisionOS:EnableMenuItem(self.CopyShortcut, self.currentNote > 0)
    pixelVisionOS:EnableMenuItem(self.ClearShortcut, self.currentNote > 0)
  
    local playing = gameEditor:SongPlaying()
  
    editorUI:ChangeStringStepperValue(self.noteStepper, self:ConvertMidiToNote(self.currentNote), false, not playing)
  
    local tmpOctave = 0

    if(self.currentNote == 0) then
    
      tmpOctave = self.octave
    else
      tmpOctave = self.currentNote / 12
    end

    editorUI:ChangeNumberStepperValue(self.octaveStepper, math.floor(tmpOctave), false, not playing)
  
    local hasNote = HasNote(self.currentTrack, self.currentBeat)
  
    editorUI:SelectPicker(self.pickerData, CalculateIndex(self.currentBeat, self.currentTrack, 32), false)
  
    -- end
  end
  
  
  function MusicTool:ConvertMidiToNote(midiValue)
  
    if(midiValue == 0) then
      return ""
    end
  
    local offset = midiValue - 24
  
  
    -- TODO this is missing the last value, need to look into the calculation
  
    -- print("midiValue", midiValue, offset, offset % #midiTable, midiTable[(offset % #midiTable) + 1])
    -- self.currentNote
  
    return self.midiTable[(offset % #self.midiTable) + 1]
  end
  
  function MusicTool:ConvertNoteToMidi(note)
  
    local value = table.indexOf(self.midiTable, note)
  
    if(value == -1) then
      return 0
    else
      return value
    end
  
  end

 
function MusicTool:OnPickerSelection(value)

  -- Calculate the beat and track
  local beat, track = math.pos(value, self.pickerData.columns)

  -- Select the correct track
  self:SelectTrack(track)

  -- Select the correct beat
  self:SelectBeat(beat)

  local note = gameEditor:Note(self.currentTrack, self.currentBeat)

  if(note > 0 and self.disablePreview ~= true) then
    gameEditor:PlayNote(self.currentTrack, note)
  end

  -- TODO Hack to make sure we reset the preview in the event it was disabled and not enabled again on the next selection
  self.disablePreview = false

end
