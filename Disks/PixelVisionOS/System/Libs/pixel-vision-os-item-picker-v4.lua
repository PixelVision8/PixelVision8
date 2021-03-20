function PixelVisionOS:CreateItemPicker(rect, itemSize, columns, rows, colorOffset, spriteName, toolTip, enableDragging, draggingLabel)

    -- Create the component's base values
    local data = editorUI:CreateData(rect)

    data.name = "ItemPicker" .. data.name

    data.itemSize = itemSize

    -- Create the viewport based on the size of the component passed in
    data.viewport = NewRect(0, 0, rect.w, rect.h)

    data.tmpItemRect = NewRect(0, 0, 8, 8)

    -- Calculate the real viewport
    data.realWidth = columns * itemSize.x
    data.realHeight = rows * itemSize.y

    -- Define the visible rect based on the size of the item or the viewport
    data.visibleRect = {
        x = rect.x,
        y = rect.y,
        w = math.min(data.realWidth, data.viewport.width),
        h = math.min(data.realHeight, data.viewport.height)
    }

    data.total = columns * rows

    -- Create a canvas to store pixel data for rendering
    data.canvas = NewCanvas(data.realWidth, data.realHeight)

    -- Clear the canvas
    data.canvas:Clear()
    data.colorOffset = colorOffset

    data.showBGColor = false

    -- Store the default columns and rows
    data.columns = columns
    data.rows = rows

    data.visibleColumns = math.min(columns, data.viewport.width / data.itemSize.x)
    data.visibleRows = math.min(rows, data.viewport.height / data.itemSize.y)

    -- Total items displayed in the bounds
    data.totalItems = columns * rows

    -- Default scale is 1
    data.scale = {x = 1, y = 1}
    data.displaySize = Display()

    data.maxScale = 4
    data.zoom = 1

    -- Scroll values
    data.scrollScale = NewPoint(math.ceil((data.columns - data.visibleColumns) / data.maxScale), math.ceil((data.rows - data.visibleRows) / data.maxScale))
    data.scrollValueOffset = NewPoint(0, 0)
    data.lastStartX = 0
    data.lastStartY = 0

    data.scrollOverTime = 0
    data.scrollOverDelay = .5

    -- Tooltip values
    data.toolTipLabel = toolTip or "item"
    data.totalItemStringPadding = #tostring(data.totalItems)
    data.totalColStringPadding = #tostring(columns)
    data.totalRowStringPadding = #tostring(rows)
    data.currentSelection = -1
    data.pressSelection = nil

    data.scrollShift = NewPoint()

    data.ctrlCopyEnabled = true

    -- Force the display to invalidate
    data.invalidateDisplay = true

    if(_G["emptycolor"] ~= nil) then
        data.emptyDrawArgs = {
            {_G["emptycolor"].spriteIDs[1]},
            0,
            0,
            1,
        }
    end

    data.overItemDrawArgs = {
        nil,
        0,
        0,
        8,
        8,
        false,
        false,
        DrawMode.Sprite,
        data.colorOffset
    }

    -- data.maskSpriteDrawArgs = {
    --     0,
    --     0,
    --     8,
    --     8,
    --     data.colorOffset - 1, -- TODO this is a hack since maskColor ID isn't working so we assume the offset minus 1 is a mask color
    --     DrawMode.Sprite
    -- }

    data.onOverRender = function(data, tmpX, tmpY)
        return self:ReadItemPickerOverPixelData(data, tmpX, tmpY)
    end

    -- Picker component to use for selection
    data.picker = editorUI:CreatePicker({x = data.visibleRect.x, y = data.visibleRect.y, w = data.visibleRect.w, h = data.visibleRect.h}, itemSize.x, itemSize.y, data.totalItems, spriteName, data.colorOffset, toolTip)

    -- Keep the picker from drawing the over graphic
    data.picker.drawOver = false

    data.picker.onAction = function(value, doubleClick)

        -- print("OnAction")
        if(editorUI.inFocusUI.name == data.name) then
            
            -- editorUI.inFocusUI = nil
            data.pressSelection = nil

            value = self:CalculateItemPickerPosition(data).index

            self:SelectItemPickerIndex(data, value, true, false)

            if(data.onAction ~= nil) then
                
                data.onAction(value)
            end
        end

    end

    data.picker.onPress = function()

        -- editorUI.inFocusUI = data

        -- Store the current ID being pressed
        data.pressSelection = self:CalculateItemPickerPosition(data)

        if(data.onPress ~= nil) then
            
            data.onPress(data.pressSelection.index)
        end

        if(data.onStartDrag ~= nil) then

            data.onStartDrag(data)
            -- end

        end

    end

    --print(data.name, "scroller", data.viewport.height < data.realHeight, _G["vsliderhandleup"])
    if(_G["vsliderhandleup"] ~= nil and data.viewport.height < data.realHeight) then

        data.vSlider = editorUI:CreateSlider(
            { x = rect.x + rect.w + 1, y = rect.y, w = 16, h = rect.h},
            "vsliderhandleup",
            "Scroll vertically.",
            false
        )

        data.vSlider.onAction = function(value)

            self:OnItemPickerVerticalScroll(data, value)

        end

    end

    if(data.realWidth > data.viewport.width ) then

        data.hSlider = editorUI:CreateSlider(
            { x = rect.x, y = rect.y + rect.h + 1, w = rect.w, h = 10},
            "hsliderhandle",
            "Scroll horizontally.",
            true
        )

        -- Rout the slider updates to the horizontal scroll function
        data.hSlider.onAction = function(value)
            self:OnItemPickerHorizontalScroll(data, value)
        end
    end

    -- Set the default size to 1
    self:ChangeItemPickerScale(data, 1, {x= 1, y = 1})

    data.draggingLabel = draggingLabel or "ItemPicker"
    data.enableDragging = enableDragging

    if(data.enableDragging == true) then
        editorUI.collisionManager:EnableDragging(data, .2, data.draggingLabel)

        -- Make sure we capture the onEndDrag event, clear the selection and pass it back to the collisionManager
        data.onEndDrag = function(target)

            editorUI.collisionManager:EndDrag(target)

            data.pressSelection = nil
        end

    end

    data.UpdateToolTip = function(tmpData)

        local action = ""
        local label = tmpData.toolTipLabel
        local index = 0
        local ending = ""
        
        local toolTipMessage = "%s %s ID %0" .. tmpData.totalItemStringPadding .. "d %s"
        
        
        if(tmpData.dragging) then

            if(tmpData.overPos.index ~= nil and tmpData.overPos.index ~= -1 and tmpData.overPos.index < tmpData.picker.total) then
                
                

                action = tmpData.copyDrag == true and "Copy" or "Swap"
               
                index = tmpData.pressSelection.index
                ending = (tmpData.copyDrag == true and "to" or "width") .. " ".. string.lpad(tostring(tmpData.overPos.index), tmpData.totalItemStringPadding, "0")

                -- tmpData.picker.toolTip = string.format(toolTipMessage, "swap", tmpData.toolTipLabel, tmpData.pressSelection.index, "with ".. string.lpad(tostring(tmpData.overPos.index), tmpData.totalItemStringPadding, "0"))--"Swap "..tmpData.toolTipLabel.." ID " .. string.lpad(tostring(tmpData.pressSelection.index), tmpData.totalItemStringPadding, "0") .. " with ".. string.lpad(tostring(tmpData.overPos.index), tmpData.totalItemStringPadding, "0")
            
            else
            
                action = "Dragging"
                index = tmpData.pressSelection.index

                -- tmpData.picker.toolTip = "Dragging "..tmpData.toolTipLabel.." ID " .. string.lpad(tostring(tmpData.pressSelection.index), tmpData.totalItemStringPadding, "0")
            
            end

        elseif(tmpData.overPos.index ~= nil and tmpData.overPos.index ~= -1) then

            action = "Select"
            index = tmpData.overPos.index

            -- Update the tooltip with the index and position
            -- tmpData.picker.toolTip = "Select "..tmpData.toolTipLabel.." ID " .. string.lpad(tostring(tmpData.overPos.index), tmpData.totalItemStringPadding, "0")

        else
            toolTipMessage = ""
        end

        tmpData.picker.toolTip = string.format(toolTipMessage, action, label, index, ending)

    end

    return data

