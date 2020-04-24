-- Create table to store the workspace tool logic
SettingsTool = {}
SettingsTool.__index = SettingsTool

LoadScript("code-key-code-map")

function SettingsTool:Init()

    -- Create an global instance of the Pixel Vision OS
    pixelVisionOS = PixelVisionOS:Init()
  
    -- Used for debugging
    -- pixelVisionOS.displayFPS = true
  
    -- Get a global reference to the Editor UI
    editorUI = pixelVisionOS.editorUI
  
    -- Create a new table for the instance with default properties
    local _settingTool = {
      toolName = "System Settings",
      runnerName = SystemName(),
      rootPath = ReadMetadata("RootPath", "/"),
      invalid = true,
      SaveShortcut = 6
    }
  
    -- Create a global reference of the new workspace tool
    setmetatable(_settingTool, SettingsTool)
  
    -- Update title if it running from the disk
    if(string.starts(_settingTool.rootPath, "/Disks/")) then
      _settingTool.toolName = _settingTool.toolName .. " (DISK)"
    end
  
    -- Change the title
    pixelVisionOS:ChangeTitle(_settingTool.toolName, "toolbaricontool")

    _settingTool:CreateDropDownMenu()
  
    -- Return the new instance of the editor ui
    return _settingTool
  
end

function SettingsTool:Update(timeDelta)

end

-- function SettingsTool:Draw()

-- end

function SettingsTool:InvalidateData()

    -- Only everything if it needs to be
    if(invalid == true)then
      return
    end
  
    pixelVisionOS:ChangeTitle(toolName .."*", "toolbaricontool")
  
    invalid = true
  
    pixelVisionOS:EnableMenuItem(self.SaveShortcut, true)
  
  end
  
  function SettingsTool:ResetDataValidation()
  
    -- Only everything if it needs to be
    if(invalid == false)then
      return
    end
  
    pixelVisionOS:ChangeTitle(toolName, "toolbaricontool")
    invalid = false
  
    pixelVisionOS:EnableMenuItem(self.SaveShortcut, false)
  
  end

  function SettingsTool:CreateDropDownMenu()

    local menuOptions =
    {
        -- About ID 1
        {name = "About", action = function() pixelVisionOS:ShowAboutModal(toolName) end, toolTip = "Learn about PV8."},
        {divider = true},
        {name = "Sound Effects", action = ToggleSoundEffects, toolTip = "Toggle system sound."}, -- Reset all the values
        {divider = true},
        {name = "Save", action = OnSave, enabled = false, key = Keys.S, toolTip = "Save changes made to the controller mapping."}, -- Reset all the values
        {name = "Reset", action = OnReset, key = Keys.R, toolTip = "Revert controller mapping to its default value."}, -- Reset all the values
        {divider = true},
        {name = "Quit", key = Keys.Q, action = OnQuit, toolTip = "Quit the current game."}, -- Quit the current game
    }

    pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")

  end

  function SettingsTool:CreatePreferencePanel()

    self.volumeKnobData = editorUI:CreateKnob({x = 16, y = 192, w = 24, h = 24}, "knob", "Change the volume.")
    self.volumeKnobData.onAction = OnVolumeChange
    self.volumeKnobData.value = Volume() / 100

    editorUI:Enable(self.volumeKnobData, not Mute())

    self.brightnessKnobData = editorUI:CreateKnob({x = 40, y = 192, w = 24, h = 24}, "knob", "Change the brightness.")
    self.brightnessKnobData.onAction = OnBrightnessChange
    self.brightnessKnobData.value = (Brightness() - .5)

    self.sharpnessKnobData = editorUI:CreateKnob({x = 64, y = 192, w = 24, h = 24}, "knob", "Change the sharpness.")
    self.sharpnessKnobData.onAction = OnSharpnessChange
    self.sharpnessKnobData.value = (((Sharpness() * - 1) / 6))

    self.scaleInputData = editorUI:CreateInputField({x = 112, y = 200, w = 8}, Scale(), "This changes the scale of the window when not in fullscreen.", "number")
    self.scaleInputData.min = 1
    self.scaleInputData.max = 4
    self.scaleInputData.onAction = OnChangeScale


    self.fullScreenCheckBoxData = editorUI:CreateToggleButton({x = 128, y = 192, w = 8, h = 8}, "checkbox", "Toggle full screen mode.")
    self.fullScreenCheckBoxData.hitRect = {x = 131, y = 192, w = 8, h = 8}
    self.fullScreenCheckBoxData.onAction = function(value)
        Fullscreen(value)

        WriteBiosData("FullScreen", value == true and "True" or "False")
    end
    editorUI:ToggleButton(self.fullScreenCheckBoxData, Fullscreen(), false)
    
    self.cropCheckBoxData = editorUI:CreateToggleButton({x = 128, y = 200, w = 8, h = 8}, "checkbox", "Enable the window to crop.")
    self.cropCheckBoxData.onAction = function (value)
        CropScreen(value)

        WriteBiosData("CropScreen", value == true and "True" or "False")
    end
    
    editorUI:ToggleButton(self.cropCheckBoxData, CropScreen())
    
    self.stretchCheckBoxData = editorUI:CreateToggleButton({x = 128, y = 208, w = 8, h = 8}, "checkbox", "Stretch the display to fit the window.")
    
    self.stretchCheckBoxData.onAction = function (value)
        StretchScreen(value)

        WriteBiosData("StretchScreen", value == true and "True" or "False")
    end
    
    editorUI:ToggleButton(self.stretchCheckBoxData, StretchScreen())
    
    self.crtToggleButton = editorUI:CreateToggleButton({x = 128, y = 216, w = 8, h = 8}, "checkbox", "Toggle the CRT effect.")
    
    self.crtToggleButton.onAction = function (value)
        OnToggleCRT(value)

        WriteBiosData("CRT", value == true and "True" or "False")
    end
    
    editorUI:ToggleButton(self.crtToggleButton, EnableCRT())

  end

  function SettingsTool:UpdatePrefPanel()

    -- TOTO Check for modal

    editorUI:UpdateKnob(volumeKnobData)
    editorUI:UpdateKnob(brightnessKnobData)
    editorUI:UpdateKnob(sharpnessKnobData)

    -- Update toggle groups
    editorUI:UpdateButton(fullScreenCheckBoxData)
    editorUI:UpdateButton(cropCheckBoxData)
    editorUI:UpdateButton(stretchCheckBoxData)
    editorUI:UpdateButton(crtToggleButton)

  end
