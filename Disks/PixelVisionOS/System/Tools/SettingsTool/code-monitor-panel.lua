
function SettingsTool:CreateMonitorPanel()

    self.volumeKnobData = editorUI:CreateKnob({x = 16, y = 192, w = 24, h = 24}, "knob", "Change the volume.")
    self.volumeKnobData.onAction = function(value) self:OnVolumeChange(value) end 
    self.volumeKnobData.value = Volume() / 100

    -- editorUI:Enable(self.volumeKnobData, not Mute())

    self.brightnessKnobData = editorUI:CreateKnob({x = 40, y = 192, w = 24, h = 24}, "knob", "Change the brightness.")
    self.brightnessKnobData.onAction = function(value) self:OnBrightnessChange(value) end  
    self.brightnessKnobData.value = (Brightness() - .5)

    self.sharpnessKnobData = editorUI:CreateKnob({x = 64, y = 192, w = 24, h = 24}, "knob", "Change the sharpness.")
    self.sharpnessKnobData.onAction = function(value) self:OnSharpnessChange(value) end 
    self.sharpnessKnobData.value = (((Sharpness() * - 1) / 6))

    
    pixelVisionOS:RegisterUI({name = "UpdateMonitorPanel"}, "UpdateMonitorPanel", self)

  end

function SettingsTool:OnVolumeChange(value)

  local newValue = Volume(math.ceil(value * 100))

  WriteBiosData("Volume", newValue)

  pixelVisionOS:DisplayMessage(string.format("Volume is now set to %03d%%.", newValue))

  self.playSound = true

end


  function SettingsTool:UpdateMonitorPanel()

    -- TOTO Check for modal

    editorUI:UpdateKnob(self.volumeKnobData)
    editorUI:UpdateKnob(self.brightnessKnobData)
    editorUI:UpdateKnob(self.sharpnessKnobData)

    if(editorUI.collisionManager.mouseDown == false and self.playSound == true) then

      -- unmute
      if(Volume() > 0 and Mute() == true) then
        Mute(false)
      end

      PlayRawSound("0,1,,.2,,.2,.3,.1266,,,,,,,,,,,,,,,,,,1,,,,,,")
      self.playSound = false
    end

    -- Modify mute buttons if global value changes
    local newMuteValue = Mute()

    if(self.lastMuteValue ~= newMuteValue) then
      self.lastMuteValue = newMuteValue
      editorUI:ChangeKnob(self.volumeKnobData, Volume() / 100, true)
      pixelVisionOS.titleBar.muteInvalid = true
    end

    local newCRTValue = EnableCRT()

    if(self.lastCRTValue ~= newCRTValue) then
      self.lastCRTValue = newCRTValue

        self.sharpnessKnobData.value = (((Sharpness() * - 1) / 6))
        self.brightnessKnobData.value = (Brightness() - .5)

        editorUI:Enable(self.brightnessKnobData, newCRTValue)
        editorUI:Enable(self.sharpnessKnobData, newCRTValue)

    end

  end

function SettingsTool:OnBrightnessChange(value)

  local newValue = (value * 1) + .5

  Brightness(newValue)

end

function SettingsTool:OnSharpnessChange(value)

  local newValue = ((value * 4) + 2) * - 1

  Sharpness(newValue)

end