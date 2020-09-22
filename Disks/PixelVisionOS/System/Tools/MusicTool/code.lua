--[[
	Pixel Vision 8 - Music Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- API Bridge
LoadScript("sb-sprites.lua")
LoadScript("pixel-vision-os-v2")
LoadScript("pixel-vision-os-music-configure-modal-v1")
LoadScript("code-song-input-field")
LoadScript("code-progress-modal")

local toolName = "Music Editor"
configModal = nil
local octaveRange = {x = 1, y = 8}
local playMode = -1
local songScrollOffset = 0
local noteInClipboard = nil
currentPatternID = 0
local currentSongID = 0
local currentSongPatterns = nil
local totalBeats = 32
local currentTrack = -1
local currentBeat = 0
local octave = 0
local disableWhenPlaying = {}
local toolTitle = ""
local currentSelectedSong = -1
local canEdit = ConfigureModal ~= nil

local SaveShortcut, RevertShortcut, UndoShortcut, RedoShortcut, ClearShortcut, CopyShortcut, PasteShortcut, ExportPatternShortcut, ExportSongShortcut = 6, 7, 9, 10, 11, 12, 13, 15, 16

local midiTable = 
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

local validNotes = {
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

local validWaves = {
  "!\"", -- any
  "%&", -- square
  ":;", -- saw tooth
  -- "'(", -- sine (Not enabled by default)
  ")*", -- noise
  "<=", -- triangle
  "./" -- wave
}

local waveTypeIDs = {
  -1,
  0,
  1,
  -- 2,
  3,
  4,
  5
}

-- Store map for black keys
local blackKeyMap = {
  1, 3, 6, 8, 10, 13, 15, 18, 20, 22
}

-- Store map for white keys
local whiteKeyMap = {
  0, 2, 4, 5, 7, 9, 11, 12, 14, 16, 17, 19, 21, 23, 24
}

function Invalidate()

  -- Only redraw the title when needed
  if(invalid == true)then
    return
  end

  pixelVisionOS:ChangeTitle(toolTitle .."*", "toolbariconfile")

  pixelVisionOS:EnableMenuItem(SaveShortcut, true)


  invalid = true

end

function ResetValidation()

  -- Only everything if it needs to be
  if(invalid == false)then
    return
  end

  pixelVisionOS:ChangeTitle(toolTitle, "toolbariconfile")

  pixelVisionOS:EnableMenuItem(SaveShortcut, false)

  invalid = false

end

function Init()

  BackgroundColor(5)

  -- Disable the back key in this tool
  EnableBackKey(false)
  EnableAutoRun(false)

  -- Create an global instance of the Pixel Vision OS
  _G["pixelVisionOS"] = PixelVisionOS:Init()

  -- -- Create an instance of the Pixel Vision OS
  -- pixelVisionOS = PixelVisionOS:Init()

  -- -- Get a reference to the Editor UI
  -- editorUI = pixelVisionOS.editorUI

  -- Reset the undo history so it's ready for the tool
  -- pixelVisionOS:ResetUndoHistory()

  rootDirectory = ReadMetadata("directory", nil)

  if(rootDirectory ~= nil) then

    -- Load only the game data we really need
    success = gameEditor.Load(rootDirectory, {SaveFlags.System, SaveFlags.Sounds, SaveFlags.Music})

  end


  -- If data loaded activate the tool
  if(success == true) then

    if(canEdit == true) then
      configModal = ConfigureModal:Init(editorUI)
      configModal.editorUI = editorUI
    end

    local pathSplit = string.split(rootDirectory, "/")

    -- Update title with file path
    toolTitle = pathSplit[#pathSplit] .. "/" .. "music.json"

    totalTracks = gameEditor:TotalTracks()
    totalChannels = gameEditor:TotalChannels()

    local menuOptions = 
    {
      -- About ID 1
      {name = "About", action = function() pixelVisionOS:ShowAboutModal(toolName) end, toolTip = "Learn about PV8."},
      {divider = true},
      {name = "Edit Generator", action = OnConfig, enabled = canEdit, toolTip = "Configure the song generation tool."},
      {name = "Reset Generator", action = OnResetConfig, enabled = canEdit, toolTip = "Reset the music generator to its default values."},
      {divider = true},
      {name = "Save", action = OnSave, enabled = false, key = Keys.S, toolTip = "Save changes made to the music file."}, -- Reset all the values
      {name = "Revert", action = nil, enabled = false, toolTip = "Revert the music file to its previous state."}, -- Reset all the values
      {divider = true},
      {name = "Undo Note", action = OnUndo, enabled = false, key = Keys.Z, toolTip = "Undo last note change."}, -- Reset all the values
      {name = "Redo Note", action = OnRedo, enabled = false, key = Keys.Y, toolTip = "Redo last note change."}, -- Reset all the values
      {name = "Clear Note", action = EraseNote, enabled = true, key = Keys.D, toolTip = "Clear the currently selected note."}, -- Reset all the values
      {name = "Copy Note", action = OnCopyNote, enabled = true, key = Keys.C, toolTip = "Copy the currently selected note."}, -- Reset all the values
      {name = "Paste Note", action = OnPasteNote, enabled = false, key = Keys.V, toolTip = "Paste the last copied note."}, -- Reset all the values
      {divider = true},
      -- {name = "Export Pattern", action = ExportLoop, enabled = true, toolTip = "Export the current pattern."}, -- Reset all the values
      {name = "Export Song", action = function() OnExportSong(currentSongID, true) end, key = Keys.E, enabled = canEdit, toolTip = "Export the current song."},
      {name = "Export All Songs", action = OnExportAllSongs, enabled = false, toolTip = "Export the current song."},
      {divider = true},
      {name = "Quit", key = Keys.Q, action = OnQuit, toolTip = "Quit the current game."}, -- Quit the current game
    }

    if(PathExists(NewWorkspacePath(rootDirectory).AppendFile("code.lua"))) then
      table.insert(menuOptions, #menuOptions, {name = "Run Game", action = OnRunGame, key = Keys.R, toolTip = "Run the code for this game."})
    end

    pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")

    -- Pattern input field
    patternIDStepper = editorUI:CreateNumberStepper({x = 8, y = 88}, 16, 0, 0, gameEditor:TotalLoops() - 1, "top", "Change the length of the pattern.")

    patternIDStepper.onInputAction = OnLoopID

    -- Need to manually register each component to disable
    table.insert(disableWhenPlaying, patternIDStepper.backButton)
    table.insert(disableWhenPlaying, patternIDStepper.inputField)
    table.insert(disableWhenPlaying, patternIDStepper.nextButton)

    -- Song ID Input field

    -- This is capped at 10 songs since the UI can only support numbers 0 - 9
    songIDStepper = editorUI:CreateNumberStepper({x = 104, y = 16}, 16, 0, 0, gameEditor:TotalSongs() - 1, "top", "Load a new song into memory.")

    songIDStepper.onInputAction = OnSongIDChange

    -- Need to manually register each component to disable
    table.insert(disableWhenPlaying, songIDStepper.backButton)
    table.insert(disableWhenPlaying, songIDStepper.inputField)
    table.insert(disableWhenPlaying, songIDStepper.nextButton)

    -- Song name input field
    songNameInputData = editorUI:CreateInputField({x = 160, y = 24, w = 80}, "Untitled", "Enter a label for this song.", "file")

    songNameInputData.onAction = OnLabelChange

    table.insert(disableWhenPlaying, songNameInputData)

    -- Create song pattern input fields
    songInputFields = {}
    totalSongFields = 9

    songEndButtons = editorUI:CreateToggleGroup(true)
    songEndButtons.onAction = function(id, value)
      -- print("Toggle new button", id)

      -- TODO need to account for scroll offset
      songEndPos = gameEditor:SongEnd(currentSongID, (id - 1) + songScrollOffset)

      if(currentSelectedSong > songEndPos) then
        currentSelectedSong = songEndPos

        OnSelectSongField(currentSelectedSong - songScrollOffset - 1)
      end

      -- TODO need to redraw the song input field
      OnSongScroll()

      Invalidate()
    end

    -- First sone end button
    firstSongEndButton = editorUI:ToggleGroupButton(songEndButtons, {x = 16, y = 48, w = 8, h = 8}, "songend", "Set the end of the song.", true)

    firstSongEndButton.cachedSpriteData.disabled = songstart
    -- songEndButtons =

    for i = 1, totalSongFields do
      local data = editorUI:CreateInputField({x = 24 + ((i - 1) * 24), y = 48, w = 16}, "00", "Enter a pattern.", "number")

      data.min = 0
      data.max = gameEditor:TotalLoops() - 1

      data.index = i - 1

      data.onSelected = OnSelectSongField

      data.onAction = function(value)

        -- Convert to number
        value = tonumber(value)

        -- Update the pattern at the correct position in the song
        gameEditor:SongPatternAt(currentSongID, (data.index + songScrollOffset), value)

        LoadLoop(tonumber(songInputFields[currentSelectedSong - songScrollOffset].text))

        -- Invalidate the data
        Invalidate()

      end

      table.insert(songInputFields, data)

      -- Create song end button
      editorUI:ToggleGroupButton(songEndButtons, {x = 40 + ((i - 1) * 24), y = 48, w = 8, h = 8}, "songend", "Set the end of the song.", true)

    end




    songSliderData = editorUI:CreateSlider({x = 12, y = 58, w = 232, h = 16}, "hsliderhandle", "This is a horizontal slider.", true)
    songSliderData.onAction = OnSongScroll

    -- Force the slider to have a different start value so it can be updated correctly when the first song loads up
    songSliderData.value = -1

    tempoStepper = editorUI:CreateNumberStepper({x = 120, y = 88}, 24, 0, 1, 480, "top", "Change the length of the pattern.")
    tempoStepper.onInputAction = OnTempoChange

    table.insert(disableWhenPlaying, tempoStepper.backButton)
    table.insert(disableWhenPlaying, tempoStepper.inputField)
    table.insert(disableWhenPlaying, tempoStepper.nextButton)

    -- tempoInputData = editorUI:CreateInputField({x = 136, y = 96, w = 24}, "0", "Enter a temp between 60 and 480.", "number")
    -- tempoInputData.min = 60
    -- tempoInputData.max = 480
    -- -- tempoInputData.colorOffset = 32
    -- -- tempoInputData.disabledColorOffset = 34
    -- tempoInputData.onAction = OnTempoChange

    incrementStepper = editorUI:CreateNumberStepper({x = 200, y = 88}, 8, 1, 0, 8, "top", "Change the length of the pattern.")

    table.insert(disableWhenPlaying, incrementStepper.backButton)
    table.insert(disableWhenPlaying, incrementStepper.inputField)
    table.insert(disableWhenPlaying, incrementStepper.nextButton)

    -- TODO need to wire this up to the record feature

    patternLengthStepper = editorUI:CreateNumberStepper({x = 64, y = 88}, 16, 32, 4, gameEditor:NotesPerTrack(), "top", "Change the length of the pattern.")

    -- TODO need to validate what numbers work

    -- TODO need to change the value of NotesPerTrack on the game editor

    editorUI:EnableStepper(patternLengthStepper, false)

    -- TODO need to reenable these
    -- table.insert(disableWhenPlaying, patternLengthStepper.backButton)
    -- table.insert(disableWhenPlaying, patternLengthStepper.inputField)
    -- table.insert(disableWhenPlaying, patternLengthStepper.nextButton)

    patternLengthStepper.stepperValue = 4
    patternLengthStepper.inputField.onValidate = function (value)

      local validValues = {4, 8, 12, 16, 20, 24, 28, 32}

      value = tonumber(value)

      for i = 1, #validValues do

        if(validValues[i] == value) then
          return value
        end

      end

      return patternLengthStepper.inputField.defaultValue

    end

    patternLengthStepper.onInputAction = function(value)
      gameEditor:NotesPerTrack(tonumber(value))
    end

    instrumentStepper = editorUI:CreateNumberStepper({x = 96, y = 152 + 8}, 16, 0, 0, gameEditor:TotalSounds(), "top", "Change the length of the pattern.")
    instrumentStepper.onInputAction = OnSFXChange

    table.insert(disableWhenPlaying, instrumentStepper.backButton)
    table.insert(disableWhenPlaying, instrumentStepper.inputField)
    table.insert(disableWhenPlaying, instrumentStepper.nextButton)


    noteStepper = editorUI:CreateStringStepper({x = 152, y = 152 + 8}, 16, "", validNotes, "top", "Enter the note to play.")

    noteStepper.inputField.forceCase = "upper"
    noteStepper.inputField.allowEmptyString = true

    noteStepper.onInputAction = UpdateNoteFromFields

    table.insert(disableWhenPlaying, noteStepper.backButton)
    table.insert(disableWhenPlaying, noteStepper.inputField)
    table.insert(disableWhenPlaying, noteStepper.nextButton)


    muteToggleButton = editorUI:CreateToggleButton({x = 8, y = 168}, "checkbox", "Mute the track while playing.")

    table.insert(disableWhenPlaying, muteToggleButton)

    muteToggleButton.onAction = function(value)

      gameEditor:MuteTrack(currentTrack, not value)

      -- Force the track to redraw
      DrawTrack(currentTrack, true)

    end

    waveInputField = editorUI:CreateInputField({x = 72, y = 168, w = 16}, validWaves[1], "Wave type.")

    table.insert(disableWhenPlaying, waveInputField)

    waveInputField.editable = false

    trackStepper = editorUI:CreateNumberStepper({x = 24, y = 152 + 8}, 8, 0, 0, gameEditor:TotalChannels() - 1, "top", "Change the currently selected track.")
    trackStepper.onInputAction = function(value) SelectTrack(tonumber(value)) end

    table.insert(disableWhenPlaying, trackStepper.backButton)
    table.insert(disableWhenPlaying, trackStepper.inputField)
    table.insert(disableWhenPlaying, trackStepper.nextButton)

    octaveStepper = editorUI:CreateNumberStepper({x = 208, y = 152 + 8}, 8, 0, octaveRange.x, octaveRange.y, "top", "Change the length of the pattern.")

    octaveStepper.onInputAction = UpdateNoteFromFields

    table.insert(disableWhenPlaying, octaveStepper.backButton)
    table.insert(disableWhenPlaying, octaveStepper.inputField)
    table.insert(disableWhenPlaying, octaveStepper.nextButton)

    generateButtonData = editorUI:CreateButton({x = 8, y = 16}, "toolgenbutton", "Randomly generate a new song.")
    generateButtonData.onAction = GenerateSong

    editorUI:Enable(generateButtonData, canEdit)

    if(canEdit == true) then
      table.insert(disableWhenPlaying, generateButtonData)
    end

    --
    -- Toggle group for playback buttons
    playToggleGroupData = editorUI:CreateToggleGroup(true)
    playToggleGroupData.onAction = OnChangePlayMode


    playFromStartButtonData = editorUI:ToggleGroupButton(playToggleGroupData, {x = 32, y = 16, w = 16, h = 16}, "toolrewindbutton", "Play song from start.")
    playSingleButtonData = editorUI:ToggleGroupButton(playToggleGroupData, {x = 48, y = 16, w = 16, h = 16}, "toolplaybutton", "Play song from current pattern.")
    playLoopButtonData = editorUI:ToggleGroupButton(playToggleGroupData, {x = 64, y = 16, w = 16, h = 16}, "toolloopbutton", "Play and loop the current pattern.")

    -- Stop and rewind buttons
    stopButtonData = editorUI:CreateButton({x = 80, y = 16}, "toolstopbutton", "Stop playback.")
    stopButtonData.onAction = OnStop

    editorUI:Enable(stopButtonData, false)

    -- keyboard
    previousOctaveButtonData = editorUI:CreateButton({x = 0, y = 200 + 8}, "leftoctave", "Move up an octave.")
    previousOctaveButtonData.onAction = PreviousOctave

    table.insert(disableWhenPlaying, previousOctaveButtonData)

    nextOctaveButtonData = editorUI:CreateButton({x = 0, y = 176 + 8}, "rightoctave", "Move down an octave.")
    nextOctaveButtonData.onAction = NextOctave

    table.insert(disableWhenPlaying, nextOctaveButtonData)

    pickerData = editorUI:CreatePicker({x = 0, y = 112, w = 256, h = (8 * totalTracks)}, 8, 8, totalBeats * totalTracks, "notepicker", "Select a track and beat to preview a note value.")

    table.insert(disableWhenPlaying, pickerData)

    pickerData.borderOffset = 0

    pickerData.onAction = OnPickerSelection

    whiteKeyHitRect = {x = 0, y = 24, w = 16, h = 24}

    blackKeyHitRect = {x = 2, y = 0, w = 12, h = 24}

    --
    keyPositions = {

      {
        rect = {x = 16, y = 184},
        spriteName = "leftwhitekey",
        note = whiteKeyMap[1],
        hitRect = whiteKeyHitRect,
        key = Keys.Z
      },
      {
        rect = {x = 32, y = 184},
        spriteName = "centerwhitekey",
        note = whiteKeyMap[2],
        hitRect = whiteKeyHitRect,
        key = Keys.X
      },
      {
        rect = {x = 48, y = 184},
        spriteName = "rightwhitekey",
        note = whiteKeyMap[3],
        hitRect = whiteKeyHitRect,
        key = Keys.C
      },
      {
        rect = {x = 24, y = 184},
        spriteName = "blackkey",
        note = blackKeyMap[1],
        hitRect = blackKeyHitRect,
        key = Keys.S
      },
      {
        rect = {x = 40, y = 184},
        spriteName = "blackkey",
        note = blackKeyMap[2],
        hitRect = blackKeyHitRect,
        key = Keys.D
      },

      {
        rect = {x = 64, y = 184},
        spriteName = "leftwhitekey",
        note = whiteKeyMap[4],
        hitRect = whiteKeyHitRect,
        key = Keys.V
      },
      {
        rect = {x = 80, y = 184},
        spriteName = "centerwhitekey",
        note = whiteKeyMap[5],
        hitRect = whiteKeyHitRect,
        key = Keys.B
      },
      {
        rect = {x = 96, y = 184},
        spriteName = "centerwhitekey",
        note = whiteKeyMap[6],
        hitRect = whiteKeyHitRect,
        key = Keys.N
      },
      {
        rect = {x = 112, y = 184},
        spriteName = "rightwhitekey",
        note = whiteKeyMap[7],
        hitRect = whiteKeyHitRect,
        key = Keys.M
      },
      {
        rect = {x = 72, y = 184},
        spriteName = "blackkey",
        note = blackKeyMap[3],
        hitRect = blackKeyHitRect,
        key = Keys.G
      },
      {
        rect = {x = 88, y = 184},
        spriteName = "blackkey",
        note = blackKeyMap[4],
        hitRect = blackKeyHitRect,
        key = Keys.H
      },
      {
        rect = {x = 104, y = 184},
        spriteName = "blackkey",
        note = blackKeyMap[5],
        hitRect = blackKeyHitRect,
        key = Keys.J
      },

      -- next octave

      {
        rect = {x = 16 + 112, y = 184},
        spriteName = "leftwhitekey",
        note = whiteKeyMap[8],
        hitRect = whiteKeyHitRect,
        key = Keys.Q
      },
      {
        rect = {x = 32 + 112, y = 184},
        spriteName = "centerwhitekey",
        note = whiteKeyMap[9],
        hitRect = whiteKeyHitRect,
        key = Keys.W
      },
      {
        rect = {x = 48 + 112, y = 184},
        spriteName = "rightwhitekey",
        note = whiteKeyMap[10],
        hitRect = whiteKeyHitRect,
        key = Keys.E
      },
      {
        rect = {x = 24 + 112, y = 184},
        spriteName = "blackkey",
        note = blackKeyMap[6],
        hitRect = blackKeyHitRect,
        key = Keys.D2
      },
      {
        rect = {x = 40 + 112, y = 184},
        spriteName = "blackkey",
        note = blackKeyMap[7],
        hitRect = blackKeyHitRect,
        key = Keys.D3
      },

      {
        rect = {x = 64 + 112, y = 184},
        spriteName = "leftwhitekey",
        note = whiteKeyMap[11],
        hitRect = whiteKeyHitRect,
        key = Keys.R
      },
      {
        rect = {x = 80 + 112, y = 184},
        spriteName = "centerwhitekey",
        note = whiteKeyMap[12],
        hitRect = whiteKeyHitRect,
        key = Keys.T
      },
      {
        rect = {x = 96 + 112, y = 184},
        spriteName = "centerwhitekey",
        note = whiteKeyMap[13],
        hitRect = whiteKeyHitRect,
        key = Keys.Y
      },
      {
        rect = {x = 112 + 112, y = 184},
        spriteName = "rightwhitekey",
        note = whiteKeyMap[14],
        hitRect = whiteKeyHitRect,
        key = Keys.U
      },
      {
        rect = {x = 72 + 112, y = 184},
        spriteName = "blackkey",
        note = blackKeyMap[8],
        hitRect = blackKeyHitRect,
        key = Keys.D5
      },
      {
        rect = {x = 88 + 112, y = 184},
        spriteName = "blackkey",
        note = blackKeyMap[9],
        hitRect = blackKeyHitRect,
        key = Keys.D6
      },
      {
        rect = {x = 104 + 112, y = 184},
        spriteName = "blackkey",
        note = blackKeyMap[10],
        hitRect = blackKeyHitRect,
        key = Keys.D7
      },
      {
        rect = {x = 240, y = 184},
        spriteName = "lastwhitekey",
        note = whiteKeyMap[15],
        hitRect = whiteKeyHitRect,
        key = Keys.I -- TODO this needs to be mapped correctly?
      },
    }

    keys = {}

    for i = 1, #keyPositions do

      local data = keyPositions[i]
      -- Create keyboard
      keyButtonData = editorUI:CreateButton(data.rect, data.spriteName, "Midi note '".. data.note .."'.")
      keyButtonData.inputKey = data.key

      table.insert(disableWhenPlaying, keyButtonData)

      keyButtonData.hitRect = {x = keyButtonData.rect.x + data.hitRect.x, y = keyButtonData.rect.y + data.hitRect.y, w = data.hitRect.w, h = data.hitRect.h}

      keyButtonData.onAction = function()
        SetMidiNote(data.note)
      end

      table.insert(keys, keyButtonData)

    end


    SelectOctave(4)

    -- Build the default generator settings
    gameEditor:ConfigureGenerator()

    local startPatternID = 0
    local startSongID = 0
    local startTrackID = 0
    local startBeatID = 0
    local startScrollPos = 0
    -- Load in previous config data from save
    if(SessionID() == ReadSaveData("sessionID", "") and rootDirectory == ReadSaveData("rootDirectory", "")) then
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


      local total = totalTracks

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
    OnSongIDChange(0)

    -- Change the number stepper to the start song ID
    editorUI:ChangeNumberStepperValue(songIDStepper, startSongID)

    -- --   UpdateConfigData()
    -- OnSongIDChange(startSongID)
    -- --
    -- -- -- Load the first song
    -- LoadLoop(startPatternID)
    -- --
    -- print("startTrackID", startTrackID, startBeatID)
    SelectTrack(startTrackID)
    SelectBeat(startBeatID)

    OnSelectSongField(startPatternID)

    editorUI:ChangeSlider(songSliderData, startScrollPos)
    --
    -- -- ChangeMode(EditorMode)
    --
    ResetValidation()

  else

    -- Patch background when loading fails

    -- Left panel
    DrawRect(112, 24, 136, 8, 0, DrawMode.TilemapCache)

    DrawRect(8, 89, 171, 18, BackgroundColor(), DrawMode.TilemapCache)

    DrawRect(209, 89, 23, 18, BackgroundColor(), DrawMode.TilemapCache)

    DrawRect(0, 112, 256, 32, 0, DrawMode.TilemapCache)

    DrawRect(0, 176, 256, 48, 0, DrawMode.TilemapCache)

    DrawRect(24, 152, 48, 20, 11, DrawMode.TilemapCache)
    DrawRect(88, 152, 160, 20, 11, DrawMode.TilemapCache)

    pixelVisionOS:ChangeTitle(toolName, "toolbaricontool")

    pixelVisionOS:ShowMessageModal(toolName .. " Error", "The tool could not load without a reference to a file to edit.", 160, false,
      function()
        QuitCurrentTool()
      end
    )

  end
  --
end

function OnSelectSongField(value)

  -- print("songScrollOffset", songScrollOffset)

  if(currentSelectedSong ~= nil and currentSelectedSong ~= (value + 1)) then

    -- TODO need to calculate real index

    local realIndex = currentSelectedSong - songScrollOffset


    if(realIndex > 0 and realIndex < totalSongFields) then
      -- print("Deselect", currentSelectedSong, realIndex)

      editorUI:SelectSongInputField(songInputFields[realIndex], false)
    end

  end

  -- Account for 0 index value
  currentSelectedSong = (value + 1) + songScrollOffset

  local realIndex = currentSelectedSong - songScrollOffset

  if (realIndex < 1 or realIndex > totalSongFields) then
    local totalPatterns = #currentSongPatterns - totalSongFields

    local scroll = (currentSelectedSong - 1) / totalPatterns
    local scrollX = math.floor(scroll * songSliderData.size) + 11
    songSliderData.handleX = scrollX
    OnSongScroll(scroll)

    realIndex = currentSelectedSong - songScrollOffset
  end

  LoadLoop(tonumber(songInputFields[realIndex].text))

  OnSongScroll()

end

function OnSongIDChange(value)

  currentSongID = tonumber(value)

  -- Change the label value but don't trigger the callback
  editorUI:ChangeInputField(songNameInputData, gameEditor:SongName(currentSongID), false)

  songStartPos = gameEditor:SongStart(currentSongID)
  songEndPos = gameEditor:SongEnd(currentSongID)

  currentSongPatterns = gameEditor:SongPatterns(currentSongID)

  editorUI:ChangeSlider(songSliderData, 0)

  -- Force the song editor to render since slider may be ignored if set to 0 position in previuos song
  OnSongScroll(0)

  -- TODO select the first pattern
  editorUI:SelectSongInputField(songInputFields[1], true)

end

function SetMidiNote(value)

  -- Calculate note from octave
  local realNote = value + (12 * octave)

  UpdateCurrentNote(realNote)

  local nextBeat = Repeat((editorUI:GetNumberStepperValue(incrementStepper) + currentBeat), totalBeats)

  SelectBeat(nextBeat)

end

function OnSongScroll(value)

  if(value ~= nil) then
    local totalPatterns = #currentSongPatterns - totalSongFields
    songScrollOffset = Clamp(value * totalPatterns, 0, totalPatterns)
  end

  editorUI:ClearGroupSelections(songEndButtons)

  -- Clear music selection
  local realIndex = currentSelectedSong - songScrollOffset

  if(realIndex > 0 and realIndex < totalSongFields) then
    editorUI:SelectSongInputField(songInputFields[realIndex], false)
  end

  local enabled = false
  local field = nil
  local index = 0

  for i = 1, totalSongFields do

    index = i + songScrollOffset

    songEndButtons.buttons[i].toolTip = "Click to end the song at pattern index '".. tostring(index - 1) .. "'."

    songInputFields[i].toolTip = "Click to select the song's pattern at index '".. index .. "'."

    if(i == 1) then

      if(index == 1) then
        DrawSprite(songstart.spriteIDs[1], 16 / 8, 48 / 8, false, false, DrawMode.Tile)
      end

      editorUI:Enable(firstSongEndButton, index ~= 1)

    end

    if(index == songEndPos) then

      -- TODO this is off by 1
      editorUI:SelectToggleButton(songEndButtons, i + 1, false)

    end

    tmpValue = gameEditor:SongPatternAt(currentSongID, index - 1)

    enabled = index >= songStartPos and index <= songEndPos
    field = songInputFields[i]

    editorUI:ChangeInputField(field, tostring(tmpValue), false)

    editorUI:SelectSongInputField(songInputFields[i], currentSelectedSong == (i + songScrollOffset), false)

    editorUI:Enable(field, enabled)

  end

end


function OnPickerSelection(value)

  -- Calculate the beat and track
  local beat, track = math.pos(value, pickerData.columns)

  -- Select the correct track
  SelectTrack(track)

  -- Select the correct beat
  SelectBeat(beat)

  local note = gameEditor:Note(currentTrack, currentBeat)

  if(note > 0 and disablePreview ~= true) then
    gameEditor:PlayNote(currentTrack, note)
  end

  -- TODO Hack to make sure we reset the preview in the event it was disabled and not enabled again on the next selection
  disablePreview = false

end

function OnChangePlayMode(value)


  playMode = value

  if(playMode == 1)then

    RewindSong()

    gameEditor:PlaySong(currentSongID)

    editorUI:Enable(stopButtonData, true)

  elseif(playMode == 2) then

    RewindSong()

    -- TODO need to pass in the current start position
    gameEditor:PlaySong(currentSongID, true, currentSelectedSong - 1)

    editorUI:Enable(stopButtonData, true)

  elseif(playMode == 3)then

    RewindSong()

    -- TODO need to play the current selected song pattern
    gameEditor:PlayPattern(currentPatternID)

    editorUI:Enable(stopButtonData, true)

  end

  -- Disable UI when playing back
  local total = #disableWhenPlaying
  for i = 1, total do
    editorUI:Enable(disableWhenPlaying[i], false)
  end

end

function OnLoopID(text)

  local value = tonumber(text)

  local realIndex = currentSelectedSong - songScrollOffset

  if(realIndex > 0 and realIndex < totalSongFields) then

    editorUI:ChangeInputField(songInputFields[realIndex], value, true)
  else
    gameEditor:SongPatternAt(currentSongID, currentSelectedSong - 1, value)
    Invalidate()
  end

  LoadLoop(value)

end


-- Load a song into memory
function LoadLoop(id)

  -- Only load a new loop if the current song does not match the new id
  if(currentPatternID == value) then
    return
  end

  -- Need to make sure we load the song through the editor bridge
  gameEditor:LoadLoop(id)

  currentPatternID = id

  totalBeats = gameEditor:NotesPerTrack()

  editorUI:ChangeNumberStepperValue(patternLengthStepper, totalBeats, false)

  local isPlaying = gameEditor:SongPlaying()

  -- Update the input field but don't trigger the callback since it calls load song.
  editorUI:ChangeNumberStepperValue(patternIDStepper, tostring(id), false, not isPlaying)
  editorUI:ChangeNumberStepperValue(tempoStepper, tostring(gameEditor:Tempo()), false, not isPlaying)

  -- Redraw each track
  DrawAllTracks()

  disablePreview = true
  -- Select the first note and populate all of the fields
  editorUI:SelectPicker(pickerData, 0)

  disablePreview = false

  -- Reset the undo history so it's ready for the tool
  -- TODO configure Undo here
  -- pixelVisionOS:ResetUndoHistory()

  -- UpdateHistoryButtons()

end

function OnLabelChange(text)

  -- TODO need to rename the loop in the game editor
  gameEditor:SongName(currentSongID, text)

  Invalidate()
end

function OnTempoChange(text)

  local value = tonumber(text)

  gameEditor:Tempo(value)

  Invalidate()

end

function OnSFXChange(text)

  local value = tonumber(text)
  gameEditor:ConfigTrackSFX(currentTrack, value)

  PlayCurrentNote()

  Invalidate()

end

function OnBeatChange(text)

  local value = tonumber(text)

  SelectBeat(value)

end

function OnPlaySong(loop)
  if(gameEditor:SongPlaying() == false) then
    -- gameEditor:CurrentBeat(0)
    gameEditor:StartSequencer(loop)
    -- musicEditor:UpdateLoop(loop)
    recordMode = false
  end
end

function OnStop()

  -- Clear play mode
  if(playMode > 0) then
    playMode = 0
  end

  -- Reset the playback toggle
  editorUI:ClearGroupSelections(playToggleGroupData)
  -- StopSong()

  if(gameEditor:SongPlaying() == true) then
    gameEditor:StopSequencer()
  end

  local total = #disableWhenPlaying
  for i = 1, total do
    editorUI:Enable(disableWhenPlaying[i], true)
  end

  editorUI:Enable(playFromStartButtonData, true)
  editorUI:Enable(playSingleButtonData, true)
  editorUI:Enable(playLoopButtonData, true)
  editorUI:Enable(stopButtonData, false)

  editorUI:RefreshNumberStepper(trackStepper)

  editorUI:RefreshNumberStepper(patternIDStepper)
  editorUI:RefreshNumberStepper(tempoStepper)
  editorUI:RefreshNumberStepper(incrementStepper)

  editorUI:RefreshNumberStepper(instrumentStepper)
  editorUI:RefreshStringStepper(noteStepper)
  -- editorUI:RefreshStringStepper(waveStepper)
  editorUI:RefreshNumberStepper(octaveStepper)
  editorUI:RefreshNumberStepper(songIDStepper)

end

-- Preview the currently selected note
function PlayCurrentNote()

  -- Find the current track and beat
  -- local realTrack = currentTrack
  local note = gameEditor:Note(currentTrack, currentBeat)

  -- If the note is not empty, attempt to play it through the gameEditor
  if(note > 0) then
    gameEditor:PlayNote(currentTrack, note)
  end
end

function OnConfig()

  pixelVisionOS:OpenModal(configModal,
    function()

      if(configModal.selectionValue == false) then
        return
      end

    end
  )

end

function Update(timeDelta)

  -- Update the editor UI
  pixelVisionOS:Update(timeDelta / 1000)

  -- Only update the tool's UI when the modal isn't active
  if(pixelVisionOS:IsModalActive() == false) then

    -- TODO Should we stop music a modal pops up?

    -- Need to update the game editor's music chip so it plays
    if(gameEditor:SongPlaying() == true) then

      -- Call update on the Game Editor's music chip to move the sequencer
      gameEditor:UpdateSequencer(timeDelta)

      local songData = gameEditor:ReadSongData()

      local currentPattern = songData["pattern"]

      if((currentSelectedSong - 1) ~= currentPattern and playMode ~= 3) then
        OnSelectSongField(currentPattern - songScrollOffset)
        editorUI.refreshTime = 0
      end

      -- TODO need to put this cap of under 32 in to make sure the array didn't go out of bounds
      if(editorUI.refreshTime == 0 and songData["note"] < 32) then
        -- print("note", songData["note"])
        SelectBeat(songData["note"])
      end
    else
      -- Create a new piont to see if we need to change the sprite position
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

        SelectBeat(currentBeat + newPos.x)
        SelectTrack(currentTrack + newPos.y)

      end



    end

    editorUI:UpdatePicker(pickerData)

    editorUI:UpdateInputField(songNameInputData)

    for i = 1, totalSongFields do

      editorUI:UpdateSongInputField(songInputFields[i], editorUI.timeDelta)

    end

    editorUI:UpdateToggleGroup(songEndButtons)

    editorUI:UpdateSlider(songSliderData)

    editorUI:UpdateStepper(patternIDStepper)
    editorUI:UpdateStepper(patternLengthStepper)
    editorUI:UpdateStepper(tempoStepper)
    editorUI:UpdateStepper(incrementStepper)
    editorUI:UpdateStepper(songIDStepper)

    editorUI:UpdateStepper(instrumentStepper)
    editorUI:UpdateStepper(noteStepper)
    editorUI:UpdateInputField(waveInputField)
    editorUI:UpdateStepper(trackStepper)
    editorUI:UpdateStepper(octaveStepper)
    -- editorUI:UpdateStepper(volumeStepper)

    editorUI:UpdateButton(muteToggleButton)
    editorUI:UpdateButton(previousLengthButtonData)
    editorUI:UpdateButton(nextLengthButtonData)

    editorUI:UpdateButton(previousOctaveButtonData)
    editorUI:UpdateButton(nextOctaveButtonData)

    editorUI:UpdateButton(generateButtonData)
    editorUI:UpdateButton(stopButtonData)

    editorUI:UpdateToggleGroup(playToggleGroupData)

    local controlDown = (Key(Keys.LeftControl) == true or Key(Keys.RightControl) == true)

    local total = #keys
    for i = 1, total do
      local key = keys[i]

      editorUI:UpdateButton(key)

      if(key.inputKey ~= nil and controlDown == false and editorUI.editingInputField == false) then
        if(Key(key.inputKey, InputState.Released)) then
          editorUI:Invalidate(key)
          key.onAction()
        elseif(Key(key.inputKey)) then
          editorUI:RedrawButton(key, "over")

        end
      end
    end

    -- Increment octave
    if( controlDown == false and editorUI.editingInputField == false) then
      if(Key(Keys.OemOpenBrackets, InputState.Released) and octave > 1) then
        PreviousOctave()
      elseif(Key(Keys.OemCloseBrackets, InputState.Released) and octave < 8) then
        NextOctave()
      elseif(Key(Keys.Delete, InputState.Released) or Key(Keys.Back, InputState.Released)) then
        EraseNote()
      end
    end

  end

  if(installing == true) then


    installingTime = installingTime + editorUI.timeDelta

    if(installingTime > installingDelay) then
      installingTime = 0


      OnInstallNextStep()

      if(installingCounter >= installingTotal) then

        OnInstallComplete()

      end

    end


  end
end

function OnSave()

  -- TODO need to save music and sounds when those are broken out
  gameEditor:Save(rootDirectory, {SaveFlags.System, SaveFlags.Music, SaveFlags.Sounds})

  -- Display that the data was saved and reset invalidation
  pixelVisionOS:DisplayMessage("The game's 'music' file has been updated.", 5)

  ResetValidation()

end
--
function Draw()

  RedrawDisplay()

  pixelVisionOS:Draw()

  if(pixelVisionOS:IsModalActive() == false) then

    -- Draw note octave
    DrawText("C" .. octave, 16 + 2, 184, DrawMode.Sprite, "small", 12, - 4)
    DrawText("C" .. (octave + 1), 128 + 2, 184, DrawMode.Sprite, "small", 12, - 4)
    DrawText("C" .. (octave + 2), 240 + 2, 184, DrawMode.Sprite, "small", 12, - 4)

  end

end

-- Select the track
function SelectTrack(id)

  -- Only update if the current track is not the same as the new ID
  if(currentTrack ~= id) then

    -- Make sure the track ID is never greater than the total number of tracks
    id = Repeat(id, totalTracks)

    if(currentTrack > - 1 and currentTrack < 5) then
      -- Redraw the current track so it doesn't look selected
      DrawTrack(currentTrack, false)
    end

    local currentChannelType = gameEditor:ChannelType(id)

    local validWaveID = table.indexOf(waveTypeIDs, currentChannelType)

    editorUI:ChangeInputField(waveInputField, validWaves[validWaveID])


    -- local type = gameEditor:ChannelType(id) + 2
    --
    -- editorUI:ChangeStringStepperValue(waveStepper, validWaves[type], false, true)

    -- Save the new track value
    currentTrack = id

    -- Redraw the track
    DrawTrack(currentTrack, true)

    editorUI:ToggleButton(muteToggleButton, not gameEditor:MuteTrack(currentTrack), false)

    -- We need to save the current beat then clear the current beat so the beat updates correctly on the new track
    local beat = currentBeat
    currentBeat = nil

    -- Select the current beat but now it is on the new track
    SelectBeat(beat)

    -- TODO need to move the update logic into it's own block of code
    -- editorUI:ChangeInputField(trackInputData, tostring(currentTrack), false)

    editorUI:ChangeNumberStepperValue(trackStepper, id, false)

    editorUI:ChangeNumberStepperValue(instrumentStepper, gameEditor:TrackInstrument(currentTrack), false)

  end

end

function DrawAllTracks()

  -- Get the current number of tracks from the input field
  -- local total = tonumber(totalTracksInputData.text) - 1
  for i = 0, totalChannels - 1 do
    DrawTrack(i, i == currentTrack)
    -- TODO need to see if the track is disabled
  end
end

-- Draw the track to the display
function DrawTrack(track, selected)

  local muted = gameEditor:MuteTrack(track)
  local hasNote = false
  local disabled = track >= totalTracks
  for i = 0, totalBeats - 1 do

    DrawNote(track, i, selected, disabled, muted)
  end
end

-- Test to see if a note exists
function HasNote(track, beat)
  return (gameEditor:Note(track, beat) > 0)
end

-- Draw the note to the display
function DrawNote(track, beat, selected, disabled, muted)
  local bar = beat % 4 == 3 and 1 or 0
  local hasNote = HasNote(track, beat)

  local sprite = _G["trackbar"..bar.."disabled"].spriteIDs[1]

  if(disabled == false) then

    local spriteName = GenerateNoteSpriteName(muted == true and 4 or track, bar, hasNote, selected)

    sprite = _G[spriteName].spriteIDs[1]

  end

  if(sprite ~= nil)then
    Tile(beat, track + pickerData.tiles.r, sprite)
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
function SelectBeat(id)

  currentBeat = gameEditor:CurrentBeat(id)

  currentNote = gameEditor:Note(currentTrack, currentBeat)

  pixelVisionOS:EnableMenuItem(CopyShortcut, currentNote > 0)
  pixelVisionOS:EnableMenuItem(ClearShortcut, currentNote > 0)

  local playing = gameEditor:SongPlaying()

  editorUI:ChangeStringStepperValue(noteStepper, ConvertMidiToNote(currentNote), false, not playing)

  editorUI:ChangeNumberStepperValue(octaveStepper, math.floor(currentNote / 12), false, not playing)

  local hasNote = HasNote(currentTrack, currentBeat)

  editorUI:SelectPicker(pickerData, CalculateIndex(currentBeat, currentTrack, 32), false)

  -- end
end


function ConvertMidiToNote(midiValue)

  if(midiValue == 0) then
    return ""
  end

  local offset = midiValue - 24


  -- TODO this is missing the last value, need to look into the calculation

  -- print("midiValue", midiValue, offset, offset % #midiTable, midiTable[(offset % #midiTable) + 1])
  -- currentNote

  return midiTable[(offset % #midiTable) + 1]
end

function ConvertNoteToMidi(note)

  local value = table.indexOf(midiTable, note)

  if(value == -1) then
    return 0
  else
    return value
  end

end

-- Rewind the song
function RewindSong()

  -- TODO need to move the song scroller to the correct position
  SelectBeat(0)
end

-- Clear a note
function EraseNote()
  UpdateCurrentNote(0)
  SelectBeat(currentBeat)

  Invalidate()
end

-- Select the next set of octaves
function NextOctave()
  local nextID = octave + 1

  if(nextID > octaveRange.y) then
    nextID = octaveRange.x
  end

  SelectOctave(nextID)
end

-- Select the previous set of octaves
function PreviousOctave()
  local nextID = octave - 1

  if(nextID < octaveRange.x) then
    nextID = octaveRange.y
  end

  SelectOctave(nextID)
end

-- Select an octave
function SelectOctave(value)

  -- octave = Repeat(value, octaveRange.y)
  octave = Clamp(value, octaveRange.x, octaveRange.y)

  for i = 1, #keyPositions do

    local data = keyPositions[i]

    local keyButton = keys[i]
    keyButton.note = data.note + (12 * octave)

    keyButton.toolTip = "Note " .. ConvertMidiToNote(keyButton.note) .. " (Midi ".. keyButton.note ..")."

  end

  editorUI:ChangeNumberStepperValue(octaveStepper, octave, false)

end

-- Update the currently selected note with a new value
function UpdateCurrentNote(value, saveHistory)

  local oldNote = currentNote

  gameEditor:Note(currentTrack, currentBeat, value)


  if(saveHistory ~= false) then
    UpdateHistory(currentTrack, currentBeat, value, oldNote)
  end

  DrawNote(currentTrack, currentBeat, true, false)

  PlayCurrentNote()

  if(playMode == 1 and currentBeat < totalBeats) then
    SelectBeat(currentBeat + 1)
  end

  Invalidate()

end

function UpdateHistory(track, beat, newNote, oldNote)

  -- local historyAction = {
  --   RedoAction = function()
  --     SelectTrack(track)
  --     SelectBeat(beat)
  --     UpdateCurrentNote(newNote, false)
  --     -- print("Redo Note", newNote)
  --     -- Force the display to update again
  --     SelectBeat(beat)
  --   end,
  --   UndoAction = function()
  --     SelectTrack(track)
  --     SelectBeat(beat)
  --     UpdateCurrentNote(oldNote, false)
  --     -- Force the display to update again
  --     SelectBeat(beat)
  --   end
  -- }

  -- pixelVisionOS:AddUndoHistory(historyAction)

  -- UpdateHistoryButtons()

end



function OnQuit()

  if(invalid == true) then

    pixelVisionOS:ShowMessageModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before you quit?", 160, true,
      function()
        if(pixelVisionOS.messageModal.selectionValue == true) then
          -- Save changes
          OnSave()

        end

        -- Quit the tool
        QuitCurrentTool()

      end
    )

  else
    -- Quit the tool
    QuitCurrentTool()
  end

end

function UpdateNoteFromFields(value)

  if(value == "") then
    EraseNote()
  else
    -- local note = 0
    local note = ConvertNoteToMidi(editorUI:GetStringStepperValue(noteStepper)) - 1
    local octave = note == -1 and 0 or editorUI:GetNumberStepperValue(octaveStepper)

    local newNote = note + (12 * octave)

    UpdateCurrentNote(newNote)

  end

end



function OnCopyNote()
  -- print("Copy Note", currentNote)
  noteInClipboard = currentNote

  pixelVisionOS:EnableMenuItem(PasteShortcut, true)
end

function OnPasteNote()

  if(noteInClipboard == nil) then
    return
  end

  UpdateCurrentNote(noteInClipboard)

  SelectBeat(currentBeat)

  pixelVisionOS:EnableMenuItem(PasteShortcut, false)

  noteInClipboard = nil

end

function Shutdown()
  -- Save the current session ID
  WriteSaveData("sessionID", SessionID())
  WriteSaveData("rootDirectory", rootDirectory)

  WriteSaveData("state", tostring(currentSongID) .. "," .. tostring(currentTrack) .. "," .. tostring(currentBeat) .. "," .. tostring(currentSelectedSong - 1) .. "," .. tostring(songSliderData.value))

  WriteSaveData("configDensity", tostring(gameEditor.pcgDensity))
  WriteSaveData("configFunk", tostring(gameEditor.pcgFunk))
  WriteSaveData("configLayering", tostring(gameEditor.pcgLayering))
  -- Increase scale by 1 to account for Lua 1 base array index
  WriteSaveData("configScale", tostring(gameEditor.scale))
  WriteSaveData("configSpeedMin", tostring(gameEditor.pcgMinTempo))
  WriteSaveData("configSpeedMax", tostring(gameEditor.pcgMaxTempo))

  local total = totalTracks

  for i = 1, total do

    local trackID = i - 1

    local sfx = gameEditor:ConfigTrackSFX(trackID)
    local instrument = gameEditor:ConfigTrackInstrument(trackID)
    local octRange = gameEditor:ConfigTrackOctaveRange(trackID)

    WriteSaveData("configTrack" .. tostring(trackID), tostring(sfx) .. "," .. tostring(instrument) .. "," .. tostring(octRange.x) .. "," .. tostring(octRange.y))

  end

end

function OnRunGame()

  if(invalid == true) then

    pixelVisionOS:ShowMessageModal("Unsaved Changes", "You have unsaved changes. You will lose those changes if you run the game now?", 160, true,
      function()
        if(pixelVisionOS.messageModal.selectionValue == true) then
          LoadGame(NewWorkspacePath(rootDirectory))
        end

      end
    )

  else

    LoadGame(NewWorkspacePath(rootDirectory))

  end

end

-- TODO rewire history
function UpdateHistoryButtons()

  -- pixelVisionOS:EnableMenuItem(UndoShortcut, pixelVisionOS:IsUndoable())
  -- pixelVisionOS:EnableMenuItem(RedoShortcut, pixelVisionOS:IsRedoable())

end

function OnUndo()

  -- local action = pixelVisionOS:Undo()

  -- if(action ~= nil and action.UndoAction ~= nil) then
  --   action.UndoAction()
  -- end

  -- UpdateHistoryButtons()
end

function OnRedo()

  -- local action = pixelVisionOS:Redo()

  -- if(action ~= nil and action.RedoAction ~= nil) then
  --   action.RedoAction()
  -- end

  -- UpdateHistoryButtons()
end
