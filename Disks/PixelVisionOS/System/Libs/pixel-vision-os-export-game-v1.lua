function PixelVisionOS:OnExportGame(srcPath, closeModalCallback)


    self.OnCloseBuildModalCallback = closeModalCallback

    -- local srcPath = self.currentPath
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

    self.buildingDisk = true

    if(self.progressModal == nil) then
        --
        --   -- Create the model
        self.progressModal = ProgressModal:Init("File Action ")

        -- Open the modal
        pixelVisionOS:OpenModal(self.progressModal)

        pixelVisionOS:RegisterUI({name = "UpdateDiskExport"}, "UpdateDiskExport", self, true)

    end

end

function PixelVisionOS:UpdateDiskExport()

    print("UpdateDiskExport")

    if(self.buildingDisk) then

        local total = ReadExportPercent()

        print("percent", total)

        if(total >=100) then
            
            print("Done")

            self.buildingDisk = false
            
            pixelVisionOS:CloseModal()
            
            local response = ReadExportMessage()
            local success = response.DiskExporter_success
            local message = response.DiskExporter_message
            local path = response.DiskExporter_path

            print("response", dump(response))

            self.progressModal = nil

            -- Remove the callback from the UI update loop
            pixelVisionOS:RemoveUI("UpdateDiskExport")

            pixelVisionOS:ShowMessageModal("Build " .. (success == true and "Complete" or "Failed"), message, 160, false,

                function()
                    if(success == true) then
                        print("Callback action when closing build after success")
                        -- self:OpenWindow(NewWorkspacePath(path).ParentPath)
                        if(self.OnCloseBuildModalCallback ~= nil) then
                            self.OnCloseBuildModalCallback(response)
                        end
                    end
                end
            )
        else

            if(self.progressModal ~= nil) then

                local message = "Building new disk.\n\n\nDo not restart or shut down Pixel Vision 8."

                self.progressModal:UpdateMessage(message, total/100)
            end

        end

    end
end