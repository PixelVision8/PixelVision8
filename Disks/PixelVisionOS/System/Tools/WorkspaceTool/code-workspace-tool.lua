-- Create table to store the workspace tool logic
WorkspaceTool = {}
WorkspaceTool.__index = WorkspaceTool


LoadScript("pixel-vision-os-progress-modal-v2")
LoadScript("pixel-vision-os-file-modal-v1")

-- Import scripts needed by the workspace tool
LoadScript("code-window")
LoadScript("code-drop-down-menu")
LoadScript("code-icon-button")
LoadScript("code-process-file-actions")
LoadScript("code-file-actions")
LoadScript("code-export")
LoadScript("pixel-vision-os-item-picker-v1")

function WorkspaceTool:Init()

  -- Create a new table for the instance with default properties
  local _workspaceTool = {
    toolName = "Workspace Explorer",
    runnerName = SystemName(),
    rootPath = ReadMetadata("RootPath", "/"),
    gameName = ReadMetadata("GameName", "WorkspaceTool"),
    workspacePath = NewWorkspacePath("/Workspace/"),
    trashPath = NewWorkspacePath("/Tmp/Trash/"),
    editorMapping = pixelVisionOS:FindEditors(),
    uiComponents = {},
    uiTotal = 0
  }

  -- Create a global reference of the new workspace tool
  setmetatable(_workspaceTool, WorkspaceTool)

  -- Update title if it running from the disk
  if(string.starts(_workspaceTool.rootPath, "/Disks/")) then
    _workspaceTool.toolName = _workspaceTool.toolName .. " (DISK)"
  end

  -- Change the title
  pixelVisionOS:ChangeTitle(_workspaceTool.toolName, "toolbaricontool")

  -- Create dropdown menu
  _workspaceTool:CreateDropDownMenu()

  _workspaceTool:RestoreLastPath()

  -- Return the new instance of the editor ui
  return _workspaceTool

end

function WorkspaceTool:Shutdown()

  -- Save the current session ID
  WriteSaveData("sessionID", SessionID())

  -- Make sure we don't save paths in the tmp directory
  WriteSaveData("lastPath", self.currentPath ~= nil and self.currentPath.Path or "none")
  --
  -- Save the current session ID
  WriteSaveData("scrollPos", (self.vSliderData ~= nil and self.vSliderData.value or 0))

  -- Save the current selection
  WriteSaveData("selection", (self.windowIconButtons ~= nil and editorUI:ToggleGroupSelections(self.windowIconButtons)[1] or 0))

end

function WorkspaceTool:RestoreLastPath()

  -- TODO ned  to restore selections correctly
  local newPath = SessionID() == ReadSaveData("sessionID", "") and ReadSaveData("lastPath", "none") or "none"
  local lastScrollPos = tonumber(ReadSaveData("scrollPos", "0"))
  local lastSelection = tonumber(ReadSaveData("selection", "0"))

  -- Read metadata last path and default to the newPath
  local lastPath = ReadMetadata("overrideLastPath", newPath)

  if(lastPath ~= "none") then

    -- Clear last path from metadata
    WriteMetadata( "overrideLastPath", "none" )

    -- override the default path to open
    newPath = lastPath
    lastScrollPos = 0
    lastSelection = 0

  end

  -- Convert the path to a Workspace Path
  newPath = newPath == "none" and self.workspacePath or NewWorkspacePath(newPath)

  -- Open the window to the new path
  if(newPath.Path == self.workspacePath.Path) then
    
    local files =  GetEntities(self.workspacePath)

    if(#files == 0 or (#files == 1 and files[1] != "System")) then

      self:AutoCreateFirstProject()
    
      return
      
    end
    
  end

  self:OpenWindow(newPath, lastScrollPos, lastSelection)

end

function WorkspaceTool:AutoCreateFirstProject()

  pixelVisionOS:ShowMessageModal("Welcome to Pixel Vision 8", "It looks like you are running Pixel Vision 8 for the first time. We have automatically created a new 'Workspace drive' for you on your computer at: \n\n" .. DocumentPath().."\n\nYou can learn more about the Workspace and using PV8 by reading the documentation at:\n\nhttps://www.pixelvision8.com/documentation\n\nBefore getting started, would you like to create a new project?", 224, true,
    function()

      local defaultPath = self.workspacePath

      if(pixelVisionOS.messageModal.selectionValue == true) then

        defaultPath = self:CreateNewProject("MyFirstGame", self.workspacePath)

        local readmePath = NewWorkspacePath("/PixelVisionOS/README.txt")

        if(PathExists(readmePath)) then
          CopyTo(readmePath, self.workspacePath.AppendFile(readmePath.EntityName))
        end

      end

      self:OpenWindow(defaultPath)

    end
  )

end