end

function PixelVisionOS:SelectItemPickerIndex(data, value, callback, updateScroll)

    -- Clear the pressed ID
    data.pressSelection = nil

    -- Save the selection ID
    data.currentSelection = value or self:CalculateItemPickerPosition(data).index

    -- Calculate position
    local tmpPos = CalculatePosition(data.currentSelection, data.columns)

    -- Snap to grid
    tmpPos.x = math.floor(tmpPos.x / data.scale.x) * data.scale.x
    tmpPos.y = math.floor(tmpPos.y / data.scale.y) * data.scale.y

    -- Recalculate selection after snapping to the grid
    data.currentSelection = CalculateIndex(tmpPos.x, tmpPos.y, data.columns)

    if(data.onRelease ~= nil and callback ~= false) then
        print("picker callback")
        data.onRelease(data.currentSelection)
    end

    if(updateScroll ~= false) then

        local tmpCols = (data.realWidth / 8 - data.maxScale) / data.maxScale

        local tmpCol = math.floor(tmpPos.x / data.maxScale)

        local tmpHPer = tmpCol / tmpCols

        self:OnItemPickerHorizontalScroll(data, tmpHPer)

        local tmpRows = (data.realHeight / 8 - data.maxScale) / data.maxScale

        local tmpRow = math.floor((tmpPos.y - (data.scrollShift.y / 8)) / data.maxScale)

        local tmpVPer = tmpRow / tmpRows

        self:OnItemPickerVerticalScroll(data, tmpVPer)

    end

    self:UpdateItemPickerSelection(data)

