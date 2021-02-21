-- Create table to store the workspace tool logic
SettingsTool = {}
SettingsTool.__index = SettingsTool


LoadScript("code-drop-down-menu")
LoadScript("code-key-code-map")
LoadScript("code-controller-panel")
LoadScript("code-display-panel")
LoadScript("code-monitor-panel")
LoadScript("code-shortcut-panel")
LoadScript("code-utils")

function SettingsTool:Init()

    -- Create a new table for the instance with default properties
    local _settingsTool = {
      toolName = "System Settings",
      runnerName = SystemName(),
      rootPath = ReadMetadata("RootPath", "/"),
      invalid = true,
      -- SaveShortcut = 6
    }
  
    -- Create a global reference of the new workspace tool
    setmetatable(_settingsTool, SettingsTool)
  
    -- Change the title
    pixelVisionOS:ChangeTitle(_settingsTool.toolName, "toolbaricontool")

    _settingsTool:CreateDropDownMenu()

    _settingsTool:CreateControllerPanel()

    _settingsTool:CreateMonitorPanel()

    _settingsTool:CreateDisplayPanel()

    _settingsTool:CreateShortcutPanel()
  
    -- Return the new instance of the editor ui
    return _settingsTool
  
end

function SettingsTool:Update(timeDelta)

  

end

function SettingsTool:Draw()

  

end

function SettingsTool:InvalidateData()

    -- Only everything if it needs to be
    if(self.invalid == true)then
      return
    end
  
    pixelVisionOS:ChangeTitle(toolName .."*", "toolbaricontool")
  
    self.invalid = true
  
    -- pixelVisionOS:EnableMenuItem(self.self.SaveShortcut, true)
  
  end
  
  function SettingsTool:ResetDataValidation()
  
    -- Only everything if it needs to be
    if(self.invalid == false)then
      return
    end
  
    pixelVisionOS:ChangeTitle(toolName, "toolbaricontool")
    self.invalid = false
  
    -- pixelVisionOS:EnableMenuItem(self.self.SaveShortcut, false)
  
  end
