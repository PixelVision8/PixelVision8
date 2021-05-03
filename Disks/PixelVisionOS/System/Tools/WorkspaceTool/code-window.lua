FileTypeMap =
{
    folder = "filefolder",
    updirectory = "fileparentfolder",
    lua = "filecodelua",
    json = "filejson",
    png = "filepng",
    run = "filerun", -- TODO need to change this to run
    txt = "filetext",
    installer = "fileinstaller", -- TODO need a custom icon
    info = "fileinfo",
    pv8 = "diskempty",
    pvr = "disksystem",
    wav = "filewav",
    cs = "filecodecsharp",

    -- TODO these are not core file types
    unknown = "fileunknown",
    colors = "filecolor",
    system = "filesettings",
    font = "filefont",
    music = "filemusic",
    sounds = "filesound",
    sprites = "filesprites",
    tilemap = "filetilemap",
    pvt = "filerun",
    new = "filenewfile",
    gif = "filegif",
    tiles = "filetiles"
}

function WorkspaceTool:CreateWindow()

    self.pathHistory = {}
    -- Configure window settings
    self.iconPadding = 16
    self.iconWidth = 48
    self.iconHeight = 40
    self.windowBGColor = 11
    self.lastStartID = 0
    self.totalPerWindow = 12
    self.totalPerColumn = 3
    self.totalPerPage = 12
    
    self.refreshDelay = 5
    self.maxChars = 43

    local windowChrome = MetaSprite( FindMetaSpriteId("windowchrome" ) )

    self.totalDisks = tonumber(ReadBiosData("MaxDisks", 2))

    self.windowRect = NewRect(8, 16, windowChrome.width, windowChrome.height)
    

    -- TODO this should come from the bios file
    self.validFiles =
    {
        ".png",
        ".json",
        ".txt",
        ".lua",
        ".pv8",
        ".pvr",
        ".wav",
        ".gif",
        ".cs"
    }

    self.files = nil

end

function WorkspaceTool:UpdateWindowState()
    -- Pass the current path's window scroll and selection to the history
    self.pathHistory[self.currentPath.Path] = {
        scrollPos = self.vSliderData.value,
        selection = #self.selectedFiles > 0 and self.files[self.selectedFiles[1]].path or nil
    }
end

function WorkspaceTool:OpenWindow(path, scrollTo, selectedPath)

    -- Make sure the path exists before loading it up
    if(PathExists(path) == false) then

        -- Use the fault workspace path
        path = self.workspacePath

    end

    -- Check to see if there is a path we need to save
    if(self.currentPath ~= nil) then

        self:UpdateWindowState()
        
    end

    -- Reset values
    self.focus = true
    self.fileCount = 0
    self.totalSelections = 0

    -- Clear the window refresh time
    self.refreshTime = 0

    -- Reset runner type
    self.runnerType = "none"
    
    -- Make sure the last selections are cleared
    self:ClearSelections()

    -- print("self.pathHistory", dump(self.pathHistory))

    -- Look for the last scroll position of this path
    if(self.pathHistory[path.Path] ~= nil) then

        -- if there is a path history object, change the scrollTO and selection  value
        scrollTo = scrollTo or self.pathHistory[path.Path].scrollPos

        if(self.pathHistory[path.Path].selection ~= nil) then
            selectedPath = selectedPath or NewWorkspacePath(self.pathHistory[path.Path].selection)
        end

    end

    -- Set a default scrollTo value if none is provided
    scrollTo = scrollTo or 0
    selectedPath = selectedPath or nil

    -- print("Selections", path.Path, dump(selection))

    -- save the current directory
    self.currentPath = path

    -- TODO make sure the trash path check is valid
    self.isGameDir = pixelVisionOS:ValidateGameInDir(self.currentPath, requiredFiles) and self:TrashOpen() == false

    
    if(self.vSliderData == nil) then

        -- Create the slider for the window
        self.vSliderData = editorUI:CreateSlider({x = 192, y = 26, w = 16, h = 195}, "vsliderhandle", "This is a vertical slider")
        self.vSliderData.onAction = function(value)

            local newStartID = Clamp(self.hiddenRows * value, 0, self.hiddenRows - 1) * self.totalPerColumn

            if(newStartID ~= self.lastStartID) then

                -- Set the scroll position
                self.lastStartID = newStartID

                -- Refresh the window at the end of the frame
                self:RefreshWindow()

            end

        end

    end

    -- self:RegisterUI(self.closeButton, "UpdateButton", editorUI)
    self.desktopIconCount = 2 + self.totalDisks

    self.tmpY = 0
    self.tmpPos = null

    -- Check to see if we have window buttons
    if(self.windowIconButtons == nil) then

        -- Create a icon button group for all of the files
        self.windowIconButtons = pixelVisionOS:CreateIconGroup(false)

        self.tmpY  = 16
        for i = 1, self.totalDisks + 1 do

            pixelVisionOS:NewIconGroupButton(self.windowIconButtons, NewPoint(208, self.tmpY), "none", nil, toolTip)

            self.tmpY = self.tmpY + 40
        end

        -- -- Create the placeholder for the trash can
        pixelVisionOS:NewIconGroupButton(self.windowIconButtons, NewPoint(208, 198), "none", nil, toolTip, self.windowBGColor)

        -- Create default buttons
        for i = 1, self.totalPerWindow do

            -- Calculate the correct position
            self.tmpPos = CalculatePosition( i-1, self.totalPerColumn )
            self.tmpPos.x = (self.tmpPos.x * (self.iconWidth + self.iconPadding)) + 13
            self.tmpPos.y = (self.tmpPos.y * (self.iconHeight + self.iconPadding / 2)) + 32

            -- Create the new icon button
            pixelVisionOS:NewIconGroupButton(self.windowIconButtons, self.tmpPos, "none", nil, toolTip, self.windowBGColor)

        end

        -- Add the onTrigger callback
        self.windowIconButtons.onTrigger = function(id) self:OnWindowIconClick(id) end

        -- Make sure we disable any selection on the desktop when clicking inside of the window icon group
        self.windowIconButtons.onAction = function(id) self:OnWindowIconSelect(id) end

    end

    -- Reset the last start id
    self.lastStartID = 0

    -- TODO this shouldn't be called since it's handled in the window refresh but throws an error when taken out
    self:UpdateFileList()

    self:ChangeWindowTitle()

    -- Registere the window with the tool so it updates
    pixelVisionOS:RegisterUI({name = "Window"}, "UpdateWindow", self)

    self:UpdateContextMenu()

    -- Redraw the window without refreshing the list since we did it above
    self:RefreshWindow(true)

    if(selectedPath ~= nil and PathExists(selectedPath)) then

        self:SelectFile(selectedPath)

    end

    -- Update the slider
    editorUI:ChangeSlider(self.vSliderData, scrollTo)

