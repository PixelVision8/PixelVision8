
--[[
	Pixel Vision 8 - Display Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
--]]--
--
-- API Bridge

-- Create table to store the workspace tool logic
TilemapTool = {}
TilemapTool.__index = TilemapTool

LoadScript("pixel-vision-os-color-picker-v4")
LoadScript("pixel-vision-os-item-picker-v3")
LoadScript("pixel-vision-os-sprite-picker-v4")
--LoadScript("code-render-map-layer")
LoadScript("pixel-vision-os-tilemap-picker-v2")
LoadScript("pixel-vision-os-file-modal-v1")
LoadScript("pixel-vision-os-progress-modal-v2")

-- Tool components
LoadScript("code-toolbar")
LoadScript("code-palette-panel")
LoadScript("code-sprite-panel")
LoadScript("code-drop-down-menu")
LoadScript("code-tilemap-builder")
LoadScript("code-process-tiles")
LoadScript("code-export-tilemap-modal")

--local tools = {"pointer", "pen", "eraser", "fill"}
--local toolKeys = {Keys.v, Keys.P, Keys.E, Keys.F}

--local selectionSizes = {
--    {x = 1, y = 1, scale = 16},
--    {x = 1, y = 2, scale = 8},
--    {x = 2, y = 1, scale = 8},
--    {x = 2, y = 2, scale = 8},
--    -- {x = 3, y = 3, scale = 4},
--    {x = 4, y = 4, scale = 4}
--}
--
--local maxSpriteSize = #selectionSizes

function TilemapTool:InvalidateData()

    -- Only everything if it needs to be
    if(self.invalid == true)then
        return
    end

    pixelVisionOS:ChangeTitle(toolTitle .."*", "toolbariconfile")

    pixelVisionOS:EnableMenuItem(SaveShortcut, true)

    self.invalid = true

end

function TilemapTool:ResetDataValidation()

    -- Only everything if it needs to be
    if(self.invalid == false)then
        return
    end

    pixelVisionOS:ChangeTitle(toolTitle, "toolbariconfile")

    pixelVisionOS:EnableMenuItem(SaveShortcut, false)

    -- if(self.tilePickerData ~= nil) then
    self.tilePickerData.mapInvalid = false
    -- end

    self.invalid = false

end

function TilemapTool:Init()

    -- Create a new table for the instance with default properties
    local _tilemapTool = {
        toolName = "Tilemap Tool",
        runnerName = SystemName(),
        rootPath = ReadMetadata("RootPath", "/"),
        rootDirectory = ReadMetadata("directory", nil),
        invalid = true,
        success = false,
        colorOffset = 0,
        lastMode = nil,
        showBGColor = false,
        panelInFocus = nil,
        viewport = {x = 8, y = 80, w = 224, h = 128},
        lastBGState = false,
        uiLock = false,
        enabledUI = {},
        mode = nil,
        spriteSize = 1
    }

    -- Create a global reference of the new workspace tool
    setmetatable(_tilemapTool, TilemapTool)

    -- Create a new file model so it is ready
    newFileModal = NewFileModal:Init(editorUI)
    newFileModal.editorUI = editorUI

    --self.rootDirectory = ReadMetadata("directory", nil)

    if(_tilemapTool.rootDirectory ~= nil) then
        -- Load only the game data we really need
        _tilemapTool.success = gameEditor:Load(_tilemapTool.rootDirectory, {SaveFlags.System, SaveFlags.Meta, SaveFlags.Colors, SaveFlags.ColorMap, SaveFlags.Sprites, SaveFlags.Tilemap})
    end

    -- Set the tool name with an error message
    pixelVisionOS:ChangeTitle(_tilemapTool.toolName .. " - Error Loading", "toolbariconfile")

    -- If data loaded activate the tool
    if(_tilemapTool.success == true) then

        _tilemapTool:OnLoad()

    else

        _tilemapTool:OnError()

    end

    -- Return the draw tool data
    return _tilemapTool

end

function TilemapTool:OnError()
    DrawRect(8, 24, 128, 32, 0, DrawMode.TilemapCache)
    DrawRect(152, 60, 3, 9, BackgroundColor(), DrawMode.TilemapCache)

    DrawRect(184, 24, 64, 16, 0, DrawMode.TilemapCache)

    DrawRect(248, 44, 3, 9, BackgroundColor(), DrawMode.TilemapCache)

    pixelVisionOS:ChangeTitle(self.toolName, "toolbaricontool")

    pixelVisionOS:ShowMessageModal(self.toolName .. " Error", "The tool could not load without a reference to a file to edit.", 160, false,
            function()
                QuitCurrentTool()
            end
    )
end

