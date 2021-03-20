--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

function DrawTool:EnableSpriteBuilder()

    if(self.spriteBuilderPath == nil) then
        self.spriteBuilderPath = NewWorkspacePath(self.rootDirectory).AppendDirectory("SpriteBuilder")
    end

    return PathExists(self.spriteBuilderPath)

end


function DrawTool:OnSpriteBuilder()

    -- Make sure we can we can still do the sprite builder
    if(self:EnableSpriteBuilder() == false) then
        return
    end

    -- Configure the title and message
    local title = "Sprite Builder"
    local message = "It's important to note that performing this optimization may break any places where you have hardcoded references to sprite IDs. You will have the option to apply the optimization after the sprites are processed. \n\nDo you want to perform the following?\n\n"

    local templatePath = NewWorkspacePath(ReadMetadata("RootPath", "/")).AppendDirectory(ReadMetadata("GameName", "untitled")).AppendDirectory("Templates")

    if(PathExists(templatePath)) then

        local files = GetEntities(templatePath)

        self.spriteBuilderTemplates = {}
        for i = 1, #files do
            if(files[i].GetExtension() ~= ".txt") then

                table.insert(self.spriteBuilderTemplates, files[i])

            end
        end

        for i = 1, #self.spriteBuilderTemplates do
            message = message .. "*  Create " .. self.spriteBuilderTemplates[i].EntityName .. " file\n"
        end

    else

        pixelVisionOS:ShowMessageModal("Error", "There are no templates for the Sprite Builder, please add the them to the '" .. templatePath.Path .. "' folder.", 162)
    
        return
    end

    -- TODO need to tie these to template files
    -- message = message .. "*  Create sb-sprites.lua file\n"
    -- message = message .. "*  Create meta-sprites.lua file\n"
    -- message = message .. "*  Create meta-sprites.json file\n"

    message = message .. "\n#  Import missing sprites\n"

    -- Create the new warning model
    local warningModal = FixSpriteModal:Init(title, message .. "\n", 216, true)

    -- Open the modal
    pixelVisionOS:OpenModal( warningModal,
    
        function()
        
            -- Check to see if ok was pressed on the model
            if(warningModal.selectionValue == true) then

                local filePath = self.spriteBuilderTemplates[warningModal.selectionGroupData.currentSelection]

                -- Save selection for future use
                WriteSaveData( "lastSpriteBuilderTemplateID", tostring(warningModal.selectionGroupData.currentSelection) )

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
                self:StartSpriteBuilder()

            end
        
        end
    )

    -- Select the default file template
    editorUI:SelectToggleButton(warningModal.selectionGroupData, 1, false)

    -- TODO this should only be restored if we are editing the same project
    local lastSelection = Clamp(tonumber(ReadSaveData( "lastSpriteBuilderTemplateID", "1" )) or 1, 1, #warningModal.optionGroupData.buttons, ReadSaveData( "lastSpriteBuilderTemplateID", "1" ))

    print("lastSelection", lastSelection)

    warningModal.optionGroupData.buttons[lastSelection].selected = true
end

function DrawTool:StartSpriteBuilder()
    
    self:ResetProcessSprites()


    self.totalSpriteCount = 0
    local files = GetEntities(self.spriteBuilderPath)

    self.spriteBuilderFiles = {}

    for i = 1, #files do
        local file = files[i]
        if(file.IsFile and file.GetExtension() == ".png") then
            table.insert(self.spriteBuilderFiles, file)
        end
    end

    -- Since this is referencing a array we need to start at 1
    self.currentParsedSpriteID = 1

    -- self.spriteList = {}
    self.spritesPerLoop = 8
    self.totalSpritesToProcess = #self.spriteBuilderFiles

    -- The action to preform on each step of the sprite progress loop
    self.onSpriteProcessAction = function()
        self:SpriteBuilderStep()
    end

    -- Open the progress model
    pixelVisionOS:OpenModal(self.progressModal,
        function() 
            
            pixelVisionOS:RemoveUI("ProgressUpdate")
            
            self:FinalizeSpriteBuilderFile()
            
        end
    )

    pixelVisionOS:RegisterUI({name = "ProgressUpdate"}, "UpdateSpriteProgress", self, true)

end

function DrawTool:SpriteBuilderStep()

    local path = self.spriteBuilderFiles[self.currentParsedSpriteID]

    -- Only load the image if the path is not nil
    if(path == nil) then
        return
    end

    -- TODO pass in system colors?
    local image = ReadImage(path, pixelVisionOS.maskColor, pixelVisionOS.systemColors)

    
    local spriteIDs = ""
    local index = -1
    local spriteData = nil
    local empty = true

    for i = 1, image.TotalSprites do
        
        spriteData = image.GetSpriteData(i-1, pixelVisionOS.colorsPerSprite)

        index = gameEditor:FindSprite(spriteData, true)

        if(empty == true and index > -1) then
            empty = false
        end

        if(index == -1 and self.addMissingSprites) then

            local missing = false

            -- Check that the sprite isn't empty
            for i = 1, #spriteData do
                if(spriteData[i] > -1) then
                    missing = true
                    break;
                end
            end

            if(missing == true) then
                print("Missing sprite", i, "in", path.EntityNameWithoutExtension)
                -- TODO need to loop backwards and find the last empty sprite
            end

        end
        
        spriteIDs = spriteIDs .. index

        if(i < image.TotalSprites) then
            spriteIDs = spriteIDs .. ","
        end

    end

    if(empty == false) then

        local name = editorUI:ValidateInputFieldText(nil, path.EntityNameWithoutExtension, "variable", "lower")

        self.totalSpriteCount = self.totalSpriteCount + 1

        self.spriteFileContents = self.spriteFileContents .. string.format(self.spriteFileTemplate, name, spriteIDs, image.Columns)
    end

end

function DrawTool:FinalizeSpriteBuilderFile()

    -- Stop the progress from updating
    pixelVisionOS:RemoveUI("ProgressUpdate")

    -- Insert the sprite template values and the final count
    self.spriteFile = string.format(self.spriteFile, self.spriteFileContents, self.totalSpriteCount)

    local title = self.toolTitle
    local message = string.format("The sprite builder has generated %d meta sprites.", self.totalSpriteCount)

    -- Test to see if the file already exists
    if(PathExists(self.spriteFilePath) == true) then
        message = string.format("%s\n\nIt looks like a sprite builder file already exists at: \n\n%s\n\nDo you want to overwrite the following file?\n\n", message, self.spriteFilePath.Path)
    else
        message = string.format("%s\n\nDo you want to save the file?", message)
    end
    
    -- Display warning message
    pixelVisionOS:ShowMessageModal(title, message, 160 + 32, true, function()
        
            if(pixelVisionOS.messageModal.selectionValue == true) then
                self:SaveSpriteFile(self.spriteFilePath, self.spriteFile)
            end
        
        end
        ,"yes"
    )

    --    -- Exit out of this function so the save call at the end isn't triggered
    --    return
    --
    --end

    -- Save the file
    --self:SaveSpriteFile(self.spriteFilePath, self.spriteFile)

end

function DrawTool:SaveSpriteFile(filePath, contents)

    SaveTextToFile(filePath, contents)

end