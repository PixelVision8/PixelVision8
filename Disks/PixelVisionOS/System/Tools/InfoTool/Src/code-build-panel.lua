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
    
   --  print("self.buildFlagLabels[1]", dump(self.buildFlagLabels), "'"..gameEditor:ReadMetadata(self.buildFlagLabels[1]).."'", winRunnerExists)

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
    
      local buttons = 
      {
         {
            name = "modalbuildbutton",
            action = function(target)
               target:onParentClose()
               pixelVisionOS:OnExportGame(NewWorkspacePath(self.rootDirectory))
            end,
            key = Keys.Enter,
            tooltip = "Press 'enter' to create a new build"
         },
         {
            name = "modalcancelbutton",
            action = function(target)
               target:onParentClose()
            end,
            key = Keys.Escape,
            tooltip = "Press 'esc' to not make any changes"
         }
      }
      
       pixelVisionOS:ShowMessageModal(
           "Build Game", 
           "Are you sure you want to build ".. self.nameInputData.text .."'? This will create a new PV8 disk and executables for any selected platforms.", 
           160, 
           buttons
       )
    
    end
    
    pixelVisionOS:RegisterUI({name = buildInfoPanelID}, "BuildInfoPanelUpdate", self)

end 

function InfoTool:BuildInfoPanelUpdate()

	for i = 1, #self.buildFlagCheckboxes do
        editorUI:UpdateButton(self.buildFlagCheckboxes[i])
    end

   editorUI:UpdateButton(self.buildButtonData)

end