function TilemapTool:OnLoad()
    
    self:CreateDropDownMenu()

    --local menuOptions =
    --{
    --    -- About ID 1
    --    {name = "About", action = function() pixelVisionOS:ShowAboutModal(self.toolName) end, toolTip = "Learn about PV8."},
    --    {divider = true},
    --    {name = "Edit Sprites", enabled = spriteEditorPath ~= nil, action = function() self:OnEditSprites() end, toolTip = "Open the sprite editor."},
    --    {name = "Export PNG", action = function() self:OnPNGExport() end, enabled = true, toolTip = "Generate a 'tilemap.png' file."}, -- Reset all the values
    --    {name = "Save", action = function() self:OnSave() end, enabled = false, key = Keys.S, toolTip = "Save changes made to the tilemap.json file."}, -- Reset all the values
    --    -- {divider = true},
    --    -- {name = "Clear", action = OnNewSound, enabled = false, key = Keys.D, toolTip = "Clear the currently selected tile."}, -- Reset all the values
    --    -- {name = "Revert", action = nil, enabled = false, key = Keys.R, toolTip = "Revert the tilemap.json file to its previous state."}, -- Reset all the values
    --    {divider = true},
    --    {name = "Undo", action = function() self:OnUndo() end, enabled = false, key = Keys.Z, toolTip = "Undo the last action."}, -- Reset all the values
    --    {name = "Redo", action = function() self:OnRedo() end, enabled = false, key = Keys.Y, toolTip = "Redo the last undo."}, -- Reset all the values
    --    {name = "Copy", action = function() self:OnCopyTile() end, enabled = false, key = Keys.C, toolTip = "Copy the currently selected tile."}, -- Reset all the values
    --    {name = "Paste", action = function() self:OnPasteTile() end, enabled = false, key = Keys.V, toolTip = "Paste the last copied tile."}, -- Reset all the values
    --    {divider = true},
    --    {name = "BG Color", action = function() self:ToggleBackgroundColor(not self.showBGColor) end, key = Keys.B, toolTip = "Toggle background color."},
    --    {name = "Toggle Layer", action = function() self:ChangeMode(not self.flagModeActive) end, key = Keys.L, toolTip = "Toggle flag mode for collision."},
    --    {divider = true},
    --    {name = "Flip H", action = function() self:OnFlipH() end, enabled = false, key = Keys.H, toolTip = "Flip the current tile horizontally."}, -- Reset all the values
    --    {name = "Flip V", action = function() self:OnFlipV() end, enabled = false, key = Keys.G, toolTip = "Flip the current tile vertically."}, -- Reset all the values
    --    {divider = true},
    --    {name = "Quit", key = Keys.Q, action = function() self:OnQuit() end, toolTip = "Quit the current game."}, -- Quit the current game
    --}
    --
    --if(PathExists(NewWorkspacePath(self.rootDirectory).AppendFile("code.lua"))) then
    --    table.insert(menuOptions, #menuOptions, {name = "Run Game", action = OnRunGame, key = Keys.R, toolTip = "Run the code for this game."})
    --end
    --
    --pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")


    -- The first thing we need to do is rebuild the tool's color table to include the game's system and game colors.

    pixelVisionOS:ImportColorsFromGame()

    --_G["flagpickerover"] = {spriteIDs = spriteselection1x.spriteIDs, width = spriteselection1x.width, colorOffset = 28}
    --
    --_G["flagpickerselectedup"] = {spriteIDs = spriteselection1x.spriteIDs, width = spriteselection1x.width, colorOffset = (_G["flagpickerover"].colorOffset + 2)}

    --self:ConfigureSpritePickerSelector(1)
    
    self:CreateToolbar()

    self:CreateSpritePanel()

    self:CreatePalettePanel()
    --
    --self.sizeBtnData = editorUI:CreateButton({x = 160, y = 16}, "sprite1x", "Next sprite sizes (+). Press with Shift for previous (-).")
    --self.sizeBtnData.onAction = function() self:OnNextSpriteSize() end
    --
    --table.insert(self.enabledUI, self.sizeBtnData)
    --
    --self.toolBtnData = editorUI:CreateToggleGroup()
    --self.toolBtnData.onAction = function(value) self:OnSelectTool(value) end
    --
    --for i = 1, #tools do
    --    local offsetX = ((i - 1) * 16) + 160
    --    local rect = {x = offsetX, y = 56, w = 16, h = 16}
    --    editorUI:ToggleGroupButton(self.toolBtnData, rect, tools[i], "Select the '" .. tools[i] .. "' (".. tostring(toolKeys[i]) .. ") tool.")
    --
    --    table.insert(self.enabledUI, self.toolBtnData.buttons[i])
    --
    --end
    --
    --self.flagBtnData = editorUI:CreateToggleButton({x = 232, y = 56}, "flag", "Toggle between tilemap and flag layers (CTRL+L).")
    --
    --table.insert(self.enabledUI, self.flagBtnData)
    --
    --self.flagBtnData.onAction = function(value) self:ChangeMode(value) end

    ---- Get sprite texture dimensions
    --local totalSprites = gameEditor:TotalSprites()
    ---- This is fixed size at 16 cols (128 pixels wide)
    --local spriteColumns = 16
    --local spriteRows = math.ceil(totalSprites / 16)
    --
    --self.spritePickerData = pixelVisionOS:CreateSpritePicker(
    --        {x = 8, y = 24, w = 128, h = 32 },
    --        {x = 8, y = 8},
    --        spriteColumns,
    --        spriteRows,
    --        pixelVisionOS.colorOffset,
    --        "spritepicker",
    --        "sprite",
    --        false,
    --        "SpritePicker"
    --)
    --
    --self.spritePickerData.picker.borderOffset = 8
    --
    ---- table.insert(self.enabledUI, self.spritePickerData.picker)
    --table.insert(self.enabledUI, self.spritePickerData.vSlider)
    --table.insert(self.enabledUI, self.spritePickerData.picker)
    --
    --self.spritePickerData.onPress = function(value) self:OnSelectSprite(value) end

    --local pickerRect = {x = 184, y = 24, w = 64, h = 16}
    --
    ---- TODO setting the total to 0
    --self.paletteColorPickerData = pixelVisionOS:CreateColorPicker(
    --        pickerRect,
    --        {x = 8, y = 8},
    --        pixelVisionOS.totalPaletteColors,
    --        16, -- Total per page
    --        8, -- Max pages
    --        pixelVisionOS.colorOffset + 128,
    --        "itempicker",
    --        "palette color",
    --        false,
    --        true,
    --        false
    --)
    --
    --self.paletteColorPickerData.onDrawColor = function(data, id, x, y)
    --
    --    if(id < data.total and (id % data.totalPerPage) < data.visiblePerPage) then
    --        local colorID = id + data.altColorOffset
    --
    --        if(Color(colorID) == pixelVisionOS.maskColor) then
    --            data.canvas.DrawSprites(emptymaskcolor.spriteIDs, x, y, emptymaskcolor.width, false, false)
    --        else
    --            data.canvas.Clear(colorID, x, y, data.itemSize.x, data.itemSize.y)
    --        end
    --
    --    else
    --        data.canvas.DrawSprites(emptycolor.spriteIDs, x, y, emptycolor.width, false, false)
    --    end
    --
    --end
    --
    --pixelVisionOS:ColorPickerVisiblePerPage(self.paletteColorPickerData, pixelVisionOS.colorsPerSprite)
    --
    --pixelVisionOS:RebuildColorPickerCache(self.paletteColorPickerData)
    --
    --self.paletteColorPickerData.visiblePerPage = pixelVisionOS.paletteColorsPerPage

    --if(usePalettes == true) then
    -- Force the palette picker to only display the total colors per sprite
    --self.paletteColorPickerData.visiblePerPage = pixelVisionOS.paletteColorsPerPage

    --paletteButton = editorUI:CreateButton(pickerRect, nil, "Apply color palette")
    --
    --paletteButton.onAction = function()self:ApplyTilePalette() end

    --end

    ---- Wire up the picker to change the color offset of the sprite picker
    --self.paletteColorPickerData.onPageAction = function(value)
    --
    --    --if(usePalettes == true) then
    --
    --    -- Calculate the new color offset
    --    local newColorOffset = pixelVisionOS.colorOffset + pixelVisionOS.totalPaletteColors + ((value - 1) * 16)
    --
    --    -- Update the sprite picker color offset
    --    -- self.spritePickerData.colorOffset = newColorOffset
    --    pixelVisionOS:ChangeItemPickerColorOffset(self.spritePickerData, newColorOffset)
    --
    --    self.tilePickerData.paintColorOffset = ((value - 1) * 16) + 128
    --
    --    self:ApplyTilePalette()
    --    --end
    --
    --end

    --self.flagPicker = editorUI:CreatePicker(
    --        pickerRect,
    --        8,
    --        8,
    --        16,
    --        "flagpicker",
    --        "Pick a flag"
    --)
    --
    --table.insert(self.enabledUI, self.flagPicker)
    --
    --
    --self.flagPicker.onAction = function(value)
    --    pixelVisionOS:ChangeTilemapPaintFlag(self.tilePickerData, value, toolMode ~= 1 and toolMode ~= 3)
    --end

    self:UpdateTitle()

    self.mapSize = gameEditor:TilemapSize()

    targetSize = NewPoint(math.ceil(self.mapSize.x / 4) * 4, math.ceil(self.mapSize.y / 4) * 4)

    if(self.mapSize.x ~= targetSize.x or self.mapSize.y ~= targetSize.y) then

        displayResizeWarning = true

    else
        self:OnInitCompleated()
    end
    
end

function TilemapTool:UpdateTitle()
    local pathSplit = string.split(self.rootDirectory, "/")

    -- TODO need to load the correct file here

    local fileName = ReadMetadata("fileName")

    -- Need to make sure we show the right extenstion since json files are loaded before png if one exists even though the tool will see it as a png.
    if(PathExists(NewWorkspacePath(self.rootDirectory).AppendFile("tilemap.json")) and string.ends(fileName, "png")) then

        -- Rewrite the extension
        fileName = string.split(fileName, ".")[1] .. ".json"

    end

    -- Update title with file path
    toolTitle = pathSplit[#pathSplit] .. "/" .. fileName

end

function TilemapTool:ApplyTilePalette()
    if(self.tilePickerData.mode == 1 and self.tilePickerData.currentSelection > - 1) then

        -- TODO need to redraw the tile with the new color offset

        local pos = CalculatePosition(self.tilePickerData.currentSelection, self.tilePickerData.self.mapSize.x)
        --
        -- local tileData = gameEditor:Tile(pos.x, pos.y)
        --
        -- -- TODO need to manually loop through the tiles and apply the color offset
        --
        -- pixelVisionOS:ChangeTile(self.tilePickerData, pos.x, pos.y, tileData.spriteID, self.tilePickerData.paintColorOffset - 16)

        local total = self.spriteSize * self.spriteSize
        local tileHistory = {}

        for i = 1, total do

            local offset = CalculatePosition(i - 1, self.spriteSize)

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

            pixelVisionOS:OnChangeTile(data, nextCol, nextRow, currentTile.spriteID, self.tilePickerData.paintColorOffset, currentTile.flag)
            
            self:InvalidateData()
        end

        self:UpdateHistory(tileHistory)


    end
end

function TilemapTool:OnInitCompleated()

    -- Setup map viewport

    local mapWidth = self.mapSize.x * 8
    local mapHeight = self.mapSize.y * 8

    -- TODO need to modify the self.viewport to make sure the map fits inside of it correctly

    -- self.viewport.w = math.min(mapWidth, self.viewport.w)
    -- self.viewport.h = math.min(mapHeight, self.viewport.h)

    -- TODO need to account for tilemaps that are smaller than the default self.viewport

    self.tilePickerData = pixelVisionOS:CreateTilemapPicker(
            {x = self.viewport.x, y = self.viewport.y, w = self.viewport.w, h = self.viewport.h},
            {x = 8, y = 8},
            self.mapSize.x,
            self.mapSize.y,
            pixelVisionOS.colorOffset,
            "spritepicker",
            "tile",
            true,
            "tilemap"
    )

    self.tilePickerData.picker.borderOffset = 8

    --
    -- table.insert(self.enabledUI, self.tilePickerData.picker)
    -- table.insert(self.enabledUI, self.tilePickerData.hSlider)
    -- table.insert(self.enabledUI, self.tilePickerData.vSlider)

    self.tilePickerData.onRelease = function(src, dest) self:OnTileSelection(src, dest) end
    self.tilePickerData.onDropTarget = function(src, dest) self:OnTilePickerDrop(src, dest) end


    -- Add custom tool tip logic
    self.tilePickerData.UpdateToolTip = function(tmpData)

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

    pixelVisionOS:SelectColorPage(self.paletteColorPickerData, 1)

    --pixelVisionOS:SelectSpritePickerIndex(self.spritePickerData, 0)

    editorUI:SelectToggleButton(self.toolBtnData, 1)

    self:SelectLayer(1)

    --self:ChangeSpriteID(0)

    --self:ResetDataValidation()




    --local defaultToolID = 1
    --local defaultMode = TileMode
    --
    self.lastSpriteID = 10
    --self.lastFlagID = 0
    --self.lastPaletteColorID = 0
    --
    if(SessionID() == ReadSaveData("sessionID", "") and self.rootDirectory == ReadSaveData("rootDirectory", "")) then
    --
        print("self.maxSpriteSize", self.maxSpriteSize, ReadSaveData("lastSpriteSize", "1"), "done")
        self.spriteMode = Clamp(tonumber(ReadSaveData("lastSpriteSize", "1")) - 1, 0, self.maxSpriteSize)
        self.lastSpriteID = tonumber(ReadSaveData("lastSpriteID", "0"))
    --    defaultToolID = tonumber(ReadSaveData("lastSelectedToolID", "1"))
    --
    --    self.lastPaletteColorID = tonumber(ReadSaveData("lastPaletteColorID", "0"))
    --
    --    defaultMode = tonumber(ReadSaveData("lastMode", "1"))
    end
    --
    ---- Change the sprite mode
    --self:OnNextSpriteSize()
    --
    ---- Select the start sprite
    self:ChangeSpriteID(self.lastSpriteID)
    
    
    --
    ---- print("SCALE", self.spriteMode)
    --self:ConfigureSpritePickerSelector(self.spriteMode)
    --
    ---- Set default tool
    --editorUI:SelectToggleButton(self.toolBtnData, defaultToolID)
    --
    ---- Set default mode
    ----self:ChangeEditMode(defaultMode)
    --
    self:ResetDataValidation()

end

function TilemapTool:OnTileSelection(value)

    -- When in palette mode, change the palette page
    if(pixelVisionOS.paletteMode == true) then

        local pos = CalculatePosition(value, self.tilePickerData.tiles.w)

        local tileData = gameEditor:Tile(pos.x, pos.y)

        local colorPage = (tileData.colorOffset - 128) / 16

        -- print("Color Page", value, colorPage, tileData.colorOffset)

    end


end


function TilemapTool:OnTilePickerDrop(src, dest)

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

        pixelVisionOS:SwapTiles(self.tilePickerData, srcTile, destTile)

    end
end

--function TilemapTool:ToggleBackgroundColor(value)
--
--    self.showBGColor = value
--
--    self.tilePickerData.showBGColor = value
--
--    pixelVisionOS:InvalidateItemPickerDisplay(self.tilePickerData)
--
--    -- DrawRect(self.viewport.x, self.viewport.y, self.viewport.w, self.viewport.h, pixelVisionOS.emptyColorID, DrawMode.TilemapCache)
--
--end

--function TilemapTool:ConfigureSpritePickerSelector(size)
--
--
--    local selectionSize = selectionSizes[size]
--
--    local x = selectionSize.x
--    local y = selectionSize.y
--
--    local spriteName = "selection"..x.."x" .. y
--
--    _G["spritepickerover"] = {spriteIDs = _G[spriteName .. "over"].spriteIDs, width = _G[spriteName .. "over"].width, colorOffset = 0}
--
--    _G["spritepickerselectedup"] = {spriteIDs = _G[spriteName .. "selected"].spriteIDs, width = _G[spriteName .. "selected"].width, colorOffset = 0}
--
--    pixelVisionOS:ChangeItemPickerScale(self.spritePickerData, size, selectionSize)
--
--
--
--    --if(size < 1) then
--    --    return
--    --end
--    --
--    --_G["spritepickerover"] = {spriteIDs = _G["spriteselection"..tostring(size) .."x"].spriteIDs, width = _G["spriteselection"..tostring(size) .."x"].width, colorOffset = 28}
--    --
--    --_G["spritepickerselectedup"] = {spriteIDs = _G["spriteselection"..tostring(size) .."x"].spriteIDs, width = _G["spriteselection"..tostring(size) .."x"].width, colorOffset = (_G["spritepickerover"].colorOffset + 2)}
--    --
--    ---- pixelVisionOS:ChangeSpritePickerSize(self.spritePickerData, size)
--    --pixelVisionOS:ChangeItemPickerScale(self.spritePickerData, size)
--    --
--    if(self.tilePickerData ~= nil) then
--        pixelVisionOS:ChangeItemPickerScale(self.tilePickerData, size, selectionSize)
--    end
--
--end



--function TilemapTool:ChangeMode(value)
--
--    -- If value is true select layer 2, if not select layer 1
--    self:SelectLayer(value == true and 2 or 1)
--
--    -- Set the flag mode to the value
--    self.flagModeActive = value
--
--    -- If value is true we are in the flag mode
--    if(value == true) then
--        self.lastBGState = self.tilePickerData.showBGColor
--
--        self.tilePickerData.showBGColor = false
--
--        -- Disable bg menu option
--        pixelVisionOS:EnableMenuItem(BGColorShortcut, false)
--
--        pixelVisionOS:InvalidateItemPickerDisplay(self.tilePickerData)
--
--        self:DrawFlagPage()
--
--    else
--        -- Swicth back to tile modes
--
--        -- Restore background color state
--        self.tilePickerData.showBGColor = self.lastBGState
--
--        -- editorUI:Ena ble(bgBtnData, true)
--        pixelVisionOS:EnableMenuItem(BGColorShortcut, true)
--
--
--        pixelVisionOS:RebuildPickerPages(self.paletteColorPickerData)
--        pixelVisionOS:SelectColorPage(self.paletteColorPickerData, 1)
--        pixelVisionOS:InvalidateItemPickerDisplay(self.paletteColorPickerData)
--
--
--    end
--
--    -- Fix the button state if triggered outside of the button
--    if(self.flagBtnData.selected ~= value) then
--
--        editorUI:ToggleButton(self.flagBtnData, value, false)
--
--    end
--
--    -- Clear history between layers
--    self:ClearHistory()
--
--end

--function TilemapTool:DrawFlagPage()
--
--    local startX = 176 + 8
--    local startY = 24
--
--    local columns = 8
--
--    local total = 16
--
--    for i = 1, total do
--
--        local pos = CalculatePosition(i - 1, columns)
--
--        local spriteData = _G["flag".. i .. "small"]
--        -- print("Flag Sprite", spriteData ~= nil)
--        if(spriteData ~= nil) then
--
--            DrawSprites(
--                    spriteData.spriteIDs,
--                    (pos.x * 8) + startX,
--                    (pos.y * 8) + startY,
--                    spriteData.width,
--                    false,
--                    false,
--                    DrawMode.TilemapCache
--            )
--
--        end
--
--    end
--
--    local pageSprites = {
--        _G["pagebuttonempty"],
--        _G["pagebuttonempty"],
--        _G["pagebuttonempty"],
--        _G["pagebuttonempty"],
--        _G["pagebuttonempty"],
--        _G["pagebuttonempty"],
--        _G["pagebuttonempty"],
--        _G["pagebutton1selectedup"],
--    }
--
--    startX = 184
--    startY = 40
--
--    for i = 1, #pageSprites do
--        local spriteData = pageSprites[i]
--        DrawSprites(spriteData.spriteIDs, startX + ((i - 1) * 8), startY, spriteData.width, false, false, DrawMode.TilemapCache)
--    end
--
--
--end


function TilemapTool:SelectLayer(value)

    layerMode = value - 1

    -- Clear the color and sprite pickers
    pixelVisionOS:ClearItemPickerSelection(self.spritePickerData)
    pixelVisionOS:ClearItemPickerSelection(self.paletteColorPickerData)

    -- Check to see if we are in tilemap mode
    if(layerMode == 0) then

        -- Disable selecting the color picker
        pixelVisionOS:EnableColorPicker(self.paletteColorPickerData, false, true)
        pixelVisionOS:EnableItemPicker(self.spritePickerData, true, true)

        pixelVisionOS:ClearItemPickerSelection(self.spritePickerData)

        pixelVisionOS:ChangeItemPickerColorOffset(self.tilePickerData, pixelVisionOS.colorOffset)

        if(self.spritePickerData.currentSelection == -1) then
            self:ChangeSpriteID(self.tilePickerData.paintTileIndex)
        end

        -- Test to see if we are in flag mode
    elseif(layerMode == 1) then

        -- Disable selecting the color picker
        pixelVisionOS:EnableColorPicker(self.paletteColorPickerData, true, true)
        pixelVisionOS:EnableItemPicker(self.spritePickerData, false, true)
        pixelVisionOS:ChangeItemPickerColorOffset(self.tilePickerData, 0)

        -- If the flag has been cleared, make sure we select one
        -- if(self.tilePickerData.paintFlagIndex == -1) then

        editorUI:SelectPicker(self.flagPicker, self.tilePickerData.paintFlagIndex)

        -- end

    end

    -- Clear background
    -- DrawRect(self.viewport.x, self.viewport.y, self.viewport.w, self.viewport.h, pixelVisionOS.emptyColorID, DrawMode.TilemapCache)



    --gameEditor:RenderMapLayer(layerMode)

    self.uiLock = true

    pixelVisionOS:PreRenderMapLayer(self.tilePickerData, layerMode)

    editorUI.mouseCursor:SetCursor(5, true)

    for i = 1, #self.enabledUI do
        editorUI:Enable(self.enabledUI[i], false)
    end

    pixelVisionOS:EnableMenuItem(QuitShortcut, false)

    -- editorUI:Enable(pixelVisionOS.titleBar.iconButton, false)


end

--function TilemapTool:OnSelectTool(value)
--
--    toolMode = value
--
--    -- Clear the last draw id when switching modes
--    lastDrawTileID = -1
--
--    lastSpriteSize = self.spriteSize
--
--    local lastID = self.spritePickerData.currentSelection
--
--    if(toolMode == 1) then
--
--        -- Clear the sprite picker and tilemap picker
--        pixelVisionOS:ClearItemPickerSelection(self.tilePickerData)
--
--    elseif(toolMode == 2 or toolMode == 3) then
--
--        -- Clear any tilemap picker selection
--        pixelVisionOS:ClearItemPickerSelection(self.tilePickerData)
--
--    end
--
--    pixelVisionOS:ChangeTilemapPickerMode(self.tilePickerData, toolMode)
--
--
--end


--function TilemapTool:OnNextSpriteSize(reverse)
--
--    -- Loop backwards through the button sizes
--    if(Key(Keys.LeftShift) or reverse == true) then
--        self.spriteSize = self.spriteSize - 1
--
--        -- Skip 24 x 24 selections
--        if(self.spriteSize == 3) then
--            self.spriteSize = 2
--        end
--
--        if(self.spriteSize < 1) then
--            self.spriteSize = maxSpriteSize
--        end
--
--        -- Loop forward through the button sizes
--    else
--        self.spriteSize = self.spriteSize + 1
--
--        -- Skip 24 x 24 selections
--        if(self.spriteSize == 3) then
--            self.spriteSize = 4
--        end
--
--        if(self.spriteSize > maxSpriteSize) then
--            self.spriteSize = 1
--        end
--    end
--
--    -- Find the next sprite for the button
--    local spriteName = "sprite"..tostring(self.spriteSize).."x"
--
--    -- Change sprite button graphic
--    self.sizeBtnData.cachedSpriteData = {
--        up = _G[spriteName .. "up"],
--        down = _G[spriteName .. "down"] ~= nil and _G[spriteName .. "down"] or _G[spriteName .. "selectedup"],
--        over = _G[spriteName .. "over"],
--        selectedup = _G[spriteName .. "selectedup"],
--        selectedover = _G[spriteName .. "selectedover"],
--        selecteddown = _G[spriteName .. "selecteddown"] ~= nil and _G[spriteName .. "selecteddown"] or _G[spriteName .. "selectedover"],
--        disabled = _G[spriteName .. "disabled"],
--        empty = _G[spriteName .. "empty"] -- used to clear the sprites
--    }
--
--    self:ConfigureSpritePickerSelector(self.spriteSize)
--
--    self:ChangeSpriteID(self.spritePickerData.currentSelection)
--
--    -- Reset the flag preview
--    pixelVisionOS:ChangeTilemapPaintFlag(self.tilePickerData, self.tilePickerData.paintFlagIndex)
--
--    editorUI:Invalidate(self.sizeBtnData)
--
--end

--function TilemapTool:ChangeSpriteID(value)
--
--    -- Need to convert the text into a number
--    value = tonumber(value)
--
--    pixelVisionOS:SelectSpritePickerIndex(self.spritePickerData, value, false)
--
--    if(self.tilePickerData ~= nil) then
--
--        pixelVisionOS:ChangeTilemapPaintSpriteID(self.tilePickerData, self.spritePickerData.currentSelection, toolMode ~= 1 and toolMode ~= 3)
--    end
--
--end

--function TilemapTool:OnSave()
--
--    -- TODO need to save all of the colors back to the game
--
--    -- This will save the system data, the colors and color-map
--    gameEditor:Save(self.rootDirectory, {SaveFlags.System, SaveFlags.Colors, SaveFlags.Tilemap})-- SaveFlags.ColorMap, SaveFlags.FlagColors})
--
--    -- Display a message that everything was saved
--    pixelVisionOS:DisplayMessage("Your changes have been saved.", 5)
--
--    -- Need to fix the extension if we switched from a png to a json file
--    if(string.ends(toolTitle, "png")) then
--
--        -- Rewrite the extension
--        toolTitle = string.split(toolTitle, ".")[1] .. ".json"
--
--    end
--
--    -- Clear the validation
--    self:ResetDataValidation()
--
--end

