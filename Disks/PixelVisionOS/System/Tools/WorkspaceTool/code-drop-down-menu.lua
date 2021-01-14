-- Global sortcut enums
NewFolderShortcut, EditShortcut, RenameShortcut, CopyShortcut, PasteShortcut, DeleteShortcut, EmptyTrashShortcut, EjectDiskShortcut, NewGameShortcut, RunShortcut, BuildShortcut = "New Folder", "Edit", "Rename", "Copy", "Paste", "Delete", "Empty Trash", "Eject Disk", nil, nil, nil

-- Global focus enums
WindowFocus, DesktopIconFocus, WindowIconFocus, MultipleFiles, NoFocus = 1, 2, 3, 4, 5

function WorkspaceTool:CreateDropDownMenu()

    local tmpProjectPath = ReadBiosData("ProjectTemplate")
    self.fileTemplatePath = tmpProjectPath == nil and NewWorkspacePath(ReadMetadata("RootPath", "/")).AppendDirectory(ReadMetadata("GameName", "untitled")).AppendDirectory("ProjectTemplate") or NewWorkspacePath(tmpProjectPath)

    print("Template Path", self.fileTemplatePath, PathExists(self.fileTemplatePath))
    -- Create some enums for the focus typess

    -- TODO need to see if the log file actually exists
    local logExits = PathExists(NewWorkspacePath("/Tmp/Log.txt"))--true

    local aboutText = "The ".. self.toolName.. " offers you access to the underlying file system. "

    if(TmpPath() ~= nil) then
        aboutText = aboutText .. "\n\nTemporary files are stores on your computer at: \n\n" .. TmpPath()
    end

    if(DocumentPath() ~= nil) then

        aboutText = aboutText .. "\n\nYou can access the 'Workspace' drive on your computer at: \n\n" .. DocumentPath()

    end

    local menuOptions =
    {
        -- About ID 1
        {name = "About", action = function() pixelVisionOS:ShowAboutModal(self.toolName, aboutText, 220) end, toolTip = "Learn about PV8."},
        -- Settings ID 2
        {name = "Settings", action = function() self:OnLaunchSettings() end, toolTip = "Configure Pixel Vision OS's Settings."},
        -- Settings ID 3
        {name = "View Log", enabled = logExits, action = function() self:OnLaunchLog() end, toolTip = "Open up the log file."},

        {divider = true},

        -- New Folder ID 5
        {name = "New Folder", action = function() self:OnNewFolder() end, key = Keys.N, enabled = false, toolTip = "Create a new file."},

        {divider = true},

        -- Edit ID 7
        -- {name = "Edit", key = Keys.E, action = OnEdit, enabled = false, toolTip = "Edit the selected file."},
        -- Edit ID 8
        {name = "Rename", action = function() self:OnRename() end, enabled = false, toolTip = "Rename the currently selected file."},
        -- Copy ID 9
        {name = "Copy", key = Keys.C, action = function() self:OnCopy() end, enabled = false, toolTip = "Copy the selected file."},
        -- Paste ID 10
        {name = "Paste", key = Keys.V, action = function() self:OnPaste() end, enabled = false, toolTip = "Paste the selected file."},
        -- Delete ID 11
        {name = "Delete", key = Keys.D, action = function() self:OnDeleteFile() end, enabled = false, toolTip = "Delete the current file."},
        {divider = true},

        -- Empty Trash ID 16
        {name = "Empty Trash", action = function() self:OnEmptyTrash() end, enabled = false, toolTip = "Delete everything in the trash."},
        -- Eject ID 17
        {name = "Eject Disk", action = function() self:OnEjectDisk() end, enabled = false, toolTip = "Eject the currently selected disk."},
        -- Shutdown ID 18
        {name = "Shutdown", action = function() self:OnShutdown() end, toolTip = "Shutdown PV8."} -- Quit the current game
    }

    local addAt = 6

    if(PathExists(self.fileTemplatePath) == true) then

        table.insert(menuOptions, addAt, {name = "New Project", key = Keys.P, action = function() self:OnNewProject() end, enabled = false, toolTip = "Create a new file."})

        NewGameShortcut = "New Project"

        addAt = addAt + 1

        print("New Project")

    end

    self.newFileOptions = {}

    -- TODO this should be done better

    -- if(runnerName == DrawVersion or runnerName == TuneVersion) then

    table.insert(menuOptions, addAt, {name = "New Data", action = function() self:OnNewFile("data", "json", "data", false) end, enabled = false, toolTip = "Run the current game."})
    table.insert(self.newFileOptions, {name = "New Data", file = "data.json"})
    addAt = addAt + 1

    -- end

    -- print("Code Exists ", self.fileTemplatePath.AppendFile("code.lua"), self.fileTemplatePath)
    -- Add text options to the menu
    -- if(runnerName ~= PlayVersion and runnerName ~= DrawVersion and runnerName ~= TuneVersion) then
    -- if(PathExists(self.fileTemplatePath.AppendFile("code.lua"))) then
    table.insert(menuOptions, addAt, {name = "New Code", action = function() self:CreateNewCodeFile() end, enabled = false, toolTip = "Run the current game."})
    table.insert(self.newFileOptions, {name = "New Code"})
    addAt = addAt + 1
    -- end

    -- if(PathExists(self.fileTemplatePath.AppendFile("json.json"))) then
    table.insert(menuOptions, addAt, {name = "New JSON", action = function() self:OnNewFile("untitled", "json") end, enabled = false, toolTip = "Run the current game."})
    table.insert(self.newFileOptions, {name = "New JSON"})
    addAt = addAt + 1
    -- end

    -- Add draw options

    if(PathExists(self.fileTemplatePath.AppendFile("colors.png"))) then
        table.insert(menuOptions, addAt, {name = "New Colors", action = function() self:OnNewFile("colors", "png", "colors", false) end, enabled = false, toolTip = "Run the current game.", file = "colors.png"})
        table.insert(self.newFileOptions, {name = "New Colors", file = "colors.png"})
        addAt = addAt + 1
    end

    if(PathExists(self.fileTemplatePath.AppendFile("sprites.png"))) then

        table.insert(menuOptions, addAt, {name = "New Sprites", action = function() self:OnNewFile("sprites", "png", "sprites", false) end, enabled = false, toolTip = "Run the current game.", file = "sprites.png"})
        table.insert(self.newFileOptions, {name = "New Sprites", file = "sprites.png"})
        addAt = addAt + 1
    end

    if(PathExists(self.fileTemplatePath.AppendFile("large.font.png"))) then

        table.insert(menuOptions, addAt, {name = "New Font", action = function() self:OnNewFile("untitled", "font.png", "font") end, enabled = false, toolTip = "Run the current game."})
        table.insert(self.newFileOptions, {name = "New Font"})
        addAt = addAt + 1

    end

    if(PathExists(self.fileTemplatePath.AppendFile("tilemap.json"))) then

        table.insert(menuOptions, addAt, {name = "New Tilemap", action = function() self:OnNewFile("tilemap", "json", "tilemap", false) end, enabled = false, toolTip = "Run the current game.", file = "tilemap.json"})
        table.insert(self.newFileOptions, {name = "New Tilemap", file = "tilemap.json"})
        addAt = addAt + 1

    end

    -- Add music options

    if(PathExists(self.fileTemplatePath.AppendFile("sounds.json"))) then

        table.insert(menuOptions, addAt, {name = "New Sounds", action = function() self:OnNewFile("sounds", "json", "sounds", false) end, enabled = false, toolTip = "Run the current game.", file = "sounds.json"})
        table.insert(self.newFileOptions, {name = "New Sounds", file = "sounds.json"})
        addAt = addAt + 1
    end

    if(PathExists(self.fileTemplatePath.AppendFile("music.json"))) then

        table.insert(menuOptions, addAt, {name = "New Music", action = function() self:OnNewFile("music", "json", "music", false) end, enabled = false, toolTip = "Run the current game.", file = "music.json"})
        table.insert(self.newFileOptions, {name = "New Music", file = "music.json"})
        addAt = addAt + 1

    end

    if(PathExists(self.fileTemplatePath.AppendFile("code.lua")) or PathExists(self.fileTemplatePath.AppendFile("code.cs"))) then

        -- TODO need to add to the offset
        addAt = addAt + 6

        -- Empty Trash ID 13
        table.insert(menuOptions, addAt, {name = "Run", key = Keys.R, action = function() self:OnRun() end, enabled = false, toolTip = "Run the current game."})
        addAt = addAt + 1

        table.insert(menuOptions, addAt, {name = "Build", action = function() self:OnExportGame() end, enabled = false, toolTip = "Create a PV8 file from the current game."})
        addAt = addAt + 1

        table.insert(menuOptions, addAt, {divider = true})
        addAt = addAt + 1

        RunShortcut = "Run"
        BuildShortcut = "Build"

    end

    pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")

