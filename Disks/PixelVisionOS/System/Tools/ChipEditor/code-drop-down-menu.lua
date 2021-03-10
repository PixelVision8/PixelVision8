--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- SaveShortcut = 5

function ChipEditorTool:CreateDropDownMenu()

    -- Find the json editor
    self.textEditorPath = pixelVisionOS:FindEditors()["json"]

   local menuOptions = 
   {
     -- About ID 1
     {name = "About", action = function() pixelVisionOS:ShowAboutModal(self.toolName) end, toolTip = "Learn about PV8."},
     {divider = true},
     {name = "Edit JSON", enabled = self.textEditorPath ~= nil, action = function() self:OnEditJSON() end, toolTip = "Edit the raw info file's json data."}, -- Reset all the values
     {name = "Toggle Lock", enabled = true, action = function() self:OnToggleLock() end, toolTip = "Lock or unlock the system specs for editing."}, -- Reset all the values
     {name = "Save", key = Keys.S, action = function() self:OnSave() end, toolTip = "Save changes."},
     {divider = true}, -- Reset all the values
     {name = "Quit", key = Keys.Q, action = function() self:OnQuit() end, toolTip = "Quit the current game."}, -- Quit the current game
   }

   pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")

end

function ChipEditorTool:OnEditJSON()

  if(self.invalid == true) then

    pixelVisionOS:ShowSaveModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before you edit the raw data file?", 160,
      -- Accept
      function(target)
        self:OnSave()
        self:EditJSON()
      end,
      -- Decline
      function (target)
        self:EditJSON()
      end,
      -- Cancel
      function(target)
        target.onParentClose()
      end
    )

  else
      -- Quit the tool
      self:EditJSON()
  end

end

function ChipEditorTool:EditJSON()

  local metaData = {
      directory = self.rootDirectory,
      file = self.rootDirectory .. "data.json",
  }

  LoadGame(self.textEditorPath, metaData)

end

function ChipEditorTool:OnQuit()

  if(self.invalid == true) then

    pixelVisionOS:ShowSaveModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before you quit?", 160,
      -- Accept
      function(target)
        self:OnSave()
        QuitCurrentTool()
      end,
      -- Decline
      function (target)
        -- Quit the tool
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

function ChipEditorTool:OnSave()

    local flags = {SaveFlags.System, SaveFlags.Meta}

    if(self.invalidateColors == true) then

        table.insert(flags, SaveFlags.Colors)
        self.invalidateColors = false
    end

    -- TODO need to save music and sounds when those are broken out
    gameEditor:Save(self.rootDirectory, flags)

    -- Display that the data was saved and reset invalidation
    pixelVisionOS:DisplayMessage("The game's 'data.json' file has been updated.", 5)

    self:ResetDataValidation()

end

function ChipEditorTool:OnToggleLock()

    self.specsLocked = gameEditor:GameSpecsLocked()
  
    local title = self.specsLocked == true and "Unlock" or "Lock"
  
    local buttons = 
    {
      {
        name = "modalyesbutton",
        action = function(target)
          self.specsLocked = not self.specsLocked
          gameEditor:GameSpecsLocked(self.specsLocked)
          self:UpdateFieldLock()
          self:InvalidateData()

          if(target.onParentClose ~= nil) then
            target.onParentClose()
          end

          self.showWarning = true

        end,
        key = Keys.Enter,
        tooltip = "Press 'enter' to unlock the chips for editing"
      },
      {
        name = "modalnobutton",
        action = function(target)
          if(target.onParentClose ~= nil) then
            target.onParentClose()
          end

          self.showWarning = false
        end,
        key = Keys.Enter,
        tooltip = "Press 'esc' to keep the chips locked and view their values"
      }
    }
    
    pixelVisionOS:ShowMessageModal(title .. " System Specs", "Are you sure you want to ".. title .." the system specs?", 160, buttons)
   
  end

  function ChipEditorTool:UpdateFieldLock()

    self.showWarning = self.specsLocked
  
    for i = 1, #self.inputFields do
      editorUI:Enable(self.inputFields[i], not self.specsLocked)
    end
  
    editorUI:EnableStepper(self.waveStepper, not self.specsLocked)
  
  end