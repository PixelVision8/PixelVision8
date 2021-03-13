--[[
	Pixel Vision 8 - Music Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

MusicTool = {}
MusicTool.__index = MusicTool

-- API Bridge
LoadScript("code-configure-modal")
LoadScript("code-song-slider")
LoadScript("code-progress-modal")
LoadScript("code-export-song")
LoadScript("code-song-panel")
LoadScript("code-tracker-panel")
LoadScript("code-keyboard-panel")
LoadScript("code-drop-down-menu")

function MusicTool:Invalidate()

  -- Only redraw the title when needed
  if(self.invalid == true)then
    return
  end

  pixelVisionOS:ChangeTitle(self.toolTitle .."*", "toolbariconfile")

  pixelVisionOS:EnableMenuItem(self.SaveShortcut, true)

  self.invalid = true

end

function MusicTool:ResetValidation()

  -- Only everything if it needs to be
  if(self.invalid == false)then
    return
  end

  pixelVisionOS:ChangeTitle(self.toolTitle, "toolbariconfile")

  pixelVisionOS:EnableMenuItem(self.SaveShortcut, false)

  self.invalid = false

end

function MusicTool:Init()

  local _musicTool = {
    toolName = "Music Editor",
    runnerName = SystemName(),
    rootPath = ReadMetadata("RootPath", "/"),
    rootDirectory = ReadMetadata("directory", nil),
    targetFile = ReadMetadata("file", nil),
    invalid = true,
    octave = 0,
    octaveRange = {x = 1, y = 8},
    
    currentPatternID = 0,
    playMode = -1,
    songScrollOffset = 0,
    currentSongID = 0,
    currentSongPatterns = nil,
    totalBeats = 32,
    currentTrack = -1,
    currentBeat = 0,
    currentNote = -1,
    disableWhenPlaying = {},
    disablePreview = true,
    currentSelectedSong = -1,
    configModal = nil,
    noteInClipboard = nil,
    SaveShortcut = 6,
    RevertShortcut = 7,
    UndoShortcut = 9,
    RedoShortcut = 10,
    ClearShortcut = 11,
    CopyShortcut = 12,
    PasteShortcut = 13,
    ExportPatternShortcut = 15,
    ExportSongShortcut = 16,
    success = false,
    totalTracks = 0,
    totalChannels = 0,
    songInputFields = {},
    totalSongFields = 9,
    whiteKeyHitRect = {x = 0, y = 24, w = 16, h = 24},
    blackKeyHitRect = {x = 2, y = 0, w = 12, h = 24},
    

  }

  pixelVisionOS:ResetUndoHistory(_musicTool)

  -- Create a global reference of the new workspace tool
  setmetatable(_musicTool, MusicTool)

  _musicTool:Configure()

  return _musicTool
  --
end

function MusicTool:Configure()

  if(self.rootDirectory ~= nil) then

    -- Load only the game data we really need
    self.success = gameEditor.Load(self.rootDirectory, {SaveFlags.System, SaveFlags.Sounds, SaveFlags.Music})

  end

  -- If data loaded activate the tool
  if(self.success == true) then

    
    self.configModal = ConfigureModal:Init()
    self.configModal.editorUI = editorUI
    
    local pathSplit = string.split(self.rootDirectory, "/")

    -- Update title with file path
    self.toolTitle = pathSplit[#pathSplit] .. "/" .. "music.json"

    self.totalTracks = gameEditor:TotalTracks()
    self.totalChannels = gameEditor:TotalChannels()

    self:CreateDropDownMenu()
    self:CreateSongPanel()
    self:CreateTrackerPanel()
    self:CreateKeyboardPanel()
    self:CreateSongSlider()
    
    self:SelectOctave(4)

    -- Build the default generator settings
    gameEditor:ConfigureGenerator()

    local startPatternID = 0
    local startSongID = 0
    local startTrackID = 0
    local startBeatID = 0
    local startScrollPos = 0

    -- Load in previous config data from save
    if(SessionID() == ReadSaveData("sessionID", "") and self.rootDirectory == ReadSaveData("self.rootDirectory", "")) then
      -- startSprite = tonumber(ReadSaveData("selectedSprite", "0"))

      gameEditor.pcgDensity = tonumber(ReadSaveData("configDensity", gameEditor.pcgDensity))
      gameEditor.pcgFunk = tonumber(ReadSaveData("configFunk", gameEditor.pcgFunk))
      gameEditor.pcgLayering = tonumber(ReadSaveData("configLayering", gameEditor.pcgLayering))
      gameEditor.pcgMinTempo = tonumber(ReadSaveData("configSpeedMin", gameEditor.pcgMinTempo))
      gameEditor.pcgMaxTempo = tonumber(ReadSaveData("configSpeedMax", gameEditor.pcgMaxTempo))
      gameEditor.scale = tonumber(ReadSaveData("configScale", gameEditor.scale))

      local previousState = string.split(ReadSaveData("state", "0,0,0,0"), ",")

      startSongID = tonumber(previousState[1])
      startTrackID = tonumber(previousState[2])
      startBeatID = tonumber(previousState[3])
      startPatternID = tonumber(previousState[4])
      startScrollPos = tonumber(previousState[5] or "0")

      local total = self.totalTracks

      for i = 1, total do

        local trackID = i - 1
        local trackData = ReadSaveData("configTrack" .. tostring(trackID), nil)

        if(trackData ~= nil) then
          -- print("Parse Track", trackID, "Data", trackData)
          local values = ExplodeSettings(trackData)
          gameEditor:ConfigTrackSFX(trackID, values[1])
          gameEditor:ConfigTrackInstrument(trackID, values[2])
          gameEditor:ConfigTrackOctaveRange(trackID, NewPoint(values[3], values[4]))

        end

      end

    end

    -- TODO Need to set the song to 0 or it will fail to load correctly
    self:OnSongIDChange(0)

    -- Change the number stepper to the start song ID
    editorUI:ChangeNumberStepperValue(self.songIDStepper, startSongID)

    self:SelectTrack(startTrackID)
    self:SelectBeat(startBeatID)

    self:OnSelectSongField(startPatternID)

    self:ChangeSongSlider(startScrollPos)
   
    self:ResetValidation()

    pixelVisionOS:RegisterUI({name = "UpdateMusicToolLoop"}, "UpdateMusicTool", self)


  else

    pixelVisionOS:LoadError(self.toolName)
    
  end

end

  
-- Select an octave
function MusicTool:SelectOctave(value)

  -- octave = Repeat(value, self.octaveRange.y)
  self.octave = Clamp(value, self.octaveRange.x, self.octaveRange.y)

  for i = 1, #self.keyPositions do

    local data = self.keyPositions[i]

    local keyButton = self.keys[i]
    keyButton.note = data.note + (12 * self.octave)

    keyButton.toolTip = "Note " .. self:ConvertMidiToNote(keyButton.note) .. " (Midi ".. keyButton.note ..")."

  end

  editorUI:ChangeNumberStepperValue(self.octaveStepper, self.octave, false)

end


function MusicTool:OnSongIDChange(value)

  self.currentSongID = tonumber(value)

  -- Change the label value but don't trigger the callback
  editorUI:ChangeInputField(self.songNameInputData, gameEditor:SongName(self.currentSongID), false)

  self.songStartPos = gameEditor:SongStart(self.currentSongID)
  self.songEndPos = gameEditor:SongEnd(self.currentSongID)

  self.currentSongPatterns = gameEditor:SongPatterns(self.currentSongID)

  editorUI:ChangeSlider(self.songSliderData, 0)

  -- Force the song editor to render since slider may be ignored if set to 0 position in previuos song
  self:OnSongScroll(0)

  -- TODO select the first pattern
  editorUI:SelectSongInputField(self.songInputFields[1], true)

end

function MusicTool:SetMidiNote(value)

  -- Calculate note from octave
  local realNote = value + (12 * self.octave)

  self:UpdateCurrentNote(realNote)

  local nextBeat = Repeat((editorUI:GetNumberStepperValue(self.incrementStepper) + self.currentBeat), self.totalBeats)

  self:SelectBeat(nextBeat)

end

function MusicTool:OnLoopID(text)

  local value = tonumber(text)

  local realIndex = self.currentSelectedSong - self.songScrollOffset

  if(realIndex > 0 and realIndex < self.totalSongFields) then

    editorUI:ChangeInputField(self.songInputFields[realIndex], value, true)
  else
    gameEditor:SongPatternAt(self.currentSongID, self.currentSelectedSong - 1, value)
    self:Invalidate()
  end

  self:LoadLoop(value)

end

-- Load a song into memory
function MusicTool:LoadLoop(id)

  -- Only load a new loop if the current song does not match the new id
  if(self.currentPatternID == value) then
    return
  end

  -- Need to make sure we load the song through the editor bridge
  gameEditor:LoadLoop(id)

  self.currentPatternID = id

  self.totalBeats = gameEditor:NotesPerTrack()

  editorUI:ChangeNumberStepperValue(self.patternLengthStepper, self.totalBeats, false)

  local isPlaying = gameEditor:SongPlaying()

  -- Update the input field but don't trigger the callback since it calls load song.
  editorUI:ChangeNumberStepperValue(self.patternIDStepper, tostring(id), false, not isPlaying)
  editorUI:ChangeNumberStepperValue(self.tempoStepper, tostring(gameEditor:Tempo()), false, not isPlaying)

  -- Redraw each track
  self:DrawAllTracks()

  self.disablePreview = true
  
  -- Select the first note and populate all of the fields
  editorUI:SelectPicker(self.pickerData, 0)

  self.disablePreview = false

end

-- Preview the currently selected note
function MusicTool:PlayCurrentNote()

  -- Find the current track and beat
  -- local realTrack = self.currentTrack
  local note = gameEditor:Note(self.currentTrack, self.currentBeat)

  -- If the note is not empty, attempt to play it through the gameEditor
  if(note > 0) then
    gameEditor:PlayNote(self.currentTrack, note)
  end
end


function MusicTool:UpdateMusicTool(timeDelta)
  

    local controlDown = (Key(Keys.LeftControl) == true or Key(Keys.RightControl) == true)

    -- Increment octave
    if( controlDown == false and editorUI.editingInputField == false) then
      if(Key(Keys.OemOpenBrackets, InputState.Released) and self.octave > 1) then
        self:PreviousOctave()
      elseif(Key(Keys.OemCloseBrackets, InputState.Released) and self.octave < 8) then
        self:NextOctave()
      elseif(Key(Keys.Delete, InputState.Released) or Key(Keys.Back, InputState.Released)) then
        self:EraseNote()
      end
    end

  -- end

  
end

-- Rewind the song
function MusicTool:RewindSong()

  -- TODO need to move the song scroller to the correct position
  self:SelectBeat(0)
end

-- Update the currently selected note with a new value
function MusicTool:UpdateCurrentNote(value, saveHistory)

  local oldNote = self.currentNote

  gameEditor:Note(self.currentTrack, self.currentBeat, value)

  self:DrawNote(self.currentTrack, self.currentBeat, true, false)

  self:PlayCurrentNote()

  if(self.playMode == 1 and self.currentBeat < self.totalBeats) then
    self:SelectBeat(self.currentBeat + 1)
  end

  self:Invalidate()

end

function MusicTool:UpdateNoteFromFields(value)

  if(value == "") then
    self:EraseNote()
  else
    -- local note = 0
    local note = self:ConvertNoteToMidi(editorUI:GetStringStepperValue(self.noteStepper)) - 1
    local octave = note == -1 and 0 or editorUI:GetNumberStepperValue(self.octaveStepper)

    local newNote = note + (12 * octave)

    self:UpdateCurrentNote(newNote)

  end

end

function MusicTool:Shutdown()
  -- Save the current session ID
  WriteSaveData("sessionID", SessionID())
  WriteSaveData("self.rootDirectory", self.rootDirectory)

  WriteSaveData("state", tostring(self.currentSongID) .. "," .. tostring(self.currentTrack) .. "," .. tostring(currentBeat) .. "," .. tostring(self.currentSelectedSong - 1) .. "," .. tostring(self.songSliderData.value))

  WriteSaveData("configDensity", tostring(gameEditor.pcgDensity))
  WriteSaveData("configFunk", tostring(gameEditor.pcgFunk))
  WriteSaveData("configLayering", tostring(gameEditor.pcgLayering))
  -- Increase scale by 1 to account for Lua 1 base array index
  WriteSaveData("configScale", tostring(gameEditor.scale))
  WriteSaveData("configSpeedMin", tostring(gameEditor.pcgMinTempo))
  WriteSaveData("configSpeedMax", tostring(gameEditor.pcgMaxTempo))

  local total = self.totalTracks

  for i = 1, total do

    local trackID = i - 1

    local sfx = gameEditor:ConfigTrackSFX(trackID)
    local instrument = gameEditor:ConfigTrackInstrument(trackID)
    local octRange = gameEditor:ConfigTrackOctaveRange(trackID)

    WriteSaveData("configTrack" .. tostring(trackID), tostring(sfx) .. "," .. tostring(instrument) .. "," .. tostring(octRange.x) .. "," .. tostring(octRange.y))

  end

end


-- Generate a new song
function MusicTool:GenerateSong()

  pixelVisionOS:ShowSaveModal("Generate Music", "This will clear the current tracker, generate a new random song, and reset some of your sound effects to default musical instruments. Are you sure you want to do this?", 160,
    -- Accept
    function(target)

      pixelVisionOS:BeginUndo(self)

      target.onParentClose()
      gameEditor:GenerateSong(true)
      self:LoadLoop(self.currentPatternID)
      self:Invalidate()

      pixelVisionOS:EndUndo(self)

    end,
    -- Decline
    function (target)

      pixelVisionOS:BeginUndo(self)

      target.onParentClose()
      gameEditor:GenerateSong(false)
      self:LoadLoop(self.currentPatternID)
      self:Invalidate()

      pixelVisionOS:EndUndo(self)
    end,
    -- Cancel
    function(target)
      -- print("Cancle")
      target.onParentClose()
    end
  )

end