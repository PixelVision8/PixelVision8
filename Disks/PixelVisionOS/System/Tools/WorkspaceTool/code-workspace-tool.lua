-- Create table to store the workspace tool logic
WorkspaceTool = {}
WorkspaceTool.__index = WorkspaceTool

-- Import scripts needed by the workspace tool
LoadScript("code-window")
LoadScript("code-drop-down-menu")
LoadScript("code-icon-button")
LoadScript("code-process-file-actions")
LoadScript("code-file-actions")
LoadScript("pixel-vision-os-item-picker-v1")

function WorkspaceTool:Init()

  -- Create an global instance of the Pixel Vision OS
  pixelVisionOS = PixelVisionOS:Init()

  -- Used for debugging
  -- pixelVisionOS.displayFPS = true

  -- Get a global reference to the Editor UI
  editorUI = pixelVisionOS.editorUI

  -- Create a new table for the instance with default properties
  local _workspaceTool = {
    toolName = "Workspace Explorer",
    runnerName = SystemName(),
    rootPath = ReadMetadata("RootPath", "/"),
    gameName = ReadMetadata("GameName", "FilePickerTool"),
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

  -- Create background
  -- _workspaceTool:DrawWallpaper()
  -- _workspaceTool:RebuildDesktopIcons()



  -- Create dropdown menu
  _workspaceTool:CreateDropDownMenu()

  _workspaceTool:RestoreLastPath()

  -- Return the new instance of the editor ui
  return _workspaceTool

end

function WorkspaceTool:Update(timeDelta)

  -- This needs to be the first call to make sure all of the OS and editor UI is updated first
  pixelVisionOS:Update(timeDelta)



  -- Loop through all of the registered UI and update them
  for i = 1, self.uiTotal do

    -- Get a reference to the UI data
    local ref = self.uiComponents[i]

    if(ref ~= nil) then

      -- Only update UI when the modal is not active
      if(pixelVisionOS:IsModalActive() == false or ref.ignoreModal) then

        -- Call the UI scope's update and pass back in the UI data
        ref.uiScope[ref.uiUpdate](ref.uiScope, ref.uiData)

      end

    end

  end

end

function WorkspaceTool:Draw()

  -- The UI should be the last thing to draw after your own custom draw calls
  pixelVisionOS:Draw()

end

function WorkspaceTool:RegisterUI(data, updateCall, scope, ignoreModal)

  scope = scope or pixelVisionOS

  -- Try to remove an existing instance of the component
  self:RemoveUI(data.name)

  table.insert(self.uiComponents, {uiData = data, uiUpdate = updateCall, uiScope = scope, ignoreModal = ignoreModal or false})

  self.uiTotal = #self.uiComponents

  -- Return an instance of the component
  return data

end

function WorkspaceTool:RemoveUI(name)

  local i
  local removeItem = -1

  for i = 1, self.uiTotal do

    if(self.uiComponents[i].uiData.name == name) then

      -- Set the remove flag to true
      removeItem = i

      -- Exit out of the loop
      break

    end

  end

  -- If there is nothing to remove than exit out of the function
  if(removeItem == -1) then
    return
  end

  -- Remove item
  table.remove(self.uiComponents, removeItem)

  -- Update the total
  self.uiTotal = #self.uiComponents

  -- For debugging

  print("Remove", removeItem, "total", self.uiTotal)

  for i = 1, #self.uiComponents do
    print("Left over", self.uiComponents[i].uiData.name)
  end

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

  if(newPath.Path == self.workspacePath.Path and #GetEntities(self.workspacePath) == 0) then
    self:AutoCreateFirstProject()
  else
    self:OpenWindow(newPath, lastScrollPos, lastSelection)
  end

end

function WorkspaceTool:AutoCreateFirstProject()

  pixelVisionOS:ShowMessageModal("Welcome to Pixel Vision 8", "It looks like you are running Pixel Vision 8 for the first time. We have automatically created a new 'Workspace drive' for you on your computer at: \n\n" .. DocumentPath().."\n\nYou can learn more about the Workspace and using PV8 by reading the documentation at:\n\nhttps://www.pixelvision8.com/documentation\n\nBefore getting started, would you like to creating a new project?", 224, true,
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
