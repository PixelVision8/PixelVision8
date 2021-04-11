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
    
    -- Selected position
    self.selectedPos = {id = -1}
    self.selectedId = -1

    self.overPos = {id = -1}

    -- Used to sample pixel data from the active canvas
    self.pickerSampleRect = NewRect( 0, 0, 8, 8 )

    self.pickerTotal = 0

    self.pickerPanelRect = NewRect(112, 22, 128, 16)
    
    self.pickerSampleArea = NewRect( 0, 0, self.pickerPanelRect.Width, self.pickerPanelRect.Height )
    
    self.currentCanvas = nil

    self.pickerMode = nil

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


    self.backButton = editorUI:CreateButton({x = 104, y = 16}, "stepperback", "This is the picker back button.")

    self.backButton.onAction = function() self:OnPickerBack() end


    self.nextButton = editorUI:CreateButton({x = 240, y = 16}, "steppernext", "This is the picker next button.")

    self.nextButton.onAction = function() self:OnPickerNext() end

    self.pickerModes = {ColorMode, SpriteMode, FlagMode}
    self.pickerMode = 0

    -- Create size button
    self.modeButton = editorUI:CreateButton({x = 80, y = 16}, "colormode", "Change mode")
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

                if(tmpId >= self.pickerTotal or tmpId == self.selectedId) then

                    editorUI:ClearFocus(self.pickerPanel)

                end

                -- Test to see if the over id has changed
                if(self.overPos.id ~= tmpId) then

                    -- save the over position
                    self.overPos.pos = CalculatePosition( tmpId, 16 )

                    -- Calculate the current page
                    self.overPos.page = math.floor(self.overPos.pos.Y / 2)

                    -- Cache the over position
                    self.overPos.id = tmpId
                    self.overPos.X = self.overPos.pos.X * 8
                    self.overPos.Y = (self.overPos.pos.Y - self.overPos.page * 2) * 8

                end

                self.pickerColumn = math.floor((editorUI.mouseCursor.pos.X - self.pickerPanelRect.X)/8)
                self.pickerRow = math.floor((editorUI.mouseCursor.pos.Y - self.pickerPanelRect.Y)/8)

                -- self.pickerMessage = string.format("Over " .. self.pickerModes[self.pickerMode] .. " %04d (%02d,%02d)", self.pickerOverId, self.pickerGridPos.X, self.pickerGridPos.Y)
            
                if(MouseButton(0, InputState.Released) == true) then

                    self.selectedId = tmpId

                end
            
            else
                self.pickerMessage = nil
            end
        end

    else
        
        if(self.pickerPanel.inFocus == true) then
            editorUI:ClearFocus(self.pickerPanel)

            self.pickerMessage = nil

            pixelVisionOS:ClearMessage()
        end
    end

end