--function TilemapTool:OnSelectSprite(value)
--
--    pixelVisionOS:ChangeTilemapPaintSpriteID(self.tilePickerData, self.spritePickerData.pressSelection.index)
--
--end

function ReplaceTile(index, value, oldValue)

    local pos = CalculatePosition(index, self.mapSize.x)

    local tile = gameEditor:Tile(pos.x, pos.y)

    oldValue = oldValue or tile.spriteID

    if(tile.spriteID == oldValue) then

        pixelVisionOS:ChangeTile(self.tilePickerData, pos.x, pos.y, value, self.spritePickerData.colorOffset - 256)

        --UpdateHistory(tileHistory)
        
    end

end


local lastDrawTileID = -1

function TilemapTool:Update()

    -- Convert timeDelta to a float
    --timeDelta = timeDelta / 1000

    -- This needs to be the first call to make sure all of the editor UI is updated first
    pixelVisionOS:Update(editorUI.timeDelta)

    --editorUI:UpdateButton(paletteButton)

    -- Only update the tool's UI when the modal isn't active
    if(pixelVisionOS:IsModalActive() == false) then

        if(self.success == true) then

            --if(Key(Keys.LeftControl) == false and Key(Keys.RightControl) == false) then
            --    for i = 1, #toolKeys do
            --        if(Key(toolKeys[i], InputState.Released)) then
            --            editorUI:SelectToggleButton(self.toolBtnData, i)
            --            break
            --        end
            --    end
            --end

            --pixelVisionOS:UpdateSpritePicker(self.spritePickerData)

            --editorUI:UpdateButton(self.sizeBtnData)
            --editorUI:UpdateButton(self.flagBtnData)
            --editorUI:UpdateToggleGroup(self.toolBtnData)

            --if(layerMode == 0) then
            --
            --    pixelVisionOS:UpdateColorPicker(self.paletteColorPickerData)
            --elseif(layerMode == 1) then
            --    editorUI:UpdatePicker(self.flagPicker)
            --
            --    local over = editorUI:CalculatePickerPosition(self.flagPicker)
            --
            --    self.flagPicker.toolTip = "Select flag " .. CalculateIndex(over.x, over.y, self.flagPicker.columns) .."."
            --
            --end

            if(IsExporting()) then
                pixelVisionOS:DisplayMessage("Saving " .. tostring(ReadExportPercent()).. "% complete.", 2)
            end

            -- gameEditor:ScrollPosition(scrollPos.x, scrollPos.y)

            if(displayResizeWarning == true) then

                showResize = true
            else

                -- TODO need to find the right picker to change the selction on


                local targetPicker = self.spritePickerData.picker.enabled == true and self.spritePickerData or self.flagPicker

                -- Change the scale
                if(Key(Keys.OemMinus, InputState.Released) and self.spriteSize > 1) then
                    self:OnNextSpriteSize(true)
                elseif(Key(Keys.OemPlus, InputState.Released) and self.spriteSize < 4) then
                    self:OnNextSpriteSize()
                end

                -- Create a new piont to see if we need to change the sprite position
                local newPos = NewPoint(0, 0)

                -- Get the sacle from the sprite picker
                local scale = self.spritePickerData.picker.enabled == true and self.spritePickerData.scale or 1

                local currentSelection = self.spritePickerData.picker.enabled == true and self.spritePickerData.currentSelection or self.flagPicker.selected

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

                    if(self.spritePickerData.picker.enabled == true) then
                        self:ChangeSpriteID(newIndex)
                    else
                        editorUI:SelectPicker(self.flagPicker, newIndex)
                        -- print("Select flag", newIndex)
                    end

                end

                pixelVisionOS:UpdateTilemapPicker(self.tilePickerData)

                if(self.tilePickerData.mapInvalid and self.invalid == false) then
                    self:InvalidateData()
                end

                if(self.tilePickerData.renderingMap == true) then
                    pixelVisionOS:NextRenderStep(self.tilePickerData)

                    local percent = pixelVisionOS:ReadRenderPercent(self.tilePickerData)

                    pixelVisionOS:DisplayMessage("Rendering Layer " .. tostring(percent).. "% complete.", 2)


                    if(self.tilePickerData.vSlider.inFocus or self.tilePickerData.hSlider.inFocus) then
                        editorUI.mouseCursor:SetCursor(2, false)
                    elseif(editorUI.mouseCursor.cursorID ~= 5) then
                        editorUI.mouseCursor:SetCursor(5, true)
                    end

                elseif(self.uiLock == true or editorUI.mouseCursor.cursorID == 5) then
                    pixelVisionOS:EnableMenuItem(QuitShortcut, true)
                    editorUI.mouseCursor:SetCursor(1, false)
                    for i = 1, #self.enabledUI do
                        editorUI:Enable(self.enabledUI[i], true)
                    end

                    pixelVisionOS:EnableItemPicker(self.spritePickerData, not self.flagModeActive)

                    self.uiLock = false

                end

            end


        end
    end

