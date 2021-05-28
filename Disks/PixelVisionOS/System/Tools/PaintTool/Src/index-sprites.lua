function PaintTool:IndexSprites()

    -- Check to see if the sprites are invalidated
    if(self.spritesInvalidated == false or self.indexingSprites == true) then
        -- Exit out of the function since there is no reason to re-index the sprites
        return
    end

    self.indexingSprites = true

    self:ClearCurrentCanvas()

    -- Create a list of unique sprites and references to where they are in the image
    self.uniqueSprites = {}

    self.indexBatchSize = 64

    self.spriteCounter = 0
    
    self.spriteColumns = math.ceil(self.imageLayerCanvas.Width / 8)
    self.spriteRows = math.ceil(self.imageLayerCanvas.Height / 8)

    self.currentState.pickerTotal = self.spriteColumns * self.spriteRows

    self.currentPage = 0

    pixelVisionOS:RegisterUI({name = "OnUpdateSpriteIndex"}, "UpdateSpriteIndex", self)

end

function PaintTool:UpdateSpriteIndex()

    self.colorCanvas:SetStroke(-1, 0)
    
    for i = 1, self.indexBatchSize do

        local tilePos = CalculatePosition( self.spriteCounter, self.spriteColumns )

        local pixelData = self.imageLayerCanvas:GetPixels(tilePos.X * 8, tilePos.Y * 8, 8 , 8)

        local spriteUID = table.concat(pixelData,",")

        local index = -1

        for i = 1, #self.uniqueSprites do
            
            if(self.uniqueSprites[i].uid == spriteUID) then
                index = i
                break
            end

        end

        
        -- Check to see if the sprite is unique
        if(index == -1) then

            -- Find the next picker position
            local pickerPos = CalculatePosition( #self.uniqueSprites, 16 )

            -- Save the unique sprite
            table.insert(self.uniqueSprites, {uid = spriteUID, pixelData = pixelData})

            local spriteData = {}

            -- Shift the pixels into the correct color space
            for i = 1, #pixelData do
                spriteData[i] = pixelData[i] == -1 and -1 or pixelData[i] + self.colorOffset
            end

            -- Set the fill to the mask color
            self.spriteCanvas:SetPattern({254}, 1, 1)

            -- Draw mask color behind sprite
            self.spriteCanvas:DrawRectangle(pickerPos.X * 8, pickerPos.Y * 8, 8, 8, true)

            -- Set the sprite as the pattern
            self.spriteCanvas:SetPattern(spriteData, 8, 8)

            -- Draw the sprite to the picker
            self.spriteCanvas:DrawRectangle(pickerPos.X * 8, pickerPos.Y * 8, 8, 8, true)
       
        end

        -- Increase the number of sprites
        self.spriteCounter = self.spriteCounter + 1

        -- Recalculate pages base on the total number of sprites
        self.totalPages = (math.ceil(#self.uniqueSprites / 16 / 2)) - 1 -- Account for zero based pages by subtracting 1

        self:SetPickerLabel("Sprites")

        -- Reset current page
        self:GoToPickerPage(self.currentPage)

        -- Test to see if we read all the sprites in this batch
        if(self.spriteCounter >= self.currentState.pickerTotal) then

            -- Stop the update loop
            pixelVisionOS:RemoveUI("OnUpdateSpriteIndex")
    
            -- Display a done message
            pixelVisionOS:DisplayMessage("Indexing Done")
    
            -- Update the final sprite total
            self.currentState.pickerTotal = #self.uniqueSprites

            -- Reset the validation
            self:ResetSpriteInvalidation()

            self:RebuildBrushPreview()
            
            self.indexingSprites = false

            -- Exit the loop
            return
    
        end

        

    end

    -- Display the current sprite indexing progress
    pixelVisionOS:DisplayMessage("Indexing Sprites " .. (self.spriteCounter) .. "/" .. self.currentState.pickerTotal)

end

function PaintTool:InvalidateSprites()
    self.spritesInvalidated = true
end

function PaintTool:ResetSpriteInvalidation()
    self.spritesInvalidated = false
end