end

function WorkspaceTool:CreateNewCodeFile(defaultPath)

    local templatePath = NewWorkspacePath(ReadMetadata("RootPath", "/")).AppendDirectory(ReadMetadata("GameName", "untitled")).AppendDirectory("CodeTemplates")

    defaultPath = defaultPath or self.currentPath

    local fileName = "code"
    local ext = ".lua"

    local infoFilePath = defaultPath.AppendFile("info.json")

    if(PathExists(infoFilePath)) then

        local data = ReadJson(infoFilePath)

        print(dump(data))

        if(data["runnerType"] ~= nil) then
            ext = data["runnerType"] ~= "lua" and  ".cs" or ".lua"
        end

    elseif(PathExists(defaultPath.AppendFile("code.cs"))) then

        ext = ".cs"

    end

    local empty = PathExists(defaultPath.AppendFile(fileName .. ext))

    print("Create new code file at", defaultPath, fileName, ext)

    if(empty ~= true) then

        local newPath = defaultPath.AppendFile(fileName .. ext)

        print("Create file", templatePath.AppendFile("main-" .. fileName .. ext), "in", defaultPath.AppendFile(fileName .. ext))

        CopyTo(templatePath.AppendFile("main-" .. fileName .. ext), newPath)

        self:RefreshWindow(true)

        self:SelectFile(newPath)

    else

        local newFileModal = self:GetNewFileModal()

        newFileModal:SetText("New " .. (ext == ".cs" and "C# File" or "Lua"), "code", "Name code file", true)

        pixelVisionOS:OpenModal(newFileModal,
            function()

                -- Check to see if ok was pressed on the model
                if(newFileModal.selectionValue == true) then

                    local newPath = UniqueFilePath(defaultPath.AppendFile(newFileModal.inputField.text .. ext))
                    
                    local templatePath = templatePath.AppendFile("empty-" .. fileName .. ext)

                    -- TODO if this is a C# file, we need to rename the class
                    if(ext == ".cs") then

                        local codeTemplate = ReadTextFile(templatePath)

                        local newClassName = newPath.EntityNameWithoutExtension:sub(1,1):upper() .. newPath.EntityNameWithoutExtension:gsub('%W',' '):gsub("%W%l", string.upper):sub(2):gsub('%W','') .. "Class"

                        codeTemplate = codeTemplate:gsub( "CustomClass", newClassName)


                        print("newClassName", newClassName)
                        
                        SaveTextToFile(newPath, codeTemplate)


                    else

                        -- Just copy the Lua template as is
                        CopyTo(templatePath, newPath)

                    end

                    
                    self:RefreshWindow(true)

                    self:SelectFile(newPath)

                end

            end
        )   

        -- self:OnNewFile("code", "lua")
    end

