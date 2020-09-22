--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

local colorPanelID = "ColorPanelUI"

-- Now we need to create the item picker over sprite by using the color selection spriteIDs and changing the color offset
_G["itempickerover"] = {spriteIDs = selection2x2over.spriteIDs, width = selection2x2over.width, colorOffset = 0}

-- Next we need to create the item picker selected up sprite by using the color selection spriteIDs and changing the color offset
_G["itempickerselectedup"] = {spriteIDs = selection2x2selected.spriteIDs, width = selection2x2selected.width, colorOffset = 0}

function DrawTool:CreateColorPanel()

    -- Create the system color picker
    self.systemColorPickerData = pixelVisionOS:CreateColorPicker(
        {x = 32, y = 32, w = 128, h = 128}, -- Rect
        {x = 16, y = 16}, -- Tile size
        pixelVisionOS.totalSystemColors, -- Total colors, plus 1 for empty transparency color
        pixelVisionOS.systemColorsPerPage, -- total per page
        4, -- max pages
        pixelVisionOS.colorOffset, -- Color offset to start reading from
        "itempicker", -- Selection sprite name
        "System Color", -- Tool tip
        false, -- Modify pages
        true, -- Enable dragging,
        true -- drag between pages
    )

    self.bgDrawArgs =
    {
        bgflagicon.spriteIDs,
        0,
        0,
        bgflagicon.width,
        false,
        false,
        DrawMode.Sprite
    }

    self.systemColorPickerData.ctrlCopyEnabled = false
    
    self.systemColorPickerData.UpdateToolTip = function(tmpData)

        local action = ""
        local label = tmpData.toolTipLabel
        local index = 0
        local ending = ""
        
        
        local toolTipMessage = "%s %s %03d %s"

        if(self.changingColorIndex == false) then 

            -- TODO need to figure out which index it is over and change this accordingly
            label = "mapped index"

        end
        
        if(tmpData.dragging) then

            if(tmpData.overPos.index ~= nil and tmpData.overPos.index ~= -1 and tmpData.overPos.index < tmpData.total) then

                action = tmpData.copyDrag == true and "Copy" or "Swap"
               
                index = tmpData.pressSelection.index
                ending = string.format("%s color ID %03d", (tmpData.copyDrag == true and "to" or "with"), tmpData.overPos.index)

                -- ending = (tmpData.copyDrag == true and "to" or "with") .. " ".. string.lpad(tostring(tmpData.overPos.index), tmpData.totalItemStringPadding, "0")

                -- tmpData.picker.toolTip = string.format(toolTipMessage, "swap", tmpData.toolTipLabel, tmpData.pressSelection.index, "with ".. string.lpad(tostring(tmpData.overPos.index), tmpData.totalItemStringPadding, "0"))--"Swap "..tmpData.toolTipLabel.." ID " .. string.lpad(tostring(tmpData.pressSelection.index), tmpData.totalItemStringPadding, "0") .. " with ".. string.lpad(tostring(tmpData.overPos.index), tmpData.totalItemStringPadding, "0")
            elseif(editorUI.collisionManager:MouseInRect(self.paletteColorPickerData.rect)) then

                local page = self.paletteColorPickerData.pages.currentSelection - 1
                local shiftColor = page * 16
                index = tmpData.pressSelection.index
                -- print("self.paletteColorPickerData.overPos", self.paletteColorPickerData.overPos)

                local palIndex = pixelVisionOS:CalculateItemPickerPosition(self.paletteColorPickerData).index - shiftColor

                if(palIndex >= self.paletteColorPickerData.visiblePerPage) then
                    action = "Dragging"
                    
                else
                    -- This is a special use case where we don't want to use the default tool tip message
                    tmpData.picker.toolTip = string.format("Copy color ID %03d to color %02d in palette %0d", index, palIndex, page) --string.format("Preview color offset %03d", (page * 16) + tmpData.picker.selected + 128)
                    
                    -- -- Exit this since we already set the tooltip value
                    return
                end
            elseif(editorUI.collisionManager:MouseInRect(self.spritePickerData.rect)) then

                -- This is a special use case where we don't want to use the default tool tip message
                tmpData.picker.toolTip = string.format("Preview color offset %03d", tmpData.pressSelection.index)
                
                -- -- Exit this since we already set the tooltip value
                return
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

    self.systemColorPickerData.onPageAction = function(value)

        local bgColor = gameEditor:BackgroundColor()
        local bgPageID = math.ceil((bgColor + 1) / 64)

        self.showBGIcon = bgPageID == value

        self:UpdateBGIconPosition(bgColor)

    end

    self.systemColorPickerData.picker.borderOffset = 8

    -- Force the BG color to draw for the first time
    self.systemColorPickerData.onPageAction(1)

    -- Create a function to handle what happens when a color is dropped onto the system color picker
    self.systemColorPickerData.onDropTarget = function(src, dest) self:OnSystemColorDropTarget(src, dest) end

    -- Capture double click action to trigger edit
    self.systemColorPickerData.onAction = function(value, doubleClick)

        -- TODO need to look into how this is actually getting the doubleClick value
        if(doubleClick == true) then

            self:OnEditColor()

        end

    end

    -- Manage what happens when a color is selected
    self.systemColorPickerData.onRelease = function(value) self:OnSelectSystemColor(value) end

    -- Change the color in the tool
    self.systemColorPickerData.onChange = function(index, color) Color(index, color) end

    self.systemColorPickerData.onDrawColor = function(data, id, x, y)

        if(id < data.total and (id % data.totalPerPage) < data.visiblePerPage) then
            local colorID = id + data.altColorOffset

            if(Color(colorID) == self.maskColor) then
            data.canvas.DrawSprites(emptymaskcolor.spriteIDs, x, y, emptymaskcolor.width, false, false)
            else
            data.canvas.Clear(colorID, x, y, data.itemSize.x, data.itemSize.y)
            end
            
        else

            data.canvas.DrawSprites(emptycolor.spriteIDs, x, y, emptycolor.width, false, false)
            
        end

    end

