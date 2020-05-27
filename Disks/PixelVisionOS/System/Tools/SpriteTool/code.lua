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
LoadScript("pixel-vision-os-sprite-picker-v3")
LoadScript("pixel-vision-os-canvas-v2")
LoadScript("pixel-vision-os-item-picker-v1")

local toolName = "Sprite Tool"

local colorOffset = 0
local spriteIDInputData = nil
local rootDirectory = "/"
local sizeBtnData = nil
local toolBtnData = nil
local canvasData = nil
local flipHButton = nil
local flipVButton = nil
local usePalettes = false
-- local spritePickerData = nil
-- local paletteColorPickerData = nil
local lastSystemColorSelection = nil
local toolTitle = nil
local colorPreviewInvalid = false
local showBGColor = false

-- local systemColorsPerPage = 64
local success = false
-- local emptyColorID = -1
local originalPixelData = nil
local lastSelection = -1
local lastColorID = 0
local colorEditorPath = "/"
-- local colorCountInvalid = false
-- local flipH = false
-- local flipV = false
local spriteSize = 1
local maxSpriteSize = 4
local uniqueColors = {}
local cps = 16

local tools = {"pen", "line", "box", "circle", "eyedropper", "fill", "select"}

local toolKeys = {Keys.P, Keys.L, Keys.B, Keys.C, Keys.I, Keys.F, Keys.M}

local ClearShortcut, SaveShortcut, RevertShortcut, UndoShortcut, RedoShortcut, CopyShortcut, PasteShortcut = 4, 5, 6, 8, 9, 10, 11

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

    -- print("Reset Validation")
    -- Only everything if it needs to be
    if(invalid == false)then
        return
    end

    pixelVisionOS:ChangeTitle(toolTitle, "toolbariconfile")
    invalid = false

    pixelVisionOS:EnableMenuItem(SaveShortcut, false)

end

