--
-- Copyright (c) 2017, Jesse Freeman. All rights reserved.
--
-- Licensed under the Microsoft Public License (MS-PL) License.
-- See LICENSE file in the project root for full license information.
--
-- Contributors
-- --------------------------------------------------------
-- This is the official list of Pixel Vision 8 contributors:
--
-- Jesse Freeman - @JesseFreeman
-- Christina-Antoinette Neofotistou - @CastPixel
-- Christer Kaitila - @McFunkypants
-- Pedro Medeiros - @saint11
-- Shawn Rakowski - @shwany
--

function PixelVisionOS:CreateTilemapPicker(rect, itemSize, columns, rows, colorOffset, spriteName, toolTip, enableDragging, draggingLabel)

    -- Create the generic UI data for the component
    local data = self:CreateItemPicker(rect, itemSize, columns, rows, colorOffset, spriteName, toolTip, enableDragging, draggingLabel)

    data.name = "TilemapPicker" .. data.name
    data.mode = 1
    data.layerCache = {}
    data.maxPerLoop = 64
    data.paintTileIndex = 0
    data.paintFlagIndex = 0
    data.paintColorOffset = 0
    data.lastOverPos = NewPoint(-1, - 1)

    data.onOverRender = function(data, tmpX, tmpY)
        return self:ReadTilePickerOverPixelData(data, tmpX, tmpY)
    end

    -- Create mask pixel data for the eraser preview
    data.maskPixelData = {}

    for i = 1, 4 do

        local size = i * 8
        local total = size * size
        local tmpPixelData = {}

        for j = 1, total do
            table.insert(tmpPixelData, - 1)
        end

        table.insert(data.maskPixelData, tmpPixelData)
    end

    return data

end

function PixelVisionOS:ReadTilePickerOverPixelData(data, tmpX, tmpY)

    if(data.mode == 3) then
        return data.maskPixelData[data.scale]
    elseif(data.mode == 2 or data.mode == 4) then

        if(data.mapRenderMode == 0) then
            return data.overTilePixelData

        elseif(data.mapRenderMode == 1) then
            return data.overFlagPixelData
        end
    else
        return self:ReadItemPickerOverPixelData(data, tmpX, tmpY)
    end

end

function PixelVisionOS:UpdateTilemapPicker(data)

    if(data.mode == 2 or data.mode == 4) then

        if(data.mapRenderMode == 0) then

            if(pixelVisionOS.paletteMode == true) then
                data.overItemDrawArgs[9] = data.colorOffset + data.paintColorOffset
            end

        else

            data.overItemDrawArgs[9] = 0
        end
    else
        data.overItemDrawArgs[9] = data.colorOffset
    end

    if(self.editorUI.collisionManager.mouseDown == true and data.mode > 1 and data.enabled ~= false and data.picker.inFocus == true) then

        local overPos = self:CalculateItemPickerPosition(data)

        if(overPos.x ~= -1 and overPos.y ~= -1) then

            if(data.mode == 3) then

                self:ChangeTile(data, overPos.x, overPos.y, - 1, 0, - 1)

                self:ClearItemPickerSelection(data)

            elseif(data.mode == 2) then

                local tileData = gameEditor:Tile(overPos.x, overPos.y);

                if(data.mapRenderMode == 0) then
                    self:ChangeTile(data, overPos.x, overPos.y, data.paintTileIndex, data.paintColorOffset, tileData.flag)
                elseif(data.mapRenderMode == 1) then
                    self:UpdateFlagID(data, overPos.x, overPos.y, data.paintFlagIndex)
                end

                self:ClearItemPickerSelection(data)

            elseif(data.mode == 4) then

                gameEditor:FloodFillTilemap(data.mapRenderMode == 0 and data.paintTileIndex or data.paintFlagIndex, overPos.x, overPos.y, data.mapRenderMode, data.scale.x, data.scale.y, data.paintColorOffset)

                -- Clear the current layer's cache
                data.layerCache[data.mapRenderMode] = null

                self:PreRenderMapLayer(data, data.mapRenderMode)

                editorUI.mouseCursor:SetCursor(5, true)

                self:InvalidateMap(data)
            end

        end

    end

    self:UpdateItemPicker(data)

    -- Clear selection on release if we are not in selection mode
    if(self.editorUI.collisionManager.mouseReleased == true and data.mode > 1 and data.picker.inFocus == true) then

        -- Remove tile selection
        self:ClearItemPickerSelection(data)

    end

end

