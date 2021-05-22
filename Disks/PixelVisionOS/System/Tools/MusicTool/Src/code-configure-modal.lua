ConfigureModal = {}
ConfigureModal.__index = ConfigureModal

function ConfigureModal:Init()

  local _configureModal = {} -- our new object
  setmetatable(_configureModal, ConfigureModal) -- make Account handle lookup


  _configureModal:Configure()
  -- _configureModal.currentSelection = 1
  -- _configureModal.message = message

  return _configureModal

end

function ConfigureModal:Configure()

  -- Reset the modal so it redraws correctly when opened
  self.firstRun = nil

  local width = 224
  local height = 200

  self.canvas = NewCanvas(width, height)

  local displaySize = Display()

  self.title = "Configure Generator"

  self.rect = {
    x = math.floor(((displaySize.x - width) * .5) / 8) * 8,
    y = math.floor(((displaySize.y - height) * .5) / 8) * 8,
    w = width,
    h = height
  }

  self.selectionValue = false


  self.configData = {

    selectedTrack = 0,
    speedRange = {
      min = 0,
      max = 0
    },
    density = 0,
    funk = 0,
    layering = 0,
    sfxIDs = {},
    instruments = {},
    octMinValues = {},
    octMaxValues = {},
    scale = 0
  }

  self.instrumentMap = {
    {id = 0, name = "MELODY"},
    {id = 3, name = "DRUMS"},
    {id = 6, name = "SNARE"},
    {id = 2, name = "BASS"},
    -- {id = 8, name = "Random"},
    {id = 1, name = "HARMONY"},
    {id = 4, name = "LEAD"},
    {id = 7, name = "KICK"},
    {id = 5, name = "PAD"},
    -- {id = 9, name = "None"}
  }

  self.instrumentData = {

    -- Melody
    {
      octMin = 1,
      octMax = 5,
      value = 0,
      help = "Melody - Sets the track's sfx to the 'melody' template."
    },
    -- Drums
    {
      octMin = 1,
      octMax = 5,
      value = 1,
      help = "Drums - Sets the track's sfx to the 'Drums' template."
    },
    -- Snare
    {
      octMin = 4,
      octMax = 8,
      value = 2,
      help = "Snare - Sets the track's sfx to the 'Snare' template."
    },
    -- Bass
    {
      octMin = 2,
      octMax = 6,
      value = 3,
      help = "Bass - Sets the track's sfx to the 'Bass' template."
    },
    -- Random
    {
      octMin = 1,
      octMax = 9,
      value = 4,
      help = "Random - Sets the track to 'Random' instrument."
    },
    -- Harmony
    {
      octMin = 4,
      octMax = 8,
      value = 5,
      help = "Harmony - Sets the track's sfx to the 'Harmony' template."
    },
    -- Lead
    {
      octMin = 3,
      octMax = 7,
      value = 6,
      help = "Lead - Sets the track's sfx to the 'Lead' template."
    },
    -- Kick
    {
      octMin = 3,
      octMax = 7,
      value = 7,
      help = "Kick - Sets the track's sfx to the 'Kick' template."
    },
    -- Pad
    {
      octMin = 5,
      octMax = 9,
      value = 8,
      help = "Pad - Sets the track's sfx to the 'Pad' template."
    },
    -- None
    {
      octMin = 2,
      octMax = 9,
      value = 9,
      help = "None - Sets the track's sfx to the 'Pad' template."
    }

  }

  self.scaleMap = {
    {id = 3, name = "MelodicMinorUp", help = "Select Melodic Minor Up"},
    {id = 0, name = "Major", help = "Select Major"},
    {id = 6, name = "NaturalMinor", help = "Select Natural Minor"},

    {id = 2, name = "MelodicMinorDown", help = "Select Melodic Minor Down"},
    {id = 10, name = "Dorian", help = "Select Dorian"},
    {id = 11, name = "HarmonicMinor", help = "Select Harmonic Minor"},

    {id = 4, name = "MinorPentatonic", help = "Select Minor Pentatonic"},
    {id = 9, name = "Diatonic", help = "Select Diatonic"},
    {id = 5, name = "Mixolydian", help = "Select Mixolydian"},

    {id = 1, name = "MajorPentatonic", help = "Select Major Pentatonic"},
    {id = 8, name = "Chromatic", help = "Select Chromatic"},
    {id = 7, name = "AhavaRaba", help = "Select Ahava Raba"},
  }

  self.currentTrackSettings = -1

  self.invalidateUI = {}