end

function WorkspaceTool:UpdateFileList(useFiles)

    -- Get the list of files from the Lua Service
    self.files = useFiles or self:GetDirectoryContents(self.currentPath)

    -- Save a count of the files after we add the special files to the list
    self.fileCount = #self.files

    -- Update visible and hidden row count
    self.totalRows = math.ceil((self.fileCount- self.desktopIconCount) / self.totalPerColumn) + 1
    self.hiddenRows = self.totalRows - math.ceil(self.totalPerPage / self.totalPerColumn)

    self:EnableScrollBar()

end

function WorkspaceTool:EnableScrollBar()

    local moreFiles = ((self.fileCount - self.desktopIconCount) > self.totalPerWindow)
    local value = self.focus == true and moreFiles == true

    -- print("EnableScrollBar", value, self.focus, self.fileCount, self.desktopIconCount, self.totalPerWindow)

    if(value == false and moreFiles == false ) then
        editorUI:ChangeSlider(self.vSliderData, 0)
    end

    -- Enable the scroll bar if needed
    editorUI:Enable(self.vSliderData, value)

end

function WorkspaceTool:UpdateWindow()

    if(self.vSliderData.enabled) then
        editorUI:UpdateSlider(self.vSliderData)

        -- Check for mouse wheel scrolling
        local wheelDir = MouseWheel()

        if(wheelDir.Y ~= 0) then

            local scrollValue = Clamp(wheelDir.y, -1, 1) * -5

            editorUI:ChangeSlider(self.vSliderData, (Clamp(self.vSliderData.value * 100 + scrollValue, 0, 100)/100))
             
        
        end
    end

    pixelVisionOS:UpdateIconGroup(self.windowIconButtons)

    if(self.windowIconButtons.isDragging == true) then

        -- TODO update the selection list
        self:CurrentlySelectedFiles()

        -- local total = #self:CurrentlySelectedFiles()

        if(self.totalSelections > 1) then

            local x = editorUI.collisionManager.mousePos.x - 24 + 30
            local y = editorUI.collisionManager.mousePos.y - 12 - 2

            DrawRect(x, y, 9, 6, 0, DrawMode.SpriteAbove)
            DrawText(string.format("%02d", self.totalSelections), x + 1, y - 1, DrawMode.SpriteAbove, "small", 15, -4)
            
        end
    end

    if(self.firstPress ~= true and editorUI.collisionManager.mouseDown) then

        -- Change the flag so we don't trigger first press again
        self.firstPress = true

        self:ChangeFocus()

    elseif(self.firstPress == true and editorUI.collisionManager.mouseReleased) then

        self.firstPress = false

    end

    -- Check for file system changes
    self.refreshTime = self.refreshTime + editorUI.timeDelta

    if(self.refreshTime > self.refreshDelay) then

        local tmpFiles = self:GetDirectoryContents(self.currentPath)

        if(#tmpFiles ~= self.fileCount) then
            self:UpdateFileList(tmpFiles)
            -- Invalidate the component so it redraws at the end of the frame
            editorUI:Invalidate(self)
        end

        self.refreshTime = 0

    end

    -- Call draw window after each update
    self:DrawWindow()

end

function WorkspaceTool:ChangeFocus(value)
    
    if(editorUI.mouseCursor.pos.Y < 12) then
        return
    end
    
    value = value or (self.windowRect.contains(editorUI.mouseCursor.pos))

    self.focus = value

    -- redraw window chrome
    self:ChangeWindowTitle()

    if(editorUI.mouseCursor.cursorID == 1) then
        pixelVisionOS:ClearIconGroupSelections(self.windowIconButtons)
        self:ClearSelections()
    end

    self:EnableScrollBar()--)

    self:UpdateContextMenu("ChangeFocus")

