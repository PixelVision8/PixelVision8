--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

function DrawTool:ResetProcessSprites()
    self.processingSprites = true
    self.currentParsedSpriteID = 0

    -- Check to see if the modal exits
    if(self.progressModal == nil) then

        -- Create the model
        self.progressModal = ProgressModal:Init("Process Sprites", 168)

    end

end

function DrawTool:ProcessSprites()

    self:ResetProcessSprites()

    -- Setup the parser values
    self.spritesPerLoop = 16
    
    -- Values to keep track of
    self.colorsOutOfBound = false
    self.spritesOutOfBounds = {}
    self.emptySprites = {}
    self.colorMap = {}
    
    if(self.spritesPath ~= nil) then
        self.spriteImage = ReadImage(self.spritesPath, pixelVisionOS.maskColor, pixelVisionOS.systemColors)
        -- Set the total sprite to parse based on how many there are if they are less than what the sprite chip supports
        self.totalSpritesToProcess = math.min(self.spriteImage.TotalSprites, gameEditor:TotalSprites())
    else
        self.totalSpritesToProcess = gameEditor:TotalSprites()
    end
    
    -- The action to preform on each step of the sprite progress loop
    self.onSpriteProcessAction = function()

        if(self.spriteImage == nil) then
            -- Get the pixel data for the current sprite from the image
            self.tmpPixelData = gameEditor:Sprite(self.currentParsedSpriteID)
        else
            -- Get the pixel data for the current sprite from the image
            self.tmpPixelData = self.spriteImage.GetSpriteData(self.currentParsedSpriteID)
        end
        -- Set the flag to false
        local flag = false

        -- local emptySpriteFlag = false

        -- Loop through all of the sprite's pixels
        for j = 1, #self.tmpPixelData do

            -- get the current color
            local color = self.tmpPixelData[j]
                        
            -- Check to see if the color is not the mask and is in the color map
            if(color > -1 and table.indexOf(self.colorMap, color) == -1) then
                
                -- Add the color to the map
                table.insert(self.colorMap, color)

            end

            -- if(emptySpriteFlag == false and color > -1) then
            --     emptySpriteFlag = true
            -- end

            -- Check to see if the color is greater than the colors per sprite value
            if(color > pixelVisionOS.colorsPerSprite) then

                -- IF the colors out of bound flag is false, set it to true
                if(self.colorsOutOfBound == false) then
                    self.colorsOutOfBound = true
                end

                -- Trigger the flag to save the sprite to the out bounds sprite list
                flag = true

            end
        end

        -- If the flag is true, save the sprite ID
        if(flag) then
            table.insert(self.spritesOutOfBounds, self.currentParsedSpriteID)
        end

        -- If the flag is true, save the sprite ID
        -- if(emptySpriteFlag) then
        --     table.insert(self.emptySprites, self.currentParsedSpriteID)
        -- end

        -- TODO keep track of unique colors in order to build the first palette
        -- TODO keep track of sprites with more colors than the setting allow

        -- Save the sprite to the game chip
        gameEditor:Sprite(self.currentParsedSpriteID, self.tmpPixelData)

    end

    -- Open the modal to track the progress
    pixelVisionOS:OpenModal(self.progressModal,
        -- When the window is done, show the options   
        function() 
            self:SpriteProcessingComplete()
        end
    )

    -- Activate the process by registering it with the UI manager
    pixelVisionOS:RegisterUI({name = "ProgressUpdate"}, "UpdateSpriteProgress", self, true)

end

function DrawTool:SpriteProcessingComplete()

    -- Clear the previous image
    self.spritesPath = nil
    self.spriteImage = nil

    -- Stop the processing flag
    self.processingSprites = false

    -- Remove the callback from the UI update loop
    pixelVisionOS:RemoveUI("ProgressUpdate")

    -- Rebuild the color picker cache
    pixelVisionOS:RebuildSpritePickerCache(self.spritePickerData)

    -- Destroy the progress modal
    self.progressModal = nil

    -- Make sure the cursor is set back to 1 in case it's not triggered automatically
    editorUI.mouseCursor:SetCursor(1, false)
    
    self.analysisInvalid = false

    self:OnOptimizeSprites()

    if(self.maskInvalidated == true) then
        
        Color(pixelVisionOS.emptyColorID, pixelVisionOS.maskColor)
        self.maskInvalidated = false
        
    end

end

function DrawTool:ReIndexSprites(prevValue, newValue)

    self:ResetProcessSprites()

    self.spritesPerLoop = 16
    self.totalSpritesToProcess = gameEditor:TotalSprites()
    self.prevColorIndex = prevValue
    self.newColorIndex = newValue
    
    -- The action to preform on each step of the sprite progress loop
    self.onSpriteProcessAction = function()
        
        -- Get the sprite data
        self.tmpPixelData = gameEditor:Sprite(self.currentParsedSpriteID)

        -- Loop through each of the sprites pixel
        for j = 1, #self.tmpPixelData do

            -- Get the current color
            local color = self.tmpPixelData[j]
            
            -- Test if the color is the old color
            if(color == self.prevColorIndex) then

                -- update the sprite's pixel to the new value
                self.tmpPixelData[j] = self.newColorIndex

            end

        end
        
        -- Save the modified sprite data
        gameEditor:Sprite(self.currentParsedSpriteID, self.tmpPixelData)
        
        

        -- Return true to let the progress loop know this was executed fully
        return true

    end

    pixelVisionOS:OpenModal(self.progressModal,
        function() 
            
            self:CompleteProcessSprites()
            
        end
    )

    pixelVisionOS:RegisterUI({name = "ProgressUpdate"}, "UpdateSpriteProgress", self, true)
    

end

function DrawTool:UpdateSpriteProgress()

    if(self.processingSprites == false) then

        pixelVisionOS:CloseModal()

        -- Exit out of the function
        return
    
    end
    
    for i = 1, self.spritesPerLoop do

        self.onSpriteProcessAction()

        -- Update the ID of the current sprite
        self.currentParsedSpriteID = self.currentParsedSpriteID + 1

        -- Make sure we are able to work on the next sprite
        if(self.currentParsedSpriteID > self.totalSpritesToProcess) then
            
            -- Stop the process on the next frame
            self.processingSprites = false
            
            -- Exit out of this loop
            break

        end

    end
    
    -- Get the current percentage
    local percent = self.currentParsedSpriteID/self.totalSpritesToProcess

    -- Calculate the padding 
    local pad = #tostring(self.totalSpritesToProcess)

    -- Format the message text
    local message = string.format("Processing sprite %0" .. pad .. "d of %0" .. pad .. "d.\n\n\nDo not restart or shut down Pixel Vision 8.", self.currentParsedSpriteID, self.totalSpritesToProcess)

    -- Update the modal message
    self.progressModal:UpdateMessage(message, percent)

end

function DrawTool:CompleteProcessSprites()

    print("finished recoloring")

    pixelVisionOS:RemoveUI("ProgressUpdate")

    pixelVisionOS:RebuildSpritePickerCache(self.spritePickerData)

    self.progressModal = nil

    editorUI.mouseCursor:SetCursor(1, false)
    
    if(self.mode == SpriteMode) then
        -- Select the start sprite
        self:ChangeSpriteID(self.lastSpriteID)
    end

end