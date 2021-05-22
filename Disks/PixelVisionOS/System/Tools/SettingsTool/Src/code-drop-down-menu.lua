function SettingsTool:CreateDropDownMenu()

    local menuOptions =
    {
        -- About ID 1
        {name = "About", action = function() pixelVisionOS:ShowAboutModal(self.toolName) end, toolTip = "Learn about PV8."},
        {divider = true},
        {name = "Sound Effects", action = function() self:ToggleSoundEffects() end, toolTip = "Toggle system sound."}, -- Reset all the values
        {divider = true},
        -- {name = "Save", action = function() self:OnSave() end, enabled = false, key = Keys.S, toolTip = "Save changes made to the controller mapping."}, -- Reset all the values
        {name = "Reset", action = function() self:OnReset() end, key = Keys.R, toolTip = "Revert controller mapping to its default value."}, -- Reset all the values
        {divider = true},
        {name = "Quit", key = Keys.Q, action = function() self:OnQuit() end, toolTip = "Quit the current game."}, -- Quit the current game
    }

    pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")

  end

  function SettingsTool:OnReset()
  
    local buttons = 
    {
        {
            name = "modalyesbutton",
            action = function(target)
                target.onParentClose()
                self:ResetControllers()
                self:ResetShortcuts()
            end,
            key = Keys.Enter,
            tooltip = "Press 'enter' to reset mapping to the default value"
        },
        {
            name = "modalnobutton",
            action = function(target)
                target.onParentClose()
            end,
            key = Keys.Escape,
            tooltip = "Press 'esc' to avoid making any changes"
        }
    }

    pixelVisionOS:ShowMessageModal("Reset Key Mapping", "Do you want to reset all of the controller and shortcut key mappings back to their default values?", 160, buttons)

  end
  
  function SettingsTool:OnQuit()
  
      QuitCurrentTool()
   
  end

function SettingsTool:ToggleSoundEffects()

    -- TODO need to add a message telling the user what is going to happen

    local playSounds = ReadBiosData("PlaySystemSounds", "True") == "True"

    WriteBiosData("PlaySystemSounds", playSounds and "False" or "True")

    pixelVisionOS:DisplayMessage("Turning " .. (playSounds and "off" or "on") .. " system sound effects.", 5)

end