function PixelVisionOS:UpdateFlagID(data, col, row, flagID)

    local size = data.scale

    -- Calculate the total tiles affected
    local total = size.x * size.y

    local tileHistory = {}

    -- Loop through all the tiles that need to be updated
    for i = 1, total do

        local offset = CalculatePosition(i - 1, size.x)

        local nextCol = col + offset.x
        local nextRow = row + offset.y

        local currentTile = gameEditor:Tile(nextCol, nextRow)

        if(currentTile.flag ~= flagID) then

            local nextSpriteID = currentTile.spriteID
            local nextColorOffset = currentTile.colorOffset
            local nextColorOffset = currentTile.colorOffset

            local savedTile = {
                spriteID = currentTile.spriteID,
                col = nextCol,
                row = nextRow,
                colorOffset = currentTile.colorOffset,
                flag = currentTile.flag
            }

            table.insert(tileHistory, savedTile)

            self:OnChangeTile(data, nextCol, nextRow, nextSpriteID, nextColorOffset, flagID)

        end

    end

    -- TODO There should be a call to the history API or a callback the tool can use to save state
    --UpdateHistory(tileHistory)

end

function PixelVisionOS:ChangeTile(data, col, row, spriteID, colorOffset, flagID, scale)

    colorOffset = colorOffset or 0
    flagID = flagID or - 1

    local size = scale or data.scale

    local total = size.x * size.y

    local spriteCols = 128 / 8

    local spritePos = spriteID == -1 and nil or CalculatePosition(spriteID, spriteCols)

    local tileHistory = {}

    for i = 1, total do

        local offset = CalculatePosition(i - 1, size.x)

        local nextCol = col + offset.x
        local nextRow = row + offset.y

        local nextSpriteID = spriteID == -1 and spriteID or CalculateIndex(spritePos.x + offset.x, spritePos.y + offset.y, spriteCols)

        local currentTile = gameEditor:Tile(nextCol, nextRow)

        if(currentTile.spriteID ~= nextSpriteID or currentTile.colorOffset ~= colorOffset or currentTile.flag ~= flagID) then

            local savedTile = {
                spriteID = currentTile.spriteID,
                col = nextCol,
                row = nextRow,
                colorOffset = currentTile.colorOffset,
                flag = currentTile.flag
            }

            table.insert(tileHistory, savedTile)
            self:OnChangeTile(data, nextCol, nextRow, nextSpriteID, colorOffset, flagID)

        end
    end

    

end

function PixelVisionOS:SwapTiles(data, srcTile, destTile)

    local srcPos = CalculatePosition(srcTile.index, data.columns)
    local destPos = CalculatePosition(destTile.index, data.columns)

    local size = data.scale

    local total = size.x * size.y

    local tileHistory = {}

    for i = 1, total do
        --
        local offset = CalculatePosition(i - 1, size.x)

        local nextSrcCol = srcPos.x + offset.x
        local nextSrcRow = srcPos.y + offset.y

        local nextSrcTile = gameEditor:Tile(nextSrcCol, nextSrcRow)

        local srcTileData = {
            spriteID = nextSrcTile.spriteID,
            col = nextSrcCol,
            row = nextSrcRow,
            colorOffset = nextSrcTile.colorOffset,
            flag = nextSrcTile.flag
        }

        local nextDestCol = destPos.x + offset.x
        local nextDestRow = destPos.y + offset.y

        local nextDestTile = gameEditor:Tile(nextDestCol, nextDestRow)

        local destTileData = {
            spriteID = nextDestTile.spriteID,
            col = destPos.x + offset.x,
            row = destPos.y + offset.y,
            colorOffset = nextDestTile.colorOffset,
            flag = nextDestTile.flag
        }

        table.insert(tileHistory, srcTileData)
        table.insert(tileHistory, destTileData)

        self:OnChangeTile(data, destTileData.col, destTileData.row, srcTileData.spriteID, srcTileData.colorOffset, srcTileData.flag)

        self:OnChangeTile(data, srcTileData.col, srcTileData.row, destTileData.spriteID, destTileData.colorOffset, destTileData.flag)

    end

    

end

function PixelVisionOS:OnChangeTile(data, col, row, spriteID, colorOffset, flag)

    local tile = gameEditor:Tile(col, row, spriteID, colorOffset, flag)

    self:RenderTile(data, tile, col, row)

    self:InvalidateMap(data)

    

end

function PixelVisionOS:ReadRenderPercent(data)
    return Clamp(((data.currentLoop / data.totalLoops) * 100), 0, 100)
end

function PixelVisionOS:PreRenderMapLayer(data, mode)

    data.spriteSize = gameEditor:SpriteSize()

    data.totalPixels = data.spriteSize.x * data.spriteSize.y

    data.mapSize = gameEditor:TilemapSize()

    local realWidth = data.spriteSize.x * data.mapSize.x
    local realHeight = data.spriteSize.y * data.mapSize.y

    data.mapRenderMode = mode

    if (data.layerCache[mode] == null) then

        self.editorUI:Enable(data.picker, false)

        data.layerCache[data.mapRenderMode] = data.mapRenderMode == 0 and NewCanvas(realWidth, realHeight) or NewCanvas(realWidth, realHeight)

        data.renderingMap = true

    end

    -- Set the tmpCanvas to the cache
    data.canvas = data.layerCache[data.mapRenderMode]

    -- Rebuild the map if it hasn't been rendered yet.
    if (data.renderingMap) then

        data.canvas.Clear(-1)

        data.totalTiles = data.mapSize.x * data.mapSize.y

        data.renderingMap = true

        data.totalLoops = math.ceil(data.totalTiles / data.maxPerLoop)

        data.currentLoop = 0

    end

    self:InvalidateItemPickerDisplay(data)

