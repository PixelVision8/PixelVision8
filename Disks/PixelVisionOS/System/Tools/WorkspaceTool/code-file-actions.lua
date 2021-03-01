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

-- Helper utility to delete files by moving them to the throw out
function WorkspaceTool:DeleteFile(path)

  -- Create the base throw out path for the file
  local newPath = self.trashPath

  -- See if this is a directory or a file and add the entity name
  if(path.IsDirectory) then
    newPath = newPath.AppendDirectory(path.EntityName)
  else
    newPath = newPath.AppendFile(path.EntityName)
  end

  -- Make sure the path is unique
  newPath = UniqueFilePath(newPath)

  -- Move to the new throw out path
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
  -- self.fileActionSrc = self.currentPath

  self.targetFiles = {}

  -- Loop through all of the selections
  for i = 1, #selections do

    local srcPath = self.files[selections[i]].path

    -- Make sure the selected directory is included
    table.insert(self.targetFiles, srcPath)

  end

  -- print("Files", dump(self.targetFiles))

  -- Always make sure anything going into the throw out has a unique file name
  self:StartFileOperation(self.currentPath, self.trashPath, "throw out")

end

function WorkspaceTool:StartFileOperation(srcPath, destPath, action)

  -- print("StartFileOperation", srcPath, destPath, action)

  -- Test to see if the action is delete
  if(action == "delete") then

    -- Go right into the file action
    self:OnRunFileAction(srcPath, destPath, action)

    -- Exit out of the function
    return

  end

  -- Set a modal flag so we know what warning to display
  local fileActionFlag = 0

  -- TODO need to loop through all of the files do pre-check
    for i = 1, #self.targetFiles do

      -- Get the file to test
      local filePath = self.targetFiles[i]

      -- Figure out the final path for the file
      local finalDestPath = NewWorkspacePath(destPath.Path .. filePath.Path:sub(#srcPath.Path + 1))

      -- print("filePath", filePath.Path, destPath.Path, finalDestPath.Path)

      if(filePath.isDirectory and filePath.Path == destPath.Path) then

        fileActionFlag = 1
        break

      elseif(filePath.IsChildOf(finalDestPath)) then
        
        -- Set flag to 2 for a child path error
        fileActionFlag = 2
        break

      -- Test the final path only if it's not being moved to the throw out
      elseif(PathExists(finalDestPath) and action ~= "throw out") then

        -- Set flag to 3 for a douplicate file warning
        fileActionFlag = 3

        break

        -- TODO need to make sure you don't copy a disk into another disk

      end

    end

    -- print("File Action Flag", fileActionFlag)

  if(fileActionFlag == 1) then

    pixelVisionOS:ShowMessageModal(
      "Workspace Path Conflict",
      "Can't perform a file action on a path that is the same as the destination path.",
      144, 
      {
        {
            name = "modalokbutton",
            action = function(target)
              target.onParentClose()
              self:CancelFileActions()
            end,
            key = Keys.Enter,
            tooltip = "Press 'enter' exit this action"
        }
      }
    )
    return

  elseif(fileActionFlag == 2) then

    pixelVisionOS:ShowMessageModal(
      "Workspace Path Conflict",
      "Can't perform a file action on a path that is the child of the destination path.",
      144, 
      {
        {
            name = "modalokbutton",
            action = function(target)
              target.onParentClose()
              self:CancelFileActions()
            end,
            key = Keys.Enter,
            tooltip = "Press 'enter' exit this action"
        }
      }
    )
    return

  elseif(fileActionFlag == 3) then

    -- local duplicate = destPath.Path == self.targetFiles[1].Path


    local buttons = 
    {
        {
            name = "modalyesbutton",
            action = function(target)
                target.onParentClose()
                self:OnRunFileAction(srcPath, destPath, action)
            end,
            key = Keys.Enter,
            tooltip = "Press 'enter' to reset mapping to the default value"
        },
        -- {
        --     name = "modalnobutton",
        --     action = function(target)
        --         target.onParentClose()
        --         -- TODO remove duplicate files
        --     end,
        --     key = Keys.N,
        --     tooltip = "Press 'n' to not ignore duplicates"
        -- },
        {
          name = "modalcancelbutton",
          action = function(target)
              target.onParentClose()
              self:CancelFileActions()
          end,
          key = Keys.Escape,
          tooltip = "Press 'esc' to cancel this action"
      }
        -- TODO should there be a cancel option?
    }

    -- Ask if the file first item should be duplicated
    pixelVisionOS:ShowMessageModal(
      "Workspace Path Conflict",

      -- TODO this should give you all 3 options (duplicate, replace, cancel)
      "Looks like there is an existing file with the same name in '".. destPath.Path .. "'. Do you want to replace any duplicates?",
      200,
      buttons
    )

  else

    local buttons = 
    {
        {
            name = "modalyesbutton",
            action = function(target)
                target.onParentClose()
                self:OnRunFileAction(srcPath, destPath, action)
            end,
            key = Keys.Enter,
            tooltip = "Press 'enter' to reset mapping to the default value"
        },
        {
            name = "modalnobutton",
            action = function(target)
                target.onParentClose()
                self:CancelFileActions()
            end,
            key = Keys.N,
            tooltip = "Press 'n' to not ignore duplicates"
        }
      --   ,
      --   {
      --     name = "modalcancelbutton",
      --     action = function(target)
      --         target.onParentClose()
      --     end,
      --     key = Keys.Escape,
      --     tooltip = "Press 'esc' to cancel this action"
      -- }
        -- TODO should there be a cancel option?
    }

    pixelVisionOS:ShowMessageModal(
      "Workspace ".. action .." Action",
      string.format("Do you want to %s %s?", action, #self.targetFiles > 1 and "these files" or "this file"),
      160,
      buttons
    )

  end

end

function WorkspaceTool:OnRunFileAction(srcPath, destPath, action, duplicate)

  -- If the duplicate flag is not set, test to see if this action is to move to the trash
  duplicate = duplicate or action == "throw out"

  -- Replace the throw out action with move since this was only used to display a friendly action lable
  if(action == "throw out") then
    action = "move"
  end

  -- local parentPath = self.targetFiles[1].isDirectory and self.targetFiles[1] or self.targetFiles[i].parentPath

  -- print("path", parentPath, self.targetFiles[1].Path)
  
  -- Build the arguments
  local args = { srcPath, destPath, action, duplicate}

  for i = 1, #self.targetFiles do

    table.insert(args, self.targetFiles[i])

  end

  -- print("args", dump(args))

  local success = RunBackgroundScript("code-process-file-actions.lua", args)

  -- print("success", success)

  if(success) then

    if(self.progressModal == nil) then

      -- print("ProgressModal", ProgressModal == nil, editorUI == nil)

      -- Create the model
      self.progressModal = ProgressModal:Init("File Action ", 168)

      self.progressModal.fileAction = action
      self.progressModal.totalFiles = #self.targetFiles
    end

    -- Open the modal
    pixelVisionOS:OpenModal(self.progressModal, function() self:CancelFileActions() end)

    pixelVisionOS:RegisterUI({name = "ProgressUpdate"}, "UpdateFileActionProgress", self, true)

  end

  self:UpdateFileActionProgress()


end

function WorkspaceTool:UpdateFileActionProgress()

  -- print("UpdateFileActionProgress running", IsExporting())

  -- Check to see if exporting is done
  if(IsExporting() == false) then

    pixelVisionOS:CloseModal()

    self.progressModal = nil

    -- Refresh the window and get the new file list
    self:RefreshWindow(true)

    self.selectedFiles = nil

    -- Remove the callback from the UI update loop
    pixelVisionOS:RemoveUI("ProgressUpdate")

    -- Check to see if we should select anything
    if(self.autoSelect == true) then
      
      self.autoSelect = false

      for i = 1, #self.files do
        
        for j = 1, #self.targetFiles do
            if(self.files[i].fullName == self.targetFiles[j].EntityName) then
              self.files[i].selected = true
            end
        end

      end

    end
    
    -- Exit out of the function
    return

  end

  -- Get the current percentage
  local percent = ReadExportPercent()/100

  local fileActionActiveTotal = tonumber(BackgroundScriptData("tmpFileCount"))
  local fileActionCounter = math.floor(fileActionActiveTotal * percent)
  local pad = #tostring(fileActionActiveTotal)

  local message = string.format("%s %0" .. pad .. "d of %0" .. pad .. "d.\n\n\nDo not restart or shut down Pixel Vision 8.", self.progressModal.fileAction, fileActionCounter, fileActionActiveTotal)

  self.progressModal:UpdateMessage(message, percent)

end

function WorkspaceTool:CanCopy(file)

  return (file.name ~= "Run" and file.type ~= "updirectory" and file.type ~= "trash" and file.type ~= "workspace")

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

  -- Get the current selections
  local selections = self:CurrentlySelectedFiles()

  -- Make sure there are selections
  if(selections ~= nil) then

    -- local clipboardData = {
    --   srcPath = self.currentPath,
    --   type = "paths", 
    --   value = {}
    -- }

    local copyString = "paths:" .. self.currentPath.Path

    local totalFiles = 0

    for i = 1, #selections do
        
      local tmpItem = self.files[selections[i]]

      if(self:CanCopy(tmpItem)) then

        copyString = copyString .. ","..tmpItem.path.Path

        -- Set the flag to true if we can copy at least one file
        totalFiles = totalFiles + 1
        
        -- table.insert(clipboardData.value, tmpItem.path)

      end

    end

    if(totalFiles > 0) then
      
      -- print("CopyString", copyString)

      -- Save the clipboard data
      pixelVisionOS:SystemCopy(copyString)

      -- Toggle the paste menu item to true so it can be used
      pixelVisionOS:EnableMenuItemByName(PasteShortcut, true)

      -- local total = #clipboardData.value

      pixelVisionOS:DisplayMessage(totalFiles .. " file" .. (totalFiles == 1 and " has" or "s have") .." been copied.", 2)
      
      -- Exit out of the function so the paste menu isn't set back to false
      return
      
    end

    pixelVisionOS:EnableMenuItemByName(PasteShortcut, false)

  end

end

function WorkspaceTool:OnPaste(dest)

  -- if(pixelVisionOS:ClipboardFull() == false) then
  --   return
  -- end

  -- Get the destination directory
  dest = dest or self.currentPath

  if(PathExists(dest)) then

    local data = pixelVisionOS:SystemPaste()

    if(data ~= nil or  data ~= "" and string.starts(data, "path:")) then

    local paths = string.split(data:sub(7), ",")
    -- print("PastPaths", dump(paths))
    
    local srcPath = NewWorkspacePath(paths[1])

    -- if(data.type == "paths") then

    --   local paths = data.value

      self.targetFiles = {}

      local tmpPath = nil

      for i = 1, #paths do
        
        tmpPath = NewWorkspacePath(paths[i])

        if(PathExists(tmpPath)) then
          table.insert(self.targetFiles, tmpPath)
          -- print("paste", paths[i])
        end

      end

      if(#self.targetFiles) then

        self:StartFileOperation(srcPath, dest, "copy")

        self.autoSelect = true

      end

      

    -- end
    end

  end

  pixelVisionOS:ClearClipboard()

  pixelVisionOS:EnableMenuItemByName(srcPath, PasteShortcut, false)
  
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
  path = UniqueFilePath((path or self.workspacePath).AppendDirectory(name))

  -- Copy the contents of the template path to the new unique path
  CopyTo(self.fileTemplatePath, path)

  self:CreateNewCodeFile(path)

  -- TODO need to replace the version number in the info file with the current version

  return path

end

function WorkspaceTool:OnNewProject()

  if(PathExists(self.fileTemplatePath) == false) then
    pixelVisionOS:ShowMessageModal(toolName .. " Error", "There is no default template.", 160)
    return
  end

  local newFileModal = self:GetNewFileModal()

  newFileModal:SetText("New Project", "NewProject", "Folder Name", true)

  pixelVisionOS:OpenModal(newFileModal,
    function()

      if(newFileModal.selectionValue == true) then

        local newPath = self:CreateNewProject(newFileModal.inputField.text, self.currentPath)

        self:RefreshWindow(true)

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

        -- TODO need to see if a code.lua file exists first and decide to copy template over or make an empty file

        if(PathExists(self.currentPath.AppendFile("code.lua"))) then

          SaveText(filePath, "-- Empty code file")

        else

          CopyTo(tmpPath, filePath)
        
        end

      elseif(ext == "cs") then

        -- TODO need to see if a code.lua file exists first and decide to copy template over or make an empty file

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

        tmpPath = NewWorkspacePath("/App/Fonts/large.font.png")
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

  local buttons = 
  {
      {
          name = "modalyesbutton",
          action = function(target)
              target.onParentClose()
              -- Get all the files in the throw out
              self.targetFiles = GetEntitiesRecursive(self.trashPath)

              self:StartFileOperation(self.currentPath, self.trashPath, "delete")

              self:RefreshWindow(false)
          end,
          key = Keys.Enter,
          tooltip = "Press 'enter' to reset mapping to the default value"
      },
      {
          name = "modalnobutton",
          action = function(target)
              target.onParentClose()
          end,
          key = Keys.Escape,
          tooltip = "Press 'esc' to avoid making any changes"
      }
  }

  pixelVisionOS:ShowMessageModal("Empty Trash", "Are you sure you want to empty the throw out? This can not be undone.", 160, buttons)

end

function WorkspaceTool:TrashOpen()

  -- print("test", self.currentPath.Path, self.trashPath.Path)

  return string.starts(self.currentPath.Path, self.trashPath.Path) 

end

function WorkspaceTool:CanEject()

  local selections = self:CurrentlySelectedFiles()
  -- print("selections", dump(selections))

  if(selections == nil) then
    return
  end
  
  for i = 1, #selections do
    if(self.files[selections[i]].type == "disk") then
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
            pixelVisionOS:ShowMessageModal("Rename File Error", "A file with the same name already exists.", 160)
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
