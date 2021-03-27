--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- Global UI used by this tool
LoadScript("pixel-vision-os-item-picker-v4")
LoadScript("pixel-vision-os-color-picker-v5")
LoadScript("pixel-vision-os-sprite-picker-v4")
LoadScript("pixel-vision-os-canvas-v4")
LoadScript("pixel-vision-os-progress-modal-v2")

-- Create table to store the workspace tool logic
DrawTool = {}
DrawTool.__index = DrawTool

-- Custom code used by this tool
LoadScript("code-fix-sprite-modal")
LoadScript("code-color-editor-modal")
LoadScript("code-drop-down-menu")
LoadScript("code-color-panel")
LoadScript("code-process-sprites")
LoadScript("code-optimize-sprites")
LoadScript("code-sprite-builder")
LoadScript("code-palette-panel")
LoadScript("code-sprite-panel")
LoadScript("code-canvas-panel")
LoadScript("code-toolbar")

-- Create some Constants for the different color modes
NoFocus, SystemColorMode, PaletteMode, SpriteMode, DrawingMode = 0, 1, 2, 3, 4

function DrawTool:Init()

    -- Create a new table for the instance with default properties
    local _drawTool = {
        toolName = "Draw Tool",
        runnerName = SystemName(),
        rootPath = ReadMetadata("RootPath", "/"),
        rootDirectory = ReadMetadata("directory", nil),
        invalid = true,
        success = false,
        canEdit = EditColorModal ~= nil,
        debugMode = true,
        colorOffset = 0,
        lastMode = nil,
        showBGColor = false,
        panelInFocus = nil,
        ignoreProcessing = false,
        toolTitle = "sprites.png" -- TODO this should just read from the file
    }

    -- Reset the undo history so it's ready for the tool
    pixelVisionOS:ResetUndoHistory(self)
  
    -- Create a global reference of the new workspace tool
    setmetatable(_drawTool, DrawTool)

    if(_drawTool.rootDirectory ~= nil) then

        -- These are the default files the tool will load
        local flags = {SaveFlags.System, SaveFlags.Meta, SaveFlags.Colors}

        -- Before we load any files, we need to check the metadata to see if we need to parse the sprites

        -- Build the path to the metadata file
        local metadataPath = NewWorkspacePath(_drawTool.rootDirectory).AppendFile("info.json")

        -- Make sure the file path exists first
        if(PathExists(metadataPath)) then

            -- Read the raw metadata json file
            local metadata = ReadJson(metadataPath)

            -- Set the ignore process flag to the metadata value or to false
            _drawTool.ignoreProcessing = metadata["ignoreProcessing"] or "False"

        end

        -- If the left or right ctrl key is down, reset the processing flag
        if(Key(Keys.LeftControl, InputState.Down) == true or Key(Keys.RightControl, InputState.Down) == true) then

            -- override the flag value by setting it to false
            _drawTool.ignoreProcessing = "False"

        end

        -- Check to see if there is a flag to ignore the sprite processing
        if(_drawTool.ignoreProcessing == "True") then

            -- Add the sprites  to the list of files to load
            table.insert(flags, SaveFlags.Sprites)
        
        end

        -- Load only the game data we really need
        _drawTool.success = gameEditor.Load(_drawTool.rootDirectory, flags)

    end

    -- _drawTool.success = false
    -- _drawTool.success = false
    -- If data load fails
    if(_drawTool.success ~= true) then
        
        -- Display the load error
        _drawTool:LoadError()
        
    else

        -- The first thing we need to do is rebuild the tool's color table to include the game's system and game colors.
        pixelVisionOS:ImportColorsFromGame()

        -- Kick off the new part of the tool's boot process
        _drawTool:LoadSuccess()

    end

    -- Return the draw tool data
    return _drawTool
  
end

function DrawTool:LoadError()

    pixelVisionOS:ChangeTitle(self.toolName, "toolbaricontool")

    pixelVisionOS:ShowMessageModal(self.toolName .. " Error", "The tool could not load without a reference to a file to edit.", 160, false,
        function()
            QuitCurrentTool()
        end
    )

end

