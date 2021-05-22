function SFXTool:CreateInstrumentPanel()

    self.sfxButtonData = {
        {name = "pickup", spriteName = "sfxbutton1", x = 8, y = 40, toolTip = "Create a randomized 'pickup' or coin sound effect."},
        {name = "explosion", spriteName = "sfxbutton2", x = 24, y = 40, toolTip = "Create a randomized 'explosion' sound effect."},
        {name = "powerup", spriteName = "sfxbutton3", x = 40, y = 40, toolTip = "Create a randomized 'power-up' sound effect."},
        {name = "shoot", spriteName = "sfxbutton4", x = 56, y = 40, toolTip = "Create a randomized 'laser' or 'shoot' sound effect."},
        {name = "jump", spriteName = "sfxbutton5", x = 72, y = 40, toolTip = "Create a randomized 'jump' sound effect."},
        {name = "hurt", spriteName = "sfxbutton6", x = 88, y = 40, toolTip = "Create a randomized 'hit' or 'hurt' sound effect."},
        {name = "select", spriteName = "sfxbutton7", x = 104, y = 40, toolTip = "Create a randomized 'blip' or 'select' sound effect."},
        {name = "random", spriteName = "sfxbutton8", x = 120, y = 40, toolTip = "Create a completely random sound effect."},
        {name = "melody", spriteName = "instrumentbutton1", x = 8, y = 56, toolTip = "Create a 'melody' instrument sound effect."},
        {name = "harmony", spriteName = "instrumentbutton2", x = 24, y = 56, toolTip = "Create a 'harmony' instrument sound effect."},
        {name = "bass", spriteName = "instrumentbutton3", x = 40, y = 56, toolTip = "Create a 'bass' instrument sound effect."},
        {name = "pad", spriteName = "instrumentbutton4", x = 56, y = 56, toolTip = "Create a 'pad' instrument sound effect."},
        {name = "lead", spriteName = "instrumentbutton5", x = 72, y = 56, toolTip = "Create a 'lead' instrument sound effect."},
        {name = "drums", spriteName = "instrumentbutton6", x = 88, y = 56, toolTip = "Create a 'drums' instrument sound effect."},
        {name = "snare", spriteName = "instrumentbutton7", x = 104, y = 56, toolTip = "Create a 'snare' instrument sound effect."},
        {name = "kick", spriteName = "instrumentbutton8", x = 120, y = 56, toolTip = "Create a 'kick' instrument sound effect."}
    }

    self.totalSFXButtons = #self.sfxButtonData

    for i = 1, self.totalSFXButtons do

      local data = self.sfxButtonData[i]

      -- TODO need to build sprite tables for each state
      data.buttonUI = editorUI:CreateButton({x = data.x, y = data.y}, data.spriteName, data.toolTip)
      data.buttonUI.onAction = function()
        -- print("Click")
        self:OnSFXAction(data.name)
      end

    end

    pixelVisionOS:RegisterUI({name = "UpdateInstrumentPanel"}, "UpdateInstrumentPanel", self)

end

function SFXTool:UpdateInstrumentPanel()

    for i = 1, self.totalSFXButtons do

        local data = self.sfxButtonData[i].buttonUI
  
        editorUI:UpdateButton(data)
  
    end

end

function SFXTool:OnSFXAction(name)

    -- print("OnSFX", name)
  
    if(name == "pickup") then
      self:OnSoundTemplatePress(1)
    elseif(name == "explosion") then
        self:OnSoundTemplatePress(3)
    elseif(name == "powerup") then
        self:OnSoundTemplatePress(4)
    elseif(name == "shoot") then
        self:OnSoundTemplatePress(2)
    elseif(name == "jump") then
        self:OnSoundTemplatePress(6)
    elseif(name == "hurt") then
        self:OnSoundTemplatePress(5)
    elseif(name == "select") then
        self:OnSoundTemplatePress(7)
    elseif(name == "random") then
        self:OnSoundTemplatePress(8)
    elseif(name == "melody") then
        self:OnInstrumentTemplatePress(1)
    elseif(name == "harmony") then
        self:OnInstrumentTemplatePress(2)
    elseif(name == "bass") then
        self:OnInstrumentTemplatePress(3)
    elseif(name == "drums") then
        self:OnInstrumentTemplatePress(4)
    elseif(name == "lead") then
        self:OnInstrumentTemplatePress(5)
    elseif(name == "pad") then
        self:OnInstrumentTemplatePress(6)
    elseif(name == "snare") then
        self:OnInstrumentTemplatePress(7)
    elseif(name == "kick") then
        self:OnInstrumentTemplatePress(8)
    end
  
  end

  function SFXTool:OnSoundTemplatePress(value)

    local id = self:CurrentSoundID()

    pixelVisionOS:BeginUndo(self)

    gameEditor:GenerateSound(id, value)
  
    pixelVisionOS:EndUndo(self)

    if(self.playButton.enabled) then
      gameEditor:PlaySound(id)
    end
  
    -- Reload the sound data
    self:LoadSound(id)
  
    self:InvalidateData()
  end

  function SFXTool:OnInstrumentTemplatePress(value)

    local template = nil
  
    if(value == 1) then
      -- Melody
      template = "0,,.2,,.2,.1266,,,,,,,,,,,,,1,,,,,.5"
    elseif(value == 2) then
      -- Harmony
      template = "0,,.01,,.509,.1266,,,,,,,,.31,,,,,1,,,.1,,.5";
    elseif(value == 3) then
      -- Bass
      template = "4,,.01,,.509,.1266,,,,,,,,.31,,,,,1,,,.1,,1";
    elseif(value == 4) then
      -- Drums
      template = "3,,.01,,.209,.1668,,,,,,,,.31,,,,,.3,,,.1,,.5";
    elseif(value == 5) then
      -- Lead
      template = "4,.6,.01,,.609,.1347,,,,,.2,,,.31,,,,,1,,,.1,,.5";
    elseif(value == 6) then
      -- Pad
      template = "4,.5706,.4763,.0767,.8052,.1266,,,-.002,,.1035,.2062,,,-.0038,.8698,-.0032,,.6377,.1076,,.0221,.0164,.5";
    elseif(value == 7) then
      -- Snare
      template = "3,.032,.11,.6905,.4,.1668,.0412,-.2434,.0259,.1296,.4162,.069,.7284,.5,-.213,.0969,-.1699,.8019,.1452,-.0715,.3,.1509,.9632,.5";
    elseif(value == 8) then
      -- Kick
      template = "4,,.2981,.1079,.1122,.1826,.0583,-.2287,.1341,.3666,.0704,.1626,.2816,.0642,.3733,.2103,-.3137,-.3065,.8693,-.3045,.4969,.0218,-.015,.6";
  
    end
  
    if(template ~= nil) then
      self:UpdateSound(template)
    end
  
  end

  function SFXTool:UpdateSoundTemplates()

    -- Loop through template buttons
    self.totalSFXButtons = #self.sfxButtonData
  
    local enabled = true
  
  
    if(gameEditor:IsWav(self.currentID)) then
      enabled = false
    end
  
    for i = 1, self.totalSFXButtons do
  
      -- local data = self.sfxButtonData[i].buttonUI
      editorUI:Enable(self.sfxButtonData[i].buttonUI, enabled)
  
    end
  
  end