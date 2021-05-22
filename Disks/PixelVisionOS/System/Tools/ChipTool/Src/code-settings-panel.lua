local settingsPanelID = "SettingsPanel"

function ChipEditorTool:CreateSettingsPanel()

  local mapSize = gameEditor:TilemapSize()
  local displaySize = gameEditor:DisplaySize()

  self.sizeInputData = pixelVisionOS:CreateInputField({x = 168, y = 208, w = 24}, gameEditor:GameMaxSize(), "CPG", "number")
  self.sizeInputData.min = 64
  self.sizeInputData.max = 960
  self.sizeInputData.onAction = function(value)
    gameEditor:GameMaxSize(tonumber(value))

    self:ChangeChipGraphic(2, "chipcustomcart")
    self:InvalidateData()
  end

  table.insert(self.inputFields, self.sizeInputData)

  self.spritePagesInputData = pixelVisionOS:CreateInputField({x = 128, y = 208, w = 8}, tostring(gameEditor:SpritePages()), "Total number of sprite pages. Each page has 256 sprites.", "number")
  self.spritePagesInputData.min = 1
  self.spritePagesInputData.max = 8
  self.spritePagesInputData.onAction = function(value)
    gameEditor:SpritePages(tonumber(value))

    self:ChangeChipGraphic(2, "chipcustomcart" )
    self:InvalidateData()
  end

  table.insert(self.inputFields, self.spritePagesInputData)


  self.cpsInputData = pixelVisionOS:CreateInputField({x = 136+8, y = 208, w = 16}, tostring(gameEditor:ColorsPerSprite()), "Number of colors per sprite. Minimum is 2 and max is 16.", "number")
  self.cpsInputData.min = 2
  -- TODO This should be tied to how many colors there are?
  self.cpsInputData.max = 16
  self.cpsInputData.onAction = OnChange
  self.cpsInputData.onAction = function(value)
    gameEditor:ColorsPerSprite(tonumber(value))

    self:ChangeChipGraphic(1, "chipcustomgpu" )
    self:InvalidateData()
  end

  table.insert(self.inputFields, self.cpsInputData)

  self.maskInputData = pixelVisionOS:CreateInputField({x = 136+8+32+8, y = 208, w = 6*8}, tostring(gameEditor:MaskColor():sub(2, 7)), "The color that will be treated as transparent.", "hex")

  self.maskInputData.captureInput = function(targetData)

    return editorUI:ValidateInputFieldText(targetData, InputString():upper())

  end

  self.maskInputData.onAction = function(value)
    gameEditor:MaskColor("#"..value)
    self:InvalidateData()
  end

  table.insert(self.inputFields, self.maskInputData)

  self.mapWidthInputData = pixelVisionOS:CreateInputField({x = 32, y = 208, w = 24}, tostring(mapSize.x), "Number of columns of tiles in the map.", "number")
  self.mapWidthInputData.min = math.ceil(displaySize.x / 8)
  self.mapWidthInputData.max = 256
  self.mapWidthInputData.onAction = function(value)

    value = tonumber(value)

    -- Make sure value is divisible by 32
    value = math.ceil(value / 4) * 4

    local size = gameEditor:TilemapSize()

    gameEditor:TilemapSize(value, size.y)

    self:ChangeChipGraphic(2, "chipcustomcart" )

    self:InvalidateData()

  end

  table.insert(self.inputFields, self.mapWidthInputData)


  self.mapHeightInputData = pixelVisionOS:CreateInputField({x = 72, y = 208, w = 24}, tostring(mapSize.y), "Number of rows of tiles in the map.", "number")
  self.mapHeightInputData.min = math.ceil(displaySize.y / 8)
  self.mapHeightInputData.max = 256
  self.mapHeightInputData.onAction = function(value)

    value = tonumber(value)

    local size = gameEditor:TilemapSize()

    gameEditor:TilemapSize(size.x, value)

    self:ChangeChipGraphic(2, "chipcustomcart" )

    self:InvalidateData()

  end

  table.insert(self.inputFields, self.mapHeightInputData)


  self.displayWidthInputData = pixelVisionOS:CreateInputField({x = 24, y = 208, w = 24}, tostring(displaySize.x), "The width in pixels of the display.", "number")
  self.displayWidthInputData.min = 64
  self.displayWidthInputData.max = 512
  self.displayWidthInputData.onAction = function(value)

    -- Convert value to number
    value = tonumber(value)

    -- Get the current size
    local size = gameEditor:DisplaySize()

    -- Update the display size with the new width and previous height
    gameEditor:DisplaySize(value, tonumber(self.displayHeightInputData.text))

    -- Make sure the map
    self.mapWidthInputData.min = math.ceil(value / 8)
    editorUI:ChangeInputField(self.mapWidthInputData, self.mapWidthInputData.text)


    self:ChangeChipGraphic(1, "chipcustomgpu" )

    self:InvalidateData()

  end

  table.insert(self.inputFields, self.displayWidthInputData)


  self.displayHeightInputData = pixelVisionOS:CreateInputField({x = 64, y = 208, w = 24}, tostring(displaySize.y), "The height in pixel of the display.", "number")
  self.displayHeightInputData.min = 64
  self.displayHeightInputData.max = 480
  self.displayHeightInputData.onAction = function(value)

    -- Convert value to number
    value = tonumber(value)

    -- Get the current size
    local size = gameEditor:DisplaySize()

    -- Update the display size with the new width and previous height
    gameEditor:DisplaySize(tonumber(self.displayWidthInputData.text), value)

    -- Make sure the map
    self.mapHeightInputData.min = math.ceil(value / 8)

    editorUI:ChangeInputField(self.mapHeightInputData, self.mapHeightInputData.text)

    self:ChangeChipGraphic(1, "chipcustomgpu" )

    self:InvalidateData()

  end

  table.insert(self.inputFields, self.displayHeightInputData)

  self.drawsInputData = pixelVisionOS:CreateInputField({x = 104, y = 208, w = 24}, tostring(gameEditor:MaxSpriteCount()), "Caps the total spites on the screen. Zero removes the limit.", "number")
  self.drawsInputData.min = 0
  self.drawsInputData.max = 512
  self.drawsInputData.onAction = function(value)
    gameEditor:MaxSpriteCount(tonumber(value))
    self:ChangeChipGraphic(1, "chipcustomgpu" )
    self:InvalidateData()
  end

  table.insert(self.inputFields, self.drawsInputData)



  self.soundTotalInputData = pixelVisionOS:CreateInputField({x = 16, y = 208, w = 16}, tostring(gameEditor:TotalSounds()), "Total number of sounds.", "number")
  self.soundTotalInputData.min = 8
  self.soundTotalInputData.max = 32
  self.soundTotalInputData.onAction = function(value)
    gameEditor:TotalSounds(tonumber(value))

    self:ChangeChipGraphic(3, "chipcustomsound" )
    self:InvalidateData()
  end

  table.insert(self.inputFields, self.soundTotalInputData)


  self.channelTotalInputData = pixelVisionOS:CreateInputField({x = 48, y = 208, w = 8}, tostring(gameEditor:TotalChannels()), "Total number of channels available to play sounds.", "number")
  self.channelTotalInputData.min = 1
  self.channelTotalInputData.max = 5
  self.channelTotalInputData.onAction = function(value)

    value = tonumber(value)

    gameEditor:TotalChannels(value)

    self:ChangeChipGraphic(3, "chipcustomsound" )

    -- Need to make sure that we don't have more tracks than channels
    self.channelIDInputData.inputField.max = value - 1
    editorUI:ChangeNumberStepperValue(self.channelIDInputData, self.channelIDInputData.inputField.text)
    self:InvalidateData()
  end

  table.insert(self.inputFields, self.channelTotalInputData)


  self.loopTotalInputData = pixelVisionOS:CreateInputField({x = 216, y = 208, w = 16}, tostring(gameEditor:TotalLoops()), "Total number of song patterns.", "number")
  self.loopTotalInputData.min = 8
  self.loopTotalInputData.max = 32
  self.loopTotalInputData.onAction = function(value)
    gameEditor:TotalLoops(tonumber(value))
    self:ChangeChipGraphic(3, "chipcustomsound" )
    self:InvalidateData()
  end

  table.insert(self.inputFields, self.loopTotalInputData)

  self.songTotalInputData = pixelVisionOS:CreateInputField({x = 184, y = 208, w = 16}, tostring(gameEditor:TotalSongs()), "Total number of songs.", "number")
  self.songTotalInputData.min = 8
  self.songTotalInputData.max = 32
  self.songTotalInputData.onAction = function(value)
    gameEditor:TotalSongs(tonumber(value))
    self:ChangeChipGraphic(3, "chipcustomsound" )
    self:InvalidateData()
  end

  table.insert(self.inputFields, self.songTotalInputData)


  self.channelIDInputData = editorUI:CreateNumberStepper({x = 72, y = 200}, 8, 0, 0, gameEditor:TotalChannels() - 1, "top", "Select a channel to preview its wave type.")
  self.channelIDInputData.onInputAction = function(value)

    self:UpdateWaveStepper()

  end

  self.waveStepper = editorUI:CreateStringStepper({x = 120, y = 200}, 16, "aa", self.validWaves, "top", "Change the length of the pattern.")

  self.waveStepper.inputField.editable = false
  self.waveStepper.inputField.forceCase = "upper"
  self.waveStepper.inputField.allowEmptyString = true

  self.waveStepper.onInputAction = function(value) self:UpdateWaveType(value) end

  self.saveSlotsInputData = pixelVisionOS:CreateInputField({x = 216, y = 208, w = 8}, tostring(gameEditor:GameSaveSlots()), "Enter the total save slots the disk can have.", "number")
  self.saveSlotsInputData.min = 2
  self.saveSlotsInputData.max = 8
  self.saveSlotsInputData.onAction = function(value)
    gameEditor:GameSaveSlots(tonumber(value))
    self:ChangeChipGraphic(2, "chipcustomcart" )
    self:InvalidateData()
  end

  table.insert(self.inputFields, self.saveSlotsInputData)

  pixelVisionOS:RegisterUI({name = settingsPanelID}, "SettingsPanelUpdate", self)

