local chipSelectorID = "ChipSelector"

function ChipEditorTool:CreateChipSelectorPanel()

  self.settingPanelInvalid = false
  self.editorMode = -1
  self.selectedLineDrawArgs = {nil, 0, 0, 0, false, false, DrawMode.Sprite}
  -- Create a toggle group for the pagination buttons
  self.chipEditorGroup = editorUI:CreateToggleGroup()
  self.chipEditorGroup.onAction = function(value) self:OnSelectChip(value) end


  -- Find the chips

  local gpuChipName = gameEditor:ReadMetadata("gpuChip", "Custom")
  local cartChipName = gameEditor:ReadMetadata("cartChip", "Custom")
  local soundChipName = gameEditor:ReadMetadata("soundChip", "Custom")

  -- GPU Chip
  local metaSpriteName = "chipcustomgpuup"

  for i = 1, #self.gpuChips do

    if(self.gpuChips[i].name == gpuChipName) then
      metaSpriteName = self.gpuChips[i].spriteName .. "up"
    end

  end

  self:CopyChipSprites(metaSpriteName, "chipgpuemptyup")

  -- Cart Chip
  metaSpriteName = "chipcustomcartup"

  for i = 1, #self.cartChips do

    if(self.cartChips[i].name == cartChipName) then
      metaSpriteName = self.cartChips[i].spriteName .. "up"
    end

  end

  self:CopyChipSprites(metaSpriteName, "chipcartemptyup")

  -- Sound Chip
  metaSpriteName = "chipcustomsoundup"

  for i = 1, #self.soundChips do

    if(self.soundChips[i].name == soundChipName) then
      metaSpriteName = self.soundChips[i].spriteName .. "up"
    end

  end

  self:CopyChipSprites(metaSpriteName, "chipsoundemptyup")

  -- if(runnerName ~= TuneVersion) then
  self.gpuButton = editorUI:ToggleGroupButton(self.chipEditorGroup, {x = 96, y = 48}, "chipgpuempty", "Click to edit the GPU chip.")
  self.gpuButton.hitRect = {
    x = 101,
    y = 49,
    w = 60,
    h = 60
  }
  -- end

  -- if(runnerName ~= DrawVersion and runnerName ~= TuneVersion) then
  self.cartButton = editorUI:ToggleGroupButton(self.chipEditorGroup, {x = 64, y = 40}, "chipcartempty", "Click to edit the cart chip.")
  -- end

  -- if(runnerName ~= DrawVersion) then
  self.soundButton = editorUI:ToggleGroupButton(self.chipEditorGroup, {x = 96, y = 120}, "chipsoundempty", "Click to edit the cart chip.")
  -- end


  self.chipPicker = editorUI:CreateChipPicker({x = 0, y = 24 - 8, w = 48, h = 152 + 8}, 32, 32)


  -- Set up line sprites
  -- selectedLineDrawArgs = {nil, 0, 0, 0, false, false, DrawMode.Sprite}



  -- TODO look into selecting the correct chip based on what mode PV8 is in
  self:OnSelectChip(0)


  pixelVisionOS:RegisterUI({name = chipSelectorID}, "ChipSelectorUpdate", self)

end

function ChipEditorTool:CopyChipSprites(srcMetaSpriteName, destMetaSpriteName)

  local srcMetaSprite = MetaSprite(FindMetaSpriteId(srcMetaSpriteName))

  local destMetaSprite = MetaSprite(FindMetaSpriteId(destMetaSpriteName))

  destMetaSprite.Clear()

  local tmpSpriteData = nil

  for i = 1, #srcMetaSprite.Sprites do
    tmpSpriteData = srcMetaSprite.Sprites[i]
    destMetaSprite.AddSprite(tmpSpriteData.Id, tmpSpriteData.X, tmpSpriteData.Y, tmpSpriteData.FlipH, tmpSpriteData.FlipV, tmpSpriteData.ColorOffset)
  end

end


