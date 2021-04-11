
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
    -- for i = 1, 64 do
    --     table.insert(tmpPattern, 0)
    -- end

    -- Set pattern
    -- self.colorCanvas:SetPattern(tmpPattern, 8, 8)

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

    self.pickerTotal = #uniqueColors

    -- Save the transparent color pixel data
    -- local transparentColorData = Sprite(MetaSprite( "emptymaskcolor" ).Sprites[1].Id)

    for i = 1, self.pickerTotal do

        
        local pos = CalculatePosition( i-1, 16 )

        -- if(tmpColor == "#FF00FF")then

        --     -- print("Empty Color")
        
        --     self.colorCanvas:SetPattern(transparentColorData, 8, 8)

        -- else

        for j = 1, 64 do
            tmpPattern[j] = uniqueColors[i]
        end

        self.colorCanvas:SetPattern(tmpPattern, 8, 8)

        -- end

        self.colorCanvas:DrawRectangle(pos.X * 8, pos.Y * 8, 8, 8, true)

        -- -- local colorIndex = i - 1

        -- -- Get the color accounting for the offset
        -- local tmpColor = Color()

        -- -- Calculate the color's position
        -- local pos = CalculatePosition( colorIndex, 16 )

        -- if(tmpColor == "#FF00FF")then

        --     -- print("Empty Color")
        
        --     self.colorCanvas:SetPattern(transparentColorData, 8, 8)

        -- else

        --     for i = 1, #tmpColorPattern do
        --         tmpColorPattern[i] = colorIndex + self.colorOffset
        --     end

        --     self.colorCanvas:SetPattern(tmpColorPattern, 8, 8)

        --     self.pickerTotal = self.pickerTotal + 1

        -- end

        -- self.colorCanvas:DrawRectangle(pos.X * 8, pos.Y * 8, 8, 8, true)

    end

    self:ResetColorValidation()


end

function PaintTool:InvalidateColors()
   
    self.colorsInvalid = true

end

function PaintTool:ResetColorValidation()

    self.colorsInvalid = false

end