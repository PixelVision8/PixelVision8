--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

ColorMode, pickerMode, FlagMode = "color", "sprite", "flag"

function ImageTool:CreatePickerPanel()

    self.pickerPanel = {
        name = "pickerPanel"
    }

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
    self.colorCanvas.Clear(0)
    
    self.spriteCanvas = NewCanvas(128, 1024)
    self.colorCanvas.Clear(1)

    -- TODO read the flags from memory or if there is a flag file in the folder
    self.flagCanvas = NewCanvas(128, 16)
    self.colorCanvas.Clear(2)


    self.backButton = editorUI:CreateButton({x = 104, y = 16}, "stepperback", "This is the picker back button.")

    self.backButton.onAction = function() self:OnPickerBack() end


    self.nextButton = editorUI:CreateButton({x = 240, y = 16}, "steppernext", "This is the picker next button.")

    self.nextButton.onAction = function() self:OnPickerNext() end

    self.pickerModes = {ColorMode, pickerMode, FlagMode}
    self.pickerMode = 0

    -- Create size button
    self.modeButton = editorUI:CreateButton({x = 80, y = 16}, "colormode", "Change mode")
    self.modeButton.onAction = function() self:OnNextMode() end


    self:OnNextMode()

    

    pixelVisionOS:RegisterUI({name = "OnUpdatePickerPanel"}, "UpdatePickerPanel", self)
    pixelVisionOS:RegisterUI({name = "OnDrawPickerPanel"}, "DrawPickerPanel", self)

end

function ImageTool:UpdatePickerPanel(timeDelta)

    if(self.pickerPanelRect.Contains(editorUI.mouseCursor.pos)) then

        if(self.pickerPanel.inFocus ~= true) then
            editorUI:SetFocus(self.pickerPanel, self.focusCursor)
        end

        self.mCol = math.floor((editorUI.mouseCursor.pos.X - self.pickerPanelRect.X)/8)
        self.mRow = math.floor((editorUI.mouseCursor.pos.Y - self.pickerPanelRect.Y)/8)

        self.pickerMessage = string.format("Over " .. self.pickerModes[self.pickerMode] .. " %04d (%02d,%02d)", CalculateIndex( self.mCol, self.mRow, 16 ), self.mCol, self.mRow)
    else
        
        if(self.pickerPanel.inFocus == true) then
            editorUI:ClearFocus(self.pickerPanel)

            self.pickerMessage = nil

            pixelVisionOS:ClearMessage()
        end
    end

end

function ImageTool:DrawPickerPanel()

    if(self.currentCanvas ~= nil) then


        
        -- TODO draw the canvas to the screen

    end

    editorUI:UpdateButton(self.backButton)
    editorUI:UpdateButton(self.nextButton)
    editorUI:UpdateButton(self.modeButton)

    -- DrawRect( self.pickerPanelRect.X, self.pickerPanelRect.Y, self.pickerPanelRect.Width, self.pickerPanelRect.Height, 2, DrawMode.SpriteAbove )

    if(self.pickerPanel.inFocus == true) then

        -- print(editorUI.mouseCursor.pos)

        -- local tmpX = self.mCol * self.scrollScale
        -- local tmpY = self.mRow * self.scrollScale
        
        -- if(tmpX < (self.imageCanvas.width * self.scale) and tmpY < (self.imageCanvas.height * self.scale)) then

        --     -- TODO If dragging, snap this to the mouse position

        --     tmpX = tmpX + self.viewportRect.X - self.scaledViewport.X
        --     tmpY = tmpY + self.viewportRect.Y- self.scaledViewport.Y

        --     self.overCanvas:DrawPixels( tmpX - 3 , tmpY - 3, DrawMode.UI )

        --     self.imageCanvas:DrawPixels(tmpX, tmpY, DrawMode.SpriteAbove, self.scale, -1, self.maskColor, 0, NewRect( self.mCol * 8  + self.scaledViewport.X, self.mRow * 8   + self.scaledViewport.Y, 8, 8 ))

        -- else
        --     editorUI:ClearFocus(self.editorPanel)
        -- end

        if(self.pickerMessage ~= nil) then
            
            pixelVisionOS:DisplayMessage(self.pickerMessage)

        end
    end

