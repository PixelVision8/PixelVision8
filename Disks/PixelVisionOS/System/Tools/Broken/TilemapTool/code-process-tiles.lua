--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

function TilemapTool:ResetProcessTiles()
    self.processingTiles = true
    self.currentParsedTileID = 0

    -- Check to see if the modal exits
    if(self.progressModal == nil) then

        -- Create the model
        self.progressModal = ProgressModal:Init("Process Tiles", 168)

    end

end

function TilemapTool:ProcessTiles()

    self:ResetProcessTiles()

    -- Setup the parser values
    self.tilesPerLoop = 128
    
    -- Values to keep track of
    --self.colorsOutOfBound = false
    --self.tilesOutOfBounds = {}
    --self.emptyTiles = {}
    --self.colorMap = {}
    --
    --if(self.tilesPath ~= nil) then
    --    self.tileImage = ReadImage(self.tilesPath, pixelVisionOS.maskColor, pixelVisionOS.systemColors)
    --    -- Set the total tile to parse based on how many there are if they are less than what the tile chip supports
    --    self.totalTilesToProcess = math.min(self.tileImage.TotalTiles, gameEditor:TotalTiles())
    --else
    
    self.totalTilesToProcess = self.mapSize.x * self.mapSize.y
    --end
    
    -- The action to preform on each step of the tile progress loop
    self.onTileProcessAction = function()

        -- TODO need to overwrite this with a custom callback
        --if(self.tileImage == nil) then
        --    -- Get the pixel data for the current tile from the image
        --    self.tmpPixelData = gameEditor:Tile(self.currentParsedTileID)
        --else
        --    -- Get the pixel data for the current tile from the image
        --    self.tmpPixelData = self.tileImage.GetTileData(self.currentParsedTileID)
        --end
        ---- Set the flag to false
        --local flag = false
        --
        ---- local emptyTileFlag = false
        --
        ---- Loop through all of the tile's pixels
        --for j = 1, #self.tmpPixelData do
        --
        --    -- get the current color
        --    local color = self.tmpPixelData[j]
        --                
        --    -- Check to see if the color is not the mask and is in the color map
        --    if(color > -1 and table.indexOf(self.colorMap, color) == -1) then
        --        
        --        -- Add the color to the map
        --        table.insert(self.colorMap, color)
        --
        --    end
        --
        --    -- if(emptyTileFlag == false and color > -1) then
        --    --     emptyTileFlag = true
        --    -- end
        --
        --    -- Check to see if the color is greater than the colors per tile value
        --    if(color > pixelVisionOS.colorsPerTile) then
        --
        --        -- IF the colors out of bound flag is false, set it to true
        --        if(self.colorsOutOfBound == false) then
        --            self.colorsOutOfBound = true
        --        end
        --
        --        -- Trigger the flag to save the tile to the out bounds tile list
        --        flag = true
        --
        --    end
        --end
        --
        ---- If the flag is true, save the tile ID
        --if(flag) then
        --    table.insert(self.tilesOutOfBounds, self.currentParsedTileID)
        --end
        --
        ---- If the flag is true, save the tile ID
        ---- if(emptyTileFlag) then
        ----     table.insert(self.emptyTiles, self.currentParsedTileID)
        ---- end
        --
        ---- TODO keep track of unique colors in order to build the first palette
        ---- TODO keep track of tiles with more colors than the setting allow
        --
        ---- Save the tile to the game chip
        --gameEditor:Tile(self.currentParsedTileID, self.tmpPixelData)

    end

    -- Open the modal to track the progress
    pixelVisionOS:OpenModal(self.progressModal,
        -- When the window is done, show the options   
        function() 
            self:TileProcessingComplete()
        end
    )

    -- Activate the process by registering it with the UI manager
    pixelVisionOS:RegisterUI({name = "ProgressUpdate"}, "UpdateTileProgress", self, true)

end

function TilemapTool:TileProcessingComplete()

    -- Clear the previous image
    --self.tilesPath = nil
    --self.tileImage = nil

    -- Stop the processing flag
    self.processingTiles = false

    -- Remove the callback from the UI update loop
    pixelVisionOS:RemoveUI("ProgressUpdate")

    -- Rebuild the color picker cache
    pixelVisionOS:RebuildTilePickerCache(self.tilePickerData)

    -- Destroy the progress modal
    self.progressModal = nil

    -- Make sure the cursor is set back to 1 in case it's not triggered automatically
    editorUI.mouseCursor:SetCursor(1, false)
    
    --self.analysisInvalid = false

    --self:OnOptimizeTiles()

    --if(self.maskInvalidated == true) then
    --    
    --    Color(pixelVisionOS.emptyColorID, pixelVisionOS.maskColor)
    --    self.maskInvalidated = false
    --    
    --end

end



function TilemapTool:UpdateTileProgress()

    if(self.processingTiles == false) then

        pixelVisionOS:CloseModal()

        -- Exit out of the function
        return
    
    end
    
    for i = 1, self.tilesPerLoop do

        self.onTileProcessAction()

        -- Update the ID of the current tile
        self.currentParsedTileID = self.currentParsedTileID + 1

        -- Make sure we are able to work on the next tile
        if(self.currentParsedTileID > self.totalTilesToProcess) then
            
            -- Stop the process on the next frame
            self.processingTiles = false
            
            -- Exit out of this loop
            break

        end

    end
    
    -- Get the current percentage
    local percent = self.currentParsedTileID/self.totalTilesToProcess

    -- Calculate the padding 
    local pad = #tostring(self.totalTilesToProcess)

    -- Format the message text
    local message = string.format("Processing tile %0" .. pad .. "d of %0" .. pad .. "d.\n\n\nDo not restart or shut down Pixel Vision 8.", self.currentParsedTileID, self.totalTilesToProcess)

    -- Update the modal message
    self.progressModal:UpdateMessage(message, percent)

end

function TilemapTool:CompleteProcessTiles()

    --print("finished recoloring")

    pixelVisionOS:RemoveUI("ProgressUpdate")

    -- TODO need to see if this was invalidated or just re-draw the portion of the visible map
    --pixelVisionOS:RebuildTilePickerCache(self.tilePickerData)

    self.progressModal = nil

    editorUI.mouseCursor:SetCursor(1, false)
    
    -- TODO need to restore the last selected tile if in selection model
    --if(self.mode == TileMode) then
    --    -- Select the start tile
    --    self:ChangeTileID(self.lastTileID)
    --end

end