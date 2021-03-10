
function OnExportSong(songID, showWarning)

  songID = songID or currentSongID

  -- local workspacePath = NewWorkspacePath(rootDirectory).AppendDirectory("Wavs").AppendDirectory("Songs")

  if(showWarning == true) then

    local workspacePath = NewWorkspacePath(rootDirectory).AppendDirectory("Wavs").AppendDirectory("Songs")

    local buttons = 
    {
      {
        name = "modalyesbutton",
        action = function(target)
          
          gameEditor:ExportSong(rootDirectory, songID)
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
  
    gameEditor:ExportSong(rootDirectory, songID)
  
  end

end

function OnExportAllSongs()

  local workspacePath = NewWorkspacePath(rootDirectory).AppendDirectory("Wavs").AppendDirectory("Songs")

  local buttons = 
  {
    {
      name = "modalyesbutton",
      action = function(target)
        installing = true

        installingTime = 0
        installingDelay = .1
        installingCounter = 0
        installingTotal = songIDStepper.inputField.max

        installRoot = rootPath
        target.onParentClose()
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


  -- pixelVisionOS:ShowMessageModal("Export All Sounds", "Do you want to export all of the sound effects to '"..workspacePath.Path.."'?", 200, true,
  --   function()
  --     if(pixelVisionOS.messageModal.selectionValue == true) then

  --       installing = true

  --       installingTime = 0
  --       installingDelay = .1
  --       installingCounter = 0
  --       installingTotal = songIDStepper.inputField.max

  --       installRoot = rootPath

  --     end

  --   end
  -- )

end

function OnInstallNextStep()

  -- print("Next step")
  -- Look to see if the modal exists
  if(installingModal == nil) then

    -- Create the model
    installingModal = ProgressModal:Init("Exporting", editorUI)

    -- Open the modal
    pixelVisionOS:OpenModal(installingModal)

  end

  OnExportSong(installingCounter, false)

  installingCounter = installingCounter + 1

  local message = "Exporting song "..string.lpad(tostring(installingCounter), string.len(tostring(installingTotal)), "0") .. " of " .. installingTotal .. ".\n\n\n\nDo not restart or shut down Pixel Vision 8."

  local percent = (installingCounter / installingTotal)

  installingModal:UpdateMessage(message, percent)

  if(installingCounter >= installingTotal) then
    installingDelay = .5
  end

end

function OnInstallComplete()

  installing = false

  pixelVisionOS:CloseModal()

end

function OnResetConfig()

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