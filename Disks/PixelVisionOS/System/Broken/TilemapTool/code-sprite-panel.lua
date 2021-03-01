--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--


local spritePanelID = "SpritePanelUI"

function TilemapTool:CreateSpritePanel()

    self.selectionSizes = {
        {x = 1, y = 1, scale = 16},
        --{x = 1, y = 2, scale = 8},
        --{x = 2, y = 1, scale = 8},
        {x = 2, y = 2, scale = 8},
        -- {x = 3, y = 3, scale = 4},
        {x = 4, y = 4, scale = 4}
    }

    self.maxSpriteSize = #self.selectionSizes
    

    self:ConfigureSpritePickerSelector(1)

    self.sizeBtnData = editorUI:CreateButton({x = 160, y = 16}, "sprite1x", "Next sprite sizes (+). Press with Shift for previous (-).")
    self.sizeBtnData.onAction = function() self:OnNextSpriteSize() end

    table.insert(self.enabledUI, self.sizeBtnData)

    -- Get sprite texture dimensions
    local totalSprites = gameEditor:TotalSprites()
    -- This is fixed size at 16 cols (128 pixels wide)
    local spriteColumns = 16
    local spriteRows = math.ceil(totalSprites / 16)

    self.spritePickerData = pixelVisionOS:CreateSpritePicker(
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

    self.spritePickerData.picker.borderOffset = 8

    -- table.insert(self.enabledUI, self.spritePickerData.picker)
    table.insert(self.enabledUI, self.spritePickerData.vSlider)
    table.insert(self.enabledUI, self.spritePickerData.picker)

    self.spritePickerData.onPress = function(value) self:OnSelectSprite(value) end
    
    pixelVisionOS:RegisterUI({name = spritePanelID}, "UpdateSpritePanel", self)

end

function TilemapTool:UpdateSpritePanel()

    pixelVisionOS:UpdateSpritePicker(self.spritePickerData)

    editorUI:UpdateButton(self.sizeBtnData)
    
end

function TilemapTool:ConfigureSpritePickerSelector(size)


    local selectionSize = self.selectionSizes[size]

    local x = selectionSize.x
    local y = selectionSize.y

    local spriteName = "selection"..x.."x" .. y

    _G["spritepickerover"] = {spriteIDs = _G[spriteName .. "over"].spriteIDs, width = _G[spriteName .. "over"].width, colorOffset = 0}

    _G["spritepickerselectedup"] = {spriteIDs = _G[spriteName .. "selected"].spriteIDs, width = _G[spriteName .. "selected"].width, colorOffset = 0}

    pixelVisionOS:ChangeItemPickerScale(self.spritePickerData, size, selectionSize)

    if(self.tilePickerData ~= nil) then
        pixelVisionOS:ChangeItemPickerScale(self.tilePickerData, size, selectionSize)
    end

end

function TilemapTool:OnNextSpriteSize(reverse)

    print("reverse", reverse)
    -- Loop backwards through the button sizes
    if(Key(Keys.LeftShift) or reverse == true) then
        self.spriteSize = self.spriteSize - 1
        
        if(self.spriteSize < 1) then
            self.spriteSize = self.maxSpriteSize
        end

        -- Loop forward through the button sizes
    else
        self.spriteSize = self.spriteSize + 1

        if(self.spriteSize > self.maxSpriteSize) then
            self.spriteSize = 1
        end
    end

    -- Find the next sprite for the button
    local spriteName = "sprite"..tostring(self.spriteSize).."x"

    -- Change sprite button graphic
    self.sizeBtnData.cachedSpriteData = {
        up = _G[spriteName .. "up"],
        down = _G[spriteName .. "down"] ~= nil and _G[spriteName .. "down"] or _G[spriteName .. "selectedup"],
        over = _G[spriteName .. "over"],
        selectedup = _G[spriteName .. "selectedup"],
        selectedover = _G[spriteName .. "selectedover"],
        selecteddown = _G[spriteName .. "selecteddown"] ~= nil and _G[spriteName .. "selecteddown"] or _G[spriteName .. "selectedover"],
        disabled = _G[spriteName .. "disabled"],
        empty = _G[spriteName .. "empty"] -- used to clear the sprites
    }

    self:ConfigureSpritePickerSelector(self.spriteSize)

    self:ChangeSpriteID(self.spritePickerData.currentSelection)

    -- Reset the flag preview
    pixelVisionOS:ChangeTilemapPaintFlag(self.tilePickerData, self.tilePickerData.paintFlagIndex)

    editorUI:Invalidate(self.sizeBtnData)

end

function TilemapTool:OnSelectSprite(value)

    pixelVisionOS:ChangeTilemapPaintSpriteID(self.tilePickerData, self.spritePickerData.pressSelection.index)

end

function TilemapTool:ChangeSpriteID(value)

    -- Need to convert the text into a number
    value = tonumber(value)

    pixelVisionOS:SelectSpritePickerIndex(self.spritePickerData, value, false)

    if(self.tilePickerData ~= nil) then

        pixelVisionOS:ChangeTilemapPaintSpriteID(self.tilePickerData, self.spritePickerData.currentSelection, toolMode ~= 1 and toolMode ~= 3)
    end

end