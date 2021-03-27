--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

local spritePanelID = "SpritePanelUI"

function DrawTool:CreateSpritePanel()
   
    self.colorLayerTime = 0
    self.colorLayerDelay = 500
    self.showIsolatedColor = false

    self.selectionSizes = {
        {x = 1, y = 1, scale = 16},
        {x = 1, y = 2, scale = 8},
        {x = 2, y = 1, scale = 8},
        {x = 2, y = 2, scale = 8},
        -- {x = 3, y = 3, scale = 4},
        {x = 4, y = 4, scale = 4}
    }

    self:ConfigureSpritePickerSelector(1)

    self.newPos = NewPoint()
    -- self.posScale = 1
    
    -- Get sprite texture dimensions
    local totalSprites = gameEditor:TotalSprites()

    -- This is fixed size at 16 cols (128 pixels wide)
    local spriteColumns = 16
    local spriteRows = math.ceil(totalSprites / 16)

    self.spritePickerData = pixelVisionOS:CreateSpritePicker(
        {x = 176, y = 32, w = 64, h = 128 },
        {x = 8, y = 8},
        spriteColumns,
        spriteRows,
        pixelVisionOS.colorOffset,
        "spritepicker",
        "Pick a sprite",
        true,
        "SpritePicker"
    )

    -- Shift the picker color back for the selection
    self.spritePickerData.picker.colorOffser = PaletteOffset( 0 )

    self.spritePickerData.picker.borderOffset = 8

    self.spritePickerData.onAction = function(value) self:ChangeSpriteID(value) end
    self.spritePickerData.onDropTarget = function(src, dest) self:OnSpritePickerDrop(src, dest) end

    self.spriteIDInputData = editorUI:CreateInputField({x = 176, y = 208, w = 32}, "0", "The ID of the currently selected sprite.", "number")
    
    self.spriteIDInputData.min = 0
    self.spriteIDInputData.max = gameEditor:TotalSprites() - 1
    self.spriteIDInputData.onAction = function(value) self:ChangeSpriteID(value) end
    
    self.spriteMode = 0
    self.maxSpriteSize = #self.selectionSizes

    -- Create size button
    self.sizeBtnData = editorUI:CreateButton({x = 224, y = 200}, "spritemode1x1", "Pick the sprite size.")
    self.sizeBtnData.onAction = function() self:OnNextSpriteSize() end

    

    pixelVisionOS:RegisterUI({name = spritePanelID}, "UpdateSpritePanel", self)

end

function DrawTool:UpdateSpritePanel()

    pixelVisionOS:UpdateSpritePicker(self.spritePickerData)
    editorUI:UpdateInputField(self.spriteIDInputData)
    editorUI:UpdateButton(self.sizeBtnData)

    -- TODO need to check if the sprite panel is in focus
    if(pixelVisionOS.editingInputField ~= true and self.selectionMode == SpriteMode) then

        -- Change the scale
        if(Key(Keys.OemMinus, InputState.Released) and self.spriteMode > 1) then
            self:OnNextSpriteSize(true)
        elseif(Key(Keys.OemPlus, InputState.Released) and self.spriteMode < self.maxSpriteSize) then
            self:OnNextSpriteSize()
        end

        -- Create a new point to see if we need to change the sprite position
        self.newPos.X = 0
        self.newPos.Y = 0

        -- Offset the new position by the direction button
        if(Key(Keys.Up, InputState.Released)) then
            self.newPos.y = -1 * self.selectionSize.y
        elseif(Key(Keys.Right, InputState.Released)) then
            self.newPos.x = 1 * self.selectionSize.x
        elseif(Key(Keys.Down, InputState.Released)) then
            self.newPos.y = 1 * self.selectionSize.y
        elseif(Key(Keys.Left, InputState.Released)) then
            self.newPos.x = -1 * self.selectionSize.x
        end

        -- Test to see if the new position has changed
        if(self.newPos.x ~= 0 or self.newPos.y ~= 0) then

            local curPos = CalculatePosition(self.panelInFocus.currentSelection, self.panelInFocus.columns)

            self.newPos.x = Clamp(curPos.x + self.newPos.x, 0, self.panelInFocus.columns - 1)
            self.newPos.y = Clamp(curPos.y + self.newPos.y, 0, self.panelInFocus.rows - 1)

            local newIndex = CalculateIndex(self.newPos.x, self.newPos.y, self.panelInFocus.columns)

            self:ChangeSpriteID(newIndex)

        end

    end

    if(self.lastSpriteOffset ~= self.spritePickerData.colorOffset) then
        self.lastSpriteOffset = self.spritePickerData.colorOffset
        self.offsetDisplayText = string.format("OFFSET %03d", self.lastSpriteOffset - pixelVisionOS.colorOffset)
    end

    -- DrawText(  )

    DrawText(self.offsetDisplayText, 172, 224-2, DrawMode.Sprite, "medium", 6, -4)

    if(self.lastSpriteSize ~= self.selectionSizes[self.spriteMode]) then
        self.lastSpriteSize = self.selectionSizes[self.spriteMode]
        self.sizeDisplayText = string.format("%02dx%02d", self.lastSpriteSize.x * editorUI.spriteSize.x, self.lastSpriteSize.y * editorUI.spriteSize.y)
    end

    DrawText(self.sizeDisplayText, 224-2, 224-2, DrawMode.Sprite, "medium", 6, -4 )

end



