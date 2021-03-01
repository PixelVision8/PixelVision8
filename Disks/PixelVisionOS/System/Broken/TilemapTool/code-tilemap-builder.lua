--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

--function TilemapTool:EnableTilemapBuilder()
--
--    if(self.tilemapBuilderPath == nil) then
--        self.tilemapBuilderPath = NewWorkspacePath(self.rootDirectory).AppendDirectory("TilemapBuilder")
--    end
--
--    return PathExists(self.tilemapBuilderPath)
--
--end


function TilemapTool:OnTilemapBuilder()

    -- Make sure we can we can still do the sprite builder
    --if(self:EnableTilemapBuilder() == false) then
    --    return
    --end

    -- Configure the title and message
    local title = "Tilemap Builder"
    
    -- TODO need to mention that the file will always be exported as tilemap.json. If a lua file is created it will need to be manually loaded.
    local message = "It's important to note that performing this optimization may break any places where you have hardcoded references to sprite IDs. You will have the option to apply the optimization after the sprites are processed. \n\nDo you want to perform the following?\n\n"

    local templatePath = NewWorkspacePath(ReadMetadata("RootPath", "/")).AppendDirectory(ReadMetadata("GameName", "untitled")).AppendDirectory("Templates")

    if(PathExists(templatePath)) then

        local files = GetEntities(templatePath)

        self.tilemapBuilderTemplates = {}
        for i = 1, #files do
            if(files[i].GetExtension() ~= ".txt") then

                table.insert(self.tilemapBuilderTemplates, files[i])

            end
        end

        for i = 1, #self.tilemapBuilderTemplates do
            message = message .. "*  Create " .. self.tilemapBuilderTemplates[i].EntityName .. " file\n"
        end

    else

        pixelVisionOS:ShowMessageModal("Error", "There are no templates for the Tilemap Builder, please add the them to the '" .. templatePath.Path .. "' folder.", 162)
    
        return
    end

    -- TODO need to tie these to template files
    -- message = message .. "*  Create tiled-simple.json file\n"
    -- message = message .. "*  Create tiled-advanced.json file\n"
    -- message = message .. "*  Create tilemap.lua file\n"
    -- TODO need to add an option to save PNG

    message = message .. "\n#  Import missing sprites\n"

    -- Create the new warning model
    local warningModal = ExportTilemapModal:Init(title, message .. "\n", 216, true)

    -- Open the modal
    pixelVisionOS:OpenModal( warningModal,
    
        function()
        
            -- Check to see if ok was pressed on the model
            if(warningModal.selectionValue == true) then

                local filePath = self.tilemapBuilderTemplates[warningModal.selectionGroupData.currentSelection]

                -- Save selection for future use
                WriteSaveData( "lastTilemapBuilderTemplateID", tostring(warningModal.selectionGroupData.currentSelection) )

                local templatePath = filePath.ParentPath.AppendFile(filePath.EntityNameWithoutExtension .. "-" .. string.sub(filePath.GetExtension(), 2) .. "-template.txt") 

                if(PathExists(templatePath) == false) then

                    pixelVisionOS:ShowMessageModal("Error", "Could not find the template file that goes with '" .. filePath.Path .. "'. Please makes sure one exists at " .. templatePath.Path)
                    return
                end

                self.spriteFile = ReadTextFile(filePath)
                self.spriteFilePath = NewWorkspacePath(self.rootDirectory .. filePath.EntityName)
                self.spriteFileTemplate = ReadTextFile(templatePath)
                self.spriteFileContents = ""

                -- Kick off the process sprites logic
                self:StartTilemapBuilder()

            end
        
        end
    )

    -- Select the default file template
    editorUI:SelectToggleButton(warningModal.selectionGroupData, 1, false)

    -- TODO this should only be restored if we are editing the same project
    local lastSelection = Clamp(tonumber(ReadSaveData( "lastTilemapBuilderTemplateID", "1" )) or 1, 1, #warningModal.optionGroupData.buttons, ReadSaveData( "lastTilemapBuilderTemplateID", "1" ))

    print("lastSelection", lastSelection)

    warningModal.optionGroupData.buttons[lastSelection].selected = true
end

function TilemapTool:StartTilemapBuilder()
    
    -- TODO need to loop through all of the tiles and build the map
    
    self:ResetProcessTilemaps()


    --self.totalTilemapCount = 0
    --local files = GetEntities(self.tilemapBuilderPath)
    --
    --self.tilemapBuilderFiles = {}
    --
    --for i = 1, #files do
    --    local file = files[i]
    --    if(file.IsFile and file.GetExtension() == ".png") then
    --        table.insert(self.tilemapBuilderFiles, file)
    --    end
    --end
    --
    ---- Since this is referencing a array we need to start at 1
    --self.currentParsedTilemapID = 1
    --
    ---- self.spriteList = {}
    --self.spritesPerLoop = 8
    --self.totalTilemapsToProcess = #self.tilemapBuilderFiles
    --
    ---- The action to preform on each step of the sprite progress loop
    --self.onTilemapProcessAction = function()
    --    self:TilemapBuilderStep()
    --end
    --
    ---- Open the progress model
    --pixelVisionOS:OpenModal(self.progressModal,
    --    function() 
    --        
    --        pixelVisionOS:RemoveUI("ProgressUpdate")
    --        
    --        self:FinalizeTilemapBuilderFile()
    --        
    --    end
    --)
    --
    --pixelVisionOS:RegisterUI({name = "ProgressUpdate"}, "UpdateTilemapProgress", self, true)

end

function TilemapTool:TilemapBuilderStep()

    --local path = self.tilemapBuilderFiles[self.currentParsedTilemapID]
    --
    ---- Only load the image if the path is not nil
    --if(path == nil) then
    --    return
    --end
    --
    ---- TODO pass in system colors?
    --local image = ReadImage(path, pixelVisionOS.maskColor, pixelVisionOS.systemColors)
    --
    --
    --local spriteIDs = ""
    --local index = -1
    --local spriteData = nil
    --local empty = true
    --
    --for i = 1, image.TotalTilemaps do
    --    
    --    spriteData = image.GetTilemapData(i-1, pixelVisionOS.colorsPerTilemap)
    --
    --    index = gameEditor:FindTilemap(spriteData, true)
    --
    --    if(empty == true and index > -1) then
    --        empty = false
    --    end
    --
    --    if(index == -1 and self.addMissingTilemaps) then
    --
    --        local missing = false
    --
    --        -- Check that the sprite isn't empty
    --        for i = 1, #spriteData do
    --            if(spriteData[i] > -1) then
    --                missing = true
    --                break;
    --            end
    --        end
    --
    --        if(missing == true) then
    --            print("Missing sprite", i, "in", path.EntityNameWithoutExtension)
    --            -- TODO need to loop backwards and find the last empty sprite
    --        end
    --
    --    end
    --    
    --    spriteIDs = spriteIDs .. index
    --
    --    if(i < image.TotalTilemaps) then
    --        spriteIDs = spriteIDs .. ","
    --    end
    --
    --end
    --
    --if(empty == false) then
    --
    --    local name = editorUI:ValidateInputFieldText(nil, path.EntityNameWithoutExtension, "variable", "lower")
    --
    --    self.totalTilemapCount = self.totalTilemapCount + 1
    --
    --    self.spriteFileContents = self.spriteFileContents .. string.format(self.spriteFileTemplate, name, spriteIDs, image.Columns)
    --end

end

function TilemapTool:FinalizeTilemapBuilderFile()

    -- Stop the progress from updating
    pixelVisionOS:RemoveUI("ProgressUpdate")

    -- Insert the sprite template values and the final count
    self.spriteFile = string.format(self.spriteFile, self.spriteFileContents, self.totalTilemapCount)

    local title = self.toolTitle
    local message = string.format("The sprite builder has generated %d meta sprites.", self.totalTilemapCount)

    -- Test to see if the file already exists
    if(PathExists(self.spriteFilePath) == true) then
        message = string.format("%s\n\nIt looks like a sprite builder file already exists at: \n\n%s\n\nDo you want to overwrite the following file?\n\n", message, self.spriteFilePath.Path)
    else
        message = string.format("%s\n\nDo you want to save the file?", message)
    end
    
    -- Display warning message
    pixelVisionOS:ShowMessageModal(title, message, 160 + 32, true, function()
        
            if(pixelVisionOS.messageModal.selectionValue == true) then
                self:SaveTilemapFile(self.spriteFilePath, self.spriteFile)
            end
        
        end
        ,"yes"
    )

end

function TilemapTool:SaveTilemapFile(filePath, contents)

    SaveTextToFile(filePath, contents)

end

function TilemapTool:OnPNGExport()


    local tmpFilePath = UniqueFilePath(NewWorkspacePath(self.rootDirectory .. "tilemap-export.png"))

    newFileModal:SetText("Export Tilemap As PNG ", string.split(tmpFilePath.EntityName, ".")[1], "Name file", true)

    pixelVisionOS:OpenModal(newFileModal,
            function()

                if(newFileModal.selectionValue == false) then
                    return
                end

                local filePath = tmpFilePath.ParentPath.AppendFile( newFileModal.inputField.text .. ".png")

                SaveImage(filePath, pixelVisionOS:GenerateImage(self.tilePickerData))

            end
    )

    --

end