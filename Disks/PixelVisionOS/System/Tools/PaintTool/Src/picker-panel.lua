--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

ColorMode, SpriteMode, FlagMode = "color", "sprite", "flag"

function PaintTool:CreatePickerPanel()

    self.pickerPanel = {
        name = "pickerPanel"
    }

    self.pickerGridPos = NewPoint()
    

    -- self.currentState = {
    --     selectedPos = {id = -1},
    --     selectedId = -1,
    --     overPos = {id = -1},
    -- }
    
    -- Used to sample pixel data from the active canvas
    self.pickerSampleRect = NewRect( 0, 0, 8, 8 )

    -- self.currentState.pickerTotal = 0

    self.pickerPanelRect = NewRect(112-2, 22+5, 128, 16)
    
    self.pickerSampleArea = NewRect( 0, 0, self.pickerPanelRect.Width, self.pickerPanelRect.Height )
    
    self.currentCanvas = nil

    self.totalPages = 0
    self.currentPage = 0

    self.emptyPatternPixelData = Sprite(MetaSprite( "emptycolor" ).Sprites[1].Id)

    self.colorPixelData = {}
    
    for i = 1, 8*8 do
        table.insert(self.colorPixelData, -1)
    end

    self.colorCanvas = NewCanvas(128, 128) -- TODO need to account for missing colors and color offset
    self.spriteCanvas = NewCanvas(128, 1024)
    self.flagCanvas = NewCanvas(128, 16)


    self.pickerOverCanvas = NewCanvas( 12, 12 )
    self.pickerOverCanvas:Clear()
    self.pickerOverCanvas:SetStroke(0, 1);
    self.pickerOverCanvas:DrawRectangle( 0, 0, self.pickerOverCanvas.width, self.pickerOverCanvas.height)
    self.pickerOverCanvas:DrawRectangle( 2, 2, self.pickerOverCanvas.width - 4, self.pickerOverCanvas.height -4)
    self.pickerOverCanvas:SetStroke(15, 1);
    self.pickerOverCanvas:DrawRectangle( 1, 1, self.pickerOverCanvas.width - 2, self.pickerOverCanvas.height -2)

    self.pickerSelectedCanvas = NewCanvas( 12, 12 )
    self.pickerSelectedCanvas:SetPixels(self.pickerOverCanvas:GetPixels())
    self.pickerSelectedCanvas:SetStroke(14, 1);
    self.pickerSelectedCanvas:DrawRectangle( 1, 1, self.pickerOverCanvas.width - 2, self.pickerOverCanvas.height -2)


    self.backButton = editorUI:CreateButton({x = self.pickerPanelRect.X - 10, y = self.pickerPanelRect.Y-6}, "stepperback", "This is the picker back button.")

    self.backButton.onAction = function() self:OnPickerBack() end


    self.nextButton = editorUI:CreateButton({x = self.pickerPanelRect.X + self.pickerPanelRect.Width + 2, y = self.pickerPanelRect.Y-6}, "steppernext", "This is the picker next button.")

    self.nextButton.onAction = function() self:OnPickerNext() end

    self.pickerModes = {ColorMode, SpriteMode, FlagMode}
    self.totalModes = #self.pickerModes

    -- We set the mode to the last value since we call OnNextMode below and it will increase the mode by 1 and wrap around
    self.pickerMode = self.pickerModes[self.totalModes] -- TODO need to make this load the last mode
    
    -- Create states
    for i = 1, self.totalModes do

        self.pickerPanel[self.pickerModes[i] .. "State"] ={
            selectedPos = {id = -1},
            selectedId = 0,
            overPos = {id = -1},
            pickerTotal = 0,
            pickerLabel = self.pickerModes[i]:upper()
        }
        
    end

    -- Create size button
    self.modeButton = editorUI:CreateButton({x = 80 - 7, y = 16 + 7}, "colormode", "Change mode")
    self.modeButton.onAction = function() self:OnNextMode() end


    self:OnNextMode()

    

    pixelVisionOS:RegisterUI({name = "OnUpdatePickerPanel"}, "UpdatePickerPanel", self)
    pixelVisionOS:RegisterUI({name = "OnDrawPickerPanel"}, "DrawPickerPanel", self)

end

