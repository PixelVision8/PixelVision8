extToTypeMap = 
{
  colors = ".png",
  system = ".json",
  font = ".font.png",
  music = ".json",
  sounds = ".json",
  sprites = ".png",
  tilemap = ".json",
  installer = ".txt",
  info = ".json",
  wav = ".wav"
}

-- Helper utility to delete files by moving them to the trash
function WorkspaceTool:DeleteFile(path)

  -- Create the base trash path for the file
  local newPath = self.trashPath

  -- See if this is a directory or a file and add the entity name
  if(path.IsDirectory) then
    newPath = newPath.AppendDirectory(path.EntityName)
  else
    newPath = newPath.AppendFile(path.EntityName)
  end

  -- Make sure the path is unique
  newPath = UniqueFilePath(newPath)

  -- Move to the new trash path
  MoveTo(path, newPath)

end

function WorkspaceTool:OnDeleteFile()

  -- Get the current selections
  local selections = self:CurrentlySelectedFiles()

  -- Exit out of this if there are no selected files
  if(selections == nil) then
    return
  end

  -- TODO how should this be handled if there are multiple selections
  self.fileActionSrc = self.currentPath

  self.filesToCopy = {}

  -- Loop through all of the selections
  for i = 1, #selections do

    local srcPath = self.files[selections[i]].path

    -- Make sure the selected directory is included
    table.insert(self.filesToCopy, srcPath)

    if(srcPath.IsDirectory) then

      -- Add all of the files that need to be copied to the list
      local childEntities = GetEntitiesRecursive(srcPath)

      -- Loop through each of the children and add them to the list
      for j = 1, #childEntities do
        table.insert(self.filesToCopy, childEntities[i])
      end

    end

  end

  print("Files", dump(self.filesToCopy))

  -- Always make sure anything going into the trash has a unique file name
  self:StartFileOperation(self.trashPath, "throw out")

end