end

function WorkspaceTool:OnEjectDisk()

    -- Get all of the selected  files
    local selections = self:CurrentlySelectedFiles()
    
    -- If there is more than one selection, exit
    if(#selections > 1) then
        return
    end

    -- Get the first selection
    local currentSelection = self.files[selections[1]]
    
    -- Make sure that the selection is a disk
    if(currentSelection.type ~= "disk") then
        
        return
    end
        
    -- Ask before ejecting a disk
    pixelVisionOS:ShowMessageModal("Eject Disk", "Do you want to eject the '".. currentSelection.name .."'disk?", 160, true,
            function()

                -- Only perform the copy if the user selects OK from the modal
                if(pixelVisionOS.messageModal.selectionValue) then

                    EjectDisk(currentSelection.path)

                    --ResetGame()

                end

            end
    )

end


function WorkspaceTool:UpdateContextMenu()

    -- print("UpdateContextMenu", inFocus)

    local selections = self:CurrentlySelectedFiles()

    -- Check to see if currentPath is a game
    local canRun = self.focus == true and self.isGameDir--and pixelVisionOS:ValidateGameInDir(self.currentPath, {"code.lua"})-- and selections

    -- Look to see if the selection is a special file (parent dir or run)
    local specialFile = false

    -- Get the first file which is the current selection
    local currentSelection = nil

    if(selections ~= nil) then

        currentSelection = self.files[selections[1]]

        for i = 1, self.totalSingleSelectFiles do

            local tmpFile = self.files[selections[1]]

            if(tmpFile.type == "installer" or tmpFile.type == "updirectory" or tmpFile.type == "run" or tmpFile.type == "trash" or tmpFile.type == "drive" or tmpFile.type == "disk" ) then
                specialFile = true
                break
            end

        end

    end

    local trashOpen = self:TrashOpen()

    -- Test to see if you can rename
    local canEdit = self.focus == true and selections ~= nil and #selections == 1 and specialFile == false and trashOpen == false

    local canEject = self.focus == false and specialFile == true and currentSelection.type == "disk"

    local canCreateFile = self.focus == true and trashOpen == false

    -- local canCreateFile = canCreateFile == true and self.currentPath.Path ~= self.workspacePath.Path

    local canCreateProject = canCreateFile == true and canRun == false

    local canBuild = canRun == true and string.starts(self.currentPath.Path, "/Disks/") == false

    local canCopy = self.focus == true and selections ~= nil and #selections > 0 and specialFile == false and trashOpen == false
    local canPaste = canCreateFile == true and pixelVisionOS:ClipboardFull() == true

    pixelVisionOS:EnableMenuItemByName(RenameShortcut, canEdit)

    pixelVisionOS:EnableMenuItemByName(CopyShortcut, canCopy)

    pixelVisionOS:EnableMenuItemByName(PasteShortcut, canPaste)

    pixelVisionOS:EnableMenuItemByName(DeleteShortcut, canCopy)

    pixelVisionOS:EnableMenuItemByName(BuildShortcut, canBuild)

    pixelVisionOS:EnableMenuItemByName(NewGameShortcut, canCreateProject)

    pixelVisionOS:EnableMenuItemByName(NewFolderShortcut, canCreateFile)

    pixelVisionOS:EnableMenuItemByName(RunShortcut, canRun)

    pixelVisionOS:EnableMenuItemByName(EmptyTrashShortcut, #GetEntities(self.trashPath) > 0)

    pixelVisionOS:EnableMenuItemByName(EjectDiskShortcut, canEject)

    -- Loop through all the file creation options
    for i = 1, #self.newFileOptions do

        -- Get the new file option data
        local option = self.newFileOptions[i]

        local enable = canCreateFile

        -- Check to see if the option should be enabled
        if(enable and option.file ~= nil) then

            -- Change the enable flag based on if the file exists
            enable = not PathExists(self.currentPath.AppendFile(option.file))

        end

        -- Enable the file in the menu
        pixelVisionOS:EnableMenuItemByName(option.name, enable)

    end

end

function WorkspaceTool:ToggleOptions(enabled)

    -- Loop through all the file creation options
    for i = 1, #self.newFileOptions do

        -- Get the new file option data
        local option = self.newFileOptions[i]

        -- Check to see if the option should be enabled
        if(enable == true and option.file ~= nil) then

            -- Change the enable flag based on if the file exists
            enable = not PathExists(self.currentPath.AppendFile(option.file))

        end

        -- Enable the file in the menu
        pixelVisionOS:EnableMenuItemByName(option.name, enable)

    end

end


function WorkspaceTool:OnMenuQuit()

    QuitCurrentTool()

end

function WorkspaceTool:OnLaunchSettings()

    local editorPath = ReadBiosData("SettingsEditor")

    if(editorPath == nil) then
        editorPath = self.rootPath .."SettingsTool/"
    end

    LoadGame(editorPath)

end

function WorkspaceTool:OnLaunchLog()

    -- Get a list of all the editors
    local editorMapping = pixelVisionOS:FindEditors()

    -- Find the json editor
    textEditorPath = editorMapping["txt"]

    local metaData = {
        directory = "/Tmp/",
        file = "/Tmp/Log.txt"
    }

    LoadGame(textEditorPath, metaData)

end

function WorkspaceTool:OnShutdown()

    self:CancelFileActions()

    local runnerName = SystemName()

    local this = self

    pixelVisionOS:ShowMessageModal("Shutdown " .. runnerName, "Are you sure you want to shutdown "..runnerName.."?", 160, true,
            function()
                if(pixelVisionOS.messageModal.selectionValue == true) then

                    ShutdownSystem()

                    -- Save changes
                    this.shuttingDown = true

                end

            end
    )

end