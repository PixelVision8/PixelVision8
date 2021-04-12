
function PaintTool:CreateToolbar()


    self.tools = {

        {
            name = "pointer",
            key = Keys.V,
            toolTip = "This is the pointer"
        },
        {
            name = "hand",
            key = Keys.Space,
            toolTip = "This is the pointer"
        },
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

        {"pointer", "hand", "select"},
        {"pen", "eraser"},
        {"line", "circle", "circlefill", "rectangle", "rectanglefill"},
        {"eyedropper", "fill"}
    }


    self.toolbarPos = NewPoint(4, 23)

    self.toolBtnData = editorUI:CreateToggleGroup()
    self.toolBtnData.onAction = function(value) self:OnPickTool(value) end

    local offsetX = 0

    self.toolButtons = {}

    -- Build tools
    for i = 1, #self.defaultTools do

        offsetX = ((i - 1) * 16) + self.toolbarPos.X
        local rect = {x = offsetX, y = self.toolbarPos.Y, w = 16, h = 16}

        local btn = editorUI:CreateButton(rect, self.defaultTools[i].name, self.defaultTools[i].toolTip .. " - shortcut: ".. tostring(self.defaultTools[i].key))
        btn.onAction = function() self:OnPickTool(i) end
            
        table.insert(self.toolButtons, btn)

    end

    self.totalToolButtons = #self.toolButtons

    -- TODO this should restore the last tool selection (also need to account for sub tool if previously selected)
    self:OnPickTool(1, false)

    pixelVisionOS:RegisterUI({name = "OnUpdateToolbar"}, "UpdateToolbar", self, true)

end

function PaintTool:OnPickTool(value, displayOptions)

    if(self.lastSelection == value and self.optionMenuJustClosed == true) then

        displayOptions  = false

        -- return

    end

    if(self.lastSelection ~= nil) then
        
        local tmpBtn = self.toolButtons[self.lastSelection]
        
        tmpBtn.selected = false

        editorUI:Invalidate(tmpBtn)

    end

    if(displayOptions ~= false) then
        
        self:OpenOptionMenu(value)

    end

    self.lastSelection = value

    local currentButton = self.toolButtons[self.lastSelection]

    currentButton.selected = true

    editorUI:Invalidate(currentButton, true)

    self:OnSelectTool(currentButton.spriteName)

    

    -- TODO need all the buttons horizontally and then the drop down so everything is selectable

    self.displayOptions = displayOptions
    
end

function PaintTool:OnSelectTool(value)

    -- self:CloseOptionMenu()
   
    if(self.lastSelection == nil) then
        return
    end

    local btn = self.toolButtons[self.lastSelection]
    btn.selected = true
    btn.spriteName = value

    -- Find the next sprite for the button
    local spriteName = btn.spriteName

    -- Change sprite button graphic
    btn.cachedMetaSpriteIds = {
        up = FindMetaSpriteId(spriteName .. "up"),
        down = FindMetaSpriteId(spriteName .. "down") ~= -1 and FindMetaSpriteId(spriteName .. "down") or FindMetaSpriteId(spriteName .. "selectedup"),
        over = FindMetaSpriteId(spriteName .. "over"),
        selectedup = FindMetaSpriteId(spriteName .. "selectedup"),
        selectedover = FindMetaSpriteId(spriteName .. "selectedover"),
        selecteddown = FindMetaSpriteId(spriteName .. "selecteddown") ~= -1 and FindMetaSpriteId(spriteName .. "selecteddown") or FindMetaSpriteId(spriteName .. "selectedover"),
        disabled = FindMetaSpriteId(spriteName .. "disabled"),
        empty = FindMetaSpriteId(spriteName .. "empty") -- used to clear the sprites
    }

    for i = 1, self.totalToolButtons do
        editorUI:Invalidate(self.toolButtons[i])
    end

    self:ChangeCanvasTool(spriteName)

end

function PaintTool:UpdateToolbar()

    if(self.optionMenuOpen == true and editorUI.collisionManager.mouseReleased) then
        
        self:CloseOptionMenu()
        
    end

    for i = 1, self.totalToolButtons do
        
        editorUI:UpdateButton(self.toolButtons[i])

    end

    -- editorUI:UpdateToggleGroup(self.toolBtnData)

    if(Key(Keys.LeftControl) == false and Key(Keys.RightControl) == false) then

        -- TODO need to reconnect this based on a master list of keys?
        -- for i = 1, #toolKeys do
        --     if(Key(toolKeys[i], InputState.Released)) then
        --         print("Select Tool", i)
        --         -- editorUI:SelectToggleButton(self.toolBtnData, i)
        --         break
        --     end
        -- end
    end

    if(self.optionMenuOpen == true) then

        for i = 1, #self.optionButtons do
            editorUI:UpdateButton(self.optionButtons[i])
        end

    end

    self.optionMenuJustClosed = false

end

function PaintTool:OpenOptionMenu(value)

    self.optionMenuOpen = true


    if(value == self.lastSelection) then
        
        return
    end

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