end

function ChipEditorTool:UpdateWaveType(value)

  value = table.indexOf(self.validWaves, value)

  gameEditor:ChannelType(tonumber(self.channelIDInputData.inputField.text), self.waveTypeIDs[value])

  -- print("UpdateWaveType", value, waveTypeIDs[value])

  self.waveStepper.inputField.toolTip = self.waveToolTips[value]

  self:ChangeChipGraphic(3, "chipcustomsound" )

  self:InvalidateData()

end

function ChipEditorTool:SettingsPanelUpdate()

  -- TODO this should use the UI delayed draw API
  if(self.settingPanelInvalid == true) then
    
    DrawRect(1, 176, 254, 56, BackgroundColor(), DrawMode.TilemapCache)
    
    self.settingPanelInvalid = false

    if(self.editorMode == 0) then

      local panelSprites = FindMetaSpriteId("settingspanel")
      local maxColorPos = {x = 144, y = 184}
      local totalColorPos = {x = 144, y = 189}
      local spritePagePos = {x = 200, y = 184}
      local cpsPos = {x = 200, y = 189}
      local channelsPos = {x = 144, y = 208}
      local soundsPos = {x = 144, y = 213}
      local musicChannels = {x = 212, y = 208}
      local musicLoops = {x = 212, y = 213}

      DrawMetaSprite(panelSprites, 40, 166 + 8, false, false, DrawMode.TilemapCache)

      -- Display
      DrawText(self.displayWidthInputData.text, 64, 184, DrawMode.TilemapCache, "small", 15, - 4)
      DrawText(self.displayHeightInputData.text, 64, 189, DrawMode.TilemapCache, "small", 15, - 4)

      -- Tilemap
      DrawText(self.mapWidthInputData.text, 72, 208, DrawMode.TilemapCache, "small", 15, - 4)
      DrawText(self.mapHeightInputData.text, 72, 213, DrawMode.TilemapCache, "small", 15, - 4)

      -- Colors

      local uniqueColors = {"#FF00FF"}
      local tmpColor = nil

      for i = 1, gameEditor:TotalColors() do
        tmpColor =   gameEditor:Color(i)
        if(table.indexOf(uniqueColors, tmpColor) == -1) then
          table.insert(uniqueColors, tmpColor)
        end
      end

      DrawText(#uniqueColors - 1, maxColorPos.x, maxColorPos.y, DrawMode.TilemapCache, "small", 15, - 4)
      DrawText(math.ceil(gameEditor:TotalColors() / 64), totalColorPos.x, totalColorPos.y, DrawMode.TilemapCache, "small", 15, - 4)

      -- Sprites
      DrawText(gameEditor:SpritePages(), spritePagePos.x, spritePagePos.y, DrawMode.TilemapCache, "small", 15, - 4)
      DrawText(gameEditor:ColorsPerSprite(), cpsPos.x, cpsPos.y, DrawMode.TilemapCache, "small", 15, - 4)

      -- SFX
      DrawText(gameEditor:TotalChannels(), channelsPos.x, channelsPos.y, DrawMode.TilemapCache, "small", 15, - 4)
      DrawText(gameEditor:TotalSounds(), soundsPos.x, soundsPos.y, DrawMode.TilemapCache, "small", 15, - 4)

      -- Music
      DrawText(gameEditor:TotalChannels(), musicChannels.x, musicChannels.y, DrawMode.TilemapCache, "small", 15, - 4)
      DrawText(gameEditor:TotalLoops(), musicLoops.x, musicLoops.y, DrawMode.TilemapCache, "small", 15, - 4)
      --   end

    else

      
      DrawRect(40, 166 + 8, 176, 48, BackgroundColor(), DrawMode.TilemapCache)

      local metaSpriteName = nil

      if(self.editorMode == 1) then
        metaSpriteName = "settingsgpu"

      elseif(self.editorMode == 2) then
        metaSpriteName = "settingscart"

      elseif(self.editorMode == 3) then
        metaSpriteName = "settingssound"
      end

      if(metaSpriteName ~= nil) then
        DrawMetaSprite(FindMetaSpriteId(metaSpriteName), 8, 176, false, false, DrawMode.TilemapCache)
      end
      
    end

  end

  -- Draw Settings
  if(self.editorMode == 1) then
    editorUI:UpdateInputField(self.displayWidthInputData)
    editorUI:UpdateInputField(self.displayHeightInputData)
    editorUI:UpdateInputField(self.drawsInputData)
    editorUI:UpdateInputField(self.cpsInputData)
    editorUI:UpdateInputField(self.maskInputData)

  -- Cart Settings 
  elseif(self.editorMode == 2) then
    editorUI:UpdateInputField(self.sizeInputData)
    editorUI:UpdateInputField(self.spritePagesInputData)
    editorUI:UpdateInputField(self.saveSlotsInputData)
    editorUI:UpdateInputField(self.mapWidthInputData)
    editorUI:UpdateInputField(self.mapHeightInputData)

  -- Sound Settings
  elseif(self.editorMode == 3) then
    editorUI:UpdateInputField(self.soundTotalInputData)
    editorUI:UpdateInputField(self.channelTotalInputData)
    editorUI:UpdateInputField(self.loopTotalInputData)
    editorUI:UpdateInputField(self.songTotalInputData)
    editorUI:UpdateStepper(self.channelIDInputData)
    editorUI:UpdateStepper(self.waveStepper)
  end

end

function ChipEditorTool:ChangeChipGraphic(chipID, chipSpriteName, name)

    name = name or "Custom"

    local button = self.chipEditorGroup.buttons[chipID]
    local spriteName = self.chipSpriteNames[chipID]

    self:CopyChipSprites(chipSpriteName .. "up", spriteName .. "up")

    editorUI:Invalidate(button)

    -- Save the chip name to the info.json file
    if(chipID == 1) then
        gameEditor:WriteMetadata("gpuChip", name)
    elseif(chipID == 2) then
        gameEditor:WriteMetadata("cartChip", name)
    elseif(chipID == 3) then
        gameEditor:WriteMetadata("soundChip", name)
    end

end