end

-- TODO need to restore this logic to display the resize messageModal


--function Draw()
--
--    RedrawDisplay()
--
--    -- pixelVisionOS:DrawTilemapPicker(self.tilePickerData, self.viewport, layerMode, self.showBGColor)
--
--
--
--    -- The ui should be the last thing to update after your own custom draw calls
--    pixelVisionOS:Draw()
--
--    if(showResize == true) then
--
--        showResize = false
--        --
--        pixelVisionOS:ShowMessageModal(self.toolName .. " Warning", "The tilemap will be resized from ".. self.mapSize.x .."x" .. self.mapSize.y .." to ".. targetSize.x .. "x" .. targetSize.y .. " in order for it to work in this editor. When you save the new map size will be applied to the game's data file.", 160, true,
--                function(value)
--                    if(pixelVisionOS.messageModal.selectionValue == true) then
--
--                        self.mapSize = targetSize
--
--                        gameEditor:TilemapSize(targetSize.x, targetSize.y)
--                        self:OnInitCompleated()
--
--                        InvalidateData()
--
--                        displayResizeWarning = false
--                        showResize = false
--                    else
--
--                        QuitCurrentTool()
--                    end
--                end
--        )
--    end
--
--end

function TilemapTool:OnQuit()

    if(self.tilePickerData.renderingMap == true) then
        return
    end

    if(self.invalid == true) then

        pixelVisionOS:ShowMessageModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before you quit?", 160, true,
                function()
                    if(pixelVisionOS.messageModal.selectionValue == true) then
                        -- Save changes
                        self:OnSave()

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

