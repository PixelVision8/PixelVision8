local editColorModal = nil

EditColorModal = {}
EditColorModal.__index = EditColorModal

function EditColorModal:Init(editorUI, maskColor)

    local _editColorModal = {} -- our new object
    setmetatable(_editColorModal, EditColorModal) -- make Account handle lookup

    _editColorModal.editorUI = editorUI
    _editColorModal.maskColor = maskColor

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

    self.title = "Edit Color"

    self.rect = {
        x = math.floor(((displaySize.x - width) * .5) / 8) * 8,
        y = math.floor(((displaySize.y - height) * .5) / 8) * 8,
        w = width,
        h = height
    }

    self.selectionValue = false

    self.invalidateUI = {}

    -- Draw the black background
    self.canvas:SetStroke({5}, 1, 1)
    self.canvas:SetPattern({0}, 1, 1)
    self.canvas:DrawSquare(0, 0, self.canvas.width - 1, self.canvas.height - 1, true)

    -- Draw the brown background
    self.canvas:SetStroke({12}, 1, 1)
    self.canvas:SetPattern({11}, 1, 1)
    self.canvas:DrawSquare(3, 9, self.canvas.width - 4, self.canvas.height - 4, true)

    local tmpX = (self.canvas.width - (#self.title * 4)) * .5

    self.canvas:DrawText(self.title:upper(), tmpX, 1, "small", 15, - 4)

    -- draw highlight stroke
    self.canvas:SetStroke({15}, 1, 1)
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

    local backBtnData = self.editorUI:CreateButton({x = bX, y = bY}, "modalokbutton", "")

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

    local cancelBtnData = self.editorUI:CreateButton({x = bX, y = bY}, "modalcancelbutton", "")

    cancelBtnData.onAction = function()

        -- Set value to true when cancel is pressed
        self.selectionValue = false

        -- Close the panel
        if(self.onParentClose ~= nil) then
            self.onParentClose()
        end
    end

    table.insert(self.buttons, cancelBtnData)

    -- Settings
    self.colorHexInputData = self.editorUI:CreateInputField({x = self.rect.x + 24, y = self.rect.y + 144, w = 48}, "FF00FF", "Hex value of the selected color.", "hex", "input", 180)

    self.colorHexInputData.forceCase = "upper"

    -- Make sure we only display upper case letters for the hex value
    -- self.colorHexInputData.captureInput = function()
    --   return string.upper(InputString())
    -- end

    table.insert(self.invalidateUI, self.colorHexInputData)

    -- Red input
    self.redInputData = self.editorUI:CreateInputField({x = self.rect.x + 200, y = self.rect.y + 80, w = 24}, "000", "Hex value of the selected color.", "number", "input", 180)
    self.redInputData.min = 0
    self.redInputData.max = 255



    table.insert(self.invalidateUI, self.redInputData)


    -- Green input
    self.greenInputData = self.editorUI:CreateInputField({x = self.rect.x + 200, y = self.rect.y + 112, w = 24}, "000", "Hex value of the selected color.", "number", "input", 180)

    self.greenInputData.min = 0
    self.greenInputData.max = 255



    table.insert(self.invalidateUI, self.greenInputData)

    -- blue input
    self.blueInputData = self.editorUI:CreateInputField({x = self.rect.x + 200, y = self.rect.y + 144, w = 24}, "000", "Hex value of the selected color.", "number", "input", 180)

    self.blueInputData.min = 0
    self.blueInputData.max = 255


    table.insert(self.invalidateUI, self.blueInputData)

    -- Red slider
    self.redSlider = self.editorUI:CreateSlider(
        { x = self.rect.x + 108, y = self.rect.y + 75, w = 80, h = 16},
        "hsliderhandle",
        "Adjust the red value.",
        true
    )

    -- Green slider
    self.greenSlider = self.editorUI:CreateSlider(
        { x = self.rect.x + 108, y = self.rect.y + 107, w = 80, h = 16},
        "hsliderhandle",
        "Adjust the green value.",
        true
    )

    -- Blue slider
    self.blueSlider = self.editorUI:CreateSlider(
        { x = self.rect.x + 108, y = self.rect.y + 139, w = 80, h = 16},
        "hsliderhandle",
        "Adjust the Red value.",
        true
    )



    -- Check boxes
    self.colorModeRadioGroupData = self.editorUI:CreateToggleGroup(true)
    self.colorModeRadioGroupData.onAction = function(value)
        -- self:OnChangeTrackInstrument(data, value)
        self:ChangeColorMode(value)

        self.redSlider.toolTip = value == 1 and "Change the red value from 0 to 255" or "Change the hue percentage from 0 to 100"
        self.greenSlider.toolTip = value == 1 and "Change the green value from 0 to 255" or "Change the saturation percentage from 0 to 100"
        self.blueSlider.toolTip = value == 1 and "Change the blue value from 0 to 255" or "Change the value percentage from 0 to 100"

    end

    local rgbButton = self.editorUI:ToggleGroupButton(self.colorModeRadioGroupData, {x = self.rect.x + 184 + 8, y = self.rect.y + 40, w = 8, h = 8}, "radiobutton", "Change the current track's instrument to ", true)

    table.insert(self.invalidateUI, rgbButton)

    local hsvButton = self.editorUI:ToggleGroupButton(self.colorModeRadioGroupData, {x = self.rect.x + 184 + 8, y = self.rect.y + 48, w = 8, h = 8}, "radiobutton", "Change the current track's instrument to ", true)

    table.insert(self.invalidateUI, hsvButton)

    -- self.editorUI:Enable(hsvButton, false)

    -- Create two rects for the picker
    self.colorSpaceRect = { x = self.rect.x + 16, y = self.rect.y + 32, w = 80, h = 80}
    self.greySpaceRect = { x = self.rect.x + 16, y = self.rect.y + 96 + 16, w = 80, h = 8}


    -- Wire up UI

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
    --
    -- for i = 1, #self.colorCache do
    --   print(i, self.colorCache[i])
    -- end

    self.canvas:DrawPixels(self.rect.x, self.rect.y, DrawMode.TilemapCache)

    DrawSprites(coloreditorpanel.spriteIDs, self.rect.x + 8, self.rect.y + 16, coloreditorpanel.width, false, false, DrawMode.TilemapCache)

    -- Invalidate all of the UI buttons so they display correctly when re-opening the modal
    for i = 1, #self.buttons do
        self.editorUI:Invalidate(self.buttons[i])
    end

    local total = #self.invalidateUI
    for i = 1, total do
        self.editorUI:Invalidate(self.invalidateUI[i])
    end

    -- Save last color mode selection
    local previousSelection = self.colorModeRadioGroupData.currentSelection

    -- Clear previous selection
    self.editorUI:ClearGroupSelections(self.colorModeRadioGroupData)

    -- Force the color space to redraw
    self.editorUI:SelectToggleButton(self.colorModeRadioGroupData, previousSelection > 0 and previousSelection or 1)

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

        DrawSprites(colorlabelred.spriteIDs, self.rect.x + 112, self.rect.y + 64, colorlabelred.width, false, false, DrawMode.TilemapCache)
        DrawSprites(colorlabelgreen.spriteIDs, self.rect.x + 112, self.rect.y + 96, colorlabelgreen.width, false, false, DrawMode.TilemapCache)
        DrawSprites(colorlabelblue.spriteIDs, self.rect.x + 112, self.rect.y + 128, colorlabelblue.width, false, false, DrawMode.TilemapCache)

        DrawSprites(rgbcolorspace.spriteIDs, self.rect.x + 16, self.rect.y + 32, hsvcolorspace.width, false, false, DrawMode.TilemapCache)

    else
        self.redInputData.max = 100
        self.greenInputData.max = 100
        self.blueInputData.max = 100

        DrawSprites(colorlabelhue.spriteIDs, self.rect.x + 112, self.rect.y + 64, colorlabelred.width, false, false, DrawMode.TilemapCache)
        DrawSprites(colorlabelsaturation.spriteIDs, self.rect.x + 112, self.rect.y + 96, colorlabelsaturation.width, false, false, DrawMode.TilemapCache)
        DrawSprites(colorlabelvalue.spriteIDs, self.rect.x + 112, self.rect.y + 128, colorlabelvalue.width, false, false, DrawMode.TilemapCache)

        DrawSprites(hsvcolorspace.spriteIDs, self.rect.x + 16, self.rect.y + 32, hsvcolorspace.width, false, false, DrawMode.TilemapCache)

    end

    -- print("Reset Hex", self.colorHexInputData.text)

    self:UpdateHexValue("#"..self.colorHexInputData.text)
    -- Update the color from the hex value
    -- editorUI:ChangeInputField(self.colorHexInputData, self.colorHexInputData.text, true)

    -- TODO convert from HEX to Color

    -- TODO switch text

end

function EditColorModal:Close()

    -- print("close", self.tmpColor, self.maskColor)
    Color(self.tmpColor, self.maskColor)

end

function EditColorModal:Update(timeDelta)

    -- print("In Focus", editorUI.inFocusUI.name)

    local mouseDown = editorUI.collisionManager.mouseDown

    -- Test to see if the mouse is inside of one of the color space rects
    if(editorUI.collisionManager:MouseInRect(self.colorSpaceRect)) then

        editorUI:SetFocus(self.colorSpaceRect, 8)

        if(mouseDown) then

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

            -- TODO need to test for palette mode
            -- self.showWarning = table.indexOf(self.colorCache, newHex) > - 1

            -- print("Color Picker", editorUI.collisionManager.mousePos.x, editorUI.collisionManager.mousePos.y, h, s)

        end


    elseif(editorUI.collisionManager:MouseInRect(self.greySpaceRect)) then
        --
        editorUI:SetFocus(self.greySpaceRect, 8)

        if(mouseDown) then

            local h = 0

            -- TODO need to flip this value horizontally
            local s = 0

            -- This is always 100% when in the picker
            local v = (editorUI.collisionManager.mousePos.x - self.colorSpaceRect.x) / self.colorSpaceRect.w

            local r, g, b = self:HSVToRGB(h, s, v)

            local hex = self:RGBToHex(r, g, b)

            self:UpdateHexValue(hex)

            -- print("Grey Picker", editorUI.collisionManager.mousePos.x, editorUI.collisionManager.mousePos.y)

        end

    elseif(self.colorSpaceRect.inFocus == true) then

        editorUI:ClearFocus(self.colorSpaceRect)

    elseif(self.greySpaceRect.inFocus == true) then
        editorUI:ClearFocus(self.greySpaceRect)
        -- editorUI.cursorID = 1
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
        self.editorUI:UpdateButton(self.buttons[i])
    end
    --
    self.editorUI:UpdateInputField(self.colorHexInputData)
    self.editorUI:UpdateInputField(self.redInputData)
    self.editorUI:UpdateInputField(self.greenInputData)
    self.editorUI:UpdateInputField(self.blueInputData)

    self.editorUI:UpdateSlider(self.redSlider)
    self.editorUI:UpdateSlider(self.greenSlider)
    self.editorUI:UpdateSlider(self.blueSlider)

    self.editorUI:UpdateToggleGroup(self.colorModeRadioGroupData)

end

function EditColorModal:UpdateHexValue(value)

    local r, g, b = self:HexToRGB(value)

    if(self.rgbMode == false) then

        r, g, b = self:RGBToHSV(r, g, b)

        r = math.ceil(r * 100)
        g = math.ceil(g * 100)
        b = math.ceil(b * 100)

    end

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
--
-- function EditColorModal:HSV(h, s, v)
--   if s <= 0 then return v, v, v end
--   h, s, v = h / 256 * 6, s / 255, v / 255
--   local c = v * s
--   local x = (1 - math.abs((h%2) - 1)) * c
--   local m, r, g, b = (v - c), 0, 0, 0
--   if h < 1 then r, g, b = c, x, 0
--   elseif h < 2 then r, g, b = x, c, 0
--   elseif h < 3 then r, g, b = 0, c, x
--   elseif h < 4 then r, g, b = 0, x, c
--   elseif h < 5 then r, g, b = x, 0, c
--   else r, g, b = c, 0, x
--     end return (r + m) * 255, (g + m) * 255, (b + m) * 255
--   end
-- end


function EditColorModal:Draw()
    if(self.showWarning == true) then
        DrawSprites(colorwarningicon.spriteIDs, self.rect.x + 124, self.rect.y + 35, colorwarningicon.width, false, false, DrawMode.Sprite)
    end
end

function OnConfig()

    local colorID = systemColorPickerData.currentSelection + systemColorPickerData.altColorOffset

    if(editColorModal == nil) then
        editColorModal = EditColorModal:Init(editorUI, pixelVisionOS.maskColor)
    end

    -- TODO need to get the currently selected color
    editColorModal:SetColor(colorID)

    pixelVisionOS:OpenModal(editColorModal,
        function()

            if(editColorModal.selectionValue == true) then

                UpdateHexColor(editColorModal.colorHexInputData.text)
                -- Color(colorID, )

                -- Color(255, pixelVisionOS.maskColor)
                return
            end

        end
    )

end

function TogglePaletteMode(value, callback)

    local data = paletteColorPickerData

    if(value == true) then

        -- If we are not using palettes, we need to warn the user before activating it

        pixelVisionOS:ShowMessageModal("Activate Palette Mode", "Do you want to activate palette mode? This will split color memory in half and allocate 128 colors to the system and 128 to palettes. The sprites will also be reindexed to the first palette. Saving will rewrite the 'sprite.png' file. This can not be undone.", 160, true,
            function()
                if(pixelVisionOS.messageModal.selectionValue == true) then

                    -- Clear any colors in the clipboard
                    copyValue = nil

                    local oldCPS = gameEditor:ColorsPerSprite()

                    -- Cop the colors to memory
                    pixelVisionOS:CopyToolColorsToGameMemory()

                    -- Update the palette mode in the meta data
                    gameEditor:WriteMetadata("paletteMode", "true")

                    -- Reindex the sprites so they will work in palette mode
                    gameEditor:ReindexSprites()

                    local defaultColor = gameEditor:Color(0)

                    -- Set default color for the rest of the palettes
                    for i = 1, 7 do
                        local offset = PaletteOffset( i )
                        for j = 0, 15 do
                            gameEditor:Color( offset + j, defaultColor )
                        end
                    end

                    -- Import the colors again
                    pixelVisionOS:ImportColorsFromGame()

                    -- Update the system color picker to match the new total colors
                    pixelVisionOS:ChangeColorPickerTotal(systemColorPickerData, pixelVisionOS.totalSystemColors)

                    -- Make GPU chip custom if CPS changed
                    if(gameEditor:ColorsPerSprite() ~= oldCPS) then
                        gameEditor:WriteMetadata("gpuChip", "Custom")
                    end

                    spritesInvalid = true

                    -- TODO this needs to be routed to the Sprite Picker and clear the cache
                    pixelVisionOS:ChangeItemPickerColorOffset(spritePickerData, pixelVisionOS.colorOffset + 128)

                    pixelVisionOS:RebuildSpritePickerCache(spritePickerData)

                    -- Redraw the sprite picker to they will display the correct colors
                    -- pixelVisionOS:InvalidateItemPickerDisplay(spritePickerData)

                    -- Clear focus
                    ForcePickerFocus()

                    -- Redraw the UI
                    pixelVisionOS:RebuildColorPickerCache(systemColorPickerData)
                    -- pixelVisionOS:SelectColorPage(systemColorPickerData, 1)

                    -- Update the palette picker
                    paletteColorPickerData.total = 128
                    paletteColorPickerData.altColorOffset = pixelVisionOS.colorOffset + 128

                    -- Force the palette picker to only display the total colors per sprite
                    paletteColorPickerData.visiblePerPage = gameEditor:ColorsPerSprite()

                    pixelVisionOS:RebuildColorPickerCache(paletteColorPickerData)

                    -- Set use palettes to true
                    usePalettes = true



                    -- Invalidate the data so the tool can save
                    InvalidateData()

                    -- Trigger any callback after this is done
                    if(callback ~= nil) then
                        callback()
                    end

                end

            end
        )


    else

        pixelVisionOS:ShowMessageModal("Disable Palette Mode", "Disabeling the palette mode will return the game to 'Direct Color Mode'. Sprites will only display if they can match their colors to 'color.png' file. This process will also remove the palette colors and restore the system colors to support 256.", 160, true,
            function()
                if(pixelVisionOS.messageModal.selectionValue == true) then

                    usePalettes = false

                    -- Copy the colors to memory
                    pixelVisionOS:CopyToolColorsToGameMemory()

                    -- Update the palette mode in the meta data
                    gameEditor:WriteMetadata("paletteMode", "false")

                    -- Get the game colors
                    local oldColors = gameEditor:Colors()

                    local tmpIndex = 0

                    -- Copy the first palette to the top of the color table
                    for i = 1, 16 do
                        gameEditor:Color(tmpIndex, oldColors[i + 128])
                        tmpIndex = tmpIndex + 1
                    end

                    -- Copy the old system colors over after the first palette
                    for i = 1, 128 do
                        gameEditor:Color(16 + (i - 1), oldColors[i])
                    end



                    -- Import the colors again
                    pixelVisionOS:ImportColorsFromGame()

                    -- Redraw the sprite picker
                    spritePickerData.colorOffset = pixelVisionOS.colorOffset
                    pixelVisionOS:InvalidateItemPickerDisplay(spritePickerData)


                    -- systemColorPickerData.total = pixelVisionOS.systemColorsPerPage

                    pixelVisionOS:RebuildColorPickerCache(systemColorPickerData)

                    pixelVisionOS:RemoveColorPicker(paletteColorPickerData)

                    systemColorPickerData.lastSelectedPage = 1

                    -- Clear focus
                    ForcePickerFocus()


                    InvalidateData()
                    -- Update the game editor palette modes
                    -- gameEditor:PaletteMode(usePalettes)

                    -- TODO remove color pages

                    if(callback ~= nil) then
                        callback()
                    end

                end

            end
        )

    end


end
