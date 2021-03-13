function MusicTool:CreateSongPanel()

    -- This is capped at 10 songs since the UI can only support numbers 0 - 9
    self.songIDStepper = editorUI:CreateNumberStepper({x = 104, y = 16}, 16, 0, 0, gameEditor:TotalSongs() - 1, "top", "Load a new song into memory.")

    self.songIDStepper.onInputAction = function(value) self:OnSongIDChange(value) end

    -- Need to manually register each component to disable
    table.insert(self.disableWhenPlaying, self.songIDStepper.backButton)
    table.insert(self.disableWhenPlaying, self.songIDStepper.inputField)
    table.insert(self.disableWhenPlaying, self.songIDStepper.nextButton)

    -- Song name input field
    self.songNameInputData = editorUI:CreateInputField({x = 160, y = 24, w = 80}, "Untitled", "Enter a label for this song.", "file")

    self.songNameInputData.onAction = function(value) self:OnLabelChange(value) end

    table.insert(self.disableWhenPlaying, self.songNameInputData)

    

    self.songEndButtons = editorUI:CreateToggleGroup(true)
    self.songEndButtons.onAction = function(id, value)
      -- print("Toggle new button", id)

      -- TODO need to account for scroll offset
      self.songEndPos = gameEditor:SongEnd(self.currentSongID, (id - 1) + self.songScrollOffset)

      if(self.currentSelectedSong > self.songEndPos) then
        self.currentSelectedSong = self.songEndPos

        self:OnSelectSongField(self.currentSelectedSong - self.songScrollOffset - 1)
      end

      -- TODO need to redraw the song input field
      self:OnSongScroll()

      self:Invalidate()
    end

    -- First sone end button
    self.firstSongEndButton = editorUI:ToggleGroupButton(self.songEndButtons, {x = 16, y = 48, w = 8, h = 8}, "songend", "Set the end of the song.", true)

    self.firstSongEndButton.cachedMetaSpriteIds.disabled = FindMetaSpriteId("songstart")
    -- self.songEndButtons =

    for i = 1, self.totalSongFields do
      local data = editorUI:CreateInputField({x = 24 + ((i - 1) * 24), y = 48, w = 16}, "00", "Enter a pattern.", "number")

      data.min = 0
      data.max = gameEditor:TotalLoops() - 1

      data.index = i - 1

      data.onSelected = function(value) self:OnSelectSongField(value) end

      data.onAction = function(value)

        -- Convert to number
        value = tonumber(value)

        -- Update the pattern at the correct position in the song
        gameEditor:SongPatternAt(self.currentSongID, (data.index + self.songScrollOffset), value)

        self:LoadLoop(tonumber(self.songInputFields[self.currentSelectedSong - self.songScrollOffset].text))

        -- Invalidate the data
        self:Invalidate()

      end

      table.insert(self.songInputFields, data)

      -- Create song end button
      editorUI:ToggleGroupButton(self.songEndButtons, {x = 40 + ((i - 1) * 24), y = 48, w = 8, h = 8}, "songend", "Set the end of the song.", true)

    end

    self.generateButtonData = editorUI:CreateButton({x = 8, y = 16}, "toolgenbutton", "Randomly generate a new song.")
    self.generateButtonData.onAction = function() self:GenerateSong() end
    
    table.insert(self.disableWhenPlaying, self.generateButtonData)
    
    --
    -- Toggle group for playback buttons
    self.playToggleGroupData = editorUI:CreateToggleGroup(true)
    self.playToggleGroupData.onAction = function(value) self:OnChangePlayMode(value) end


    self.playFromStartButtonData = editorUI:ToggleGroupButton(self.playToggleGroupData, {x = 32, y = 16, w = 16, h = 16}, "toolrewindbutton", "Play song from start.")
    self.playSingleButtonData = editorUI:ToggleGroupButton(self.playToggleGroupData, {x = 48, y = 16, w = 16, h = 16}, "toolplaybutton", "Play song from current pattern.")
    self.playLoopButtonData = editorUI:ToggleGroupButton(self.playToggleGroupData, {x = 64, y = 16, w = 16, h = 16}, "toolloopbutton", "Play and loop the current pattern.")

    -- Stop and rewind buttons
    self.stopButtonData = editorUI:CreateButton({x = 80, y = 16}, "toolstopbutton", "Stop playback.")
    self.stopButtonData.onAction = function() self:OnStop() end

    editorUI:Enable(self.stopButtonData, false)

    pixelVisionOS:RegisterUI({name = "UpdateSongPanelLoop"}, "UpdateSongPanel", self)
  
  end
  
  function MusicTool:UpdateSongPanel()
  
    editorUI:UpdateToggleGroup(self.songEndButtons)
    editorUI:UpdateInputField(self.songNameInputData)
    editorUI:UpdateStepper(self.songIDStepper)
    editorUI:UpdateButton(self.generateButtonData)
    editorUI:UpdateButton(self.stopButtonData)

    editorUI:UpdateToggleGroup(self.playToggleGroupData)

  end

  function MusicTool:OnChangePlayMode(value)

    self.playMode = value
  
    if(self.playMode == 1)then
  
      self:RewindSong()
  
      gameEditor:PlaySong(self.currentSongID)
  
      editorUI:Enable(self.stopButtonData, true)
  
    elseif(self.playMode == 2) then
  
      self:RewindSong()
  
      -- TODO need to pass in the current start position
      gameEditor:PlaySong(self.currentSongID, true, self.currentSelectedSong - 1)
  
      editorUI:Enable(self.stopButtonData, true)
  
    elseif(self.playMode == 3)then
  
      self:RewindSong()
  
      -- TODO need to play the current selected song pattern
      gameEditor:PlayPattern(self.currentPatternID)
  
      editorUI:Enable(self.stopButtonData, true)
  
    end
  
    -- Disable UI when playing back
    local total = #self.disableWhenPlaying
    for i = 1, total do
      editorUI:Enable(self.disableWhenPlaying[i], false)
    end
  
  end

  function MusicTool:OnLabelChange(text)

    -- TODO need to rename the loop in the game editor
    gameEditor:SongName(self.currentSongID, text)
  
    self:Invalidate()
  end
  
  function MusicTool:OnTempoChange(text)
  
    local value = tonumber(text)
  
    gameEditor:Tempo(value)
  
    self:Invalidate()
  
  end
  
  function MusicTool:OnSFXChange(text)
  
    local value = tonumber(text)
    gameEditor:ConfigTrackSFX(self.currentTrack, value)
  
    self:PlayCurrentNote()
  
    self:Invalidate()
  
  end
  
  function MusicTool:OnStop()
  
    -- Clear play mode
    if(self.playMode > 0) then
      self.playMode = 0
    end
  
    -- Reset the playback toggle
    editorUI:ClearGroupSelections(self.playToggleGroupData)
    -- StopSong()
  
    if(gameEditor:SongPlaying() == true) then
      gameEditor:StopSequencer()
    end
  
    local total = #self.disableWhenPlaying
    for i = 1, total do
      editorUI:Enable(self.disableWhenPlaying[i], true)
    end
  
    editorUI:Enable(self.playFromStartButtonData, true)
    editorUI:Enable(self.playSingleButtonData, true)
    editorUI:Enable(self.playLoopButtonData, true)
    editorUI:Enable(self.stopButtonData, false)
  
    
  
    editorUI:RefreshNumberStepper(self.patternIDStepper)
    editorUI:RefreshNumberStepper(self.tempoStepper)
    editorUI:RefreshNumberStepper(self.incrementStepper)
  
    editorUI:RefreshNumberStepper(self.instrumentStepper)
    editorUI:RefreshStringStepper(self.noteStepper)
    -- editorUI:RefreshStringStepper(waveStepper)
    editorUI:RefreshNumberStepper(self.octaveStepper)
    editorUI:RefreshNumberStepper(self.songIDStepper)
  
  end