function PaintTool:DrawPickerPanel()


    -- Update the picker buttons
    editorUI:UpdateButton(self.backButton)
    editorUI:UpdateButton(self.nextButton)
    editorUI:UpdateButton(self.modeButton)

    -- Look for a selection
    if(self.selectedId > -1) then

        -- Test to see if the selection has changed
        if(self.selectedPos.id ~= self.selectedId) then

            -- Calculate the selector position
            self.selectedPos.pos = CalculatePosition( self.selectedId, 16 )

            -- Calculate the current page
            self.selectedPos.page = math.floor(self.selectedPos.pos.Y / 2)

            -- Cache the selected position
            self.selectedPos.id = self.selectedPosId
            self.selectedPos.X = self.selectedPos.pos.X * 8
            self.selectedPos.Y = (self.selectedPos.pos.Y - self.selectedPos.page * 2) * 8

        end

        if(self.selectedPos.page == self.currentPage) then

            self.pickerSelectedCanvas:DrawPixels( 
                self.selectedPos.X + self.pickerPanelRect.X - 2 ,
                self.selectedPos.Y + self.pickerPanelRect.Y - 2,
                DrawMode.UI
            )

        end

    end
    
    if(self.pickerPanel.inFocus == true) then

        -- print(editorUI.mouseCursor.pos)

        -- self.scale = 1

        -- local tmpX = self.pickerColumn * 8
        -- local tmpY = self.pickerRow * 8
        
        -- TODO need to test if we are over a valid index
        -- if(tmpX < (self.imageCanvas.width * self.scale) and tmpY < (self.imageCanvas.height * self.scale)) then

            -- TODO If dragging, snap this to the mouse position

            -- tmpX = tmpX + self.pickerPanelRect.X-- - self.scaledViewport.X
            -- tmpY = tmpY + self.pickerPanelRect.Y--- self.scaledViewport.Y

            
            -- Update the sample rect position and adjust for the page
            self.pickerSampleRect.X = self.overPos.X
            self.pickerSampleRect.Y = self.overPos.pos.Y * 8
            

            self.currentCanvas:DrawPixels(self.overPos.X + self.pickerPanelRect.X , self.overPos.Y+ self.pickerPanelRect.Y, DrawMode.SpriteAbove, 1, -1, self.maskColor, 0, self.pickerSampleRect)

            self.pickerOverCanvas:DrawPixels( self.overPos.X + self.pickerPanelRect.X - 2 , self.overPos.Y+ self.pickerPanelRect.Y - 2, DrawMode.SpriteAbove )


        -- else
        --     editorUI:ClearFocus(self.editorPanel)
        -- end

        if(self.pickerMessage ~= nil) then
            
            pixelVisionOS:DisplayMessage(self.pickerMessage)

        end
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

    self.pickerMode = value

    local modeLabel = self.pickerModes[value]

    if(modeLabel == ColorMode) then

        self.pickerLabel = "Image Colors - Page %02d of %02d"

        self.currentCanvas = self.colorCanvas

        self:IndexColors()

    elseif(modeLabel == SpriteMode) then

        self.pickerLabel = "Unique Sprites - Page %02d of %02d"

        -- Get all of the unique sprites from the canvas
        self.currentCanvas = self.spriteCanvas
    
        self:IndexSprites()

    elseif(modeLabel == FlagMode) then

        self.pickerLabel = "Tilemap Flags"

        self.currentCanvas = self.flagCanvas

        self:IndexFlags()

    else

        self.currentCanvas = nil
        return

    end

    -- Recalculate pages based on the canvas
    self.totalPages = ((self.currentCanvas.Height / 8) / 2) - 1 -- Account for zero based pages by subtracting 1

    -- Reset current page
    self:GoToPickerPage(0)

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
    DrawRect(100, 11, 130, 8, BackgroundColor())
    
    DrawText( string.format(self.pickerLabel, self.currentPage, self.totalPages):upper(), 104, 11, DrawMode.TilemapCache, "small", 6, -4 )

end

function PaintTool:OnNextMode()

    -- Loop backwards through the button sizes
    if(Key(Keys.LeftShift) or reverse == true) then
        self.pickerMode = self.pickerMode - 1

        if(self.pickerMode < 1) then
            self.pickerMode = #self.pickerModes
        end

    else
        self.pickerMode = self.pickerMode + 1

        if(self.pickerMode > #self.pickerModes) then
            self.pickerMode = 1
        end
    end

    local newMode = self.pickerModes[self.pickerMode]

    -- Find the next sprite for the button
    local spriteName = newMode .. "mode"

    -- Change sprite button graphic
    self.modeButton.cachedMetaSpriteIds = {
        up = FindMetaSpriteId(spriteName .. "up"),
        down = FindMetaSpriteId(spriteName .. "down") ~= -1 and FindMetaSpriteId(spriteName .. "down") or FindMetaSpriteId(spriteName .. "selectedup"),
        over = FindMetaSpriteId(spriteName .. "over"),
        selectedup = FindMetaSpriteId(spriteName .. "selectedup"),
        selectedover = FindMetaSpriteId(spriteName .. "selectedover"),
        selecteddown = FindMetaSpriteId(spriteName .. "selecteddown") ~= -1 and FindMetaSpriteId(spriteName .. "selecteddown") or FindMetaSpriteId(spriteName .. "selectedover"),
        disabled = FindMetaSpriteId(spriteName .. "disabled"),
        empty = FindMetaSpriteId(spriteName .. "empty") -- used to clear the sprites
    }

    -- self:ConfigureSpritePickerSelector(self.pickerMode)

    editorUI:Invalidate(self.modeButton)

    self:ChangeMode(self.pickerMode)

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