end

function PixelVisionOS:OnItemPickerHorizontalScroll(data, value)

    if(data.hSlider ~= nil) then

        local scaleOffset = (data.maxScale / ((data.realWidth - data.rect.w) / 8)) * 100

        local testScale = math.floor(value * 100 / scaleOffset)

        data.lastStartX = (testScale * (data.maxScale * 8))

        value = testScale / data.scrollScale.x
        editorUI:ChangeSlider(data.hSlider, value, false)

        if(value ~= data.lastHValue) then
            self:InvalidateItemPickerDisplay(data)
            data.lastHValue = value
        end

    else
        data.lastStartX = 0
    end

end

function PixelVisionOS:OnItemPickerVerticalScroll(data, value)

    if(data.vSlider ~= nil) then

        local scaleOffset = (data.maxScale / ((data.realHeight - data.rect.h) / 8)) * 100

        local testScale2 = math.floor(value * 100 / scaleOffset)

        data.lastStartY = (testScale2 * (data.maxScale * 8)) + data.scrollShift.y

        value = testScale2 / data.scrollScale.y

        editorUI:ChangeSlider(data.vSlider, value, false)

        if(value ~= data.lastVValue) then
            self:InvalidateItemPickerDisplay(data)
            data.lastVValue = value
        end

    end

end

function PixelVisionOS:InvalidateItemPickerDisplay(data)

    data.invalidateDisplay = true

end

