function SFXTool:CreateEditPanel()

    self.knobData = {

        {name = "Volume", x = 40, y = 88, propID = 24, range = 1, toolTip = "Volume is set to "},
      
        -- Envelope
        {name = "AttackTime", x = 80, y = 88, propID = 2, range = 1, toolTip = "Attack Time is set to ", disabledOffset = 28},
        {name = "SustainTime", x = 104, y = 88, propID = 3, range = 1, toolTip = "Sustain Time is set to ", disabledOffset = 28},
        {name = "SustainPunch", x = 128, y = 88, propID = 4, range = 1, toolTip = "Sustain Punch is set to ", disabledOffset = 28},
        {name = "DecayTime", x = 152, y = 88, propID = 5, range = 1, toolTip = "Decay Time is set to ", disabledOffset = 28},
      
        -- Frequency
        {name = "StartFrequency", x = 192, y = 88, propID = 6, range = 1, toolTip = "Start Frequency is set to ", disabledOffset = 28},
        {name = "MinFrequency", x = 216, y = 88, propID = 7, range = 1, toolTip = "Minimum Frequency is set to ", disabledOffset = 28},
      
        -- Slide
        {name = "Slide", x = 16, y = 128, propID = 8, range = 2, toolTip = "Slide is set to "},
        {name = "DeltaSlide", x = 40, y = 128, propID = 9, range = 2, toolTip = "Delta Slide is set to "},
      
        -- Vibrato
        {name = "VibratoDepth", x = 72, y = 128, propID = 10, range = 1, toolTip = "Vibrato Depth is set to "},
        {name = "VibratoSpeed", x = 96, y = 128, propID = 11, range = 1, toolTip = "Vibrato Speed is set to "},
      
        -- Harmonics
        {name = "OverTones", x = 128, y = 128, propID = 12, range = 1, toolTip = "Over Tones is set to "},
        {name = "OverTonesFalloff", x = 152, y = 128, propID = 13, range = 1, toolTip = "Over Tones Falloff is set to "},
      
        -- Square Wave
        {name = "SquareDuty", x = 192, y = 128, propID = 14, range = 1, toolTip = "Square Duty is set to ", disabledOffset = 32},
        {name = "DutySweep", x = 216, y = 128, propID = 15, range = 2, toolTip = "Duty Sweep is set to ", disabledOffset = 32},
      
        -- Repeat
        {name = "RepeatSpeed", x = 72, y = 168, propID = 16, range = 1, toolTip = "Repeat Speed is set to "},
      
        -- Phaser
        {name = "PhaserOffset", x = 16, y = 168, propID = 17, range = 2, toolTip = "Phaser Offset is set to "},
        {name = "PhaserSweep", x = 40, y = 168, propID = 18, range = 2, toolTip = "Phaser Sweep is set to "},
      
        -- LP Filter
        {name = "LPFilterCutoff", x = 104, y = 168, propID = 19, range = 1, toolTip = "LP Filter Cutoff is set to "},
        {name = "LPFilterCutoffSweep", x = 128, y = 168, propID = 20, range = 2, toolTip = "LP Filter Cutoff Sweep is set to "},
        {name = "LPFilterResonance", x = 152, y = 168, propID = 21, range = 1, toolTip = "LP Filter Resonance is set to "},
      
        -- HP Filter
        {name = HPFilterCutoff, x = 192, y = 168, propID = 22, range = 1, toolTip = "HP Filter Cutoff is set to "},
        {name = HPFilterCutoffSweep, x = 216, y = 168, propID = 23, range = 2, toolTip = "HP Filter Cutoff Sweep is set to "},
      
    }

    self.totalKnobs = #self.knobData

    for i = 1, self.totalKnobs do

      local data = self.knobData[i]

      data.knobUI = editorUI:CreateKnob({x = data.x, y = data.y, w = 24, h = 24}, "knob", "Change the volume.")
      data.knobUI.type = data.name

      if(data.disabledOffset ~= nil) then
        data.knobUI.colorOffsetDisabled = data.disabledOffset
      end

      data.knobUI.onAction = function(value)

        local type = data.name
        local propID = data.propID

        -- Calculate new value based on range
        local newValue = (data.range * value) - (data.range - 1)

        self:UpdateLoadedSFX(propID, newValue)

        self:UpdateKnobTooltip(data, value)
      end

    end

    pixelVisionOS:RegisterUI({name = "UpdateEditPanel"}, "UpdateEditPanel", self)

end

function SFXTool:UpdateEditPanel()

    for i = 1, self.totalKnobs do

        local data = self.knobData[i].knobUI
  
        -- TODO go through and make sure the value is correct, then update
        editorUI:UpdateKnob(data)
  
      end

      if(editorUI.collisionManager.mouseDown == false and self.playSound == true) then
        self.playSound = false
        self:ApplySoundChanges(true)
      end

end

function SFXTool:EnableKnobs(waveID)

    local enableSquarePanel = waveID == 1
  
    local isWav = gameEditor:IsWav(self.currentID)
  
    for i = 2, self.totalKnobs do
  
      local tmpKnob = self.knobData[i]
  
      editorUI:Enable(tmpKnob.knobUI, isWav == false)
  
      if(tmpKnob.name == "SquareDuty" or tmpKnob.name == "DutySweep") then
        editorUI:Enable(tmpKnob.knobUI, enableSquarePanel)
      end
  
    end
  
    -- Update any panels
  
    local spriteData = FindMetaSpriteId(enableSquarePanel == true and "squarewavepanelenabled" or "squarewavepaneldisabled")
  
    -- if(spriteData ~= nil) then
      DrawMetaSprite(spriteData, 23, 14, false, false, DrawMode.Tile)
      -- DrawSprites(spriteData.spriteIDs, 23, 14, spriteData.width, false, false, DrawMode.Tile)
    -- end
  
  end

  function SFXTool:UpdateLoadedSFX(propID, value)

    self.soundData[propID] = tostring(value)
  
    if(self.playButton.enabled) then
        self.playSound = true
    end
    -- OnPlaySound()
  
  end

  function SFXTool:UpdateKnobTooltip(knobData, value)

    -- print("value * 100" ,value * 100)
    local percentString = string.lpad(tostring(math.floor(value * 100)), 3, "0") .. "%"
  
    knobData.knobUI.toolTip = knobData.toolTip .. percentString .. "."
  
  end


function SFXTool:UpdateSound(settings, autoPlay, addToHistory)
    local id = self:CurrentSoundID()
  
    gameEditor:Sound(id, settings)
  
    if(autoPlay ~= false) then
      gameEditor:PlaySound(self:CurrentSoundID(), tonumber(self.channelIDStepper.inputField.text))
    end
  
    -- Reload the sound data
    self:LoadSound(id, false, addToHistory)
  
    self:InvalidateData()
  
  end

  function SFXTool:ApplySoundChanges(autoPlay)

    -- Save sound changes
    local settingsString = ""
    local total = #self.soundData
  
    pixelVisionOS:BeginUndo(self)

    --print("total", total)
    for i = 1, total do
      local value = self.soundData[i]
      if(value ~= "" or value ~= nil) then
        settingsString = settingsString .. self.soundData[i]
      end
      if(i < total) then
        settingsString = settingsString .. ","
      end
    end
  
    local id = self:CurrentSoundID()
    gameEditor:Sound(id, settingsString)
    self:InvalidateData()
  
    pixelVisionOS:EndUndo(self)

    if(autoPlay ~= false) then
      self:OnPlaySound()
    end
  
  end