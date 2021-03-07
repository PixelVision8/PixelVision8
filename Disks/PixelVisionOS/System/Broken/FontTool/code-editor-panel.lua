--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- SaveShortcut = 5

function FontEditorTool:CreateEditorPanel()

    self.currentCharID = -1
    self.characterWindow = {x = 152, y = 72, w = 96, h = 64}
    self.WHITE = 15
    self.message = "The quick brown fox jumps over the lazy dog."
    self.tools = {"pen", "eraser"}
    self.missingSpriteId = MetaSprite(FindMetaSpriteId("missingchar")).Sprites[1].Id

    self.charPicker = editorUI:CreatePicker(
        {x = 152, y = 72, w = 96, h = 64 },
        8,
        8,
        95,
        "charselection",
        PaletteOffset(1),
        "Pick a char"
    )

    self.charPicker.onAction = function(value) self:OnSelectChar(value) end

    for i = 1, #self.characters do

        -- Lua loops start at 1 but we need to start at 0
        self:DrawFontCharacter(i - 1)

    end


    self.spacingStepper = editorUI:CreateStringStepper({x = 200, y = 160}, 16, "0", {"-4", "-3", "-2", "-1", "0", "1", "2"}, "", "Change the length of the pattern.")

    -- TODO This is a hack becuse entering in numbers by hand could break the return value... example 2 becomes 20.
    self.spacingStepper.inputField.highlighterTheme.disabled = 15
    editorUI:Enable(self.spacingStepper.inputField, false)

    self.spacingStepper.onInputAction = function(value) self:DrawSampleText(value) end

    self.charStepper = editorUI:CreateStringStepper({x = 152, y = 160}, 8, "A", self.characters, "", "Change the length of the pattern.")

    self.charStepper.onInputAction = function(value) self:OnEnterChar(value) end

    -- local pathSplit = string.split(rootDirectory, "/")



    -- Update title with file path
    -- toolTitle = pathSplit[#pathSplit - 1] .. "/" .. fontName

    self.fontNameInputData = editorUI:CreateInputField({x = 152, y = 40, w = 96}, string.split(self.fontName, ".")[1], "Give the font a name.", "file")
    -- fontNameInputData.min = 0
    self.fontNameInputData.onAction = function(value) self:OnFontNameChange(value) end

    pixelVisionOS:RegisterUI({name = "EditorPanelUpdateLoop"}, "FontPanelUpdate", self)

end

function FontEditorTool:FontPanelUpdate()  

    editorUI:UpdatePicker(self.charPicker)
    editorUI:UpdateInputField(self.fontNameInputData)
    editorUI:UpdateStepper(self.spacingStepper)
    editorUI:UpdateStepper(self.charStepper)
    
end

function FontEditorTool:DrawFontCharacter(id )

    local pos = CalculatePosition(id, math.floor(self.characterWindow.w / 8))

    local pixelData = gameEditor:FontSprite(id)

    if(self:IsEmpty(pixelData)) then

        -- TODO need to read the pixel data from the meta sprite
        -- pixelData = Sprite(_G["fontchar" .. id].spriteIDs[1])

    else

        for j = 1, #pixelData do
            if(pixelData[j] < 0) then
                -- Set the pixel data to the first color
                pixelData[j] = 0
            else
                -- Convert the pixel to white
                pixelData[j] = self.WHITE--pixelData[j] + colorOffset
            end
        end

    end
    -- need to replace transparent color


    local x = (pos.x * 8) + self.characterWindow.x
    local y = (pos.y * 8) + self.characterWindow.y

    DrawPixels(pixelData, x, y, 8, 8, false, false, DrawMode.TilemapCache)

end


function FontEditorTool:OnEnterChar(value)

    local charID = table.indexOf(self.characters, value)

    if(charID ~= -1) then
        editorUI:SelectPicker(self.charPicker, charID - 1)
    end


    self:OnSelectChar(charID - 1)

end

function FontEditorTool:OnSelectChar(value)

    if(self.currentCharID == value) then
        return
    end

    local lastCharID = self.currentCharID

    local visibleWidth = math.floor(self.charPicker.rect.w / 8)

    local pos = CalculatePosition(value, visibleWidth)

    self.currentCharID = CalculateIndex(pos.x, pos.y, visibleWidth)

    local char = self.characters[self.currentCharID + 1]
   
    editorUI:ChangeStringStepperValue(self.charStepper, char, false)
    
    -- Save the original pixel data from the selection
    local tmpPixelData = gameEditor:FontSprite(self.currentCharID)

    if(lastCharID ~= self.currentCharID) then
        -- Clear the original pixel data
        
        -- Need to loop through the pixel data and change the offset
        local total = #tmpPixelData
        for i = 1, total do

            if(tmpPixelData[i] > - 1) then

                -- Convert the pixel data to white
                tmpPixelData[i] = self.WHITE
            else
                tmpPixelData[i] = -1
            end

        end

    end

    local scale = 16 -- TODO need to get the real scale
    local size = NewPoint(8, 8)

    -- TODO simulate selecting the first sprite
    pixelVisionOS:ResizeCanvas(self.canvasData, size, scale, tmpPixelData)

    pixelVisionOS:ChangeCanvasTool(self.canvasData, self.tools[1])

    pixelVisionOS:EnableMenuItem(self.RevertShortcut, false)

    -- Only enable the clear menu when the sprite is not empty
    pixelVisionOS:EnableMenuItem(self.ClearShortcut, not self:IsEmpty(tmpPixelData))


    pixelVisionOS:EnableMenuItem(self.CopyShortcut, true)

    -- self:ClearHistory()
    -- print("OnSelectChar", value)

end

function FontEditorTool:DrawSampleText(spacing)

    local rect = NewRect(8, 192, 240, 16)

    DrawRect(rect.x, rect.y, rect.width, rect.height, 0, DrawMode.TilemapCache)

    spacing = spacing or tonumber(editorUI:GetStringStepperValue(self.spacingStepper))
    
    local charWidth = 8 + spacing

    local wrap = WordWrap(self.message, math.floor(rect.width / charWidth))
    local lines = SplitLines(wrap)
    local total = #lines
    -- local startY = (rect.y + rect.height / 8) - total

    local charOffset = 32

    -- Make the text white (based on the tool's own colors)
    -- local colorOffset = 15

    -- We want to render the text from the bottom of the screen so we offset it and loop backwards.
    for i = total, 1, - 1 do

        local line = lines[i]

        -- Calculated once per line
        local y = ((i - 1) * 8) + rect.y

        for j = 1, #line do
            local charID = string.byte (line, j) - charOffset

            local pixelData = gameEditor:FontSprite(charID)

            if(self:IsEmpty(pixelData) and charID ~= 0) then

                -- TODO need to find empty sprite
                pixelData = Sprite(self.missingSpriteId)

            end

            local x = ((j - 1) * charWidth) + rect.x

            DrawPixels(pixelData, x, y, 8, 8, false, false, DrawMode.TilemapCache, self.WHITE)

        end

    end

end

function FontEditorTool:OnFontNameChange(value)

    if(self.fontName ~= value .. ".font.png") then
        self:InvalidateData()
        -- print("Rename Font", value)
    end

end

function FontEditorTool:IsEmpty(spriteData)

    for i = 1, #spriteData do

        if(spriteData[i] > - 1) then
            return false
        end
    end

    return true

end