--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

function ImageTool:CreateEditorPanel()

    self.editorPanel = {
        name = "editorPanel"
    }

    self.imageCanvas = nil
    self.viewportRect = NewRect(8, 48, 224, 160)
    self.boundaryRect = NewRect(0,0,0,0)
    self.displayInvalid = true
    self.scrollScale = 8

    -- self.scale = 1
    self.scaledViewport = NewRect()
    self.scaleValues = {.5, 1, 2, 4}--, 8}
    self.scaleMode = 1 -- TODO need to read last scale
    
    -- Change the pointer to a hand when inside of the component
    self.focusCursor = 2

    self.overCanvas = NewCanvas( 8, 8 )
    self.overCanvas.SetStroke(0, 1);

    -- Get the image pixels
    local pixelData = self.image.GetPixels()

    -- Create a new canvas
    self.imageCanvas = NewCanvas(self.image.width, self.image.height)

    -- Copy the modified image pixel data over to the new canvas
    self.imageCanvas.SetPixels(pixelData)
    
    self.vSliderData = editorUI:CreateSlider({x = 235, y = 44, w = 10, h = 193-24}, "vsliderhandle", "Scroll text vertically.")
    self.vSliderData.onAction = function(value) self:OnVerticalScroll(value) end
    
    self.hSliderData = editorUI:CreateSlider({ x = 4, y = 211, w = 233, h = 10}, "hsliderhandle", "Scroll text horizontally.", true)
    self.hSliderData.onAction = function(value) self:OnHorizontalScroll(value) end
    
    self:OnNextZoom()

    self:InvalidateDisplay()

    pixelVisionOS:RegisterUI({name = "OnUpdatePanelUpdate"}, "UpdatePreviewPanel", self)
    pixelVisionOS:RegisterUI({name = "OnDrawPreviewPanel"}, "DrawPreviewPanel", self)


end

function ImageTool:InvalidateDisplay()
    self.displayInvalid = true
end

function ImageTool:ResetMapValidation()
    self.displayInvalid = false
end