function TilemapTool:Shutdown()

    -- TODO this should just kill the tilepicker
    editorUI:Enable(self.tilePickerData, false)

    editorUI:Shutdown()

    -- Save the state of the tool if it was correctly loaded
    if(self.success == true) then
        -- Save the current session ID
        WriteSaveData("sessionID", SessionID())
        WriteSaveData("rootDirectory", self.rootDirectory)

        WriteSaveData("lastSpriteSize", self.spriteSize)
        WriteSaveData("lastSpriteID", self.spritePickerData.currentSelection)
        
        print("Save", self.spritePickerData.currentSelection)
        --print("self.spritePickerData.currentSelection", self.spritePickerData.currentSelection)
        --WriteSaveData("lastSelectedToolID", self.lastSelectedToolID)
        --
        --WriteSaveData("lastMode", self.mode)
        --WriteSaveData("lastPaletteColorID", self.lastPaletteColorID)
    end
    
    -- WriteSaveData("editing", gameEditor:Name())
    -- WriteSaveData("tab", tostring(colorTabBtnData.currentSelection))
    -- WriteSaveData("selected", CalculateRealIndex(systemColorPickerData.picker.selected))

end


--function TilemapTool:OnPNGExport()
--
--
--    local tmpFilePath = UniqueFilePath(NewWorkspacePath(self.rootDirectory .. "tilemap-export.png"))
--
--    newFileModal:SetText("Export Tilemap As PNG ", string.split(tmpFilePath.EntityName, ".")[1], "Name file", true)
--
--    pixelVisionOS:OpenModal(newFileModal,
--            function()
--
--                if(newFileModal.selectionValue == false) then
--                    return
--                end
--
--                local filePath = tmpFilePath.ParentPath.AppendFile( newFileModal.inputField.text .. ".png")
--
--                SaveImage(filePath, pixelVisionOS:GenerateImage(self.tilePickerData))
--
--            end
--    )
--
--    --
--
--end