function Init()


    BackgroundColor(22)

    -- Disable the back key in this tool
    EnableBackKey(false)

      -- Create an global instance of the Pixel Vision OS
    _G["pixelVisionOS"] = PixelVisionOS:Init()

    -- Create an instance of the Pixel Vision OS
    -- pixelVisionOS = PixelVisionOS:Init()

    -- Reset the undo history so it's ready for the tool
    pixelVisionOS:ResetUndoHistory()

    -- Get a reference to the Editor UI
    -- editorUI = pixelVisionOS.editorUI

    rootDirectory = ReadMetadata("directory", nil)

    if(rootDirectory == nil) then

    else
        -- Load only the game data we really need
        success = gameEditor:Load(rootDirectory, {SaveFlags.System, SaveFlags.Meta, SaveFlags.Colors, SaveFlags.ColorMap, SaveFlags.Sprites}) -- colorEditor:LoadTmpEngine()
    end

    -- If data loaded activate the tool
    if(success == true) then

        cps = gameEditor:ColorsPerSprite()

        -- Get a list of all the editors
        local editorMapping = pixelVisionOS:FindEditors()

        -- Find the json editor
        colorEditorPath = editorMapping["colors"]

        local menuOptions = 
        {
            -- About ID 1
            {name = "About", action = function() pixelVisionOS:ShowAboutModal(toolName) end, toolTip = "Learn about PV8."},
            {divider = true},
            {name = "Edit Colors", enabled = colorEditorPath ~= nil, action = OnEditColors, toolTip = "Open the color editor."},
            {name = "Clear", action = OnClear, enabled = false, key = Keys.D, toolTip = "Clear the currently selected sprite."}, -- Reset all the values
            {name = "Save", action = OnSave, key = Keys.S, enabled = false, toolTip = "Save changes made to the sprites file."}, -- Reset all the values

            {name = "Revert", action = OnRevert, enabled = false, toolTip = "Revert the sprite to its previous state."}, -- Reset all the values
            {divider = true},
            {name = "Undo", action = OnUndo, enabled = false, key = Keys.Z, toolTip = "Undo the last action."}, -- Reset all the values
            {name = "Redo", action = OnRedo, enabled = false, key = Keys.Y, toolTip = "Redo the last undo."}, -- Reset all the values
            {name = "Copy", action = OnCopySprite, enabled = false, key = Keys.C, toolTip = "Copy the currently selected sprite."}, -- Reset all the values
            {name = "Paste", action = OnPasteSprite, enabled = false, key = Keys.V, toolTip = "Paste the last copied sprite."}, -- Reset all the values
            {name = "Fill", action = OnFill, enabled = true, key = Keys.F, toolTip = "Paste the last copied sprite."}, -- Reset all the values
            {name = "Select All", action = OnSelectAll, enabled = true, key = Keys.A, toolTip = "Paste the last copied sprite."}, -- Reset all the values

            {divider = true},
            {name = "BG Color", action = function() ToggleBackgroundColor(not showBGColor) end, key = Keys.B, toolTip = "Toggle background color."},
            {name = "Optimize", action = OnOptimize, toolTip = "Remove duplicate sprites."},
            {name = "Sprite Builder", action = OnSpriteBuilder, toolTip = "Generate a sprite table from a project's SpriteBuilder dir."}, -- Reset all the values
            {divider = true},
            {name = "Quit", key = Keys.Q, action = OnQuit, toolTip = "Quit the current game."}, -- Quit the current game
        }

        if(PathExists(NewWorkspacePath(rootDirectory).AppendFile("code.lua"))) then
            table.insert(menuOptions, #menuOptions, {name = "Run Game", action = OnRunGame, key = Keys.R, toolTip = "Run the code for this game."})
        end

        pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")

        -- The first thing we need to do is rebuild the tool's color table to include the game's system and game colors.

        ImportColorsFromGame()

        _G["itempickerover"] = {spriteIDs = colorselection.spriteIDs, width = colorselection.width, colorOffset = 28}

        _G["itempickerselectedup"] = {spriteIDs = colorselection.spriteIDs, width = colorselection.width, colorOffset = (_G["itempickerover"].colorOffset + 4)}

        ConfigureSpritePickerSelector(1)

        local textColorOffset = 55

        spriteIDInputData = editorUI:CreateInputField({x = 176, y = 208, w = 32}, "0", "The ID of the currently selected sprite.", "number", nil, textColorOffset)
        spriteIDInputData.min = 0
        spriteIDInputData.max = gameEditor:TotalSprites() - 1
        spriteIDInputData.onAction = ChangeSpriteID

        sizeBtnData = editorUI:CreateButton({x = 224, y = 200}, "sprite1x", "Pick the sprite size.")
        sizeBtnData.onAction = function() OnNextSpriteSize() end

        toolBtnData = editorUI:CreateToggleGroup()
        toolBtnData.onAction = OnSelectTool

        -- TODO if using palettes, need to replace this with palette color value
        local totalColors = pixelVisionOS.totalSystemColors
        local totalPerPage = 16--pixelVisionOS.systemColorsPerPage
        local maxPages = 16
        colorOffset = pixelVisionOS.colorOffset

        -- Check the game editor if palettes are being used
        usePalettes = pixelVisionOS.paletteMode

        -- Add the eraser if we are in direct color mode
        table.insert(tools, 2, "eraser")
        table.insert(toolKeys, 2, Keys.E)

        if(usePalettes == true) then

            -- Change the total colors when in palette mode
            totalColors = 128
            colorOffset = colorOffset + 128

            -- Change color label to palette
            -- DrawSprites(gamepalettetext.spriteIDs, 32 / 8, 168 / 8, gamepalettetext.width, false, false, DrawMode.Tile)

        end

        local offsetY = 0
        -- Build tools
        for i = 1, #tools do
            offsetY = ((i - 1) * 16) + 32
            local rect = {x = 152, y = offsetY, w = 16, h = 16}
            editorUI:ToggleGroupButton(toolBtnData, rect, tools[i], "Select the '" .. tools[i] .. "' (".. tostring(toolKeys[i]) .. ") tool.")
        end

        canvasData = pixelVisionOS:CreateCanvas(
            {
                x = 16,
                y = 32,
                w = 128,
                h = 128
            },
            {
                x = 128,
                y = 128
            },
            1,
            colorOffset,
            "Draw on the canvas",
            pixelVisionOS.emptyColorID
        )

        -- TODO draw flip buttons
        flipHButton = editorUI:CreateButton({x = 152, y = offsetY + 16 + 8, w = 16, h = 16}, "hflip", "Preview the sprite flipped horizontally.")

        flipHButton.onAction = function(value)
            -- Save the new pixel data to history
            SaveCanvasState()

            -- Update the canvas and flip the H value
            UpdateCanvas(lastSelection, true, false)

            

            -- Save the new pixel data back to the sprite chip
            OnSaveCanvasChanges()

        end

        flipVButton = editorUI:CreateButton({x = 152, y = offsetY + 32 + 8, w = 16, h = 16}, "vflip", "Preview the sprite flipped vertically.")

        flipVButton.onAction = function(value)
            
            -- Save the new pixel data to history
            SaveCanvasState()

            -- Update the canvas and flip the H value
            UpdateCanvas(lastSelection, false, true)
            
            
 
            -- Save the new pixel data back to the sprite chip
            OnSaveCanvasChanges()

        end



        canvasData.onPress = function()
            
            SaveCanvasState()
            -- local pixelData = editorUI:GetCanvasPixelData(canvasData)

            canvasData.inDrawMode = true

            -- UpdateHistory(pixelData)
        end

        editorUI.collisionManager:EnableDragging(canvasData, .5, "SpritePicker")

        canvasData.onDropTarget = OnCanvasDrop

        canvasData.onAction = OnSaveCanvasChanges

        -- Get sprite texture dimensions
        local totalSprites = gameEditor:TotalSprites()
        -- This is fixed size at 16 cols (128 pixels wide)
        local spriteColumns = 16
        local spriteRows = math.ceil(totalSprites / 16)

        -- Create item picker
        spritePickerData = pixelVisionOS:CreateSpritePicker(
            {x = 176, y = 32, w = 64, h = 128},
            {x = 8, y = 8},
            spriteColumns,
            spriteRows,
            pixelVisionOS.colorOffset,
            "spritepicker",
            "sprite",
            true,
            "SpritePicker"
        )

        spritePickerData.onRelease = OnSelectSprite
        spritePickerData.onDropTarget = OnSpritePickerDrop

        -- TODO setting the total to 0
        paletteColorPickerData = pixelVisionOS:CreateColorPicker(
            {x = 16, y = 184, w = 128, h = 32},
            {x = 16, y = 16},
            totalColors,
            totalPerPage,
            maxPages,
            colorOffset,
            "itempicker",
            usePalettes == true and "Select palette color " or "Select system color ",
            false,
            false,
            false
        )

        if(usePalettes == true) then
            -- Force the palette picker to only display the total colors per sprite
            paletteColorPickerData.visiblePerPage = pixelVisionOS.paletteColorsPerPage
        end

        paletteColorPickerData.onAction = function(value)

            -- if we are in palette mode, just get the currents selection. If we are in direct color mode calculate the real color index
            if(usePalettes) then
                value = paletteColorPickerData.picker.selected
            end

            -- Make sure if we select the last color, we mark it as the mask color
            if(value == paletteColorPickerData.total) then
                value = -1
            end

            lastColorID = value

            local enableCanvas = true

            editorUI:Enable(canvasData, enableCanvas)

            -- Set the canvas brush color
            pixelVisionOS:CanvasBrushColor(canvasData, value)

        end

        -- Wire up the picker to change the color offset of the sprite picker
        paletteColorPickerData.onPageAction = function(value)

            -- If we are not in palette mode, don't change the sprite color offset
            if(usePalettes == true) then

                local pageOffset = ((value - 1) * 16)

                -- Calculate the new color offset
                local newColorOffset = pixelVisionOS.colorOffset + pixelVisionOS.totalPaletteColors + pageOffset

                pixelVisionOS:ChangeItemPickerColorOffset(spritePickerData, newColorOffset)

                -- Update the canvas color offset
                canvasData.colorOffset = newColorOffset

                pixelVisionOS:InvalidateItemPickerDisplay(spritePickerData)

                UpdateCanvas(lastSelection)

                -- Need to reselect the current color in the new palette if we are in draw mode
                if(canvasData.tool ~= "eraser" or canvasData.tool ~= "eyedropper") then

                    lastColorID = Clamp(lastColorID, 0, 15)

                    pixelVisionOS:SelectColorPickerColor(paletteColorPickerData, lastColorID + pageOffset)

                    pixelVisionOS:CanvasBrushColor(canvasData, lastColorID)
                    -- pixelVisionOS:SelectItemPickerIndex(paletteColorPickerData, lastColorID + pageOffset, true, false)

                end

                -- Make sure we shift the colors by the new page number
                InvalidateColorPreview()

            end

        end

        -- Need to convert sprites per page to editor's sprites per page value
        -- local spritePages = math.floor(gameEditor:TotalSprites() / 192)

        if(gameEditor:Name() == ReadSaveData("editing", "undefined")) then
            lastSystemColorSelection = tonumber(ReadSaveData("systemColorSelection", "0"))

        end

        local pathSplit = string.split(rootDirectory, "/")

        -- Update title with file path
        toolTitle = pathSplit[#pathSplit] .. "/" .. "sprites.png"


        editorUI:SelectToggleButton(toolBtnData, 1)

        -- pixelVisionOS:SelectColorPage(paletteColorPickerData, 1)
        pixelVisionOS:SelectColorPickerColor(paletteColorPickerData, 0)

        -- pixelVisionOS:SelectSpritePickerPage(spritePickerData, 1)

        pixelVisionOS:SelectSpritePickerIndex(spritePickerData, 0)
        -- TODO this is not being triggered, need a better way to select the first sprite

        local startSprite = 0

        if(SessionID() == ReadSaveData("sessionID", "") and rootDirectory == ReadSaveData("rootDirectory", "")) then
            startSprite = tonumber(ReadSaveData("selectedSprite", "0"))
            spriteSize = tonumber(ReadSaveData("spriteSize", "1")) - 1
            OnNextSpriteSize()
        end

        -- pixelVisionOS:ResetSpritePicker(spritePickerData)
        -- spritePickerData.currentSelection = -1
        -- -- Change the input field and load the correct sprite
        -- editorUI:ChangeInputField(spriteIDInputData, startSprite)

        ChangeSpriteID(startSprite)

        pixelVisionOS:ChangeCanvasPixelSize(canvasData, spriteSize)

        -- OnSelectSprite(startSprite)
        -- local tmpPixelData = gameEditor:Sprite(0)
        --
        -- -- TODO simulate selecting the first sprite
        -- editorUI:ResizeCanvas(canvasData, NewPoint(8, 8), 16, tmpPixelData)

        ResetDataValidation()

    else

        -- Patch background when loading fails

        -- Left panel
        DrawRect(8, 32, 128, 128, 0, DrawMode.TilemapCache)

        DrawRect(168, 32, 80, 128, 0, DrawMode.TilemapCache)

        DrawRect(8, 184, 128, 32, 0, DrawMode.TilemapCache)

        DrawRect(176, 208, 32, 8, 0, DrawMode.TilemapCache)

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

function OnFill()

    print("Fill")
    
    local colorID = 0
    pixelVisionOS:FillCanvasSelection(canvasData)

end



function DrawColorPerSpriteDisplay()

    -- TODO create unique colors

    local pixelData = gameEditor:ReadGameSpriteData(spritePickerData.currentSelection, spriteSize, spriteSize)

    -- Clear unique color list

    uniqueColors = {}

    -- loop through all the pixel data and look for unique colors
    for i = 1, #pixelData do

        -- Get the color id and the index if it exists in the unique color array
        local colorID = pixelData[i]

        if(colorID > - 1) then
            local index = table.indexOf(uniqueColors, colorID)

            -- If this is a new color, add it to the unique color array
            if(index == - 1) then
                table.insert(uniqueColors, colorID)
            end
        end

    end

    local backgroundSprites = {
        _G["colorbarleft"],
        _G["colorbarright"],
    }

    local totalSections = math.ceil(cps / 2)

    local totalColors = Clamp(#uniqueColors, 2, 16)

    -- TODO need to fix this
    if(totalColors / 2 > totalSections) then
        totalSections = totalColors / 2
    end

    for i = 1, totalSections do
        table.insert(backgroundSprites, 2, _G["colorbarmiddle"])
    end

    local totalSections = #backgroundSprites

    local maxSections = 12

    local shiftOffset = 0

    -- Pad background
    if(totalSections < maxSections) then

        local emptyTotal = maxSections - totalSections

        shiftOffset = emptyTotal * 8

        for i = 1, emptyTotal do
            table.insert(backgroundSprites, 1, _G["pagebuttonempty"])
        end

    end

    -- local startX = 144
    local nextX = 152 ---- (8 - totalSections * 8)

    for i = maxSections, 1, - 1 do

        nextX = nextX - 8

        -- for i = 1, #backgroundSprites do
        DrawSprites(backgroundSprites[i].spriteIDs, nextX, 160, 1, false, false, DrawMode.TilemapCache)

    end

    local colorOffset = pixelVisionOS.colorOffset
    --
    if(pixelVisionOS.paletteMode) then

        colorOffset = colorOffset + 128 + ((paletteColorPickerData.pages.currentSelection - 1) * 16)

    end

    -- Shift next x over
    nextX = nextX + 4 + shiftOffset
    for i = 1, cps do

        local color = i <= #uniqueColors and uniqueColors[i] + colorOffset or pixelVisionOS.emptyColorID
        --
        -- local drawColor = true
        -- --
        -- if(usePalettes == false and color == pixelVisionOS.emptyColorID) then
        --   drawColor = false
        -- end

        nextX = nextX + 4
        -- if(drawColor == true) then
        DrawRect(nextX, 164, 4, 4, color, DrawMode.TilemapCache)
        -- end
    end

    -- Redraw the palette label over the CPS display background
    if(usePalettes == true) then
        -- Change color label to palette
        DrawSprites(gamepalettetext.spriteIDs, 32 / 8, 168 / 8, gamepalettetext.width, false, false, DrawMode.Tile)

    end

end

function InvalidateColorPreview()

    colorPreviewInvalid = true
end

function ResetColorPreviewValidation()
    colorPreviewInvalid = false
end

function OnCanvasDrop(src, dest)

    if(src.name == spritePickerData.name) then

        ChangeSpriteID(src.pressSelection.index)

        -- TODO this is overkill, we just need a way to refresh the selection
        pixelVisionOS:InvalidateItemPickerDisplay(spritePickerData)
    end



end

function OnSpritePickerDrop(src, dest)

    if(dest.inDragArea == false) then
        return
    end

    -- If the src and the dest are the same, we want to swap colors
    if(src.name == dest.name) then

        -- Get the source color ID
        local srcSpriteID = src.pressSelection.index

        -- Exit this swap if there is no src selection
        if(srcSpriteID == nil) then
            return
        end

        -- Get the destination color ID
        local destSpriteID = pixelVisionOS:CalculateItemPickerPosition(src).index

        -- Make sure the colors are not the same
        if(srcSpriteID ~= destSpriteID) then

            -- Need to shift src and dest ids based onthe color offset
            -- local realSrcID = srcSpriteID-- + systemColorPickerData.colorOffset
            -- local realDestID = destSpriteID-- + systemColorPickerData.colorOffset

            -- TODO need to account for the scroll offset?
            -- print("Swap sprite", srcSpriteID, destSpriteID)

            local srcSprite = gameEditor:ReadGameSpriteData(srcSpriteID, spriteSize, spriteSize)
            local destSprite = gameEditor:ReadGameSpriteData(destSpriteID, spriteSize, spriteSize)

            -- Swap the sprite in the tool's color memory
            gameEditor:WriteSpriteData(srcSpriteID, destSprite, spriteSize, spriteSize)
            gameEditor:WriteSpriteData(destSpriteID, srcSprite, spriteSize, spriteSize)

            -- Update the pixel data in the spritePicker

            local itemSize = spriteSize * 8

            pixelVisionOS:UpdateItemPickerPixelDataAt(spritePickerData, srcSpriteID, destSprite, itemSize, itemSize)
            pixelVisionOS:UpdateItemPickerPixelDataAt(spritePickerData, destSpriteID, srcSprite, itemSize, itemSize)

            pixelVisionOS:InvalidateItemPickerDisplay(src)

            -- ChangeSpriteID(destSpriteID)

            InvalidateData()

        end

    end

end

function OnEditColors()
    pixelVisionOS:ShowMessageModal("Edit Colors", "Do you want to open the Color Editor? All unsaved changes will be lost.", 160, true,
        function()
            if(pixelVisionOS.messageModal.selectionValue == true) then

                -- Set up the meta data for the editor for the current directory
                local metaData = {
                    directory = rootDirectory,
                }

                -- Load the tool
                LoadGame(colorEditorPath, metaData)

            end

        end
    )
end

local copiedSpriteData = nil

function OnCopySprite()

    copiedSpriteData = gameEditor:ReadGameSpriteData(spritePickerData.currentSelection, spriteSize, spriteSize)

    pixelVisionOS:EnableMenuItem(PasteShortcut, true)

end

function SaveCanvasState()

    UpdateHistory(pixelVisionOS:GetCanvasPixelData(canvasData))

end

function OnPasteSprite()

    if(copiedSpriteData == nil) then
        return
    end

    -- Shift the pixel data for the canvas
    for i = 1, #copiedSpriteData do
        copiedSpriteData[i] = copiedSpriteData[i] + canvasData.colorOffset
    end

    SaveCanvasState()

    -- Update the canvas with the new pixel data
    pixelVisionOS:SetCanvasPixels(canvasData, copiedSpriteData)

    -- Save the canvas
    OnSaveCanvasChanges()

    copiedSpriteData = nil
    pixelVisionOS:EnableMenuItem(PasteShortcut, false)

end

-- TODO this is probably no longer needed?
function OnRevert()

    pixelVisionOS:ShowMessageModal("Clear Sprite", "Do you want to revert the sprite's pixel data to it's original state?", 160, true,
        function()
            if(pixelVisionOS.messageModal.selectionValue == true) then
                -- Save changes
                RevertSprite()

            end

        end
    )

end

function RevertSprite()

    -- print("Revert Sprite", originalPixelData)

    local index = spritePickerData.currentSelection

    gameEditor:WriteSpriteData(index, originalPixelData, spriteSize, spriteSize)


    -- Select the current sprite to update the canvas
    pixelVisionOS:SelectSpritePickerIndex(spritePickerData, index)

    -- Redraw the sprite picker page
    -- pixelVisionOS:RedrawSpritePickerPage(spritePickerData)

    -- Invalidate the tool's data
    InvalidateData()

    pixelVisionOS:EnableMenuItem(RevertShortcut, false)
    pixelVisionOS:EnableMenuItem(ClearShortcut, not IsSpriteEmpty(originalPixelData))

end

function OnClear()

    pixelVisionOS:ShowMessageModal("Clear Sprite", "Do you want to clear all of the pixel data for the current sprite?", 160, true,
        function()
            if(pixelVisionOS.messageModal.selectionValue == true) then
                -- Save changes
                ClearSprite()

            end

        end
    )

end

function ClearSprite()

    -- Save the new pixel data to history
    SaveCanvasState()

    pixelVisionOS:ClearCanvas(canvasData)
    OnSaveCanvasChanges()
   
    UpdateCanvas(spritePickerData.currentSelection)
    
    -- Invalidate the tool's data
    InvalidateData()

    pixelVisionOS:EnableMenuItem(RevertShortcut, true)
    pixelVisionOS:EnableMenuItem(ClearShortcut, false)

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


function OnOptimize()

    pixelVisionOS:ShowMessageModal("Optimize Sprites", "Before you optimize the sprites, you should make a backup copy of the current 'sprite.png' file. After this process, if you save your changes, it will overwrite the existing 'sprite.png' file.", 160, true,
        function()
            if(pixelVisionOS.messageModal.selectionValue == true) then
                TriggerOptimization()
                InvalidateData()
            end
        end
    )

end

function TriggerOptimization()

    local oldCount = gameEditor:SpritesInRam()

    gameEditor:OptimizeSprites()

    local newCount = gameEditor:SpritesInRam()

    pixelVisionOS:RebuildSpritePickerCache(spritePickerData)

    local percent = Clamp(100 - math.ceil(newCount / oldCount * 100), 0, 100)

    -- Show summary modal and invalidate the data when the modal is closed
    pixelVisionOS:ShowMessageModal("Optimization Complete", "The sprite optimizer was able to compress sprite memory by ".. percent .. "%. If you save your changes, the previous 'sprite.png' file will be overwritten.", 160, false)


end

function OnSaveCanvasChanges()

    canvasData.inDrawMode = false

    -- Get the raw pixel data
    local pixelData = pixelVisionOS:GetCanvasPixelData(canvasData)

    -- Get the canvas size
    local canvasSize = pixelVisionOS:GetCanvasSize(canvasData)

    -- Get the total number of pixel
    local total = #pixelData

    -- Loop through all the pixel data
    for i = 1, total do

        -- Shift the color value based on the canvas color offset
        local newColor = pixelData[i] - canvasData.colorOffset

        -- Set the new pixel index value
        pixelData[i] = newColor < 0 and - 1 or newColor

    end

    -- Redraw the colors per sprite display
    InvalidateColorPreview()

    -- Update the spritePickerData
    if(spritePickerData.currentSelection > - 1) then
        pixelVisionOS:UpdateItemPickerPixelDataAt(spritePickerData, spritePickerData.currentSelection, pixelData, canvasSize.width, canvasSize.height)
    end

    -- Update the current sprite in the picker
    gameEditor:WriteSpriteData(spritePickerData.currentSelection, pixelData, spriteSize, spriteSize)

    -- Test to see if the canvas is invalid
    if(canvasData.invalid == true) then

        -- Invalidate the sprite tool since we change some pixel data
        InvalidateData()

        -- Reset the canvas invalidation since we copied it
        editorUI:ResetValidation(canvasData)

    end

    -- Make sure the clear button is enabled since a change has happened to the canvas
    pixelVisionOS:EnableMenuItem(ClearShortcut, true)
    pixelVisionOS:EnableMenuItem(RevertShortcut, true)

end


function OnSelectTool(value)

    local toolName = tools[value]

    pixelVisionOS:ChangeCanvasTool(canvasData, toolName)

    -- We disable the color selection when switching over to the eraser
    if(toolName == "eraser") then

        --  Clear the current color selection
        pixelVisionOS:ClearItemPickerSelection(paletteColorPickerData)

        -- Disable the color picker
        pixelVisionOS:EnableItemPicker(paletteColorPickerData, false)

        -- Make sure the canvas is enabled
        editorUI:Enable(canvasData, true)

        -- ResetColorInvalidation()

    else

        -- We need to restore the color when switching back to a new tool

        -- Make sure the last color is in range
        if(lastColorID == nil or lastColorID == -1) then

            -- For palette mode, we set the color to the last color per sprite but for direct color mode we set it to the last system color
            lastColorID = usePalettes and gameEditor:ColorsPerSprite() or paletteColorPickerData.total - 1

        end

        -- Enable co
        pixelVisionOS:EnableItemPicker(paletteColorPickerData, true)

        -- Need to find the right color if we are in palette mode
        if(usePalettes == true) then

            -- Need to offset the last color id by the current palette page
            lastColorID = lastColorID + ((paletteColorPickerData.pages.currentSelection - 1) * 16)

        end

        pixelVisionOS:SelectColorPickerColor(paletteColorPickerData, lastColorID)

    end

end



function OnNextSpriteSize(reverse)

    local lastID = tonumber(spriteIDInputData.text)

    -- Loop backwards through the button sizes
    if(Key(Keys.LeftShift) or reverse == true) then
        spriteSize = spriteSize - 1

        -- Skip 24 x 24 selections
        if(spriteSize == 3) then
            spriteSize = 2
        end

        if(spriteSize < 1) then
            spriteSize = maxSpriteSize
        end

        -- Loop forward through the button sizes
    else
        spriteSize = spriteSize + 1

        -- Skip 24 x 24 selections
        if(spriteSize == 3) then
            spriteSize = 4
        end

        if(spriteSize > maxSpriteSize) then
            spriteSize = 1
        end
    end

    -- Find the next sprite for the button
    local spriteName = "sprite"..tostring(spriteSize).."x"

    -- Change sprite button graphic
    sizeBtnData.cachedSpriteData = {
        up = _G[spriteName .. "up"],
        down = _G[spriteName .. "down"] ~= nil and _G[spriteName .. "down"] or _G[spriteName .. "selectedup"],
        over = _G[spriteName .. "over"],
        selectedup = _G[spriteName .. "selectedup"],
        selectedover = _G[spriteName .. "selectedover"],
        selecteddown = _G[spriteName .. "selecteddown"] ~= nil and _G[spriteName .. "selecteddown"] or _G[spriteName .. "selectedover"],
        disabled = _G[spriteName .. "disabled"],
        empty = _G[spriteName .. "empty"] -- used to clear the sprites
    }

    ConfigureSpritePickerSelector(spriteSize)

    -- Need to clear any sprite data that is in the clipboard
    copiedSpriteData = nil
    pixelVisionOS:EnableMenuItem(PasteShortcut, false)

    editorUI:Invalidate(sizeBtnData)

    pixelVisionOS:ChangeCanvasPixelSize(canvasData, spriteSize)

    -- Force the sprite editor to update to the new selection from the sprite picker
    ChangeSpriteID(spritePickerData.currentSelection)

    ClearHistory()

    InvalidateColorPreview()

    -- TODO need to reindex the colors?

end

function ConfigureSpritePickerSelector(size)

    _G["spritepickerover"] = {spriteIDs = _G["spriteselection"..tostring(size) .."x"].spriteIDs, width = _G["spriteselection"..tostring(size) .."x"].width, colorOffset = 28}

    _G["spritepickerselectedup"] = {spriteIDs = _G["spriteselection"..tostring(size) .."x"].spriteIDs, width = _G["spriteselection"..tostring(size) .."x"].width, colorOffset = (_G["spritepickerover"].colorOffset + 2)}

    -- pixelVisionOS:ChangeSpritePickerSize(spritePickerData, size)

    pixelVisionOS:ChangeItemPickerScale(spritePickerData, size)

    -- This is called before the sprite picker is created
    -- if(spritePickerData ~= nil) then
    --   OnSelectSprite(spritePickerData.currentSelection)
    -- end
end

function ImportColorsFromGame()


    pixelVisionOS:ImportColorsFromGame()

end


function OnSave()

    -- TODO need to save all of the colors back to the game

    -- This will save the system data, the colors and color-map
    gameEditor:Save(rootDirectory, {SaveFlags.System, SaveFlags.Sprites})

    -- Display a message that everything was saved
    pixelVisionOS:DisplayMessage("Your changes have been saved.", 5)

    -- Clear the validation
    ResetDataValidation()

end


function OnSelectSprite(value)

    -- Reset history
    ClearHistory()

    -- Update the input field
    editorUI:ChangeInputField(spriteIDInputData, value, false)

    UpdateCanvas(value)

    -- ResetColorInvalidation()

end

local lastCanvasScale = 0
local lastCanvasSize = 0

function UpdateCanvas(value, flipH, flipV)

    flipH = flipH or false
    flipV = flipV or false

    -- Save the original pixel data from the selection
    local tmpPixelData = gameEditor:ReadGameSpriteData(value, spriteSize, spriteSize, flipH, flipV)--
    lastCanvasScale = Clamp(8 * (3 - spriteSize), 4, 16)

    lastCanvasSize = NewPoint(8 * spriteSize, 8 * spriteSize)

    pixelVisionOS:ResizeCanvas(canvasData, lastCanvasSize, lastCanvasScale, tmpPixelData)

    originalPixelData = {}

    -- local colorOffset = pixelVisionOS.gameColorOffset
    -- Need to loop through the pixel data and change the offset
    local total = #tmpPixelData
    for i = 1, total do

        -- TODO index the canvas colors here
        local newColor = tmpPixelData[i] - colorOffset

        originalPixelData[i] = newColor

    end

    lastSelection = value

    pixelVisionOS:EnableMenuItem(RevertShortcut, false)

    -- Only enable the clear menu when the sprite is not empty
    pixelVisionOS:EnableMenuItem(ClearShortcut, not IsSpriteEmpty(tmpPixelData))


    pixelVisionOS:EnableMenuItem(CopyShortcut, true)

    InvalidateColorPreview()

end

function IsSpriteEmpty(pixelData)

    local total = #pixelData

    for i = 1, total do
        if(pixelData[i] ~= -1) then
            return false
        end
    end

    return true

end

function ChangeSpriteID(value)

    -- Need to convert the text into a number
    value = tonumber(value)

    pixelVisionOS:SelectSpritePickerIndex(spritePickerData, value, false)

    editorUI:ChangeInputField(spriteIDInputData, spritePickerData.currentSelection, false)

    ClearHistory()

    UpdateCanvas(spritePickerData.currentSelection)

    spritePickerData.dragging = false

end

function Update(timeDelta)

    -- Convert timeDelta to a float
    timeDelta = timeDelta / 1000

    -- This needs to be the first call to make sure all of the editor UI is updated first
    pixelVisionOS:Update(timeDelta)

    -- Only update the tool's UI when the modal isn't active
    if(pixelVisionOS:IsModalActive() == false) then
        if(success == true) then

            -- Only trigger shortcuts when not editing a text field
            if(spriteIDInputData.editing == false) then

                if(Key(Keys.LeftControl) == false and Key(Keys.RightControl) == false) then

                    for i = 1, #toolKeys do
                        if(Key(toolKeys[i], InputState.Released)) then
                            editorUI:SelectToggleButton(toolBtnData, i)
                            break
                        end
                    end
                end

                -- Change the scale
                if(Key(Keys.OemMinus, InputState.Released) and spriteSize > 1) then
                    OnNextSpriteSize(true)
                elseif(Key(Keys.OemPlus, InputState.Released) and spriteSize < 4) then
                    OnNextSpriteSize()
                end

                -- Create a new piont to see if we need to change the sprite position
                local newPos = NewPoint(0, 0)

                -- Get the sacle from the sprite picker
                local scale = spritePickerData.scale

                -- Offset the new position by the direction button
                if(Key(Keys.Up, InputState.Released)) then
                    newPos.y = -1 * scale
                elseif(Key(Keys.Right, InputState.Released)) then
                    newPos.x = 1 * scale
                elseif(Key(Keys.Down, InputState.Released)) then
                    newPos.y = 1 * scale
                elseif(Key(Keys.Left, InputState.Released)) then
                    newPos.x = -1 * scale
                end


                -- Test to see if the new position has changed
                if(newPos.x ~= 0 or newPos.y ~= 0) then

                    local curPos = CalculatePosition(spritePickerData.currentSelection, spritePickerData.columns)

                    newPos.x = Clamp(curPos.x + newPos.x, 0, spritePickerData.columns - 1)
                    newPos.y = Clamp(curPos.y + newPos.y, 0, spritePickerData.rows - 1)

                    local newIndex = CalculateIndex(newPos.x, newPos.y, spritePickerData.columns)

                    ChangeSpriteID(newIndex)

                end

            end
            -- if(spritePickerData.dragging) then
            --   spriteIDInputData.text = ""
            -- end
            -- editorUI:Enable(spriteIDInputData, not spritePickerData.dragging)


            editorUI:UpdateInputField(spriteIDInputData)
            -- editorUI:UpdateInputField(colorOffsetInputData)
            -- editorUI:UpdateInputField(colorHexInputData)

            editorUI:UpdateButton(sizeBtnData)
            editorUI:UpdateButton(flipHButton)
            editorUI:UpdateButton(flipVButton)

            pixelVisionOS:UpdateCanvas(canvasData)

            if(canvasData.tool == "eyedropper" and canvasData.inFocus and MouseButton(0)) then

                local colorID = canvasData.overColor

                -- Only update the color selection when it's new
                if(colorID ~= lastColorID) then

                    lastColorID = colorID

                    if(colorID < 0) then

                        pixelVisionOS:ClearItemPickerSelection(paletteColorPickerData)

                        -- Force the lastColorID to be back in range so there is a color to draw with
                        lastColorID = 0

                    else

                        pixelVisionOS:CanvasBrushColor(canvasData, lastColorID)

                        local selectionID = lastColorID

                        -- Check to see if in palette mode
                        if(usePalettes == true) then
                            local pageOffset = ((paletteColorPickerData.pages.currentSelection - 1) * 16)

                            selectionID = Clamp(lastColorID, 0, 15) + pageOffset
                            -- pixelVisionOS:SelectColorPickerColor(paletteColorPickerData, Clamp(lastColorID, 0, 15) + pageOffset)
                        end
                        -- else
                        pixelVisionOS:SelectColorPickerColor(paletteColorPickerData, selectionID)

                        -- end


                        -- Select the


                    end

                end

            end

            editorUI:UpdateToggleGroup(toolBtnData)

            -- System picker
            pixelVisionOS:UpdateColorPicker(paletteColorPickerData)

            pixelVisionOS:UpdateSpritePicker(spritePickerData)

            if(IsExporting()) then
                pixelVisionOS:DisplayMessage("Saving " .. tostring(ReadExportPercent()).. "% complete.", 2)
            end

            if(colorPreviewInvalid == true) then

                DrawColorPerSpriteDisplay()

                ResetColorPreviewValidation()

            end

        end
    end

end

function Draw()

    RedrawDisplay()

    -- The ui should be the last thing to update after your own custom draw calls
    pixelVisionOS:Draw()

end

function Shutdown()

    -- TODO this is a hack since the cancel button for the model is over the canvas and is triggered when closing it

    -- Kill the canvas
    canvasData.onAction = nil

    -- Save the current session ID
    WriteSaveData("sessionID", SessionID())

    WriteSaveData("rootDirectory", rootDirectory)

    WriteSaveData("selectedSprite", spritePickerData.currentSelection)

    WriteSaveData("spriteSize", spriteSize)

    editorUI:Shutdown()

    -- TODO need to add selected tool, color and color page

end

function OnSpriteBuilder()

    -- print("rootDirectory", rootDirectory)
    local count = gameEditor:RunSpriteBuilder(rootDirectory)

    if(count > 0) then
        pixelVisionOS:DisplayMessage(count .. " sprites were processed for the 'sb-sprites.lua' file.")
    else
        pixelVisionOS:DisplayMessage("No sprites were found in the 'SpriteBuilder' folder.")
    end

end

-- function ChangeColorOffset(value)
--
--   -- Change the sprite picker color offset
--   spritePickerData.colorOffset = value + pixelVisionOS.colorOffset
--
--   -- Redraw the page
--   pixelVisionOS:InvalidateItemPickerDisplay(spritePickerData)
--
-- end

function UpdateHistory(pixelData)

    local historyAction = {
        -- sound = settingsString,
        Action = function()

            canvasData.paintCanvas:SetPixels(pixelData)

            OnSaveCanvasChanges()


        end
    }

    pixelVisionOS:AddUndoHistory(historyAction)

    -- We only want to update the buttons in some situations
    -- if(updateButtons ~= false) then
    UpdateHistoryButtons()
    -- end

end

-- local historyPos = 1

function OnUndo()

    local action = pixelVisionOS:Undo()

    if(action ~= nil and action.Action ~= nil) then
        action.Action()
    end

    UpdateHistoryButtons()
end

function OnRedo()

    local action = pixelVisionOS:Redo()

    if(action ~= nil and action.Action ~= nil) then
        action.Action()
    end

    UpdateHistoryButtons()
end

function UpdateHistoryButtons()

    pixelVisionOS:EnableMenuItem(UndoShortcut, pixelVisionOS:IsUndoable())
    pixelVisionOS:EnableMenuItem(RedoShortcut, pixelVisionOS:IsRedoable())

end

function ClearHistory()

    -- Reset history
    pixelVisionOS:ResetUndoHistory()
    UpdateHistoryButtons()

end

function ToggleBackgroundColor(value)


    showBGColor = value

    canvasData.showBGColor = value

    if(usePalettes == true) then

        pixelVisionOS:SelectColorPage(paletteColorPickerData, paletteColorPickerData.picker.selected)

    else
        canvasData.emptyColorID = pixelVisionOS.emptyColorID
    end

    pixelVisionOS:InvalidateCanvas(canvasData)

    -- spritePickerData.showBGColor = value

    -- TODO need a way to replace the mask color in palette mode here

    -- pixelVisionOS:InvalidateItemPickerDisplay(spritePickerData)

    -- DrawRect(viewport.x, viewport.y, viewport.w, viewport.h, pixelVisionOS.emptyColorID, DrawMode.TilemapCache)

end

function OnRunGame()

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
