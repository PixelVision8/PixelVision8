--[[
	Pixel Vision 8 - Debug Tool
	Copyright (C) 2016, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- Create table to store the workspace tool logic
InfoTool = {}
InfoTool.__index = InfoTool

LoadScript("code-drop-down-menu")
LoadScript("code-game-info-panel")
LoadScript("code-build-panel")
LoadScript("code-library-panel")
LoadScript("code-runner-panel")
LoadScript("code-installer-modal")
LoadScript("pixel-vision-os-progress-modal-v1")

function InfoTool:Init()

    -- Create a new table for the instance with default properties
    local _infoTool = {
    
        toolName = "Info Editor",
        runnerName = SystemName(),
        rootPath = ReadMetadata("RootPath", "/"),
        rootDirectory = ReadMetadata("directory", nil),
        targetFile = ReadMetadata("file", nil),
        invalid = true,
        maxFilesToDisplay = 6,
        fileCheckboxes = {},
        buildFlagCheckboxes = {},
        buildSettingsCheckboxes = {},
        filePaths = {},
        SaveShortcut = 5,
        -- Master list of shared library files
        sharedLibFiles = {},
        buildFlagLabels = {
            "buildWin",
            "buildMac",
            "buildLinux",
            "buildExtras",
            "buildFullscreen"
        },
        buildTemplatePaths = {
            {name = "Windows", path = NewWorkspacePath("/Workspace/System/Runners/Runner Windows.pvr")},
            {name = "Mac", path = NewWorkspacePath("/Workspace/System/Runners/Runner Mac.pvr")},
            {name = "Linux", path = NewWorkspacePath("/Workspace/System/Runners/Runner Linux.pvr")}
        }
        
    }

    -- Create a global reference of the new workspace tool
    setmetatable(_infoTool, InfoTool)


    

    if(_infoTool.targetFile ~= nil) then

        local pathSplit = string.split(_infoTool.rootDirectory, "/")
        _infoTool.toolTitle = pathSplit[#pathSplit] .. "/data.json"

        _infoTool:CreateDropDownMenu()
        
        _infoTool:CreateGameInfoPanelPanel()
        _infoTool:CreateLibraryInfoPanel()
        _infoTool:CreateBuildInfoPanel()
        _infoTool:CreateRunnerInfoPanel()

        _infoTool:ResetDataValidation()

    else

        pixelVisionOS:ChangeTitle(_infoTool.toolName, "toolbaricontool")

        DrawRect(48, 40, 160, 8, 0, DrawMode.TilemapCache)
        DrawRect(16, 72, 208, 64, 0, DrawMode.TilemapCache)
        DrawRect(16, 168, 228, 56, 11, DrawMode.TilemapCache)

        pixelVisionOS:ShowMessageModal(_infoTool.toolName .. " Error", "The tool could not load without a reference to a file to edit.", 160, false,
            function()
                QuitCurrentTool()
            end
        )

    end
    
    -- Return the draw tool data
    return _infoTool

end

function InfoTool:BuildUI()

    
    

    
    --
    --local startY = 168 + 8
    --
    ---- Get all of the shared library paths
    --local libPath = SharedLibPaths()
    --
    ---- Load libs and split
    --local includedLibs = string.split(gameEditor:ReadMetadata("includeLibs", ""), ",")
    --
    --for i = 1, #libPath do
    --
    --    local tmpFiles = GetEntities(libPath[i])
    --
    --    for i = 1, #tmpFiles do
    --
    --        local tmpFile = tmpFiles[i]
    --
    --        if(tmpFile.IsFile and tmpFile.GetExtension() == ".lua") then
    --
    --            local tmpName = tmpFile.EntityNameWithoutExtension
    --
    --            if(self.sharedLibFiles[tmpName] == nil) then
    --
    --                local selected = table.indexOf(includedLibs, tmpName) > - 1
    --
    --                -- Store the file and path
    --                self.sharedLibFiles[tmpName] = tmpFile
    --
    --                -- Add to the display list
    --                table.insert(self.filePaths, {name = tmpName, selected = selected, path = tmpFile.Path})
    --
    --            end
    --
    --        end
    --
    --    end
    --
    --
    --end
    --
    ---- Need to see if there are enough files to display
    --if(#self.filePaths < self.maxFilesToDisplay) then
    --    self.maxFilesToDisplay = #self.filePaths
    --
    --    -- TODO disable scroller
    --end
    --
    --for i = 1, self.maxFilesToDisplay do
    --    local tmpCheckbox = editorUI:CreateToggleButton({x = 16, y = startY, w = 8, h = 8}, "checkbox", "Select a library file to include with the build.")
    --    tmpCheckbox.onAction = function(value)
    --
    --        if(Key(Keys.LeftShift) or Key(Keys.RightShift)) then
    --
    --            -- Loop through all of the files
    --            for i = 1, #self.filePaths do
    --
    --                -- Change all of the file values
    --                self.filePaths[i].selected = value
    --
    --            end
    --
    --            self:DrawFileList(fileListOffset)
    --
    --        else
    --
    --            -- Change a single file value
    --            self.filePaths[i + fileListOffset].selected = value
    --        end
    --
    --        self:InvalidateData()
    --
    --    end
    --
    --    startY = startY + 8
    --
    --    table.insert(self.fileCheckboxes, tmpCheckbox)
    --
    --end
    --
    --local buildFlagWin = editorUI:CreateToggleButton({x = 200, y = 176, w = 8, h = 8}, "checkbox", "Create a Windows executable.")
    --
    --buildFlagWin.onAction = function(value)
    --
    --    self:InvalidateData()
    --
    --end
    --
    --local winRunnerExists = PathExists(NewWorkspacePath(self.buildTemplatePaths[1].path))
    --
    --if(winRunnerExists) then
    --    editorUI:ToggleButton(buildFlagWin, gameEditor:ReadMetadata(self.buildFlagLabels[1]) == "true")
    --end
    --
    --editorUI:Enable(buildFlagWin, winRunnerExists)
    --
    --table.insert(self.buildFlagCheckboxes, buildFlagWin)
    --
    --local buildFlagMac = editorUI:CreateToggleButton({x = 200, y = 184, w = 8, h = 8}, "checkbox", "Create a Mac App.")
    --
    --
    --
    --buildFlagMac.onAction = function(value)
    --
    --    self:InvalidateData()
    --
    --end
    --
    --local macRunnerExists = PathExists(NewWorkspacePath(self.buildTemplatePaths[2].path))
    --
    --if(macRunnerExists) then
    --    editorUI:ToggleButton(buildFlagMac, gameEditor:ReadMetadata(self.buildFlagLabels[2]) == "true")
    --end
    --
    --editorUI:Enable(buildFlagMac, macRunnerExists)
    --
    ---- editorUI:Enable(buildFlagFullscreen, PathExists(NewWorkspacePath("/Workspace/System/Runners/MacRunner.pvr")))
    --
    --table.insert(self.buildFlagCheckboxes, buildFlagMac)
    --
    --local buildFlagLinux = editorUI:CreateToggleButton({x = 200, y = 192, w = 8, h = 8}, "checkbox", "Create a Linux executable.")
    --
    ---- editorUI:ToggleButton(buildFlagLinux, gameEditor:ReadMetadata(buildFlagLabels[3]) == "true")
    --
    --buildFlagLinux.onAction = function(value)
    --
    --    self:InvalidateData()
    --
    --end
    --
    --local linuxRunnerExists = PathExists(NewWorkspacePath(self.buildTemplatePaths[3].path))
    --
    --if(linuxRunnerExists) then
    --    editorUI:ToggleButton(buildFlagLinux, gameEditor:ReadMetadata(self.buildFlagLabels[3]) == "true")
    --end
    --
    --editorUI:Enable(buildFlagLinux, linuxRunnerExists)
    --
    --
    --table.insert(self.buildFlagCheckboxes, buildFlagLinux)
    --
    --
    --local buildFlagExtras = editorUI:CreateToggleButton({x = 200, y = 208, w = 8, h = 8}, "checkbox", "Create a Linux executable.")
    --
    --editorUI:ToggleButton(buildFlagExtras, gameEditor:ReadMetadata(self.buildFlagLabels[4]) == "true")
    --
    --buildFlagExtras.onAction = function(value)
    --
    --    self:InvalidateData()
    --
    --end
    --
    --table.insert(self.buildFlagCheckboxes, buildFlagExtras)
    --
    --editorUI:Enable(buildFlagExtras, false)
    --
    --
    --
    --
    --local buildFlagFullscreen = editorUI:CreateToggleButton({x = 200, y = 216, w = 8, h = 8}, "checkbox", "Create a Linux executable.")
    --
    --editorUI:ToggleButton(buildFlagFullscreen, gameEditor:ReadMetadata(self.buildFlagLabels[5]) == "true")
    --
    --buildFlagFullscreen.onAction = function(value)
    --
    --    self:InvalidateData()
    --
    --end
    --
    --table.insert(self.buildFlagCheckboxes, buildFlagFullscreen)
    --
    --editorUI:Enable(buildFlagFullscreen, false)
    --
    --self.fileSliderData = editorUI:CreateSlider({x = 168, y = 168 + 8, w = 16, h = 56 - 8}, "vsliderhandle", "Scroll to see the more files to install.")
    --self.fileSliderData.onAction = function(value) self:OnFileValueChange(value) end
    --
    --editorUI:Enable(self.fileSliderData, #filePaths > maxFilesToDisplay)
    --
    --self.buildButtonData = editorUI:CreateButton({x = 216, y = 32}, "buildbutton", "Build the game disk.")
    --self.buildButtonData.onAction = function(value)
    --
    --    libFilesToCopy = {}
    --
    --    for i = 1, #filePaths do
    --        -- print("Include", dump(filePaths[i]))
    --        if(filePaths[i].selected == true) then
    --
    --            table.insert(libFilesToCopy, filePaths[i].name)
    --        end
    --    end
    --
    --    -- buildFlagWin
    --
    --    pixelVisionOS:ShowMessageModal("Build Game", "Are you sure you want to build ".. self.nameInputData.text .."'? This will create a new PV8 disk and executables for any selected platforms.", 160, true,
    --        function()
    --
    --            if(pixelVisionOS.messageModal.selectionValue == true) then
    --
    --                OnExportGame()
    --                -- TODO need to call the build function
    --                -- OnInstall()
    --
    --            end
    --
    --        end
    --    )
    --
    --end
    --
    --self.cleanCheckboxData = editorUI:CreateToggleButton({x = 216, y = 56, w = 8, h = 8}, "radiobutton", "Toggles doing a clean build and removes all previous builds.")
    --
    --editorUI:ToggleButton(self.cleanCheckboxData, gameEditor:ReadMetadata("clear", "false") == "true", false)
    --
    --self.cleanCheckboxData.onAction = function(value)
    --
    --    -- print("Clean build", value)
    --
    --    if(value == false) then
    --        self:InvalidateData()
    --        return
    --    end
    --
    --    pixelVisionOS:ShowMessageModal("Warning", "Are you sure you want to do a clean build? This will delete the build directory before creating the PV8 disk. Old builds will be deleted. This can not be undone.", 160, true,
    --        function()
    --
    --
    --            if(pixelVisionOS.messageModal.selectionValue == false) then
    --
    --                -- Force the checkbox back into the false state
    --                editorUI:ToggleButton(self.cleanCheckboxData, false, false)
    --
    --            else
    --                -- Force the checkbox back into the false state
    --                editorUI:ToggleButton(self.cleanCheckboxData, true, false)
    --            end
    --
    --            -- Force the button to redraw since restoring the modal will show the old state
    --            editorUI:Invalidate(self.cleanCheckboxData)
    --            self:InvalidateData()
    --        end
    --    )
    --
    --end
    --
    ---- TODO need to read all the libs and build flags to see what should be checked
    --
    ---- Reset list
    --self:DrawFileList()
    ---- DrawAboutLines()
    --
    --self:ResetDataValidation()


end

function InfoTool:Shutdown()

  -- Save the current session ID
  WriteSaveData("sessionID", SessionID())

  if(targetFile ~= nil) then
    
    WriteSaveData("targetFile", self.targetFile)
    
  end

end


function InfoTool:InvalidateData()

    -- Only everything if it needs to be
    if(self.invalid == true)then
        return
    end

    pixelVisionOS:ChangeTitle(self.toolTitle .."*", "toolbaricontool")
    -- pixelVisionOS:EnableActionButton(1, true)

    pixelVisionOS:EnableMenuItem(SaveShortcut, true)

    self.invalid = true

end

function InfoTool:ResetDataValidation()

    -- Only everything if it needs to be
    if(self.invalid == false)then
        return
    end

    pixelVisionOS:ChangeTitle(self.toolTitle, "toolbaricontool")
    self.invalid = false

    pixelVisionOS:EnableMenuItem(SaveShortcut, false)
    -- pixelVisionOS:EnableActionButton(1, false)
end

-- The Init() method is part of the game's lifecycle and called a game starts. We are going to
-- use this method to configure background color, ScreenBufferChip and draw a text box.
function Init()

    -- Disable the back key in this tool
    EnableBackKey(false)

    -- Create an global instance of the Pixel Vision OS
    _G["pixelVisionOS"] = PixelVisionOS:Init()

    

    -- If data loaded activate the tool
    if(success == true) then

        -- local pathSplit = string.split(rootDirectory, "/")

        -- Update title with file path
        toolTitle = pathSplit[#pathSplit] .. "/data.json"

        --LoadInstallScript(targetFile)
        local name = gameEditor:ReadMetadata("name", "untitled")
        local description = gameEditor:ReadMetadata("description", "")

        -- local wrap = WordWrap(description, 52)
        -- aboutLines = SplitLines(wrap)
        --
        -- if(#aboutLines < maxAboutLines) then
        --   maxAboutLines = #aboutLines
        -- end

        self.nameInputData = editorUI:CreateInputField({x = 48, y = 40, w = 160}, name, "Enter in a file name to this string input field.", "file")

        self.nameInputData.onAction = function(value)

            gameEditor:WriteMetadata("name", value)

            InvalidateData()

        end

        self.inputAreaData = editorUI:CreateInputArea({x = 16, y = 72, w = 208, h = 56}, description, "Click to edit the text.")
        self.inputAreaData.wrap = false
        self.inputAreaData.editable = true
        self.inputAreaData.autoDeselect = false
        self.inputAreaData.colorize = codeMode

        self.inputAreaData.onAction = function(value)

            gameEditor:WriteMetadata("description", value)

            InvalidateData()
        end

        -- Prepare the input area for scrolling
        self.inputAreaData.scrollValue = {x = 0, y = 0}

        self.vSliderData = editorUI:CreateSlider({x = 235 - 8, y = 73 - 5, w = 10, h = 56 + 9}, "vsliderhandle", "Scroll text vertically.")
        self.vSliderData.onAction = OnVerticalScroll

        self.hSliderData = editorUI:CreateSlider({ x = 16 + 4 - 8, y = 136 - 5, w = 208 + 9, h = 10}, "hsliderhandle", "Scroll text horizontally.", true)
        self.hSliderData.onAction = OnHorizontalScroll


        local startY = 168 + 8

        

        -- Get all of the shared library paths
        local libPath = SharedLibPaths()

        -- Load libs and split
        local includedLibs = string.split(gameEditor:ReadMetadata("includeLibs", ""), ",")

        for i = 1, #libPath do
            
            local tmpFiles = GetEntities(libPath[i])

            for i = 1, #tmpFiles do
                
                local tmpFile = tmpFiles[i]

                if(tmpFile.IsFile and tmpFile.GetExtension() == ".lua") then

                    local tmpName = tmpFile.EntityNameWithoutExtension

                    if(_infoTool.sharedLibFiles[tmpName] == nil) then

                        local selected = table.indexOf(includedLibs, tmpName) > - 1

                        -- Store the file and path
                        _infoTool.sharedLibFiles[tmpName] = tmpFile

                        -- Add to the display list
                        table.insert(filePaths, {name = tmpName, selected = selected, path = tmpFile.Path})
                    
                    end

                end

            end


        end

        -- local debugTotal = #sharedLibFiles
        -- local debugString = dump(sharedLibFiles)

        -- -- TODO need to get the libraries from the gameEditor
        -- local tmpFiles = gameEditor:LibraryPaths()

        -- local osFiles = GetEntitiesRecursive(NewWorkspacePath("/PixelVisionOS/Libs/"))

        -- local totalFiles = #osFiles

    
        -- local diskPaths = DiskPaths()

        -- for i = 1, #diskPaths do
        --     local tmpDiskPath = diskPaths[i].AppendDirectory("System").AppendDirectory("Libs")
        --     if(PathExists(tmpDiskPath)) then

        --     end
        -- end


        

        -- for i = 1, #tmpFiles do

        --     local name = tmpFiles[i]:sub(1, - 5)

        --     local selected = table.indexOf(includedLibs, name) > - 1

        --     table.insert(filePaths, {name = name, selected = selected})

        -- end

        -- Need to see if there are enough files to display
        if(#filePaths < maxFilesToDisplay) then
            maxFilesToDisplay = #filePaths

            -- TODO disable scroller
        end

        for i = 1, maxFilesToDisplay do
            local tmpCheckbox = editorUI:CreateToggleButton({x = 16, y = startY, w = 8, h = 8}, "checkbox", "Select a library file to include with the build.")
            tmpCheckbox.onAction = function(value)

                if(Key(Keys.LeftShift) or Key(Keys.RightShift)) then

                    -- Loop through all of the files
                    for i = 1, #filePaths do

                        -- Change all of the file values
                        filePaths[i].selected = value

                    end

                    DrawFileList(fileListOffset)

                else

                    -- Change a single file value
                    filePaths[i + fileListOffset].selected = value
                end

                InvalidateData()

            end

            startY = startY + 8

            table.insert(fileCheckboxes, tmpCheckbox)

        end


        -- Build flags

        -- This flag is always activated
        -- local buildFlagPv8 = editorUI:CreateToggleButton({x = 200, y = 168, w = 8, h = 8}, "checkbox", "Create a PV8 Disk.")
        --
        -- editorUI:ToggleButton(buildFlagPv8, true)
        -- editorUI:Enable(buildFlagPv8, false)
        --
        -- table.insert(buildFlagCheckboxes, buildFlagPv8)


        local buildFlagWin = editorUI:CreateToggleButton({x = 200, y = 176, w = 8, h = 8}, "checkbox", "Create a Windows executable.")

        buildFlagWin.onAction = function(value)

            InvalidateData()

        end

        local winRunnerExists = PathExists(NewWorkspacePath(buildTemplatePaths[1].path))

        if(winRunnerExists) then
            editorUI:ToggleButton(buildFlagWin, gameEditor:ReadMetadata(buildFlagLabels[1]) == "true")
        end

        editorUI:Enable(buildFlagWin, winRunnerExists)

        table.insert(buildFlagCheckboxes, buildFlagWin)

        local buildFlagMac = editorUI:CreateToggleButton({x = 200, y = 184, w = 8, h = 8}, "checkbox", "Create a Mac App.")



        buildFlagMac.onAction = function(value)

            InvalidateData()

        end

        local macRunnerExists = PathExists(NewWorkspacePath(buildTemplatePaths[2].path))

        if(macRunnerExists) then
            editorUI:ToggleButton(buildFlagMac, gameEditor:ReadMetadata(buildFlagLabels[2]) == "true")
        end

        editorUI:Enable(buildFlagMac, macRunnerExists)

        -- editorUI:Enable(buildFlagFullscreen, PathExists(NewWorkspacePath("/Workspace/System/Runners/MacRunner.pvr")))

        table.insert(buildFlagCheckboxes, buildFlagMac)

        local buildFlagLinux = editorUI:CreateToggleButton({x = 200, y = 192, w = 8, h = 8}, "checkbox", "Create a Linux executable.")

        -- editorUI:ToggleButton(buildFlagLinux, gameEditor:ReadMetadata(buildFlagLabels[3]) == "true")

        buildFlagLinux.onAction = function(value)

            InvalidateData()

        end

        local linuxRunnerExists = PathExists(NewWorkspacePath(buildTemplatePaths[3].path))

        if(linuxRunnerExists) then
            editorUI:ToggleButton(buildFlagLinux, gameEditor:ReadMetadata(buildFlagLabels[3]) == "true")
        end

        editorUI:Enable(buildFlagLinux, linuxRunnerExists)


        -- editorUI:Enable(buildFlagLinux, PathExists(NewWorkspacePath("/Workspace/System/Runners/LinuxRunner.pvr")))

        -- editorUI:Enable(buildFlagLinux, PathExists(NewWorkspacePath("/Workspace/System/Runners/LinuxRunner.pvr")))

        table.insert(buildFlagCheckboxes, buildFlagLinux)


        local buildFlagExtras = editorUI:CreateToggleButton({x = 200, y = 208, w = 8, h = 8}, "checkbox", "Create a Linux executable.")

        editorUI:ToggleButton(buildFlagExtras, gameEditor:ReadMetadata(buildFlagLabels[4]) == "true")

        buildFlagExtras.onAction = function(value)

            InvalidateData()

        end

        table.insert(buildFlagCheckboxes, buildFlagExtras)

        editorUI:Enable(buildFlagExtras, false)




        local buildFlagFullscreen = editorUI:CreateToggleButton({x = 200, y = 216, w = 8, h = 8}, "checkbox", "Create a Linux executable.")

        editorUI:ToggleButton(buildFlagFullscreen, gameEditor:ReadMetadata(buildFlagLabels[5]) == "true")

        buildFlagFullscreen.onAction = function(value)

            InvalidateData()

        end

        table.insert(buildFlagCheckboxes, buildFlagFullscreen)

        editorUI:Enable(buildFlagFullscreen, false)

        --
        -- table.insert(buildSettingsCheckboxes, buildFlagExtras)

        -- local buildFlagFullscreen = editorUI:CreateToggleButton({x = 200, y = 216, w = 8, h = 8}, "checkbox", "Toggle the game to load up in full screen mode.")
        --
        -- editorUI:ToggleButton(buildFlagFullscreen, gameEditor:ReadMetadata(buildFlagLabels[5]) == "true")
        --
        -- buildFlagFullscreen.onAction = function(value)
        --
        --   InvalidateData()
        --
        -- end
        --
        -- -- editorUI:Enable(buildFlagFullscreen, false)
        --
        -- table.insert(buildSettingsCheckboxes, buildFlagFullscreen)


        -- aboutSliderData = editorUI:CreateSlider({x = 227, y = 68, w = 16, h = 72}, "vsliderhandle", "Scroll to see more of the about text.")
        -- aboutSliderData.onAction = OnAboutValueChange
        --
        -- editorUI:Enable(aboutSliderData, #aboutLines > maxAboutLines)

        fileSliderData = editorUI:CreateSlider({x = 168, y = 168 + 8, w = 16, h = 56 - 8}, "vsliderhandle", "Scroll to see the more files to install.")
        fileSliderData.onAction = OnFileValueChange

        editorUI:Enable(fileSliderData, #filePaths > maxFilesToDisplay)

        buildButtonData = editorUI:CreateButton({x = 216, y = 32}, "buildbutton", "Build the game disk.")
        buildButtonData.onAction = function(value)

            libFilesToCopy = {}

            for i = 1, #filePaths do
                -- print("Include", dump(filePaths[i]))
                if(filePaths[i].selected == true) then

                    table.insert(libFilesToCopy, filePaths[i].name)
                end
            end

            -- buildFlagWin

            pixelVisionOS:ShowMessageModal("Build Game", "Are you sure you want to build ".. self.nameInputData.text .."'? This will create a new PV8 disk and executables for any selected platforms.", 160, true,
                function()

                    if(pixelVisionOS.messageModal.selectionValue == true) then

                        OnExportGame()
                        -- TODO need to call the build function
                        -- OnInstall()

                    end

                end
            )

        end

        cleanCheckboxData = editorUI:CreateToggleButton({x = 216, y = 56, w = 8, h = 8}, "radiobutton", "Toggles doing a clean build and removes all previous builds.")

        editorUI:ToggleButton(cleanCheckboxData, gameEditor:ReadMetadata("clear", "false") == "true", false)

        cleanCheckboxData.onAction = function(value)

            -- print("Clean build", value)

            if(value == false) then
                InvalidateData()
                return
            end

            pixelVisionOS:ShowMessageModal("Warning", "Are you sure you want to do a clean build? This will delete the build directory before creating the PV8 disk. Old builds will be deleted. This can not be undone.", 160, true,
                function()


                    if(pixelVisionOS.messageModal.selectionValue == false) then

                        -- Force the checkbox back into the false state
                        editorUI:ToggleButton(cleanCheckboxData, false, false)

                    else
                        -- Force the checkbox back into the false state
                        editorUI:ToggleButton(cleanCheckboxData, true, false)
                    end

                    -- Force the button to redraw since restoring the modal will show the old state
                    editorUI:Invalidate(cleanCheckboxData)
                    InvalidateData()
                end
            )




        end

        -- TODO need to read all the libs and build flags to see what should be checked

        -- Reset list
        DrawFileList()
        -- DrawAboutLines()

        ResetDataValidation()

    else

        pixelVisionOS:ChangeTitle(toolName, "toolbaricontool")

        DrawRect(48, 40, 160, 8, 0, DrawMode.TilemapCache)
        DrawRect(16, 72, 208, 64, 0, DrawMode.TilemapCache)
        DrawRect(16, 168, 228, 56, 11, DrawMode.TilemapCache)

        pixelVisionOS:ShowMessageModal(toolName .. " Error", "The tool could not load without a reference to a file to edit.", 160, false,
            function()
                QuitCurrentTool()
            end
        )
    end

end

function OnExportGame()

    local srcPath = NewWorkspacePath(rootDirectory)
    local destPath = srcPath.AppendDirectory("Builds")
    local infoFile = srcPath.AppendFile("info.json")
    local dataFile = srcPath.AppendFile("data.json")

    -- TODO need to read game name from info file
    if(PathExists(srcPath.AppendDirectory("info.json")) == false) then
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

    local response = CreateDisk(gameName, gameFiles, destPath, maxSize)

    buildingDisk = true

    if(progressModal == nil) then
        --
        --   -- Create the model
        progressModal = ProgressModal:Init("File Action ", editorUI)

        -- Open the modal
        pixelVisionOS:OpenModal(progressModal)

    end

    -- end

end

-- function LoadInstallScript(path)
--
--   local rawData = ReadTextFile(path)
--
--   biosProperties = {}
--   variables = {}
--   filePaths = {}
--
--   for s in rawData:gmatch("[^\r\n]+") do
--
--     local type = string.sub(s, 1, 1)
--
--     if(type == "/") then
--
--       -- Make sure the file exists
--       -- if(PathExists(s)) then
--
--       -- Add the path to the list
--       table.insert(filePaths, {s, true})
--
--       -- end
--
--     elseif(type == "$") then
--
--       -- Need to check if this is a bios property or a variable
--       if(string.sub(s, 1, 5) == "$bios") then
--         local split = string.split(string.sub(s, 7, #s), "|")
--
--         -- print("bios", split[1], split[2])
--
--         table.insert(biosProperties, split)
--
--       else
--
--         local split = string.split(string.sub(s, 2, #s), "=")
--
--         -- print("var", split[1], split[2])
--         variables[split[1]] = split[2]
--
--       end
--
--     end
--
--     -- print("Type", type, s)
--
--     -- table.insert(lines, s)
--   end
--
-- end

function OnAboutValueChange(value)

    local offset = math.ceil((#aboutLines - maxAboutLines - 1) * value)

    -- DrawAboutLines(offset)

end

-- function DrawAboutLines(offset)
--
--   DrawRect(16, 64 + 8, 208, 64, 0, DrawMode.TilemapCache)
--   offset = offset or 0
--
--   for i = 1, maxAboutLines do
--
--     local line = aboutLines[i + offset]
--
--     line = string.rpad(line, 52, " "):upper()
--     DrawText(line, 16, 64 + (i * 8), DrawMode.TilemapCache, "medium", 15, - 4)
--
--   end
--
-- end






-- function OnInstall()

--     local srcPath = NewWorkspacePath(rootDirectory)
--     local destPath = srcPath.AppendDirectory("Builds")

--     if (cleanCheckboxData.selected == true and PathExists(destPath)) then
--         Delete(destPath)
--     end

--     -- TODO need to read game name from info file

--     local metaData = ReadJson(srcPath.AppendFile("info.json"))

--     gameName = metaData["name"] or srcPath.EntityName

--     -- Manually create a game disk from the current folder's files
--     gameFiles = GetEntities(srcPath)

--     local diskSize = 512 -- TODO this needs to come from the system specs

--     lastBuildResponse = CreateDisk(gameName, gameFiles, destPath, diskSize, libFilesToCopy)

--     if(lastBuildResponse.success == false) then
--         -- Display error
--         pixelVisionOS:ShowMessageModal("Build Failed", lastBuildResponse.message, 160, false)

--     else

--         lastBuildPath = NewWorkspacePath(lastBuildResponse.path)

--         buildPaths = {}

--         -- Look for other builds
--         for i = 1, #buildFlagCheckboxes do
--             if(buildFlagCheckboxes[i].selected == true and buildFlagCheckboxes[i].enabled == true) then
--                 table.insert(buildPaths, buildTemplatePaths[i])
--             end
--         end

--         -- Start build process
--         installing = true

--         installingTime = 0
--         installingDelay = .5
--         installingCounter = 0
--         installingTotal = #buildPaths

--     end
-- end

-- function OnInstallNextStep()

--     if(#buildPaths == 0) then
--         return
--     end
--     -- Look to see if the modal exists
--     if(installingModal == nil) then

--         -- Create the model
--         installingModal = InstallerModal:Init("Building", editorUI)

--         -- Open the modal
--         pixelVisionOS:OpenModal(installingModal)

--     end

--     installingCounter = installingCounter + 1

--     local path = buildPaths[installingCounter].path

--     if(PathExists(path)) then

--         local dest = NewWorkspacePath(lastBuildResponse.path).ParentPath

--         CreateExe(gameName .. " " .. buildPaths[installingCounter].name, gameFiles, path, dest, libFilesToCopy)

--         installingModal:UpdateMessage(installingCounter, installingTotal)


--     end

--     if(installingCounter >= installingTotal) then
--         installingDelay = 4
--     end

-- end

-- function OnInstallComplete()

--     installing = false
--     --
--     -- installingModal:OnComplete()

--     pixelVisionOS:CloseModal()

--     pixelVisionOS:ShowMessageModal("Build " .. (lastBuildResponse.success == true and "Complete" or "Failed"), lastBuildResponse.message .. " Any external builds will not be visible inside Pixel Vision 8. Please go to the Workspace folder on your desktop to see the exported games that were created.", 160, false,

--         function()
--             if(lastBuildResponse.success == true) then

--                 local metaData = {
--                     lastPath = NewWorkspacePath(lastBuildResponse.path).ParentPath.path
--                 }

--                 QuitCurrentTool(metaData)

--             end
--         end
--     )

-- end

-- The Update() method is part of the game's life cycle. The engine calls Update() on every frame
-- before the Draw() method. It accepts one argument, timeDelta, which is the difference in
-- milliseconds since the last frame.
function Update(timeDelta)

    -- Convert timeDelta to a float
    timeDelta = timeDelta / 1000

    -- This needs to be the first call to make sure all of the OS and editor UI is updated first
    pixelVisionOS:Update(timeDelta)

    -- Only update the tool's UI when the modal isn't active
    if(pixelVisionOS:IsModalActive() == false) then

        -- editorUI:UpdateInputField(self.nameInputData)

        -- editorUI:UpdateInputArea(self.inputAreaData)

        -- if(self.inputAreaData.invalidText == true) then
        --   InvalidateData()
        --   -- InvalidateLineNumbers()
        -- end

        -- -- Check to see if we should show the horizontal slider
        -- local showVSlider = #self.inputAreaData.buffer > self.inputAreaData.tiles.h

        -- -- Test if we need to show or hide the slider
        -- if(self.vSliderData.enabled ~= showVSlider) then
        --     editorUI:Enable(self.vSliderData, showVSlider)
        -- end

        -- if(self.vSliderData.enabled == true) then
        --     self.inputAreaData.scrollValue.y = (self.inputAreaData.vy - 1) / (#self.inputAreaData.buffer - self.inputAreaData.tiles.h)

        --     if(self.vSliderData.value ~= self.inputAreaData.scrollValue.y) then

        --         -- InvalidateLineNumbers()

        --         editorUI:ChangeSlider(self.vSliderData, self.inputAreaData.scrollValue.y, false)
        --     end

        -- end

        -- -- Update the slider
        -- editorUI:UpdateSlider(self.vSliderData)

        -- -- Check to see if we should show the vertical slider
        -- local showHSlider = self.inputAreaData.maxLineWidth > self.inputAreaData.tiles.w

        -- -- Test if we need to show or hide the slider
        -- if(self.hSliderData.enabled ~= showHSlider) then
        --     editorUI:Enable(self.hSliderData, showHSlider)
        -- end

        -- if(self.hSliderData.enabled == true) then
        --     self.inputAreaData.scrollValue.x = (self.inputAreaData.vx - 1) / ((self.inputAreaData.maxLineWidth + 1) - self.inputAreaData.tiles.w)

        --     if(self.hSliderData.value ~= self.inputAreaData.scrollValue.x) then
        --         -- print(self.inputAreaData.vx, self.inputAreaData.maxLineWidth, self.inputAreaData.tiles.w)
        --         -- print("self.inputAreaData.scrollValue.x", self.inputAreaData.scrollValue.x)

        --         editorUI:ChangeSlider(self.hSliderData, self.inputAreaData.scrollValue.x, false)
        --     end

        -- end

        -- -- Update the slider
        -- editorUI:UpdateSlider(self.hSliderData)


        editorUI:UpdateButton(buildButtonData)
        editorUI:UpdateButton(cleanCheckboxData)

        for i = 1, maxFilesToDisplay do
            editorUI:UpdateButton(fileCheckboxes[i])
        end

        for i = 1, #buildFlagCheckboxes do
            editorUI:UpdateButton(buildFlagCheckboxes[i])
        end

        editorUI:UpdateButton(buildFlagExtras)
        editorUI:UpdateButton(buildFlagFullscreen)

        -- editorUI:UpdateSlider(aboutSliderData)
        editorUI:UpdateSlider(fileSliderData)

    end

    -- if(installing == true) then


    --     installingTime = installingTime + timeDelta

    --     if(installingTime > installingDelay) then
    --         installingTime = 0


    --         OnInstallNextStep()

    --         if(installingCounter >= installingTotal) then

    --             OnInstallComplete()

    --         end

    --     end


    -- end

    if(buildingDisk) then

        local total = ReadExportPercent()

        if(total >=100) then
            
            buildingDisk = false
            
            pixelVisionOS:CloseModal()
            
            local response = ReadExportMessage()
            local success = response.DiskExporter_success
            local message = response.DiskExporter_message
            local path = response.DiskExporter_path

            -- print("Disk Message", dump(response), success, message, path)

            progressModal = nil

            pixelVisionOS:ShowMessageModal("Build " .. (success == true and "Complete" or "Failed"), message, 160, false,

                function()
                    if(success == true) then

                        local metaData = {
                            lastPath = NewWorkspacePath(path).ParentPath.path
                        }
        
                        QuitCurrentTool(metaData)

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

-- The Draw() method is part of the game's life cycle. It is called after Update() and is where
-- all of our draw calls should go. We'll be using this to render sprites to the display.
function Draw()

    -- We can use the RedrawDisplay() method to clear the screen and redraw the tilemap in a
    -- single call.
    RedrawDisplay()

    -- The UI should be the last thing to draw after your own custom draw calls
    pixelVisionOS:Draw()

end





function OnReset()

    pixelVisionOS:ShowMessageModal("Reset Installer", "Do you want to reset the installer to its default values?", 160, true,
        function()
            if(pixelVisionOS.messageModal.selectionValue == true) then

                for i = 1, #filePaths do
                    filePaths[i].selected = true
                end

                DrawFileList(fileListOffset)
            end
        end
    )

end


function OnSave()

    local flags = {SaveFlags.Meta}

    local includeString = ""

    for i = 1, #filePaths do

        local file = filePaths[i]

        if(file.selected == true) then
            includeString = includeString .. file.name .. ","
        end

    end

    gameEditor:WriteMetadata("clear", tostring(cleanCheckboxData.selected))

    -- Write the includeString and remove the last comma from the end

    gameEditor:WriteMetadata("includeLibs", includeString:sub(1, - 2))

    -- Add the build flags
    for i = 1, #buildFlagCheckboxes do
        gameEditor:WriteMetadata(buildFlagLabels[i], tostring(buildFlagCheckboxes[i].selected))
    end

    -- TODO need to save music and sounds when those are broken out
    gameEditor:Save(rootDirectory, flags)

    -- Display that the data was saved and reset invalidation
    pixelVisionOS:DisplayMessage("The game's 'data.json' file has been updated.", 5)

    ResetDataValidation()

end

-- function OnHorizontalScroll(value)

--     local charPos = math.ceil(((self.inputAreaData.maxLineWidth + 1) - (self.inputAreaData.tiles.w)) * value) + 1

--     if(self.inputAreaData.vx ~= charPos) then
--         self.inputAreaData.vx = charPos
--         editorUI:TextEditorInvalidateBuffer(self.inputAreaData)
--     end

-- end

-- function OnVerticalScroll(value)

--     local line = math.ceil((#self.inputAreaData.buffer - (self.inputAreaData.tiles.h - 1)) * value)
--     if(self.inputAreaData.vy ~= line) then
--         self.inputAreaData.vy = Clamp(line, 1, #self.inputAreaData.buffer)

--         editorUI:TextEditorInvalidateBuffer(self.inputAreaData)
--     end

--     -- InvalidateLineNumbers()

-- end
