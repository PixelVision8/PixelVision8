--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

local libraryInfoPanelID = "LibraryInfoPanel"

function InfoTool:CreateLibraryInfoPanel()

    
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
    
               if(self.sharedLibFiles[tmpName] == nil) then
    
                   local selected = table.indexOf(includedLibs, tmpName) > - 1
    
                   -- Store the file and path
                   self.sharedLibFiles[tmpName] = tmpFile
    
                   -- Add to the display list
                   table.insert(self.filePaths, {name = tmpName, selected = selected, path = tmpFile.Path})
    
               end
    
           end
    
       end
    
    
    end
    
    -- Need to see if there are enough files to display
    if(#self.filePaths < self.maxFilesToDisplay) then
       self.maxFilesToDisplay = #self.filePaths
    
       -- TODO disable scroller
    end
    
    for i = 1, self.maxFilesToDisplay do
       local tmpCheckbox = editorUI:CreateToggleButton({x = 16, y = startY, w = 8, h = 8}, "checkbox", "Select a library file to include with the build.")
       tmpCheckbox.onAction = function(value)
    
           if(Key(Keys.LeftShift) or Key(Keys.RightShift)) then
    
               -- Loop through all of the files
               for i = 1, #self.filePaths do
    
                   -- Change all of the file values
                   self.filePaths[i].selected = value
    
               end
    
               self:DrawFileList(self.fileListOffset)
    
           else
    
               -- Change a single file value
               self.filePaths[i + self.fileListOffset].selected = value
           end
    
           self:InvalidateData()
    
       end
    
       startY = startY + 8
    
       table.insert(self.fileCheckboxes, tmpCheckbox)
    
    end

    self.fileSliderData = editorUI:CreateSlider({x = 168, y = 168 + 8, w = 16, h = 56 - 8}, "vsliderhandle", "Scroll to see the more files to install.")
    self.fileSliderData.onAction = function(value) self:OnFileValueChange(value) end
    
    editorUI:Enable(self.fileSliderData, #self.filePaths > self.maxFilesToDisplay)

    -- Reset list
    self:DrawFileList()

    pixelVisionOS:RegisterUI({name = libraryInfoPanelID}, "LibraryInfoPanelUpdate", self)

end 

function InfoTool:LibraryInfoPanelUpdate()

	for i = 1, self.maxFilesToDisplay do
        editorUI:UpdateButton(self.fileCheckboxes[i])
    end

    editorUI:UpdateSlider(self.fileSliderData)

end

function InfoTool:DrawFileList(offset)

    self.fileListOffset = offset or 0

    DrawRect(24, 168 + 8, 144, 56 - 8, 11, DrawMode.TilemapCache)

    for i = 1, self.maxFilesToDisplay do

        local file = self.filePaths[i + self.fileListOffset] or ""
        local fileName = file.name
        local checkValue = file.selected

        editorUI:ToggleButton(self.fileCheckboxes[i], checkValue, false)

        if(#fileName > 35) then
            fileName = fileName:sub(1, 32) .. "..."
        end

        -- TODO need to check the size of the name

        fileName = string.rpad(fileName, 200, " "):upper()

        DrawText(fileName, 25, 168 + (i * 8), DrawMode.TilemapCache, "small", 0, - 4)

    end

end

function InfoTool:OnFileValueChange(value)

    local offset = math.ceil((#self.filePaths - self.maxFilesToDisplay) * value)

    self:DrawFileList(offset)

end

function InfoTool:OnReset()

    pixelVisionOS:ShowMessageModal("Reset Installer", "Do you want to reset the installer to its default values?", 160, true,
        function()
            if(pixelVisionOS.messageModal.selectionValue == true) then

                for i = 1, #self.filePaths do
                    self.filePaths[i].selected = true
                end

                self:DrawFileList(self.fileListOffset)
            end
        end
    )

end