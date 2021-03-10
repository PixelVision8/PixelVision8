function SFXTool:CreateDropDownMenu()

  self.SaveShortcut = 4
  self.UndoShortcut = 9
  self.RedoShortcut = 10
  self.CopyShortcut = 11
  self.PasteShortcut = 12

  local menuOptions =
    {
      -- About ID 1
      {name = "About", action = function() pixelVisionOS:ShowAboutModal(self.toolName) end, toolTip = "Learn about PV8."},
      {divider = true},
      {name = "New", action = function() self:OnNewSound() end, key = Keys.N, toolTip = "Revert the sound to empty."}, -- Reset all the values
      {name = "Save", action = function() self:OnSave() end, key = Keys.S, toolTip = "Save changes made to the sounds file."}, -- Reset all the values
      {name = "Export SFX", action = function() self.OnExport(self.currentID, true) end, key = Keys.E, enabled = self.canExport, toolTip = "Create a wav for the current SFX file."}, -- Reset all the values
      {name = "Export All", action = function() self:OnExportAll() end, enabled = self.canExport, toolTip = "Export all sound effects to wavs."}, -- Reset all the values
      {name = "Revert", action = nil, key = Keys.R, enabled = false, toolTip = "Revert the sounds.json file to its previous state."}, -- Reset all the values
      {divider = true},
      {name = "Undo", action = function() self:OnUndo() end, enabled = true, key = Keys.Z, toolTip = "Undo the last action."}, -- Reset all the values
      {name = "Redo", action = function() self:OnRedo() end, enabled = false, key = Keys.Y, toolTip = "Redo the last undo."}, -- Reset all the values
      {name = "Copy", action = function() self:OnCopySound() end, key = Keys.C, toolTip = "Copy the currently selected sound."}, -- Reset all the values
      {name = "Paste", action = function() self:OnPasteSound() end, key = Keys.V, enabled = false, toolTip = "Paste the last copied sound."}, -- Reset all the values
      {name = "Mutate", action = function() self:OnMutate() end, key = Keys.M, toolTip = "Mutate the sound to produce random variations."}, -- Reset all the values
      {divider = true},
      {name = "Quit", key = Keys.Q, action = function() self:OnQuit() end, toolTip = "Quit the current game."}, -- Quit the current game
    }

    if(PathExists(NewWorkspacePath(self.rootDirectory).AppendFile("code.lua"))) then
      table.insert(menuOptions, #menuOptions, {name = "Run Game", action = function() self:OnRunGame() end, key = Keys.R, toolTip = "Run the code for this game."})
    end

    pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")

end

function SFXTool:OnNewSound()
  gameEditor:NewSound(CurrentSoundID())

  -- Reload the sound
  LoadSound(CurrentSoundID())

  InvalidateData()
end

function SFXTool:OnSave()

  -- This will save the system data, the colors and color-map
  gameEditor:Save(self.rootDirectory, {SaveFlags.System, SaveFlags.Sounds})

  -- Display a message that everything was saved
  pixelVisionOS:DisplayMessage("You're changes have been saved.", 5)

  -- Clear the validation
  self:ResetDataValidation()

  -- Clear the sound cache
  self.originalSounds = {}

end

function SFXTool:OnRunGame()


  local parentPath = self.targetFilePath.ParentPath

  if(self.invalid == true) then

      pixelVisionOS:ShowSaveModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before running the game?", 160,
        -- Accept
        function(target)
          self:OnSave()
          LoadGame(parentPath.Path, data)
        end,
        -- Decline
        function (target)
          LoadGame(parentPath.Path, data)
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

function SFXTool:OnQuit()

  if(self.invalid == true) then

    pixelVisionOS:ShowSaveModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before you quit?", 160,
      -- Accept
      function(target)
        self:OnSave()
        QuitCurrentTool()
      end,
      -- Decline
      function (target)
        QuitCurrentTool()
      end,
      -- Cancel
      function(target)
        target.onParentClose()
      end
    )

  else
    -- Quit the tool
    QuitCurrentTool()
  end

end

function SFXTool:OnMutate()
  local id = self:CurrentSoundID()

  pixelVisionOS:BeginUndo(self)
  gameEditor:Mutate(id)
  pixelVisionOS:EndUndo(self)
  
  gameEditor:PlaySound(id, tonumber(self.channelIDStepper.inputField.text))

  self:LoadSound(id)

  self:InvalidateData()
end

function SFXTool:GetState()

  -- Current sfx ID
  local id = self:CurrentSoundID()
  
  -- Sfx setting string
  local data = gameEditor:Sound(id)

  local state = {
      
      Action = function()

        gameEditor:Sound(id, data)

        self:LoadSound(id)

        self:OnPlaySound()

        self:InvalidateData()
      end
  }

  return state

end

function SFXTool:OnCopySound()
  
  local id = self:CurrentSoundID()

  pixelVisionOS:SystemCopy(gameEditor:Sound(id))

  -- soundClipboard = {name = songNameFieldData.text, data = gameEditor:Sound(id)}

  pixelVisionOS:DisplayMessage("Sound data has been copied.", 5)

  pixelVisionOS:EnableMenuItem(self.PasteShortcut, true)
end

function SFXTool:OnPasteSound()
  
  local id = self:CurrentSoundID()
  
  local data = pixelVisionOS:SystemPaste()
  
  -- TODO need to validate the data

  pixelVisionOS:BeginUndo(self)

  gameEditor:Sound(id, data)

  pixelVisionOS:EndUndo(self)

  pixelVisionOS:DisplayMessage("New sound data has been pasted into sound '".. id .. "'.", 5)

  self:LoadSound(id)

  self:InvalidateData()

  self:OnPlaySound()

  pixelVisionOS:EnableMenuItem(self.PasteShortcut, false)

end