--function TilemapTool:OnRunGame()
--    -- TODO should this ask to launch the game first?
--
--    if(self.invalid == true) then
--
--        pixelVisionOS:ShowMessageModal("Unsaved Changes", "You have unsaved changes. You will lose those changes if you run the game now?", 160, true,
--                function()
--                    if(pixelVisionOS.messageModal.selectionValue == true) then
--                        LoadGame(NewWorkspacePath(self.rootDirectory))
--                    end
--
--                end
--        )
--
--    else
--
--        LoadGame(NewWorkspacePath(self.rootDirectory))
--
--    end
--
--end

--function TilemapTool:OnCopyTile()
--
--end
--
--function TilemapTool:OnPasteTile()
--
--end


--function TilemapTool:UpdateHistory(tiles)
--
--    -- if(#tiles == 0) then
--    --     return
--    -- end
--
--    -- local historyAction = {
--    --     -- sound = settingsString,
--    --     Action = function()
--
--    --         local total = #tiles
--
--    --         for i = 1, total do
--
--    --             local tile = tiles[i]
--
--    --             pixelVisionOS:OnChangeTile(self.tilePickerData, tile.col, tile.row, tile.spriteID, tile.colorOffset, tile.flag)
--
--    --         end
--
--    --     end
--    -- }
--
--    -- pixelVisionOS:AddUndoHistory(historyAction)
--
--    -- -- We only want to update the buttons in some situations
--    -- -- if(updateButtons ~= false) then
--    -- self:UpdateHistoryButtons()
--    -- end
--
--end

--function TilemapTool:OnUndo()
--
--    -- local action = pixelVisionOS:Undo()
--
--    -- if(action ~= nil and action.Action ~= nil) then
--    --     action.Action()
--    -- end
--
--    -- self:UpdateHistoryButtons()
--end
--
--function TilemapTool:OnRedo()
--
--    -- local action = pixelVisionOS:Redo()
--
--    -- if(action ~= nil and action.Action ~= nil) then
--    --     action.Action()
--    -- end
--
--    -- self:UpdateHistoryButtons()
--end
--
--function TilemapTool:UpdateHistoryButtons()
--
--    -- pixelVisionOS:EnableMenuItem(UndoShortcut, pixelVisionOS:IsUndoable())
--    -- pixelVisionOS:EnableMenuItem(RedoShortcut, pixelVisionOS:IsRedoable())
--
--end
--
--function TilemapTool:ClearHistory()
--
--    -- Reset history
--    -- pixelVisionOS:ResetUndoHistory()
--    -- self:UpdateHistoryButtons()
--
--end
