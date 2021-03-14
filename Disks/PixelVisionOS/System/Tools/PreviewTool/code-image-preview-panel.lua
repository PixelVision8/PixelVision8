--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

local editorPanelID = "EditorPanelUI"

function ImageTool:CreateEditorPanel()

    -- Setup viewport
    self.viewportRect.Width = math.min(self.imageCanvas.width, self.viewportRect.Width)
    self.viewportRect.Height = math.min(self.imageCanvas.height, self.viewportRect.Height)

    -- Calculate the boundary for scrolling
    self.boundaryRect.Width = self.imageCanvas.width - self.viewportRect.Width
    self.boundaryRect.Height = self.imageCanvas.height - self.viewportRect.Height

    if(self.boundaryRect.Height > 0) then
        self.vSliderData = editorUI:CreateSlider({x = 235, y = 20, w = 10, h = 193}, "vsliderhandle", "Scroll text vertically.")
        self.vSliderData.onAction = function(value) self:OnVerticalScroll(value) end
    end

    if(self.boundaryRect.Width > 0) then
        self.hSliderData = editorUI:CreateSlider({ x = 4, y = 211, w = 233, h = 10}, "hsliderhandle", "Scroll text horizontally.", true)
        self.hSliderData.onAction = function(value) self:OnHorizontalScroll(value) end
    end

    self:InvalidateDisplay()

    pixelVisionOS:RegisterUI({name = editorPanelID}, "UpdateEditorPanel", self)


end

function ImageTool:InvalidateDisplay()
    self.displayInvalid = true
end

function ImageTool:ResetMapValidation()
    self.displayInvalid = false
end

function ImageTool:UpdateEditorPanel(timeDelta)

    -- We only want to run this when a modal isn't active. Mostly to stop the tool if there is an error modal on load
    if(pixelVisionOS:IsModalActive() == false) then

        -- -- Only update the tool's UI when the modal isn't active
        if(self.targetFile ~= nil and self.toolLoaded == true) then

            -- Update the slider
            editorUI:UpdateSlider(self.vSliderData)

            -- Update the slider
            editorUI:UpdateSlider(self.hSliderData)

        end

    end

end

function ImageTool:OnHorizontalScroll(value)

    -- TODO this is wrong but works when I use ABS... need to fix it
    self.viewportRect.X = math.abs(math.floor(((self.viewportRect.Width - self.boundaryRect.Width) - self.viewportRect.Width) * value))

    self:InvalidateDisplay()
end

function ImageTool:OnVerticalScroll(value)
    self.viewportRect.Y = math.abs(math.floor(((self.viewportRect.Height - self.boundaryRect.Height) - self.viewportRect.Height) * value))

    self:InvalidateDisplay()

end

function ImageTool:Draw()

-- print("Draw")

    if(self.displayInvalid == true and pixelVisionOS:IsModalActive() == false) then

        -- Draw the pixel data in the upper left hand cornver of the tool's window
        self.imageCanvas:DrawPixels(8, 24, DrawMode.TilemapCache, 1, -1, self.maskColor, self.colorOffset, self.viewportRect)

        -- if(self.debugMode) then
        --     self.colorMemoryCanvas:DrawPixels(8, 24, self.DrawMode.UI, 3)
        -- end

    end

    -- -- The ui should be the last thing to update after your own custom draw calls
    -- pixelVisionOS:Draw()

end
