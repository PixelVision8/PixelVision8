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
LoadScript("pixel-vision-os-canvas-v2")
LoadScript("code-render-map-layer")
LoadScript("pixel-vision-os-tilemap-picker-v1")
LoadScript("pixel-vision-os-file-modal-v1")

local toolName = "Tilemap Tool"

local colorOffset = 0
local systemColorsPerPage = 64
local success = false
local viewport = {x = 8, y = 80, w = 224, h = 128}
local lastBGState = false
local currentTileSelection = nil
local mapSize = NewPoint()
local flagPicker = nil
local flagModeActive = false
local showBGColor = false
local spriteSize = 1
local maxSpriteSize = 4
local lastTileSelection = -1
local enabledUI = {}
local SaveShortcut, UndoShortcut, RedoShortcut, CopyShortcut, PasteShortcut, BGColorShortcut, QuitShortcut = 5, 7, 8, 9, 10, 12, 18
local uiLock = false
local tools = {"pointer", "pen", "eraser", "fill"}
local toolKeys = {Keys.v, Keys.P, Keys.E, Keys.F}

function InvalidateData()

    -- Only everything if it needs to be
    if(invalid == true)then
        return
    end

    pixelVisionOS:ChangeTitle(toolTitle .."*", "toolbariconfile")

    pixelVisionOS:EnableMenuItem(SaveShortcut, true)

    invalid = true

end

function ResetDataValidation()

    -- Only everything if it needs to be
    if(invalid == false)then
        return
    end

    pixelVisionOS:ChangeTitle(toolTitle, "toolbariconfile")

    pixelVisionOS:EnableMenuItem(SaveShortcut, false)

    -- if(tilePickerData ~= nil) then
    tilePickerData.mapInvalid = false
    -- end

    invalid = false

end

