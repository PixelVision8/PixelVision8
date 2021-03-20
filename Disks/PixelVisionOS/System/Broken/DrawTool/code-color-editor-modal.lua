--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

EditColorModal = {}
EditColorModal.__index = EditColorModal

function EditColorModal:Init(editorUI, maskColor, title)

    local _editColorModal = {} -- our new object
    setmetatable(_editColorModal, EditColorModal) -- make Account handle lookup

    _editColorModal.title = title or "Edit Color"
    _editColorModal.editorUI = editorUI
    _editColorModal.maskColor = maskColor
    _editColorModal.warningMetaSpriteId = FindMetaSpriteId("colorwarningicon")

    _editColorModal:Configure()
    -- _editColorModal.currentSelection = 1
    -- _editColorModal.message = message

    return _editColorModal

end

function EditColorModal:Configure()

    self.colorCache = {}

    self.tmpColor = 255

    self.rgbMode = true

    local width = 224 + 16
    local height = 200 - 8

    self.canvas = NewCanvas(width, height)

    local displaySize = Display()

    -- self.title = "Edit Color"

    self.rect = {
        x = math.floor(((displaySize.x - width) * .5) / 8) * 8,
        y = math.floor(((displaySize.y - height) * .5) / 8) * 8,
        w = width,
        h = height
    }

    self.selectionValue = false

    self.invalidateUI = {}

    -- Draw the black background
    self.canvas:SetStroke(5, 1)
    self.canvas:SetPattern({0}, 1, 1)
    self.canvas:DrawRectangle(0, 0, self.canvas.width, self.canvas.height, true)

    -- Draw the brown background
    self.canvas:SetStroke(12, 1)
    self.canvas:SetPattern({11}, 1, 1)
    self.canvas:DrawRectangle(3, 9, self.canvas.width - 6, self.canvas.height - 12, true)

    local tmpX = (self.canvas.width - (#self.title * 4)) * .5

    self.canvas:DrawText(self.title:upper(), tmpX, 1, "small", 15, - 4)

    -- draw highlight stroke
    self.canvas:SetStroke(15, 1)
    self.canvas:DrawLine(3, 9, self.canvas.width - 5, 9)
    self.canvas:DrawLine(3, 9, 3, self.canvas.height - 5)

    self.buttons = {}

    -- TODO Create button states?

    local buttonSize = {x = 32, y = 16}

    -- TODO center ok button when no cancel button is shown
    local bX = (self.rect.w - buttonSize.x - 8)

    -- snap the x value to the grid
    bX = math.floor((bX + self.rect.x) / 8) * 8

    -- Fix the button to the bottom of the window
    local bY = math.floor(((self.rect.y + self.rect.h) - buttonSize.y - 8) / 8) * 8

    local backBtnData = editorUI:CreateButton({x = bX, y = bY}, "modalokbutton", "")

    backBtnData.onAction = function()

        -- Set value to true when ok is pressed
        self.selectionValue = true

        if(self.onParentClose ~= nil) then
            self.onParentClose()
        end
    end

    table.insert(self.buttons, backBtnData)

    -- Offset the bX value and snap to the grid
    bX = math.floor((bX - buttonSize.x - 8) / 8) * 8

    local cancelBtnData = editorUI:CreateButton({x = bX, y = bY}, "modalcancelbutton", "")

    cancelBtnData.onAction = function()

        -- Set value to true when cancel is pressed
        self.selectionValue = false

        -- Close the panel
        if(self.onParentClose ~= nil) then
            self.onParentClose()
        end
    end

    table.insert(self.buttons, cancelBtnData)

    -- Create two rects for the picker
    self.colorSpaceRect = { x = self.rect.x + 16, y = self.rect.y + 32, w = 80, h = 80}
    self.greySpaceRect = { x = self.rect.x + 16, y = self.rect.y + 96 + 16, w = 80, h = 8}

    local colorPickerButton = editorUI:CreateButton({ x = self.colorSpaceRect.x, y = self.colorSpaceRect.y, w = self.colorSpaceRect.w, h = self.colorSpaceRect.h + self.greySpaceRect.h}, nil, "Pick a color.")
    -- TODO need to figure out a way  to change the focus
    colorPickerButton.buttonCursor = 8
    
    colorPickerButton.onPress = function(value)

        self.updateColorPicker = true

    end

    colorPickerButton.onAction = function(value)
       
        self.updateColorPicker = false
    end

    table.insert(self.buttons, colorPickerButton)

    -- Settings
    self.colorHexInputData = editorUI:CreateInputField({x = self.rect.x + 24, y = self.rect.y + 144, w = 48}, "FF00FF", "Hex value of the selected color.", "hex")

    self.colorHexInputData.forceCase = "upper"

    table.insert(self.invalidateUI, self.colorHexInputData)

    -- Red input
    self.redInputData = editorUI:CreateInputField({x = self.rect.x + 200, y = self.rect.y + 80, w = 24}, "000", "Hex value of the selected color.", "number")
    
    self.redInputData.min = 0
    self.redInputData.max = 255

    table.insert(self.invalidateUI, self.redInputData)

    -- Green input
    self.greenInputData = editorUI:CreateInputField({x = self.rect.x + 200, y = self.rect.y + 112, w = 24}, "000", "Hex value of the selected color.", "number")

    self.greenInputData.min = 0
    self.greenInputData.max = 255

    table.insert(self.invalidateUI, self.greenInputData)

    -- blue input
    self.blueInputData = editorUI:CreateInputField({x = self.rect.x + 200, y = self.rect.y + 144, w = 24}, "000", "Hex value of the selected color.", "number")

    self.blueInputData.min = 0
    self.blueInputData.max = 255

    table.insert(self.invalidateUI, self.blueInputData)

    -- Red slider
    self.redSlider = editorUI:CreateSlider(
        { x = self.rect.x + 108, y = self.rect.y + 75, w = 80, h = 16},
        "hsliderhandle",
        "Adjust the red value.",
        true
    )

    -- Green slider
    self.greenSlider = editorUI:CreateSlider(
        { x = self.rect.x + 108, y = self.rect.y + 107, w = 80, h = 16},
        "hsliderhandle",
        "Adjust the green value.",
        true
    )

    -- Blue slider
    self.blueSlider = editorUI:CreateSlider(
        { x = self.rect.x + 108, y = self.rect.y + 139, w = 80, h = 16},
        "hsliderhandle",
        "Adjust the Red value.",
        true
    )

    -- Check boxes
    self.colorModeRadioGroupData = editorUI:CreateToggleGroup(true)
    self.colorModeRadioGroupData.onAction = function(value)
        -- self:OnChangeTrackInstrument(data, value)
        self:ChangeColorMode(value)

        self.redSlider.toolTip = value == 1 and "Change the red value from 0 to 255" or "Change the hue percentage from 0 to 100"
        self.greenSlider.toolTip = value == 1 and "Change the green value from 0 to 255" or "Change the saturation percentage from 0 to 100"
        self.blueSlider.toolTip = value == 1 and "Change the blue value from 0 to 255" or "Change the value percentage from 0 to 100"

    end

    local rgbButton = editorUI:ToggleGroupButton(self.colorModeRadioGroupData, {x = self.rect.x + 184 + 8, y = self.rect.y + 40, w = 8, h = 8}, "radiobutton", "Change the current track's instrument to ", true)

    table.insert(self.invalidateUI, rgbButton)

    local hsvButton = editorUI:ToggleGroupButton(self.colorModeRadioGroupData, {x = self.rect.x + 184 + 8, y = self.rect.y + 48, w = 8, h = 8}, "radiobutton", "Change the current track's instrument to ", true)

    table.insert(self.invalidateUI, hsvButton)

    

    -- Hex input needs to convert to RGB and update the input fields which will update the sliders.
    self.colorHexInputData.onAction = function(value)

        -- TODO need to check which mode we are in for the value

        self:UpdateHexValue("#"..value)

        -- self.showWarning = table.indexOf(self.colorCache, ("#"..value)) > - 1

        self:Invalidate()

    end


    self.redInputData.onAction = function(value)

        value = self.rgbMode == true and (value / 255) or (value / 100)

        editorUI:ChangeSlider(self.redSlider, value, false)

        self:Invalidate()

    end

    self.greenInputData.onAction = function(value)

        value = self.rgbMode == true and (value / 255) or (value / 100)

        editorUI:ChangeSlider(self.greenSlider, value, false)

        self:Invalidate()
    end

    self.blueInputData.onAction = function(value)

        value = self.rgbMode == true and (value / 255) or (value / 100)

        editorUI:ChangeSlider(self.blueSlider, value, false)

        self:Invalidate()
    end

    self.redSlider.onAction = function(value)

        value = self.rgbMode == true and math.ceil(value * 255) or math.ceil(value * 100)

        editorUI:ChangeInputField(self.redInputData, tostring(value), false)

        self:Invalidate()
    end

    self.greenSlider.onAction = function(value)

        value = self.rgbMode == true and math.ceil(value * 255) or math.ceil(value * 100)

        editorUI:ChangeInputField(self.greenInputData, tostring(value), false)

        self:Invalidate()
    end

    self.blueSlider.onAction = function(value)

        value = self.rgbMode == true and math.ceil(value * 255) or math.ceil(value * 100)

        editorUI:ChangeInputField(self.blueInputData, tostring(value), false)

        self:Invalidate()
    end


end

function EditColorModal:Open()

    -- Cache current colors

    self.colorCache = {}

    for i = 1, 128 do

        table.insert(self.colorCache, Color(pixelVisionOS.colorOffset + (i - 1)))

    end

    self.canvas:DrawPixels(self.rect.x, self.rect.y, DrawMode.TilemapCache)

    -- DrawSprites(coloreditorpanel.spriteIDs, self.rect.x + 8, self.rect.y + 16, coloreditorpanel.width, false, false, DrawMode.TilemapCache)

    DrawMetaSprite(FindMetaSpriteId("coloreditorpanel"), self.rect.x + 8, self.rect.y + 16, false, false, DrawMode.TilemapCache)
    
    -- Invalidate all of the UI buttons so they display correctly when re-opening the modal
    for i = 1, #self.buttons do
        editorUI:Invalidate(self.buttons[i])
    end

    local total = #self.invalidateUI
    for i = 1, total do
        editorUI:Invalidate(self.invalidateUI[i])
    end

    -- Save last color mode selection
    local previousSelection = self.colorModeRadioGroupData.currentSelection

    -- Clear previous selection
    editorUI:ClearGroupSelections(self.colorModeRadioGroupData)

    -- Force the color space to redraw
    editorUI:SelectToggleButton(self.colorModeRadioGroupData, previousSelection > 0 and previousSelection or 1)

end

function EditColorModal:SetColor(colorID)

    colorID = colorID or self.tmpColor

    -- print("Set Color", colorID)
    self.editingColorID = colorID
    self.currentColor = Color(colorID)

    editorUI:ChangeInputField(self.colorHexInputData, self.currentColor, true)

    -- TODO convert to RGB and update the input fields



    self:Invalidate()

end

function EditColorModal:ChangeColorMode(value)

    --print("Toggle Color Mode", self.rgbMode, value)

    self.rgbMode = value == 1 and true or false

    if(self.rgbMode == true) then
        self.redInputData.max = 255
        self.greenInputData.max = 255
        self.blueInputData.max = 255

        DrawMetaSprite(FindMetaSpriteId("colorlabelred"), self.rect.x + 112, self.rect.y + 64, false, false, DrawMode.TilemapCache)
        DrawMetaSprite(FindMetaSpriteId("colorlabelgreen"), self.rect.x + 112, self.rect.y + 96, false, false, DrawMode.TilemapCache)
        DrawMetaSprite(FindMetaSpriteId("colorlabelblue"), self.rect.x + 112, self.rect.y + 128, false, false, DrawMode.TilemapCache)

        DrawMetaSprite(FindMetaSpriteId("rgbcolorspace"), self.rect.x + 16, self.rect.y + 32, false, false, DrawMode.TilemapCache)

    else
        self.redInputData.max = 100
        self.greenInputData.max = 100
        self.blueInputData.max = 100

        DrawMetaSprite(FindMetaSpriteId("colorlabelhue"), self.rect.x + 112, self.rect.y + 64, false, false, DrawMode.TilemapCache)
        DrawMetaSprite(FindMetaSpriteId("colorlabelsaturation"), self.rect.x + 112, self.rect.y + 96, false, false, DrawMode.TilemapCache)
        DrawMetaSprite(FindMetaSpriteId("colorlabelvalue"), self.rect.x + 112, self.rect.y + 128, false, false, DrawMode.TilemapCache)

        DrawMetaSprite(FindMetaSpriteId("hsvcolorspace"), self.rect.x + 16, self.rect.y + 32, false, false, DrawMode.TilemapCache)

    end

    self:UpdateHexValue("#"..self.colorHexInputData.text)
    
end

function EditColorModal:Close()

    Color(self.tmpColor, self.maskColor)

end

function EditColorModal:Update(timeDelta)

    if(self.updateColorPicker == true) then
        
        if(editorUI.collisionManager:MouseInRect(self.colorSpaceRect)) then

                if(self.rgbMode == true) then
    
                    self.tmpR = ((editorUI.collisionManager.mousePos.x - self.colorSpaceRect.x) / self.colorSpaceRect.w) * 255
                    self.tmpG = ((editorUI.collisionManager.mousePos.y - self.colorSpaceRect.y) / self.colorSpaceRect.h ) * 255
                    self.tmpB = (1 - (editorUI.collisionManager.mousePos.x - self.colorSpaceRect.x) / self.colorSpaceRect.w) * 255
    
                else
    
                    self.tmpH = (editorUI.collisionManager.mousePos.x - self.colorSpaceRect.x) / self.colorSpaceRect.w
    
                    -- TODO need to flip this value horizontally
                    self.tmpS = ((editorUI.collisionManager.mousePos.y - self.colorSpaceRect.y + 10) / (self.colorSpaceRect.h + 20))
    
                    -- This is always 100% when in the picker
                    self.tmpV = 1 - (((editorUI.collisionManager.mousePos.y - self.colorSpaceRect.y) / (self.colorSpaceRect.h + 30)))
    
                    self.tmpR, self.tmpG, self.tmpB = self:HSVToRGB(self.tmpH, self.tmpS, self.tmpV)
    
                end
    
                local newHex = self:RGBToHex(self.tmpR, self.tmpG, self.tmpB)
    
                self:UpdateHexValue(newHex)
    
        elseif(editorUI.collisionManager:MouseInRect(self.greySpaceRect)) then
        
            local h = 0

            -- TODO need to flip this value horizontally
            local s = 0

            -- This is always 100% when in the picker
            local v = (editorUI.collisionManager.mousePos.x - self.colorSpaceRect.x) / self.colorSpaceRect.w

            local r, g, b = self:HSVToRGB(h, s, v)

            local hex = self:RGBToHex(r, g, b)

            self:UpdateHexValue(hex)

        end

    end

    if(self.invalid == true) then

        local newHex = nil

        if(self.rgbMode == true) then

            newHex = self:RGBToHex(
                -- {
                tonumber(self.redInputData.text),
                tonumber(self.greenInputData.text),
                tonumber(self.blueInputData.text)
                -- }
            )

        else

            local r, g, b = self:HSVToRGB(
                tonumber(self.redInputData.text) / 100,
                tonumber(self.greenInputData.text) / 100,
                tonumber(self.blueInputData.text) / 100
            )

            newHex = self:RGBToHex(r, g, b )

        end

        editorUI:ChangeInputField(self.colorHexInputData, newHex:sub(2, - 1), false)

        self.showWarning = table.indexOf(self.colorCache, (newHex)) > - 1


        -- Set tmp color
        Color(self.tmpColor, newHex)

        DrawRect(self.rect.x + 144, self.rect.y + 32, 24, 24, self.editingColorID, DrawMode.TilemapCache)

        DrawRect(self.rect.x + 120, self.rect.y + 32, 24, 24, self.tmpColor, DrawMode.TilemapCache)

        self.invalid = false

    end

    for i = 1, #self.buttons do
        editorUI:UpdateButton(self.buttons[i])
    end
    --
    editorUI:UpdateInputField(self.colorHexInputData)
    editorUI:UpdateInputField(self.redInputData)
    editorUI:UpdateInputField(self.greenInputData)
    editorUI:UpdateInputField(self.blueInputData)

    editorUI:UpdateSlider(self.redSlider)
    editorUI:UpdateSlider(self.greenSlider)
    editorUI:UpdateSlider(self.blueSlider)

    editorUI:UpdateToggleGroup(self.colorModeRadioGroupData)

end

function EditColorModal:UpdateHexValue(value)

    local r, g, b = self:HexToRGB(value)

    if(self.rgbMode == false) then

        r, g, b = self:RGBToHSV(r, g, b)

        r = math.ceil(r * 100)
        g = math.ceil(g * 100)
        b = math.ceil(b * 100)

    end

    --TODO need to force these to update
    editorUI:ChangeInputField(self.redInputData, tostring(r))
    editorUI:ChangeInputField(self.greenInputData, tostring(g))
    editorUI:ChangeInputField(self.blueInputData, tostring(b))

    self.showWarning = table.indexOf(self.colorCache, (value)) > - 1

end

function EditColorModal:Invalidate()
    self.invalid = true
end

function EditColorModal:HexToRGB(hex)
    hex = hex:gsub("#", "")

    return tonumber("0x"..hex:sub(1, 2), 16), tonumber("0x"..hex:sub(3, 4), 16), tonumber("0x"..hex:sub(5, 6), 16)
end

function EditColorModal:RGBToHex(r, g, b)
    return string.format("#%.2X%.2X%.2X", r, g, b)
end

-- Color conversion by Emmanuel Oga from - https://github.com/EmmanuelOga/columns/blob/master/utils/color.lua

--[[
 * Converts an HSV color value to RGB. Conversion formula
 * adapted from http://en.wikipedia.org/wiki/HSV_color_space.
 * Assumes h, s, and v are contained in the set [0, 1] and
 * returns r, g, and b in the set [0, 255].
 *
 * @param   Number  h       The hue
 * @param   Number  s       The saturation
 * @param   Number  v       The value
 * @return  Array           The RGB representation
]]
function EditColorModal:HSVToRGB(h, s, v)
    local r, g, b

    local i = math.floor(h * 6);
    local f = h * 6 - i;
    local p = v * (1 - s);
    local q = v * (1 - f * s);
    local t = v * (1 - (1 - f) * s);

    i = i % 6

    if i == 0 then r, g, b = v, t, p
    elseif i == 1 then r, g, b = q, v, p
    elseif i == 2 then r, g, b = p, v, t
    elseif i == 3 then r, g, b = p, q, v
    elseif i == 4 then r, g, b = t, p, v
    elseif i == 5 then r, g, b = v, p, q
    end

    return r * 255, g * 255, b * 255
end

--[[
 * Converts an RGB color value to HSV. Conversion formula
 * adapted from http://en.wikipedia.org/wiki/HSV_color_space.
 * Assumes r, g, and b are contained in the set [0, 255] and
 * returns h, s, and v in the set [0, 1].
 *
 * @param   Number  r       The red color value
 * @param   Number  g       The green color value
 * @param   Number  b       The blue color value
 * @return  Array           The HSV representation
]]
function EditColorModal:RGBToHSV(r, g, b)
    r, g, b = r / 255, g / 255, b / 255
    local max, min = math.max(r, g, b), math.min(r, g, b)
    local h, s, v
    v = max

    local d = max - min
    if max == 0 then s = 0 else s = d / max end

    if max == min then
        h = 0 -- achromatic
    else
        if max == r then
            h = (g - b) / d
            if g < b then h = h + 6 end
        elseif max == g then h = (b - r) / d + 2
        elseif max == b then h = (r - g) / d + 4
        end
        h = h / 6
    end

    return h, s, v
end

function EditColorModal:Draw()
    if(self.showWarning == true) then
        -- DrawSprites(colorwarningicon.spriteIDs, self.rect.x + 124, self.rect.y + 35, colorwarningicon.width, false, false, DrawMode.Sprite)
        DrawMetaSprite(self.warningMetaSpriteId, self.rect.x + 124, self.rect.y + 35)
    end
end

