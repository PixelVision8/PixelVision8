function SFXTool:CreateWaveformPanel()

    self.waveButtonData = {
        {name = "Template1", spriteName = "wavebutton0", x = 112 - 24, y = 200, waveID = 0, toolTip = "Wave type square."},
        {name = "Template2", spriteName = "wavebutton1", x = 120, y = 200, waveID = 1, toolTip = "Wave type saw."},
        {name = "Template3", spriteName = "wavebutton3", x = 152, y = 200, waveID = 3, toolTip = "Wave type noise."},
        {name = "Template4", spriteName = "wavebutton4", x = 184, y = 200, waveID = 4, toolTip = "Wave type triangle."},
        {name = "Template5", spriteName = "wavebutton5", x = 216, y = 200, waveID = 5, toolTip = "Load wav sample file."},
    }

    self.waveGroupData = editorUI:CreateToggleGroup(true)
    self.waveGroupData.onAction = function(value)
      -- print("Select Wave Button", value)
      self:OnChangeWave(value)
      --TODO refresh wave buttons
      -- TODO save wave data
    end

    self.totalWaveButtons = #self.waveButtonData

    for i = 1, self.totalWaveButtons do

      local data = self.waveButtonData[i]

      -- TODO need to build sprite tables for each state
      editorUI:ToggleGroupButton(self.waveGroupData, {x = data.x, y = data.y}, data.spriteName, data.toolTip)

    end

    pixelVisionOS:RegisterUI({name = "UpdateWaveformPanel"}, "UpdateWaveformPanel", self)

end

function SFXTool:UpdateWaveformPanel()

    editorUI:UpdateToggleGroup(self.waveGroupData)

end

function SFXTool:OnChangeWave(value)

    self.currentWaveType = value
  
    self:UpdateLoadedSFX(1, self.waveButtonData[value].waveID)
  
    self:EnableKnobs(value)
  
  end

  function SFXTool:UpdateWaveButtons()

    editorUI:ClearGroupSelections(self.waveGroupData)
  
    local isWav = gameEditor:IsWav(self.currentID)
    local tmpID = isWav and 5 or 1
    local waveID = self.soundData[1]
  
    for i = 1, self.totalWaveButtons do
  
      local tmpButton = self.waveGroupData.buttons[i]
  
      local enabled = not isWav
  
      if(i == 5) then
        enabled = isWav
      end
  
      -- Enable the button
      editorUI:Enable(tmpButton, enabled)
  
      if(tonumber(waveID) == self.waveButtonData[i].waveID) then
        tmpID = i
      end
  
    end
  
    if(isWav) then
      tmpID = 5
    end
  
    self:EnableKnobs(tmpID)
  
    editorUI:SelectToggleButton(self.waveGroupData, tmpID, false)
  
  end