function PixelVisionOS:UpdateItemPicker(data)

    editorUI:UpdatePicker(data.picker)
    

    if(data.invalidateDisplay == true) then

        self:UpdateItemPickerSelection(data)

        -- Calculate the bg color
        local bgColor = data.showBGColor and gameEditor:BackgroundColor() or self.emptyColorID
        
        -- TODO need to optimize this, the bg color isn't rendering correctly because of the color offset
        DrawRect(data.visibleRect.x, data.visibleRect.y, data.visibleRect.w, data.visibleRect.h, bgColor, DrawMode.TilemapCache);
        
        data.canvas:DrawPixels( data.visibleRect.x, data.visibleRect.y, DrawMode.TilemapCache, data.zoom, -1, -1, data.colorOffset, NewRect(data.viewport.x + data.lastStartX, data.viewport.y + data.lastStartY, data.visibleRect.w, data.visibleRect.h), data.isolateSpriteColors)

        -- Reset the display invalidation
        data.invalidateDisplay = false

    end

    editorUI:UpdateSlider(data.vSlider)
    editorUI:UpdateSlider(data.hSlider)

    -- Clear dragging value if dragging is not enabled
    if(data.enableDragging == false) then
        data.dragging = false
        data.pressSelection = nil
    end

    if(data.picker.inFocus == true) then

        if(data.dragging == false and data.draggingSpriteID ~= nil) then

            data.draggingSpriteID = nil

        end

        data.overPos = self:CalculateItemPickerPosition(data)

        -- Reset copy drag flag
        data.copyDrag = false
        
        if(data.dragging == true) then

            -- See if control is down while dragging
            data.copyDrag = (Key(Keys.LeftControl) == true or Key(Keys.RightControl) == true) and data.ctrlCopyEnabled == true

            if(editorUI.collisionManager:MouseInRect(data.visibleRect) and data.overPos.index < data.total) then

                -- data.overPos = self:CalculateItemPickerPosition(data)
                data.overPos.x = data.visibleRect.x + (data.overPos.x * data.itemSize.x) - data.lastStartX
                data.overPos.y = data.visibleRect.y + (data.overPos.y * data.itemSize.y) - data.lastStartY
            else
                data.overPos.x = editorUI.collisionManager.mousePos.x
                data.overPos.y = editorUI.collisionManager.mousePos.y

                -- Offset the item so you can see it under the cursor
                -- local offset =  

                -- Apply offset
                data.overPos.x = data.overPos.x - ((data.scale.x * 8) * .5)
                data.overPos.y = data.overPos.y - ((data.scale.y * 8) * .35)

            end

            -- TODO this scale is off
            -- local tileSize = data.scale * 8

            if(data.emptyDrawArgs ~= nil) then
                -- Update the empty tile position
                data.emptyDrawArgs[2] = data.visibleRect.x + (data.pressSelection.x * data.itemSize.x) - data.lastStartX
                data.emptyDrawArgs[3] = data.visibleRect.y + (data.pressSelection.y * data.itemSize.y) - data.lastStartY

                -- Make sure that the empty sprites are inside the viewport before drawing them
                if(data.drawEmpty ~= false and data.viewport:Contains(NewPoint(data.emptyDrawArgs[2] - data.visibleRect.x, data.emptyDrawArgs[3] - data.visibleRect.y))) then

                    -- Draw empty tiles
                    editorUI:NewDraw("DrawSprites", data.emptyDrawArgs)

                end

            end

            data.scrollOverTime = data.scrollOverTime + editorUI.timeDelta

            if(data.scrollOverTime > data.scrollOverDelay) then
                -- See if the dragged item is inside of the other UI elements
                if(data.vSlider ~= nil and editorUI.collisionManager:MouseInRect(data.vSlider.rect) == true) then

                    editorUI:UpdateSliderPosition(data.vSlider)

                elseif(data.hSlider ~= nil and editorUI.collisionManager:MouseInRect(data.hSlider.rect) == true) then

                    editorUI:UpdateSliderPosition(data.hSlider)

                else
                    -- Reset page over flag
                    data.scrollOverLast = -1
                    data.scrollOverTime = 0
                end

            end

        else


            data.overPos = self:CalculateItemPickerPosition(data)


            -- Make sure we are following the mouse if we are not over an item
            if(data.overPos.index == -1) then
                data.overPos = editorUI.collisionManager.mousePos
            end

            data.overPos.x = data.rect.x + (data.overPos.x * data.itemSize.x) - data.lastStartX
            data.overPos.y = data.rect.y + (data.overPos.y * data.itemSize.y) - data.lastStartY
        end

        -- test to invalidate over sprite data
        -- local invalidPixelData = data.lastOverID ~= data.overPos.index

        -- Update the over selection
        data.picker.overPos.x = data.overPos.x - data.picker.borderOffset
        data.picker.overPos.y = data.overPos.y - data.picker.borderOffset

        -- Only update the sprite data when over a new sprite
        if(data.lastOverID ~= data.overPos.index) then

            local tmpX = data.pressSelection == nil and data.overPos.x - data.rect.x + data.lastStartX or data.pressSelection.x * data.itemSize.x
            local tmpY = data.pressSelection == nil and data.overPos.y - data.rect.y + data.lastStartY or data.pressSelection.y * data.itemSize.y

            data.lastOverID = data.overPos.index

            data.overItemDrawArgs[1] = data.onOverRender(data, tmpX, tmpY)

            data.overItemDrawArgs[4] = data.picker.itemWidth -- * 8
            data.overItemDrawArgs[5] = data.picker.itemHeight --* 8

        end

        data.overItemDrawArgs[2] = data.overPos.x
        data.overItemDrawArgs[3] = data.overPos.y


        -- data.maskSpriteDrawArgs[1] = data.overItemDrawArgs[2]
        -- data.maskSpriteDrawArgs[2] = data.overItemDrawArgs[3]
        -- data.maskSpriteDrawArgs[3] = data.overItemDrawArgs[4]
        -- data.maskSpriteDrawArgs[4] = data.overItemDrawArgs[5]

        local offscreen = Clamp(data.tmpItemRect.width / 2, 0, 8)

        if( data.drawOver ~= false and data.overPos.x > - offscreen and (data.overPos.x + data.tmpItemRect.width) < (data.displaySize.x + offscreen) and data.overPos.y > - offscreen and (data.overPos.y + data.tmpItemRect.height) < (data.displaySize.y + offscreen)) then
            -- -- Draw rect behind the over selection
            -- editorUI:NewDraw("DrawRect", data.maskSpriteDrawArgs)

            DrawRect(
                data.overPos.x,
                data.overPos.y,
                data.picker.itemWidth,
                data.picker.itemHeight,
                data.colorOffset - 1,
                DrawMode.Sprite
            )

            -- -- Draw selected sprites in the background
            -- editorUI:NewDraw("DrawPixels", data.overItemDrawArgs)

            -- -- Draw the selection box on top
            -- editorUI:NewDraw("DrawSprites", data.picker.overDrawArgs)

            DrawMetaSprite(
                data.picker.cachedMetaSpriteIds.over,
                data.picker.overPos.x,
                data.picker.overPos.y,
                false, 
                false, 
                DrawMode.Sprite, 
                1
            )

        end

        data.UpdateToolTip(data)

    end