function Init()

    BackgroundColor(22)

    -- Disable the back key in this tool
    EnableBackKey(false)

    -- Create an instance of the Pixel Vision OS
    pixelVisionOS = PixelVisionOS:Init()

    -- Reset the undo history so it's ready for the tool
    pixelVisionOS:ResetUndoHistory()

    -- Get a reference to the Editor UI
    editorUI = pixelVisionOS.editorUI

    newFileModal = NewFileModal:Init(editorUI)
    newFileModal.editorUI = editorUI

    -- print(dump(ReadAllMetadata()))

    rootDirectory = ReadMetadata("directory", nil)

    if(rootDirectory ~= nil) then
        -- Load only the game data we really need
        success = gameEditor:Load(rootDirectory, {SaveFlags.System, SaveFlags.Meta, SaveFlags.Colors, SaveFlags.ColorMap, SaveFlags.Sprites, SaveFlags.Tilemap, SaveFlags.TilemapFlags})
    end

    -- Set the tool name with an error message
    pixelVisionOS:ChangeTitle(toolName .. " - Error Loading", "toolbariconfile")

    -- If data loaded activate the tool
    if(success == true) then

        local menuOptions = 
        {
            -- About ID 1
            {name = "About", action = function() pixelVisionOS:ShowAboutModal(toolName) end, toolTip = "Learn about PV8."},
            {divider = true},
            {name = "Edit Sprites", enabled = spriteEditorPath ~= nil, action = OnEditSprites, toolTip = "Open the sprite editor."},
            {name = "Export PNG", action = OnPNGExport, enabled = true, toolTip = "Generate a 'tilemap.png' file."}, -- Reset all the values
            {name = "Save", action = OnSave, enabled = false, key = Keys.S, toolTip = "Save changes made to the tilemap.json file."}, -- Reset all the values
            -- {divider = true},
            -- {name = "Clear", action = OnNewSound, enabled = false, key = Keys.D, toolTip = "Clear the currently selected tile."}, -- Reset all the values
            -- {name = "Revert", action = nil, enabled = false, key = Keys.R, toolTip = "Revert the tilemap.json file to its previous state."}, -- Reset all the values
            {divider = true},
            {name = "Undo", action = OnUndo, enabled = false, key = Keys.Z, toolTip = "Undo the last action."}, -- Reset all the values
            {name = "Redo", action = OnRedo, enabled = false, key = Keys.Y, toolTip = "Redo the last undo."}, -- Reset all the values
            {name = "Copy", action = OnCopyTile, enabled = false, key = Keys.C, toolTip = "Copy the currently selected tile."}, -- Reset all the values
            {name = "Paste", action = OnPasteTile, enabled = false, key = Keys.V, toolTip = "Paste the last copied tile."}, -- Reset all the values
            {divider = true},
            {name = "BG Color", action = function() ToggleBackgroundColor(not showBGColor) end, key = Keys.B, toolTip = "Toggle background color."},
            {name = "Toggle Layer", action = function() ChangeMode(not flagModeActive) end, key = Keys.L, toolTip = "Toggle flag mode for collision."},
            {divider = true},
            {name = "Flip H", action = OnFlipH, enabled = false, key = Keys.H, toolTip = "Flip the current tile horizontally."}, -- Reset all the values
            {name = "Flip V", action = OnFlipV, enabled = false, key = Keys.G, toolTip = "Flip the current tile vertically."}, -- Reset all the values
            {divider = true},
            {name = "Quit", key = Keys.Q, action = OnQuit, toolTip = "Quit the current game."}, -- Quit the current game
        }

        if(PathExists(NewWorkspacePath(rootDirectory).AppendFile("code.lua"))) then
            table.insert(menuOptions, #menuOptions, {name = "Run Game", action = OnRunGame, key = Keys.R, toolTip = "Run the code for this game."})
        end

        pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")


        -- The first thing we need to do is rebuild the tool's color table to include the game's system and game colors.

        pixelVisionOS:ImportColorsFromGame()

        _G["flagpickerover"] = {spriteIDs = spriteselection1x.spriteIDs, width = spriteselection1x.width, colorOffset = 28}

        _G["flagpickerselectedup"] = {spriteIDs = spriteselection1x.spriteIDs, width = spriteselection1x.width, colorOffset = (_G["flagpickerover"].colorOffset + 2)}

        ConfigureSpritePickerSelector(1)

        sizeBtnData = editorUI:CreateButton({x = 160, y = 16}, "sprite1x", "Next sprite sizes (+). Press with Shift for previous (-).")
        sizeBtnData.onAction = function() OnNextSpriteSize() end

        table.insert(enabledUI, sizeBtnData)

        toolBtnData = editorUI:CreateToggleGroup()
        toolBtnData.onAction = OnSelectTool

        for i = 1, #tools do
            local offsetX = ((i - 1) * 16) + 160
            local rect = {x = offsetX, y = 56, w = 16, h = 16}
            editorUI:ToggleGroupButton(toolBtnData, rect, tools[i], "Select the '" .. tools[i] .. "' (".. tostring(toolKeys[i]) .. ") tool.")

            table.insert(enabledUI, toolBtnData.buttons[i])

        end

        flagBtnData = editorUI:CreateToggleButton({x = 232, y = 56}, "flag", "Toggle between tilemap and flag layers (CTRL+L).")

        table.insert(enabledUI, flagBtnData)

        flagBtnData.onAction = ChangeMode

        -- Get sprite texture dimensions
        local totalSprites = gameEditor:TotalSprites()
        -- This is fixed size at 16 cols (128 pixels wide)
        local spriteColumns = 16
        local spriteRows = math.ceil(totalSprites / 16)

        spritePickerData = pixelVisionOS:CreateSpritePicker(
            {x = 8, y = 24, w = 128, h = 32 },
            {x = 8, y = 8},
            spriteColumns,
            spriteRows,
            pixelVisionOS.colorOffset,
            "spritepicker",
            "sprite",
            false,
            "SpritePicker"
        )

        -- table.insert(enabledUI, spritePickerData.picker)
        table.insert(enabledUI, spritePickerData.vSlider)
        table.insert(enabledUI, spritePickerData.picker)

        -- TODO had to disable these buttons because it breaks the arrow keys on the sprite picker

        -- for i = 1, #spritePickerData.pages.buttons do
        --   table.insert(enabledUI, spritePickerData.pages.buttons[i])
        -- end

        -- spritePickerData.scrollScale = 4
        spritePickerData.onPress = OnSelectSprite

        -- Check the game editor if palettes are being used
        usePalettes = pixelVisionOS.paletteMode

        local totalColors = gameEditor:TotalColors(true)--pixelVisionOS.realSystemColorTotal + 1
        local totalPerPage = 16--pixelVisionOS.systemColorsPerPage
        local maxPages = 8
        local colorOffset = pixelVisionOS.colorOffset

        -- Configure tool for palette mode
        if(usePalettes == true) then

            -- Change the total colors when in palette mode
            totalColors = 128
            colorOffset = colorOffset + 128

        end


        -- TODO if using palettes, need to replace this with palette color value

        local pickerRect = {x = 184, y = 24, w = 64, h = 16}

        -- TODO setting the total to 0
        paletteColorPickerData = pixelVisionOS:CreateColorPicker(
            pickerRect,
            {x = 8, y = 8},
            totalColors,
            totalPerPage,
            maxPages,
            colorOffset,
            "spritepicker",
            "Select a color."
        )

        if(usePalettes == true) then
            -- Force the palette picker to only display the total colors per sprite
            paletteColorPickerData.visiblePerPage = pixelVisionOS.paletteColorsPerPage

            paletteButton = editorUI:CreateButton(pickerRect, nil, "Apply color palette")

            paletteButton.onAction = ApplyTilePalette

        end

        -- Wire up the picker to change the color offset of the sprite picker
        paletteColorPickerData.onPageAction = function(value)

            if(usePalettes == true) then

                -- Calculate the new color offset
                local newColorOffset = pixelVisionOS.colorOffset + pixelVisionOS.totalPaletteColors + ((value - 1) * 16)

                -- Update the sprite picker color offset
                -- spritePickerData.colorOffset = newColorOffset
                pixelVisionOS:ChangeItemPickerColorOffset(spritePickerData, newColorOffset)

                tilePickerData.paintColorOffset = ((value - 1) * 16) + 128

                ApplyTilePalette()
            end

        end

        flagPicker = editorUI:CreatePicker(
            pickerRect,
            8,
            8,
            16,
            "flagpicker",
            "Pick a flag"
        )

        table.insert(enabledUI, flagPicker)


        flagPicker.onAction = function(value)
            pixelVisionOS:ChangeTilemapPaintFlag(tilePickerData, value, toolMode ~= 1 and toolMode ~= 3)
        end

        UpdateTitle()

        mapSize = gameEditor:TilemapSize()

        targetSize = NewPoint(math.ceil(mapSize.x / 4) * 4, math.ceil(mapSize.y / 4) * 4)

        if(mapSize.x ~= targetSize.x or mapSize.y ~= targetSize.y) then

            displayResizeWarning = true

        else
            OnInitCompleated()
        end

    else

        DrawRect(8, 24, 128, 32, 0, DrawMode.TilemapCache)
        DrawRect(152, 60, 3, 9, BackgroundColor(), DrawMode.TilemapCache)

        DrawRect(184, 24, 64, 16, 0, DrawMode.TilemapCache)

        DrawRect(248, 44, 3, 9, BackgroundColor(), DrawMode.TilemapCache)

        pixelVisionOS:ChangeTitle(toolName, "toolbaricontool")

        pixelVisionOS:ShowMessageModal(toolName .. " Error", "The tool could not load without a reference to a file to edit.", 160, false,
            function()
                QuitCurrentTool()
            end
        )

    end

end

function UpdateTitle()
    local pathSplit = string.split(rootDirectory, "/")

    -- TODO need to load the correct file here

    local fileName = ReadMetadata("fileName")

    -- Need to make sure we show the right extenstion since json files are loaded before png if one exists even though the tool will see it as a png.
    if(PathExists(NewWorkspacePath(rootDirectory).AppendFile("tilemap.json")) and string.ends(fileName, "png")) then

        -- Rewrite the extension
        fileName = string.split(fileName, ".")[1] .. ".json"

    end

    -- Update title with file path
    toolTitle = pathSplit[#pathSplit] .. "/" .. fileName

end

function ApplyTilePalette()
    if(tilePickerData.mode == 1 and tilePickerData.currentSelection > - 1) then

        -- TODO need to redraw the tile with the new color offset

        local pos = CalculatePosition(tilePickerData.currentSelection, tilePickerData.mapSize.x)
        --
        -- local tileData = gameEditor:Tile(pos.x, pos.y)
        --
        -- -- TODO need to manually loop through the tiles and apply the color offset
        --
        -- pixelVisionOS:ChangeTile(tilePickerData, pos.x, pos.y, tileData.spriteID, tilePickerData.paintColorOffset - 16)

        local total = spriteSize * spriteSize
        local tileHistory = {}

        for i = 1, total do

            local offset = CalculatePosition(i - 1, spriteSize)

            local nextCol = pos.x + offset.x
            local nextRow = pos.y + offset.y

            -- local nextSpriteID = spriteID == -1 and spriteID or CalculateIndex(spritePos.x + offset.x, spritePos.y + offset.y, spriteCols)

            local currentTile = gameEditor:Tile(nextCol, nextRow)

            local savedTile = {
                spriteID = currentTile.spriteID,
                col = nextCol,
                row = nextRow,
                colorOffset = currentTile.colorOffset,
                flag = currentTile.flag
            }

            -- TODO need to save changes to history?
            -- print("Tile History", currentTile.spriteID, nextSpriteID)
            table.insert(tileHistory, savedTile)

            -- local tile = gameEditor:Tile(nextCol, nextRow, nextSpriteID, colorOffset, flag)

            pixelVisionOS:OnChangeTile(data, nextCol, nextRow, currentTile.spriteID, tilePickerData.paintColorOffset, currentTile.flag)

        end

        UpdateHistory(tileHistory)


    end
end

function OnInitCompleated()

    -- Setup map viewport

    local mapWidth = mapSize.x * 8
    local mapHeight = mapSize.y * 8

    -- TODO need to modify the viewport to make sure the map fits inside of it correctly

    -- viewport.w = math.min(mapWidth, viewport.w)
    -- viewport.h = math.min(mapHeight, viewport.h)

    -- TODO need to account for tilemaps that are smaller than the default viewport

    tilePickerData = pixelVisionOS:CreateTilemapPicker(
        {x = viewport.x, y = viewport.y, w = viewport.w, h = viewport.h},
        {x = 8, y = 8},
        mapSize.x,
        mapSize.y,
        pixelVisionOS.colorOffset,
        "spritepicker",
        "tile",
        true,
        "tilemap"
    )
    --
    -- table.insert(enabledUI, tilePickerData.picker)
    -- table.insert(enabledUI, tilePickerData.hSlider)
    -- table.insert(enabledUI, tilePickerData.vSlider)

    tilePickerData.onRelease = OnTileSelection
    tilePickerData.onDropTarget = OnTilePickerDrop


    -- Add custom tool tip logic
    tilePickerData.UpdateToolTip = function(tmpData)

        if(tmpData.dragging) then

            if(tmpData.overPos.index ~= nil and tmpData.overPos.index ~= -1 and tmpData.overPos.index < tmpData.picker.total) then

                local tmpPosA = CalculatePosition( tmpData.overPos.index, tmpData.columns)

                local tmpColA = string.lpad(tostring(tmpPosA.x), #tostring(tmpData.columns), "0")
                local tmpRowA = string.lpad(tostring(tmpPosA.y), #tostring(tmpData.rows), "0")

                local tmpPosB = CalculatePosition( tmpData.pressSelection.index, tmpData.columns)

                local tmpColB = string.lpad(tostring(tmpPosB.x), #tostring(tmpData.columns), "0")
                local tmpRowB = string.lpad(tostring(tmpPosB.y), #tostring(tmpData.rows), "0")

                tmpData.picker.toolTip = "Swap "..tmpData.toolTipLabel.." " .. tmpColB .. "," .. tmpRowB .." (ID " .. string.lpad(tostring(tmpData.pressSelection.index), tmpData.totalItemStringPadding, "0")..")" .. " with "..tmpColA .. "," .. tmpRowA .." (ID " .. string.lpad(tostring(tmpData.overPos.index), tmpData.totalItemStringPadding, "0")..")"
            else

                local tmpPos = CalculatePosition( tmpData.pressSelection.index, tmpData.columns)

                local tmpCol = string.lpad(tostring(tmpPos.x), #tostring(tmpData.columns), "0")
                local tmpRow = string.lpad(tostring(tmpPos.y), #tostring(tmpData.rows), "0")

                tmpData.picker.toolTip = "Dragging "..tmpData.toolTipLabel.." " .. tmpCol .. "," .. tmpRow .." (ID " .. string.lpad(tostring(tmpData.pressSelection.index), tmpData.totalItemStringPadding, "0")..")"

            end

        elseif(tmpData.overPos.index ~= nil and tmpData.overPos.index ~= -1) then

            local tmpPos = CalculatePosition( tmpData.overPos.index, tmpData.columns)

            local tmpCol = string.lpad(tostring(tmpPos.x), #tostring(tmpData.columns), "0")
            local tmpRow = string.lpad(tostring(tmpPos.y), #tostring(tmpData.rows), "0")

            -- print("Tool Mode", toolMode)

            local action = "Select"

            if(toolMode == 2) then
                action = "Draw"
            elseif(toolMode == 3) then
                action = "Erase"
            elseif(toolMode == 4) then
                action = "Fill"
            end

            -- Update the tooltip with the index and position
            tmpData.picker.toolTip = action .. " "..tmpData.toolTipLabel.." " .. tmpCol .. "," .. tmpRow .." (ID " .. string.lpad(tostring(tmpData.overPos.index), tmpData.totalItemStringPadding, "0")..")"

        else
            tmpData.picker.toolTip = ""
        end

    end

    -- Need to convert sprites per page to editor's sprites per page value
    -- local spritePages = math.floor(gameEditor:TotalSprites() / 192)

    if(gameEditor:Name() == ReadSaveData("editing", "undefined")) then
        lastSystemColorSelection = tonumber(ReadSaveData("systemColorSelection", "0"))
        -- lastTab = tonumber(ReadSaveData("tab", "1"))
        -- lastSelection = tonumber(ReadSaveData("selected", "0"))
    end

    pixelVisionOS:SelectColorPage(paletteColorPickerData, 1)

    pixelVisionOS:SelectSpritePickerIndex(spritePickerData, 0)

    editorUI:SelectToggleButton(toolBtnData, 1)

    SelectLayer(1)

    ChangeSpriteID(0)

    ResetDataValidation()

end

function OnTileSelection(value)

    -- When in palette mode, change the palette page
    if(pixelVisionOS.paletteMode == true) then

        local pos = CalculatePosition(value, tilePickerData.tiles.w)

        local tileData = gameEditor:Tile(pos.x, pos.y)

        local colorPage = (tileData.colorOffset - 128) / 16

        -- print("Color Page", value, colorPage, tileData.colorOffset)

    end


end


function OnTilePickerDrop(src, dest)

    if(dest.inDragArea == false) then
        return
    end

    -- If the src and the dest are the same, we want to swap colors
    if(src.name == dest.name) then

        local srcPos = src.pressSelection

        -- Get the source color ID
        local srcTile = gameEditor:Tile(srcPos.x, srcPos.y)

        local srcIndex = srcTile.index
        local srcSpriteID = srcTile.spriteID

        local destPos = pixelVisionOS:CalculateItemPickerPosition(src)

        -- Get the destination color ID
        local destTile = gameEditor:Tile(destPos.x, destPos.y)

        local destIndex = destTile.index
        local destSpriteID = destTile.spriteID

        -- ReplaceTile(destIndex, srcSpriteID)
        --
        -- ReplaceTile(srcIndex, destSpriteID)

        pixelVisionOS:SwapTiles(tilePickerData, srcTile, destTile)

    end
end

function ToggleBackgroundColor(value)

    showBGColor = value

    tilePickerData.showBGColor = value

    pixelVisionOS:InvalidateItemPickerDisplay(tilePickerData)

    -- DrawRect(viewport.x, viewport.y, viewport.w, viewport.h, pixelVisionOS.emptyColorID, DrawMode.TilemapCache)

end

function ConfigureSpritePickerSelector(size)

    if(size < 1) then
        return
    end

    _G["spritepickerover"] = {spriteIDs = _G["spriteselection"..tostring(size) .."x"].spriteIDs, width = _G["spriteselection"..tostring(size) .."x"].width, colorOffset = 28}

    _G["spritepickerselectedup"] = {spriteIDs = _G["spriteselection"..tostring(size) .."x"].spriteIDs, width = _G["spriteselection"..tostring(size) .."x"].width, colorOffset = (_G["spritepickerover"].colorOffset + 2)}

    -- pixelVisionOS:ChangeSpritePickerSize(spritePickerData, size)
    pixelVisionOS:ChangeItemPickerScale(spritePickerData, size)

    if(tilePickerData ~= nil) then
        pixelVisionOS:ChangeItemPickerScale(tilePickerData, size)
    end

end



function ChangeMode(value)

    -- If value is true select layer 2, if not select layer 1
    SelectLayer(value == true and 2 or 1)

    -- Set the flag mode to the value
    flagModeActive = value

    -- If value is true we are in the flag mode
    if(value == true) then
        lastBGState = tilePickerData.showBGColor

        tilePickerData.showBGColor = false

        -- Disable bg menu option
        pixelVisionOS:EnableMenuItem(BGColorShortcut, false)

        pixelVisionOS:InvalidateItemPickerDisplay(tilePickerData)

        DrawFlagPage()

    else
        -- Swicth back to tile modes

        -- Restore background color state
        tilePickerData.showBGColor = lastBGState

        -- editorUI:Ena ble(bgBtnData, true)
        pixelVisionOS:EnableMenuItem(BGColorShortcut, true)


        pixelVisionOS:RebuildPickerPages(paletteColorPickerData)
        pixelVisionOS:SelectColorPage(paletteColorPickerData, 1)
        pixelVisionOS:InvalidateItemPickerDisplay(paletteColorPickerData)


    end

    -- Fix the button state if triggered outside of the button
    if(flagBtnData.selected ~= value) then

        editorUI:ToggleButton(flagBtnData, value, false)

    end

    -- Clear history between layers
    ClearHistory()

end

function DrawFlagPage()

    local startX = 176 + 8
    local startY = 24

    local columns = 8

    local total = 16

    for i = 1, total do

        local pos = CalculatePosition(i - 1, columns)

        local spriteData = _G["flag".. i .. "small"]
        -- print("Flag Sprite", spriteData ~= nil)
        if(spriteData ~= nil) then

            DrawSprites(
                spriteData.spriteIDs,
                (pos.x * 8) + startX,
                (pos.y * 8) + startY,
                spriteData.width,
                false,
                false,
                DrawMode.TilemapCache
            )

        end

    end

    local pageSprites = {
        _G["pagebuttonempty"],
        _G["pagebuttonempty"],
        _G["pagebuttonempty"],
        _G["pagebuttonempty"],
        _G["pagebuttonempty"],
        _G["pagebuttonempty"],
        _G["pagebuttonempty"],
        _G["pagebutton1selectedup"],
    }

    startX = 184
    startY = 40

    for i = 1, #pageSprites do
        local spriteData = pageSprites[i]
        DrawSprites(spriteData.spriteIDs, startX + ((i - 1) * 8), startY, spriteData.width, false, false, DrawMode.TilemapCache)
    end


end


function SelectLayer(value)

    layerMode = value - 1

    -- Clear the color and sprite pickers
    pixelVisionOS:ClearItemPickerSelection(spritePickerData)
    pixelVisionOS:ClearItemPickerSelection(paletteColorPickerData)

    -- Check to see if we are in tilemap mode
    if(layerMode == 0) then

        -- Disable selecting the color picker
        pixelVisionOS:EnableColorPicker(paletteColorPickerData, false, true)
        pixelVisionOS:EnableItemPicker(spritePickerData, true, true)

        pixelVisionOS:ClearItemPickerSelection(spritePickerData)

        pixelVisionOS:ChangeItemPickerColorOffset(tilePickerData, pixelVisionOS.colorOffset)

        if(spritePickerData.currentSelection == -1) then
            ChangeSpriteID(tilePickerData.paintTileIndex)
        end

        -- Test to see if we are in flag mode
    elseif(layerMode == 1) then

        -- Disable selecting the color picker
        pixelVisionOS:EnableColorPicker(paletteColorPickerData, true, true)
        pixelVisionOS:EnableItemPicker(spritePickerData, false, true)
        pixelVisionOS:ChangeItemPickerColorOffset(tilePickerData, 0)

        -- If the flag has been cleared, make sure we select one
        -- if(tilePickerData.paintFlagIndex == -1) then

        editorUI:SelectPicker(flagPicker, tilePickerData.paintFlagIndex)

        -- end

    end

    -- Clear background
    -- DrawRect(viewport.x, viewport.y, viewport.w, viewport.h, pixelVisionOS.emptyColorID, DrawMode.TilemapCache)



    gameEditor:RenderMapLayer(layerMode)

    uiLock = true

    pixelVisionOS:PreRenderMapLayer(tilePickerData, layerMode)

    editorUI.mouseCursor:SetCursor(5, true)

    for i = 1, #enabledUI do
        editorUI:Enable(enabledUI[i], false)
    end

    pixelVisionOS:EnableMenuItem(QuitShortcut, false)

    -- editorUI:Enable(pixelVisionOS.titleBar.iconButton, false)


end

function OnSelectTool(value)

    toolMode = value

    -- Clear the last draw id when switching modes
    lastDrawTileID = -1

    lastSpriteSize = spriteSize

    local lastID = spritePickerData.currentSelection

    if(toolMode == 1) then

        -- Clear the sprite picker and tilemap picker
        pixelVisionOS:ClearItemPickerSelection(tilePickerData)

    elseif(toolMode == 2 or toolMode == 3) then

        -- Clear any tilemap picker selection
        pixelVisionOS:ClearItemPickerSelection(tilePickerData)

    end

    pixelVisionOS:ChangeTilemapPickerMode(tilePickerData, toolMode)


end

function OnNextSpriteSize(reverse)

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

    ChangeSpriteID(spritePickerData.currentSelection)

    -- Reset the flag preview
    pixelVisionOS:ChangeTilemapPaintFlag(tilePickerData, tilePickerData.paintFlagIndex)

    editorUI:Invalidate(sizeBtnData)

end

function ChangeSpriteID(value)

    -- Need to convert the text into a number
    value = tonumber(value)

    pixelVisionOS:SelectSpritePickerIndex(spritePickerData, value, false)

    if(tilePickerData ~= nil) then

        pixelVisionOS:ChangeTilemapPaintSpriteID(tilePickerData, spritePickerData.currentSelection, toolMode ~= 1 and toolMode ~= 3)
    end

end

function OnSave()

    -- TODO need to save all of the colors back to the game

    -- This will save the system data, the colors and color-map
    gameEditor:Save(rootDirectory, {SaveFlags.System, SaveFlags.Colors, SaveFlags.Tilemap})-- SaveFlags.ColorMap, SaveFlags.FlagColors})

    -- Display a message that everything was saved
    pixelVisionOS:DisplayMessage("Your changes have been saved.", 5)

    -- Need to fix the extension if we switched from a png to a json file
    if(string.ends(toolTitle, "png")) then

        -- Rewrite the extension
        toolTitle = string.split(toolTitle, ".")[1] .. ".json"

    end

    -- Clear the validation
    ResetDataValidation()

end

function OnSelectSprite(value)

    pixelVisionOS:ChangeTilemapPaintSpriteID(tilePickerData, spritePickerData.pressSelection.index)

end

function ReplaceTile(index, value, oldValue)

    local pos = CalculatePosition(index, mapSize.x)

    local tile = gameEditor:Tile(pos.x, pos.y)

    oldValue = oldValue or tile.spriteID

    if(tile.spriteID == oldValue) then

        pixelVisionOS:ChangeTile(tilePickerData, pos.x, pos.y, value, spritePickerData.colorOffset - 256)

    end

end


local lastDrawTileID = -1

function Update(timeDelta)

    -- Convert timeDelta to a float
    timeDelta = timeDelta / 1000

    -- This needs to be the first call to make sure all of the editor UI is updated first
    pixelVisionOS:Update(timeDelta)

    editorUI:UpdateButton(paletteButton)

    -- Only update the tool's UI when the modal isn't active
    if(pixelVisionOS:IsModalActive() == false) then

        if(success == true) then

            if(Key(Keys.LeftControl) == false and Key(Keys.RightControl) == false) then
                for i = 1, #toolKeys do
                    if(Key(toolKeys[i], InputState.Released)) then
                        editorUI:SelectToggleButton(toolBtnData, i)
                        break
                    end
                end
            end

            pixelVisionOS:UpdateSpritePicker(spritePickerData)

            editorUI:UpdateButton(sizeBtnData)
            editorUI:UpdateButton(flagBtnData)
            editorUI:UpdateToggleGroup(toolBtnData)

            if(layerMode == 0) then

                pixelVisionOS:UpdateColorPicker(paletteColorPickerData)
            elseif(layerMode == 1) then
                editorUI:UpdatePicker(flagPicker)

                local over = editorUI:CalculatePickerPosition(flagPicker)

                flagPicker.toolTip = "Select flag " .. CalculateIndex(over.x, over.y, flagPicker.columns) .."."

            end

            if(IsExporting()) then
                pixelVisionOS:DisplayMessage("Saving " .. tostring(ReadExportPercent()).. "% complete.", 2)
            end

            -- gameEditor:ScrollPosition(scrollPos.x, scrollPos.y)

            if(displayResizeWarning == true) then

                showResize = true
            else

                -- TODO need to find the right picker to change the selction on


                local targetPicker = spritePickerData.picker.enabled == true and spritePickerData or flagPicker

                -- Change the scale
                if(Key(Keys.OemMinus, InputState.Released) and spriteSize > 1) then
                    OnNextSpriteSize(true)
                elseif(Key(Keys.OemPlus, InputState.Released) and spriteSize < 4) then
                    OnNextSpriteSize()
                end

                -- Create a new piont to see if we need to change the sprite position
                local newPos = NewPoint(0, 0)

                -- Get the sacle from the sprite picker
                local scale = spritePickerData.picker.enabled == true and spritePickerData.scale or 1

                local currentSelection = spritePickerData.picker.enabled == true and spritePickerData.currentSelection or flagPicker.selected

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

                    local curPos = CalculatePosition(currentSelection, targetPicker.columns)

                    newPos.x = Clamp(curPos.x + newPos.x, 0, targetPicker.columns - 1)
                    newPos.y = Clamp(curPos.y + newPos.y, 0, targetPicker.rows - 1)

                    local newIndex = CalculateIndex(newPos.x, newPos.y, targetPicker.columns)

                    if(spritePickerData.picker.enabled == true) then
                        ChangeSpriteID(newIndex)
                    else
                        editorUI:SelectPicker(flagPicker, newIndex)
                        -- print("Select flag", newIndex)
                    end

                end

                pixelVisionOS:UpdateTilemapPicker(tilePickerData)

                if(tilePickerData.mapInvalid and invalid == false) then
                    InvalidateData()
                end

                if(tilePickerData.renderingMap == true) then
                    pixelVisionOS:NextRenderStep(tilePickerData)

                    local percent = pixelVisionOS:ReadRenderPercent(tilePickerData)

                    pixelVisionOS:DisplayMessage("Rendering Layer " .. tostring(percent).. "% complete.", 2)


                    if(tilePickerData.vSlider.inFocus or tilePickerData.hSlider.inFocus) then
                        editorUI.mouseCursor:SetCursor(2, false)
                    elseif(editorUI.mouseCursor.cursorID ~= 5) then
                        editorUI.mouseCursor:SetCursor(5, true)
                    end

                elseif(uiLock == true or editorUI.mouseCursor.cursorID == 5) then
                    pixelVisionOS:EnableMenuItem(QuitShortcut, true)
                    editorUI.mouseCursor:SetCursor(1, false)
                    for i = 1, #enabledUI do
                        editorUI:Enable(enabledUI[i], true)
                    end

                    pixelVisionOS:EnableItemPicker(spritePickerData, not flagModeActive)

                    uiLock = false

                end

            end


        end
    end

end



function Draw()

    RedrawDisplay()

    -- pixelVisionOS:DrawTilemapPicker(tilePickerData, viewport, layerMode, showBGColor)



    -- The ui should be the last thing to update after your own custom draw calls
    pixelVisionOS:Draw()

    if(showResize == true) then

        showResize = false
        --
        pixelVisionOS:ShowMessageModal(toolName .. " Warning", "The tilemap will be resized from ".. mapSize.x .."x" .. mapSize.y .." to ".. targetSize.x .. "x" .. targetSize.y .. " in order for it to work in this editor. When you save the new map size will be applied to the game's data file.", 160, true,
            function(value)
                if(pixelVisionOS.messageModal.selectionValue == true) then

                    mapSize = targetSize

                    gameEditor:TilemapSize(targetSize.x, targetSize.y)
                    OnInitCompleated()

                    InvalidateData()

                    displayResizeWarning = false
                    showResize = false
                else

                    QuitCurrentTool()
                end
            end
        )
    end

end

function OnQuit()

    if(tilePickerData.renderingMap == true) then
        return
    end

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

function Shutdown()

    editorUI:Enable(tilePickerData, false)

    editorUI:Shutdown()

    -- WriteSaveData("editing", gameEditor:Name())
    -- WriteSaveData("tab", tostring(colorTabBtnData.currentSelection))
    -- WriteSaveData("selected", CalculateRealIndex(systemColorPickerData.picker.selected))

end

function UpdateHistory(tiles)

    if(#tiles == 0) then
        return
    end

    local historyAction = {
        -- sound = settingsString,
        Action = function()

            local total = #tiles

            for i = 1, total do

                local tile = tiles[i]

                pixelVisionOS:OnChangeTile(tilePickerData, tile.col, tile.row, tile.spriteID, tile.colorOffset, tile.flag)

            end

        end
    }

    pixelVisionOS:AddUndoHistory(historyAction)

    -- We only want to update the buttons in some situations
    -- if(updateButtons ~= false) then
    UpdateHistoryButtons()
    -- end

end

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

function OnPNGExport()


    local tmpFilePath = UniqueFilePath(NewWorkspacePath(rootDirectory .. "tilemap-export.png"))

    newFileModal:SetText("Export Tilemap As PNG ", string.split(tmpFilePath.EntityName, ".")[1], "Name file", true)

    pixelVisionOS:OpenModal(newFileModal,
        function()

            if(newFileModal.selectionValue == false) then
                return
            end

            local filePath = tmpFilePath.ParentPath.AppendFile( newFileModal.inputField.text .. ".png")

            SaveImage(filePath, pixelVisionOS:GenerateImage(tilePickerData))

        end
    )

    --

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

function OnCopyTile()

end

function OnPasteTile()

end
