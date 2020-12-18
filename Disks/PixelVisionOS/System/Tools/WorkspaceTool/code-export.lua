function WorkspaceTool:OnExportGame()

    local srcPath = self.currentPath
    local destPath = srcPath.AppendDirectory("Builds")
    local infoFile = srcPath.AppendFile("info.json")
    local dataFile = srcPath.AppendFile("data.json")

    -- TODO need to read game name from info file
    if(PathExists(infoFile) == false) then
        SaveText(infoFile, "{\"name\":\""..srcPath.EntityName.."\"}")
    end

    local metaData = ReadJson(infoFile)

    local gameName = (metaData ~= nil and metaData["name"] ~= nil) and metaData["name"] or srcPath.EntityName


    local systemData = ReadJson(dataFile)

    local maxSize = 512

    if(systemData["GameChip"]) then

        if(systemData["GameChip"]["maxSize"]) then
            maxSize = systemData["GameChip"]["maxSize"]
        end
    end

    -- Manually create a game disk from the current folder's files
    local srcFiles = GetEntities(srcPath)
    local pathOffset = #srcPath.Path

    local gameFiles = {}
    
    for i = 1, #srcFiles do
        local srcFile = srcFiles[i]
        local destFile = NewWorkspacePath(srcFile.Path:sub(pathOffset))
        gameFiles[srcFile] = destFile
    end

    -- Add shared library files

    -- Get all of the shared library paths
    local libPath = SharedLibPaths()

    -- Load libs and split
    local includedLibs = string.split((metaData["includeLibs"] or ""), ",")

    local test = dump(libPath)

    for i = 1, #libPath do
        
        local tmpFiles = GetEntities(libPath[i])

        for i = 1, #tmpFiles do
            
            local srcFile = tmpFiles[i]

            if(srcFile.IsFile and srcFile.GetExtension() == ".lua") then

                    if(gameFiles[srcFile] ==nil and table.indexOf(includedLibs, srcFile.EntityNameWithoutExtension) > - 1) then
                    local destFile = NewWorkspacePath("/" .. srcFile.EntityName)
                    gameFiles[srcFile] = destFile
                end
                
            end

        end

    end

    local response = CreateDisk(gameName, gameFiles, destPath, maxSize)

    local debugResponse = dump(response)

    buildingDisk = true

    if(progressModal == nil) then
        --
        --   -- Create the model
        progressModal = ProgressModal:Init("File Action ", editorUI)

        -- Open the modal
        pixelVisionOS:OpenModal(progressModal)

        pixelVisionOS:RegisterUI({name = "ExportUpdate"}, "UpdateExport", self, true)

    end

end

function WorkspaceTool:UpdateExport()

    if(buildingDisk) then

        local total = ReadExportPercent()

        print("total", total)
        if(total >=100) then
            
            buildingDisk = false
            
            pixelVisionOS:CloseModal()
            
            local response = ReadExportMessage()
            local success = response.DiskExporter_success
            local message = response.DiskExporter_message
            local path = response.DiskExporter_path

            -- print("Disk Message", dump(response), success, message, path)

            progressModal = nil

            -- Remove the callback from the UI update loop
            pixelVisionOS:RemoveUI("ExportUpdate")

            pixelVisionOS:ShowMessageModal("Build " .. (success == true and "Complete" or "Failed"), message, 160, false,

                function()
                    if(success == true) then
                        self:OpenWindow(NewWorkspacePath(path).ParentPath)
                    end
                end
            )
        else
            if(progressModal ~= nil) then

                local message = "Building new disk.\n\n\nDo not restart or shut down Pixel Vision 8."

                progressModal:UpdateMessage(message, total/100)
            end
        end

    end
end