function ChipEditorTool:ChipSelectorUpdate()

  if(self.editorMode > 0 and editorUI.collisionManager.mouseReleased == true and editorUI.collisionManager:MouseInRect(self.cancelSelectionRect)) then
    -- print("ChipSelectorUpdate")

    local overFlag = 0

    for i = 1, #self.chipEditorGroup.buttons do

      if(editorUI.collisionManager:MouseInRect(self.chipEditorGroup.buttons[i].hitRect)) then
        
        overFlag = i

        if(self.chipPicker.isDragging == true) then
          
          -- print("Dragging")
          if(self.chipEditorGroup.buttons[i].selected == true) then
            
            -- print("Copy chip settings", overFlag)

            local data = editorUI:ChipPickerSelectedData(self.chipPicker)

            self:OnReplaceChip(data, i)

          else

            overFlag = 0

            -- TODO this is a bit of a hack to disable this message when specs are locked and you get the warning message
            if(self.specsLocked == false) then

              local buttons = 
              {
                {
                  name = "modalokbutton",
                  action = function(target)
                    if(target.onParentClose ~= nil) then
                      target.onParentClose()
                    end
                    editorUI:ClearChipPickerSelection(self.chipPicker)
                  end,
                  key = Keys.Enter,
                  tooltip = "Press 'enter' to return"
                }
              }
              
              pixelVisionOS:ShowMessageModal( "Chip Error", "You can't use this chip here. Please drag the chip onto the open slot.", 160, buttons )

            end

          end

          return

        end

      end

    end

    if(overFlag == 0) then

      if(self.chipPicker.isDragging == false) then
        
        editorUI:ClearGroupSelections(self.chipEditorGroup)
      end

      self:OnSelectChip(0)

    end

    editorUI:ClearChipPickerSelection(self.chipPicker)

  end

  -- Update toggle groups
  editorUI:UpdateToggleGroup(self.chipEditorGroup)

  --
  editorUI:UpdateChipPicker(self.chipPicker)

end

function ChipEditorTool:OnSelectChip(value)

  if(self.editorMode ~= value) then
    self.settingPanelInvalid = true
  end

  self.editorMode = value

  if(self.editorMode == 0) then

    editorUI:CloseChipPicker(self.chipPicker)

    editorUI:ClearGroupSelections(self.chipEditorGroup)

    editorUI:ClearFocus()

    for i = 1, #self.inputFields do
      if(self.inputFields[i].editing) then
        editorUI:EditInputField(self.inputFields[i], false)
      end
    end

  else

    local targetButton = nil

    if(self.editorMode == 1) then

      editorUI:TextEditorInvalidateBuffer(self.displayWidthInputData)
      editorUI:TextEditorInvalidateBuffer(self.displayHeightInputData)
      --editorUI:TextEditorInvalidateBuffer(overscanRightInputData)
      --editorUI:TextEditorInvalidateBuffer(overscanBottomInputData)
      editorUI:TextEditorInvalidateBuffer(self.drawsInputData)

      editorUI:TextEditorInvalidateBuffer(self.cpsInputData)
      editorUI:TextEditorInvalidateBuffer(self.maskInputData)

      if(runnerName == DrawVersion) then
        editorUI:TextEditorInvalidateBuffer(self.spritePagesInputData)
      end

      self.selectedLineDrawArgs[1] = FindMetaSpriteId("chipgpuselectedline")--.spriteIDs
      self.selectedLineDrawArgs[2] = 128
      self.selectedLineDrawArgs[3] = 112
      -- selectedLineDrawArgs[4] = chipgpuselectedline.width

      editorUI:ConfigureChipPicker(self.chipPicker, self.gpuChips)

      targetButton = self.gpuButton

    elseif(self.editorMode == 2) then

      editorUI:TextEditorInvalidateBuffer(self.sizeInputData)
      editorUI:TextEditorInvalidateBuffer(self.spritePagesInputData)
      editorUI:TextEditorInvalidateBuffer(self.saveSlotsInputData)
      editorUI:TextEditorInvalidateBuffer(self.mapWidthInputData)
      editorUI:TextEditorInvalidateBuffer(self.mapHeightInputData)

      self.selectedLineDrawArgs[1] = FindMetaSpriteId("chipcartselectedline")--.spriteIDs
      self.selectedLineDrawArgs[2] = 64
      self.selectedLineDrawArgs[3] = 144
      -- selectedLineDrawArgs[4] = chipcartselectedline.width

      editorUI:ConfigureChipPicker(self.chipPicker, self.cartChips)

      targetButton = self.cartButton

    elseif(self.editorMode == 3) then

      editorUI:TextEditorInvalidateBuffer(self.soundTotalInputData)
      editorUI:TextEditorInvalidateBuffer(self.channelTotalInputData)
      editorUI:TextEditorInvalidateBuffer(self.songTotalInputData)
      editorUI:TextEditorInvalidateBuffer(self.loopTotalInputData)
      editorUI:TextEditorInvalidateBuffer(self.channelIDInputData.backButton)
      editorUI:TextEditorInvalidateBuffer(self.channelIDInputData.nextButton)
      editorUI:TextEditorInvalidateBuffer(self.waveStepper.backButton)
      editorUI:TextEditorInvalidateBuffer(self.waveStepper.nextButton)

      self.selectedLineDrawArgs[1] = FindMetaSpriteId("chipsoundselectedline")--.spriteIDs
      self.selectedLineDrawArgs[2] = 104
      self.selectedLineDrawArgs[3] = 144

      -- Reset this to 0 when opening up the tray
      editorUI:ChangeNumberStepperValue(self.channelIDInputData, 0)

      self:UpdateWaveStepper()

      editorUI:ConfigureChipPicker(self.chipPicker, self.soundChips)

      targetButton = self.soundButton

    end

    -- TODO need to configure chip picker

    if(self.specsLocked == true and self.showWarning == true) then

      -- Force the button to not redraw over the modal
      targetButton.invalid = false

      self:OnToggleLock()

    elseif(self.specsLocked == false) then
      editorUI:OpenChipPicker(self.chipPicker)
    end

  end