end

function PixelVisionOS:NextRenderStep(data)

    local offset = data.currentLoop * data.maxPerLoop;

    for i = 1, data.maxPerLoop do

        local index = (i - 1) + offset;

        if (index >= data.totalTiles) then

            data.renderingMap = false;
            self.editorUI:Enable(data.picker, true)

            break;

        end

        local pos = CalculatePosition(index, data.mapSize.x)

        local tileData = gameEditor:Tile(pos.x, pos.y);

        -- Check for palette mode
        if(pixelVisionOS.paletteMode) then

            -- Check to see if the tile is in a palette
            if(tileData.colorOffset < 128) then

                -- Update the tile with the new palette offset
                tileData = gameEditor:Tile(pos.x, pos.y, tileData.spriteID, 128, tileData.flag)

            end

        end

        -- Render the tile to the display
        self:RenderTile(data, tileData, pos.x, pos.y);

    end

    data.currentLoop = data.currentLoop + 1

end

function PixelVisionOS:RenderTile(data, tileData, col, row)

    local spriteData = nil
    local flagData = nil

    col = col * data.spriteSize.x;
    row = row * data.spriteSize.y;

    local layer = data.layerCache[data.mapRenderMode]

    if (layer ~= null) then

        -- Make sure the area is cleared first
        layer:Clear(-1, col, row, 8, 8)

        if(data.mapRenderMode == 0) then
            if (tileData.spriteID > - 1) then

                --if(pixelVisionOS.paletteMode == true) then
                    local spriteData = gameEditor:Sprite(tileData.spriteID)

                    layer:MergePixels(col, row, 8, 8, spriteData, false, false, tileData.colorOffset, true)
                --else
                --    layer:DrawSprite(tileData.spriteID, col, row, false, false, tileData.colorOffset)
                --end
            end
        elseif(data.mapRenderMode == 1) then
            if (tileData.flag > - 1) then

                local spriteData = _G["flag"..(tileData.flag + 1).."small"]
                if(spriteData ~= nil) then
                    layer:DrawSprite(spriteData.spriteIDs[1], col, row)
                end
            end
        end

    end

    self:InvalidateItemPickerDisplay(data)

end

function PixelVisionOS:ChangeTilemapPickerMode(data, value)

    data.mode = value

    if(data.mode == 1) then
        data.enableDragging = true

        data.picker.focusCursor = 2

    else
        data.enableDragging = false

        if(value == 2) then
            data.picker.focusCursor = 6
        elseif(value == 3) then
            data.picker.focusCursor = 7
        elseif(value == 4) then
            data.picker.focusCursor = 8
        end

    end

    if(data.picker.inFocus == true) then

        -- Force the picker to change the cursor if it's in focus
        self.editorUI:SetFocus(data.picker, data.picker.focusCursor)

        -- Invalidate the last over ID so it redraws
        data.lastOverID = -1

    end

end

function PixelVisionOS:InvalidateMap(data)
    data.mapInvalid = true
end

function PixelVisionOS:ResetMapValidation(data)
    data.mapInvalid = false
end

function PixelVisionOS:ChangeTilemapPaintFlag(data, value, updatePreview)

    data.paintFlagIndex = value

    local size = NewPoint(data.scale.x * 8, data.scale.y * 8)

    local tmpCanvas = NewCanvas(size.x, size.y)

    tmpCanvas:SetPattern(Sprite(_G["flag"..(data.paintFlagIndex + 1).."small"].spriteIDs[1]), 8, 8)

    tmpCanvas:FloodFill(1, 1)

    data.overFlagPixelData = tmpCanvas:GetPixels()

    if(updatePreview ~= false) then
        -- Force the over item draw to display the new pixel data
        data.overItemDrawArgs[1] = data.overFlagPixelData
    end

end

function PixelVisionOS:ChangeTilemapPaintSpriteID(data, value, updatePreview)

    data.paintTileIndex = value

    data.overTilePixelData = gameEditor:ReadGameSpriteData(data.paintTileIndex, data.scale.x, data.scale.y)

    if(updatePreview ~= false) then
        -- Force the over item draw to display the new pixel data
        data.overItemDrawArgs[1] = data.overTilePixelData
    end
end