function PaintTool:UpdatePickerPanel(timeDelta)

    local overrideFocus = (self.pickerPanel.inFocus == true and editorUI.collisionManager.mouseDown)

    local inRect = self.pickerPanelRect.Contains(editorUI.mouseCursor.pos) == true
    
    if(inRect or overrideFocus) then

        if(self.pickerPanel.inFocus ~= true and editorUI.inFocusUI == nil) then
            editorUI:SetFocus(self.pickerPanel, self.focusCursor)
        end

        if(self.pickerPanel.inFocus == true) then

            if(inRect and editorUI.collisionManager.mouseDown == false) then

                -- Calculate the over index id
                local tmpId = CalculateIndex( 
                    math.floor((editorUI.mouseCursor.pos.X - self.pickerPanelRect.X)/8),
                    math.floor((editorUI.mouseCursor.pos.Y - self.pickerPanelRect.Y)/8) + (2 * self.currentPage),
                    16
                )

                if(tmpId >= self.currentState.pickerTotal or tmpId == self.currentState.selectedId) then

                    self:ClearPickerFocus()

                else

                    -- Test to see if the over id has changed
                    if(self.currentState.overPos.id ~= tmpId) then

                        -- save the over position
                        self.currentState.overPos.pos = CalculatePosition( tmpId, 16 )

                        -- Calculate the current page
                        self.currentState.overPos.page = math.floor(self.currentState.overPos.pos.Y / 2)

                        -- Cache the over position
                        self.currentState.overPos.id = tmpId
                        self.currentState.overPos.X = self.currentState.overPos.pos.X * 8
                        self.currentState.overPos.Y = (self.currentState.overPos.pos.Y - self.currentState.overPos.page * 2) * 8

                    end

                    self.pickerColumn = math.floor((editorUI.mouseCursor.pos.X - self.pickerPanelRect.X)/8)
                    self.pickerRow = math.floor((editorUI.mouseCursor.pos.Y - self.pickerPanelRect.Y)/8)

                    -- self.pickerMessage = string.format("Over " .. self.pickerModes[self.pickerMode] .. " %04d (%02d,%02d)", self.pickerOverId, self.pickerGridPos.X, self.pickerGridPos.Y)
                
                    if(MouseButton(0, InputState.Released) == true) then

                        self:OnPickerSelection(tmpId)

                    end

                end
            
            else
                self.pickerMessage = nil
            end
        end

    else
        
        if(self.pickerPanel.inFocus == true) then

            self:ClearPickerFocus()
            
        end
    end

    if(self.invalidBrush == true) then
        self:RebuildBrushPreview()
    end

end

function PaintTool:ClearPickerFocus()

    editorUI:ClearFocus(self.pickerPanel)

    self.pickerMessage = nil

    pixelVisionOS:ClearMessage()

    self.currentState.overPos.id = -1

end

function PaintTool:DrawPickerPanel()


    -- Update the picker buttons
    editorUI:UpdateButton(self.backButton)
    editorUI:UpdateButton(self.nextButton)
    editorUI:UpdateButton(self.modeButton)

    -- Look for a selection
    if(self.currentState.selectedId > -1) then

        if(self.currentState.selectedPos.page == self.currentPage) then

            self.pickerSelectedCanvas:DrawPixels( 
                self.currentState.selectedPos.X + self.pickerPanelRect.X - 2 ,
                self.currentState.selectedPos.Y + self.pickerPanelRect.Y - 2,
                DrawMode.UI
            )

        end

    end
    
    if(self.pickerPanel.inFocus == true) then
            
        -- Update the sample rect position and adjust for the page
        self.pickerSampleRect.X = self.currentState.overPos.X
        self.pickerSampleRect.Y = self.currentState.overPos.pos.Y * 8

        self.currentCanvas:DrawPixels(self.currentState.overPos.X + self.pickerPanelRect.X , self.currentState.overPos.Y+ self.pickerPanelRect.Y, DrawMode.SpriteAbove, 1, -1, self.maskColor, 0, self.pickerSampleRect)

        self.pickerOverCanvas:DrawPixels( self.currentState.overPos.X + self.pickerPanelRect.X - 2 , self.currentState.overPos.Y+ self.pickerPanelRect.Y - 2, DrawMode.SpriteAbove )

        if(self.pickerMessage ~= nil) then
            
            pixelVisionOS:DisplayMessage(self.pickerMessage)

        end
    end

    if(self.pickerMode == ColorMode) then

        -- TODO Calculate icon position
        DrawMetaSprite( "iconmask", self.pickerPanelRect.X, self.pickerPanelRect.Y, false, false, DrawMode.SpriteAbove )

        local pos = CalculatePosition( self.backgroundColorId, 16 )

        -- local page = pos/16

        -- TODO make sure we are on the right page first and use the page for the Y position
        DrawMetaSprite( "iconbgcolor", (pos.X * 8) + self.pickerPanelRect.X, (pos.Y * 8) + self.pickerPanelRect.Y, false, false, DrawMode.SpriteAbove )


    end

end

