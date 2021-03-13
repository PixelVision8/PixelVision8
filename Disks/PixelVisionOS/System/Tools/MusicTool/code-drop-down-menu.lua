--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- SaveShortcut = 5

function MusicTool:CreateDropDownMenu()

    local menuOptions = 
    {
      -- About ID 1
      {name = "About", action = function() pixelVisionOS:ShowAboutModal(self.toolName) end, toolTip = "Learn about PV8."},
      {divider = true},
      {name = "Edit Generator", action = function() self:OnConfig() end, enabled = true, toolTip = "Configure the song generation tool."},
      {name = "Reset Generator", action = function() self:OnResetConfig() end, enabled = true, toolTip = "Reset the music generator to its default values."},
      {divider = true},
      {name = "Save", action = function() self:OnSave() end, enabled = false, key = Keys.S, toolTip = "Save changes made to the music file."}, -- Reset all the values
      {name = "Revert", action = nil, enabled = false, toolTip = "Revert the music file to its previous state."}, -- Reset all the values
      {divider = true},
      {name = "Undo", action = function() self:OnUndo() end, enabled = false, key = Keys.Z, toolTip = "Undo last note change."}, -- Reset all the values
      {name = "Redo", action = function() self:OnRedo() end, enabled = false, key = Keys.Y, toolTip = "Redo last note change."}, -- Reset all the values
      {name = "Clear Note", action = function() self:EraseNote() end, enabled = true, key = Keys.D, toolTip = "Clear the currently selected note."}, -- Reset all the values
      {name = "Copy Note", action = function() self:OnCopyNote() end, enabled = true, key = Keys.C, toolTip = "Copy the currently selected note."}, -- Reset all the values
      {name = "Paste", action = function() self:OnPaste() end, enabled = false, key = Keys.V, toolTip = "Paste the last copied note."}, -- Reset all the values
      {divider = true},
      {name = "Clear Track", action = function() self:OnClearCurrentTrack() end, enabled = true, toolTip = "Clear the currently selected note."}, -- Reset all the values
      {name = "Copy Track", action = function() self:OnCopyTrack() end, enabled = true, toolTip = "Copy the currently selected note."}, -- Reset all the values
      {divider = true},
      {name = "Clear Pattern", action = function() self:OnClearPattern() end, enabled = true, toolTip = "Clear the currently selected note."}, -- Reset all the values
      {name = "Copy Pattern", action = function() self:OnCopyPattern() end, enabled = true, toolTip = "Copy the currently selected note."}, -- Reset all the values
      {divider = true},
      -- {name = "Export Pattern", action = ExportLoop, enabled = true, toolTip = "Export the current pattern."}, -- Reset all the values
      {name = "Export Song", action = function() self:OnExportSong(self.currentSongID, true) end, key = Keys.E, enabled = true, toolTip = "Export the current song."},
      {name = "Export All Songs", action = function() self:OnExportAllSongs() end, enabled = false, toolTip = "Export the current song."},
      {divider = true},
      {name = "Quit", key = Keys.Q, action = function() self:OnQuit() end, toolTip = "Quit the current game."}, -- Quit the current game
    }

    if(PathExists(NewWorkspacePath(self.rootDirectory).AppendFile("code.lua"))) then
      table.insert(menuOptions, #menuOptions, {name = "Run Game", action = function() self:OnRunGame() end, key = Keys.R, toolTip = "Run the code for this game."})
    end

    pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")


end

function MusicTool:OnConfig()

  pixelVisionOS:OpenModal(self.configModal,
    function()

      if(self.configModal.selectionValue == false) then
        return
      end

    end
  )

end

function MusicTool:OnSave()

  -- TODO need to save music and sounds when those are broken out
  gameEditor:Save(self.rootDirectory, {SaveFlags.System, SaveFlags.Music, SaveFlags.Sounds})

  -- Display that the data was saved and reset self.invalidation
  pixelVisionOS:DisplayMessage("The game's 'music' file has been updated.", 5)

  self:ResetValidation()

end

-- Clear a note
function MusicTool:EraseNote()
  self:UpdateCurrentNote(0)
  self:SelectBeat(self.currentBeat)

  self:Invalidate()
end

function MusicTool:OnQuit()

  -- if(self.invalid == true) then

  if(self.invalid == true) then

    pixelVisionOS:ShowSaveModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before you quit?", 160,
      -- Accept
      function(target)
        -- print("Save/quit")
        self:OnSave()
        QuitCurrentTool()
      end,
      -- Decline
      function (target)
        -- print("Quit")
        QuitCurrentTool()
      end,
      -- Cancel
      function(target)
        -- print("Cancel")
        target.onParentClose()
      end
    )

  else
    -- Quit the tool
    QuitCurrentTool()
  end

end


function MusicTool:OnRunGame()

  -- if(self.invalid == true) then

    if(self.invalid == true) then

      pixelVisionOS:ShowSaveModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before running the game?", 160,
        -- Accept
        function(target)
          self:OnSave()
          LoadGame(NewWorkspacePath(self.rootDirectory))
        end,
        -- Decline
        function (target)
          LoadGame(NewWorkspacePath(self.rootDirectory))
        end,
        -- Cancel
        function(target)
          target.onParentClose()
        end
      )

  else
      -- Quit the tool
      LoadGame(parentPath.Path, data)
  end

end


