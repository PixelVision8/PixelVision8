--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

SpriteMode, ColorMode = 0, 1

local toolbarID = "ToolBarUI"

local tools = {"pen", "eraser", "line", "box", "circle", "eyedropper", "fill", "select"}

local toolKeys = {Keys.B, Keys.E, Keys.L, Keys.U, Keys.C, Keys.I, Keys.F, Keys.M}

local pos = NewPoint(8, 56)

function DrawTool:CreateToolbar()

    self.lastSelectedToolID = 1

    -- Labels for mode changes
    self.editorLabelArgs = {nil, 4, 2, nil, false, false, DrawMode.Tile}

    -- Add the eraser if we are in direct color mode
    -- table.insert(tools, 2, "eraser")
    -- table.insert(toolKeys, 2, Keys.E)

    -- self.lastColorID = 0

    self.modeButton = editorUI:CreateToggleButton({x=pos.X, y=pos.y - 32}, "editormode", "Change into color edit mode.")
    self.modeButton.onAction = function(value) self:ChangeEditMode(value and ColorMode or SpriteMode) end

    self.toolBtnData = editorUI:CreateToggleGroup()
    self.toolBtnData.onAction = function(value) self:OnSelectTool(value) end

    local offsetY = 0

    -- Build tools
    for i = 1, #tools do
        offsetY = ((i - 1) * 16) + pos.Y
        local rect = {x = pos.X, y = offsetY, w = 16, h = 16}
        editorUI:ToggleGroupButton(self.toolBtnData, rect, tools[i], "Select the '" .. tools[i] .. "' (".. tostring(toolKeys[i]) .. ") tool.")
    end

    self.flipHButton = editorUI:CreateButton({x = pos.X, y = offsetY + 16 + 8, w = 16, h = 16}, "hflip", "Preview the sprite flipped horizontally.")

    self.flipHButton.onAction = function(value)
        -- Save the new pixel data to history
        self:SaveCanvasState()

        -- Update the canvas and flip the H value
        self:UpdateCanvas(self.lastSelection, true, false)

        -- Save the new pixel data back to the sprite chip
        self:OnSaveCanvasChanges()

    end

    self.flipVButton = editorUI:CreateButton({x = pos.X, y = offsetY + 32 + 8, w = 16, h = 16}, "vflip", "Preview the sprite flipped vertically.")

    self.flipVButton.onAction = function(value)
        
        -- Save the new pixel data to history
        self:SaveCanvasState()

        -- Update the canvas and flip the H value
        self:UpdateCanvas(self.lastSelection, false, true)
        
        -- Save the new pixel data back to the sprite chip
        self:OnSaveCanvasChanges()

    end

    pixelVisionOS:RegisterUI({name = toolbarID}, "UpdateToolbar", self)

end

function DrawTool:UpdateToolbar()

    editorUI:UpdateToggleGroup(self.toolBtnData)
    editorUI:UpdateButton(self.flipHButton)
    editorUI:UpdateButton(self.flipVButton)
    editorUI:UpdateButton(self.modeButton)

    -- if(spriteIDInputData.editing == false) then

        if(Key(Keys.LeftControl) == false and Key(Keys.RightControl) == false) then

            for i = 1, #toolKeys do
                if(Key(toolKeys[i], InputState.Released)) then
                    editorUI:SelectToggleButton(self.toolBtnData, i)
                    break
                end
            end
        end

    -- end
end


function DrawTool:ChangeEditMode(mode)
    -- print("Mode", mode)
    
    if(mode == self.mode) then
        return
    end

    self.mode = mode

    if(self.mode == ColorMode) then

        self.toolTitle = "colors.png"
        
        -- Make sure the mode button is selected
        if(self.modeButton.selected == false) then
            editorUI:ToggleButton(self.modeButton, true, false)
        end


        self:ShowColorPanel()
        self:HideCanvasPanel()
       
        self:ToggleToolBar(false)

        self.editorLabelArgs[1] = systemcolorlabel.spriteIDs
        self.editorLabelArgs[4] = systemcolorlabel.width

        editorUI:Enable(self.sizeBtnData,false)
        editorUI:Enable(self.spriteIDInputData,false)

        -- Toggle menu options
        pixelVisionOS:EnableMenuItem(SelectAllShortcut, false)
        pixelVisionOS:EnableMenuItem(ShowBGShortcut, false)
        pixelVisionOS:EnableMenuItem(ShowGrid, false)

        -- Enable the palette panel
        pixelVisionOS:EnableItemPicker(self.paletteColorPickerData, true)

        self:ForcePickerFocus(self.systemColorPickerData)

    elseif(self.mode == SpriteMode) then

        self:ClearCanvasPanelBackground()

        self.toolTitle = "sprites.png"

        -- Make sure the mode button is selected
        if(self.modeButton.selected == true) then
            editorUI:ToggleButton(self.modeButton, false, false)
        end

        self:HideColorPanel()
        self:ShowCanvasPanel()
       
        self:ToggleToolBar(true)

        self.editorLabelArgs[1] = spriteeditorlabel.spriteIDs
        self.editorLabelArgs[4] = spriteeditorlabel.width

        editorUI:Enable(self.sizeBtnData, true)
        editorUI:Enable(self.spriteIDInputData, true)

        -- Toggle menu options
        pixelVisionOS:EnableMenuItem(SelectAllShortcut, true)
        pixelVisionOS:EnableMenuItem(ShowBGShortcut, true)
        pixelVisionOS:EnableMenuItem(ShowGrid, true)

        self:ForcePickerFocus(self.spritePickerData)

    end

    editorUI:NewDraw("DrawSprites", self.editorLabelArgs)

    self:UpdateTitle()

