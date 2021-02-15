--[[
	Pixel Vision 8 - Debug Tool
	Copyright (C) 2016, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- Load in the editor framework script to access tool components
-- LoadScript("sb-sprites")
LoadScript("pixel-vision-os-v2")
LoadScript("code-chip-picker")
LoadScript("code-chip-templates")

-- Create an global instance of the Pixel Vision OS
-- _G["pixelVisionOS"] = PixelVisionOS:Init()

local editorMode = -1
local toolName = "Chip Editor"
local inputFields = {}
local specsLocked = false

local DrawVersion, TuneVersion = "Pixel Vision 8 Draw", "Pixel Vision 8 Tune"
local runnerName = SystemName()

-- TODO for testing
-- runnerName = TuneVersion

local validWaves = {
  "ab", -- any
  "ef", -- square
  "cd", -- saw tooth
  -- "gh", -- Sine (Not enabled by default)
  "ij", -- noise
  "{|", -- triangle
  "kl", -- wave
}

local waveTypeIDs = {
  -1,
  0,
  1,
  -- 2,
  3,
  4,
  5
}

local waveToolTips =
{
  "Support for square wave form on this channel",
  "Support for saw tooth wave form on this channel",
  -- "Sine waves are not currently supported.",
  "Support for noise wave form on this channel",
  "Support for triangle wave form on this channel",
  "Support for wav sample files on this channel",
  "Support for any wave form on this channel.",
}

-- This this is an empty game, we will the following text. We combined two sets of fonts into
-- the default.font.png. Use uppercase for larger characters and lowercase for a smaller one.
local title = "EMPTY TOOL"
local messageTxt = "This is an empty tool template. Press Ctrl + 1 to open the editor or modify the files found in your workspace game folder."

local selectedLineDrawArgs = nil

local chipPicker = nil

local cancelSelectionRect = {
  x = 40,
  y = 16,
  w = 216,
  h = 152 + 8
}

local chipSpriteNames = {
  "chipgpuempty",
  "chipcartempty",
  "chipsoundempty"
}

local SaveShortcut = 5

function InvalidateData()

  -- Only everything if it needs to be
  if(invalid == true)then
    return
  end

  pixelVisionOS:ChangeTitle(toolTitle .."*", "toolbariconfile")
  -- pixelVisionOS:EnableActionButton(1, true)

  pixelVisionOS:EnableMenuItem(SaveShortcut, true)

  invalid = true

end

function ResetDataValidation()

  -- Only everything if it needs to be
  if(invalid == false)then
    return
  end

  pixelVisionOS:ChangeTitle(toolTitle, "toolbariconfile")
  invalid = false

  pixelVisionOS:EnableMenuItem(SaveShortcut, false)
  -- pixelVisionOS:EnableActionButton(1, false)
end

-- The Init() method is part of the game's lifecycle and called a game starts. We are going to
-- use this method to configure background color, ScreenBufferChip and draw a text box.
function Init()

  BackgroundColor(5)

  -- Disable the back key in this tool
  EnableBackKey(false)

  -- Create an global instance of the Pixel Vision OS
  _G["pixelVisionOS"] = PixelVisionOS:Init()

  -- -- Create an instance of the Pixel Vision OS
  -- pixelVisionOS = PixelVisionOS:Init()

  -- -- Get a reference to the Editor UI
  -- editorUI = pixelVisionOS.editorUI

  rootDirectory = ReadMetadata("directory", nil)


  if(rootDirectory == nil) then

  else

    -- Load only the game data we really need
    success = gameEditor:Load(rootDirectory, {SaveFlags.System, SaveFlags.Meta, SaveFlags.Colors})

  end

  -- If data loaded activate the tool
  if(success == true) then

    -- Get a list of all the editors
    local editorMapping = pixelVisionOS:FindEditors()

    -- Find the json editor
    textEditorPath = editorMapping["json"]

    local menuOptions =
    {
      -- About ID 1
      {name = "About", action = function() pixelVisionOS:ShowAboutModal(toolName) end, toolTip = "Learn about PV8."},
      {divider = true}, -- Reset all the values
      {name = "Edit Data", enabled = textEditorPath ~= nil, action = OnEditJSON, toolTip = "Edit the raw JSON file."}, -- Reset all the values
      {name = "Toggle Lock", enabled = true, action = OnToggleLock, toolTip = "Lock or unlock the system specs for editing."}, -- Reset all the values
      {name = "Save", key = Keys.S, action = OnSave, toolTip = "Save changes."}, -- Reset all the values
      {divider = true}, -- Reset all the values
      {name = "Quit", key = Keys.Q, action = OnQuit, toolTip = "Quit the current game."} -- Quit the current game
    }

    pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")

    local mapSize = gameEditor:TilemapSize()
    local displaySize = gameEditor:DisplaySize()

    local pathSplit = string.split(rootDirectory, "/")

    -- Update title with file path
    toolTitle = pathSplit[#pathSplit] .. "/data.json"

    sizeInputData = editorUI:CreateInputField({x = 168, y = 208, w = 24}, gameEditor:GameMaxSize(), "CPG", "number")
    sizeInputData.min = 64
    sizeInputData.max = 960
    sizeInputData.onAction = function(value)
      gameEditor:GameMaxSize(tonumber(value))

      ChangeChipGraphic(2, "chipcustomcart")
      InvalidateData()
    end

    table.insert(inputFields, sizeInputData)

    spritePagesInputData = editorUI:CreateInputField({x = runnerName == DrawVersion and 88 or 128, y = 208, w = 8}, tostring(gameEditor:SpritePages()), "Total number of sprite pages. Each page has 256 sprites.", "number")
    spritePagesInputData.min = 1
    spritePagesInputData.max = 8
    spritePagesInputData.onAction = function(value)
      gameEditor:SpritePages(tonumber(value))

      ChangeChipGraphic(2, "chipcustomcart" )
      InvalidateData()
    end

    table.insert(inputFields, spritePagesInputData)


    cpsInputData = editorUI:CreateInputField({x = runnerName == DrawVersion and 152 or 224, y = 208, w = 16}, tostring(gameEditor:ColorsPerSprite()), "Number of colors per sprite. Minimum is 2 and max is 16.", "number")
    cpsInputData.min = 2
    -- TODO This should be tied to how many colors there are?
    cpsInputData.max = 16
    cpsInputData.onAction = OnChange
    cpsInputData.onAction = function(value)
      gameEditor:ColorsPerSprite(tonumber(value))

      ChangeChipGraphic(1, "chipcustomgpu" )
      InvalidateData()
    end

    table.insert(inputFields, cpsInputData)


    mapWidthInputData = editorUI:CreateInputField({x = 32, y = 208, w = 24}, tostring(mapSize.x), "Number of columns of tiles in the map.", "number")
    mapWidthInputData.min = math.ceil(displaySize.x / 8)
    mapWidthInputData.max = 256
    mapWidthInputData.onAction = function(value)

      value = tonumber(value)

      -- Make sure value is divisible by 32
      value = math.ceil(value / 4) * 4

      local size = gameEditor:TilemapSize()

      gameEditor:TilemapSize(value, size.y)

      ChangeChipGraphic(2, "chipcustomcart" )

      InvalidateData()

    end

    table.insert(inputFields, mapWidthInputData)


    mapHeightInputData = editorUI:CreateInputField({x = 72, y = 208, w = 24}, tostring(mapSize.y), "Number of rows of tiles in the map.", "number")
    mapHeightInputData.min = math.ceil(displaySize.y / 8)
    mapHeightInputData.max = 256
    mapHeightInputData.onAction = function(value)

      value = tonumber(value)

      local size = gameEditor:TilemapSize()

      gameEditor:TilemapSize(size.x, value)

      ChangeChipGraphic(2, "chipcustomcart" )

      InvalidateData()

    end

    table.insert(inputFields, mapHeightInputData)


    displayWidthInputData = editorUI:CreateInputField({x = 16, y = 208, w = 24}, tostring(displaySize.x), "The width in pixels of the display.", "number")
    displayWidthInputData.min = 64
    displayWidthInputData.max = 512
    displayWidthInputData.onAction = function(value)

      -- Convert value to number
      value = tonumber(value)

      -- Get the current size
      local size = gameEditor:DisplaySize()

      -- Update the display size with the new width and previous height
      gameEditor:DisplaySize(value, tonumber(displayHeightInputData.text))

      -- Make sure the map
      mapWidthInputData.min = math.ceil(value / 8)
      editorUI:ChangeInputField(mapWidthInputData, mapWidthInputData.text)


      ChangeChipGraphic(1, "chipcustomgpu" )

      InvalidateData()

    end

    table.insert(inputFields, displayWidthInputData)


    displayHeightInputData = editorUI:CreateInputField({x = 56, y = 208, w = 24}, tostring(displaySize.y), "The height in pixel of the display.", "number")
    displayHeightInputData.min = 64
    displayHeightInputData.max = 480
    displayHeightInputData.onAction = function(value)

      -- Convert value to number
      value = tonumber(value)

      -- Get the current size
      local size = gameEditor:DisplaySize()

      -- Update the display size with the new width and previous height
      gameEditor:DisplaySize(tonumber(displayWidthInputData.text), value)

      -- Make sure the map
      mapHeightInputData.min = math.ceil(value / 8)

      editorUI:ChangeInputField(mapHeightInputData, mapHeightInputData.text)

      ChangeChipGraphic(1, "chipcustomgpu" )

      InvalidateData()

    end

    table.insert(inputFields, displayHeightInputData)


    --local overscanSize = gameEditor:OverscanBorder()
    --
    --overscanRightInputData = editorUI:CreateInputField({x = 96, y = 208, w = 8}, tostring(overscanSize.x), "Crops a column (8 px) from the right of the display.", "number")
    --overscanRightInputData.min = 0
    --overscanRightInputData.max = 4
    --overscanRightInputData.onAction = function(value)
    --
    --  local size = gameEditor:OverscanBorder()
    --
    --  gameEditor:OverscanBorder(tonumber(value), tonumber(overscanBottomInputData.text))
    --
    --  ChangeChipGraphic(1, "chipcustomgpu" )
    --
    --  InvalidateData()
    --
    --end
    --
    --table.insert(inputFields, overscanRightInputData)
    --
    --
    --overscanBottomInputData = editorUI:CreateInputField({x = 120, y = 208, w = 8}, tostring(overscanSize.y), "Crops a row (8 px) from the bottom of the display.", "number")
    --overscanBottomInputData.min = 0
    --overscanBottomInputData.max = 4
    --overscanBottomInputData.onAction = function(value)
    --
    --  local size = gameEditor:OverscanBorder()
    --
    --  gameEditor:OverscanBorder(tonumber(overscanRightInputData.text), tonumber(value))
    --
    --  ChangeChipGraphic(1, "chipcustomgpu" )
    --
    --  InvalidateData()
    --
    --end
    --
    --table.insert(inputFields, overscanBottomInputData)


    drawsInputData = editorUI:CreateInputField({x = 144, y = 208, w = 24}, tostring(gameEditor:MaxSpriteCount()), "Caps the total spites on the screen. Zero removes the limit.", "number")
    drawsInputData.min = 0
    drawsInputData.max = 512
    drawsInputData.onAction = function(value)
      gameEditor:MaxSpriteCount(tonumber(value))
      ChangeChipGraphic(1, "chipcustomgpu" )
      InvalidateData()
    end

    table.insert(inputFields, drawsInputData)



    soundTotalInputData = editorUI:CreateInputField({x = 16, y = 208, w = 16}, tostring(gameEditor:TotalSounds()), "Total number of sounds.", "number")
    soundTotalInputData.min = 8
    soundTotalInputData.max = 32
    soundTotalInputData.onAction = function(value)
      gameEditor:TotalSounds(tonumber(value))

      ChangeChipGraphic(3, "chipcustomsound" )
      InvalidateData()
    end

    table.insert(inputFields, soundTotalInputData)


    channelTotalInputData = editorUI:CreateInputField({x = 48, y = 208, w = 8}, tostring(gameEditor:TotalChannels()), "Total number of channels available to play sounds.", "number")
    channelTotalInputData.min = 1
    channelTotalInputData.max = 5
    channelTotalInputData.onAction = function(value)

      value = tonumber(value)

      gameEditor:TotalChannels(value)

      ChangeChipGraphic(3, "chipcustomsound" )

      -- Need to make sure that we don't have more tracks than channels
      channelIDInputData.inputField.max = value - 1
      editorUI:ChangeNumberStepperValue(channelIDInputData, channelIDInputData.inputField.text)
      InvalidateData()
    end

    table.insert(inputFields, channelTotalInputData)


    loopTotalInputData = editorUI:CreateInputField({x = 216, y = 208, w = 16}, tostring(gameEditor:TotalLoops()), "Total number of song patterns.", "number")
    loopTotalInputData.min = 8
    loopTotalInputData.max = 32
    loopTotalInputData.onAction = function(value)
      gameEditor:TotalLoops(tonumber(value))
      ChangeChipGraphic(3, "chipcustomsound" )
      InvalidateData()
    end

    table.insert(inputFields, loopTotalInputData)

    songTotalInputData = editorUI:CreateInputField({x = 184, y = 208, w = 16}, tostring(gameEditor:TotalSongs()), "Total number of songs.", "number")
    songTotalInputData.min = 8
    songTotalInputData.max = 32
    songTotalInputData.onAction = function(value)
      gameEditor:TotalSongs(tonumber(value))
      ChangeChipGraphic(3, "chipcustomsound" )
      InvalidateData()
    end

    table.insert(inputFields, songTotalInputData)


    channelIDInputData = editorUI:CreateNumberStepper({x = 72, y = 200}, 8, 0, 0, gameEditor:TotalChannels() - 1, "top", "Select a channel to preview its wave type.")
    channelIDInputData.onInputAction = function(value)

      UpdateWaveStepper()

    end

    waveStepper = editorUI:CreateStringStepper({x = 120, y = 200}, 16, "aa", validWaves, "top", "Change the length of the pattern.")

    waveStepper.inputField.editable = false
    waveStepper.inputField.forceCase = "upper"
    waveStepper.inputField.allowEmptyString = true

    waveStepper.onInputAction = UpdateWaveType

    saveSlotsInputData = editorUI:CreateInputField({x = 216, y = 208, w = 8}, tostring(gameEditor:GameSaveSlots()), "Enter the total save slots the disk can have.", "number")
    saveSlotsInputData.min = 2
    saveSlotsInputData.max = 8
    saveSlotsInputData.onAction = function(value)
      gameEditor:GameSaveSlots(tonumber(value))
      ChangeChipGraphic(2, "chipcustomcart" )
      InvalidateData()
    end

    table.insert(inputFields, saveSlotsInputData)


    -- totalColorsInputData = editorUI:CreateInputField({x = runnerName == DrawVersion and 112 or 184, y = 208, w = 24}, tostring(gameEditor:MaximumColors()), "How many colors the chip can support.", "number")
    -- totalColorsInputData.min = 2
    -- totalColorsInputData.max = 256
    -- totalColorsInputData.onAction = function(value)
    --   gameEditor:MaximumColors(tonumber(value))
    --   ChangeChipGraphic(1, "chipcustomgpu" )
    --   InvalidateData()
    -- end

    -- table.insert(inputFields, totalColorsInputData)


    -- Create a toggle group for the pagination buttons
    chipEditorGroup = editorUI:CreateToggleGroup()
    chipEditorGroup.onAction = OnSelectChip


    -- Find the chips

    local gpuChipName = gameEditor:ReadMetadata("gpuChip", "Custom")
    local cartChipName = gameEditor:ReadMetadata("cartChip", "Custom")
    local soundChipName = gameEditor:ReadMetadata("soundChip", "Custom")

    -- print("Names", gpuChipName, cartChipName, soundChipName)

    local spriteData = "chipcustomgpuup"

    for i = 1, #gpuChips do

      if(gpuChips[i].name == gpuChipName) then
        spriteData = gpuChips[i].spriteName .. "up"
      end

    end


    -- Create chip up sprite data
    _G["chipgpuemptyup"] = _G[spriteData]
    _G["chipgpuemptydown"] = _G[spriteData]

    spriteData = "chipcustomcartup"

    for i = 1, #cartChips do

      if(cartChips[i].name == cartChipName) then
        spriteData = cartChips[i].spriteName .. "up"
      end

    end

    _G["chipcartemptyup"] = _G[spriteData]
    _G["chipcartemptydown"] = _G[spriteData]

    spriteData = "chipcustomsoundup"

    for i = 1, #soundChips do

      if(soundChips[i].name == soundChipName) then
        spriteData = soundChips[i].spriteName .. "up"
      end

    end

    _G["chipsoundemptyup"] = _G[spriteData]
    _G["chipsoundemptydown"] = _G[spriteData]

    if(runnerName ~= TuneVersion) then
      gpuButton = editorUI:ToggleGroupButton(chipEditorGroup, {x = 96, y = 48}, "chipgpuempty", "Click to edit the GPU chip.")
      gpuButton.hitRect = {
        x = 101,
        y = 49,
        w = 60,
        h = 60
      }
    end

    if(runnerName ~= DrawVersion and runnerName ~= TuneVersion) then
      cartButton = editorUI:ToggleGroupButton(chipEditorGroup, {x = 64, y = 40}, "chipcartempty", "Click to edit the cart chip.")
    end

    if(runnerName ~= DrawVersion) then
      soundButton = editorUI:ToggleGroupButton(chipEditorGroup, {x = 96, y = 120}, "chipsoundempty", "Click to edit the cart chip.")
    end


    chipPicker = editorUI:CreateChipPicker({x = 0, y = 24 - 8, w = 48, h = 152 + 8}, 32, 32)


    -- Set up line sprites
    selectedLineDrawArgs = {nil, 0, 0, 0, false, false, DrawMode.Sprite}



    -- TODO look into selecting the correct chip based on what mode PV8 is in
    OnSelectChip(0)

    ResetDataValidation()

    specsLocked = gameEditor:GameSpecsLocked()

    showWarning = specsLocked

    UpdateFieldLock()



  else

    pixelVisionOS:ChangeTitle(toolName, "toolbaricontool")

    pixelVisionOS:ShowMessageModal(toolName .. " Error", "The tool could not load without a reference to a file to edit.", 160, false,
      function()
        QuitCurrentTool()
      end
    )

  end

end



function UpdateWaveType(value)

  value = table.indexOf(validWaves, value)

  gameEditor:ChannelType(tonumber(channelIDInputData.inputField.text), waveTypeIDs[value])

  -- print("UpdateWaveType", value, waveTypeIDs[value])

  waveStepper.inputField.toolTip = waveToolTips[value]

  ChangeChipGraphic(3, "chipcustomsound" )

  InvalidateData()

end

function UpdateWaveStepper()
  --
  -- print(channelIDInputData.inputField.name, channelIDInputData.inputField.text, channelIDInputData.inputField.buffer[1])

  value = tonumber(channelIDInputData.inputField.buffer[1])

  local type = gameEditor:ChannelType(value)

  -- local wavID = table.indexOf(waveTypeIDs, value)
  local validWaveID = table.indexOf(waveTypeIDs, type)

  -- print("Stepper Wav Raw Value", type, validWaveID)



  editorUI:ChangeStringStepperValue(waveStepper, validWaves[validWaveID], false, true)

end

local rootPath = ReadMetadata("RootPath", "/")

function OnEditJSON()

  if(invalid == true) then

    pixelVisionOS:ShowMessageModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before you edit the raw data file?", 160, true,
      function()

        if(pixelVisionOS.messageModal.selectionValue == true) then
          -- Save changes
          OnSave()

        end

        -- Quit the tool
        EditJSON()

      end
    )

  else
    -- Quit the tool
    EditJSON()
  end

end

function EditJSON()

  local metaData = {
    directory = rootDirectory,
    file = rootDirectory .. "data.json",
  }

  LoadGame(textEditorPath, metaData)


end

local settingPanelInvalid = false

function OnSelectChip(value)

  if(editorMode ~= value) then
    settingPanelInvalid = true
  end

  editorMode = value

  if(editorMode == 0) then

    editorUI:CloseChipPicker(chipPicker)

    editorUI:ClearGroupSelections(chipEditorGroup)

    editorUI:ClearFocus()

    for i = 1, #inputFields do
      if(inputFields[i].editing) then
        editorUI:EditInputField(inputFields[i], false)
      end
    end

  else

    -- Force the chip to go into the correct mode based on the version of the runner
    if(runnerName == DrawVersion) then
      editorMode = 1
    elseif(runnerName == TuneVersion) then
      editorMode = 3
    end

    local targetButton = nil

    if(editorMode == 1) then

      editorUI:TextEditorInvalidateBuffer(displayWidthInputData)
      editorUI:TextEditorInvalidateBuffer(displayHeightInputData)
      --editorUI:TextEditorInvalidateBuffer(overscanRightInputData)
      --editorUI:TextEditorInvalidateBuffer(overscanBottomInputData)
      editorUI:TextEditorInvalidateBuffer(drawsInputData)

      editorUI:TextEditorInvalidateBuffer(cpsInputData)
      -- editorUI:TextEditorInvalidateBuffer(totalColorsInputData)

      if(runnerName == DrawVersion) then
        editorUI:TextEditorInvalidateBuffer(spritePagesInputData)
      end

      selectedLineDrawArgs[1] = chipgpuselectedline.spriteIDs
      selectedLineDrawArgs[2] = 128
      selectedLineDrawArgs[3] = 112
      selectedLineDrawArgs[4] = chipgpuselectedline.width

      editorUI:ConfigureChipPicker(chipPicker, "gpuChips")

      targetButton = gpuButton

    elseif(editorMode == 2) then

      editorUI:TextEditorInvalidateBuffer(sizeInputData)
      editorUI:TextEditorInvalidateBuffer(spritePagesInputData)
      editorUI:TextEditorInvalidateBuffer(saveSlotsInputData)
      editorUI:TextEditorInvalidateBuffer(mapWidthInputData)
      editorUI:TextEditorInvalidateBuffer(mapHeightInputData)

      selectedLineDrawArgs[1] = chipcartselectedline.spriteIDs
      selectedLineDrawArgs[2] = 64
      selectedLineDrawArgs[3] = 144
      selectedLineDrawArgs[4] = chipcartselectedline.width

      editorUI:ConfigureChipPicker(chipPicker, "cartChips")

      targetButton = cartButton

    elseif(editorMode == 3) then

      editorUI:TextEditorInvalidateBuffer(soundTotalInputData)
      editorUI:TextEditorInvalidateBuffer(channelTotalInputData)
      editorUI:TextEditorInvalidateBuffer(songTotalInputData)
      editorUI:TextEditorInvalidateBuffer(loopTotalInputData)
      editorUI:TextEditorInvalidateBuffer(channelIDInputData.backButton)
      editorUI:TextEditorInvalidateBuffer(channelIDInputData.nextButton)
      editorUI:TextEditorInvalidateBuffer(waveStepper.backButton)
      editorUI:TextEditorInvalidateBuffer(waveStepper.nextButton)

      selectedLineDrawArgs[1] = chipsoundselectedline.spriteIDs
      selectedLineDrawArgs[2] = 104
      selectedLineDrawArgs[3] = 144
      selectedLineDrawArgs[4] = chipsoundselectedline.width

      -- Reset this to 0 when opening up the tray
      editorUI:ChangeNumberStepperValue(channelIDInputData, 0)

      UpdateWaveStepper()

      editorUI:ConfigureChipPicker(chipPicker, "soundChips")

      targetButton = soundButton

    end

    -- TODO need to configure chip picker

    if(specsLocked == true and showWarning == true) then

      -- Force the button to not redraw over the modal
      targetButton.invalid = false

      pixelVisionOS:ShowMessageModal("Warning", "The system specs are locked. Do you want to unlock them so you can make changes?", 160, true,
        function()

          if(pixelVisionOS.messageModal.selectionValue == true) then
            specsLocked = not specsLocked
            gameEditor:GameSpecsLocked(specsLocked)
            UpdateFieldLock()
            InvalidateData()

          end

          showWarning = false

        end

      )

    elseif(specsLocked == false) then
      editorUI:OpenChipPicker(chipPicker)
    end

  end

end


function Update(timeDelta)

  -- Convert timeDelta to a float
  timeDelta = timeDelta / 1000

  -- This needs to be the first call to make sure all of the OS and editor UI is updated first
  pixelVisionOS:Update(timeDelta)

  -- Only update the UI when the modal isn't active
  if(pixelVisionOS:IsModalActive() == false) then
    if(success == true) then

      if(editorMode > 0 and editorUI.collisionManager.mouseReleased == true and editorUI.collisionManager:MouseInRect(cancelSelectionRect)) then


        local overFlag = 0

        for i = 1, #chipEditorGroup.buttons do

          if(editorUI.collisionManager:MouseInRect(chipEditorGroup.buttons[i].hitRect)) then
            overFlag = i


            if(chipPicker.isDragging == true) then

              if(chipEditorGroup.buttons[i].selected == true) then
                -- print("Copy chip settings", overFlag)

                local data = editorUI:ChipPickerSelectedData(chipPicker)

                OnReplaceChip(data, i)

              else

                overFlag = 0

                -- TODO this is a bit of a hack to disable this message when specs are locked and you get the warning message
                if(specsLocked == false) then
                  pixelVisionOS:ShowMessageModal(
                    "Chip Error", "You can't use this chip here. Please drag the chip onto the open slot.", 160, false,
                    function()
                      editorUI:ClearChipPickerSelection(chipPicker)
                    end
                  )
                end

              end

              return

            end

          end

        end

        if(overFlag == 0) then

          if(chipPicker.isDragging == false) then
            editorUI:ClearGroupSelections(chipEditorGroup)
          end

          OnSelectChip(0)

        end

        editorUI:ClearChipPickerSelection(chipPicker)

      end

      -- TODO this should use the UI delayed draw API
      if(settingPanelInvalid == true) then
        DrawRect(1, 176, 254, 56, BackgroundColor(), DrawMode.TilemapCache)
        settingPanelInvalid = false

        if(editorMode == 0) then


          local panelSprites = FindMetaSpriteId("settingspanel")
          local maxColorPos = {x = 144, y = 184}
          local totalColorPos = {x = 144, y = 189}
          local spritePagePos = {x = 200, y = 184}
          local cpsPos = {x = 200, y = 189}
          local channelsPos = {x = 144, y = 208}
          local soundsPos = {x = 144, y = 213}
          local musicChannels = {x = 212, y = 208}
          local musicLoops = {x = 212, y = 213}


          if(runnerName == DrawVersion) then

            panelSprites = FindMetaSpriteId("settingspaneldraw")

            maxColorPos.x = maxColorPos.x - 32
            totalColorPos.x = totalColorPos.x - 32
            spritePagePos.x = spritePagePos.x - 32
            cpsPos.x = cpsPos.x - 32

          elseif(runnerName == TuneVersion) then
            panelSprites = FindMetaSpriteId("settingspaneltune")

            channelsPos.x = channelsPos.x - 32
            channelsPos.y = channelsPos.y - 24
            soundsPos.x = soundsPos.x - 32
            soundsPos.y = soundsPos.y - 24
            musicChannels.x = musicChannels.x - 32
            musicChannels.y = musicChannels.y - 24
            musicLoops.x = musicLoops.x - 32
            musicLoops.y = musicLoops.y - 24

          end

          print("Draw", panelSprites, FindMetaSpriteId("topstepperbackup"))
          DrawMetaSprite(panelSprites, 40, 166 + 8, false, false, DrawMode.TilemapCache)
          -- DrawSprites(panelSprites.spriteIDs, 40, 166 + 8, panelSprites.width, false, false, DrawMode.TilemapCache)

          if(runnerName ~= DrawVersion and runnerName ~= TuneVersion) then
            -- Display
            DrawText(displayWidthInputData.text, 64, 184, DrawMode.TilemapCache, "small", 15, - 4)
            DrawText(displayHeightInputData.text, 64, 189, DrawMode.TilemapCache, "small", 15, - 4)



            -- Tilemap
            DrawText(mapWidthInputData.text, 72, 208, DrawMode.TilemapCache, "small", 15, - 4)
            DrawText(mapHeightInputData.text, 72, 213, DrawMode.TilemapCache, "small", 15, - 4)

          end

          if(runnerName ~= TuneVersion) then

            -- Colors
            -- DrawText(gameEditor:MaximumColors(), maxColorPos.x, maxColorPos.y, DrawMode.TilemapCache, "small", 15, - 4)
            DrawText(math.ceil(gameEditor:TotalColors() / 64), totalColorPos.x, totalColorPos.y, DrawMode.TilemapCache, "small", 15, - 4)


            -- Sprites
            DrawText(gameEditor:SpritePages(), spritePagePos.x, spritePagePos.y, DrawMode.TilemapCache, "small", 15, - 4)
            DrawText(gameEditor:ColorsPerSprite(), cpsPos.x, cpsPos.y, DrawMode.TilemapCache, "small", 15, - 4)

          end

          if(runnerName ~= DrawVersion) then

            -- SFX
            DrawText(gameEditor:TotalChannels(), channelsPos.x, channelsPos.y, DrawMode.TilemapCache, "small", 15, - 4)
            DrawText(gameEditor:TotalSounds(), soundsPos.x, soundsPos.y, DrawMode.TilemapCache, "small", 15, - 4)

            -- Music
            DrawText(gameEditor:TotalChannels(), musicChannels.x, musicChannels.y, DrawMode.TilemapCache, "small", 15, - 4)
            DrawText(gameEditor:TotalLoops(), musicLoops.x, musicLoops.y, DrawMode.TilemapCache, "small", 15, - 4)
          end

        else

          DrawRect(40, 166 + 8, 176, 48, BackgroundColor(), DrawMode.TilemapCache)

          local spriteData = nil

          if(editorMode == 1) then
            spriteData = runnerName == DrawVersion and _G["settingsgpudraw"] or _G["settingsgpu"]

          elseif(editorMode == 2) then
            spriteData = _G["settingscart"]

          elseif(editorMode == 3) then
            spriteData = _G["settingssound"]

          end

          if(spriteData ~= nil) then
            DrawSprites(spriteData.spriteIDs, 8, 176, spriteData.width, false, false, DrawMode.TilemapCache)
          end
          -- end
        end


      end

      if(editorMode == 1) then

        if(runnerName ~= DrawVersion) then
          editorUI:UpdateInputField(displayWidthInputData)
          editorUI:UpdateInputField(displayHeightInputData)
          --editorUI:UpdateInputField(overscanRightInputData)
          --editorUI:UpdateInputField(overscanBottomInputData)
          editorUI:UpdateInputField(drawsInputData)
        else
          editorUI:UpdateInputField(spritePagesInputData)
        end

        editorUI:UpdateInputField(cpsInputData)
        -- editorUI:UpdateInputField(totalColorsInputData)

      elseif(editorMode == 2) then
        editorUI:UpdateInputField(sizeInputData)
        editorUI:UpdateInputField(spritePagesInputData)
        editorUI:UpdateInputField(saveSlotsInputData)

        editorUI:UpdateInputField(mapWidthInputData)
        editorUI:UpdateInputField(mapHeightInputData)

      elseif(editorMode == 3) then
        editorUI:UpdateInputField(soundTotalInputData)
        editorUI:UpdateInputField(channelTotalInputData)
        editorUI:UpdateInputField(loopTotalInputData)
        editorUI:UpdateInputField(songTotalInputData)
        editorUI:UpdateStepper(channelIDInputData)
        editorUI:UpdateStepper(waveStepper)
      end

      -- Update toggle groups
      editorUI:UpdateToggleGroup(chipEditorGroup)
      --
      editorUI:UpdateChipPicker(chipPicker)

    end
  end

end

function OnReplaceChip(chipData, chipID)

  pixelVisionOS:ShowMessageModal("Replace Chip", "Do you want to use the " .. chipData["name"] .. " chip? ".. chipData.message .. " This will affect " .. #chipData["fields"] .. " values and can not be undone.", 160, true,
    function()
      if(pixelVisionOS.messageModal.selectionValue == true) then
        -- Save changes
        ReplaceChip(chipData, chipID)

      end

    end
  )

end



function ReplaceChip(chipData, chipID)

  local fields = chipData["fields"]

  for i = 1, #fields do

    local name = fields[i]["name"]
    local value = fields[i]["value"]

    local field = _G[name]

    if(field ~= nil) then

      editorUI:ChangeInputField(field, value, true)

    end

  end

  if(chipData.colors ~= nil) then

    gameEditor:ClearColors(chipData.colors[1])

    local totalColors = #chipData.colors

    gameEditor:MaximumColors(totalColors)

    for i = 1, totalColors do
      gameEditor:Color(i - 1, chipData.colors[i])
    end

    invalidateColors = true

    local usePalettes = chipData.paletteMode

    -- Save palette mode to the info file for the color editor
    gameEditor:WriteMetadata("paletteMode", usePalettes and "true" or "false")

    if(usePalettes == true) then

      -- TODO need to add color-map

      invalidateColorMap = true

      gameEditor:ReindexSprites()

      for i = 1, #chipData.palette do
        -- Find the palette start index
        local offset = ((i - 1) * 16) + 128
        for j = 1, 10 do
          gameEditor:Color(offset + (j - 1), chipData.palette[i][j])
        end

      end
    else

      local colorMapPath = NewWorkspacePath(rootDirectory).AppendFile("color-map.png")

      if(PathExists(colorMapPath)) then
        Delete(colorMapPath)
      end

    end

    -- Update the colors based on the list of colors
    -- editorUI:ChangeInputField(totalColorsInputData, totalColors, true)

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

  ChangeChipGraphic(chipID, chipData.spriteName, chipData.name)

  InvalidateData()
  -- Need to look for special values like colors and what not

end

function ChangeChipGraphic(chipID, chipSpriteName, name)

  -- Force the chipID to always be 3 if we are only editing sounds
  if(runnerName == TuneVersion) then
    chipID = 3
  end

  name = name or "Custom"

  local button = chipEditorGroup.buttons[(runnerName == DrawVersion or runnerName == TuneVersion) and 1 or chipID]
  local spriteName = chipSpriteNames[chipID]

  _G[spriteName .. "up"] = _G[chipSpriteName .. "up"]
  _G[spriteName .. "down"] = _G[chipSpriteName .. "up"]

  editorUI:RebuildSpriteCache(button)

  -- Save the chip name to the info.json file
  if(chipID == 1) then
    gameEditor:WriteMetadata("gpuChip", name)
  elseif(chipID == 2) then
    gameEditor:WriteMetadata("cartChip", name)
  elseif(chipID == 3) then
    gameEditor:WriteMetadata("soundChip", name)
  end

end

-- The Draw() method is part of the game's life cycle. It is called after Update() and is where
-- all of our draw calls should go. We'll be using this to render sprites to the display.
function Draw()

  -- We can use the RedrawDisplay() method to clear the screen and redraw the tilemap in a
  -- single call.
  RedrawDisplay()

  -- The UI should be the last thing to draw after your own custom draw calls
  pixelVisionOS:Draw()



  if(pixelVisionOS:IsModalActive() == false) then
    if(success == true and editorMode ~= 0) then

      editorUI:NewDraw("DrawSprites", selectedLineDrawArgs)

      editorUI:DrawChipPicker(chipPicker)

    end
  end

end

function OnSave()

  local flags = {SaveFlags.System, SaveFlags.Meta}

  if(invalidateColors == true) then

    table.insert(flags, SaveFlags.Colors)
    invalidateColors = false
  end

  if(invalidateColorMap == true) then

    table.insert(flags, SaveFlags.ColorMap)
    invalidateColorMap = false
  end

  -- TODO need to save music and sounds when those are broken out
  gameEditor:Save(rootDirectory, flags)

  -- Display that the data was saved and reset invalidation
  pixelVisionOS:DisplayMessage("The game's 'data.json' file has been updated.", 5)

  ResetDataValidation()

end

function OnQuit()

  if(invalid == true) then

    pixelVisionOS:ShowMessageModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before you quit?", 160, true,
      function()
        if(pixelVisionOS.messageModal.selectionValue == true) then
          -- Save changes
          OnSave()

        end

        -- Quit the tool
        QuitCurrentTool()

      end
    )

  else
    -- Quit the tool
    QuitCurrentTool()
  end

end

function OnToggleLock()

  specsLocked = gameEditor:GameSpecsLocked()



  local title = specsLocked == true and "Unlock" or "Lock"

  pixelVisionOS:ShowMessageModal(title .. " System Specs", "Are you sure you want to ".. title .." the system specs?", 160, true,
    function()
      if(pixelVisionOS.messageModal.selectionValue == true) then

        specsLocked = not specsLocked
        gameEditor:GameSpecsLocked(specsLocked)
        UpdateFieldLock()
        InvalidateData()
      end

    end
  )

end

function UpdateFieldLock()

  showWarning = specsLocked

  for i = 1, #inputFields do
    editorUI:Enable(inputFields[i], not specsLocked)
  end

  editorUI:EnableStepper(waveStepper, not specsLocked)

end
