local toolbarID = "ToolBarUI"
local tools = {"pointer", "pen", "eraser", "fill"}
local toolKeys = {Keys.v, Keys.P, Keys.E, Keys.F}

function TilemapTool:CreateToolbar()

    self.toolBtnData = editorUI:CreateToggleGroup()
    self.toolBtnData.onAction = function(value) self:OnSelectTool(value) end

    for i = 1, #tools do
        local offsetX = ((i - 1) * 16) + 160
        local rect = {x = offsetX, y = 56, w = 16, h = 16}
        editorUI:ToggleGroupButton(self.toolBtnData, rect, tools[i], "Select the '" .. tools[i] .. "' (".. tostring(toolKeys[i]) .. ") tool.")

        table.insert(self.enabledUI, self.toolBtnData.buttons[i])

    end

    self.flagBtnData = editorUI:CreateToggleButton({x = 232, y = 56}, "flag", "Toggle between tilemap and flag layers (CTRL+L).")

    table.insert(self.enabledUI, self.flagBtnData)

    self.flagBtnData.onAction = function(value) self:ChangeMode(value) end

    pixelVisionOS:RegisterUI({name = toolbarID}, "UpdateToolbar", self)
    
end

function TilemapTool:UpdateToolbar()

    --editorUI:UpdateButton(self.sizeBtnData)
    editorUI:UpdateButton(self.flagBtnData)
    editorUI:UpdateToggleGroup(self.toolBtnData)

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

function TilemapTool:OnSelectTool(value)

    toolMode = value

    -- Clear the last draw id when switching modes
    lastDrawTileID = -1

    -- TODO not sure why this was in here
    --lastSpriteSize = self.spriteSize
    --
    --local lastID = self.spritePickerData.currentSelection

    if(toolMode == 1) then

        -- Clear the sprite picker and tilemap picker
        pixelVisionOS:ClearItemPickerSelection(self.tilePickerData)

    elseif(toolMode == 2 or toolMode == 3) then

        -- Clear any tilemap picker selection
        pixelVisionOS:ClearItemPickerSelection(self.tilePickerData)

    end

    pixelVisionOS:ChangeTilemapPickerMode(self.tilePickerData, toolMode)


end

function TilemapTool:ChangeMode(value)

    -- If value is true select layer 2, if not select layer 1
    self:SelectLayer(value == true and 2 or 1)

    -- Set the flag mode to the value
    self.flagModeActive = value

    -- If value is true we are in the flag mode
    if(value == true) then
        self.lastBGState = self.tilePickerData.showBGColor

        self.tilePickerData.showBGColor = false

        -- Disable bg menu option
        pixelVisionOS:EnableMenuItem(BGColorShortcut, false)

        pixelVisionOS:InvalidateItemPickerDisplay(self.tilePickerData)

        --self:DrawFlagPage()
        
        self:HidePalette()
        self:ShowFlags()

    else
        -- Swicth back to tile modes

        -- Restore background color state
        self.tilePickerData.showBGColor = self.lastBGState

        -- editorUI:Ena ble(bgBtnData, true)
        pixelVisionOS:EnableMenuItem(BGColorShortcut, true)


        pixelVisionOS:RebuildPickerPages(self.paletteColorPickerData)
        pixelVisionOS:SelectColorPage(self.paletteColorPickerData, 1)
        pixelVisionOS:InvalidateItemPickerDisplay(self.paletteColorPickerData)

        self:HideFlags()
        self:ShowPalette()
        

    end

    -- Fix the button state if triggered outside of the button
    if(self.flagBtnData.selected ~= value) then

        editorUI:ToggleButton(self.flagBtnData, value, false)

    end

    -- Clear history between layers
    self:ClearHistory()

end