end


function WorkspaceTool:CurrentlySelectedFiles()

    if(self.selectedFiles == nil) then

        -- print("Build selected file list")

        self.selectedFiles = {}

        local tmpFile = nil

        -- Loop through all of the files
        for i = 1, self.fileCount do

            -- Get the current file
            tmpFile = self.files[i]
            -- print("CurrentlySelectedFiles tmpFile", tmpFile.name, tmpFile.selected)
            -- check to see if the file is selected
            if(tmpFile.selected) then
                -- print("Add file", i)
                -- Insert the selected file into the array
                table.insert(self.selectedFiles, i)
            end
        end

        self.totalSelections = #self.selectedFiles

    end

    -- print("end selection", #self.selectedFiles, dump(self.selectedFiles), self.totalSelections)

    -- Return all of the selected files or nil if there are no selections
    return self.totalSelections > 0 and self.selectedFiles or nil

end

function WorkspaceTool:RefreshWindow(updateFileList)

    if(self.currentPath == nil) then
        self.currentPath = NewWorkspacePath("/Workspace/")
    end
    -- print("self.currentPath", self.currentPath)
    local infoPath = self.currentPath.AppendFile("info.json")

    self.runnerType = "none"

    -- TODO need to enable or disable the Run command based on what runner type and code file exist

    if(PathExists(infoPath)) then

        local data = ReadJson(infoPath)

        if(data["runnerType"] ~= nil) then

            if(data["runnerType"] == "lua" and PathExists(self.currentPath.AppendFile("code.lua"))) then

                self.runnerType = "lua"
            elseif(data["runnerType"] == "csharp" and PathExists(self.currentPath.AppendFile("code.cs"))) then
            
                self.runnerType = "csharp"
            end

        elseif(PathExists(self.currentPath.AppendFile("code.cs"))) then

            self.runnerType = "csharp"

        elseif(PathExists(self.currentPath.AppendFile("code.lua"))) then

            self.runnerType = "lua"

        end

    else

        if(PathExists(self.currentPath.AppendFile("code.cs"))) then

            self.runnerType = "csharp"

        elseif(PathExists(self.currentPath.AppendFile("code.lua"))) then
           
            self.runnerType = "lua"

        end

    end

    -- Check to see if we need to refresh the file list
    if(updateFileList == true) then

        -- Update the file list
        self:UpdateFileList()

    end


    -- print("RefreshWindow", "Runner Type", self.runnerType)

    -- TODO test to see if this is a game project

    -- TODO read info file

    -- TODO determin code mode

    -- Invalidate the component so it redraws at the end of the frame
    editorUI:Invalidate(self)

end

-- This is a helper for changing the text on the title bar
function WorkspaceTool:ChangeWindowTitle()

    -- Get the window path
    local pathTitle = self.currentPath.Path

    -- Set the color based on focus
    local colorID = self.focus == true and 15 or 5

    -- Clean up the path
    if(pathTitle:sub(1, 7) == "/Disks/") then
        pathTitle = pathTitle:sub(7, #pathTitle)
    elseif(pathTitle:sub(1, 5) == "/Tmp/") then
        pathTitle = pathTitle:sub(5, #pathTitle)
    end

    -- Clear the title bar text
    DrawRect(24, 16, 168, 8, 0, DrawMode.TilemapCache)

    -- Clip the title if it's to long
    if(#pathTitle > self.maxChars) then
        pathTitle = pathTitle:sub(0, self.maxChars - 3) .. "..."
    else
        pathTitle = string.rpad(pathTitle,self.maxChars, "")
    end

    -- Draw the new title bar text   
    DrawText(pathTitle, 19, 17, DrawMode.TilemapCache, "medium", colorID, - 4)

end

function WorkspaceTool:OnWindowIconSelect(id)

    -- print("Click", id, self.lastStartID, id + self.lastStartID, self.files[id].type)

    -- #3
    if(self.playingWav) then
        StopWav()
        self.playingWav = false
    end

    local realFileID = id > self.desktopIconCount and id + (self.lastStartID) or id
    local selectedFile = self.files[realFileID]

    if(selectedFile == nil) then
        return
    end

    local selectionFlag = true
    local clearSelections = self.files[id].type == "disk"
    -- TODO test for shift or ctrl

    -- TODO need to clear all selected files

    local selections = self:CurrentlySelectedFiles()
    self:InvalidateSelectedFiles()

    -- local totalSelections = selections == nil and 0 or #selections

    local specialFile = selectedFile.type == "updirectory" or selectedFile.type == "run"  or selectedFile.type == "run"

    if(Key(Keys.LeftShift, InputState.Down) or Key( Keys.RightShift, InputState.Down )) then

        -- Find the first selection and select all files all the way down to the current selection

        if(selections ~= nil) then

            -- Create the range between the first selected file and the one that was just selected
            local range = {Clamp(realFileID, self.totalSingleSelectFiles + 1, #self.files), Clamp(selections[1], self.totalSingleSelectFiles + 1, #self.files)}

            -- TODO should we test for the total disks?

            if(selections[1] > self.desktopIconCount) then

                -- Sort the range from lowest to highest
                table.sort(range, function(a,b) return a < b end)

                -- Loop through all of the files and fix the selected states
                for i = 1, self.fileCount do

                    -- Change the value based on if it is within the range
                    self.files[i].selected = (i >= range[1] and i <= range[2])

                end

            else

                clearSelections = true

            end

        end

        -- Make sure that we don't select the current file if it can't be selected in a group
        if(realFileID <= self.totalSingleSelectFiles) then
            selectionFlag = false
        end

        -- Update the selection
        selections = self:CurrentlySelectedFiles()

    elseif(Key(Keys.LeftControl, InputState.Down) or Key( Keys.RightControl, InputState.Down )) then

        -- change the selection flag to the opposite of the current file's selection value
        selectionFlag = not selectedFile.selected

        -- Check to see if we need to clear the selections
        if(self.totalSelections > 0) then
            local lastSelection = selections[#selections]

            --     print("Last Selections", lastSelection)

            if(specialFile == true or id <= self.totalSingleSelectFiles or (id <= self.totalSingleSelectFiles and lastSelection > self.totalSingleSelectFiles) or (lastSelection <= self.totalSingleSelectFiles and id > self.totalSingleSelectFiles)) then
                self:ClearSelections()

                --         -- Force deselect on all previous selections
                --         for i = 1, #selections do
                --             self.files[selections[i]].selected = false
                --         end
                self:RefreshWindow()

                -- print("Clear Selection CTRL")
                --         -- clearSelections = true
            end
        end

    else

        if(selectedFile.selected == false or self.focus == false) then
            clearSelections = true
        end

    end

    -- Deselect all the files
    if(selections ~= nil and clearSelections == true) then

        self:ClearSelections()

    end

    -- Set the selection of the file that was just selected
    selectedFile.selected = selectionFlag

    local lastValue = false

    -- Loop through all of the window buttons
    for i = 1, #self.windowIconButtons.buttons do

        -- Get a reference to the window button
        local tmpButton = self.windowIconButtons.buttons[i]

        if(tmpButton.fileID ~= -1) then

            -- Manually fix the selection of the buttons being displayed
            lastValue = tmpButton.selected

            tmpButton.selected = self.files[tmpButton.fileID].selected

            if(lastValue ~= tmpButton.selected) then
                editorUI:Invalidate(tmpButton)
            end

        end

    end

    -- #4
    -- self:UpdateContextMenu("OnWindowIconSelect")

end

function WorkspaceTool:OnWindowIconClick(id)


    -- Make sure desktop icons are not selected
    -- pixelVisionOS:ClearIconGroupSelections(self.desktopIconButtons)

    -- -- local index = id + (lastStartID)-- TODO need to add the scrolling offset

    local realFileID = id > self.desktopIconCount and id + (self.lastStartID) or id


    -- print("Click", id, self.lastStartID, id + self.lastStartID)
    local tmpItem = self.files[realFileID]--CurrentlySelectedFiles()-- files[index]

    local type = tmpItem.type
    local path = tmpItem.path
    local type = tmpItem.type

    -- If the type is a folder, open it
    if(type == "folder" or type == "updirectory" or type == "disk" or type == "drive" or type == "trash") then


        if(#self.selectedFiles > 0 and type == "folder") then
            self.selectedFiles = {realFileID}
        else
            self.selectedFiles = {}
        end

        self.totalSelections = #self.selectedFiles

        -- self.selectedFiles = {}
        -- print("Open Click", realFileID, dump(self.selectedFiles), dump(tmpItem))

        self:OpenWindow(tmpItem.path)

    -- Check to see if the file is in the trash
    elseif(self:TrashOpen()) then

        -- Show warning message about trying to edit files in the trash
        pixelVisionOS:ShowMessageModal(self.toolName .. " Error", "You are not able to edit files inside of the trash.", 160)

        -- Check to see if the file is an executable
    elseif(type == "run") then

        if(self.runnerType == nil) then

            -- TODO You shouldn't be able to run a game if the runnerType is not set
            return

        end

        LoadGame(path, { runnerType = self.runnerType })

    elseif(type == "pv8") then

        local buttons = 
      {
        {
          name = "modalyesbutton",
          action = function(target)
            MountDisk(NewWorkspacePath(path))
          end,
          key = Keys.Enter,
          tooltip = "Press 'enter' to quit the tool"
        },
        {
            name = "modalnobutton",
            action = function(target)
              target:onParentClose()
            end,
            key = Keys.Enter,
            tooltip = "Press 'enter' to quit the tool"
          }
      }

        -- TODO need to see if there is space to mount another disk
        -- TODO need to know if this disk is being mounted as read only
        -- TODO don't run
        pixelVisionOS:ShowMessageModal("Run Disk", "Do you want to mount this disk?", 160, buttons)

    elseif(type == "wav") then

        PlayWav(NewWorkspacePath(path))

        self.playingWav = true

        -- Check to see if there is an editor for the type or if the type is unknown
    elseif(self.editorMapping[type] == nil or type == "unknown") then

        pixelVisionOS:ShowMessageModal(self.toolName .. " Error", "There is no tool installed to edit this file.", 160)

        -- Now we are ready to try to edit a file
    else

        if(type == "installer") then

            if(PathExists(NewWorkspacePath("/Workspace/")) == false) then

                pixelVisionOS:ShowMessageModal("Installer Error", "You need to create a 'Workspace' drive before you can run an install script.", 160)

                return

                -- TODO this could be optimized by using the path segments?
            elseif(string.starts(self.currentPath.Path, "/Disks/") == false) then

                -- TODO need to see if there is space to mount another disk
                -- TODO need to know if this disk is being mounted as read only
                -- TODO don't run
                pixelVisionOS:ShowMessageModal("Installer Error", "Installers can only be run from a disk.", 160)

                return

            end
        end

        -- When trying to load a tilemap.png file, check if there is a json file first
        if(type == "tiles" and PathExists(self.currentPath.AppendFile("tilemap.json"))) then
            -- Change the type to PNG so the image editor is used instead of the tilemap editor
            type = "png"
        end

        -- Find the correct editor from the list
        local editorPath = self.editorMapping[type]

        -- Set up the meta data for the editor
        local metaData = {
            directory = self.currentPath.Path,
            file = tmpItem.path,
            filePath = tmpItem.path, -- TODO this should be the root path
            fileName = tmpItem.fullName,
            -- introMessage = "Editing '" .. tmpItem.fullName .."'."
        }

        -- Check to see if the path to the editor exists
        if(PathExists(NewWorkspacePath(editorPath))) then

            -- Load the tool
            LoadGame(editorPath, metaData)

        end

        -- TODO find an editor for the file's extension
    end


end

function WorkspaceTool:OnOverDropTarget(src, dest)

    if(src.iconPath ~= dest.iconPath) then

        editorUI:HighlightIconButton(dest, true)

    end

end

function WorkspaceTool:DrawWindow()

    -- DrawRect( self.windowRect.x, self.windowRect.y, self.windowRect.width, self.windowRect.height, 2, DrawMode.Sprite )
    -- Check to see if the window has been invalidated before drawing it
    if(self.invalid ~= true or self.files == nil or self.fileActionActive == true) then
        return
    end

    local requiredFiles = {"data.json"}

    -- TODO this may be redundant
    -- if(self.runnerName ~= DrawVersion and self.runnerName ~= TuneVersion) then
    --     table.insert(requiredFiles, "info.json")
    -- end



    -- local tmpPath = NewWorkspacePath(item.path)
    local pathParts = self.currentPath.GetDirectorySegments()
    local systemRoot = ((pathParts[1] == "Workspace" and #pathParts == 1) or (pathParts[1] == "Disks" and #pathParts == 2))


    for i = 1, #self.windowIconButtons.buttons do

        -- Calculate the real index
        local fileID = i + self.lastStartID

        -- Get a reference to the button
        local button = self.windowIconButtons.buttons[i]

        local item = nil
        local spriteName = "none"

        -- Determine which index to use for the pathParts
        local pathOffset = pathParts[1] == "Workspace" and 1 or 2

        -- We'll use this name to figure out which desktop icon to show as open
        local desktopName = pathParts[ pathOffset ]

        -- Make sure the index is less than the maximum icon count
        if(i <= self.desktopIconCount) then

            -- Pick the files from the top of the list (desktop icons)
            fileID = i

            -- Check to see if the icon should be set to open if the name matches the desktopName
            pixelVisionOS:OpenIconButton(button, self.files[i].name == desktopName)

        end

        if(fileID <= self.fileCount) then

            item = self.files[fileID]

            -- Find the right type for the file
            self:UpdateFileType(item, self.isGameDir)

            spriteName = item.sprite ~= nil and item.sprite or self:GetIconSpriteName(item)

            if(spriteName == FileTypeMap["folder"] and systemRoot == true) then

                -- TODO need another check for libs and tools

                if(item.name == "System" or item.name == "Libs" or item.name == "Tools") then

                    -- TODO should we check to make sure the folder isn't empty?

                    local correctParent = self.currentPath.EntityName == "System"

                    if(item.name == "System") then
                        spriteName = "fileosfolder"
                    elseif(correctParent and correctParent) then
                        spriteName = "fileosfolder"
                    end
                end
            end

            local toolTip = "Double click to "

            if(item.name == "Run") then
                toolTip = toolTip .. "run this game."
            elseif(item.name == "..") then

                toolTip = toolTip .. "go to the parent folder."

            elseif(item.isDirectory == true) then

                toolTip = toolTip .. "open the " .. item.name .. " folder."
            else
                toolTip = toolTip .. "edit " .. item.fullName .. "."

            end
        else

        end


        -- Set the button values
        button.fileID = item ~= nil and fileID or -1
        button.iconName = item ~= nil and item.name or ""
        button.iconType = item ~= nil and item.type or "none"
        button.iconPath = item ~= nil and item.path or ""
        button.selected = item ~= nil and item.selected or false

        -- Reset button value
        button.onOverDropTarget = nil
        button.onDropTarget = nil
        button.dragDelay = item ~= nil and item.dragDelay or .5

        local enable = item ~= nil

        if(spriteName == "empty") then
            enable = false
        end

        editorUI:Enable(button, enable)

        -- print(button.name, item ~= nil, button, spriteName)

        if(item ~= nil) then

            -- Disable the drag on files that don't exist in the directory
            if(item.type == "updirectory" or item.type == "folder" or item.type == "disk" or item.type == "drive" or item.type == "trash") then

                -- updirectory and folder share the same code but we don't want to drag updirectory
                if(item.type == "updirectory") then
                    button.dragDelay = -1
                end

                button.onOverDropTarget = function(src, dest) self:OnOverDropTarget(src, dest) end

                -- -- Add on drop target code to each folder type
                button.onDropTarget = function(src, dest) self:FileDropAction(src, dest) end

            elseif(item.type == "run" or item.type == "installer") then

                button.dragDelay = -1

            end

        end

        pixelVisionOS:CreateIconButtonStates(button, spriteName, item ~= nil and item.name or "", item ~= nil and item.bgColor or self.windowBGColor)


    end

    -- Reset the component's validation once drwaing is done
    editorUI:ResetValidation(self)

    self:UpdateContextMenu()

end

function WorkspaceTool:OnOverDropTarget(src, dest)

    if(src.iconPath ~= dest.iconPath) then

        pixelVisionOS:HighlightIconButton(dest, true)

    end

end

function WorkspaceTool:FileDropAction(src, dest)

    -- print("drop",self.currentPath.path, src.iconPath, src.iconPath.parentPath, dest.iconPath)

    local srcPath = src.iconPath.parentPath
    -- build a list of files to process
    local destPath = dest.iconPath

    -- Get the current selections
    local selections = self:CurrentlySelectedFiles()

    -- Exit out of this if there are no selected files
    if(selections == nil) then
        return
    end

    -- print(self.files[selections[1]].path, destPath)

    -- do a quick test to make sure you don't just drop the same folder on itself and get an error
    if(#selections == 1 and self.files[selections[1]].isDirectory and self.files[selections[1]].path == destPath) then
        return
    end
    -- Clear the target file list
    self.targetFiles = {}

    -- Loop through all of the selections
    for i = 1, #selections do

        -- Make sure the selected directory is included
        table.insert(self.targetFiles, self.files[selections[i]].path)

    end

    local action = "move"

    local srcSeg = srcPath.GetDirectorySegments()
    local destSeg = destPath.GetDirectorySegments()

    if(srcSeg[1] == "Tmp" and srcSeg[2] == "Trash") then
        -- print("Trash")
        action = "move"
    elseif(destSeg[1] == "Tmp" and destSeg[2] == "Trash") then

        if(#selections == 1 and self.files[selections[1]].type == "disk") then

            self:OnEjectDisk(self.files[selections[1]].path)

            return

        else
            -- print("Trash")
            action = "throw out"
        end
    elseif(srcSeg[1] == "Disks" and destSeg[1] == "Disks") then
        if(srcSeg[2] ~= destSeg[2]) then
            -- print("Copy")
            action = "copy"
        end
    elseif(srcSeg[1] ~= destSeg[1]) then
        action = "copy"
    end

    -- Perform the file action
    self:StartFileOperation(srcPath, destPath, action)

end

function WorkspaceTool:UpdateFileType(item, isGameFile)

    --print("self.editorMapping", item.type, isGameFile)
    local key = item.type--item.isDirectory and item.type or item.ext

    key = item.type

    -- TODO support legacy files and test new extensions
    if(key == "png" and isGameFile == true) then
        -- -- print("Is PNG")
        if(item.name == "sprites" and self.editorMapping["sprites"] ~= nil) then
            key = "sprites"
        elseif(item.name == "tilemap" and self.editorMapping["tilemap"] ~= nil) then
            key = "tiles"
        elseif(item.name == "colors" and self.editorMapping["colors"] ~= nil) then
            key = "colors"
        end
    elseif(key == "sprites.png" and self.editorMapping["sprites"] ~= nil and isGameFile == true) then
        key = "sprites"
    elseif(key == "tiles.png" and self.editorMapping["tilemap"] ~= nil and isGameFile == true) then
        key = "tiles"
    elseif(key == "tilemap.json" and self.editorMapping["tilemap"] ~= nil and isGameFile == true) then
        key = "tilemap"
    elseif(key == "font.png") then

        if(isGameFile == false or self.editorMapping["font"] == nil) then
            key = "png"
        else
            key = "font"
        end

    elseif(key == "json" and isGameFile == true) then

        if(item.name == "sounds" and self.editorMapping["sounds"] ~= nil)then
            key = "sounds"
        elseif(item.name == "tilemap" and self.editorMapping["tilemap"] ~= nil) then
            key = "tilemap"
        elseif(item.name == "music" and self.editorMapping["music"] ~= nil) then
            key = "music"
        elseif(item.name == "data" and self.editorMapping["system"] ~= nil) then
            key = "system"
        elseif(item.name == "info" and self.editorMapping["info"] ~= nil) then
            key = "info"
        end

    end

    if(key == "wav") then
        item.ext = "wav"
    end

    -- Fix type for pv8 and runner templates
    if(item.type == "pv8" or item.type == "pvr") then
        key = item.type
    end

    -- Last chance to fix any special edge cases like the installer and info which share text file extensions
    if(key == "txt" and item.name:lower() == "installer") then
        key = "installer"
    end

    item.type = key

end

function WorkspaceTool:GetIconSpriteName(item)

    local iconName = FileTypeMap[item.type]

    -- print("name", name, iconName)

    if(iconName == "filecodelua") then

        if(self.runnerType == "none" or self.runnerType == "csharp") then

            iconName = "filedisabledcodelua"

        end

    elseif(iconName == "filecodecsharp") then

        if(self.runnerType == "none" or self.runnerType == "lua") then

            iconName = "filedisabledcodecsharp"

        end

    end
    
    return iconName == nil and "fileunknown" or iconName

end



function WorkspaceTool:GetDirectoryContents(workspacePath)

    -- Create empty entities table
    local entities = {}

    self.totalSingleSelectFiles = self.desktopIconCount

    -- Create the workspace desktop icon
    table.insert(
            entities,
            {
                name = "Workspace",
                type = "drive",
                path = self.workspacePath,
                isDirectory = true,
                selected = false,
                dragDelay = -1,
                sprite = "filedrive",
                bgColor = BackgroundColor()
            }
    )


    local disks = DiskPaths()

    -- TODO this should loop through the maximum number of disks
    for i = 1, self.totalDisks do

        local noDisk = i > #disks

        local name = noDisk and "none" or disks[i].EntityName
        local path = noDisk and "none" or disks[i]

        table.insert(entities, {
            name = name,
            isDirectory = true,
            sprite = noDisk and "empty" or "diskempty",
            tooltip = "Double click to open the '".. name .. "' disk.",
            tooltipDrag = "You are dragging the '".. name .. "' disk.",
            path = path,
            type = noDisk and "none" or "disk",
            bgColor = BackgroundColor()
        })
    end

    --  

    -- Check to see if there is a trash
    if(PathExists(self.trashPath) == false) then

        -- Create the trash directory
        CreateDirectory(self.trashPath)

    end

    -- TODO need to set the correct icon and background
    -- Create the trash entity
    table.insert(entities, {
        name = "Trash",
        sprite = #GetEntities(self.trashPath) > 0 and "filetrashfull" or "filetrashempty",
        tooltip = "The trash folder",
        path = self.trashPath,
        type = "trash",
        isDirectory = true,
        bgColor = BackgroundColor(),
        dragDelay = -1,
    })


    -- Get the parent directory
    local parentDirectory = workspacePath.ParentPath

    -- Check to see if this is a root directory
    if(parentDirectory.Path ~= "/Disks/" and parentDirectory.Path ~= "/Tmp/" and parentDirectory.Path ~= "/") then

        -- Add an entity to go up one directory
        table.insert(
                entities,
                {
                    name = "..",
                    type = "updirectory",
                    path = parentDirectory,
                    isDirectory = true,
                    selected = false,
                    bgColor = self.windowBGColor
                }
        )

        self.totalSingleSelectFiles = self.totalSingleSelectFiles + 1

    end

    local srcSeg = workspacePath.GetDirectorySegments()

    local totalSeg = #srcSeg

    local codeFilename = "code.lua"

    if(PathExists(NewWorkspacePath(self.currentPath).AppendFile("code.cs"))) then
        codeFilename = "code.cs"
    end

    local showRunner = self.runnerType ~= "none" and pixelVisionOS:ValidateGameInDir(workspacePath, {codeFilename})

    -- print("self.runnerType", self.runnerType)
    -- Check to see if this is a game directory and we should display the run exe
    if(showRunner and self:TrashOpen() == false) then

        if((srcSeg[1] == "Disks") or (srcSeg[1] == "Workspace" and totalSeg ~= 1)) then

            -- Add an entity to run the game
            table.insert(
                    entities,
                    {
                        name = "Run",
                        type = "run",
                        ext = "run",
                        path = self.currentPath,
                        isDirectory = false,
                        selected = false
                    }
            )

            if(pixelVisionOS:LoadCustomIcon(workspacePath.AppendFile("icon.png"))) then

                -- local sprites = 
                FileTypeMap.run = "filecustomicon"

                -- TODO overrite the run icon name with new sprite IDs
            else
                -- TODO if no icon, reset to default value
                FileTypeMap.run = "filerun"
            end
            self.totalSingleSelectFiles = self.totalSingleSelectFiles + 1

            self.isGameDir = true

        else
            self.isGameDir = false

        end


    end

    -- Get all of the entities in the directory
    local srcEntities = GetEntities(workspacePath)

    -- Make sure the src entity value is not empty
    if(srcEntities ~= nil) then

        -- Get the total and create a entity placeholder
        local total = #srcEntities
        local tmpEntity = nil

        -- Loop through each entity
        for i = 1, total do

            -- Get the current entity
            tmpEntity = srcEntities[i]

            -- print(tmpEntity.Path)
            if(string.starts(tmpEntity.EntityName, ".") == false) then
                -- Create the new file
                local tmpFile = {
                    fullName = tmpEntity.EntityName,
                    isDirectory = tmpEntity.IsDirectory,
                    parentPath = tmpEntity.ParentPath,
                    path = tmpEntity,
                    selected = false,
                    ext = "",
                    type = "none"
                }

                -- Split the file name by .
                local nameSplit = string.split(tmpFile.fullName, ".")

                -- The file name is the first item in the array
                tmpFile.name = nameSplit[1]

                -- Check to see if this is a directory
                if(tmpFile.isDirectory) then

                    tmpFile.type = "folder"

                    -- Insert the table
                    table.insert(entities, tmpFile)

                else

                    -- Get the entity's extension
                    tmpFile.ext = tmpEntity.GetExtension()

                    -- make sure that the extension is valid
                    if(table.indexOf(self.validFiles, tmpFile.ext) > - 1) then

                        -- Remove the first item from the name split since it's already used as the name
                        table.remove(nameSplit, 1)

                        -- Join the nameSplit table with . to create the type
                        tmpFile.type = table.concat(nameSplit, ".")

                        -- Add theh entity
                        table.insert(entities, tmpFile)
                    end

                end

            end

        end

    end

    return entities

end

function WorkspaceTool:SelectFile(workspacePath)

    -- set the default value to 0 (invald file ID)
    local fileID = 0

    -- Loop through all of the files
    for i = 1, self.fileCount do

        -- Test the file path with the workspace path
        if(self.files[i].path.Path == workspacePath.Path) then

            -- Save the file ID and exit the loop
            fileID = i
            break

        end

    end

    -- Make sure there is a file that can be selected
    if(fileID > 0) then

        self:ClearSelections()

        -- Select the current file
        self.files[fileID].selected = true

        -- Calculate position
        local tmpVPer = CalculatePosition(fileID, self.totalPerColumn).Y / self.totalRows

        -- Update slide position
        editorUI:ChangeSlider(self.vSliderData, tmpVPer)

        -- Tell the window to redraw
        self:RefreshWindow()

    end

end

function WorkspaceTool:ClearSelections()

    -- Get the current selections
    local selections = self:CurrentlySelectedFiles()

    -- print("Clearing", dump(selections))

    -- Make sure there are selections
    if(selections ~= nil) then

        -- Loop through the selections and disable them
        for i = 1, #selections do
            self.files[selections[i]].selected = false
        end

    end

    self:InvalidateSelectedFiles()

    -- self:RefreshWindow(false)

end

function WorkspaceTool:InvalidateSelectedFiles()

    self.selectedFiles = nil
    self.totalSelections = 0

end