end

function DrawTool:OnSelectSystemColor(value)

    -- print("SELECT COLOR", value)
    self.lastSystemColorID = value

    -- Change the focus of the current color picker
    self:ForcePickerFocus(self.systemColorPickerData)


end


function DrawTool:ShowColorPanel()
    -- print("Show")
    if(self.colorPanelActive == true) then
        return
    end

    self.colorPanelActive = true

    self:RefreshBGColorIcon()

    pixelVisionOS:RegisterUI({name = colorPanelID}, "UpdateColorPanel", self)

    pixelVisionOS:InvalidateItemPickerDisplay(self.systemColorPickerData)
    pixelVisionOS:InvalidateItemPickerPageButton(self.systemColorPickerData)
    -- TODO clear page area?

    editorUI:NewDraw("DrawSprites", {pickerbottompageedge.spriteIDs, 20, 20, pickerbottompageedge.width, false, false,  DrawMode.Tile})


end

function DrawTool:HideColorPanel()

    if(self.colorPanelActive == false) then
        return
    end

    self.colorPanelActive = false

    pixelVisionOS:RemoveUI(colorPanelID)

    -- Clear bottom of the main window
    for i = 1, 8 do
        editorUI:NewDraw("DrawSprites", {pagebuttonempty.spriteIDs, 11 + i, 20, pagebuttonempty.width, false, false,  DrawMode.Tile})
    end

    editorUI:NewDraw("DrawSprites", {canvasbottomrightcorner.spriteIDs, 20, 20, canvasbottomrightcorner.width, false, false,  DrawMode.Tile})


end