function PaintTool:ClearCurrentCanvas()

    -- Clear the sprite canvas
    self.currentCanvas:Clear(-1)

    self.currentCanvas:SetStroke(-1, 0)

    self.currentCanvas:SetPattern(self.emptyPatternPixelData, 8, 8)

    self.currentCanvas:DrawRectangle(0, 0, self.currentCanvas.Width, self.currentCanvas.Height, true)

end

function PaintTool:ChangeMode(value)

    -- TODO this is not protected from picking a bad mode
    self.pickerMode = value

    self.currentState = self.pickerPanel[self.pickerMode .. "State"]

    if(self.pickerMode == ColorMode) then

        self.currentCanvas = self.colorCanvas

        self:IndexColors()

    elseif(self.pickerMode == SpriteMode) then

        -- Get all of the unique sprites from the canvas
        self.currentCanvas = self.spriteCanvas
    
        self:IndexSprites()

    elseif(self.pickerMode == FlagMode) then

        self.currentCanvas = self.flagCanvas

        self:IndexFlags()

    else

        self.currentCanvas = nil
        return

    end

    -- Update the state button to show the correct graphic
    editorUI:RebuildMetaSpriteCache(self.modeButton, self.pickerMode .. "mode")

    -- TODO This should just be calculated by the picker
    self.totalPages = math.ceil(self.currentState.pickerTotal / 16) - 1

    -- Select the last picker state
    self:OnPickerSelection(self.currentState.selectedId)

    self:InvalidateCanvas()
    self:RebuildBrushPreview()

    -- Update menu options

    -- Loop through colors
    for i = 1, #self.colorOptions do
        pixelVisionOS:EnableMenuItemByName(self.colorOptions[i], self.pickerMode == ColorMode)
    end

end

function PaintTool:OnPickerSelection(value)

    value = Clamp(value, 0, self.currentState.pickerTotal)

    -- Test to see if the selection has changed
    if(self.currentState.selectedPos.id ~= value) then

        -- Calculate the selector position
        self.currentState.selectedPos.pos = CalculatePosition( value, 16 )

        -- Calculate the current page
        self.currentState.selectedPos.page = math.floor(self.currentState.selectedPos.pos.Y / 2)

        self.currentState.selectedId = value

        -- Cache the selected position
        self.currentState.selectedPos.id = self.currentState.selectedPosId
        self.currentState.selectedPos.X = self.currentState.selectedPos.pos.X * 8
        self.currentState.selectedPos.Y = (self.currentState.selectedPos.pos.Y - self.currentState.selectedPos.page * 2) * 8

    end

    self:GoToPickerPage(self.currentState.selectedPos.page)

    self:InvalidateBrushPreview()
    
end

function PaintTool:InvalidateBrushPreview()

    self.invalidBrush = true

end

function PaintTool:ResetBrushInvalidation()

    self.invalidBrush = false

end

function PaintTool:RebuildBrushPreview()

    
    -- TODO need to make this indexing more generic if other processes are going on like setting up the flags
    if(self.invalidBrush == false or self.indexingSprites == true) then
        return
    end

    -- Check to see which picker mode we are in
    if(self.pickerMode == ColorMode) then
        
        -- Set the brush color based on the current color selection or to the mask color if we are using the eraser
        self.brushColor = self.tool == "eraser" and self.maskColor or self.currentState.selectedId
        
        -- Clear the entire brush
        self.brushCanvas:Clear()

        -- Fill the brush with the brush color based on the stroke size
        self.brushCanvas:Clear(self.brushColor, 0, 0, self.defaultStrokeWidth, self.defaultStrokeWidth)

        self.brushColorOffset = self.colorOffset

        editorUI:Enable(self.toolButtons[self.shapeTools], true)
        editorUI:Enable(self.toolButtons[self.fillTools], true)
    
        self:ResetBrushInvalidation()

        return

    end

    -- Set the brush color to match the correct color offset
    self.brushColorOffset = self.colorOffset

    if(self.tool == "eraser") then
        
        -- Clear the brush canvas with the mask color
        self.brushCanvas:Clear(self.maskColor)

    else

        if(self.pickerMode == SpriteMode) then
        
            -- Copy the sprite pixel data into the brush canvas
            self.brushCanvas:SetPixels(self.uniqueSprites[self.currentState.selectedId + 1].pixelData)
    
        elseif(self.pickerMode == FlagMode) then
    
            -- TODO calculate the flag's grid position
            local pos = CalculatePosition( self.currentState.selectedId, 16 )

            self.brushCanvas:SetPixels(self.currentCanvas:GetPixels(pos.X * 8, pos.Y * 8, 8, 8))
    
            self.brushColorOffset = 0
        end

    end

    self:ResetBrushInvalidation()

    self:DisableShapeTools()
    
