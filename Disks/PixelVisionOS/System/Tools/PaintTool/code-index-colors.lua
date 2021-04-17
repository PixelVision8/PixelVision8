
function PaintTool:IndexColors()

    if(self.colorsInvalid == false) then
        return
    end

    self.colorBrightness = {}

    -- Configure the canvas
    self:ClearCurrentCanvas()

    -- Remove the stroke
    self.colorCanvas:SetStroke(-1, 0)

    -- Create a pattern
    local tmpPattern = {}
    
    -- Find unique colors
    local uniqueColors = {}

    -- Loop through all of the colors and pull out the unique colors
    for i = self.colorOffset, 255 do
        
        -- Get the color
        local tmpColor = Color(i)

        -- Make sure it is not transparent
        if(tmpColor ~= MaskColor()) then
            
            -- Add to the unique color table
            table.insert(uniqueColors, i)


            local h, s, v = self:RGBToHSV(self:HexToRGB(tmpColor))
            -- local r,g,b = )
            print("Color", tmpColor, v)
            
        end

    end

    self.currentState.pickerTotal = #uniqueColors

    for i = 1, self.currentState.pickerTotal do

        
        local pos = CalculatePosition( i-1, 16 )

        for j = 1, 64 do
            tmpPattern[j] = uniqueColors[i]
        end

        self.colorCanvas:SetPattern(tmpPattern, 8, 8)

        -- end

        self.colorCanvas:DrawRectangle(pos.X * 8, pos.Y * 8, 8, 8, true)

    end

    self:SetPickerLabel("Colors")

    self:ResetColorValidation()

end

function PaintTool:InvalidateColors()
   
    self.colorsInvalid = true

end

function PaintTool:ResetColorValidation()

    self.colorsInvalid = false

end

-- TODO need to create a look up table for lum

function PaintTool:HexToRGB(hex)
    hex = hex:gsub("#", "")
    return tonumber("0x"..hex:sub(1, 2), 16), tonumber("0x"..hex:sub(3, 4), 16), tonumber("0x"..hex:sub(5, 6), 16)
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
function PaintTool:RGBToHSV(r, g, b)
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