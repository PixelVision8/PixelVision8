--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

local palettePanelID = "PalettePanelUI"
local flagPanelID = "FlagPanelUI"

function TilemapTool:CreatePalettePanel()

    local pickerRect = {x = 184, y = 24, w = 64, h = 16}

    -- TODO setting the total to 0
    self.paletteColorPickerData = pixelVisionOS:CreateColorPicker(
            pickerRect,
            {x = 8, y = 8},
            pixelVisionOS.totalPaletteColors,
            16, -- Total per page
            8, -- Max pages
            pixelVisionOS.colorOffset + 128,
            "itempicker",
            "palette color",
            false,
            true,
            false
    )

    self.paletteColorPickerData.onDrawColor = function(data, id, x, y)

        if(id < data.total and (id % data.totalPerPage) < data.visiblePerPage) then
            local colorID = id + data.altColorOffset

            if(Color(colorID) == pixelVisionOS.maskColor) then
                data.canvas.DrawSprites(emptymaskcolor.spriteIDs, x, y, emptymaskcolor.width, false, false)
            else
                data.canvas.Clear(colorID, x, y, data.itemSize.x, data.itemSize.y)
            end

        else
            data.canvas.DrawSprites(emptycolor.spriteIDs, x, y, emptycolor.width, false, false)
        end

    end

    pixelVisionOS:ColorPickerVisiblePerPage(self.paletteColorPickerData, pixelVisionOS.colorsPerSprite)

    pixelVisionOS:RebuildColorPickerCache(self.paletteColorPickerData)

    self.paletteColorPickerData.visiblePerPage = pixelVisionOS.paletteColorsPerPage

    -- Wire up the picker to change the color offset of the sprite picker
    self.paletteColorPickerData.onPageAction = function(value)

        --if(usePalettes == true) then

        -- Calculate the new color offset
        local newColorOffset = pixelVisionOS.colorOffset + pixelVisionOS.totalPaletteColors + ((value - 1) * 16)

        -- Update the sprite picker color offset
        -- self.spritePickerData.colorOffset = newColorOffset
        pixelVisionOS:ChangeItemPickerColorOffset(self.spritePickerData, newColorOffset)

        self.tilePickerData.paintColorOffset = ((value - 1) * 16) + 128

        self:ApplyTilePalette()
        --end

    end

    paletteButton = editorUI:CreateButton(pickerRect, nil, "Apply color palette")

    paletteButton.onAction = function()self:ApplyTilePalette() end

    self.flagPicker = editorUI:CreatePicker(
            pickerRect,
            8,
            8,
            16,
            "flagpicker",
            "Pick a flag"
    )

    table.insert(self.enabledUI, self.flagPicker)


    self.flagPicker.onAction = function(value)
        pixelVisionOS:ChangeTilemapPaintFlag(self.tilePickerData, value, toolMode ~= 1 and toolMode ~= 3)
    end
    
    --pixelVisionOS:CanvasBrushColor(self.canvasData, value)

    -- TODO this should be triggered by the mode
    --self:ShowPalette()
end

function TilemapTool:ShowPalette()
    pixelVisionOS:RegisterUI({name = palettePanelID}, "UpdatePalette", self)
end

function TilemapTool:HidePalette()
    pixelVisionOS:RemoveUI(palettePanelID)
end

function TilemapTool:ShowFlags()

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
    
    pixelVisionOS:RegisterUI({name = flagPanelID}, "UpdateFlags", self)
    
end

function TilemapTool:HideFlags()
    pixelVisionOS:RemoveUI(flagPanelID)
end

function TilemapTool:UpdatePalette()
    editorUI:UpdateButton(paletteButton)
    pixelVisionOS:UpdateColorPicker(self.paletteColorPickerData)
end

function TilemapTool:UpdateFlags()
    
    editorUI:UpdatePicker(self.flagPicker)

    -- TODO this should only happen when the flag picker is in focus
    local over = editorUI:CalculatePickerPosition(self.flagPicker)

    self.flagPicker.toolTip = "Select flag " .. CalculateIndex(over.x, over.y, self.flagPicker.columns) .."."

end
--
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