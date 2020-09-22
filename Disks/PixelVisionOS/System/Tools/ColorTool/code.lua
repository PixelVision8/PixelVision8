--[[
	Pixel Vision 8 - Display Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- API Bridge
LoadScript("sb-sprites")
LoadScript("pixel-vision-os-v2")
LoadScript("pixel-vision-os-color-picker-v3")
LoadScript("pixel-vision-os-item-picker-v1")
LoadScript("pixel-vision-os-sprite-picker-v3")
LoadScript("pixel-vision-os-color-editor-modal-v1")

local toolName = "Color Tool"

local colorOffset = 0
-- local systemColorsPerPage = 64
local success = false
local emptyColorID = -1
local dragTime = 0
local dragDelay = .5

local canEdit = EditColorModal ~= nil
local maxPalettePages = 8
local paletteOffset = 0
local paletteColorPages = 0
local spriteEditorPath = ""
spritesInvalid = false
local totalPalettePages = 0
local debugMode = false
local showBGIcon = false
local BGIconX = 0
local BGIconY = 0

local SaveShortcut, AddShortcut, EditShortcut, ClearShortcut, DeleteShortcut, BGShortcut, UndoShortcut, RedoShortcut, CopyShortcut, PasteShortcut = 5, 7, 8, 9, 10, 11, 13, 14, 15, 16

-- Create some Constants for the different color modes
local NoColorMode, SystemColorMode, PaletteMode = 0, 1, 2

-- The default selected mode is NoColorMode
local selectionMode = NoColorMode

function InvalidateData()

    -- Only everything if it needs to be
    if(invalid == true)then
        return
    end

    pixelVisionOS:ChangeTitle(toolTitle .."*", "toolbariconfile")

    invalid = true

    pixelVisionOS:EnableMenuItem(SaveShortcut, true)

end

function ResetDataValidation()

    -- Only everything if it needs to be
    if(invalid == false)then
        return
    end

    pixelVisionOS:ChangeTitle(toolTitle, "toolbariconfile")
    invalid = false

    pixelVisionOS:EnableMenuItem(SaveShortcut, false)

end

function Init()

    -- Disable the back key in this tool
    EnableBackKey(false)

    BackgroundColor(5)

    -- Create an instance of the Pixel Vision OS
    pixelVisionOS = PixelVisionOS:Init()

    -- Get a reference to the Editor UI
    editorUI = pixelVisionOS.editorUI

    -- Reset the undo history so it's ready for the tool
    pixelVisionOS:ResetUndoHistory()

    rootDirectory = ReadMetadata("directory", nil)

    if(rootDirectory ~= nil) then

        -- Load only the game data we really need
        success = gameEditor.Load(rootDirectory, {SaveFlags.System, SaveFlags.Meta, SaveFlags.Colors, SaveFlags.ColorMap, SaveFlags.Sprites})

    end

    -- If data loaded activate the tool
    if(success == true) then

        -- Get a list of all the editors
        local editorMapping = pixelVisionOS:FindEditors()

        -- Find the json editor
        spriteEditorPath = editorMapping["sprites"]

        -- The first thing we need to do is rebuild the tool's color table to include the game's system and game colors.
        pixelVisionOS:ImportColorsFromGame()

        local menuOptions = 
        {
            -- About ID 1
            {name = "About", action = function() pixelVisionOS:ShowAboutModal(toolName) end, toolTip = "Learn about PV8."},
            {divider = true},
            --
            -- {name = "Toggle Mask", action = OnToggleMask, enabled = false, toolTip = "Toggle between the background and mask color."}, -- Reset all the values
            {name = "Toggle Mode", action = function() TogglePaletteMode(not usePalettes) end, enabled = canEdit, toolTip = "Toggle between palette and direct color mode."}, -- Reset all the values
            {name = "Edit Sprites", enabled = spriteEditorPath ~= nil, action = OnEditSprites, toolTip = "Open the sprite editor."},
            -- Reset all the values
            {name = "Save", action = OnSave, enabled = false, key = Keys.S, toolTip = "Save changes made to the colors.png file."}, -- Reset all the values
            {divider = true},
            {name = "Add", action = OnAdd, enabled = false, key = Keys.A, toolTip = "Add a new color to the currently selected picker."},
            {name = "Edit", action = OnConfig, enabled = false, key = Keys.E, toolTip = "Edit the currently selected color."},
            {name = "Clear", action = OnClear, enabled = false, key = Keys.B, toolTip = "Clear the currently selected color."},
            {name = "Delete", action = OnDelete, enabled = false, key = Keys.D, toolTip = "Remove the currently selected color."},
            {name = "Set BG Color", action = OnSetBGColor, enabled = false, toolTip = "Set the current color as the background."}, -- Reset all the values
            {divider = true},
            {name = "Undo", action = OnRevert, enabled = false, key = Keys.Z, toolTip = "Undo the last action."}, -- Reset all the values
            {name = "Redo", action = OnRevert, enabled = false, key = Keys.Y, toolTip = "Redo the last undo."}, -- Reset all the values
            {name = "Copy", action = OnCopy, enabled = false, key = Keys.C, toolTip = "Copy the currently selected sound."}, -- Reset all the values
            {name = "Paste", action = OnPaste, enabled = false, key = Keys.V, toolTip = "Paste the last copied sound."}, -- Reset all the values
            {divider = true},
            {name = "Quit", key = Keys.Q, action = OnQuit, toolTip = "Quit the current game."}, -- Quit the current game
        }

        if(PathExists(NewWorkspacePath(rootDirectory).AppendFile("code.lua"))) then
            table.insert(menuOptions, #menuOptions, {name = "Run Game", action = OnRunGame, key = Keys.R, toolTip = "Run the code for this game."})
        end

        pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")

        -- Split the root directory path
        local pathSplit = string.split(rootDirectory, "/")

        -- save the title with file path
        toolTitle = pathSplit[#pathSplit] .. "/colors.png"

        -- TODO this is debug code and can be removed when things are working

        if(debugMode == true) then
            colorMemoryCanvas = NewCanvas(8, TotalColors() / 8)

            local pixels = {}
            for i = 1, TotalColors() do
                local index = i - 1
                table.insert(pixels, index)
            end

            colorMemoryCanvas:SetPixels(pixels)
        end


        -- We need to modify the color selection sprite so we start with a reference to it
        local selectionPixelData = colorselection

        -- Now we need to create the item picker over sprite by using the color selection spriteIDs and changing the color offset
        _G["itempickerover"] = {spriteIDs = colorselection.spriteIDs, width = colorselection.width, colorOffset = 28}

        -- Next we need to create the item picker selected up sprite by using the color selection spriteIDs and changing the color offset
        _G["itempickerselectedup"] = {spriteIDs = colorselection.spriteIDs, width = colorselection.width, colorOffset = (_G["itempickerover"].colorOffset + 2)}


        -- Create an input field for the currently selected color ID
        colorIDInputData = editorUI:CreateInputField({x = 152, y = 208, w = 24}, "0", "The ID of the currently selected color.", "number", "input", 180)

        -- The minimum value is always 0 and we'll set the maximum value based on which color picker is currently selected
        colorIDInputData.min = 0

        -- Map the on action to the ChangeColorID method
        colorIDInputData.onAction = ChangeColorID

        -- Create a hex color input field
        colorHexInputData = editorUI:CreateInputField({x = 200, y = 208, w = 48}, "FF00FF", "Hex value of the selected color.", "hex", "input", 180)

        colorHexInputData.forceCase = "upper"
        -- Call the UpdateHexColor function when a change is made
        colorHexInputData.onAction = UpdateHexColor

        -- Get the palette mode
        usePalettes = pixelVisionOS.paletteMode

        -- Create the system color picker
        systemColorPickerData = pixelVisionOS:CreateColorPicker(
            {x = 8, y = 32, w = 128, h = 128}, -- Rect
            {x = 16, y = 16}, -- Tile size
            pixelVisionOS.totalSystemColors, -- Total colors, plus 1 for empty transparancy color
            pixelVisionOS.systemColorsPerPage, -- total per page
            4, -- max pages
            pixelVisionOS.colorOffset, -- Color offset to start reading from
            "itempicker", -- Selection sprite name
            "System Color", -- Tool tip
            false, -- Modify pages
            true, -- Enable dragging,
            true -- drag between pages
        )

        systemColorPickerData.onPageAction = function(value)

            local bgColor = gameEditor:BackgroundColor()
            local bgPageID = math.ceil((bgColor + 1) / 64)

            showBGIcon = bgPageID == value

            -- TODO need to make sure we only show this when it's on the correct page
            UpdateBGIconPosition(bgColor)

        end



        -- Force the BG color to draw for the first time
        systemColorPickerData.onPageAction(1)

        -- Create a function to handle what happens when a color is dropped onto the system color picker
        systemColorPickerData.onDropTarget = OnSystemColorDropTarget

        -- Manage what happens when a color is selected
        systemColorPickerData.onPress = function(value)

            -- Call the OnSelectSystemColor method to update the fields
            OnSelectSystemColor(value)

            -- Change the focus of the current color picker
            ForcePickerFocus(systemColorPickerData)
        end

        systemColorPickerData.onAction = function(value, doubleClick)

            if(doubleClick == true and canEdit == true) then

                OnConfig()

            end

        end

        -- Create the palette color picker
        paletteColorPickerData = pixelVisionOS:CreateColorPicker(
            {x = 8, y = 184, w = 128, h = 32},
            {x = 16, y = 16},
            pixelVisionOS.totalPaletteColors,
            pixelVisionOS.paletteColorsPerPage,
            8,
            pixelVisionOS.colorOffset + pixelVisionOS.totalPaletteColors,
            "itempicker",
            "palette color",
            false,
            true,
            true
        )

        -- TODO this shouldn't have to be called?
        pixelVisionOS:RebuildColorPickerCache(paletteColorPickerData)

        paletteColorPickerData.onAction = function(value)
            ForcePickerFocus(paletteColorPickerData)

            OnSelectPaletteColor(value)

        end

        paletteColorPickerData.onDropTarget = OnPalettePickerDrop

        -- Get sprite texture dimensions
        local totalSprites = gameEditor:TotalSprites()

        -- This is fixed size at 16 cols (128 pixels wide)
        local spriteColumns = 16
        local spriteRows = math.ceil(totalSprites / 16)

        spritePickerData = pixelVisionOS:CreateSpritePicker(
            {x = 152, y = 32, w = 96, h = 128 },
            {x = 8, y = 8},
            spriteColumns,
            spriteRows,
            pixelVisionOS.colorOffset,
            "spritepicker",
            "Pick a sprite",
            true,
            "SpritePicker"
        )

        -- The sprite picker shouldn't be selectable on this screen but you can still change pages
        pixelVisionOS:EnableItemPicker(spritePickerData, false, true)

        --
        -- Wire up the picker to change the color offset of the sprite picker
        paletteColorPickerData.onPageAction = function(value)

            pixelVisionOS:ChangeItemPickerColorOffset(spritePickerData, pixelVisionOS.colorOffset + pixelVisionOS.totalPaletteColors + ((value - 1) * 16))

        end

        if(usePalettes == true) then
            pixelVisionOS:OnColorPickerPage(paletteColorPickerData, 1)
        end

        RefreshBGColorIcon()

        local selectedSpritePage = 0
        local paletteColorPicker = 0

        if(SessionID() == ReadSaveData("sessionID", "") and rootDirectory == ReadSaveData("rootDirectory", "")) then

            selectedSpritePage = tonumber(ReadSaveData("selectedSpritePage", "1"))
            paletteColorPicker = tonumber(ReadSaveData("selectedPalettePage", "1"))

        end

        pixelVisionOS:OnSpritePickerPage(spritePickerData, selectedSpritePage)

        pixelVisionOS:OnColorPickerPage(paletteColorPickerData, paletteColorPicker)

        -- Reset the validation to update the title and set the validation flag correctly for any changes
        ResetDataValidation()

        -- Set the focus mode to none
        ForcePickerFocus()

    else

        -- Patch background when loading fails

        -- Left panel
        DrawRect(8, 32, 128, 128, 0, DrawMode.TilemapCache)
        DrawRect(152, 32, 96, 128, 0, DrawMode.TilemapCache)
        DrawRect(152, 208, 24, 8, 0, DrawMode.TilemapCache)
        DrawRect(200, 208, 48, 8, 0, DrawMode.TilemapCache)
        DrawRect(136, 164, 3, 9, BackgroundColor(), DrawMode.TilemapCache)
        DrawRect(248, 180, 3, 9, BackgroundColor(), DrawMode.TilemapCache)
        DrawRect(136, 220, 3, 9, BackgroundColor(), DrawMode.TilemapCache)

        pixelVisionOS:ChangeTitle(toolName, "toolbaricontool")

        pixelVisionOS:ShowMessageModal(toolName .. " Error", "The tool could not load without a reference to a file to edit.", 160, false,
            function()
                QuitCurrentTool()
            end
        )

    end



end



function OnPalettePickerDrop(src, dest)
    -- print("Palette Picker On Drop", src.name, dest.name)

    -- Two modes, accept colors from the system color picker or swap colors in the palette

    if(src.name == systemColorPickerData.name) then

        -- Get the index and add 1 to offset it correctly
        local id = pixelVisionOS:CalculateItemPickerPosition(dest).index

        -- Get the correct hex value
        local srcHex = Color(src.pressSelection.index + src.altColorOffset)

        -- print("srcHex", srcHex, "id", id, "ColorID", (src.pressSelection.index + src.altColorOffset))

        if(usePalettes == false) then

            if(canEdit == true) then
                -- We want to manually toggle the palettes before hand so we can add the first color before calling the AddPalettePage()
                TogglePaletteMode(true, function() OnAddDroppedColor(id, dest, srcHex) end)
            end

        else

            OnAddDroppedColor(id, dest, srcHex)
        end
    else
        -- print("Swap colors")

        OnSystemColorDropTarget(src, dest)

    end


end

function OnAddDroppedColor(id, dest, color)

    local index = pixelVisionOS.colorOffset + pixelVisionOS.totalPaletteColors + (id)

    Color(index, color)

    InvalidateData()

end

function OnSystemColorDropTarget(src, dest)

    -- If the src and the dest are the same, we want to swap colors
    if(src.name == dest.name) then

        -- Get the source color ID
        srcPos = src.pressSelection

        -- Get the destination color ID
        local destPos = pixelVisionOS:CalculateItemPickerPosition(src)

        local pageOffset = (systemColorPickerData.pages.currentSelection - 1) * systemColorPickerData.totalPerPage

        if((destPos.index - pageOffset) < dest.picker.total) then

            -- Need to shift src and dest ids based onthe color offset
            local realSrcID = srcPos.index + src.altColorOffset
            local realDestID = destPos.index + dest.altColorOffset
            -- Make sure the colors are not the same
            if(realSrcID ~= realDestID) then

                -- Get the src and dest color hex value
                local srcColor = Color(realSrcID)
                local destColor = Color(realDestID)

                -- Make sure we are not moving a transparent color
                if(srcColor == pixelVisionOS.maskColor or destColor == pixelVisionOS.maskColor) then

                    if(usePalettes == true and dest.name == systemColorPickerData.name) then

                        pixelVisionOS:ShowMessageModal(toolName .." Error", "You can not replace the last color which is reserved for transparency.", 160, false)

                        return

                    end

                end

                -- Swap the colors in the tool's color memory
                Color(realSrcID, destColor)
                Color(realDestID, srcColor)

                -- Update just the colors that changed
                local srcPixelData = pixelVisionOS:ReadItemPickerOverPixelData(src, srcPos.x, srcPos.y)

                local destPixelData = pixelVisionOS:ReadItemPickerOverPixelData(dest, destPos.x, destPos.y)

                src.canvas.SetPixels(srcPos.x, srcPos.y, src.itemSize.x, src.itemSize.y, srcPixelData)

                pixelVisionOS:InvalidateItemPickerDisplay(src)

                src.canvas.SetPixels(destPos.x, destPos.y, dest.itemSize.x, dest.itemSize.y, destPixelData)
                -- Redraw the color page
                pixelVisionOS:InvalidateItemPickerDisplay(dest)

                pixelVisionOS:DisplayMessage("Color ID '"..srcColor.."' was swapped with Color ID '"..destColor .."'", 5)

                -- Invalidate the data so the tool can save
                InvalidateData()

            end

        end

    end

end

function OnEditSprites()
    pixelVisionOS:ShowMessageModal("Edit Sprites", "Do you want to open the Sprite Editor? All unsaved changes will be lost.", 160, true,
        function()
            if(pixelVisionOS.messageModal.selectionValue == true) then

                -- Set up the meta data for the editor for the current directory
                local metaData = {
                    directory = rootDirectory,
                }

                -- Load the tool
                LoadGame(spriteEditorPath, metaData)

            end

        end
    )
end

local lastMode = nil

-- Changes the focus of the currently selected color picker
function ForcePickerFocus(src)

    -- Only one picker can be selected at a time so remove the selection from the opposite one.
    if(src == nil) then

        -- Save the mode
        selectionMode = NoColorMode

        -- Disable input fields
        editorUI:Enable(colorIDInputData, false)
        ToggleHexInput(false)

        pixelVisionOS:ClearItemPickerSelection(systemColorPickerData)
        pixelVisionOS:ClearItemPickerSelection(paletteColorPickerData)

        -- Disable all option
        pixelVisionOS:EnableMenuItem(AddShortcut, false)
        pixelVisionOS:EnableMenuItem(ClearShortcut, false)
        pixelVisionOS:EnableMenuItem(EditShortcut, false)
        pixelVisionOS:EnableMenuItem(DeleteShortcut, false)
        pixelVisionOS:EnableMenuItem(BGShortcut, false)
        pixelVisionOS:EnableMenuItem(CopyShortcut, false)
        pixelVisionOS:EnableMenuItem(PasteShortcut, false)

    elseif(src.name == systemColorPickerData.name) then

        -- Change the color mode to system color mode
        selectionMode = SystemColorMode

        -- Clear the picker selection
        pixelVisionOS:ClearItemPickerSelection(paletteColorPickerData)

        -- Enable the hex input field
        ToggleHexInput(true)

    elseif(src.name == paletteColorPickerData.name) then

        -- Change the selection mode to palette mode
        selectionMode = PaletteMode

        pixelVisionOS:EnableMenuItem(AddShortcut, false)
        pixelVisionOS:EnableMenuItem(DeleteShortcut, false)
        -- Clear the system color picker selection
        pixelVisionOS:ClearItemPickerSelection(systemColorPickerData)

        -- Disable the hex input since you can't change palette colors directly
        ToggleHexInput(false)

    end

    -- Save the last mode
    lastMode = selectionMode

end

local copyValue = nil

function OnCopy()

    local src = lastMode == 1 and systemColorPickerData or paletteColorPickerData

    local colorID = src.currentSelection + src.altColorOffset

    copyValue = Color(colorID)

    -- print("OnCopy", lastMode, src.name, colorID, copyValue)

    -- TODO This should only be enabled when you are in a palette since you can't copy system colors
    pixelVisionOS:EnableMenuItem(PasteShortcut, true)

    pixelVisionOS:DisplayMessage("Copied Color '"..copyValue .."'.")

end

function OnPaste()

    if(copyValue == nil or lastMode == 1) then
        return
    end

    local src = lastMode == 1 and systemColorPickerData or paletteColorPickerData

    local colorID = src.currentSelection + src.altColorOffset

    -- print("Paste", lastMode, src.name, colorID, src.currentSelection, src.altColorOffset)

    Color(colorID, copyValue)
    pixelVisionOS:EnableMenuItem(CopyShortcut, true)
    pixelVisionOS:EnableMenuItem(PasteShortcut, false)
    copyValue = nil

    InvalidateData()

end


function ToggleHexInput(value)
    editorUI:Enable(colorHexInputData, value)

    DrawText("#", 24, 26, DrawMode.Tile, "input", value and colorHexInputData.highlighterTheme.text or colorHexInputData.highlighterTheme.disabled)

    if(value == false) then
        -- Clear values in fields
        -- Update the color id field
        editorUI:ChangeInputField(colorIDInputData, - 1, false)

        -- Update the color id field
        editorUI:ChangeInputField(colorHexInputData, string.sub(pixelVisionOS.maskColor, 2, 7), false)
    end
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

function OnSave()

    -- Copy all of the colors over to the game
    pixelVisionOS:CopyToolColorsToGameMemory()

    -- These are the default flags we are going to save
    local flags = {SaveFlags.System, SaveFlags.Meta, SaveFlags.Colors, SaveFlags.ColorMap}

    -- TODO need to tell if we are not in palette mode any more and recolor sprites and delete color-map.png file?

    gameEditor:WriteMetadata("paletteMode", usePalettes and "true" or "false")

    -- If the sprites have been re-indexed and we are using palettes we need to save the changes
    if(spritesInvalid == true) then

        -- print("Save Sprites", usePalettes)
        if(usePalettes == true) then

            -- Add the color map flag
            table.insert(flags, SaveFlags.ColorMap)

        else
            -- TODO look for a color-map.png file in the current directory and delete it
        end

        -- Add the sprite flag
        table.insert(flags, SaveFlags.Sprites)

        spritesInvalid = false

    end

    -- This will save the system data, the colors and color-map
    gameEditor:Save(rootDirectory, flags)

    -- Display a message that everything was saved
    pixelVisionOS:DisplayMessage("Your changes have been saved.", 5)

    -- Clear the validation
    ResetDataValidation()

end

local maskColorError = "This color is reserved for transparency and can not be assigned to a system color."

-- Adds a transparent color to the end of the system color or palette color picker. This only works if there is enough space and the system color picker doesn't already have a transparent color.
function OnAdd()

    if(selectionMode == PaletteMode) then

        -- Make sure we are not at the end of the total per page value
        if(paletteColorPickerData.visiblePerPage < paletteColorPickerData.totalPerPage) then


            local index = paletteColorPickerData.picker.selected

            -- Rebuild all of the palettes

            local totalPerPalette = 16
            local totalPalettes = self.totalPaletteColors / 16

            for i = 1, totalPalettes do

                local palette = {}

                for j = 1, totalPerPalette do

                end

                table.insert(palette, index, 0)

            end

            pixelVisionOS:ColorPickerVisiblePerPage(paletteColorPickerData, paletteColorPickerData.visiblePerPage + 1)

            InvalidateData()

            -- Disable add option

            UpdateAddDeleteShortcuts()

            gameEditor:ColorsPerSprite(paletteColorPickerData.visiblePerPage)


        end

    else

        if(systemColorPickerData.total >= gameEditor:MaximumColors()) then
            pixelVisionOS:ShowMessageModal(toolName .." Error", "You can not add any more system colors. Please increase the maximum color limit in the data.json file.", 160, false)

            return
        end

        local currentSelection = systemColorPickerData.currentSelection

        if(editColorModal == nil) then
            editColorModal = EditColorModal:Init(editorUI, pixelVisionOS.maskColor)
        end

        editColorModal:SetColor()

        pixelVisionOS:OpenModal(editColorModal,
            function()

                if(editColorModal.selectionValue == true) then

                    local newColorHex = "#"..editColorModal.colorHexInputData.text

                    if(newColorHex == pixelVisionOS.maskColor) then

                        -- if(usePalettes == false) then
                        --   OnClear()
                        -- else

                        pixelVisionOS:ShowMessageModal(toolName .." Error", maskColorError, 160, false)

                        -- end

                        return false

                    else

                        -- Make sure the game as the current colors from the tool
                        pixelVisionOS:CopyToolColorsToGameMemory()

                        -- Get all the colors in the game
                        local colors = gameEditor:Colors()

                        -- Make sure this color doesn't already exist

                        local matchingColorIndex = table.indexOf(colors, newColorHex)
                        if(matchingColorIndex > - 1) then

                            pixelVisionOS:ShowMessageModal(toolName .." Error", "'".. newColorHex .."' the same as system color ".. (matchingColorIndex - 1) ..", enter a new color.", 160, false)

                        else

                            table.insert(colors, currentSelection + 2, newColorHex)

                            -- TODO need to clamp this to 256 or 128 depending on what mode
                            -- Increase the system colors by 1
                            pixelVisionOS.totalSystemColors = pixelVisionOS.totalSystemColors + 1

                            -- Copy the colors back to the editor
                            for i = 1, pixelVisionOS.totalSystemColors do

                                local index = (i - 1) + pixelVisionOS.colorOffset
                                Color(index, colors[i])

                            end


                            pixelVisionOS:AddNewColorToPicker(systemColorPickerData)
                            --
                            -- pixelVisionOS:RefreshColorPickerColor(systemColorPickerData, currentSelection + 1)
                            -- pixelVisionOS:ChangeColorPickerTotal(systemColorPickerData, pixelVisionOS.totalSystemColors, true)

                            pixelVisionOS:SelectColorPickerIndex(systemColorPickerData, currentSelection + 1)

                            OnSelectSystemColor(currentSelection + 1)

                            InvalidateData()

                        end
                    end

                end
            end
        )

    end

end

function UpdateAddDeleteShortcuts()

    -- Make sure add is active if there are extra color spaces in the palette
    pixelVisionOS:EnableMenuItem(AddShortcut, paletteColorPickerData.visiblePerPage < 16 and canEdit == true)

    -- Make sure delete is active if there are more than two colors
    pixelVisionOS:EnableMenuItem(DeleteShortcut, paletteColorPickerData.visiblePerPage > 2 and canEdit == true)

end

-- This method handles the logic for clearing a color based on the currently selected palette and the current color mode.
function OnClear()

    -- Find the currently selected picker
    local picker = selectionMode == SystemColorMode and systemColorPickerData or paletteColorPickerData

    -- Get the real color ID from the offset
    local realColorID = picker.currentSelection + picker.altColorOffset

    -- Set the color to the mask value
    Color(realColorID, pixelVisionOS.maskColor)

    pixelVisionOS:DrawColorPickerColorItem(picker, picker.currentSelection)

    -- Redraw the pickers current page
    -- pixelVisionOS:DrawColorPage(picker)

    -- Invalidate the tool's data
    InvalidateData()

end

function OnDelete()

    if(selectionMode == SystemColorMode) then
        OnDeleteSystemColor(systemColorPickerData.currentSelection)
        -- elseif(selectionMode == PaletteMode) then
        --   OnDeletePaletteColor(paletteColorPickerData.currentSelection)
    end
    --
    -- UpdateHexColor("ff00ff")
end

function UpdateHexColor(value)

    if(selectionMode == PaletteMode) then
        return false
    end

    value = "#".. value

    local colorID = systemColorPickerData.currentSelection

    local realColorID = colorID + systemColorPickerData.altColorOffset

    local currentColor = Color(realColorID)

    if(value == pixelVisionOS.maskColor) then

        -- if(usePalettes == false) then
        --   OnClear()
        -- else

        pixelVisionOS:ShowMessageModal(toolName .." Error", maskColorError, 160, false)

        -- end

        return false

    else

        -- Make sure the color isn't duplicated when in palette mode
        for i = 1, 128 do

            -- Test the new color against all of the existing system colors
            if(value == Color(pixelVisionOS.colorOffset + (i - 1))) then

                pixelVisionOS:ShowMessageModal(toolName .." Error", "'".. value .."' the same as system color ".. (i - 1) ..", enter a new color.", 160, false,
                    -- Make sure we restore the color value after the modal closes
                    function()

                        -- Change the color back to the original value in the input field
                        editorUI:ChangeInputField(colorHexInputData, currentColor:sub(2, - 1), false)

                    end
                )

                -- Exit out of the update function
                return false

            end

        end

        -- Test if the color is at the end of the picker and the is room to add a new color
        if(colorID == systemColorPickerData.total - 1 and systemColorPickerData.total < 255) then

            -- TODO this should use the Add Color logic?
            -- pixelVisionOS:AddNewColorToPicker(systemColorPickerData)

            -- TODO need to rebuild the pages if we are in the system color picker

            -- Select the current color we are editing
            pixelVisionOS:SelectColorPickerColor(systemColorPickerData, realColorID)

        end

        -- end

        -- Update the editor's color
        local newColor = Color(realColorID, value)


        -- TODO this should be used in the palette not the system colors
        -- pixelVisionOS:RefreshColorPickerColor(systemColorPickerData, colorID)

        -- After updating the color, check to see if in palette mode and replace all matching colors in the palettes
        if(usePalettes == true) then

            -- Loop through the palette color memery to remove replace all matching colors
            for i = 127, pixelVisionOS.totalColors do

                local index = (i - 1) + pixelVisionOS.colorOffset

                -- Get the current color in the tool's memory
                local tmpColor = Color(index)

                -- See if that color matches the old color
                if(tmpColor == currentColor and tmpColor ~= pixelVisionOS.maskColor) then

                    -- Set the color to equal the new color
                    Color(index, value)

                end

            end

        end

        -- Redraw the pickers current page
        -- pixelVisionOS:DrawColorPage(systemColorPickerData)

        InvalidateData()

        return true
    end

end

function OnDeleteSystemColor(value)

    -- Calculate the total system colors from the picker
    local totalColors = systemColorPickerData.total - 1

    if(totalColors == 1) then

        -- Display a message to keep the user from deleting all of the system colors
        pixelVisionOS:ShowMessageModal(toolName .. " Error", "You must have at least 2 colors.", 160, false)

    else

        pixelVisionOS:ShowMessageModal("Delete Color", "Are you sure you want to delete this system color? Doing so will shift all the colors over and may change the colors in your sprites." .. (usePalettes and " This color will also be removed from any palettes that are referencing it." or ""), 160, true,
            function()
                if(pixelVisionOS.messageModal.selectionValue == true) then
                    -- If the selection if valid, remove the system color
                    DeleteSystemColor(value)
                end

            end
        )
    end
    -- end

end

function DeleteSystemColor(value)

    local currentSelection = systemColorPickerData.currentSelection

    -- Calculate the real color ID in the tool's memory
    local realColorID = value + systemColorPickerData.altColorOffset

    -- Set the current tool's color to the mask value
    Color(realColorID, pixelVisionOS.maskColor)

    -- Copy all the colors to the game
    pixelVisionOS:CopyToolColorsToGameMemory()

    -- Reimport the game colors to rebuild the unique system color list
    pixelVisionOS:ImportColorsFromGame()

    -- Remove the last system color from the picker
    pixelVisionOS:RemoveColorFromPicker(systemColorPickerData)

    -- Need to rebuild the palette cache
    if(usePalettes == true) then
        -- TODO need to enable this

        -- TODO this is probably not needed since the update above should automatically remove any colors in the palette
        pixelVisionOS:RebuildColorPickerCache(paletteColorPickerData)
    end

    pixelVisionOS:SelectColorPickerIndex(systemColorPickerData, Clamp(currentSelection - 1, 0, 255))

    RefreshBGColorIcon()

    -- Invalidate the tool's data
    InvalidateData()

end

function RefreshBGColorIcon()

    -- Correct the BG color if needed
    if(gameEditor:BackgroundColor() >= pixelVisionOS.totalSystemColors) then

        gameEditor:BackgroundColor(pixelVisionOS.totalSystemColors - 1)

    end

    UpdateBGIconPosition(gameEditor:BackgroundColor())

end

-- Manages selecting the correct color from a picker based on a change to the color id field
function ChangeColorID(value)

    value = tonumber(value)
    -- print("Change Color ID", value)
    -- Check to see what mode we are in
    if(selectionMode == SystemColorMode) then

        -- Select the new color id in the system color picker
        pixelVisionOS:SelectColorPickerColor(systemColorPickerData, value)

    elseif(selectionMode == PaletteMode) then

        -- Select the new color id in the palette color picker
        pixelVisionOS:SelectColorPickerColor(paletteColorPickerData, value)

    end

end

local lastSelection = nil
local lastColor = nil

-- This is called when the picker makes a selection
function OnSelectSystemColor(value)

    -- Calculate the color ID from the picker
    -- local colorID = pixelVisionOS:CalculateRealColorIndex(systemColorPickerData, value)

    -- Update the ID input field's max value from the OS's system color total
    colorIDInputData.max = pixelVisionOS.totalSystemColors - 1

    -- Enable the color id input field
    editorUI:Enable(colorIDInputData, true)

    -- Update the color id field
    editorUI:ChangeInputField(colorIDInputData, tostring(value), false)

    -- Enable the hex input field
    ToggleHexInput(true)

    -- Get the current hex value of the selected color
    local colorHex = Color(value + systemColorPickerData.altColorOffset):sub(2, - 1)

    if(lastSelection ~= value) then

        lastSelection = value
        lastColor = colorHex

    end

    -- Update the selected color hex value
    editorUI:ChangeInputField(colorHexInputData, colorHex, false)

    -- Update menu menu items

    -- TODO need to enable this when the color editor pop-up is working
    pixelVisionOS:EnableMenuItem(EditShortcut, canEdit)

    -- These are only available based on the palette mode
    pixelVisionOS:EnableMenuItem(AddShortcut, canEdit)
    pixelVisionOS:EnableMenuItem(DeleteShortcut, canEdit)
    pixelVisionOS:EnableMenuItem(BGShortcut, ("#"..colorHex) ~= pixelVisionOS.maskColor)

    -- You can only copy a color when in direct color mode
    -- pixelVisionOS:EnableMenuItem(ClearShortcut, not usePalettes)
    pixelVisionOS:EnableMenuItem(CopyShortcut, true)

    -- Only enable the paste button if there is a copyValue and we are not in palette mode
    pixelVisionOS:EnableMenuItem(PasteShortcut, copyValue ~= nil and usePalettes == false)

end

function OnSelectPaletteColor(value)

    -- local colorID = pixelVisionOS:CalculateRealColorIndex(paletteColorPickerData, value)

    colorIDInputData.max = 256

    -- Disable the hex input field
    ToggleHexInput(false)
    editorUI:Enable(colorIDInputData, false)

    editorUI:ChangeInputField(colorIDInputData, tostring(value + 128), false)

    local colorHex = Color(value + paletteColorPickerData.altColorOffset):sub(2, - 1)

    -- Update the selected color hex value
    editorUI:ChangeInputField(colorHexInputData, colorHex, false)

    -- Update menu menu items
    pixelVisionOS:EnableMenuItem(AddShortcut, false)
    pixelVisionOS:EnableMenuItem(DeleteShortcut, false)
    pixelVisionOS:EnableMenuItem(EditShortcut, false)
    pixelVisionOS:EnableMenuItem(ClearShortcut, true)

    pixelVisionOS:EnableMenuItem(CopyShortcut, true)

    -- Only enable the paste button if there is a copyValue and we are not in palette mode
    pixelVisionOS:EnableMenuItem(PasteShortcut, copyValue ~= nil and usePalettes == true)

end

function Update(timeDelta)

    -- Convert timeDelta to a float
    timeDelta = timeDelta / 1000

    -- This needs to be the first call to make sure all of the editor UI is updated first
    pixelVisionOS:Update(timeDelta)

    -- Only update the tool's UI when the modal isn't active
    if(pixelVisionOS:IsModalActive() == false) then

        if(success == true) then

            pixelVisionOS:UpdateSpritePicker(spritePickerData)

            editorUI:UpdateInputField(colorIDInputData)
            editorUI:UpdateInputField(colorHexInputData)

            -- System picker
            -- pixelVisionOS:UpdateColorPicker(gameColorPicker)
            pixelVisionOS:UpdateColorPicker(systemColorPickerData)

            -- Only update the palette color picker when we are in palette mode
            if(usePalettes == true) then
                pixelVisionOS:UpdateColorPicker(paletteColorPickerData)
            end

            if(IsExporting()) then
                pixelVisionOS:DisplayMessage("Saving " .. tostring(ReadExportPercent()).. "% complete.", 2)
            end

        end

    end

end

function Draw()

    RedrawDisplay()

    -- The ui should be the last thing to update after your own custom draw calls
    pixelVisionOS:Draw()

    if(showBGIcon == true and pixelVisionOS:IsModalActive() == false) then

        DrawSprites(bgflagicon.spriteIDs, systemColorPickerData.rect.x + BGIconX, systemColorPickerData.rect.y + BGIconY, bgflagicon.width, false, false, DrawMode.UI)

    end

    if(debugMode) then
        colorMemoryCanvas:DrawPixels(256 - (8 * 3) - 2, 12, DrawMode.UI, 3)
    end

end

function Shutdown()

    -- Save the current session ID
    WriteSaveData("sessionID", SessionID())

    WriteSaveData("rootDirectory", rootDirectory)

    if(systemColorPickerData ~= nil) then
        WriteSaveData("selectedSpritePage", spritePickerData.pages.currentSelection)
    end
    if(paletteColorPickerData ~= nil) then
        WriteSaveData("selectedPalettePage", paletteColorPickerData.pages.currentSelection)
    end

end

function OnSetBGColor()

    local oldBGColor = gameEditor:BackgroundColor()

    local colorID = systemColorPickerData.currentSelection

    pixelVisionOS:ShowMessageModal("Set Background Color", "Do you want to change the current background color ID from " .. oldBGColor .. " to " .. colorID .. "?", 160, true,
        function()
            if(pixelVisionOS.messageModal.selectionValue == true) then

                gameEditor:BackgroundColor(colorID)

                showBGIcon = true

                RefreshBGColorIcon()

                InvalidateData()
            end
        end
    )

end

function UpdateBGIconPosition(id)

    local pos = CalculatePosition(id % 64, 8)

    BGIconX = pos.x * 16
    BGIconY = pos.y * 16

end

function OnRunGame()
    -- TODO should this ask to launch the game first?

    if(invalid == true) then

        pixelVisionOS:ShowMessageModal("Unsaved Changes", "You have unsaved changes. You will lose those changes if you run the game now?", 160, true,
            function()
                if(pixelVisionOS.messageModal.selectionValue == true) then
                    LoadGame(NewWorkspacePath(rootDirectory))
                end

            end
        )

    else

        LoadGame(NewWorkspacePath(rootDirectory))

    end

end
