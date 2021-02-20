--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

local editorPanelID = "EditorPanelUI"

function TextTool:CreateEditorPanel()

    self.vSliderData = editorUI:CreateSlider({x = 235, y = 20, w = 10, h = 193}, "vsliderhandle", "Scroll text vertically.")
    self.vSliderData.onAction = function(value) self:OnVerticalScroll(value) end

    self.hSliderData = editorUI:CreateSlider({ x = 4, y = 211, w = 233, h = 10}, "hsliderhandle", "Scroll text horizontally.", true)
    self.hSliderData.onAction = function(value) self:OnHorizontalScroll(value) end

    -- Create input area
    self.inputAreaData = editorUI:CreateInputArea({x = 8, y = 24, w = 224, h = 184}, nil, "Click to edit the text.")
    self.inputAreaData.wrap = false
    self.inputAreaData.editable = true
    self.inputAreaData.autoDeselect = false
    self.inputAreaData.colorize = self.codeMode

    -- Prepare the input area for scrolling
    self.inputAreaData.scrollValue = {x = 0, y = 0}

    self:RefreshEditor()

    pixelVisionOS:RegisterUI({name = editorPanelID}, "UpdateEditorPanel", self)


end

function TextTool:UpdateEditorPanel(timeDelta)

    -- Convert timeDelta to a float
    --timeDelta = timeDelta / 1000

    -- This needs to be the first call to make sure all of the editor UI is updated first
    --pixelVisionOS:Update(timeDelta)

    if(self.inputAreaData ~= nil and self.inputAreaData.inFocus == true and pixelVisionOS:IsModalActive()) then
        editorUI:ClearFocus(self.inputAreaData)
    end
    -- Only update the tool's UI when the modal isn't active
    if(pixelVisionOS:IsModalActive() == false and self.targetFile ~= nil and pixelVisionOS.titleBar.menu.showMenu == false) then

        -- Check to see if we should show the horizontal slider
        local showVSlider = #self.inputAreaData.buffer > self.inputAreaData.tiles.h

        -- Check for mouse wheel scrolling
        local wheelDir = MouseWheel()

        -- Test if we need to show or hide the slider
        if(self.vSliderData.enabled ~= showVSlider) then

            editorUI:Enable(self.vSliderData, showVSlider)
        end

        if(wheelDir.Y ~= 0) then

            local scrollValue = Clamp(wheelDir.y, -1, 1) * -5

            if(Key(Keys.LeftControl) == true or Key( Keys.RightControl)) then
                self:OnHorizontalScroll((Clamp(self.hSliderData.value * 100 + scrollValue, 0, 100)/100))
            else
                self:OnVerticalScroll((Clamp(self.vSliderData.value * 100 + scrollValue, 0, 100)/100))
            end

        elseif(wheelDir.X ~= 0) then

            self:OnHorizontalScroll((Clamp(self.hSliderData.value * 100 + (Clamp(wheelDir.y, -1, 1) * -5), 0, 100)/100))

        end

        if(self.vSliderData.enabled == true) then
            self.inputAreaData.scrollValue.y = (self.inputAreaData.vy - 1) / (#self.inputAreaData.buffer - self.inputAreaData.tiles.h)

            if(self.vSliderData.value ~= self.inputAreaData.scrollValue.y) then

                -- print("scroll", wheelDir, , inputAreaData.scrollValue.y)
                self:InvalidateLineNumbers()

                -- inputAreaData.scrollValue.y = inputAreaData.scrollValue.y + Clamp(wheelDir.y, -1, 1)/100

                editorUI:ChangeSlider(self.vSliderData, self.inputAreaData.scrollValue.y , false)
            end

        end

        -- Update the slider
        editorUI:UpdateSlider(self.vSliderData)

        -- Check to see if we should show the vertical slider
        local showHSlider = self.inputAreaData.maxLineWidth > self.inputAreaData.tiles.w

        -- Test if we need to show or hide the slider
        if(self.hSliderData.enabled ~= showHSlider) then
            editorUI:Enable(self.hSliderData, showHSlider)
        end

        if(self.hSliderData.enabled == true) then
            self.inputAreaData.scrollValue.x = (self.inputAreaData.vx - 1) / ((self.inputAreaData.maxLineWidth + 1) - self.inputAreaData.tiles.w)

            if(self.hSliderData.value ~= self.inputAreaData.scrollValue.x or wheelDir.x ~= 0) then
                editorUI:ChangeSlider(self.hSliderData, self.inputAreaData.scrollValue.x + Clamp(wheelDir.x, -1, 1), false)
            end

        end

        -- Update the slider
        editorUI:UpdateSlider(self.hSliderData)

        -- Reset focus back to the text editor
        if(self.hSliderData.inFocus == false and self.vSliderData.inFocus == false and self.inputAreaData.inFocus == false and pixelVisionOS.titleBar.menu.showMenu == false) then
            editorUI:EditTextEditor(self.inputAreaData, true, false)
        end

        editorUI:UpdateInputArea(self.inputAreaData)

        -- TODO need a better way to check if the text has been changed in the editor
        if(self.inputAreaData.invalidText == true) then
            self:InvalidateData()
            self:InvalidateLineNumbers()
        end

    end

end

function TextTool:InvalidateLineNumbers()
    self.lineNumbersInvalid = true
end

function TextTool:ResetLineNumberInvalidation()
    self.lineNumbersInvalid = false
end

function TextTool:RefreshEditor()

    -- Read the text file and update the input area
    editorUI:ChangeInputArea(self.inputAreaData, ReadTextFile(self.targetFile))

    self:ResetDataValidation()

    self:InvalidateLineNumbers()

    if(SessionID() == ReadSaveData("sessionID", "") and self.targetFile == ReadSaveData("targetFile", "")) then

        local cursorPosString = ReadSaveData("cursor", "0,0")

        local tmpCursor = editorUI:TextEditorGetState(self.inputAreaData)

        local map = {
            "cx",
            "cy",
            "sxs",
            "sys",
            "sxe",
            "sye",
        }

        local counter = 1
        for word in string.gmatch(cursorPosString, '([^,]+)') do

            tmpCursor[map[counter]] = tonumber(word)
            counter = counter + 1

        end

        editorUI:TextEditorSetState(self.inputAreaData, tmpCursor)

        -- Restore last scroll position
        local scrollPosString = ReadSaveData("scroll", "0,0")

        map = {"vx", "vy"}

        counter = 1
        for word in string.gmatch(scrollPosString, '([^,]+)') do

            self.inputAreaData[map[counter]] = tonumber(word)
            counter = counter + 1

        end


    end

    editorUI:EditTextEditor(self.inputAreaData, true, false)

    --
    -- inputAreaData.inFocus = true
    -- editorUI:SetFocus(inputAreaData, 3)

end

function TextTool:OnHorizontalScroll(value)

    -- print("Scroll", value)
    
    local charPos = math.ceil(((self.inputAreaData.maxLineWidth + 1) - (self.inputAreaData.tiles.w)) * value) + 1

    if(self.inputAreaData.vx ~= charPos) then
        self.inputAreaData.vx = charPos
        editorUI:TextEditorInvalidateBuffer(self.inputAreaData)
    end

end

function TextTool:OnVerticalScroll(value)
    -- print("Scroll", value)
    local line = math.ceil((#self.inputAreaData.buffer - (self.inputAreaData.tiles.h - 1)) * value)
    if(self.inputAreaData.vy ~= line) then
        self.inputAreaData.vy = Clamp(line, 1, #self.inputAreaData.buffer)

        editorUI:TextEditorInvalidateBuffer(self.inputAreaData)
    end

    self:InvalidateLineNumbers()

end

function TextTool:DrawLineNumbers()

    if(self.codeMode == false) then
        return
    end

    -- Make sure the gutter is the correct size
    self:CalculateLineGutter()

    -- Only draw the line numbers if show lines is true
    if(self.showLines ~= true) then
        return
    end


    local offset = self.inputAreaData.vy - 1
    -- local totalLines = self.inputAreaData.tiles.h
    local padWidth = (self.lineWidth / 8) - 1

    for i = 1, self.inputAreaData.tiles.h do

        DrawText(string.lpad(tostring(i + offset), padWidth, "0") .. " ", 1, 2 + i, DrawMode.Tile, "large", 6)

    end

end

function TextTool:CalculateLineGutter()

    -- Update total
    self.totalLines = #self.inputAreaData.buffer

    self.lineWidth = self.showLines == true and ((#tostring(self.totalLines) + 1) * 8) or 0

    -- Only resize the input field if the size doesn't match
    local newWidth = 224 - self.lineWidth

    if(self.inputAreaData.rect.w ~= newWidth) then
        editorUI:ResizeTexdtEditor(self.inputAreaData, newWidth, self.inputAreaData.rect.h, 8 + self.lineWidth, self.inputAreaData.rect.y)
    end

end