function ImageTool:UpdatePreviewPanel(timeDelta)

    -- We only want to run this when a modal isn't active. Mostly to stop the tool if there is an error modal on load
    -- if(pixelVisionOS:IsModalActive() == false) then

    -- Change the scale
    if(Key(Keys.OemMinus, InputState.Released) and self.scaleMode > 1) then
        self:OnNextZoom(true)
    elseif(Key(Keys.OemPlus, InputState.Released) and self.scaleMode <= #self.scaleValues) then
        self:OnNextZoom()
    end

    -- -- Only update the tool's UI when the modal isn't active
    if(self.targetFile ~= nil and self.toolLoaded == true) then

        -- Update the slider
        editorUI:UpdateSlider(self.vSliderData)

        -- Update the slider
        editorUI:UpdateSlider(self.hSliderData)

    end


    if(self.viewportRect.Contains(editorUI.mouseCursor.pos)) then

        if(self.editorPanel.inFocus ~= true) then
            editorUI:SetFocus(self.editorPanel, self.focusCursor)
        end

        self.mCol = math.floor((editorUI.mouseCursor.pos.X - self.viewportRect.X + self.scaledViewport.X)/self.scrollScale)
        self.mRow = math.floor((editorUI.mouseCursor.pos.Y - self.viewportRect.Y + self.scaledViewport.Y)/self.scrollScale)
        
        -- print("Preview", self.mCol, self.mRow)

    else
        
        if(self.editorPanel.inFocus == true) then
            editorUI:ClearFocus(self.editorPanel)
        end
    end

    -- end

end

function ImageTool:OnHorizontalScroll(value)

    -- TODO this is wrong but works when I use ABS... need to fix it
    self.scaledViewport.X = math.floor(math.abs(math.floor(((self.scaledViewport.Width - self.boundaryRect.Width) - self.scaledViewport.Width) * value))/self.scrollScale) * self.scrollScale

    self:InvalidateDisplay()
end

function ImageTool:OnVerticalScroll(value)

    self.scaledViewport.Y = math.floor(math.abs(math.floor(((self.scaledViewport.Height - self.boundaryRect.Height) - self.scaledViewport.Height) * value))/self.scrollScale) * self.scrollScale

    self:InvalidateDisplay()

end



function ImageTool:DrawPreviewPanel()

    if(self.displayInvalid == true and pixelVisionOS:IsModalActive() == false) then

        if(self.clearBG) then
            -- print("Refresh background")
            self:ClearBackground()
            self.clearBG = false
        
        end
        
        -- Draw the pixel data in the upper left hand cornver of the tool's window
        self.imageCanvas:DrawPixels(self.viewportRect.X, self.viewportRect.Y, DrawMode.TilemapCache, self.scale, -1, self.maskColor, self.colorOffset, self.scaledViewport)

        self.displayInvalid = false

    end

    if(self.editorPanel.inFocus == true) then

        -- print(editorUI.mouseCursor.pos)

        local tmpX = self.mCol * self.scrollScale
        local tmpY = self.mRow * self.scrollScale
        
        if(tmpX < (self.imageCanvas.width * self.scale) and tmpY < (self.imageCanvas.height * self.scale)) then

            -- TODO If dragging, snap this to the mouse position

            tmpX = tmpX + self.viewportRect.X - self.scaledViewport.X
            tmpY = tmpY + self.viewportRect.Y- self.scaledViewport.Y

            self.overCanvas:DrawPixels( tmpX - 3 , tmpY - 3, DrawMode.UI )

            self.imageCanvas:DrawPixels(tmpX, tmpY, DrawMode.SpriteAbove, self.scale, -1, self.maskColor, 0, NewRect( self.mCol * 8  + self.scaledViewport.X, self.mRow * 8   + self.scaledViewport.Y, 8, 8 ))

        else
            editorUI:ClearFocus(self.editorPanel)
        end
    end


end


function ImageTool:OnNextZoom(reverse)

    -- Loop backwards through the button sizes
    if(Key(Keys.LeftShift) or reverse == true) then
        self.scaleMode = self.scaleMode - 1
  
        if(self.scaleMode < 1) then
            self.scaleMode = 1
        end

        -- TODO disable zoom out menu
  
    else
        self.scaleMode = self.scaleMode + 1
  
        if(self.scaleMode > #self.scaleValues) then
            self.scaleMode = #self.scaleValues
        end

        -- TODO disable zoom in menu

    end
  
    self:ChangeScale( self.scaleValues[self.scaleMode])

  end

  function ImageTool:ChangeScale(value)
   
    self.scale = value--Clamp(value, 1, 8)

    local imageWidth = math.floor(self.imageCanvas.width * self.scale)
    local imageHeight = math.floor(self.imageCanvas.height * self.scale)

    local viewWidth = math.floor(self.viewportRect.Width / self.scale)
    local viewHeight = math.floor(self.viewportRect.Height / self.scale)

    self.scaledViewport.Width = Clamp(viewWidth, 1, math.max(imageWidth, self.imageCanvas.width)) --math.min(self.viewportRect.Width, math.min(self.imageCanvas.width * self.scale, math.ceil(self.viewportRect.Width / self.scale)))
    self.scaledViewport.Height = Clamp(viewHeight, 1, math.max(imageHeight, self.imageCanvas.height))--, self.viewportRect.Height) --math.min(self.viewportRect.Height, math.min(self.imageCanvas.height * self.scale, math.ceil(self.viewportRect.Height / self.scale)))

    -- Calculate the boundary for scrolling
    self.boundaryRect.Width = self.imageCanvas.width - self.scaledViewport.Width
    self.boundaryRect.Height = self.imageCanvas.height - self.scaledViewport.Height

    editorUI:Enable(self.hSliderData, self.boundaryRect.Width > 0)

    editorUI:Enable(self.vSliderData, self.boundaryRect.Height > 0)

    -- TODO enable and disable menu


    self.clearBG = false

    if(self.hSliderData.enabled == true) then
        local oldValue = self.hSliderData.value
        self.hSliderData.value = -1
        editorUI:ChangeSlider(self.hSliderData, oldValue, true)
    else
        editorUI:ChangeSlider(self.hSliderData, 0, true)
        self.clearBG = true
    end

    if(self.vSliderData.enabled == true) then
        local oldValue = self.vSliderData.value
        self.vSliderData.value = -1
        editorUI:ChangeSlider(self.vSliderData, oldValue, true)
    else
        editorUI:ChangeSlider(self.vSliderData, 0, true)
        self.clearBG = true
    end

    self.scrollScale = 8 * self.scale


    self.overCanvas.Resize(self.scrollScale + 6, self.scrollScale + 6)
    self.overCanvas.Clear(15)
    self.overCanvas.DrawRectangle(0, 0, self.overCanvas.Width, self.overCanvas.Height);
    self.overCanvas.DrawRectangle(2, 2, self.overCanvas.Width-4, self.overCanvas.Height-4);

    self:InvalidateDisplay()

    pixelVisionOS:DisplayMessage("Image scale " .. (self.scale * 100) .. "%")

end

function ImageTool:ClearBackground()
    
    -- TODO need to find only the area we need to redraw to reduce flicker

    local cols = self.viewportRect.Width / 8
    
    local total = cols * (self.viewportRect.Height / 8)

    local spriteId = MetaSprite( "emptycolor" ).Sprites[1].Id

    for i = 1, total do
        
        local pos = CalculatePosition( i-1, cols )

        DrawSprite(spriteId, self.viewportRect.X + (pos.X * 8), self.viewportRect.Y + (pos.Y * 8), false, false, DrawMode.TilemapCache)
        
    end

end