function MusicTool:GetState()

  -- Current sfx ID
  local songId = self.currentSongID
  local patternId = self.currentPatternID
  local totalBeats = self.totalBeats
  local tempo = self.tempoStepper.inputField.text
  local totalTracks = self.totalTracks
  local tracks = {}
  local currentBeat = self.currentBeat
  local currentTrack = self.currentTrack

  for i = 1, totalTracks do
    
    local track = {}

    for j = 1, totalBeats do
      table.insert(track, gameEditor:Note(i-1, j-1))
    end

    table.insert(tracks, track)
    
  end

  local state = {
      
      Action = function()

        self:OnSongIDChange(songId)

        editorUI:ChangeNumberStepperValue(self.patternLengthStepper, tostring(self.totalBeats), false)
        editorUI:ChangeNumberStepperValue(self.patternIDStepper, tostring(patternId), false, false)
        editorUI:ChangeNumberStepperValue(self.tempoStepper, tempo, false, false)
        

        for i = 1, totalTracks do
    
          for j = 1, totalBeats do
            
            gameEditor:Note(i-1, j-1, tracks[i][j])

          end
      
        end

        self:DrawAllTracks()

        self:SelectTrack(currentTrack)

        self:SelectBeat(currentBeat)
        -- gameEditor:Sound(id, data)

        -- self:LoadSound(id)

        -- self:OnPlaySound()

        -- self:InvalidateData()
      end
  }

  return state

end

function MusicTool:OnClearCurrentTrack()
  
  pixelVisionOS:BeginUndo(self)

  self:ClearTrack(self.currentTrack)

  pixelVisionOS:EndUndo(self)

  self:SelectBeat(0)

  self:Invalidate()

end

function MusicTool:ClearTrack(id)

  for i = 1, self.totalBeats do
    gameEditor:Note(id, i-1, 0)
  end

  self:DrawTrack(id, id == self.currentTrack)

end

function MusicTool:OnClearPattern()
  
  pixelVisionOS:BeginUndo(self)

  for i = 1, self.totalTracks do
    self:ClearTrack(i-1)
  end

  pixelVisionOS:EndUndo(self)

  self:SelectTrack(0)
  self:SelectBeat(0)

  self:Invalidate()

end

function MusicTool:OnCopyNote()
  
  local data = "note:" .. self.currentNote

  pixelVisionOS:EnableMenuItem(self.PasteShortcut, true)

  pixelVisionOS:SystemCopy(data)

end

function MusicTool:OnCopyTrack()

  local data = "track:"

  for i = 1, self.totalBeats do
    data = data .. gameEditor:Note(self.currentTrack, i-1) .. ","
  end

  pixelVisionOS:EnableMenuItem(self.PasteShortcut, true)

  pixelVisionOS:SystemCopy(data)

end

function MusicTool:OnCopyPattern()

  local data = "pattern:" .. self.totalTracks .. "|" .. self.totalBeats .. "|" .. self.tempoStepper.inputField.text .. "|"

  local instruments = ""

  local notes  = ""

  -- TODO need to add in temp and instruments

  for i = 1, self.totalTracks do
    
    instruments = instruments .. gameEditor:TrackInstrument(i-1) .. ","

    for j = 1, self.totalBeats do
      notes = notes .. gameEditor:Note(i-1, j-1) .. ","
    end

  end

  data = data .. instruments .. "|" .. notes
  
  pixelVisionOS:EnableMenuItem(self.PasteShortcut, true)

  pixelVisionOS:SystemCopy(data)

end


function MusicTool:OnPaste()

  if(pixelVisionOS:ClipboardFull() == false) then
    return
  end

  pixelVisionOS:BeginUndo(self)
  -- TODO need to see what is in the clipboard

  local cancelBtn = 
  {
    name = "modalnobutton",
    action = onDecline,
    key = Keys.N,
    tooltip = "Press 'n' to exit pasting"
  }

  local data = string.split(pixelVisionOS:SystemPaste(), ":")

  if(data[1] == "note") then

    self:UpdateCurrentNote(tonumber(data[2]))

    self:SelectBeat(self.currentBeat)

  elseif(data[1] == "track") then

    local notes = string.split(data[2], ",")

    for i = 1, #notes do
    
      local index = i-1
      
      if(index < self.totalBeats) then
        gameEditor:Note(self.currentTrack, index, tonumber(notes[i]))
      end

      self:DrawTrack(self.currentTrack, true)

      self:SelectTrack(self.currentTrack)
      self:SelectBeat(0)

    end

  elseif(data[1] == "pattern") then

    local split = string.split(data[2], "|")

    local totalTracks = split[1]
    local totalBeats = split[2]

    editorUI:ChangeNumberStepperValue(self.tempoStepper, split[3], false, false)

    local instruments = string.split(split[4], ",")
    local notes = string.split(split[5], ",")

    for i = 1, totalTracks do
      
      local trackId = i-1

      if(trackId < self.totalTracks) then
        
        gameEditor:TrackInstrument(trackId, tonumber(instruments[i]))

        for j = 1, totalBeats do
          
          local beatId = j-1

          if(beatId < self.totalBeats) then

            gameEditor:Note(trackId, beatId, tonumber(notes[(beatId + trackId * totalBeats) + 1]))

          end

        end

      end

    end

    self:DrawAllTracks()
    self:SelectTrack(0)
    self:SelectBeat(0)

  end

  pixelVisionOS:EndUndo(self)

  pixelVisionOS:ClearClipboard()

  pixelVisionOS:EnableMenuItem(self.PasteShortcut, false)

  self:Invalidate()

end