function DrawTool:UpdateColorPanel()

    pixelVisionOS:UpdateColorPicker(self.systemColorPickerData)

    -- Reset the render flag if dragging normally 
    if(self.systemColorPickerData.dragging == true) then
        self.systemColorPickerData.drawOver = true
        self.systemColorPickerData.drawEmpty = true
    end
    
    if(self.selectionMode == SystemColorMode and (self.changingColorIndex == true or Key(Keys.LeftAlt, InputState.Down) or Key(Keys.RightAlt, InputState.Down))) then

        if(self.colorMap == nil) then

            local title = "Error"
            local message = "The sprites were not analyzed so you will not be able to remap the color indexes. If you'd like to use this feature, you will need to restart the tool and hold down the CTRL key until the analyze sprite message appears."
            pixelVisionOS:ShowMessageModal(title, message, 164)
        else

            -- TODO set the sprite picker color offset to 0

            -- print("Dragging", self.systemColorPickerData.dragging)

            self.changingColorIndex = false

            self.colorLayerTime = self.colorLayerTime + (editorUI.timeDelta * 1000)

            if( self.colorLayerTime > self.colorLayerDelay) then

                self.colorLayerTime = 0

                self.showIsolatedColor = not self.showIsolatedColor

                local baseColorIDs = {}
                -- print("Selected", self.systemColorPickerData.currentSelection)
                -- local currentSelected = 0--self.systemColorPickerData.pressSelection.index
                for i = 1, #self.colorMap do
                    if(self.colorMap[i] ~= self.systemColorPickerData.currentSelection) then
                        table.insert(baseColorIDs, self.colorMap[i])
                    end
                end

                self.spritePickerData.isolateSpriteColors = self.showIsolatedColor == false and self.colorMap or baseColorIDs

                pixelVisionOS:InvalidateItemPickerDisplay(self.spritePickerData)

            end

            -- if(self.systemColorPickerData.dragging == true) then
            --     print("pressSelection", dump(self.systemColorPickerData.pressSelection))
            -- end

            

            -- Display color ID numbers
            for i = 1, #self.colorMap do
                local index =  i - 1
                
                -- TODO need to cache this?
                local pos = CalculatePosition(self.colorMap[i], 8)
                local drawMode = DrawMode.Sprite

                if(self.systemColorPickerData.pressSelection ~= nil and self.systemColorPickerData.pressSelection.index == self.colorMap[i])  then
                    
                    self.changingColorIndex = true
                    
                    pos.x = self.systemColorPickerData.overItemDrawArgs[2]
                    pos.y = self.systemColorPickerData.overItemDrawArgs[3]

                    self.systemColorPickerData.drawOver = false
                    self.systemColorPickerData.drawEmpty = false
                    -- drawMode = DrawMode.SpriteAbove
                    editorUI:NewDraw("DrawSprites", self.systemColorPickerData.picker.overDrawArgs)
                    
                else

                    

                    -- shift the position
                    pos.x = (pos.x * self.systemColorPickerData.itemSize.x) + self.systemColorPickerData.rect.x
                    pos.y = (pos.y * self.systemColorPickerData.itemSize.y) + self.systemColorPickerData.rect.y

                end

                local spriteData = _G["colorindex"..index]
                -- DrawText( text, x, y, DrawMode, font, colorOffset, spacing )
                -- editorUI:NewDraw("DrawText", {tostring(index), pos.x, pos.y, drawMode, "small", 15})
                editorUI:NewDraw("DrawSprites", {spriteData.spriteIDs, pos.x, pos.y, spriteData.width, false, false, drawMode})
                
            end
        end



    else
        if(self.showIsolatedColor == true) then
            self.spritePickerData.isolateSpriteColors = nil

            pixelVisionOS:InvalidateItemPickerDisplay(self.spritePickerData)

            self.showIsolatedColor= false
        end

        if(self.showBGIcon == true) then
            editorUI:NewDraw("DrawSprites", self.bgDrawArgs)
        end

    end

end

function DrawTool:UpdateHexColor(value)

    if(self.selectionMode == PaletteMode) then
        return false
    end

    value = "#".. value


    if(value == pixelVisionOS.maskColor) then

        -- TODO this are is throwing an error
        -- print("MASK COLOR ERROR")

        local title = self.toolName .." Error"
        local message = "This color is reserved for transparency and can not be assigned to a system color."

        pixelVisionOS:ShowMessageModal(title, message, 160, false)

        return false

    else

        local realColorID = self.systemColorPickerData.currentSelection + self.systemColorPickerData.altColorOffset

        local currentColor = Color(realColorID)

        -- Make sure the color isn't duplicated when in palette mode
        for i = 1, 128 do

            -- Test the new color against all of the existing system colors
            if(value == Color(pixelVisionOS.colorOffset + (i - 1))) then

                -- TODO need to move this to the config at the top
                pixelVisionOS:ShowMessageModal(self.toolName .." Error", "'".. value .."' the same as system color ".. (i - 1) ..", enter a new color.", 160, false)

                -- Exit out of the update function
                return false

            end

        end

        -- Test if the color is at the end of the picker and the is room to add a new color
        if(self.colorID == self.systemColorPickerData.total - 1 and self.systemColorPickerData.total < 255) then

            -- Select the current color we are editing
            pixelVisionOS:SelectColorPickerColor(self.systemColorPickerData, realColorID)

        end

        -- Update the editor's color
        pixelVisionOS:ColorPickerChangeColor(self.systemColorPickerData, realColorID, value)

        -- Loop through the palette color memory to remove replace all matching colors
        for i = 127, pixelVisionOS.totalColors do

            local index = (i - 1) + pixelVisionOS.colorOffset

            -- Get the current color in the tool's memory
            local tmpColor = Color(index)

            -- See if that color matches the old color
            if(tmpColor == currentColor and tmpColor ~= pixelVisionOS.maskColor) then

                -- Set the color to equal the new color
                pixelVisionOS:ColorPickerChangeColor(self.paletteColorPickerData, index, value)

            end

        end

        self:InvalidateData()

        return true
    end

end