function WorkspaceTool:StartFileOperation(destPath, action)

  -- Clear the path filter (used to change base path if there is a duplicate)
  local fileActionPathFilter = nil

  local fileActionSrc = self.currentPath

  if(action == "delete") then
    self:OnRunFileAction(destPath, action)
    -- invalidateTrashIcon = true
    -- fileActionActive = true
    return
  end

  -- Modify the destPath with the first item for testing
  destPath = destPath.AppendPath(self.filesToCopy[1].Path:sub( #fileActionSrc.Path + 1))

  if(action == "throw out") then
    fileActionPathFilter = UniqueFilePath(destPath)
    -- invalidateTrashIcon = true
  end

  if(self.filesToCopy[1].IsChildOf(destPath)) then

    pixelVisionOS:ShowMessageModal(
      "Workspace Path Conflict",
      "Can't perform a file action on a path that is the child of the destination path.",
      128 + 16, false, function() self:CancelFileActions() end
    )
    return

  elseif(PathExists(destPath) and fileActionPathFilter == nil) then

    local duplicate = destPath.Path == self.filesToCopy[1].Path

    -- Ask if the file first item should be duplicated
    pixelVisionOS:ShowMessageModal(
      "Workspace Path Conflict",
      "Looks like there is an existing file with the same name in '".. destPath.Path .. "'. Do you want to " .. (duplicate and "duplicate" or "replace") .. " '"..destPath.EntityName.."'?",
      200,
      true,
      function()

        -- Only perform the copy if the user selects OK from the modal
        if(pixelVisionOS.messageModal.selectionValue) then

          if(duplicate == true) then

            fileActionPathFilter = UniqueFilePath(destPath)

          else
            -- print("Delete", destPath)
            self:SafeDelete(destPath)
          end
          -- Start the file action process
          -- fileActionActive = true
          self:OnRunFileAction(destPath, action, fileActionPathFilter)

        else
          self:CancelFileActions()
          self:RefreshWindow(true)

        end

      end
    )

  else

    pixelVisionOS:ShowMessageModal(
      "Workspace ".. action .." Action",
      "Do you want to ".. action .. " " .. #self.filesToCopy .. " files?",
      160,
      true,
      function()

        -- -- Only perform the copy if the user selects OK from the modal
        if(pixelVisionOS.messageModal.selectionValue) then

          -- Start the file action process
          -- fileActionActive = true
          self:OnRunFileAction(destPath, action, fileActionPathFilter)

        else
          self:CancelFileActions()
          self:RefreshWindow(true)
        end

      end
    )

  end

end

function WorkspaceTool:OnRunFileAction(destPath, action, fileActionPathFilter)

  local args = { self.currentPath, destPath, action, fileActionPathFilter or "" }

  for i = 1, #self.filesToCopy do
    table.insert(args, self.filesToCopy[i].Path)
  end

  print("args", dump(args))

  local success = RunBackgroundScript("code-process-file-actions.lua", args)

  print("success", success)

  if(success) then

    if(self.progressModal == nil) then

      -- Create the model
      self.progressModal = ProgressModal:Init("File Action ", editorUI, function() self:CancelFileActions() end)

      self.progressModal.fileAction = action
      self.progressModal.totalFiles = (#self.filesToCopy - 2)
    end

    -- Open the modal
    pixelVisionOS:OpenModal(self.progressModal)

    self:RegisterUI({name = "ProgressUpdate"}, "UpdateFileActionProgress", self, true)

  end

  self:UpdateFileActionProgress()


end

function WorkspaceTool:UpdateFileActionProgress(data)

  print("UpdateFileActionProgress running", IsExporting())

  -- Check to see if exporting is done
  if(IsExporting() == false) then

    pixelVisionOS:CloseModal()

    self.progressModal = nil

    -- Refresh the window and get the new file list
    self:RefreshWindow(true)

    -- Remove the callback from the UI update loop
    self:RemoveUI("ProgressUpdate")

    -- Exit out of the function
    return

  end

  -- Get the current percentage
  local percent = ReadExportPercent()

  local fileActionActiveTotal = self.progressModal.totalFiles
  local fileActionCounter = math.ceil(fileActionActiveTotal * (percent / 100))

  local message = "test"--self.progressModal.fileAction .. " "..string.lpad(tostring(fileActionCounter), string.len(tostring(fileActionActiveTotal)), "0") .. " of " .. fileActionActiveTotal .. ".\n\n\nDo not restart or shut down Pixel Vision 8."

  self.progressModal:UpdateMessage(message, percent)



end

function WorkspaceTool:CanCopy(file)

  return (file.name ~= "Run" and file.type ~= "updirectory")

end

function WorkspaceTool:CancelFileActions()

  if(self.fileActionActive == true) then
    self:OnFileActionComplete()

    -- editorUI.mouseCursor:SetCursor(1, false)
  end

end

function WorkspaceTool:SafeDelete(srcPath)

  self:Delete(srcPath)--, trashPath)

end

function WorkspaceTool:OnCopy()

  -- if(windowIconButtons ~= nil) then

  --     -- Remove previous files to be copied
  --     filesToCopy = {}
  --     fileActionSrc = self.currentPath

  --     -- TODO this needs to eventually support multiple selections

  --     local file = CurrentlySelectedFile()

  --     if(CanCopy(file)) then

  --         local tmpPath = NewWorkspacePath(file.path)

  --         -- Test if the path is a directory
  --         if(tmpPath.IsDirectory) then

  --             -- Add all of the files that need to be copied to the list
  --             filesToCopy = GetEntitiesRecursive(tmpPath)

  --         end

  --         -- Make sure the selected directory is included
  --         table.insert(filesToCopy, 1, NewWorkspacePath(file.path))

  --         -- print("Copy File", file.name, file.path, #filesToCopy, dump(filesToCopy))

  --         -- Enable the paste shortcut
  --         pixelVisionOS:EnableMenuItemByName(PasteShortcut, true)

  --         -- TODO eventually need to change the message to handle multiple files
  --         pixelVisionOS:DisplayMessage(#filesToCopy .. " file" .. (#filesToCopy == 1 and " has" or "s have") .." been copied.", 2)

  --     else

  --         -- Display a message that the file can not be copied
  --         pixelVisionOS:ShowMessageModal(toolName .. "Error", "'".. file.name .. "' can not be copied.", 160, false)

  --         -- Make sure we can't activate paste
  --         pixelVisionOS:EnableMenuItemByName(PasteShortcut, false)

  --     end

  -- end

end

function WorkspaceTool:OnPaste(dest)

  -- -- Get the destination directory
  -- dest = dest or self.currentPath

  -- local destPath = NewWorkspacePath(dest)

  -- -- If there are no files to copy, exit out of this function
  -- if(filesToCopy == nil) then
  --     return
  -- end

  -- -- Perform the file action validation
  -- StartFileOperation(destPath, "copy")

  -- pixelVisionOS:DisplayMessage("Entit" .. (#filesToCopy > 1 and "ies have" or "y has") .. " has been pasted.", 2)

end

function WorkspaceTool:OnNewFolder(name)

  if(self.currentPath == nil) then
    return
  end

  if(name == nil) then
    name = "Untitled"
  end

  -- Create a new unique workspace path for the folder
  local newPath = UniqueFilePath(self.currentPath.AppendDirectory(name))

  local newFileModal = self:GetNewFileModal()

  -- Set the new file modal to show the folder name
  newFileModal:SetText("New Folder", newPath.EntityName, "Folder Name", true)

  -- Open the new file modal before creating the folder
  pixelVisionOS:OpenModal(newFileModal,
    function()

      if(newFileModal.selectionValue == false) then
        return
      end

      -- Create a new workspace path
      local filePath = self.currentPath.AppendDirectory(newFileModal.inputField.text)

      -- Make sure the path doesn't exist before trying to make a new directory
      if(PathExists(filePath) == false) then

        -- This is a bit of a hack to get around an issue creating folders on disks.

        -- Test to see if we are creating a folder on a disk
        if(string.starts(filePath.Path, "/Disks/")) then

          -- Create a new path in the tmp directory
          local tmpPath = UniqueFilePath(NewWorkspacePath("/Tmp/"..newFileModal.inputField.text .. "/"))

          -- Create a new folder in the tmp directory
          CreateDirectory(tmpPath)

          -- Move the folder from tmp to the new location
          MoveTo(tmpPath, filePath)

        else
          -- Create a new directory
          CreateDirectory(filePath)

        end

        -- Refresh the window to show the new folder
        self:RefreshWindow(true)

        self:SelectFile(filePath)

      end

    end
  )

end

function WorkspaceTool:GetNewFileModal()

  if(self.newFileModal == nil) then

    self.newFileModal = NewFileModal:Init(editorUI)
    self.newFileModal.editorUI = editorUI

  end

  return self.newFileModal

end

function WorkspaceTool:CreateNewProject(name, path)

  if(self.fileTemplatePath == nil) then
    return
  end

  name = name or "Untitled"
  path = UniqueFilePath((path or workspacePath).AppendDirectory(name))

  -- Copy the contents of the template path to the new unique path
  CopyTo(self.fileTemplatePath, path)

  return path

end

function WorkspaceTool:OnNewProject()

  if(PathExists(self.fileTemplatePath) == false) then
    pixelVisionOS:ShowMessageModal(toolName .. " Error", "There is no default template.", 160, false)
    return
  end

  local newFileModal = self:GetNewFileModal()

  newFileModal:SetText("New Project", "NewProject", "Folder Name", true)

  pixelVisionOS:OpenModal(newFileModal,
    function()

      if(newFileModal.selectionValue == true) then

        local newPath = CreateNewProject(newFileModal.inputField.text, currentDirectory)

        RefreshWindow(true)

        self:SelectFile(newPath)
      end

    end
  )

end

function WorkspaceTool:OnNewFile(fileName, ext, type, editable)

  if(type == nil) then
    type = ext
  end

  local newFileModal = self:GetNewFileModal()

  newFileModal:SetText("New ".. type, fileName, "Name " .. type .. " file", editable == nil and true or false)

  pixelVisionOS:OpenModal(newFileModal,
    function()

      if(newFileModal.selectionValue == false) then
        return
      end

      local filePath = UniqueFilePath(self.currentPath.AppendFile(newFileModal.inputField.text .. "." .. ext))

      local tmpPath = self.fileTemplatePath.AppendFile(filePath.EntityName)

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

        tmpPath = self.fileTemplatePath.AppendFile("large.font.png")
        CopyTo(tmpPath, filePath)

      else
        -- print("File not supported")
        -- TODO need to display an error message that the file couldn't be created
        return
      end

      -- Refresh the window to show the new folder
      self:RefreshWindow(true)

      self:SelectFile(filePath)

    end
  )

end

function WorkspaceTool:OnRun()

  -- Only try to run if the directory is a game
  if(self.currentPath == nil or pixelVisionOS:ValidateGameInDir(self.currentPath) == false) then
    return
  end

  -- TODO this should also accept a workspace path?
  LoadGame(self.currentPath.Path)

end

function WorkspaceTool:OnEmptyTrash()

  -- pixelVisionOS:ShowMessageModal("Empty Trash", "Are you sure you want to empty the trash? This can not be undone.", 160, true,
  --     function()
  --         if(pixelVisionOS.messageModal.selectionValue == true) then

  --             -- Get all the files in the trash
  --             filesToCopy = GetEntitiesRecursive(trashPath)

  --             StartFileOperation(trashPath, "delete")

  --         end

  --     end
  -- )

end

function WorkspaceTool:TrashOpen()

  return self.currentPath.Path == self.trashPath.Path

end

function WorkspaceTool:CanEject()

  local selections = self:CurrentlySelectedFiles()

  if(selections == nil) then
    return
  end

  for i = 1, #selections do
    if(selections[i].type == "disk") then
      return true
    end
  end

  return false

end

function WorkspaceTool:OnRename()

  local selections = self:CurrentlySelectedFiles()

  if(selections == nil) then

    -- Return because we can't rename if there are no selections
    return

  elseif(#selections > 1) then

    -- TODO display warning about renaming multiple files

    return

  else

    local newFileModal = self:GetNewFileModal()

    local file = self.files[selections[1]]

    newFileModal:SetText("Rename File", file.name, "Name " .. file.type, true)

    pixelVisionOS:OpenModal(newFileModal,
      function()

        -- Get the new file name from the input field
        local text = newFileModal.inputField.text

        if(newFileModal.selectionValue == true and text ~= file.name) then

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

          local newPath = NewWorkspacePath(file.parentPath.Path .. text)

          if(PathExists(newPath) == false) then
            MoveTo(NewWorkspacePath(file.path), newPath)
          else
            pixelVisionOS:ShowMessageModal("Rename File Error", "A file with the same name already exists.", 160, false)
          end

          -- Refresh the window to show the new folder
          self:RefreshWindow(true)

          self:SelectFile(newPath)
          -- end

        end

      end
    )

  end

end