end

function PixelVisionOS:ReadItemPickerOverPixelData(data, tmpX, tmpY)

    return data.canvas:GetPixels(tmpX, tmpY, data.picker.itemWidth, data.picker.itemHeight)

end

function PixelVisionOS:ChangeItemPickerColorOffset(data, value)

    -- data.colorOffset = value
    -- data.overItemDrawArgs[9] = value

    -- self:InvalidateItemPickerDisplay(data)
end

function PixelVisionOS:UpdateItemPickerSelection(data)

    local spriteSize = data.itemSize.x
    local cols = data.visibleColumns

    -- Get the current position of the selection
    local tmpPos = CalculatePosition(data.currentSelection, data.columns)

    -- Offset the position
    data.tmpItemRect.x = (tmpPos.x * spriteSize) - data.lastStartX
    data.tmpItemRect.y = (tmpPos.y * spriteSize) - data.lastStartY

    -- Quick test to see if the selection is inside of the viewport
    if(data.viewport.Contains(data.tmpItemRect)) then

        -- Adjust for scale
        -- spriteSize = spriteSize * data.scale.x
        cols = cols / data.scale.x

        -- Find the new index value for the picker
        local newIndex = CalculateIndex(data.tmpItemRect.x / (spriteSize * data.scale.x), data.tmpItemRect.y / (spriteSize * data.scale.y), cols)

        editorUI:SelectPicker(data.picker, newIndex, false)

    else
        editorUI:ClearPickerSelection(data.picker)
    end

end