end

function ImageTool:ChangeMode(value)

    self.pickerMode = value

    local modeLabel = self.pickerModes[value]

    if(modeLabel == ColorMode) then

        self.pickerLabel = "Image Colors - Page %02d of %02d"

        -- TODO update colors from memory
        self.currentCanvas = self.colorCanvas

        self.currentCanvas:SetStroke(-1, 0)

        for i = 1, 255-16 do

            local colorIndex = i - 1 + self.colorOffset

            local tmpColor = Color(colorIndex)
            local pos = CalculatePosition( i-1, 16 )

            if(tmpColor == "#FF00FF")then
                
                self.currentCanvas:SetPattern(self.emptyPatternPixelData, 8, 8)

            else

                for j = 1, #self.colorPixelData do
                    self.colorPixelData[j] = colorIndex
                end

                self.currentCanvas:SetPattern(self.colorPixelData, 8, 8)
                
            end

            self.currentCanvas:DrawRectangle(pos.X * 8, pos.Y * 8, 8, 8, true)

        end

    elseif(modeLabel == pickerMode) then

        self.pickerLabel = "Unique Sprites - Page %02d of %02d"

        -- Get all of the unique sprites from the canvas
        self.currentCanvas = self.spriteCanvas

        local targetSize = NewPoint(math.ceil(self.imageCanvas.Width / 8) * 8, math.ceil(self.imageCanvas.height / 8) * 8)

        print("Resize canvas", self.imageCanvas.Width,targetSize.X,self.imageCanvas.Height,targetSize.Y)

        if(self.imageCanvas.Width ~= targetSize.X or self.imageCanvas.Height ~= targetSize.Y) then

            print("Resize canvas", self.imageCanvas.Width,targetSize.X,self.imageCanvas.Height,targetSize.Y)

        end

    elseif(modeLabel == FlagMode) then

        self.pickerLabel = "Tilemap Flags - Page %02d of %02d"

        self.currentCanvas = self.flagCanvas

        self.currentCanvas:SetStroke(-1, 0)
        self.currentCanvas:SetPattern(self.emptyPatternPixelData, 8, 8)

        for i = 1, 32  do

            local index = i - 1
            local pos = CalculatePosition( index, 16 )

            if(index < 16) then
                
                self.currentCanvas:DrawMetaSprite(FindMetaSpriteId("flag".. i .. "small"), pos.X * 8, pos.Y * 8)

            else
                self.currentCanvas:DrawRectangle(pos.X * 8, pos.Y * 8, 8, 8, true)
            end
            
        end

    else
        self.currentCanvas = nil
        return
    end


    -- Recalculate pages
    self.totalPages = ((self.currentCanvas.Height / 8) / 2) - 1 -- Account for zero based pages by subtracting 1

    -- Reset current page
    self:GoToPickerPage(0)

end

function ImageTool:OnPickerBack()

    self:GoToPickerPage(self.currentPage - 1)

end

function ImageTool:OnPickerNext()
    
    self:GoToPickerPage(self.currentPage + 1)

end

function ImageTool:GoToPickerPage(value)

    self.currentPage = Clamp(value, 0, self.totalPages )


    editorUI:Enable(self.backButton, self.currentPage > 0)
    editorUI:Enable(self.nextButton, self.currentPage < self.totalPages)


    print( "Current Page", self.currentPage, self.totalPages)

    self.pickerSampleArea.Y = 16 * self.currentPage

    self.currentCanvas:DrawPixels(self.pickerPanelRect.X, self.pickerPanelRect.Y, DrawMode.TilemapCache, 1, -1, self.maskColor, 0, self.pickerSampleArea)

    -- -- TODO clear label area 
    DrawRect(100, 11, 130, 8, BackgroundColor())
    -- DrawText("test", 100, 11, "small")
    DrawText( string.format(self.pickerLabel, self.currentPage, self.totalPages):upper(), 104, 11, DrawMode.TilemapCache, "small", 6, -4 )

end

function ImageTool:OnNextMode()

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