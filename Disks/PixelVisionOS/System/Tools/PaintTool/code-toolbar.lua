
function PaintTool:CreateToolbar()


    self.tools = {

        {
            name = "pointer",
            key = Keys.V,
            toolTip = "This is the pointer"
        },
        -- {
        --     name = "hand",
        --     key = Keys.Space,
        --     toolTip = "This is the pointer"
        -- },
        {
            name = "select",
            key = Keys.M,
            toolTip = "This is the selection"
        },
        {
            name = "pen",
            key = Keys.B,
            toolTip = "This is the pen"
        },
        {
            name = "eraser",
            key = Keys.E,
            toolTip = "This is the eraser"
        },
        {
            name = "line",
            key = Keys.L,
            toolTip = "This is the line"
        },
        {
            name = "circle",
            key = Keys.C,
            toolTip = "This is the circle"
        },
        {
            name = "circlefill",
            key = Keys.C,
            shift = true,
            toolTip = "This is the circle fill"
        },
        {
            name = "rectangle",
            key = Keys.R,
            toolTip = "This is the rectangle"
        },
        {
            name = "rectanglefill",
            key = Keys.R,
            shift = true,
            toolTip = "This is the circle rectangle"
        },
        {
            name = "eyedropper",
            key = Keys.I,
            toolTip = "This is the eye dropper"
        },
        {
            name = "fill",
            key = Keys.F,
            toolTip = "This is the fill"
        },

    }

    self.defaultTools = self:GetToolButton({"pointer", "pen", "line", "eyedropper"})

    self.toolOptions ={

        {"pointer", "select"}, --"hand", 
        {"pen", "eraser"},
        {"line", "circle", "circlefill", "rectangle", "rectanglefill"},
        {"eyedropper", "fill"}
    }

    self.shapeTools = 3
    self.fillTools = 4

    self.toolbarPos = NewPoint(4, 23)

    self.toolBtnData = editorUI:CreateToggleGroup()
    self.toolBtnData.onAction = function(value) self:OnPickTool(value) end

    local offsetX = 0

    self.toolButtons = {}

    -- Build tools
    for i = 1, #self.defaultTools do
  
        -- Shift the offset over to the right
        offsetX = ((i - 1) * 16) + self.toolbarPos.X
        
        -- Create the rect for the button
        local rect = {x = offsetX, y = self.toolbarPos.Y, w = 16, h = 16}

        -- Create the new button and its action
        local btn = editorUI:CreateButton(rect, self.defaultTools[i].name, self.defaultTools[i].toolTip .. " - shortcut: ".. tostring(self.defaultTools[i].key))
        btn.onAction = function() self:OnPickTool(i) end
            
        -- Add the button to the table
        table.insert(self.toolButtons, btn)

    end

    -- Save the total number of buttons
    self.totalToolButtons = #self.toolButtons

    -- Register the update loop
    pixelVisionOS:RegisterUI({name = "OnUpdateToolbar"}, "UpdateToolbar", self, true)

end

function PaintTool:OnPickTool(value, displayOptions)

    if(displayOptions ~= false) then

        self.optionMenuOpen = true

    end
        
    self:OpenOptionMenu(value)

    self:OnSelectTool(self.toolButtons[value].spriteName)

    self.displayOptions = displayOptions
    
end

function PaintTool:OnSelectTool(value)

    -- Store values for buttonId and option
    local buttonId = 0
    local optionId = 0

    -- Loop through buttons
    for i = 1, #self.toolOptions do
        
        -- Loop through options
        for j = 1, #self.toolOptions[i] do
            
            -- Look for name
            if(self.toolOptions[i][j] == value) then

                -- Save values since we found the button
                buttonId = i
                optionId = j

                -- Exit the for loop
                break

            end

        end

    end

    -- If no tool was found, exit out of the function
    if(buttonId == 0 or optionId == 0 or self.toolButtons[buttonId].enabled == false) then
        return
    end

    -- Check to see if we need to unselect the previous button
    if(self.lastButtonId ~= nil and self.lastButtonId ~= buttonId) then

        -- Unselect the previous button
        local tmpBtn = self.toolButtons[self.lastButtonId]
        
        -- Set the previous button's selected value to false
        tmpBtn.selected = false

        -- Invalidate the button it redraws on the next frame
        editorUI:Invalidate(tmpBtn)

    end

    -- Save the last buttonId
    self.lastButtonId = buttonId

    -- Select the current button
    local currentButton = self.toolButtons[self.lastButtonId]

    -- Change the current button's selected value to true
    currentButton.selected = true
    
    -- Rebuild the button's sprite cache with the tool's name
    editorUI:RebuildMetaSpriteCache(currentButton, value)
    
    -- Pass the new tool value over to the canvas
    self:ChangeCanvasTool(value)

end

function PaintTool:UpdateToolbar()

    -- Check to see if the option menu should be closed
    if(self.optionMenuOpen == true and editorUI.collisionManager.mouseReleased) then
        
        -- Close the option window
        self:CloseOptionMenu()
        
    end

    -- Update all of the buttons
    for i = 1, self.totalToolButtons do
        
        editorUI:UpdateButton(self.toolButtons[i])

    end

    -- Check if the options menu is open
    if(self.optionMenuOpen == true) then

        -- Loop through all of the option buttons and update them
        for i = 1, #self.optionButtons do
            editorUI:UpdateButton(self.optionButtons[i])
        end

    end

    self.optionMenuJustClosed = false

    if(Key(Keys.LeftControl) == false and Key(Keys.RightControl) == false) then
        -- Loop through all of the tool's data
        for i = 1, #self.tools do
            
            local tmpTool = self.tools[i]

            local usesShift = tmpTool.shift or false

            if(Key(tmpTool.key, InputState.Released) and (Key(Keys.LeftShift) == usesShift or Key(Keys.RightShift) == usesShift)) then
                self:OnSelectTool(tmpTool.name)
            end

        end
    end

end

function PaintTool:OpenOptionMenu(value)

    -- Save option buttons
    local optionButtonLabels = self:GetToolButton(self.toolOptions[value], self.toolButtons[value].spriteName)

    local pos = NewPoint( self.toolbarPos.X + (16 * (value-1)), self.toolbarPos.Y - 16)

    self.optionButtons = {}

    local bX = pos.X
    local bY = pos.Y + 16

    for i = 1, #optionButtonLabels do

        local tmpButton = optionButtonLabels[i]
        bY = bY + 16
        local tmpBtn = editorUI:CreateButton({x = bX, y = bY}, tmpButton.name, tmpButton.tooltip or "")
        tmpBtn.drawMode = DrawMode.Sprite

        tmpBtn.onPress = function() self:OnSelectTool(tmpBtn.spriteName) end
    
        table.insert(self.optionButtons, tmpBtn)

    end
    
end

function PaintTool:CloseOptionMenu()
    self.optionMenuOpen = false
    self.optionMenuJustClosed = true
    editorUI:ClearFocus()
end

function PaintTool:GetToolButton(ids, selected)

    local buttons = {}
    local totalTools = #self.tools
    local total = #ids

    for i = 1, total do
        
        for j = 1, totalTools do

            local tmpBtn = self.tools[j]

            if(tmpBtn.name == ids[i]) then

                local data = {
                    name = tmpBtn.name,
                    key = tmpBtn.key,
                    shift = tmpBtn.shift == true,
                    toolTip = tmpBtn.toolTip
                }

                -- Make the first button the current selection
                if(tmpBtn.name ~= selected) then
                --     table.insert(buttons, 1, data)
                -- else
                    table.insert(buttons, data)
                end

            end

        end
        
    end

    return buttons

end