end

function PaintTool:DisableShapeTools()


    if(self.toolButtons[self.shapeTools].selected == true or self.toolButtons[self.fillTools].selected == true) then

        self:OnPickTool(self.shapeTools - 1, false)
        

    end
    -- local shapeToolButton = self.toolButtons[self.shapeTools]


    -- print("shapeToolButton", shapeToolButton)
    -- if(shapeToolButton.selected == true) then

    --     shapeToolButton.selected = false


    --     self:OnPickTool(self.shapeTools - 1, false)
    --     -- local penToolButton = self.toolButtons[self.shapeTools - 1]

    --     -- penToolButton.selected = true
    --     -- editorUI:Invalidate(penToolButton)

    --     -- self:OnSelectTool(penToolButton.spriteName)

    -- end

    editorUI:Enable(self.toolButtons[self.shapeTools], false)
    editorUI:Enable(self.toolButtons[self.fillTools], false)
end

function PaintTool:OnPickerBack()

    local offset = (Key(Keys.LeftShift) or Key(Keys.RightShift)) and 10 or 1
    self:GoToPickerPage(self.currentPage - offset)

end

function PaintTool:OnPickerNext()
    
    local offset = (Key(Keys.LeftShift) or Key(Keys.RightShift)) and 10 or 1
    self:GoToPickerPage(self.currentPage + offset)

end

function PaintTool:GoToPickerPage(value)

    self.currentPage = Clamp(value, 0, self.totalPages )

    editorUI:Enable(self.backButton, self.currentPage > 0)
    editorUI:Enable(self.nextButton, self.currentPage < self.totalPages)

    self.pickerSampleArea.Y = 16 * self.currentPage

    self.currentCanvas:DrawPixels(self.pickerPanelRect.X, self.pickerPanelRect.Y, DrawMode.TilemapCache, 1, -1, self.maskColor, 0, self.pickerSampleArea)

    -- -- TODO clear label area 
    DrawRect(98, 15, 130, 8, BackgroundColor())

    DrawText( string.format(self.currentState.pickerLabel, self.currentPage, self.totalPages):upper(), 100, 14, DrawMode.TilemapCache, "small", 6, -4 )

end

function PaintTool:OnNextMode()

    local modeIndex = table.indexOf(self.pickerModes, self.pickerMode)

    -- Loop backwards through the button sizes
    if(Key(Keys.LeftShift) or reverse == true) then
        modeIndex = modeIndex - 1

        if(modeIndex < 1) then
            modeIndex = self.totalModes
        end

    else
        modeIndex = modeIndex + 1

        if(modeIndex > self.totalModes) then
            modeIndex = 1
        end
    end

    -- local newMode = 

    -- Find the next sprite for the button
    -- local spriteName = newMode .. "mode"

    
    -- -- Change sprite button graphic
    -- self.modeButton.cachedMetaSpriteIds = {
    --     up = FindMetaSpriteId(spriteName .. "up"),
    --     down = FindMetaSpriteId(spriteName .. "down") ~= -1 and FindMetaSpriteId(spriteName .. "down") or FindMetaSpriteId(spriteName .. "selectedup"),
    --     over = FindMetaSpriteId(spriteName .. "over"),
    --     selectedup = FindMetaSpriteId(spriteName .. "selectedup"),
    --     selectedover = FindMetaSpriteId(spriteName .. "selectedover"),
    --     selecteddown = FindMetaSpriteId(spriteName .. "selecteddown") ~= -1 and FindMetaSpriteId(spriteName .. "selecteddown") or FindMetaSpriteId(spriteName .. "selectedover"),
    --     disabled = FindMetaSpriteId(spriteName .. "disabled"),
    --     empty = FindMetaSpriteId(spriteName .. "empty") -- used to clear the sprites
    -- }

    -- -- self:ConfigureSpritePickerSelector(self.pickerMode)

    -- editorUI:Invalidate(self.modeButton)

    self:ChangeMode(self.pickerModes[modeIndex])

end

function PaintTool:InvalidatePicker()
    
    if(self.pickerMode == ColorMode) then

        self:InvalidateColors()

    elseif(self.pickerMode == SpriteMode) then

        self:InvalidateSprites()

    elseif(self.pickerMode == FlagMode) then
        
        self:InvalidateFlags()

    end

end

function PaintTool:SetPickerLabel(title)

    self.currentState.pickerLabel = title

    if(self.totalPages > 1) then
        
        local padding = #tostring(self.totalPages)
        self.currentState.pickerLabel = self.currentState.pickerLabel .. " - Page %0".. padding .."d/%0".. padding .."d"

    end

end