function DrawTool:OnSystemColorDropTarget(src, dest)

    -- If the src and the dest are the same, we want to swap colors
    if(src.name == dest.name) then

        -- Get the source color ID
        local srcPos = src.pressSelection

        -- Get the destination color ID
        local destPos = pixelVisionOS:CalculateItemPickerPosition(src)

        -- Need to shift src and dest ids based on the color offset
        local realSrcID = srcPos.index + src.altColorOffset
        local realDestID = destPos.index + dest.altColorOffset

        if(self.changingColorIndex == true) then

            local colorIndex = table.indexOf(self.colorMap, srcPos.index)

            print("Change index", colorIndex, "to", destPos.index)

            -- Reset the flag
            self.changingColorIndex = false

            -- TODO need to be able to merge colors (make 3 and 5 into 3 for example)
            -- TODO need to be able to swap the colors
            if(colorIndex > 0 and table.indexOf(self.colorMap, destPos.index) == -1) then

                local title = "Warning"
                local message = "You are about to set the color index at %02d to %02d. This will require re-indexing all of the sprites.%s\n\nDo you want to do this?"

                local warningMessage = ""

                if(destPos.index > pixelVisionOS.colorsPerSprite) then
                    warningMessage = "\n\nBy setting the color index greater than the number of colors a sprite can support, it may cause errors when changing palettes."
                end

                pixelVisionOS:ShowMessageModal(title, string.format(message, self.colorMap[colorIndex], destPos.index, warningMessage), 216, true, 
                function ()

                    if(pixelVisionOS.messageModal.selectionValue == true) then

                        self:ReIndexSprites(self.colorMap[colorIndex], destPos.index)

                        self.colorMap[colorIndex] = destPos.index

                    end

                    -- TODO Reindex the sprites

                end
            
            )
                
            end

            -- print("New map", dump(self.colorMap))

        else

            print("realSrcID", realSrcID, "realDestID", realDestID)

            -- Make sure the colors are not the same
            if(realSrcID ~= realDestID and destPos.index < dest.total) then

                -- Get the src and dest color hex value
                local srcColor = Color(realSrcID)
                local destColor = src.copyDrag == true and srcColor or Color(realDestID)
                
                if(Key(Keys.LeftControl) == true or Key(Keys.RightControl) == true) then
                    -- print("Copy")
                end

                -- Make sure we are not moving a transparent color
                if(srcColor == pixelVisionOS.maskColor or destColor == pixelVisionOS.maskColor) then

                    if(self.usePalettes == true and dest.name == self.systemColorPickerData.name) then

                        pixelVisionOS:ShowMessageModal(self.toolName .." Error", "You can not replace the last color which is reserved for transparency.", 160, false)

                        return

                    end

                end

                self:BeginUndo()
                
                -- Swap the colors in the tool's color memory
                pixelVisionOS:ColorPickerChangeColor(dest, realSrcID, destColor)
                pixelVisionOS:ColorPickerChangeColor(dest, realDestID, srcColor)

                -- Update just the colors that changed
                local srcPixelData = pixelVisionOS:ReadItemPickerOverPixelData(src, srcPos.x, srcPos.y)

                local destPixelData = pixelVisionOS:ReadItemPickerOverPixelData(dest, destPos.x, destPos.y)

                src.canvas.SetPixels(srcPos.x, srcPos.y, src.itemSize.x, src.itemSize.y, srcPixelData)

                pixelVisionOS:InvalidateItemPickerDisplay(src)

                src.canvas.SetPixels(destPos.x, destPos.y, dest.itemSize.x, dest.itemSize.y, destPixelData)
                -- Redraw the color page
                pixelVisionOS:InvalidateItemPickerDisplay(dest)

                pixelVisionOS:DisplayMessage("Color ID '"..realSrcID.."' was swapped with Color ID '"..realDestID .."'", 5)

                self:EndUndo()

                -- Invalidate the data so the tool can save
                self:InvalidateData()

            end
        end

    end

end

function DrawTool:RefreshBGColorIcon()
    
    -- Correct the BG color if needed
    if(gameEditor:BackgroundColor() >= pixelVisionOS.totalSystemColors) then

        gameEditor:BackgroundColor(pixelVisionOS.totalSystemColors - 1)

    end

    self:UpdateBGIconPosition(gameEditor:BackgroundColor())

end

function DrawTool:UpdateBGIconPosition(id)

    local pos = CalculatePosition(id % 64, 8)

    self.bgDrawArgs[2] = self.systemColorPickerData.rect.x + (pos.x * 16)
    self.bgDrawArgs[3] = self.systemColorPickerData.rect.y + (pos.y * 16)

end