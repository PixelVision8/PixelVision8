
function PaintTool:IndexColors()

    if(self.colorsInvalid == false) then
        return
    end

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