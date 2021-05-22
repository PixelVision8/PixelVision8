
function MusicTool:OnExportSong(songID, showWarning)

  songID = songID or self.currentSongID

  -- local workspacePath = NewWorkspacePath(rootDirectory).AppendDirectory("Wavs").AppendDirectory("Songs")

  if(showWarning == true) then

    local workspacePath = NewWorkspacePath(self.rootDirectory).AppendDirectory("Wavs").AppendDirectory("Songs")

    local buttons = 
    {
      {
        name = "modalyesbutton",
        action = function(target)
          
          gameEditor:ExportSong(self.rootDirectory, songID)
          target.onParentClose()
          
        end,
        key = Keys.Enter,
        tooltip = "Press 'enter' to export the song"
      },
      {
        name = "modalnobutton",
        action = function(target)
          target.onParentClose()
        end,
        key = Keys.Escape,
        tooltip = "Press 'esc' to cancel"
      }
    }
    
    pixelVisionOS:ShowMessageModal("Export Song", "Do you want to export this song to '"..workspacePath.Path.."'?", 200, buttons)
  
  else
  
    gameEditor:ExportSong(self.rootDirectory, songID)
  
  end

end

function MusicTool:OnExportAllSongs()

  local workspacePath = NewWorkspacePath(self.rootDirectory).AppendDirectory("Wavs").AppendDirectory("Songs")

  local buttons = 
  {
    {
      name = "modalyesbutton",
      action = function(target)
        -- self.installing = true

        self.installingTime = 0
        self.installingDelay = .1
        self.installingCounter = 0
        self.installingTotal = songIDStepper.inputField.max

        installRoot = rootPath
        target.onParentClose()

        print("Trigger Export")
        pixelVisionOS:RegisterUI({name = "UpdateExportLoop"}, "UpdateExport", self)

      end,
      key = Keys.Enter,
      tooltip = "Press 'enter' to export all songs"
    },
    {
      name = "modalnobutton",
      action = function(target)
        target.onParentClose()
      end,
      key = Keys.Escape,
      tooltip = "Press 'esc' to cancel"
    }
  }
  
  pixelVisionOS:ShowMessageModal("Export All Songs", "Do you want to export all of the songs to '"..workspacePath.Path.."'?", 200, buttons)

end

function MusicTool:OnInstallNextStep()

  -- print("Next step")
  -- Look to see if the modal exists
  if(self.installingModal == nil) then

    -- Create the model
    self.installingModal = ProgressModal:Init("Exporting", editorUI)

    -- Open the modal
    pixelVisionOS:OpenModal(self.installingModal)

  end

  OnExportSong(self.installingCounter, false)

  self.installingCounter = self.installingCounter + 1

  local message = "Exporting song "..string.lpad(tostring(self.installingCounter), string.len(tostring(self.installingTotal)), "0") .. " of " .. self.installingTotal .. ".\n\n\n\nDo not restart or shut down Pixel Vision 8."

  local percent = (self.installingCounter / self.installingTotal)

  self.installingModal:UpdateMessage(message, percent)

  if(self.installingCounter >= self.installingTotal) then
    self.installingDelay = .5
  end

end

function MusicTool:OnInstallComplete()

  pixelVisionOS:RemoveUI("UpdateExportLoop")

  pixelVisionOS:CloseModal()

end

function MusicTool:OnResetConfig()

  local buttons = 
  {
    {
      name = "modalyesbutton",
      action = function(target)
        -- Save changes
        gameEditor.pcgDensity = 5
        gameEditor.pcgFunk = 5
        gameEditor.pcgLayering = 5

        gameEditor.pcgMinTempo = 50
        gameEditor.pcgMaxTempo = 120

        gameEditor.scale = 1

        disablePreview = true
        configModal:OnResetConfig()
      end,
      key = Keys.Enter,
      tooltip = "Press 'enter' to quit the tool"
    },
    {
      name = "modalnobutton",
      action = function(target)
        target.onParentClose()
      end,
      key = Keys.Escape,
      tooltip = "Press 'esc' to cancel"
    }
  }
  
  pixelVisionOS:ShowMessageModal("Reset Music Generator", "Do you want to restore the music generator to its default values?", 160, buttons)

end


-- TODO this needs to be wired up
function MusicTool:UpdateExport()
  
  print("Update Export")
    self.installingTime = self.installingTime + editorUI.timeDelta

    if(self.installingTime > self.installingDelay) then
      self.installingTime = 0


      self:OnInstallNextStep()

      if(self.installingCounter >= self.installingTotal) then

        self:OnInstallComplete()

      end

    end

end