function DrawTool:OnNextSpriteSize(reverse)

    -- Loop backwards through the button sizes
    if(Key(Keys.LeftShift) or reverse == true) then
        self.spriteMode = self.spriteMode - 1

        if(self.spriteMode < 1) then
            self.spriteMode = #self.selectionSizes
        end

    else
        self.spriteMode = self.spriteMode + 1

        if(self.spriteMode > #self.selectionSizes) then
            self.spriteMode = 1
        end
    end

    -- Find the next sprite for the button
    local spriteName = "spritemode"..self.selectionSizes[self.spriteMode].x.."x" .. self.selectionSizes[self.spriteMode].y

    -- Change sprite button graphic
    self.sizeBtnData.cachedMetaSpriteIds = {
        up = FindMetaSpriteId(spriteName .. "up"),
        down = FindMetaSpriteId(spriteName .. "down") ~= -1 and FindMetaSpriteId(spriteName .. "down") or FindMetaSpriteId(spriteName .. "selectedup"),
        over = FindMetaSpriteId(spriteName .. "over"),
        selectedup = FindMetaSpriteId(spriteName .. "selectedup"),
        selectedover = FindMetaSpriteId(spriteName .. "selectedover"),
        selecteddown = FindMetaSpriteId(spriteName .. "selecteddown") ~= -1 and FindMetaSpriteId(spriteName .. "selecteddown") or FindMetaSpriteId(spriteName .. "selectedover"),
        disabled = FindMetaSpriteId(spriteName .. "disabled"),
        empty = FindMetaSpriteId(spriteName .. "empty") -- used to clear the sprites
    }

    self:ConfigureSpritePickerSelector(self.spriteMode)

    editorUI:Invalidate(self.sizeBtnData)

    -- -- TODO need to rewire this
    pixelVisionOS:ChangeCanvasPixelSize(self.canvasData, self.selectionSizes[self.spriteMode].scale)

    -- -- -- Force the sprite editor to update to the new selection from the sprite picker
    -- self:ChangeSpriteID(self.spritePickerData.currentSelection)

end

function DrawTool:ConfigureSpritePickerSelector(size)
    
    self.selectionSize = self.selectionSizes[size]

    local x = self.selectionSize.x
    local y = self.selectionSize.y

    local spriteName = "selection"..x.."x" .. y

    -- print(self.spritePickerData.cachedMetaSpriteIds)
 
    -- _G["spritepickerover"] = {spriteIDs = _G[spriteName .. "over"].spriteIDs, width = _G[spriteName .. "over"].width, colorOffset = 0}

    -- local state = self.selectionMode == SpriteMode and "selected" or "over"

    -- _G["spritepickerselectedup"] = {spriteIDs = _G[spriteName .. "selected"].spriteIDs, width = _G[spriteName .. "selected"].width, colorOffset = 0}

    pixelVisionOS:ChangeItemPickerScale(self.spritePickerData, size, self.selectionSize)

end

function DrawTool:ChangeSpriteID(value)

    -- print("value", value)

    -- Need to convert the text into a number
    value = tonumber(value)

    pixelVisionOS:SelectSpritePickerIndex(self.spritePickerData, value, false)

    editorUI:ChangeInputField(self.spriteIDInputData, self.spritePickerData.currentSelection, false)

    -- -- -- ClearHistory()

    self:UpdateCanvas(self.spritePickerData.currentSelection)
    
    self.spritePickerData.dragging = false

    self:ForcePickerFocus(self.spritePickerData)

    -- -- -- Update the input field
    editorUI:ChangeInputField(self.spriteIDInputData, value, false)

    self:UpdateCanvas(value)
    
    if(self.mode ~= SpriteMode) then

        self:ChangeEditMode(SpriteMode)
        
    end

end

function DrawTool:OnSpritePickerDrop(src, dest)

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

            local selection = self.selectionSizes[self.spriteMode]

            self:BeginUndo(self)

            local srcSprite = gameEditor:ReadGameSpriteData(srcSpriteID, selection.x, selection.y)
            local destSprite = gameEditor:ReadGameSpriteData(destSpriteID, selection.x, selection.y)

            -- TODO need to track the switch for undo

            -- Swap the sprite in the tool's color memory
            gameEditor:WriteSpriteData(srcSpriteID, destSprite, selection.x, selection.y)
            gameEditor:WriteSpriteData(destSpriteID, srcSprite, selection.x, selection.y)

            -- Update the pixel data in the spritePicker

            local itemSizeX = selection.x * 8
            local itemSizeY = selection.y * 8

            pixelVisionOS:UpdateItemPickerPixelDataAt(self.spritePickerData, srcSpriteID, destSprite, itemSizeX, itemSizeY)
            pixelVisionOS:UpdateItemPickerPixelDataAt(self.spritePickerData, destSpriteID, srcSprite, itemSizeX, itemSizeY)

            pixelVisionOS:InvalidateItemPickerDisplay(src)

            self:EndUndo()
            -- ChangeSpriteID(destSpriteID)

            self:InvalidateData()

        end
    elseif(src.name == self.systemColorPickerData.name and self.changingColorIndex ~= true) then

        -- Get the current color
        -- local colorOffset = src.pressSelection.index

        pixelVisionOS:ChangeItemPickerColorOffset(dest, src.pressSelection.index + pixelVisionOS.colorOffset)

    elseif(src.name == self.paletteColorPickerData.name) then
        
        -- Get the current color
        local colorOffset = pixelVisionOS.colorOffset + pixelVisionOS.totalPaletteColors + (((src.pages.currentSelection - 1) * 16) + src.picker.selected)

        pixelVisionOS:ChangeItemPickerColorOffset(dest, colorOffset)

    end

end