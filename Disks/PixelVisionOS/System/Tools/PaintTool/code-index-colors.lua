
function PaintTool:IndexColors()

    -- Only run if the colors have been invalidated
    if(self.colorsInvalid == false) then
        return
    end

    -- Configure the canvas
    self:ClearCurrentCanvas()

    -- Remove the stroke
    self.colorCanvas:SetStroke(-1, 0)

    -- Create a pattern
    local tmpPattern = {}
    
    -- Create a table to store the unique colors
    local uniqueColors = {}

    -- Loop through all of the colors and pull out the unique colors
    for i = self.colorOffset, 255 do
        
        -- Get the color stating at the colorOffset
        local tmpColor = Color(i)

        -- Make sure it is not transparent
        if(tmpColor ~= MaskColor()) then
            
            -- Add to the unique color table
            table.insert(uniqueColors, i)

        end

    end

    -- Set the total number of colors in the picker
    self.currentState.pickerTotal = #uniqueColors

    -- Loop through each of the colors
    for i = 1, self.currentState.pickerTotal do

        -- Calculate the color's position
        local pos = CalculatePosition( i-1, 16 )

        -- Loop through all of the pixels in the tmpPattern to change the value to match the color
        for j = 1, 64 do
            tmpPattern[j] = uniqueColors[i]
        end

        -- Set the pattern on the canvas
        self.colorCanvas:SetPattern(tmpPattern, 8, 8)

        -- Draw a rectangle using the current color's palette
        self.colorCanvas:DrawRectangle(pos.X * 8, pos.Y * 8, 8, 8, true)

    end

    -- TODO hardcoded for testing - should be read from game file?
    self.backgroundColorId = 1

    -- Change the picker label
    self:SetPickerLabel("Colors")

    -- Reset the color validation
    self:ResetColorValidation()

end

function PaintTool:InvalidateColors()
   
    -- Set the color invalid flag to true
    self.colorsInvalid = true

end

function PaintTool:ResetColorValidation()

    -- Set the color invalid flag to false
    self.colorsInvalid = false

end