end

function DrawTool:ToggleToolBar(value)

    if(value == false) then

        -- Force the current selection to be enabled so it will display the disabled graphic
        self.toolBtnData.buttons[self.lastSelectedToolID].enabled = true

        editorUI:ClearGroupSelections(self.toolBtnData)
    end

    -- Loop through all of the buttons and toggle them
    for i = 1, #self.toolBtnData.buttons do
        editorUI:Enable(self.toolBtnData.buttons[i], value)
    end

    editorUI:Enable(self.flipHButton, value)
    editorUI:Enable(self.flipVButton, value)

    if(value == true and self.lastSelectedToolID ~= nil) then

        -- Restore last selection
        editorUI:SelectToggleButton(self.toolBtnData, self.lastSelectedToolID, true)

    end

end

function DrawTool:OnSelectTool(value)

    -- Save the current tool selection ID
    self.lastSelectedToolID = Clamp(value, 1, #tools)

    local toolName = tools[value]

    pixelVisionOS:ChangeCanvasTool(self.canvasData, toolName)

    -- We disable the color selection when switching over to the eraser
    if(toolName == "eraser" or  toolName == "select") then

        --  Clear the current color selection
        pixelVisionOS:ClearItemPickerSelection(self.paletteColorPickerData)

        -- Disable the color picker
        pixelVisionOS:EnableItemPicker(self.paletteColorPickerData, false)

    else

        -- Change to fill mode if shift is down
        if(Key(Keys.LeftShift) or Key(Keys.RightShift)) then

            self:ToggleFill(true)

        else

            self:ToggleFill(false)

        end

        -- Make sure the palette picker is enabled
        pixelVisionOS:EnableItemPicker(self.paletteColorPickerData, true)

        -- Restore the last palette selection
        if(self.paletteColorPickerData.currentSelection == -1) then

            -- Need to calculate the exact position if the pages have changed
            -- pixelVisionOS:SelectColorPickerIndex(self.paletteColorPickerData, self.lastPaletteColorID)

            local index = self.lastPaletteColorID % self.paletteColorPickerData.totalPerPage
            local newID = ((self.paletteColorPickerData.pages.currentSelection - 1)* self.paletteColorPickerData.totalPerPage) + index

            -- if(self.lastPaletteColorID ~= newID) then

            pixelVisionOS:SelectColorPickerIndex(self.paletteColorPickerData, newID)
            -- end
            -- print("test", index, newID, self.lastPaletteColorID)
        end

    end

end

function DrawTool:ToggleFill(value)
    
    -- TODO need to change the value of the canvas
    

    local boxButton = self.toolBtnData.buttons[4]
    local circleButton = self.toolBtnData.buttons[5]

    local resetBoxButton = false
    local resetCircleButton = false

    if(self.canvasData.tool == "box") then
        
        boxButton.spriteName = "box" .. (value == true and "fill" or "")
        editorUI:RebuildSpriteCache(boxButton)

        resetCircleButton = true

    elseif(self.canvasData.tool == "circle") then
        
        circleButton.spriteName = "circle" .. (value == true and "fill" or "")
        editorUI:RebuildSpriteCache(circleButton)

        resetBoxButton = true
    else
        
        resetBoxButton = true
        resetCircleButton = true
        
        -- Reset the fill value regardless of the value
        value = false
    end

    if(boxButton.spriteName == "boxfill" and resetBoxButton == true) then
        boxButton.spriteName = "box"
        editorUI:RebuildSpriteCache(boxButton)
    end

    if(circleButton.spriteName == "circlefill"  and resetCircleButton == true) then
        circleButton.spriteName = "circle"
        editorUI:RebuildSpriteCache(circleButton)
    end

    pixelVisionOS:ToggleCanvasFill(self.canvasData, value)
    

    -- TODO set fill to match the brush color

    -- pixelVisionOS:ToggleCanvasFill(self.canvasData, false)

end