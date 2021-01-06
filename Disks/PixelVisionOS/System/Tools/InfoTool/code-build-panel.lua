--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

local buildInfoPanelID = "BuildInfoPanel"

function InfoTool:CreateBuildInfoPanel()

    local buildFlagWin = editorUI:CreateToggleButton({x = 200, y = 176+ 24, w = 8, h = 8}, "checkbox", "Create a Windows executable.")
    
    buildFlagWin.onAction = function(value)
    
       self:InvalidateData()
    
    end
    
    local winRunnerExists = PathExists(NewWorkspacePath(self.buildTemplatePaths[1].path))
    
    if(winRunnerExists) then
       editorUI:ToggleButton(buildFlagWin, gameEditor:ReadMetadata(self.buildFlagLabels[1]) == "true")
    end
    
    editorUI:Enable(buildFlagWin, winRunnerExists)
    
    table.insert(self.buildFlagCheckboxes, buildFlagWin)
    
    local buildFlagMac = editorUI:CreateToggleButton({x = 200, y = 184+ 24, w = 8, h = 8}, "checkbox", "Create a Mac App.")

    
    buildFlagMac.onAction = function(value)
    
       self:InvalidateData()
    
    end
    
    local macRunnerExists = PathExists(NewWorkspacePath(self.buildTemplatePaths[2].path))
    
    if(macRunnerExists) then
       editorUI:ToggleButton(buildFlagMac, gameEditor:ReadMetadata(self.buildFlagLabels[2]) == "true")
    end
    
    editorUI:Enable(buildFlagMac, macRunnerExists)
    
    table.insert(self.buildFlagCheckboxes, buildFlagMac)
    
    local buildFlagLinux = editorUI:CreateToggleButton({x = 200, y = 192+ 24, w = 8, h = 8}, "checkbox", "Create a Linux executable.")
    
    buildFlagLinux.onAction = function(value)
    
       self:InvalidateData()
    
    end
    
    local linuxRunnerExists = PathExists(NewWorkspacePath(self.buildTemplatePaths[3].path))
    
    if(linuxRunnerExists) then
       editorUI:ToggleButton(buildFlagLinux, gameEditor:ReadMetadata(self.buildFlagLabels[3]) == "true")
    end
    
    editorUI:Enable(buildFlagLinux, linuxRunnerExists)
    
    
    table.insert(self.buildFlagCheckboxes, buildFlagLinux)
    
    
    local buildFlagExtras = editorUI:CreateToggleButton({x = 200, y = 208, w = 8, h = 8}, "checkbox", "Create a Linux executable.")
    
    editorUI:ToggleButton(buildFlagExtras, gameEditor:ReadMetadata(self.buildFlagLabels[4]) == "true")
    
    buildFlagExtras.onAction = function(value)
    
       self:InvalidateData()
    
    end

    -- These UI elements live outside of the panel

    self.buildButtonData = editorUI:CreateButton({x = 216, y = 32}, "buildbutton", "Build the game disk.")
    self.buildButtonData.onAction = function(value)
    
       self.libFilesToCopy = {}
    
       for i = 1, #self.filePaths do
           -- print("Include", dump(filePaths[i]))
           if(self.filePaths[i].selected == true) then
    
               table.insert(self.libFilesToCopy, self.filePaths[i].name)
           end
       end
    
       -- buildFlagWin
    
       pixelVisionOS:ShowMessageModal("Build Game", "Are you sure you want to build ".. self.nameInputData.text .."'? This will create a new PV8 disk and executables for any selected platforms.", 160, true,
           function()
    
               if(pixelVisionOS.messageModal.selectionValue == true) then
    
                   self:OnExportGame()
                   -- TODO need to call the build function
                   -- OnInstall()
    
               end
    
           end
       )
    
    end
    
      -- self.cleanCheckboxData = editorUI:CreateToggleButton({x = 216, y = 56, w = 8, h = 8}, "radiobutton", "Toggles doing a clean build and removes all previous builds.")
      
      -- editorUI:ToggleButton(self.cleanCheckboxData, gameEditor:ReadMetadata("clear", "false") == "true", false)
      
      -- self.cleanCheckboxData.onAction = function(value)
    
      --  -- print("Clean build", value)
    
      --  if(value == false) then
      --      self:InvalidateData()
      --      return
      --  end
    
      --  pixelVisionOS:ShowMessageModal("Warning", "Are you sure you want to do a clean build? This will delete the build directory before creating the PV8 disk. Old builds will be deleted. This can not be undone.", 160, true,
      --      function()
    
    
      --          if(pixelVisionOS.messageModal.selectionValue == false) then
    
      --              -- Force the checkbox back into the false state
      --              editorUI:ToggleButton(self.cleanCheckboxData, false, false)
    
      --          else
      --              -- Force the checkbox back into the false state
      --              editorUI:ToggleButton(self.cleanCheckboxData, true, false)
      --          end
    
      --          -- Force the button to redraw since restoring the modal will show the old state
      --          editorUI:Invalidate(self.cleanCheckboxData)
      --          self:InvalidateData()
      --      end
      --  )
    
--    end
    

    pixelVisionOS:RegisterUI({name = buildInfoPanelID}, "BuildInfoPanelUpdate", self)

end 

function InfoTool:BuildInfoPanelUpdate()

	for i = 1, #self.buildFlagCheckboxes do
        editorUI:UpdateButton(self.buildFlagCheckboxes[i])
    end

   editorUI:UpdateButton(self.buildButtonData)
   -- editorUI:UpdateButton(self.cleanCheckboxData)

end

function InfoTool:OnExportGame()

   local srcPath = NewWorkspacePath(self.rootDirectory)
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

   self.buildingDisk = true

   if(self.progressModal == nil) then
       --
       --   -- Create the model
       self.progressModal = ProgressModal:Init("File Action ", editorUI)

       -- Open the modal
       pixelVisionOS:OpenModal(self.progressModal)

   end

   -- end

end