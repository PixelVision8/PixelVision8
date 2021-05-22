function PaintTool:IndexFlags()

    if(self.flagsInvalid == false) then
        return
    end

    self:ClearCurrentCanvas()

    self.currentState.pickerTotal = 16

    local flagPath = NewWorkspacePath(self.rootDirectory).AppendFile("flags.png")

    if(PathExists(flagPath)) then

        local colors = {}

        for i = 1, 16 do
            table.insert(colors, Color(i-1))
        end
        
        print(dump(colors))
        local flagImage = ReadImage(flagPath, "#FF00FF", colors)

        local columns = math.ceil(flagImage.Width / 8)
        local rows = math.ceil(flagImage.Height / 8)

        print("size", columns, rows)

        self.currentState.pickerTotal = columns * rows

        local totalFlags = math.max(32, math.min(self.currentState.pickerTotal, 255))

        print("total flags", totalFlags)

        local pixelData = nil

        for i = 1, totalFlags do
            
            if(i > self.currentState.pickerTotal) then
                pixelData = self.emptyPatternPixelData
            else
                local pos = CalculatePosition( i-1, columns )

                pixelData = flagImage:GetPixels(pos.X * 8, pos.Y * 8, 8, 8)
            end
            
            -- TODO should we ignore transparent flags?
            pos = CalculatePosition( i-1, 16 )

            self.flagCanvas:SetPixels(pos.X * 8, pos.Y * 8, 8, 8, pixelData)

        end

    else

        self.currentCanvas:SetStroke(-1, 0)
        self.currentCanvas:SetPattern(self.emptyPatternPixelData, 8, 8)

        for i = 1, 32  do

            local index = i - 1
            local pos = CalculatePosition( index, 16 )

            if(index < self.currentState.pickerTotal) then
                
                self.currentCanvas:DrawMetaSprite(FindMetaSpriteId("flag".. i .. "small"), pos.X * 8, pos.Y * 8)

            else
                self.currentCanvas:DrawRectangle(pos.X * 8, pos.Y * 8, 8, 8, true)
            end
            
        end
    end

    self:SetPickerLabel("Flags")

    self:ResetFlagValidation()
    
end

function PaintTool:InvalidateFlags()
   
    self.flagsInvalid = true

end

function PaintTool:ResetFlagValidation()

    self.flagsInvalid = false

end