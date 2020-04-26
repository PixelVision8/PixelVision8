    --[[
	Pixel Vision 8 - Debug Tool
	Copyright (C) 2016, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- Load in the editor framework script to access tool components


-- new code modules
LoadScript("sb-sprites")
LoadScript("pixel-vision-os-v2")
LoadScript("code-workspace-tool")

-- Create an global instance of the Pixel Vision OS
_G["pixelVisionOS"] = PixelVisionOS:Init()

-- Reference to the workspace tool
local workspaceTool = nil

-- local toolName = "Workspace Explorer V2"
local filesToCopy = nil
-- local pixelVisionOS = nil
-- local editorUI = nil
-- local lastStartID = nil
-- local windowInvalidated = false
-- local DrawVersion, TuneVersion = "Pixel Vision 8 Draw", "Pixel Vision 8 Tune"
-- local runnerName = SystemName()
-- local totalPerWindow = 12
-- local currentDirectory = nil
-- local shuttingDown = false
-- local files = nil
local windowIconButtons = nil
-- local trashPath = NewWorkspacePath("/Tmp/Trash/")
-- local refreshTime = 0
-- local refreshDelay = 5
-- local fileCount = 0
-- local overTarget = nil
-- local shutdownScreen = false

-- local fileTypeMap = 
-- {
--     folder = "filefolder",
--     updirectory = "fileparentfolder",
--     lua = "filecode",
--     json = "filejson",
--     png = "filepng",
--     run = "filerun", -- TODO need to change this to run
--     txt = "filetext",
--     installer = "fileinstaller", -- TODO need a custom icon
--     info = "fileinfo",
--     pv8 = "diskempty",
--     pvr = "disksystem",
--     wav = "filewav",

--     -- TODO these are not core file types
--     unknown = "fileunknown",
--     colors = "filecolor",
--     system = "filesettings",
--     font = "filefont",
--     music = "filemusic",
--     sounds = "filesound",
--     sprites = "filesprites",
--     tilemap = "filetilemap",
--     pvt = "filerun",
--     new = "filenewfile",
--     gif = "filegif",
--     tiles = "filetiles"
-- }

-- local extToTypeMap = 
-- {
--     colors = ".png",
--     system = ".json",
--     font = ".font.png",
--     music = ".json",
--     sounds = ".json",
--     sprites = ".png",
--     tilemap = ".json",
--     installer = ".txt",
--     info = ".json",
--     wav = ".wav"
-- }

-- local rootPath = ReadMetadata("RootPath", "/")
-- local gameName = ReadMetadata("GameName", "FilePickerTool")

-- local windowScrollHistory = {}

-- local newFileModal = nil

-- local fileTemplatePath = nil

-- local tmpProjectPath = ReadBiosData("ProjectTemplate")

-- fileTemplatePath = tmpProjectPath == nil and NewWorkspacePath(rootPath .. gameName .. "/ProjectTemplate/") or NewWorkspacePath(tmpProjectPath)

-- -- This this is an empty game, we will the following text. We combined two sets of fonts into
-- -- the default.font.png. Use uppercase for larger characters and lowercase for a smaller one.
-- local title = "EMPTY TOOL"
-- local messageTxt = "This is an empty tool template. Press Ctrl + 1 to open the editor or modify the files found in your workspace game folder."

-- -- Container for horizontal slider data

-- local desktopIcons = nil
-- local vSliderData = nil

-- local currentSelectedFile = nil

-- -- Flags for managing focus
-- local WindowFocus, DesktopIconFocus, WindowIconFocus, NoFocus = 1, 2, 3, 4

-- local desktopIcons = {}

-- NewFolderShortcut, EditShortcut, RenameShortcut, CopyShortcut, PasteShortcut, DeleteShortcut, EmptyTrashShortcut, EjectDiskShortcut = "New Folder", "Edit", "Rename", "Copy", "Paste", "Delete", "Empty Trash", "Eject Disk"



-- Get all of the available editors
local editorMapping = {}

-- The Init() method is part of the game's lifecycle and called a game starts. We are going to
-- use this method to configure background color, ScreenBufferChip and draw a text box.
function Init()

    -- Disable the back key in this tool
    EnableBackKey(false)
    EnableAutoRun(false)

    -- Update background
    BackgroundColor(tonumber(ReadBiosData("DefaultBackgroundColor", "5")))

    -- Create new workspace tool instance
    workspaceTool = WorkspaceTool:Init()

    -- pixelVisionOS:EnableClipboard(true)

    -- TODO legacy code

    -- runningFromDisk = string.starts(rootPath, "/Disks/")

    -- DrawWallpaper()

    -- -- Create an instance of the Pixel Vision OS
    -- pixelVisionOS = PixelVisionOS:Init()

    -- -- Get a reference to the Editor UI
    -- editorUI = pixelVisionOS.editorUI

    -- newFileModal = NewFileModal:Init(editorUI)
    -- newFileModal.editorUI = editorUI

    -- -- Find all the editors
    -- editorMapping = pixelVisionOS:FindEditors()

    -- -- workspaceTool = WorkspaceTool:Init()


    -- local aboutText = "The ".. toolName.. " offers you access to the underlying file system. "

    -- if(TmpPath() ~= nil) then
    --     aboutText = aboutText .. "\n\nTemporary files are stores on your computer at: \n\n" .. TmpPath()
    -- end

    -- if(DocumentPath() ~= nil) then

    --     aboutText = aboutText .. "\n\nYou can access the 'Workspace' drive on your computer at: \n\n" .. DocumentPath()

    -- end

    -- -- TODO need to see if the log file actually exists
    -- local logExits = true

    -- local menuOptions = 
    -- {
    --     -- About ID 1
    --     {name = "About", action = function() pixelVisionOS:ShowAboutModal(toolName, aboutText, 220) end, toolTip = "Learn about PV8."},
    --     -- Settings ID 2
    --     {name = "Settings", action = OnLaunchSettings, toolTip = "Configure Pixel Vision OS's Settings."},
    --     -- Settings ID 3
    --     {name = "View Log", enabled = logExits, action = OnLaunchLog, toolTip = "Open up the log file."},
    --     {divider = true},

    --     -- New Folder ID 5
    --     {name = "New Folder", action = OnNewFolder, key = Keys.N, enabled = false, toolTip = "Create a new file."},

    --     {divider = true},

    --     -- Edit ID 7
    --     -- {name = "Edit", key = Keys.E, action = OnEdit, enabled = false, toolTip = "Edit the selected file."},
    --     -- Edit ID 8
    --     {name = "Rename", action = OnTriggerRename, enabled = false, toolTip = "Rename the currently selected file."},
    --     -- Copy ID 9
    --     {name = "Copy", key = Keys.C, action = OnCopy, enabled = false, toolTip = "Copy the selected file."},
    --     -- Paste ID 10
    --     {name = "Paste", key = Keys.V, action = OnPaste, enabled = false, toolTip = "Paste the selected file."},
    --     -- Delete ID 11
    --     {name = "Delete", key = Keys.D, action = OnDeleteFile, enabled = false, toolTip = "Delete the current file."},
    --     {divider = true},

    --     -- Empty Trash ID 16
    --     {name = "Empty Trash", action = OnEmptyTrash, enabled = false, toolTip = "Delete everything in the trash."},
    --     -- Eject ID 17
    --     {name = "Eject Disk", action = OnEjectDisk, enabled = false, toolTip = "Eject the currently selected disk."},
    --     -- Shutdown ID 18
    --     {name = "Shutdown", action = OnShutdown, toolTip = "Shutdown PV8."} -- Quit the current game
    -- }

    -- local addAt = 6

    -- if(PathExists(fileTemplatePath) == true) then

    --     table.insert(menuOptions, addAt, {name = "New Project", key = Keys.P, action = OnNewGame, enabled = false, toolTip = "Create a new file."})

    --     NewGameShortcut = "New Project"

    --     addAt = addAt + 1

    -- end

    -- newFileOptions = {}

    -- -- TODO this should be done better

    -- if(runnerName == DrawVersion or runnerName == TuneVersion) then

    --     table.insert(menuOptions, addAt, {name = "New Data", action = function() OnNewFile("data", "json", "data", false) end, enabled = false, toolTip = "Run the current game."})
    --     table.insert(newFileOptions, {name = "New Data", file = "data.json"})
    --     addAt = addAt + 1

    --     -- table.insert(menuOptions, addAt, {name = "New Info", action = function() OnNewFile("info", "json", "info", false) end, enabled = false, toolTip = "Run the current game."})
    --     -- table.insert(newFileOptions, {name = "New Info", file = "info.json"})
    --     -- addAt = addAt + 1
    -- end

    -- -- Add text options to the menu
    -- if(runnerName ~= PlayVersion and runnerName ~= DrawVersion and runnerName ~= TuneVersion) then

    --     table.insert(menuOptions, addAt, {name = "New Code", action = function() OnNewFile("code", "lua") end, enabled = false, toolTip = "Run the current game."})
    --     table.insert(newFileOptions, {name = "New Code"})
    --     addAt = addAt + 1

    --     table.insert(menuOptions, addAt, {name = "New JSON", action = function() OnNewFile("untitled", "json") end, enabled = false, toolTip = "Run the current game."})
    --     table.insert(newFileOptions, {name = "New JSON"})
    --     addAt = addAt + 1

    -- end

    -- -- Add draw options

    -- if(PathExists(fileTemplatePath.AppendFile("colors.png"))) then
    --     table.insert(menuOptions, addAt, {name = "New Colors", action = function() OnNewFile("colors", "png", "colors", false) end, enabled = false, toolTip = "Run the current game.", file = "colors.png"})
    --     table.insert(newFileOptions, {name = "New Colors", file = "colors.png"})
    --     addAt = addAt + 1
    -- end

    -- if(PathExists(fileTemplatePath.AppendFile("sprites.png"))) then

    --     table.insert(menuOptions, addAt, {name = "New Sprites", action = function() OnNewFile("sprites", "png", "sprites", false) end, enabled = false, toolTip = "Run the current game.", file = "sprites.png"})
    --     table.insert(newFileOptions, {name = "New Sprites", file = "sprites.png"})
    --     addAt = addAt + 1
    -- end

    -- if(PathExists(fileTemplatePath.AppendFile("large.font.png"))) then

    --     table.insert(menuOptions, addAt, {name = "New Font", action = function() OnNewFile("untitled", "font.png", "font") end, enabled = false, toolTip = "Run the current game."})
    --     table.insert(newFileOptions, {name = "New Font"})
    --     addAt = addAt + 1

    -- end

    -- if(PathExists(fileTemplatePath.AppendFile("tilemap.json"))) then

    --     table.insert(menuOptions, addAt, {name = "New Tilemap", action = function() OnNewFile("tilemap", "json", "tilemap", false) end, enabled = false, toolTip = "Run the current game.", file = "tilemap.json"})
    --     table.insert(newFileOptions, {name = "New Tilemap", file = "tilemap.json"})
    --     addAt = addAt + 1

    -- end

    -- -- Add music options

    -- if(PathExists(fileTemplatePath.AppendFile("sounds.json"))) then

    --     table.insert(menuOptions, addAt, {name = "New Sounds", action = function() OnNewFile("sounds", "json", "sounds", false) end, enabled = false, toolTip = "Run the current game.", file = "sounds.json"})
    --     table.insert(newFileOptions, {name = "New Sounds", file = "sounds.json"})
    --     addAt = addAt + 1
    -- end

    -- if(PathExists(fileTemplatePath.AppendFile("music.json"))) then

    --     table.insert(menuOptions, addAt, {name = "New Music", action = function() OnNewFile("music", "json", "music", false) end, enabled = false, toolTip = "Run the current game.", file = "music.json"})
    --     table.insert(newFileOptions, {name = "New Music", file = "music.json"})
    --     addAt = addAt + 1

    -- end

    -- if(runnerName ~= DrawVersion and runnerName ~= TuneVersion) then

    --     -- TODO need to add to the offset
    --     addAt = addAt + 6
    --     -- Empty Trash ID 13
    --     table.insert(menuOptions, addAt, {name = "Run", key = Keys.R, action = OnRun, enabled = false, toolTip = "Run the current game."})
    --     addAt = addAt + 1

    --     table.insert(menuOptions, addAt, {name = "Build", action = OnExportGame, enabled = false, toolTip = "Create a PV8 file from the current game."})
    --     addAt = addAt + 1

    --     table.insert(menuOptions, addAt, {divider = true})
    --     addAt = addAt + 1

    --     RunShortcut, BuildShortcut = "Run", "Build"

    -- end


    -- pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")

    -- -- Change the title
    -- pixelVisionOS:ChangeTitle(toolName, "toolbaricontool")

    -- RebuildDesktopIcons()



    -- desktopHitRect = NewRect(0, 12, 256, 229)

    -- local newPath = ReadSaveData("lastPath", "none")
    -- local lastScrollPos = tonumber(ReadSaveData("scrollPos", "0"))
    -- local lastSelection = tonumber(ReadSaveData("selection", "0"))
    -- local showUpgrade = "true"
    
    -- -- Read metadata last path
    -- local lastPath =  ReadMetadata("overrideLastPath", "none")

    -- if(lastPath ~= "none") then
    --     -- Clear last path from metadata
    --     WriteMetadata( "overrideLastPath", "none" )

    --     -- override the default path to open
    --     newPath = lastPath
    --     lastScrollPos = 0
    --     lastSelection = 0
    -- end

    -- if(SessionID() == ReadSaveData("sessionID", "")) then

    --     showUpgrade = ReadSaveData("showUpgrade", showUpgrade)

    --     -- TODO need to convert this to a path from the start and pass into Open window
    --     if(newPath ~= "none" and PathExists(NewWorkspacePath(newPath))) then
    --         OpenWindow(newPath, lastScrollPos, lastSelection)
    --     end

    -- end

    -- local installerPath = NewWorkspacePath("/PixelVisionOS/System/OSInstaller/")

    -- -- Check for the installer
    -- if(PathExists(installerPath) and showUpgrade == "true") then

    --     local versionFilePath = installerPath.AppendFile("pixel-vision-os-version.lua")

    --     -- Make sure there is a version file
    --     if(PathExists(versionFilePath)) then

    --         local text = ReadTextFile(versionFilePath.Path)

    --         local ver = text:sub(#text - 6, #text - 3)

    --         if(ver ~= pixelVisionOS.version) then

    --             pixelVisionOS:ShowMessageModal("Upgrade to " .. ver, "It looks like you are running an older version of Pixel Vision 8. If you hit cancel you will not see this again until you restart Pixel Vision 8. You can upgrade at any time by selecting \"Install OS\" from the settings tool menu.\n\nDo you want to upgrade to the latest version? ", 168, true,
    --                 function()
    --                     if(pixelVisionOS.messageModal.selectionValue == true) then

    --                         WriteSaveData("showUpgrade", "false")

    --                         LoadGame(installerPath.Path)

    --                     else
    --                         WriteSaveData("showUpgrade", "false")
    --                     end

    --                 end
    --             )

    --         end

    --     end

    -- end

    -- print("Test", PathExists(NewWorkspacePath("/Disks/PixelVisionOS/")))

end

-- local wallPaperThemes = {
--     {0, 5},
--     {5, 5}
-- }

-- function DrawWallpaper()

--     -- Set up logo values
--     local logoSpriteData = runningFromDisk == false and _G["logo"] or nil
--     local colorOffset = 0
--     local backgroundColor = runningFromDisk == false and tonumber(ReadBiosData("DefaultBackgroundColor", "5")) or 11 -- TODO Changed this to red so I can tell when I'm running off the disk or not easier

--     if(runnerName == DrawVersion) then
--         logoSpriteData = _G["logodraw"]
--         colorOffset = 5
--         -- backgroundColor = 1
--     elseif(runnerName == TuneVersion) then
--         logoSpriteData = _G["logotune"]
--         -- backgroundColor = 8
--     end

--     -- Update background
--     BackgroundColor(backgroundColor)

--     -- Draw logo
--     if(logoSpriteData ~= nil) then
--         DrawSprites(logoSpriteData.spriteIDs, 13, 13, logoSpriteData.width, false, false, DrawMode.Tile, colorOffset)
--     end

-- end

function OnNewFile(fileName, ext, type, editable)

    if(type == nil) then
        type = ext
    end

    newFileModal:SetText("New ".. type, fileName, "Name " .. type .. " file", editable == nil and true or false)

    pixelVisionOS:OpenModal(newFileModal,
        function()

            if(newFileModal.selectionValue == false) then
                return
            end

            local filePath = UniqueFilePath(currentDirectory.AppendFile(newFileModal.inputField.text .. "." .. ext))

            local tmpPath = fileTemplatePath.AppendFile(filePath.EntityName)

            -- Check for lua files first since we always want to make them empty
            if(ext == "lua") then
                SaveText(filePath, "-- Empty code file")

                -- Check for any files in the template folder we can copy over
            elseif(PathExists(tmpPath)) then

                CopyTo(tmpPath, filePath)
                -- -- print("Copy from template", tmpPath.Path)

                -- Create an empty text file
            elseif( ext == "txt") then
                SaveText(filePath, "")

                -- Create an empty json file
            elseif(ext == "json") then
                SaveText(filePath, "{}")
            elseif(type == "font") then

                tmpPath = fileTemplatePath.AppendFile("large.font.png")
                CopyTo(tmpPath, filePath)

            else
                -- print("File not supported")
                -- TODO need to display an error message that the file couldn't be created
                return
            end

            -- NewFile(filePath)
            RefreshWindow()

        end
    )

end

function OnTriggerRename(callback)

    local file = CurrentlySelectedFile()

    newFileModal:SetText("Rename File", file.name, "Name " .. file.type, true)

    pixelVisionOS:OpenModal(newFileModal,
        function()

            if(newFileModal.selectionValue == false) then
                return
            end

            OnRenameFile(newFileModal.inputField.text)

        end
    )

end

function OnRenameFile(text)

    -- Extra check to make sure the name is not empty
    if(text == "") then
        return
    end

    local file = CurrentlySelectedFile()

    -- Make sure the new name is not the same as the old name
    if(text ~= file.name) then

        -- Check to see if the file is an extension
        if(file.isDirectory == true) then

            -- Add a trailing slash to the extension
            text = text .. "/"

        else

            -- Remap the extension by looking for it in the extToTypeMap
            local tmpExt = extToTypeMap[file.type]

            -- If the type doesn't exist, use the default ext
            if(tmpExt == nil) then
                tmpExt = file.ext
            end

            -- Add the new ext to the file
            text = text .. tmpExt

        end

        local newPath = NewWorkspacePath(file.parentPath .. text)

        if(PathExists(newPath) == false) then
            MoveTo(NewWorkspacePath(file.path), newPath)
        else
            pixelVisionOS:ShowMessageModal("Rename File Error", "A file with the same name already exists.", 160, false)
        end

        RefreshWindow()
        -- end

    end

end


-- function RefreshWindow()
--     windowInvalidated = true

-- end

-- function OnEmptyTrash()

--     pixelVisionOS:ShowMessageModal("Empty Trash", "Are you sure you want to empty the trash? This can not be undone.", 160, true,
--         function()
--             if(pixelVisionOS.messageModal.selectionValue == true) then

--                 -- Get all the files in the trash
--                 filesToCopy = GetEntitiesRecursive(trashPath)

--                 StartFileOperation(trashPath, "delete")

--             end

--         end
--     )

-- end

-- function OnRun()

--     -- Only try to run if the directory is a game
--     if(currentDirectory == nil or pixelVisionOS:ValidateGameInDir(currentDirectory) == false) then
--         return
--     end

--     -- TODO this should also accept a workspace path?
--     LoadGame(currentDirectory.Path)

-- end

-- function OnCopy()

--     if(windowIconButtons ~= nil) then

--         -- Remove previous files to be copied
--         filesToCopy = {}
--         fileActionSrc = currentDirectory

--         -- TODO this needs to eventually support multiple selections

--         local file = CurrentlySelectedFile()

--         if(CanCopy(file)) then

--             local tmpPath = NewWorkspacePath(file.path)

--             -- Test if the path is a directory
--             if(tmpPath.IsDirectory) then

--                 -- Add all of the files that need to be copied to the list
--                 filesToCopy = GetEntitiesRecursive(tmpPath)

--             end

--             -- Make sure the selected directory is included
--             table.insert(filesToCopy, 1, NewWorkspacePath(file.path))

--             -- print("Copy File", file.name, file.path, #filesToCopy, dump(filesToCopy))

--             -- Enable the paste shortcut
--             pixelVisionOS:EnableMenuItemByName(PasteShortcut, true)

--             -- TODO eventually need to change the message to handle multiple files
--             pixelVisionOS:DisplayMessage(#filesToCopy .. " file" .. (#filesToCopy == 1 and " has" or "s have") .." been copied.", 2)

--         else

--             -- Display a message that the file can not be copied
--             pixelVisionOS:ShowMessageModal(toolName .. "Error", "'".. file.name .. "' can not be copied.", 160, false)

--             -- Make sure we can't activate paste
--             pixelVisionOS:EnableMenuItemByName(PasteShortcut, false)

--         end

--     end

-- end

function GetPathToFile(parent, file)

    local tmpPath = parent .. file.fullName

    if(file.isDirectory) then
        tmpPath = tmpPath .. "/"
    end

    return NewWorkspacePath(tmpPath)

end

-- function OnPaste(dest)

--     -- Get the destination directory
--     dest = dest or currentDirectory

--     local destPath = NewWorkspacePath(dest)

--     -- If there are no files to copy, exit out of this function
--     if(filesToCopy == nil) then
--         return
--     end

--     -- Perform the file action validation
--     StartFileOperation(destPath, "copy")

--     pixelVisionOS:DisplayMessage("Entit" .. (#filesToCopy > 1 and "ies have" or "y has") .. " has been pasted.", 2)

-- end

-- function StartFileOperation(destPath, action)
--     fileActionActiveTotal = #filesToCopy
--     fileActionDest = destPath
--     -- Clear the path filter (used to change base path if there is a duplicate)
--     fileActionPathFilter = nil



--     fileAction = action
--     fileActionActiveTime = 0
--     fileActionDelay = .02
--     fileActionCounter = 0
--     fileActionBasePath = destPath
--     fileCleanup = {}

--     if(action == "delete") then
--         invalidateTrashIcon = true
--         fileActionActive = true
--         return
--     end

--     -- Modify the destPath with the first item for testing
--     destPath = destPath.AppendPath(filesToCopy[1].Path:sub( #fileActionSrc.Path + 1))
--     fileActionBasePath = destPath

--     if(action == "throw out") then
--         fileActionPathFilter = UniqueFilePath(destPath)
--         invalidateTrashIcon = true
--     end

--     if(filesToCopy[1].IsChildOf(destPath)) then

--         pixelVisionOS:ShowMessageModal(
--             "Workspace Path Conflict",
--             "Can't perform a file action on a path that is the child of the destination path.",
--             128 + 16, false, function() CancelFileActions() end
--         )
--         return

--     elseif(PathExists(destPath) and fileActionPathFilter == nil) then

--         local duplicate = destPath.Path == filesToCopy[1].Path

--         -- Ask if the file first item should be duplicated
--         pixelVisionOS:ShowMessageModal(
--             "Workspace Path Conflict",
--             "Looks like there is an existing file with the same name in '".. destPath.Path .. "'. Do you want to " .. (duplicate and "duplicate" or "replace") .. " '"..destPath.EntityName.."'?",
--             200,
--             true,
--             function()

--                 -- Only perform the copy if the user selects OK from the modal
--                 if(pixelVisionOS.messageModal.selectionValue) then

--                     if(duplicate == true) then

--                         fileActionPathFilter = UniqueFilePath(destPath)

--                     else
--                         -- print("Delete", destPath)
--                         SafeDelete(destPath)
--                     end
--                     -- Start the file action process
--                     fileActionActive = true

--                 else
--                     CancelFileActions()
--                     RefreshWindow()
--                 end

--             end
--         )

--     else

--         pixelVisionOS:ShowMessageModal(
--             "Workspace ".. action .." Action",
--             "Do you want to ".. action .. " " .. fileActionActiveTotal .. " files?",
--             160,
--             true,
--             function()

--                 -- -- Only perform the copy if the user selects OK from the modal
--                 if(pixelVisionOS.messageModal.selectionValue) then

--                     -- Start the file action process
--                     fileActionActive = true

--                 else
--                     CancelFileActions()
--                     RefreshWindow()
--                 end

--             end
--         )

--     end

-- end

-- function CancelFileActions()

--     if(fileActionActive == true) then
--         OnFileActionComplete()

--         -- editorUI.mouseCursor:SetCursor(1, false)
--     end

-- end

-- function TriggerSingleFileAction(srcPath, destPath, action)

--     -- Copy the file to the new location, if a file with the same name exists it will be overwritten
--     if(action == "copy") then

--         -- Only copy files over since we create the directory in the previous step
--         if(destPath.isFile) then
--             -- print("CopyTo", srcPath, destPath)
--             CopyTo(srcPath, destPath)
--         end

--     elseif(action == "move" or action == "throw out") then

--         -- Need to keep track of directories that listed since we want to clean them up when they are empty at the end
--         if(srcPath.IsDirectory) then
--             -- print("Save file path", srcPath)
--             table.insert(fileCleanup, srcPath)
--         else
--             MoveTo(srcPath, destPath)
--             -- print("MoveTo", srcPath, destPath)
--         end

--     elseif(action == "delete") then
--         if(srcPath.IsDirectory) then
--             -- print("Save file path", srcPath)
--             table.insert(fileCleanup, srcPath)
--         else
--             Delete(srcPath)
--             -- print("MoveTo", srcPath, destPath)
--         end
--     else
--         -- nothing happened so exit before we refresh the window
--         return
--     end

--     -- Refresh the window
--     RefreshWindow()

-- end

-- function CanCopy(file)

--     return (file.name ~= "Run" and file.type ~= "updirectory")

-- end

function OnEjectDisk(diskName)

    if(diskName == nil) then
        local id = desktopIconButtons.currentSelection
        diskName = desktopIcons[id].name
    end

    pixelVisionOS:ShowMessageModal("Eject Disk", "Do you want to eject the '".. diskName.."'disk?", 160, true,
        function()

            -- Only perform the copy if the user selects OK from the modal
            if(pixelVisionOS.messageModal.selectionValue) then

                if(currentDirectory ~= nil) then

                    -- Close the window of the disk you are trying to eject
                    if(currentDirectory.GetDirectorySegments()[1] == "disk" and currentDirectory.GetDirectorySegments()[2] == diskName) then
                        CloseWindow()
                    end

                end

                EjectDisk(NewWorkspacePath("/Disks/" .. diskName .. "/"))

                ResetGame()

            end

        end
    )



end

-- function OnShutdown()

--     CancelFileActions()

--     pixelVisionOS:ShowMessageModal("Shutdown " .. runnerName, "Are you sure you want to shutdown "..runnerName.."?", 160, true,
--         function()
--             if(pixelVisionOS.messageModal.selectionValue == true) then

--                 ShutdownSystem()

--                 -- Save changes
--                 shuttingDown = true

--             end

--         end
--     )

-- end

-- function RebuildDesktopIcons()

--     -- print("RebuildDesktopIcons")

--     -- TODO clear desktop with background color
--     DrawRect(216, 16, 39, 216, BackgroundColor(), DrawMode.TilemapCache)

--     -- Place holder for the old selction
--     local oldOpen = -1

--     -- See if there are any desktop buttons
--     if(desktopIconButtons ~= nil) then

--         -- Find the total buttons
--         local total = #desktopIconButtons.buttons

--         -- Loop through all of the desktop buttons
--         for i = 1, total do

--             -- See if any of the desktop buttons are open before redrawing them
--             if(desktopIconButtons.buttons[i].open) then
--                 oldOpen = i
--             end
--         end

--     end

--     -- Build Desktop Icons
--     desktopIcons = {}

--     local wPath = NewWorkspacePath("/Workspace/")
--     if(PathExists(wPath)) then

--         table.insert(desktopIcons, {
--             name = "Workspace",
--             sprite = PathExists(wPath.AppendDirectory("System")) and "filedriveos" or "filedrive",
--             tooltip = "This is the 'Workspace' drive",
--             path = "/Workspace/",
--             type = "workspace",
--             dragDelay = -1
--         })
--     end

--     local disks = DiskPaths()

--     for i = 1, #disks do

--         local name = disks[i].EntityName
--         local path = disks[i].Path

--         table.insert(desktopIcons, {
--             name = name,
--             sprite = "diskempty",
--             tooltip = "Double click to open the '".. name .. "' disk.",
--             tooltipDrag = "You are dragging the '".. name .. "' disk.",
--             path = path,
--             type = "disk"
--         })
--     end


--     -- Draw desktop icons
--     local startY = 16

--     desktopIconButtons = editorUI:CreateIconGroup()
--     desktopIconButtons.onTrigger = OnDesktopIconClick
--     desktopIconButtons.onAction = OnDesktopIconSelected

--     for i = 1, #desktopIcons do

--         local item = desktopIcons[i]

--         local button = editorUI:NewIconGroupButton(desktopIconButtons, {x = 216 - 8, y = startY}, item.sprite, item.name, item.tooltip, bgColor)

--         button.iconName = item.name
--         button.iconType = item.type
--         button.iconPath = item.path

--         if(item.dragDelay ~= nil) then
--             button.dragDelay = item.dragDelay
--         end

--         button.toolTipDragging = item.tooltipDrag

--         button.onOverDropTarget = OnOverDropTarget

--         button.onDropTarget = FileDropAction

--         startY = startY + 32 + 8

--     end

--     -- See if the trash exists

--     if(PathExists(trashPath) == false) then
--         CreateDirectory(trashPath)
--     end

--     local trashFiles = GetDirectoryContents(trashPath)

--     table.insert(desktopIcons, {
--         name = "Trash",
--         sprite = #trashFiles > 0 and "filetrashfull" or "filetrashempty",
--         tooltip = "The trash folder",
--         path = trashPath.Path,
--         type = "throw out"
--     })

--     pixelVisionOS:EnableMenuItemByName(EmptyTrashShortcut, #trashFiles > 0)

--     local item = desktopIcons[#desktopIcons]

--     local trashButton = editorUI:NewIconGroupButton(desktopIconButtons, {x = 216 - 8, y = 200 - 2}, item.sprite, item.name, item.tooltip, bgColor)

--     trashButton.iconName = item.name
--     trashButton.iconType = item.type
--     trashButton.iconPath = item.path

--     -- Lock the trash from Dragging
--     trashButton.dragDelay = -1

--     trashButton.onOverDropTarget = OnOverDropTarget

--     trashButton.onDropTarget = function(src, dest)

--         -- -- print("OnDropTarget", "Trash Icon", src.name, dest.name)
--         if(src.iconType == "disk") then

--             OnEjectDisk(src.iconName)

--         else
--             OnDeleteFile(src.iconPath)
--             -- -- print("Move To", src.iconPath, dest.iconPath)
--         end

--     end

--     -- Restore old open value
--     if(oldOpen > - 1) then
--         editorUI:OpenIconButton(desktopIconButtons.buttons[oldOpen])
--     end

-- end

function FileDropAction(src, dest)

    -- if src and dest paths are the same, exit
    if(src == dest) then
        return
    end

    filesToCopy = {}

    fileActionSrc = currentDirectory

    -- TODO need to find the base path
    local srcPath = NewWorkspacePath(src.iconPath)
    if(srcPath.IsDirectory) then

        -- Add all of the files that need to be copied to the list
        filesToCopy = GetEntitiesRecursive(srcPath)

    end

    -- Make sure the selected directory is included
    table.insert(filesToCopy, 1, srcPath)


    local destPath = NewWorkspacePath(dest.iconPath)

    local action = "move"

    local srcSeg = srcPath.GetDirectorySegments()
    local destSeg = destPath.GetDirectorySegments()

    if(srcSeg[1] == "Tmp" and srcSeg[2] == "Trash") then
        -- print("Trash")
        action = "move"
    elseif(srcSeg[1] == "Disks" and destSeg[1] == "Disks") then
        if(srcSeg[2] ~= destSeg[2]) then
            action = "copy"
        end
    elseif(srcSeg[1] ~= destSeg[1]) then
        action = "copy"
    end

    -- print(action, dump(srcSeg), dump(destSeg))

    -- print("Drop Action", action, srcPath, destPath, srcSeg[1], srcSeg[2])

    -- Perform the file action
    StartFileOperation(destPath, action)

end

function OnDesktopIconSelected(value)

    -- TODO need to check if the disk can be ejected?

    if(playingWav) then
        StopWav()
        playingWav = false
    end

    UpdateContextMenu(DesktopIconFocus)

    -- Clear any window selections
    editorUI:ClearIconGroupSelections(windowIconButtons)

    currentSelectedFile = nil

end

local currentOpenIconButton = nil

function OnDesktopIconClick(value, doubleClick)



    -- Close the currently open button
    if(currentOpenIconButton ~= nil) then
        editorUI:CloseIconButton(currentOpenIconButton)
    end

    currentOpenIconButton = desktopIconButtons.buttons[value]
    editorUI:OpenIconButton(currentOpenIconButton)

    OpenWindow(desktopIcons[value].path)


end


-- function OnNewGame()

--     if(PathExists(fileTemplatePath) == false) then
--         pixelVisionOS:ShowMessageModal(toolName .. " Error", "There is no default template.", 160, false)
--         return
--     end

--     newFileModal:SetText("New Project", "NewProject", "Folder Name", true)

--     pixelVisionOS:OpenModal(newFileModal,
--         function()

--             if(newFileModal.selectionValue == false) then
--                 return
--             end

--             -- Create a new workspace path
--             local newPath = currentDirectory.AppendDirectory(newFileModal.inputField.text)

--             -- Copy the contents of the template path to the new unique path
--             CopyTo(fileTemplatePath, UniqueFilePath(newPath))

--             RefreshWindow()

--         end
--     )

-- end

-- function OnNewFolder(name)

--     if(currentDirectory == nil) then
--         return
--     end

--     if(name == nil) then
--         name = "Untitled"
--     end

--     -- Create a new unique workspace path for the folder
--     local newPath = UniqueFilePath(currentDirectory.AppendDirectory(name))

--     -- Set the new file modal to show the folder name
--     newFileModal:SetText("New Folder", newPath.EntityName, "Folder Name", true)

--     -- Open the new file modal before creating the folder
--     pixelVisionOS:OpenModal(newFileModal,
--         function()

--             if(newFileModal.selectionValue == false) then
--                 return
--             end



--             -- Create a new workspace path
--             local filePath = currentDirectory.AppendDirectory(newFileModal.inputField.text)

--             -- Make sure the path doesn't exist before trying to make a new directory
--             if(PathExists(filePath) == false) then

--                 -- This is a bit of a hack to get around an issue creating folders on disks.

--                 -- Test to see if we are creating a folder on a disk
--                 if(string.starts(filePath.Path, "/Disks/")) then

--                     -- Create a new path in the tmp directory
--                     local tmpPath = UniqueFilePath(NewWorkspacePath("/Tmp/"..newFileModal.inputField.text .. "/"))

--                     -- Create a new folder in the tmp directory
--                     CreateDirectory(tmpPath)

--                     -- Move the folder from tmp to the new location
--                     MoveTo(tmpPath, filePath)

--                 else
--                     -- Create a new directory
--                     CreateDirectory(filePath)

--                 end

--                 -- Refresh the window to show the new folder
--                 RefreshWindow()

--             end

--         end
--     )

-- end

-- function OnDeleteFile(path)

--     if(path == nil) then

--         if(currentSelectedFile == nil) then
--             return
--         end

--         path = currentSelectedFile.path
--     end

--     filesToCopy = {}

--     fileActionSrc = currentDirectory

--     -- TODO need to find the base path
--     local srcPath = NewWorkspacePath(path)
--     if(srcPath.IsDirectory) then

--         -- Add all of the files that need to be copied to the list
--         filesToCopy = GetEntitiesRecursive(srcPath)

--     end

--     -- Make sure the selected directory is included
--     table.insert(filesToCopy, 1, srcPath)


--     local destPath = trashPath

--     local action = "throw out"

--     -- print("Delete Action", action, srcPath, destPath)

--     -- Perform the file action

--     selection = nil

--     -- Always make sure anything going into the trash has a unique file name
--     StartFileOperation(destPath, action)

-- end

-- -- Helper utility to delete files by moving them to the trash
-- function DeleteFile(path)

--     -- Create the base trash path for the file
--     local newPath = trashPath

--     -- See if this is a directory or a file and add the entity name
--     if(path.IsDirectory) then
--         newPath = newPath.AppendDirectory(path.EntityName)
--     else
--         newPath = newPath.AppendFile(path.EntityName)
--     end

--     -- Make sure the path is unique
--     newPath = UniqueFilePath(newPath)

--     -- Move to the new trash path
--     MoveTo(path, newPath)

-- end

-- function OpenWindow(path, scrollTo, selection)



--     if(scrollTo == nil and windowScrollHistory[path] ~= nil) then
--         scrollTo = windowScrollHistory[path]
--     end

--     refreshTime = 0

--     -- Clear the previous file list
--     files = {}

--     -- TODO maybe this should be a valid path before being passed into open window?
--     -- save the current directory
--     currentDirectory = NewWorkspacePath(path)

--     -- Set a default scrollTo value if none is provided
--     scrollTo = scrollTo or 0
--     selection = selection or 0

--     -- Draw the window chrome
--     DrawSprites(windowchrome.spriteIDs, 8, 16, windowchrome.width, false, false, DrawMode.TilemapCache)

--     if(vSliderData == nil) then
--         -- Create the slider for the window
--         vSliderData = editorUI:CreateSlider({x = 192, y = 26, w = 16, h = 195}, "vsliderhandle", "This is a vertical slider")
--         vSliderData.onAction = OnValueChange
--     end

--     -- Reset the slider position
--     vSliderData.value = scrollTo

--     -- Create the close button
--     if(closeButton == nil) then
--         closeButton = editorUI:CreateButton({x = 192, y = 16}, "closewindow", "Close the window.")
--         closeButton.hitRect = {x = closeButton.rect.x + 2, y = closeButton.rect.y + 2, w = 10, h = 10}
--         closeButton.onAction = CloseWindow
--     end

--     -- Need to clear the previous button drop targets
--     if(windowIconButtons ~= nil) then
--         for i = 1, #windowIconButtons.buttons do
--             editorUI.collisionManager:RemoveDragTarget(windowIconButtons.buttons[i])
--             -- editorUI:ToggleGroupRemoveButton(windowIconButtons, i)
--         end
--         -- editorUI:ClearIconGroup(windowIconButtons)

--         editorUI:ClearFocus()
--     else
--         -- Create a icon button group for all of the files
--         windowIconButtons = editorUI:CreateIconGroup()
--         windowIconButtons.onTrigger = OnWindowIconClick

--         -- Make sure we disable any selection on the desktop when clicking inside of the window icon group
--         windowIconButtons.onAction = OnWindowIconSelect
--     end

--     -- DrawRect()

--     -- Reset the last start id
--     lastStartID = -1

--     -- Parse files

--     -- Get the list of files from the Lua Service
--     files = GetDirectoryContents(currentDirectory)

    

--     -- TODO need to see if the game can be run only if there is a code file

--     if(runnerName ~= DrawVersion and runnerName ~= TuneVersion) then

--         -- Check to see if this is a game directory
--         if(pixelVisionOS:ValidateGameInDir(currentDirectory, {"code.lua"}) and TrashOpen() == false) then

--             table.insert(
--                 files,
--                 1,
--                 {
--                     name = "Run",
--                     type = "run",
--                     ext = "run",
--                     path = currentDirectory.Path,
--                     isDirectory = false,
--                     selected = false
--                 }

--             )
--         end

--     end

--     local parentDirectory = currentDirectory.ParentPath.Path

--     -- Check to see if this is a root directory
--     if(parentDirectory ~= "/Disks/" and parentDirectory ~= "/Tmp/" and parentDirectory ~= "/") then

--         table.insert(
--             files,
--             1,
--             {
--                 name = "..",
--                 type = "updirectory",
--                 path = parentDirectory,
--                 isDirectory = true,
--                 selected = false
--             }

--         )
--     end

--     -- Save a count of the files after we add the special files to the list
--     fileCount = #files

--     -- Enable the scroll bar if needed
--     editorUI:Enable(vSliderData, #files > totalPerWindow)

--     OnValueChange(scrollTo)

--     currentSelectedFile = nil

--     -- Select file
--     if(selection > 0) then
--         editorUI:SelectIconButton(windowIconButtons, selection, true)
--     else
--         UpdateContextMenu(WindowFocus)
--     end

--     ChangeWindowTitle(currentDirectory.Path, "toolbaricontool")

-- end

-- function UpdateContextMenu(inFocus)

--     if(inFocus == WindowFocus) then

--         local canRun = pixelVisionOS:ValidateGameInDir(currentDirectory, {"code.lua"}) and not TrashOpen()

--         if(runnerName == DrawVersion or runnerName == TuneVersion) then
--             canRun = false
--         end

--         -- New File options
--         if(runnerName ~= PlayVersion) then
--             pixelVisionOS:EnableMenuItemByName(NewGameShortcut, not canRun and not TrashOpen())
--         end

--         pixelVisionOS:EnableMenuItemByName(NewFolderShortcut, not TrashOpen())
--         -- pixelVisionOS:EnableMenuItemByName(NewFileShortcut, not TrashOpen())
--         for i = 1, #newFileOptions do

--             local option = newFileOptions[i]
--             local enable = not TrashOpen()

--             if(enable == true) then

--                 if(option.file ~= nil) then

--                     enable = not PathExists(currentDirectory.AppendFile(option.file))

--                 end

--             end

--             pixelVisionOS:EnableMenuItemByName(option.name, enable)

--         end

--         -- File options
--         pixelVisionOS:EnableMenuItemByName(EditShortcut, false)

--         if(RunShortcut ~= nil) then
--             pixelVisionOS:EnableMenuItemByName(RunShortcut, canRun)
--         end

--         pixelVisionOS:EnableMenuItemByName(RenameShortcut, false)
--         pixelVisionOS:EnableMenuItemByName(CopyShortcut, false)
--         pixelVisionOS:EnableMenuItemByName(DeleteShortcut, false)

--         if(BuildShortcut ~= nil) then
--             pixelVisionOS:EnableMenuItemByName(BuildShortcut, canRun and string.starts(currentDirectory.Path, "/Disks/") == false)
--         end

--         pixelVisionOS:EnableMenuItemByName(EjectDiskShortcut, CanEject())

--         -- Special cases

--         -- Only active paste if there is something to paste
--         pixelVisionOS:EnableMenuItemByName(PasteShortcut, filesToCopy ~= nil and #filesToCopy > 0)

--     elseif(inFocus == DesktopIconFocus) then

--         -- New File options
--         if(runnerName ~= PlayVersion) then
--             pixelVisionOS:EnableMenuItemByName(NewGameShortcut, false)
--         end

--         pixelVisionOS:EnableMenuItemByName(NewFolderShortcut, false)
--         -- pixelVisionOS:EnableMenuItemByName(NewFileShortcut, false)
--         for i = 1, #newFileOptions do
--             pixelVisionOS:EnableMenuItemByName(newFileOptions[i].name, false)
--         end

--         -- File options
--         -- pixelVisionOS:EnableMenuItemByName(EditShortcut, false)
--         pixelVisionOS:EnableMenuItemByName(EditShortcut, false)

--         if(RunShortcut ~= nil) then
--             pixelVisionOS:EnableMenuItemByName(RunShortcut, false)
--         end

--         pixelVisionOS:EnableMenuItemByName(RenameShortcut, false)
--         pixelVisionOS:EnableMenuItemByName(CopyShortcut, false)
--         pixelVisionOS:EnableMenuItemByName(PasteShortcut, false)
--         pixelVisionOS:EnableMenuItemByName(DeleteShortcut, false)
--         if(BuildShortcut ~= nil) then
--             pixelVisionOS:EnableMenuItemByName(BuildShortcut, false)
--         end
--         -- Disk options
--         pixelVisionOS:EnableMenuItemByName(EjectDiskShortcut, CanEject())


--     elseif(inFocus == WindowIconFocus) then

--         local currentSelection = CurrentlySelectedFile()

--         local specialFile = currentSelection.name == ".." or currentSelection.name == "Run"

--         -- Check to see if currentDirectory is a game
--         local canRun = pixelVisionOS:ValidateGameInDir(currentDirectory, {"code.lua"}) and not TrashOpen()

--         if(runnerName == DrawVersion or runnerName == TuneVersion) then
--             canRun = false
--         end
--         if(BuildShortcut ~= nil) then
--             pixelVisionOS:EnableMenuItemByName(BuildShortcut, canRun and string.starts(currentDirectory.Path, "/Disks/") == false)
--         end

--         -- New File options
--         if(runnerName ~= PlayVersion) then
--             pixelVisionOS:EnableMenuItemByName(NewGameShortcut, not canRun and not TrashOpen())
--         end

--         pixelVisionOS:EnableMenuItemByName(NewFolderShortcut, not TrashOpen())

--         for i = 1, #newFileOptions do


--             local option = newFileOptions[i]
--             local enable = not TrashOpen()

--             if(enable == true) then

--                 if(option.file ~= nil) then

--                     enable = not PathExists(currentDirectory.AppendFile(option.file))

--                 end

--             end

--             pixelVisionOS:EnableMenuItemByName(option.name, enable)

--         end

--         pixelVisionOS:EnableMenuItemByName(EditShortcut, not TrashOpen() and not specialFile)

--         -- TODO Can't rename up directory?
--         pixelVisionOS:EnableMenuItemByName(RenameShortcut, not TrashOpen() and not specialFile)

--         if(RunShortcut ~= nil) then
--             pixelVisionOS:EnableMenuItemByName(RunShortcut, canRun)
--         end

--         pixelVisionOS:EnableMenuItemByName(CopyShortcut, not TrashOpen() and not specialFile)

--         -- TODO need to makes sure the file can be deleted
--         pixelVisionOS:EnableMenuItemByName(DeleteShortcut, not TrashOpen() and not specialFile)

--         -- Disk options
--         pixelVisionOS:EnableMenuItemByName(EjectDiskShortcut, false)

--     else

--         -- New File options
--         if(runnerName ~= PlayVersion) then
--             pixelVisionOS:EnableMenuItemByName(NewGameShortcut, false)
--         end

--         pixelVisionOS:EnableMenuItemByName(NewFolderShortcut, false)
--         -- pixelVisionOS:EnableMenuItemByName(NewFileShortcut, false)

--         for i = 1, #newFileOptions do
--             pixelVisionOS:EnableMenuItemByName(newFileOptions[i].name, false)
--         end

--         -- File options
--         -- pixelVisionOS:EnableMenuItemByName(EditShortcut, false)
--         pixelVisionOS:EnableMenuItemByName(EditShortcut, false)

--         if(RunShortcut ~= nil) then
--             pixelVisionOS:EnableMenuItemByName(RunShortcut, false)
--         end

--         pixelVisionOS:EnableMenuItemByName(RenameShortcut, false)
--         pixelVisionOS:EnableMenuItemByName(CopyShortcut, false)
--         pixelVisionOS:EnableMenuItemByName(PasteShortcut, false)
--         pixelVisionOS:EnableMenuItemByName(DeleteShortcut, false)

--         if(BuildShortcut ~= nil) then
--             pixelVisionOS:EnableMenuItemByName(BuildShortcut, false)
--         end
--         -- Disk options
--         pixelVisionOS:EnableMenuItemByName(EjectDiskShortcut, false)

--     end

-- end

-- function OnLaunchSettings()

--     local editorPath = ReadBiosData("SettingsEditor")

--     if(editorPath == nil) then
--         editorPath = rootPath .."SettingsTool/"
--     end

--     local success = LoadGame(editorPath)

-- end

-- function OnLaunchLog()

--     -- Get a list of all the editors
--     local editorMapping = pixelVisionOS:FindEditors()

--     -- Find the json editor
--     textEditorPath = editorMapping["txt"]

--     local metaData = {
--         directory = "/Tmp/",
--         file = "/Tmp/Log.txt"
--     }

--     LoadGame(textEditorPath, metaData)

-- end

-- -- This is a helper for changing the text on the title bar
-- function ChangeWindowTitle(pathTitle, titleIconName)

--     -- Clean up the path
--     if(pathTitle:sub(1, 7) == "/Disks/") then
--         pathTitle = pathTitle:sub(7, #pathTitle)
--     elseif(pathTitle:sub(1, 5) == "/Tmp/") then
--         pathTitle = pathTitle:sub(5, #pathTitle)
--     end

--     DrawRect(24, 16, 168, 8, 0, DrawMode.TilemapCache)

--     local maxChars = 43
--     if(#pathTitle > maxChars) then
--         pathTitle = pathTitle:sub(0, maxChars - 3) .. "..."
--     else
--         pathTitle = string.rpad(pathTitle, maxChars, "")
--     end

--     DrawText(pathTitle:upper(), 19, 17, DrawMode.TilemapCache, "medium", 15, - 4)

--     -- Look for desktop icon
--     -- TODO make sure the correct desktop item is highlighted

--     local pathSplit = string.split(pathTitle, "/")

--     local desktopIconName = pathSplit[1]

--     local iconID = -1

--     for i = 1, #desktopIcons do
--         if(desktopIcons[i].name == desktopIconName) then
--             iconID = i
--             break
--         end
--     end

--     -- Try to find the icon button if we open a window and its not selected beforehand
--     if(currentOpenIconButton == nil and iconID > 0) then
--         currentOpenIconButton = desktopIconButtons.buttons[iconID]

--         editorUI:OpenIconButton(currentOpenIconButton)
--     end
-- end

-- function CloseWindow()

--     -- Clear the previous scroll history
--     windowScrollHistory = {}

--     closeButton = nil

--     vSliderData = nil

--     windowIconButtons = nil

--     currentSelectedFile = nil

--     currentDirectory = nil

--     DrawRect(8, 16, windowchrome.width * 8, math.floor(#windowchrome.spriteIDs / windowchrome.width) * 8, BackgroundColor(), DrawMode.TilemapCache)

--     DrawWallpaper()

--     editorUI:ClearGroupSelections(desktopIconButtons)

--     if(currentOpenIconButton ~= nil) then
--         editorUI:CloseIconButton(currentOpenIconButton)
--     end

--     editorUI:ClearFocus()

--     UpdateContextMenu(NoFocus)

-- end

-- function OnWindowIconSelect(id)

--     if(playingWav) then
--         StopWav()
--         playingWav = false
--     end

--     if(currentSelectedFile ~= nil) then
--         currentSelectedFile.selected = false
--         currentSelectedFile.open = false

--         -- TODO this is not optimized, force old selected button to reset
--         for i = 1, #windowIconButtons.buttons do
--             local btn = windowIconButtons.buttons[i]

--             if(btn.iconPath == currentSelectedFile.path) then

--                 btn.selected = false
--                 editorUI:Invalidate(btn)
--             end
--         end

--         editorUI:RedrawIconButton(currentOpenIconButton)

--         -- TODO clearing this doesn't always redraw the button
--         -- currentSelectedFile.invalid = true
--     end

--     local index = id + (lastStartID)-- TODO need to add the scrolling offset

--     local tmpItem = files[index]

--     -- Select file
--     tmpItem.selected = true

--     local type = tmpItem.type
--     local path = tmpItem.path

--     -- TODO need a way to clear any stuck icons that are selected

--     -- Set new selected file
--     currentSelectedFile = tmpItem

--     -- Clear desktop selection
--     editorUI:ClearIconGroupSelections(desktopIconButtons)

--     UpdateContextMenu(WindowIconFocus)

-- end

-- function TrashOpen()

--     return currentDirectory.Path == trashPath.Path

-- end

-- function CanEject()

--     local value = false

--     local id = desktopIconButtons.currentSelection

--     if(id > 0) then

--         local selection = desktopIcons[id]

--         value = selection.name ~= "Workspace" and selection.name ~= "Trash"

--     end

--     return value

-- end

-- function CurrentlySelectedFile()

--     -- TODO need to return an array to support multiple selections

--     local file = nil
--     for i = 1, #files do
--         file = files[i]
--         if(file.selected) then
--             return file
--         end
--     end
--     --
--     -- local index = windowIconButtons.currentSelection + lastStartID
--     --
--     -- local tmpItem = files[index]
--     --
--     -- return tmpItem

-- end

-- function OnWindowIconClick(id)

--     -- Make sure desktop icons are not selected
--     editorUI:ClearGroupSelections(desktopIconButtons)

--     -- local index = id + (lastStartID)-- TODO need to add the scrolling offset

--     local tmpItem = files[id + lastStartID]--CurrentlySelectedFile()-- files[index]

--     local type = tmpItem.type
--     local path = tmpItem.path


--     -- TODO need a list of things we can't delete

--     -- Enable delete option

--     -- -- print("Window Icon Click", tmpItem.name)
--     local type = tmpItem.type

--     -- If the type is a folder, open it
--     if(type == "folder" or type == "updirectory") then

--         windowScrollHistory[currentDirectory.Path] = vSliderData.value

--         OpenWindow(tmpItem.path)

--         -- Check to see if the file is in the trash
--     elseif(TrashOpen()) then

--         -- Show warning message about trying to edit files in the trash
--         pixelVisionOS:ShowMessageModal(toolName .. " Error", "You are not able to edit files inside of the trash.", 160, false
--         )

--         -- Check to see if the file is an executable
--     elseif(type == "run") then


--         LoadGame(path)



--     elseif(type == "pv8") then

--         -- TODO need to see if there is space to mount another disk
--         -- TODO need to know if this disk is being mounted as read only
--         -- TODO don't run
--         pixelVisionOS:ShowMessageModal("Run Disk", "Do you want to mount this disk?", 160, true,
--             function()

--                 -- Only perform the copy if the user selects OK from the modal
--                 if(pixelVisionOS.messageModal.selectionValue) then

--                     MountDisk(NewWorkspacePath(path))

--                     -- TODO need to load the game in read only mode
--                     -- LoadGame(path)

--                 end

--             end
--         )

--     elseif(type == "wav") then

--         PlayWav(NewWorkspacePath(path))

--         playingWav = true

--         -- Check to see if there is an editor for the type or if the type is unknown
--     elseif(editorMapping[type] == nil or type == "unknown") then

--         pixelVisionOS:ShowMessageModal(toolName .. " Error", "There is no tool installed to edit this file.", 160, false
--         )

--         -- Now we are ready to try to edit a file
--     else

--         if(type == "installer") then

--             if(PathExists(NewWorkspacePath("/Workspace/")) == false) then

--                 pixelVisionOS:ShowMessageModal("Installer Error", "You need to create a 'Workspace' drive before you can run an install script.", 160, false)

--                 return

--                 -- TODO this could be optimized by using the path segments?
--             elseif(string.starts(currentDirectory.Path, "/Disks/") == false) then

--                 -- TODO need to see if there is space to mount another disk
--                 -- TODO need to know if this disk is being mounted as read only
--                 -- TODO don't run
--                 pixelVisionOS:ShowMessageModal("Installer Error", "Installers can only be run from a disk.", 160, false)

--                 return

--             end
--         end

--         -- When trying to load a tilemap.png file, check if there is a json file first
--         if(type == "tiles" and PathExists(currentDirectory.AppendFile("tilemap.json"))) then
--             -- Change the type to PNG so the image editor is used instead of the tilemap editor
--             type = "png"
--         end

--         -- Find the correct editor from the list
--         local editorPath = editorMapping[type]

--         -- Set up the meta data for the editor
--         local metaData = {
--             directory = currentDirectory.Path,
--             file = tmpItem.path,
--             filePath = tmpItem.path, -- TODO this should be the root path
--             fileName = tmpItem.fullName,
--             -- introMessage = "Editing '" .. tmpItem.fullName .."'."
--         }

--         -- Check to see if the path to the editor exists
--         if(PathExists(NewWorkspacePath(editorPath))) then

--             -- Load the tool
--             LoadGame(editorPath, metaData)

--         end

--         -- TODO find an editor for the file's extension
--     end


-- end

-- function OnMenuQuit()

--     QuitCurrentTool()

-- end

-- function OnValueChange(value)

--     local totalPerRow = 3
--     local totalPerPage = 12

--     local totalFiles = #files

--     local totalRows = math.ceil(totalFiles / totalPerRow) + 1

--     local hiddenRows = totalRows - math.ceil(totalPerPage / totalPerRow)

--     local offset = Clamp(hiddenRows * value, 0, hiddenRows - 1)

--     DrawWindow(files, offset * totalPerRow, totalPerPage)

-- end

-- function DrawWindow(files, startID, total)

--     if(startID < 0) then
--         startID = 0
--     end
--     -- -- print("DrawWindow", startID)

--     if(lastStartID == startID) then
--         return
--     end

--     -- TODO the icon buttons should have their own clear graphic
--     -- DrawRect(10, 28, 180, 192, 11, DrawMode.TilemapCache)

--     editorUI:ClearIconGroup(windowIconButtons)

--     lastStartID = startID

--     local startX = 13
--     local startY = 32
--     local row = 0
--     local maxColumns = 3
--     local padding = 16
--     local width = 48
--     local height = 40
--     local bgColor = 11

--     local requiredFiles = {"data.json"}

--     if(runnerName ~= DrawVersion and runnerName ~= TuneVersion) then
--         table.insert(requiredFiles, "info.json")
--     end

--     -- TODO make sure the trash path check is valid
--     local isGameDir = pixelVisionOS:ValidateGameInDir(currentDirectory, requiredFiles) and TrashOpen() == false

--     -- local tmpPath = NewWorkspacePath(item.path)
--     local pathParts = currentDirectory.GetDirectorySegments()
--     local systemRoot = ((pathParts[1] == "Workspace" and #pathParts == 1) or (pathParts[1] == "Disks" and #pathParts == 2))

--     -- print("parts", #pathParts, dump(pathParts), systemRoot)

--     for i = 1, total do

--         -- Calculate the real index
--         local fileID = i + startID


--         local index = i - 1

--         -- Update column value
--         local column = index % maxColumns

--         local newX = index % maxColumns * (width + padding) + startX
--         local newY = row * (height + padding / 2) + startY

--         -- Update the row for the next loop
--         if (column == (maxColumns - 1)) then
--             row = row + 1
--         end

--         if(fileID <= #files) then

--             local item = files[fileID]

--             -- Find the right type for the file
--             UpdateFileType(item, isGameDir)

--             local spriteName = GetIconSpriteName(item)

--             if(spriteName == fileTypeMap["folder"] and systemRoot == true) then

--                 -- TODO need another check for libs and tools

--                 if(item.name == "System" or item.name == "Libs" or item.name == "Tools") then

--                     -- TODO should we check to make sure the folder isn't empty?

--                     local correctParent = currentDirectory.EntityName == "System"

--                     if(item.name == "System") then
--                         spriteName = "fileosfolder"
--                     elseif(correctParent and correctParent) then
--                         spriteName = "fileosfolder"
--                     end
--                 end
--             end

--             local toolTip = "Double click to "

--             if(item.name == "Run") then
--                 toolTip = toolTip .. "run this game."
--             elseif(item.name == "..") then

--                 toolTip = toolTip .. "go to the parent folder."

--             elseif(item.isDirectory == true) then

--                 toolTip = toolTip .. "open the " .. item.name .. " folder."
--             else
--                 toolTip = toolTip .. "edit " .. item.fullName .. "."

--             end

--             local button = editorUI:NewIconGroupButton(windowIconButtons, {x = newX, y = newY}, spriteName, item.name, toolTip, bgColor)

--             button.iconName = item.name
--             button.iconType = item.type
--             button.iconPath = item.path

--             -- TODO this is keeping the updir and run from selecting
--             button.selected = item.selected

--             -- Disable the drag on files that don't exist in the directory
--             if(item.type == "updirectory" or item.type == "folder") then

--                 -- updirectory and folder share the same code but we don't want to drag updirectory
--                 if(item.type == "updirectory") then
--                     button.dragDelay = -1
--                 end

--                 -- button.onPress = function()
--                 --   -- print("Starting Drag")
--                 -- end

--                 button.onOverDropTarget = OnOverDropTarget

--                 -- Add on drop target code to each folder type
--                 button.onDropTarget = FileDropAction


--             elseif(item.type == "run" or item.type == "unknown" or item.type == "installer") then

--                 editorUI.collisionManager:DisableDragging(button)
--                 button.onDropTarget = nil

--             end

--         else

--             editorUI:NewDraw("DrawRect", {newX, newY, 48, 40, bgColor, DrawMode.TilemapCache})

--         end



--     end


-- end

-- function OnOverDropTarget(src, dest)

--     if(src.iconPath ~= dest.iconPath) then

--         editorUI:HighlightIconButton(dest, true)

--     end

-- end


-- function UpdateFileType(item, isGameFile)

--     local key = item.type--item.isDirectory and item.type or item.ext

--     key = item.type

--     -- TODO support legacy files
--     if(key == "png" and isGameFile == true) then
--         -- -- print("Is PNG")
--         if(item.name == "sprites" and editorMapping["sprites"] ~= nil) then
--             key = "sprites"
--         elseif(item.name == "tilemap" and editorMapping["tilemap"] ~= nil) then
--             key = "tiles"
--         elseif(item.name == "colors" and editorMapping["colors"] ~= nil) then
--             key = "colors"
--         end
--     elseif(key == "font.png") then

--         if(isGameFile == false or editorMapping["font"] == nil) then
--             key = "png"
--         else
--             key = "font"
--         end

--     elseif(key == "json" and isGameFile == true) then

--         if(item.name == "sounds" and editorMapping["sounds"] ~= nil)then
--             key = "sounds"
--         elseif(item.name == "tilemap" and editorMapping["tilemap"] ~= nil) then
--             key = "tilemap"
--         elseif(item.name == "music" and editorMapping["music"] ~= nil) then
--             key = "music"
--         elseif(item.name == "data" and editorMapping["system"] ~= nil) then
--             key = "system"
--         elseif(item.name == "info") then
--             key = "info"
--         end

--     end

--     if(key == "wav") then
--         item.ext = "wav"
--     end

--     -- Fix type for pv8 and runner templates
--     if(item.type == "pv8" or item.type == "pvr") then
--         key = item.type
--     end

--     -- Last chance to fix any special edge cases like the installer and info which share text file extensions
--     if(key == "txt" and item.name:lower() == "installer") then
--         key = "installer"
--     end

--     item.type = key

-- end

-- function GetIconSpriteName(item)

--     local iconName = fileTypeMap[item.type]
--     -- -- print("name", name, iconName)
--     return iconName == nil and "fileunknown" or fileTypeMap[item.type]

-- end

local filePos = NewPoint()
local currentSelectionID = 0

-- The Update() method is part of the game's life cycle. The engine calls Update() on every frame
-- before the Draw() method. It accepts one argument, timeDelta, which is the difference in
-- milliseconds since the last frame.
function Update(timeDelta)

    
    -- Convert timeDelta to a float
    timeDelta = timeDelta / 1000

    if(workspaceTool.shuttingDown == true) then
        return
    end

    -- Update the workspace tool
    workspaceTool:Update(timeDelta)


    -- OLD CODE

    -- if(shuttingDown == true) then
    --     return
    -- end

    -- -- Convert timeDelta to a float
    -- timeDelta = timeDelta / 1000

    -- -- This needs to be the first call to make sure all of the OS and editor UI is updated first
    -- pixelVisionOS:Update(timeDelta)

    -- -- Only update the tool's UI when the modal isn't active
    -- if(pixelVisionOS:IsModalActive() == false) then

    --     if(currentDirectory ~= nil) then

    --         -- Check for file system changes
    --         refreshTime = refreshTime + timeDelta

    --         if(refreshTime > refreshDelay) then

    --             -- TODO This should use a workspace path
    --             tmpFiles = GetDirectoryContents(currentDirectory)

    --             if(#tmpFiles > fileCount) then
    --                 RefreshWindow()
    --             end

    --             refreshTime = 0

    --         end

    --         if(windowInvalidated == true) then
    --             -- TODO this should use a workspace path
    --             OpenWindow(currentDirectory.Path, scrollTo, selection)
    --             windowInvalidated = false
    --         end

    --         -- Create a new piont to see if we need to change the sprite position
    --         local newPos = NewPoint(0, 0)

    --         -- Offset the new position by the direction button
    --         if(Key(Keys.Up, InputState.Released)) then
    --             newPos.y = -1
    --         elseif(Key(Keys.Right, InputState.Released)) then
    --             newPos.x = 1
    --         elseif(Key(Keys.Down, InputState.Released)) then
    --             newPos.y = 1
    --         elseif(Key(Keys.Left, InputState.Released)) then
    --             newPos.x = -1
    --         end

    --         -- Test to see if the new position has changed
    --         if(newPos.x ~= 0 or newPos.y ~= 0) then

    --             -- local currentSelection = CurrentlySelectedFile()--spritePickerData.picker.enabled == true and spritePickerData.currentSelection or flagPicker.selected

    --             -- local debug = dump(currentSelection)

    --             local columns = 3
    --             local rows = math.ceil(fileCount / columns)

    --             local curPos = CalculatePosition(currentSelectionID, columns)

    --             newPos.x = Clamp(curPos.x + newPos.x, 0, columns - 1)
    --             newPos.y = Clamp(curPos.y + newPos.y, 0, rows - 1)

    --             currentSelectionID = CalculateIndex(newPos.x, newPos.y, columns)

    --             print("pos", currentSelectionID, Clamp(CalculateIndex(newPos.x, newPos.y, columns) + 1, 1, fileCount), curPos, newPos)

    --             -- editorUI:SelectIconButton(windowIconButtons, currentSelectionID, true)
    --             -- OnWindowIconSelect(currentSelectionID)

    --             -- if(spritePickerData.picker.enabled == true) then
    --             --     ChangeSpriteID(newIndex)
    --             -- else
    --             --     editorUI:SelectPicker(flagPicker, newIndex)
    --             --     -- print("Select flag", newIndex)
    --             -- end

    --         end

    --     end

    --     editorUI:UpdateIconGroup(desktopIconButtons)
    --     editorUI:UpdateIconGroup(windowIconButtons)

    --     editorUI:UpdateButton(closeButton)

    --     editorUI:UpdateSlider(vSliderData)

    --     if(editorUI.collisionManager.mouseDown and desktopHitRect:Contains(editorUI.collisionManager.mousePos.x, editorUI.collisionManager.mousePos.y) and editorUI.cursorID == 1) then
    --         if(windowIconButtons ~= nil and windowIconButtons.currentSelection > 0) then
    --             editorUI:ClearIconGroupSelections(windowIconButtons)
    --         elseif(desktopIconButtons.currentSelection > 0) then
    --             editorUI:ClearGroupSelections(desktopIconButtons)
    --         end

    --     end

    -- end

    -- if(fileActionActive == true) then

    --     fileActionActiveTime = fileActionActiveTime + timeDelta

    --     if(fileActionActiveTime > fileActionDelay) then
    --         fileActionActiveTime = 0

    --         OnFileActionNextStep()

    --         if(fileActionCounter >= fileActionActiveTotal) then

    --             OnFileActionComplete()

    --         end

    --     end


    -- end

    -- if(buildingDisk) then

    --     local total = ReadExportPercent()

    --     print("total", total)
    --     if(total >=100) then
            
    --         buildingDisk = false
            
    --         pixelVisionOS:CloseModal()
            
    --         local response = ReadExportMessage()
    --         local success = response.DiskExporter_success
    --         local message = response.DiskExporter_message
    --         local path = response.DiskExporter_path

    --         -- print("Disk Message", dump(response), success, message, path)

    --         progressModal = nil

    --         pixelVisionOS:ShowMessageModal("Build " .. (success == true and "Complete" or "Failed"), message, 160, false,

    --             function()
    --                 if(success == true) then
    --                     OpenWindow(NewWorkspacePath(path).ParentPath.path)
    --                 end
    --             end
    --         )
    --     else
    --         if(progressModal ~= nil) then

    --             local message = "Building new disk.\n\n\nDo not restart or shut down Pixel Vision 8."

    --             progressModal:UpdateMessage(message, total/100)
    --         end
    --     end

        
    -- end

end


-- The Draw() method is part of the game's life cycle. It is called after Update() and is where
-- all of our draw calls should go. We'll be using this to render sprites to the display.
function Draw()

    -- We can use the RedrawDisplay() method to clear the screen and redraw the tilemap in a
    -- single call.
    RedrawDisplay()

    -- OLD Code

    if(workspaceTool.shuttingDown == true) then

        local runnerName = SystemName()

        if(workspaceTool.shutdownScreen ~= true) then

            BackgroundColor(0)

            DrawRect(0, 0, 256, 480, 0, DrawMode.TilemapCache)

            local startX = math.floor((32 - #runnerName) * .5)
            DrawText(runnerName:upper(), startX, 10, DrawMode.Tile, "large", 15)
            DrawText("IS READY FOR SHUTDOWN.", 5, 11, DrawMode.Tile, "large", 15)

            workspaceTool.shutdownScreen = true
        end

        return
    end

    workspaceTool:Draw()

    -- -- The UI should be the last thing to draw after your own custom draw calls
    -- pixelVisionOS:Draw()

end

function Shutdown()

    workspaceTool:Shutdown()

--     -- Save the current session ID
--     WriteSaveData("sessionID", SessionID())

--     -- Make sure we don't save paths in the tmp directory
--     WriteSaveData("lastPath", currentDirectory ~= nil and currentDirectory.Path or "none")
--     --
--     -- Save the current session ID
--     WriteSaveData("scrollPos", (vSliderData ~= nil and vSliderData.value or 0))

--     -- Save the current selection
--     WriteSaveData("selection", (windowIconButtons ~= nil and editorUI:ToggleGroupSelections(windowIconButtons)[1] or 0))

end

-- function OnExportGame()

--         local srcPath = currentDirectory
--         local destPath = srcPath.AppendDirectory("Builds")
--         local infoFile = srcPath.AppendFile("info.json")
--         local dataFile = srcPath.AppendFile("data.json")

--         -- TODO need to read game name from info file
--         if(PathExists(infoFile) == false) then
--             SaveText(infoFile, "{\"name\":\""..srcPath.EntityName.."\"}")
--         end

--         local metaData = ReadJson(infoFile)

--         local gameName = (metaData ~= nil and metaData["name"] ~= nil) and metaData["name"] or srcPath.EntityName


--         local systemData = ReadJson(dataFile)

--         local maxSize = 512

--         if(systemData["GameChip"]) then

--             if(systemData["GameChip"]["maxSize"]) then
--                 maxSize = systemData["GameChip"]["maxSize"]
--             end
--         end

--         -- Manually create a game disk from the current folder's files
--         local srcFiles = GetEntities(srcPath)
--         local pathOffset = #srcPath.Path

--         local gameFiles = {}
        
--         for i = 1, #srcFiles do
--             local srcFile = srcFiles[i]
--             local destFile = NewWorkspacePath(srcFile.Path:sub(pathOffset))
--             gameFiles[srcFile] = destFile
--         end

--         -- Add shared library files

--         -- Get all of the shared library paths
--         local libPath = SharedLibPaths()

--         -- Load libs and split
--         local includedLibs = string.split((metaData["includeLibs"] or ""), ",")

--         local test = dump(libPath)

--         for i = 1, #libPath do
            
--             local tmpFiles = GetEntities(libPath[i])

--             for i = 1, #tmpFiles do
                
--                 local srcFile = tmpFiles[i]

--                 if(srcFile.IsFile and srcFile.GetExtension() == ".lua") then

--                         if(gameFiles[srcFile] ==nil and table.indexOf(includedLibs, srcFile.EntityNameWithoutExtension) > - 1) then
--                         local destFile = NewWorkspacePath("/" .. srcFile.EntityName)
--                         gameFiles[srcFile] = destFile
--                     end
                    
--                 end

--             end

--         end

--         local response = CreateDisk(gameName, gameFiles, destPath, maxSize)

--         local debugResponse = dump(response)

--         buildingDisk = true

--         if(progressModal == nil) then
--             --
--             --   -- Create the model
--             progressModal = ProgressModal:Init("File Action ", editorUI)
    
--             -- Open the modal
--             pixelVisionOS:OpenModal(progressModal)
    
--         end

-- end

-- function OnFileActionNextStep()

--     if(#filesToCopy == 0) then
--         return
--     end

--     -- Increment the counter
--     fileActionCounter = fileActionCounter + 1

--     -- Test to see if the counter is equil to the total
--     if(fileActionCounter > fileActionActiveTotal) then

--         fileActionDelay = 4
--         return
--     end

--     local srcPath = filesToCopy[fileActionCounter]

--     -- -- Look to see if the modal exists
--     if(progressModal == nil) then
--         --
--         --   -- Create the model
--         progressModal = ProgressModal:Init("File Action ", editorUI)

--         -- Open the modal
--         pixelVisionOS:OpenModal(progressModal)

--     end

--     local message = fileAction .. " "..string.lpad(tostring(fileActionCounter), string.len(tostring(fileActionActiveTotal)), "0") .. " of " .. fileActionActiveTotal .. ".\n\n\nDo not restart or shut down Pixel Vision 8."

--     local percent = (fileActionCounter / fileActionActiveTotal)

--     progressModal:UpdateMessage(message, percent)

--     local destPath = fileAction == "delete" and fileActionDest or NewWorkspacePath(fileActionDest.Path .. srcPath.Path:sub( #fileActionSrc.Path + 1))

--     if(fileActionPathFilter ~= nil) then

--         destPath = NewWorkspacePath(fileActionPathFilter.Path .. destPath.Path:sub( #fileActionBasePath.Path + 1))

--     end

--     -- Find the path to the directory being copied
--     local dirPath = destPath.IsFile and destPath.ParentPath or destPath

--     -- Make sure the directory exists
--     if(PathExists(dirPath) == false) then

--         CreateDirectory(dirPath)
--     end

--     if(srcPath.IsFile) then

--         TriggerSingleFileAction(srcPath, destPath, fileAction)
--     elseif(fileAction ~= "copy") then

--         table.insert(fileCleanup, srcPath)
--     end

-- end

-- function OnFileActionComplete()

--     -- TODO perform any cleanup after moving
--     local totalCleanup = #fileCleanup
--     for i = 1, totalCleanup do
--         local path = fileCleanup[i]
--         if(PathExists(path)) then
--             Delete(path)
--         end
--     end


--     -- Turn off the file action loop
--     fileActionActive = false

--     -- Close the modal
--     pixelVisionOS:CloseModal()

--     -- Destroy the progress modal
--     progressModal = nil

--     -- Clear files to copy list
--     filesToCopy = nil

--     RefreshWindow()

--     if(invalidateTrashIcon == true) then
--         RebuildDesktopIcons()
--         invalidateTrashIcon = false
--     end

-- end

-- function SafeDelete(srcPath)

--     Delete(srcPath)--, trashPath)

-- end