function DrawTool:LoadSuccess()

    -- Everything loaded so finish initializing the tool
    
    

    -- Get the target file
    local targetFile = ReadMetadata("file", nil)

    local targetFilePath = NewWorkspacePath(targetFile)

    local colorMode = targetFilePath.EntityName == "colors.png"

    local pathSplit = string.split(targetFile, "/")

    self.titlePath = pathSplit[#pathSplit - 1] .. "/"

    self:CreateDropDownMenu()

    self:CreateSpritePanel()
    
    self:CreateColorPanel()

    self:CreatePalettePanel()

    self:CreateCanvas()

    self:CreateToolbar()

    if(self.debugMode == true) then
        self.colorMemoryCanvas = NewCanvas(8, TotalColors() / 8)

        local pixels = {}
        for i = 1, TotalColors() do
            local index = i - 1
            table.insert(pixels, index)
        end

        self.colorMemoryCanvas:SetPixels(pixels)

        pixelVisionOS:RegisterUI({name = "DebugPanel"}, "DrawDebugPanel", self)
    end


    local defaultToolID = 1
    local defaultMode = colorMode == true and ColorMode or SpriteMode

    self.lastSpriteID = 0
    self.lastSystemColorID = 0
    self.lastPaletteColorID = 0

    -- TODO need to make sure we are editing the same file

    -- print("Session", SessionID(), ReadSaveData("sessionID", ""), ReadMetadata("file", nil))
    if(SessionID() == ReadSaveData("sessionID", "") and self.rootDirectory == ReadSaveData("rootDirectory", "")) then

        self.spriteMode = Clamp(tonumber(ReadSaveData("lastSpriteSize", "1")) - 1, 0, #self.selectionSizes)
        self.lastSpriteID = tonumber(ReadSaveData("lastSpriteID", "0"))
        defaultToolID = tonumber(ReadSaveData("lastSelectedToolID", "1"))

        self.lastSystemColorID = tonumber(ReadSaveData("lastSystemColorID", "0"))
        self.lastPaletteColorID = tonumber(ReadSaveData("lastPaletteColorID", "0"))
        
    end

    -- -- Change the sprite mode
    self:OnNextSpriteSize()

    -- Select the start sprite
    self:ChangeSpriteID(self.lastSpriteID)

    -- print("SCALE", self.spriteMode)
    -- print("self.spriteMode", self.spriteMode)
    self:ConfigureSpritePickerSelector(self.spriteMode)

    -- -- Set default tool
    editorUI:SelectToggleButton(self.toolBtnData, defaultToolID)

    -- -- Set default mode
    self:ChangeEditMode(defaultMode)
    
    self:ResetDataValidation()

    -- -- print("Test", self.systemColorPickerData)

    self:ForcePickerFocus(self.systemColorPickerData)
    
    -- Need to read the project meta data to see if it should be called
    if(self.ignoreProcessing == "False") then

        -- Create the path to the default sprite.png file
        self.spritesPath = NewWorkspacePath(self.rootDirectory.."sprites.png")

        -- Check to see if the path exist
        if(PathExists(self.spritesPath) == true) then
             
            -- Start the process delay by setting the value to 0
            self.processSpriteDelay = 0
            
            pixelVisionOS:RegisterUI({name = "DelayProcessDialog"}, "DelayProcessDialog", self, true)

        end

    end

end

function DrawTool:DelayProcessDialog()

    -- Look to see if the timer is past the delay (this lets the background draw first)
    if(self.processSpriteDelay > 500) then
        
        -- TODO this should come from a file to help localize text later on
        local title = "Analyze Sprites"
        local message = "Analyzing the sprites will take a few moments and unlocks the ability to make changes to the color index values assigned by the parser. You can skip this process the next time you load the project if there isn't a need to make changes to the default assigned system colors. You can always re-enable this message by holding down the CTRL key while the tool loads up.\n\n#  Skip sprite analyzation in the future"
        -- Create the new modal
        local warningModal = FixSpriteModal:Init(title, message, 216, false)

        -- Open the modal
        pixelVisionOS:OpenModal(
            warningModal,

            -- Callback for the motel when it is closed
            function() 

                -- Check to see if ok was pressed on the model
                if(warningModal.selectionValue == true) then
                    
                    gameEditor:WriteMetadata("ignoreProcessing",  warningModal.optionGroupData.currentSelection == 1 and "True" or "False")
                        
                    -- Force the meta data to save 
                    gameEditor:Save(self.rootDirectory, {SaveFlags.Meta})

                    -- Kick off the process sprites logic
                    self:ProcessSprites()

                end

                -- TODO need to check that this shouldn't be shown again

            end
        )

        -- Remove the callback from the UI update loop
        pixelVisionOS:RemoveUI("DelayProcessDialog")

    else

        -- TODO need to multiply but 1000 since the UI runs on legacy milliseconds

        -- Increment the counter by the timeDelta each frame
        self.processSpriteDelay = self.processSpriteDelay + (editorUI.timeDelta * 1000)

    end

end

function DrawTool:DrawDebugPanel()
    self.colorMemoryCanvas:DrawPixels(256 - (8 * 3) - 2, 12, DrawMode.UI, 3)
end

function DrawTool:InvalidateData()

    -- Only everything if it needs to be
    if(self.invalid == true)then
        return
    end

    self.invalid = true

    self:UpdateTitle()

    pixelVisionOS:EnableMenuItem(SaveShortcut, true)

end

function DrawTool:ResetDataValidation()

    -- Only everything if it needs to be
    if(self.invalid == false)then
        return
    end

    self.invalid = false

    self:UpdateTitle()

    pixelVisionOS:EnableMenuItem(SaveShortcut, false)

end

function DrawTool:UpdateTitle()

    pixelVisionOS:ChangeTitle(self.titlePath .. self.toolTitle .. (self.invalid == true and "*" or ""), "toolbariconfile")

end

-- Changes the focus of the currently selected color picker
function DrawTool:ForcePickerFocus(src)

    -- Ignore this when the panel is already in focus
    if((self.panelInFocus ~= nil and src.name == self.panelInFocus.name) or self.ignoreFocus == true) then
        -- print("Ignore focus switch")
        return
    end

    -- TODO need to check what's in the clipboard and see if paste needs to cleared
    -- pixelVisionOS:EnableMenuItem(PasteShortcut, self.copyValue ~= nil)

    if(src.name == self.systemColorPickerData.name) then

        print("System Color picker in focus")

        -- Change the color mode to system color mode
        self.selectionMode = SystemColorMode

        -- Change sprite picker focus color
        -- TODO Need to change the selector
        -- self.spritePickerData.picker.selectedDrawArgs[1] = _G["spritepickerover"].spriteIDs

        -- Toggle menu options
        pixelVisionOS:EnableMenuItem(CopyShortcut, false)
        pixelVisionOS:EnableMenuItem(PasteShortcut, false)
        pixelVisionOS:EnableMenuItem(ClearShortcut, false)
        pixelVisionOS:EnableMenuItem(AddShortcut, true)
        pixelVisionOS:EnableMenuItem(EditShortcut, true)
        pixelVisionOS:EnableMenuItem(DeleteShortcut, true)
        pixelVisionOS:EnableMenuItem(SetBGShortcut, true)
        pixelVisionOS:EnableMenuItem(SetMaskColor, true)
        
        -- Restore last system color
        if(self.lastSystemColorID ~= nil) then
            print("Reset Pal")
            pixelVisionOS:SelectColorPickerIndex(self.systemColorPickerData, self.lastSystemColorID)
        end

        -- Clear the palette picker selection in one one color can be selected at a time
        pixelVisionOS:ClearItemPickerSelection(self.paletteColorPickerData)

    elseif(src.name == self.paletteColorPickerData.name) then

        print("Palette Color Picker in focus")

    --     -- Change the selection mode to palette mode
        self.selectionMode = PaletteMode

        pixelVisionOS:ClearItemPickerSelection(self.systemColorPickerData)

        -- Change selection focus colors
        -- self.spritePickerData.picker.selectedDrawArgs[1] = _G["spritepickerover"].spriteIDs
        -- self.paletteColorPickerData.picker.selectedDrawArgs[1] = _G["itempickerselectedup"].spriteIDs

        -- Toggle menu options
        pixelVisionOS:EnableMenuItem(CopyShortcut, true)
        pixelVisionOS:EnableMenuItem(PasteShortcut, false)
        pixelVisionOS:EnableMenuItem(ClearShortcut, true)
        pixelVisionOS:EnableMenuItem(AddShortcut, true)
        pixelVisionOS:EnableMenuItem(EditShortcut, false)
        pixelVisionOS:EnableMenuItem(DeleteShortcut, true)
        pixelVisionOS:EnableMenuItem(SetBGShortcut, false)
        pixelVisionOS:EnableMenuItem(SetMaskColor, false)
        -- print("paletteColorPickerData sel", self.paletteColorPickerData.currentSelection)

    elseif(src.name == self.spritePickerData.name) then
        self.selectionMode = SpriteMode

        print("Sprite Picker in focus")
    --     -- Clear the system color picker selection
        pixelVisionOS:ClearItemPickerSelection(self.systemColorPickerData)

        -- self.spritePickerData.picker.selectedDrawArgs[1] = _G["spritepickerselectedup"].spriteIDs
        -- self.paletteColorPickerData.picker.selectedDrawArgs[1] = _G["itempickerover"].spriteIDs

        pixelVisionOS:EnableMenuItem(CopyShortcut, true)
        pixelVisionOS:EnableMenuItem(PasteShortcut, false)
        pixelVisionOS:EnableMenuItem(ClearShortcut, true)
        pixelVisionOS:EnableMenuItem(AddShortcut, false)
        pixelVisionOS:EnableMenuItem(EditShortcut, false)
        pixelVisionOS:EnableMenuItem(DeleteShortcut, false)
        pixelVisionOS:EnableMenuItem(SetBGShortcut, false)
        pixelVisionOS:EnableMenuItem(SetMaskColor, false)

        print("Restore palette", self.lastPaletteColorID)

        if(self.lastSystemColorID ~= nil) then
            pixelVisionOS:SelectColorPickerIndex(self.paletteColorPickerData, self.lastPaletteColorID)
        end

    elseif(src.name == self.canvasData.name) then

        self.selectionMode = DrawingMode
        
        print("Canvas in focus now")

        self.spritePickerData.picker.selectedDrawArgs[1] = _G["spritepickerover"].spriteIDs
        self.paletteColorPickerData.picker.selectedDrawArgs[1] = _G["itempickerover"].spriteIDs

 
        pixelVisionOS:EnableMenuItem(CopyShortcut, false)
        pixelVisionOS:EnableMenuItem(PasteShortcut, false)
        pixelVisionOS:EnableMenuItem(ClearShortcut, true)
        pixelVisionOS:EnableMenuItem(AddShortcut, false)
        pixelVisionOS:EnableMenuItem(EditShortcut, false)
        pixelVisionOS:EnableMenuItem(DeleteShortcut, false)
        pixelVisionOS:EnableMenuItem(SetBGShortcut, false)
        pixelVisionOS:EnableMenuItem(SetMaskColor, false)

    end

    -- TODO need to check if we should clear the copy command
    self.panelInFocus = src

    -- Save the last mode
    self.lastMode = self.selectionMode
    
end


function DrawTool:OnAddDroppedColor(id, dest, color)

    local index = pixelVisionOS.colorOffset + pixelVisionOS.totalPaletteColors + (id)
    
    self:BeginUndo()
    pixelVisionOS:ColorPickerChangeColor(dest, index, color)
    self:EndUndo()

    self:InvalidateData()

end

function DrawTool:OnEditColor()

    local colorID = self.systemColorPickerData.currentSelection + self.systemColorPickerData.altColorOffset

    if(self.editColorModal == nil) then
        self.editColorModal = EditColorModal:Init(editorUI, pixelVisionOS.maskColor)
    end

    -- TODO need to get the currently selected color
    self.editColorModal:SetColor(colorID)

    local currentColor = Color(colorID)

    pixelVisionOS:OpenModal(self.editColorModal,
        function()

            if(self.editColorModal.selectionValue == true and currentColor ~= "#" .. self.editColorModal.colorHexInputData.text) then

                self:UpdateHexColor(self.editColorModal.colorHexInputData.text)
                
                return
            end

        end
    )

end

function DrawTool:Shutdown()

    -- Shutdown all the editor update and draw calls
    editorUI:Shutdown()

    -- Kill the canvas
    if(self.canvasData ~= nil) then
        self.canvasData.onAction = nil
    end

    -- Save the state of the tool if it was correctly loaded
    if(self.success == true) then
        -- Save the current session ID
        WriteSaveData("sessionID", SessionID())
        WriteSaveData("rootDirectory", self.rootDirectory)

        WriteSaveData("lastSpriteSize", self.spriteMode)
        WriteSaveData("lastSpriteID", self.spritePickerData.currentSelection)
        WriteSaveData("lastSelectedToolID", self.lastSelectedToolID)

        WriteSaveData("lastSystemColorID", self.lastSystemColorID)
        WriteSaveData("lastPaletteColorID", self.lastPaletteColorID)
    end

end