end

function ConfigureModal:Open()

  if(self.firstRun == nil) then

    pixelVisionOS:CreateModalChrome(self.canvas, self.title)
    -- -- Draw the black background
    -- self.canvas:SetStroke(5, 1)
    -- self.canvas:SetPattern({0}, 1, 1)
    -- self.canvas:DrawRectangle(0, 0, self.canvas.width, self.canvas.height, true)

    -- -- Draw the brown background
    -- self.canvas:SetStroke(12, 1)
    -- self.canvas:SetPattern({11}, 1, 1)
    -- self.canvas:DrawRectangle(3, 9, self.canvas.width - 6, self.canvas.height - 12, true)

    -- local tmpX = (self.canvas.width - (#self.title * 4)) * .5

    -- self.canvas:DrawText(self.title:upper(), tmpX, 1, "small", 15, - 4)

    -- -- draw highlight stroke
    -- self.canvas:SetStroke(15, 1)
    -- self.canvas:DrawLine(3, 9, self.canvas.width - 5, 9)
    -- self.canvas:DrawLine(3, 9, 3, self.canvas.height - 5)


    -- Draw message text
    -- local wrap = WordWrap(self.message, (self.rect.w / 4) - 4)
    -- local lines = SplitLines(wrap)
    -- local total = #self.lines
    -- local startX = 8--
    -- local startY = 16--self.rect.y + 8
    --
    -- -- We want to render the text from the bottom of the screen so we offset it and loop backwards.
    -- for i = 1, total do
    --   self.canvas:DrawText(self.lines[i]:upper(), startX, (startY + ((i - 1) * 8)), "medium", 0, - 4)
    -- end


    self.buttons = {}

    -- TODO Create button states?

    local buttonSize = {x = 32, y = 16}

    -- TODO center ok button when no cancel button is shown
    local bX = (self.rect.w - buttonSize.x - 8)

    -- snap the x value to the grid
    bX = math.floor((bX + self.rect.x) / 8) * 8

    -- Fix the button to the bottom of the window
    local bY = math.floor(((self.rect.y + self.rect.h) - buttonSize.y - 8) / 8) * 8

    local backBtnData = self.editorUI:CreateButton({x = bX, y = bY}, "modalokbutton", "")

    backBtnData.onAction = function()

      -- Set value to true when ok is pressed
      self.selectionValue = true

      if(self.onParentClose ~= nil) then
        self.onParentClose()
      end
    end

    table.insert(self.buttons, backBtnData)

    -- Offset the bX value and snap to the grid
    bX = math.floor((bX - buttonSize.x - 8) / 8) * 8

    local cancelBtnData = self.editorUI:CreateButton({x = bX, y = bY}, "modalcancelbutton", "")

    cancelBtnData.onAction = function()

      -- Set value to true when cancel is pressed
      self.selectionValue = false

      -- Close the panel
      if(self.onParentClose ~= nil) then
        self.onParentClose()
      end
    end

    table.insert(self.buttons, cancelBtnData)

    -- Settings
    self.settingTrackInputData = self.editorUI:CreateInputField({x = self.rect.x + 40, y = self.rect.y + 40, w = 8}, "0", "Enter a track number to edit.", "number")
    -- self.settingTrackInputData.colorOffset = 32
    -- self.settingTrackInputData.disabledColorOffset = 34
    self.settingTrackInputData.onAction = function(value)
      self:SelectSettingsTrack(value)
    end

    table.insert(self.invalidateUI, self.settingTrackInputData)


    --
    self.previousSettingTrackButtonData = self.editorUI:CreateButton({x = self.rect.x + 24, y = self.rect.y + 32}, "topstepperback", "Move to the previous track.")
    self.previousSettingTrackButtonData.onAction = function(value)
      self:PreviousSettingsTrack()
    end

    table.insert(self.invalidateUI, self.previousSettingTrackButtonData)

    self.nextSettingTrackButtonData = self.editorUI:CreateButton({x = self.rect.x + 48, y = self.rect.y + 32}, "topsteppernext", "Move to the next track.")
    self.nextSettingTrackButtonData.onAction = function(value)
      self:NextSettingsTrack()
    end

    table.insert(self.invalidateUI, self.nextSettingTrackButtonData)

    --
    self.settingSFXInputData = self.editorUI:CreateInputField({x = self.rect.x + 88, y = self.rect.y + 40, w = 16}, "0", "Enter a SFX number for the track to use.", "number")
    -- self.settingSFXInputData.colorOffset = 32
    -- self.settingSFXInputData.disabledColorOffset = 34
    self.settingSFXInputData.onAction = function(value)


      self.configData.sfxIDs[self.currentTrackSettings + 1] = tonumber(value)

    end

    table.insert(self.invalidateUI, self.settingSFXInputData)

    self.settingOctMinInputData = self.editorUI:CreateInputField({x = self.rect.x + 128, y = self.rect.y + 40, w = 16}, "0", "Enter a minimum octave for this track.", "number")
    -- self.settingOctMinInputData.colorOffset = 32
    self.settingOctMinInputData.min = 1
    self.settingOctMinInputData.max = 10
    -- self.settingOctMinInputData.disabledColorOffset = 34

    table.insert(self.invalidateUI, self.settingOctMinInputData)


    self.settingOctMaxInputData = self.editorUI:CreateInputField({x = self.rect.x + 176, y = self.rect.y + 40, w = 16}, "0", "Enter a maximum octave for this track.", "number")
    -- self.settingOctMaxInputData.colorOffset = 32
    self.settingOctMaxInputData.min = 1
    self.settingOctMaxInputData.max = 10
    -- self.settingOctMaxInputData.disabledColorOffset = 34
    table.insert(self.invalidateUI, self.settingOctMaxInputData)

    self.settingOctMinInputData.onAction = function(value)

      local min = tonumber(value)
      local max = tonumber(self.settingOctMaxInputData.text)

      if(min > max) then
        min = max
        self.editorUI:ChangeInputField(self.settingOctMinInputData, tostring(min), false)
      end

      self.configData.octMinValues[self.currentTrackSettings + 1] = tonumber(value)

      -- gameEditor:ConfigTrackOctaveRange(currentTrackSettings, NewPoint(min, max))

    end

    self.settingOctMaxInputData.onAction = function(value)

      local min = tonumber(self.settingOctMinInputData.text)
      local max = tonumber(value)

      if(max < min) then
        max = min
        self.editorUI:ChangeInputField(self.settingOctMaxInputData, tostring(max), false)
      end

      -- gameEditor:ConfigTrackOctaveRange(currentTrackSettings, NewPoint(min, max))
      self.configData.octMaxValues[self.currentTrackSettings + 1] = tonumber(value)


    end

    -- Check boxes
    self.trackCheckboxGroupData = self.editorUI:CreateToggleGroup(true)
    self.trackCheckboxGroupData.onAction = function(data, value)
      self:OnChangeTrackInstrument(data, value)
    end

    local y = 64
    local cols = 4
    local columnX = {40, 80, 120, 160, 192}
    local col = 1


    for i = 1, #self.instrumentMap do

      local btn = self.editorUI:ToggleGroupButton(self.trackCheckboxGroupData, {x = self.rect.x + columnX[col], y = self.rect.y + y, w = 8, h = 8}, "radiobutton", "Change the current track's instrument to ", true)
      local data = self.instrumentMap[i]
      btn.instrumentName = data.name
      btn.instrumentID = data.id
      btn.toolTip = btn.toolTip .. data.name .. "."

      table.insert(self.invalidateUI, btn)

      col = col + 1
      if(i % cols == 0) then
        y = y + 8
        col = 1
      end
    end

    self.settingSpeenMinInputData = self.editorUI:CreateInputField({x = self.rect.x + 16, y = self.rect.y + 112, w = 24}, tostring(gameEditor.pcgMinTempo), "Enter the minimum speed (tempo) value.", "number")
    -- self.settingSpeenMinInputData.colorOffset = 32
    -- self.settingSpeenMinInputData.disabledColorOffset = 34
    self.settingSpeenMinInputData.min = 50
    self.settingSpeenMinInputData.max = 350
    self.settingSpeenMinInputData.onAction = function(value)

      value = tonumber(value)

      local maxValue = tonumber(self.settingSpeenMaxInputData.text)

      if(value > maxValue) then
        value = maxValue
      end

      -- gameEditor.pcgMinTempo = value

      self.configData.speedRange.min = value

      self.editorUI:ChangeInputField(self.settingSpeenMinInputData, tostring(value), false)

    end

    table.insert(self.invalidateUI, self.settingSpeenMinInputData)



    self.settingSpeenMaxInputData = self.editorUI:CreateInputField({x = self.rect.x + 72, y = self.rect.y + 112, w = 24}, tostring(gameEditor.pcgMaxTempo), "Enter the maximum speed (tempo) value.", "number")
    -- self.settingSpeenMaxInputData.colorOffset = 32
    -- self.settingSpeenMaxInputData.disabledColorOffset = 34
    self.settingSpeenMaxInputData.min = 50
    self.settingSpeenMaxInputData.max = 350
    self.settingSpeenMaxInputData.onAction = function(value)
      -- gameEditor.pcgMaxTempo = tonumber(value)

      value = tonumber(value)

      local minValue = tonumber(self.settingSpeenMinInputData.text)

      if(value < minValue) then
        value = minValue
      end

      self.configData.speedRange.max = value
      -- gameEditor.pcgMaxTempo = value

      self.editorUI:ChangeInputField(self.settingSpeenMaxInputData, tostring(value), false)

    end

    table.insert(self.invalidateUI, self.settingSpeenMaxInputData)


    self.settingDensityInputData = self.editorUI:CreateInputField({x = self.rect.x + 112, y = self.rect.y + 112, w = 16}, tostring(gameEditor.pcgDensity), "Changes the chance each beta in the song contains a note.", "number")
    self.settingDensityInputData.min = 1
    self.settingDensityInputData.max = 10
    -- settingDensityInputData.colorOffset = 32
    -- settingDensityInputData.disabledColorOffset = 34
    self.settingDensityInputData.onAction = function(value)
      -- gameEditor.pcgDensity = tonumber(value)
      self.configData.density = tonumber(value)
    end

    table.insert(self.invalidateUI, self.settingDensityInputData)


    self.settingFunkInputData = self.editorUI:CreateInputField({x = self.rect.x + 144, y = self.rect.y + 112, w = 16}, tostring(gameEditor.pcgFunk), "Likeliness bass is more boxed-in than the melody or harmony.", "number")
    self.settingFunkInputData.min = 1
    self.settingFunkInputData.max = 10
    -- settingFunkInputData.colorOffset = 32
    -- settingFunkInputData.disabledColorOffset = 34
    self.settingFunkInputData.onAction = function(value)
      -- gameEditor.pcgFunk = tonumber(value)
      self.configData.funk = tonumber(value)
    end

    table.insert(self.invalidateUI, self.settingFunkInputData)


    self.settingLayeringInputData = self.editorUI:CreateInputField({x = self.rect.x + 176, y = self.rect.y + 112, w = 16}, tostring(gameEditor.pcgLayering), "Adjusts the likelihood instruments will playing at once.", "number")
    self.settingLayeringInputData.min = 1
    self.settingLayeringInputData.max = 10
    -- settingLayeringInputData.colorOffset = 32
    -- settingLayeringInputData.disabledColorOffset = 34
    self.settingLayeringInputData.onAction = function(value)
      -- gameEditor.pcgLayering = tonumber(value)
      self.configData.layering = tonumber(value)
    end

    table.insert(self.invalidateUI, self.settingLayeringInputData)


    self.typeCheckboxGroupData = self.editorUI:CreateToggleGroup(true)
    self.typeCheckboxGroupData.onAction = function(value)
      --gameEditor.scale = scaleMap[value].id

      self.configData.scale = self.scaleMap[value].id

    end

    local y = 136
    local cols = 3
    local col = 1



    local columnX = {8, 96, 144}

    for i = 1, 12 do

      local btn = self.editorUI:ToggleGroupButton(self.typeCheckboxGroupData, {x = self.rect.x + columnX[col], y = self.rect.y + y, w = 8, h = 8}, "radiobutton", self.scaleMap[i].help)

      table.insert(self.invalidateUI, btn)

      col = col + 1
      if(i % cols == 0) then
        y = y + 8
        col = 1
      end
    end

    -- self:OnResetConfig()

    self.firstRun = false;

  else
    local total = #self.invalidateUI
    for i = 1, total do
      self.editorUI:Invalidate(self.invalidateUI[i])
    end
  end

  for i = 1, #self.buttons do
    self.editorUI:Invalidate(self.buttons[i])
  end

  self.canvas:DrawPixels(self.rect.x, self.rect.y, DrawMode.TilemapCache)


  DrawMetaSprite(FindMetaSpriteId("settingspanel"), self.rect.x + 8, self.rect.y + 16, false, false, DrawMode.TilemapCache)

  self:UpdateConfigData()

  self:UpdateFields()

  self:SelectSettingsTrack(0)

end

function ConfigureModal:Close()

  if(self.selectionValue == true) then

    gameEditor:ConfigTrackOctaveRange(self.currentTrackSettings, NewPoint(tonumber(self.settingOctMinInputData.text), tonumber(self.settingOctMaxInputData.text)))

    gameEditor.pcgMinTempo = tonumber(self.settingSpeenMinInputData.text)

    gameEditor.pcgMaxTempo = tonumber(self.settingSpeenMaxInputData.text)

    gameEditor.pcgDensity = tonumber(self.settingDensityInputData.text)

    gameEditor.pcgFunk = tonumber(self.settingFunkInputData.text)

    gameEditor.pcgLayering = tonumber(self.settingLayeringInputData.text)

    gameEditor.scale = self.scaleMap[self.typeCheckboxGroupData.currentSelection].id

    local total = gameEditor:TotalTracks()

    for i = 1, total do

      local trackID = i - 1

      gameEditor:ConfigTrackSFX(trackID, self.configData.sfxIDs[i])
      gameEditor:ConfigTrackInstrument(trackID, self.configData.instruments[i])

      -- print("Saving Range", self.configData.octMinValues[i], self.configData.octMaxValues[i])
      -- TODO this is not working
      gameEditor:ConfigTrackOctaveRange(trackID, NewPoint(self.configData.octMinValues[i], self.configData.octMaxValues[i]))

    end

    -- self.editorUI:UpdateInputField(self.settingFunkInputData)


    -- TODO need a way to disable the input fields
    for i = 1, #self.invalidateUI do
      local tmpUI = self.invalidateUI[i]
      -- print("Shutdown", i, tmpUI.name, tmpUI.editing)
      if(tmpUI.editing ~= nil) then
        self.editorUI:EditTextEditor(tmpUI, false)

      end
      self.editorUI:ResetValidation(tmpUI)
      --   print("Disable field", i, self.invalidateUI[i].editing)
      --   -- if(self.invalidateUI[i].editing == true)then
      --
      --   -- self.editorUI:EditInputField(self.invalidateUI[i], false)
      --   self.editorUI:ResetValidation(self.invalidateUI[i])
      --   -- end
      --
    end

  end
  -- print("Modal Close")
  -- if(self.onParentClose ~= nil) then
  --   self.onParentClose()
  -- end

  -- TODO go through data object and save values

end

function ConfigureModal:Update(timeDelta)

  for i = 1, #self.buttons do
    self.editorUI:UpdateButton(self.buttons[i])
  end

  self.editorUI:UpdateInputField(self.settingTrackInputData)
  self.editorUI:UpdateInputField(self.settingSFXInputData)
  self.editorUI:UpdateInputField(self.settingOctMinInputData)
  self.editorUI:UpdateInputField(self.settingOctMaxInputData)
  self.editorUI:UpdateInputField(self.settingSpeenMinInputData)
  self.editorUI:UpdateInputField(self.settingSpeenMaxInputData)
  self.editorUI:UpdateInputField(self.settingDensityInputData)
  self.editorUI:UpdateInputField(self.settingFunkInputData)
  self.editorUI:UpdateInputField(self.settingLayeringInputData)
  --
  self.editorUI:UpdateToggleGroup(self.trackCheckboxGroupData)
  self.editorUI:UpdateToggleGroup(self.typeCheckboxGroupData)
  --
  self.editorUI:UpdateButton(self.previousSettingTrackButtonData)
  self.editorUI:UpdateButton(self.nextSettingTrackButtonData)

end

function ConfigureModal:Draw()

end

function ConfigureModal:UpdateConfigData()

  -- Get the total numver of tracks
  local total = gameEditor:TotalTracks()

  -- Loop through each track and set up the values
  for i = 1, total do

    -- Adjust of track IDs being 0 based
    local track = i - 1

    -- get the SFX ID
    local sfxID = gameEditor:ConfigTrackSFX(track) -- TODO need to make this read the actual data from the music chip
    -- local sfx = gameEditor:TrackInstrument(i)
    local instID = gameEditor:ReadInstrumentID(track) + 1

    local range = gameEditor:ConfigTrackOctaveRange(track)

    -- print("instID", instID)

    self:UpdateConfigInstrument(i, instID, sfxID, range)

  end

  -- Set this to the middle of the road
  self.configData.density = gameEditor.pcgDensity
  self.configData.funk = gameEditor.pcgFunk
  self.configData.layering = gameEditor.pcgLayering

  self.configData.speedRange.min = gameEditor.pcgMinTempo
  self.configData.speedRange.max = gameEditor.pcgMaxTempo

  self.configData.scale = gameEditor.scale







end

function ConfigureModal:UpdateFields()

  self.editorUI:ChangeInputField(self.settingDensityInputData, self.configData.density)
  self.editorUI:ChangeInputField(self.settingFunkInputData, self.configData.funk)
  self.editorUI:ChangeInputField(self.settingLayeringInputData, self.configData.layering)

  self.editorUI:ChangeInputField(self.settingSpeenMinInputData, self.configData.speedRange.min)
  self.editorUI:ChangeInputField(self.settingSpeenMaxInputData, self.configData.speedRange.max)

  -- local scaleValue =

  for i = 1, #self.scaleMap do
    local id = self.scaleMap[i].id

    if(id == self.configData.scale) then
      self.editorUI:SelectToggleButton(self.typeCheckboxGroupData, i)
      break
    end

  end

  --

end

function ConfigureModal:UpdateConfigInstrument(track, value, sfxID, octRange)

  -- print("UpdateConfigInstrument", track, value, sfxID, octRange.x, octRange.y)

  local instData = self.instrumentData[value]
  --
  -- if(instData ~= nil) then
  --
  --   local instID = instData.value

  self.configData.instruments[track] = instData ~= nil and instData.value or 0

  -- print("UpdateConfigInstrument", track, self.configData.instruments[track])

  self.configData.sfxIDs[track] = sfxID

  -- if(octRange)

  self.configData.octMinValues[track] = octRange.x
  self.configData.octMaxValues[track] = octRange.y

  -- print("Config", dump(self.configData))
  -- end

end

-- function ConfigureModal:SelectConfigScale(id)
--   configData.scale = id
--
--   -- TODO need to read this from the data?
--   configData.speedRange.min = 50
--   configData.speedRange.max = 250
-- end

function ConfigureModal:OnResetConfig()

  -- self.editorUI:ChangeInputField(self.settingDensityInputData, "5")
  -- self.editorUI:ChangeInputField(self.settingFunkInputData, "5")
  -- self.editorUI:ChangeInputField(self.settingLayeringInputData, "5")
  --
  -- self.editorUI:ChangeInputField(self.settingSpeenMinInputData, "120")
  -- self.editorUI:ChangeInputField(self.settingSpeenMaxInputData, "320")
  --
  -- self.editorUI:SelectToggleButton(self.typeCheckboxGroupData, 2)

  local configTrackData = 
  {
    "0,0,6,6",
    "1,1,6,7",
    "2,2,3,4",
    "3,3,6,6"
  }

  for i = 1, #configTrackData do

    local trackID = i - 1

    local trackData = configTrackData[i]

    local values = ExplodeSettings(trackData)
    gameEditor:ConfigTrackSFX(trackID, values[1])
    gameEditor:ConfigTrackInstrument(trackID, values[2])
    gameEditor:ConfigTrackOctaveRange(trackID, NewPoint(values[3], values[4]))

  end

  -- self:SelectSettingsTrack(0)

  -- gameEditor.pcgMinTempo = configData.speedRange.min
  -- gameEditor.pcgMaxTempo = configData.speedRange.max

end

function ConfigureModal:SelectSettingsTrack(value)

  -- print("SelectSettingsTrack", value)

  value = tonumber(value)

  -- Make sure the track ID is never greater than the total number of tracks
  value = Repeat(value, gameEditor:TotalTracks())

  self.currentTrackSettings = value

  -- gameEditor:ConfigTrackSFX(trackID, self.configData.sfxIDs[i])
  -- gameEditor:ConfigTrackInstrument(trackID, self.configData.instruments[i])
  -- gameEditor:ConfigTrackOctaveRange(trackID, self.configData.octMinValues[i], self.configData.octMaxValues[i])




  -- local octRange = gameEditor:ConfigTrackOctaveRange(self.currentTrackSettings)

  -- Update the input fields without trigger their actions
  self.editorUI:ChangeInputField(self.settingTrackInputData, tostring(value), false)

  -- Need to offset the value for lua's 1 based index
  value = value + 1


  local instrument = self.configData.instruments[value]

  -- print("Select instrument", instrument, dump(self.configData.instruments))
  --
  local sfx = self.configData.sfxIDs[value]--gameEditor:ConfigTrackSFX(self.currentTrackSettings)
  local octMin = self.configData.octMinValues[value]
  local octMax = self.configData.octMaxValues[value]

  -- TODO there is a race condition where this is called before the config data has all the track info
  self.editorUI:ChangeInputField(self.settingSFXInputData, tostring(sfx), false)

  self.editorUI:ChangeInputField(self.settingOctMinInputData, tostring(octMin), false)
  self.editorUI:ChangeInputField(self.settingOctMaxInputData, tostring(octMax), false)

  for i = 1, #self.trackCheckboxGroupData.buttons do

    if(self.trackCheckboxGroupData.buttons[i].instrumentID == instrument ) then
      self.editorUI:SelectToggleButton(self.trackCheckboxGroupData, i, false)
    end

  end

end

-- Select the next track
function ConfigureModal:NextSettingsTrack()
  self:SelectSettingsTrack(self.currentTrackSettings + 1)
end

-- Select the previous track
function ConfigureModal:PreviousSettingsTrack()
  self:SelectSettingsTrack(self.currentTrackSettings - 1)
end

function ConfigureModal:OnChangeTrackInstrument(data, value)


  local instID = editorUI:ToggleGroupCurrentSelection(self.trackCheckboxGroupData, value).instrumentID
  --
  -- print("OnChangeTrackInstrument", instID)

  local trackID = self.currentTrackSettings + 1

  -- local instrument = self.configData.sfxIDs[trackID]
  -- local octMin = self.configData.octMinValues[trackID]
  -- local octMax = self.configData.octMaxValues[trackID]

  self.configData.instruments[trackID] = instID
  --
  local instData = self.instrumentData[self.trackCheckboxGroupData.currentSelection]

  -- print("instIndex", instIndex, self.trackCheckboxGroupData.currentSelection)
  -- print("set instrument", trackID, self.configData.instruments[trackID])
  --
  -- print("config", dump(self.configData.instruments))
  -- gameEditor:ConfigTrackInstrument(self.currentTrackSettings, instID)

  -- TODO not sure why I was reading the track octave here
  -- local octRange = gameEditor:ConfigTrackOctaveRange(currentTrackSettings)
  editorUI:ChangeInputField(self.settingOctMinInputData, tostring(instData.octMin))
  editorUI:ChangeInputField(self.settingOctMaxInputData, tostring(instData.octMax))

  -- print("enable", enable, instID == 8, instID == 9)
  editorUI:Enable(self.settingOctMinInputData, instID ~= 8)
  editorUI:Enable(self.settingOctMaxInputData, instID ~= 8)

  if(instID ~= 8 and instID ~= 9)then
    gameEditor:PreviewInstrument(instID)
  end

end

