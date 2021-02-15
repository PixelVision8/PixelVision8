--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

local palettePanelID = "PalettePanelUI"

function DrawTool:CreatePalettePanel()

    -- local totalColors = pixelVisionOS.totalSystemColors
    -- local totalPerPage = 16--pixelVisionOS.systemColorsPerPage
    -- local maxPages = 16
    -- self.colorOffset = pixelVisionOS.colorOffset

    self.paletteLabelArgs = {gamecolortext.spriteIDs, 4, 21, gamecolortext.width, false, false, DrawMode.Tile}
    
    -- Create the palette color picker
    self.paletteColorPickerData = pixelVisionOS:CreateColorPicker(
        {x = 32, y = 184, w = 128, h = 32},
        {x = 16, y = 16},
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

    pixelVisionOS:ColorPickerVisiblePerPage(self.paletteColorPickerData, pixelVisionOS.colorsPerSprite)

    self.paletteColorPickerData.picker.borderOffset = 8

    -- TODO this shouldn't have to be called?
    pixelVisionOS:RebuildColorPickerCache(self.paletteColorPickerData)

    self.paletteColorPickerData.UpdateToolTip = function(tmpData)

        -- print("Update Tool Tip", tmpData.name, tmpData.dragging)
        local action = ""
        local ending = ""
        local pos = 0
        local page = tmpData.pages.currentSelection - 1
        
        -- print("page", self.paletteColorPickerData.picker.overPos.index)
        local toolTipMessage = "%s color %02d from palette %d %s"
        
        if(tmpData.dragging) then

            local shiftColor = page * 16
            local palIndex = tmpData.overPos.index - shiftColor

            -- print("tmpData.overPos.index", )
            if(palIndex > -1 and palIndex < tmpData.visiblePerPage) then

                action = tmpData.copyDrag == true and "Copy" or "Swap"
               
                -- index = palIndex
                ending = string.format("%s color %02d in palette %d", (tmpData.copyDrag == true and "to" or "with"), tostring(palIndex), page)
                pos = tmpData.picker.selected

            elseif(editorUI.collisionManager:MouseInRect(self.spritePickerData.rect)) then

                -- This is a special use case where we don't want to use the default tool tip message
                tmpData.picker.toolTip = string.format("Preview color offset %03d", (page * 16) + tmpData.picker.selected + 128)
                
                -- -- Exit this since we already set the tooltip value
                return

            else
            
                action = "Dragging"
                index = tmpData.pressSelection.index
                pos = tmpData.picker.selected
                -- tmpData.picker.toolTip = "Dragging "..tmpData.toolTipLabel.." ID " .. string.lpad(tostring(tmpData.pressSelection.index), tmpData.totalItemStringPadding, "0")
            
            end

        elseif(tmpData.overPos.index ~= nil and tmpData.overPos.index ~= -1) then

            action = "Select"
            index = tmpData.overPos.index
            pos = editorUI:CalculatePickerPosition(tmpData.picker).index

        else
            toolTipMessage = ""
        end

        -- This is hard coded to add 128 to the index since that is where palette IDs begin in the re-mapped color chip
        tmpData.picker.toolTip = string.format(toolTipMessage, action, pos, page, ending)
        -- print ("New Tool Tip", tmpData.picker.name, tmpData.picker.toolTip)
    end

    -- print("paletteColorPickerData")
    self.paletteColorPickerData.onAction = function(value, doubleClick)

        if(doubleClick == true and self.canEdit == true) then

            -- editorUI:ToggleButton(self.modeButton, not self.modeButton.value)
            self:ChangeEditMode(ColorMode)

            local colorID = table.indexOf(pixelVisionOS.systemColors, Color(pixelVisionOS.colorOffset + 128 + value)) - 1 

            print("Focus Pal")
            self:ForcePickerFocus(self.systemColorPickerData)

            self:OnSelectSystemColor(colorID)

            pixelVisionOS:SelectColorPickerIndex(self.systemColorPickerData, colorID)
            
            -- TODO find and select color in picker
            return
        end

        print("paletteColorPickerData.onAction", value)

        self:OnSelectPaletteColor(value)

        self:ForcePickerFocus(self.paletteColorPickerData)

    end

    self.paletteColorPickerData.onDropTarget = function(src, dest) self:OnPalettePickerDrop(src, dest) end
    
    -- if(self.usePalettes == true) then
        -- Force the palette picker to only display the total colors per sprite
    self.paletteColorPickerData.visiblePerPage = pixelVisionOS.paletteColorsPerPage

    pixelVisionOS:OnColorPickerPage(self.paletteColorPickerData, 1)
    -- else

    -- end

    -- Wire up the picker to change the color offset of the sprite picker
    self.paletteColorPickerData.onPageAction = function(value)

       -- Calculate page offset value
        local pageOffset = ((value - 1) * 16)

        -- Calculate the new color offset
        local newColorOffset = pixelVisionOS.colorOffset + pixelVisionOS.totalPaletteColors + pageOffset

        -- Change the spite picker color offset
        pixelVisionOS:ChangeItemPickerColorOffset(self.spritePickerData, newColorOffset)

        -- Update the canvas color offset
        self.canvasData.colorOffset = newColorOffset

        pixelVisionOS:InvalidateItemPickerDisplay(self.spritePickerData)

        self:UpdateCanvas(self.lastSelection)

        -- Need to reselect the current color in the new palette if we are in draw mode
        if(self.canvasData.tool ~= "eraser" and self.canvasData.tool ~= "select") then

            local colorID = (self.lastPaletteColorID % 16) + pageOffset

            -- print("colorID", colorID, self.paletteColorPickerData.picker.selected, pageOffset, self.lastPaletteColorID % 16)
            if(self.paletteColorPickerData.currentSelection ~= colorID) then
                pixelVisionOS:SelectItemPickerIndex(self.paletteColorPickerData, colorID, false, false)
                self:OnSelectPaletteColor(colorID)
            end
            
        end

    end

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

    self.paletteColorPickerData.onChange = function(index, color)
    
        Color(index, color)

    end

    self:DrawPalettePanelLabel()

    pixelVisionOS:RegisterUI({name = palettePanelID}, "UpdatePalettePanel", self)

end

function DrawTool:OnSelectPaletteColor(value)

    print("OnSelectPaletteColor", value)

    -- print("OnSelectPaletteColor", value)

    
        
    self.lastPaletteColorID = value
    
    -- -- Force value to be in palette mode
    value = self.paletteColorPickerData.picker.selected

    -- -- print("Saving last pal", self.lastPaletteColorID)
    

    --     -- Set the canvas brush color
    pixelVisionOS:CanvasBrushColor(self.canvasData, value)

end

function DrawTool:DrawPalettePanelLabel()

    -- if(self.usePalettes == true) then
        
        self.paletteLabelArgs[1] = gamepalettetext.spriteIDs
        self.paletteLabelArgs[4] = gamepalettetext.width

    -- else

        -- self.paletteLabelArgs[1] = gamecolortext.spriteIDs
        -- self.paletteLabelArgs[4] = gamecolortext.width

    -- end

    editorUI:NewDraw("DrawSprites", self.paletteLabelArgs)
end

function DrawTool:UpdatePalettePanel()

    pixelVisionOS:UpdateColorPicker(self.paletteColorPickerData)

    if(self.canvasData.tool == "eyedropper" and self.canvasData.inFocus and MouseButton(0)) then

        local colorID = self.canvasData.overColor

        -- Only update the color selection when it's new
        if(colorID ~= self.lastColorID) then

            self.lastColorID = colorID

            if(colorID < 0) then

                pixelVisionOS:ClearItemPickerSelection(self.paletteColorPickerData)

                -- Force the lastColorID to be back in range so there is a color to draw with
                self.lastColorID = -1

            else

                pixelVisionOS:CanvasBrushColor(self.canvasData, self.lastColorID)

                local selectionID = lastColorID

                -- Check to see if in palette mode
                -- if(usePalettes == true) then
                local pageOffset = ((self.paletteColorPickerData.pages.currentSelection - 1) * 16)

                selectionID = Clamp(self.lastColorID, 0, 15) + pageOffset
                    -- pixelVisionOS:SelectColorPickerColor(paletteColorPickerData, Clamp(lastColorID, 0, 15) + pageOffset)
                -- end
                -- else
                pixelVisionOS:SelectColorPickerColor(self.paletteColorPickerData, selectionID)

                -- end


                -- Select the


            end

        end

    end

end

function DrawTool:OnPalettePickerDrop(src, dest)
    
    if(src.name == self.systemColorPickerData.name) then

        -- Get the index and add 1 to offset it correctly
        local id = pixelVisionOS:CalculateItemPickerPosition(dest).index

        -- Get the correct hex value
        local srcHex = Color(src.pressSelection.index + src.altColorOffset)

        self:OnAddDroppedColor(id, dest, srcHex)
        
    else
        -- Test that the destination position is visible
        if(editorUI:CalculatePickerPosition(dest.picker).index < dest.visiblePerPage) then
            self:OnSystemColorDropTarget(src, dest)
        end

    end

end