function PixelVisionOS:CalculateItemPickerPosition(data, x, y)

    x = x or editorUI.collisionManager.mousePos.x
    y = y or editorUI.collisionManager.mousePos.y

    local position = 
    {
        x = math.floor((x - data.rect.x) / data.itemSize.x),
        y = math.floor((y - data.rect.y) / data.itemSize.y),
    }

    if(editorUI.collisionManager:MouseInRect(data.visibleRect)) then

        -- Offset for scroll
        position.x = position.x + (data.lastStartX / data.itemSize.x)
        position.y = position.y + (data.lastStartY / data.itemSize.y)

        -- Snap position to the grid
        position.x = math.floor(position.x / data.scale.x) * data.scale.x
        position.y = math.floor(position.y / data.scale.y) * data.scale.y

        position.index = math.index(position.x, position.y, data.columns)
    else

        position.x = -1
        position.y = -1
        position.index = -1

    end

    return position

end

function PixelVisionOS:ChangeItemPickerScale(data, scale, selection)

    if(data == nil) then
        return
    end

    -- print("selection", dump(selection))

    data.scale = selection--{x = scale, y = scale}

    --local tmpItemSize = scale * data.itemSize

    -- Resize picker
    data.picker.itemWidth = selection.x * data.itemSize.x
    data.picker.itemHeight = selection.y * data.itemSize.y
    data.picker.columns = math.floor(data.picker.rect.w / data.picker.itemWidth)
    data.picker.rows = math.floor(data.picker.rect.h / data.picker.itemHeight)

    -- print(data.name, "spriteName", data.picker.spriteName)
    -- if(_G[data.picker.spriteName .. "over"] ~= nil) then
    --     data.picker.overDrawArgs[1] = _G[data.picker.spriteName .. "over"].spriteIDs
    --     data.picker.overDrawArgs[4] = _G[data.picker.spriteName .. "over"].width
    -- end

    -- if(_G[data.picker.spriteName .. "over"] ~= nil) then
    --     data.picker.selectedDrawArgs[1] = _G[data.picker.spriteName .. "selectedup"].spriteIDs
    --     data.picker.selectedDrawArgs[4] = _G[data.picker.spriteName .. "selectedup"].width
    -- end

    data.tmpItemRect.width = data.picker.itemWidth
    data.tmpItemRect.height = data.picker.itemHeight

    if(_G["emptycolor"] ~= nil) then
        -- Need to resize empty sprite block
        local emptySpriteList = {}
        local totalSprites = data.scale.x * data.scale.y

        for i = 1, totalSprites do
            table.insert(emptySpriteList, _G["emptycolor"].spriteIDs[1])
        end

        data.emptyDrawArgs[1] = emptySpriteList
        data.emptyDrawArgs[4] = data.scale.x
    end

    -- Need to update the current selection if it's not set to -1
    if(data.currentSelection > - 1) then

        self:SelectItemPickerIndex(data, data.currentSelection, false, true)

    end
end

-- Sets all the pixel data in the item picker canvas
function PixelVisionOS:SetItemPickerPixelData(data, pixelData)

    data.canvas:SetPixels(pixelData)

    data.invalidateDisplay = true

end

-- Changes the pixel data at a specific item's position in the canvas
function PixelVisionOS:UpdateItemPickerPixelDataAt(data, index, pixelData, width, height)

    local pos = CalculatePosition(index, data.columns)

    pos.x = pos.x * 8
    pos.y = pos.y * 8

    data.canvas:Clear(-1, pos.x, pos.y, width, height)

    data.canvas:SetPixels(pos.x, pos.y, width, height, pixelData)
    data.invalidateDisplay = true

end

function PixelVisionOS:ClearItemPickerSelection(data)
    data.currentSelection = -1
    editorUI:ClearPickerSelection(data.picker)

end

function PixelVisionOS:EnableItemPicker (data, value)
    
    data.enabled = value
    
    -- Set the flag on the picker
    editorUI:Enable(data.picker, value)

end

function PixelVisionOS:GenerateImage(data)

    local colors = gameEditor:Colors()

    local pixelData = data.canvas:GetPixels()

    return NewImage(data.realWidth, data.realHeight, colors, pixelData)

end

function PixelVisionOS:InvalidateItemPickerPageButton(data)

    -- print(dump(data.pages))
    for i = 1, #data.pages.buttons do
        editorUI:Invalidate(data.pages.buttons[i])
    end
end