end

function ChipEditorTool:UpdateWaveStepper()
  --
  -- print(channelIDInputData.inputField.name, channelIDInputData.inputField.text, channelIDInputData.inputField.buffer[1])

  local value = tonumber(self.channelIDInputData.inputField.buffer[1])

  local type = gameEditor:ChannelType(value)

  -- local wavID = table.indexOf(waveTypeIDs, value)
  local validWaveID = table.indexOf(self.waveTypeIDs, type)

  -- print("Stepper Wav Raw Value", type, validWaveID)



  editorUI:ChangeStringStepperValue(self.waveStepper, self.validWaves[validWaveID], false, true)

end


function ChipEditorTool:OnReplaceChip(chipData, chipID)

  local buttons = 
  {
    {
      name = "modalyesbutton",
      action = function(target)
        self:ReplaceChip(chipData, chipID)
        target:onParentClose()
      end,
      key = Keys.Enter,
      tooltip = "Press 'enter' to apply the new chip values"
    },
    {
      name = "modalnobutton",
      action = function(target)
        target:onParentClose()
      end,
      key = Keys.Escape,
      tooltip = "Press 'esc' to not make any changes"
    }
  }

  pixelVisionOS:ShowMessageModal("Replace Chip", "Do you want to use the " .. chipData["name"] .. " chip? ".. chipData.message .. " This will affect " .. #chipData["fields"] .. " values and can not be undone.", 160, buttons)
  
end

function ChipEditorTool:ReplaceChip(chipData, chipID)

  local fields = chipData["fields"]

  for i = 1, #fields do

    local name = fields[i]["name"]
    local value = fields[i]["value"]

    local field = self[name]

    if(field ~= nil) then

      editorUI:ChangeInputField(field, value, true)

    end

  end

  if(chipData.colors ~= nil) then

    gameEditor:ClearColors(chipData.colors[1])

    local totalColors = #chipData.colors

    for i = 1, totalColors do
      gameEditor:Color(i - 1, chipData.colors[i])
    end

    self.invalidateColors = true

    local usePalettes = chipData.paletteMode

    -- Save palette mode to the info file for the color editor
    gameEditor:WriteMetadata("paletteMode", usePalettes and "true" or "false")

    if(usePalettes == true) then

      self.invalidateColorMap = true

      gameEditor:ReindexSprites()

      for i = 1, #chipData.palette do
        -- Find the palette start index
        local offset = ((i - 1) * 16) + 128
        for j = 1, 10 do
          gameEditor:Color(offset + (j - 1), chipData.palette[i][j])
        end

      end

    end

  end

  -- Reset channels
  for i = 1, gameEditor:TotalChannels() do

    local type = -1

    if(chipData.channels ~= nil) then

      if(i <= #chipData.channels) then
        type = chipData.channels[i]
      end

    end

    gameEditor:ChannelType(i - 1, type)

  end

  self:ChangeChipGraphic(chipID, chipData.spriteName, chipData.name)

  self:InvalidateData()
  -- Need to look for special values like colors and what not

end