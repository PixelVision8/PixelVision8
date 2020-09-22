--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

function DrawTool:OnOptimizeSprites()

    if(self.colorMap == nil or self.analysisInvalid == true) then

        local title = "Warning"
        local message = "In order to remap colors you will need to process the sprites before running the optimization."

        pixelVisionOS:ShowMessageModal(title, message, 162, true, 
            function()
                if(pixelVisionOS.messageModal.selectionValue == true) then

                    self:ProcessSprites()

                end
            end
        )

        return

    end

    -- TODO skip this if the meta data flag has been set
    if(gameEditor:ReadMetadata("ignoreOptimizeWarning", "False") == "True") then
        
        -- Kick off the process sprites logic
        self:StartOptimizingSprites()

        -- Exit before the waring message id displayed
        return
    end

    -- Configure the title and message
    local title = "Analyze Sprites"
    local message = "It's important to note that performing this optimization may break any places where you have hardcoded references to sprite IDs. You will have the option to apply the optimization after the sprites are processed. \n\nDo you want to perform the following?\n\n"

    -- Add the options
    self.optimizationActions = {}

    message = message .. "#  Remove Empty Sprites\n"
    -- set this always available since we always want the option to remove empty
    table.insert(self.optimizationActions, function() self.removeEmptySprites = true end)
    self.removeEmptySprites = false

    message = message .. "#  Remove Duplicate Sprites\n"
    -- set this always available since we always want the option to remove duplicates
    table.insert(self.optimizationActions, function() self.removeDuplicatedSprites = true end)
    self.removeDuplicatedSprites = false

    -- test to see if the colors need to be re-mapped
    if(#self.spritesOutOfBounds > 0) then

        message = message .. "#  Remap colors\n"

        table.insert(self.optimizationActions, function() self.remapColors = true end)

        -- Reset flag value
        self.remapColors = false
    end

    if(self.colorsOutOfBound == true) then

        message = message .. "#  Resize Colors Per Sprite\n"

        table.insert(self.optimizationActions, function() self.expandCPS = true end)

        -- Reset flag value
        self.expandCPS = false

    end

    if(pixelVisionOS.palettesAreEmpty == true) then

        message = message .. "#  Create new palette\n"

        table.insert(self.optimizationActions, function() self.fixFirstPalette = true end)

        -- Reset flag value
        self.fixFirstPalette = false

    end

    -- Create the new warning model
    local warningModal = FixSpriteModal:Init(title, message .. "\n", 216, true)

    -- Open the modal
    pixelVisionOS:OpenModal( warningModal,
    
        function()
        
            -- Check to see if ok was pressed on the model
            if(warningModal.selectionValue == true) then
                    
                -- Get the current selection
                local selections = editorUI:ToggleGroupSelections(warningModal.optionGroupData)

                -- Test to see if there are selections
                if(#selections < 1) then
                    -- If there are no selections, then exit
                    return
                end

                for i = 1, #selections do
                    self.optimizationActions[selections[i]]()
                end

                -- Kick off the process sprites logic
                self:StartOptimizingSprites()

            end
        
        end
    )

    -- This is a hack to force the buttons to be selected after the model is opened
    for i = 1, #warningModal.optionGroupData.buttons do
        
        local button = warningModal.optionGroupData.buttons[i]
        button.selected = true
        
    end

end

function DrawTool:StartOptimizingSprites()

    -- TODO Setup the processor
    -- TODO Process sprites
    -- TODO Callback to as if the sprites should be optimized

    self:ResetProcessSprites()

    self.usedSprites = {}
    self.spritesPerLoop = 16
    self.totalSpritesToProcess = gameEditor:TotalSprites()

    -- The action to preform on each step of the sprite progress loop
    self.onSpriteProcessAction = function()
        self:OptimizeSpriteStep()
    end

    -- Open the progress model
    pixelVisionOS:OpenModal(self.progressModal,
        function() 
            
            pixelVisionOS:RemoveUI("ProgressUpdate")
            
            -- pixelVisionOS:RegisterUI({name = "FinalizeOptimization"}, "FinalizeSpriteOptimization", self, true)

            self:FinalizeSpriteOptimization()
            
        end
    )

    pixelVisionOS:RegisterUI({name = "ProgressUpdate"}, "UpdateSpriteProgress", self, true)
    
end

function DrawTool:OptimizeSpriteStep()

    local pixelData = gameEditor:Sprite(self.currentParsedSpriteID)

    if(self.currentParsedSpriteID < 5 )then
        print("Optimize", self.currentParsedSpriteID, self.removeEmptySprites, self.remapColors)
    end
    -- If remove sprites is true set to true by default
    local empty = self.removeEmptySprites == true
    
    for i = 1, #pixelData do
        
        if(pixelData[i] > -1) then

            -- If the sprite is flagged as empty and has a pixel, change that
            if(empty == true) then
                empty = false
            end

            if(self.remapColors == true) then
                local mapColor = table.indexOf(self.colorMap, pixelData[i])

                if( mapColor > -1) then
                    pixelData[i] = mapColor-1
                end
            end

        end
        
    end

    if(empty == false) then
        
        table.insert(self.usedSprites, pixelData)

    end

end

function DrawTool:FinalizeSpriteOptimization()

    print("Sprites to optimize", #self.usedSprites)

    self.spritesPerLoop = 16
    self.totalSpritesToProcess = gameEditor:TotalSprites()
    local title = "Optimize Sprites"
    local message = "The sprite optimization is ready to make the following changes:\n\n"
    
    if(self.removeEmptySprites == true) then

        local totalSprites = gameEditor:TotalSprites(true)

        local percent = tostring(100 - math.floor((#self.usedSprites / totalSprites) * 100)).."%"

        message = message .. string.format("#  Free %s of sprite memory.\n", percent)
    end

    -- test to see if the colors need to be re-mapped
    if(self.remapColors == true) then
        message = message .. string.format("#  Remap %02d colors.\n", #self.colorMap)
    end

    if(self.expandCPS == true) then
        message = message .. string.format("#  %s Colors per sprite to %02d.\n", pixelVisionOS.colorsPerSprite < #self.colorMap and "Increase" or "decrease", #self.colorMap)
    end

    if(self.fixFirstPalette == true) then
        message = message .. "#  Rebuild the palette 0.\n"
    end
    
    -- Create the new warning model
    local warningModal = FixSpriteModal:Init(title, message, 216, false)

    -- Open the modal
    pixelVisionOS:OpenModal( warningModal,
    
        function()
        
            -- Check to see if ok was pressed on the model
            if(warningModal.selectionValue == true) then
                
                -- print("Actions", "self.removeEmptySprites", self.removeEmptySprites,  "self.remapColors", self.remapColors, "self.expandCPS", self.expandCPS, "self.fixFirstPalette", self.fixFirstPalette)


                -- TODO read the check values to see what actions to perform


                -- Change the CPS
                if(self.expandCPS == true) then

                    pixelVisionOS.colorsPerSprite = gameEditor:ColorsPerSprite(#self.colorMap)

                    pixelVisionOS:ColorPickerVisiblePerPage(self.paletteColorPickerData, pixelVisionOS.colorsPerSprite)
                    pixelVisionOS:RebuildPickerPages(self.paletteColorPickerData)

                end
                
                -- Kick off the process sprites logic
                self:ResetProcessSprites()
    
                self.totalUsedSprites = #self.usedSprites
                self.spritesPerLoop = 16
                self.optimizeSpriteCounter = 0
                self.totalSpritesToProcess = gameEditor:TotalSprites()
                self.destSpriteID = 0
                
                -- Clear the sprite chip
                gameEditor:ClearSprites()
                
                -- The action to preform on each step of the sprite progress loop
                self.onSpriteProcessAction = function()
                    self:ClearEmptySpriteStep()
                end
            
                -- Open the progress model
                pixelVisionOS:OpenModal(self.progressModal,
                    function() 
                        
                        self:OnOptimizeSpritesComplete()
                        
                    end
                )
            
                pixelVisionOS:RegisterUI({name = "ProgressUpdate"}, "UpdateSpriteProgress", self, true)

            end
        
        end
    )

    -- This is a hack to force the buttons to be selected after the model is opened
    for i = 1, #warningModal.optionGroupData.buttons do
        
        local button = warningModal.optionGroupData.buttons[i]
        -- button.selected = true
        editorUI:Enable(button, false)

    end

end


function DrawTool:ClearEmptySpriteStep()
    
    -- TODO need to create a tmpPixelData value instead of creating on each loop
    
    -- Get the pixel data for the current sprite
    local pixelData = self.usedSprites[self.currentParsedSpriteID+1]

    -- Exit if the sprite data is nil or the duplicate flag is true and triggered
    if(pixelData == nil or (self.removeDuplicatedSprites and gameEditor:FindSprite(pixelData, false) > -1)) then
        return
    end

    -- Save the sprite's pixel data
    gameEditor:Sprite(self.destSpriteID, pixelData)

    -- Increment the dest sprite ID
    self.destSpriteID = self.destSpriteID + 1

end

function DrawTool:OnOptimizeSpritesComplete() 

    self:CompleteProcessSprites()

    self.remapColors = false

     -- Then add all the colors to the palette
     if(self.fixFirstPalette == true) then

        for i = 1, #self.colorMap do
            Color(pixelVisionOS.colorOffset + 127 + i, Color(pixelVisionOS.colorOffset + self.colorMap[i]))
        end

        self.fixFirstPalette = false

    end

    for i = 1, #self.colorMap do
        self.colorMap[i] = i-1

        self.expandCPS = false

    end

   

    -- Now that everything is done, we need to tell the optimizer to parse the sprites again
    self.analysisInvalid = true

    -- Reset the colors to make sure all the values are correct for the next pass
    pixelVisionOS:CopyToolColorsToGameMemory()
    pixelVisionOS:ImportColorsFromGame()
    
    self:InvalidateData()

end