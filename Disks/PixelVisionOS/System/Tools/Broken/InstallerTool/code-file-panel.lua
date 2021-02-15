--[[
	Pixel Vision 8 - Debug Tool
	Copyright (C) 2016, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- Create table to store the workspace tool logic
InstallerTool = {}
InstallerTool.__index = InstallerTool

LoadScript("code-installer-modal")

toolName = "Installer"
local maxFilesToDisplay = 7
local maxAboutLines = 8
local fileCheckBoxes = {}
local aboutLines = {}

function InstallerTool:Init()

  -- Create a new table for the instance with default properties
  local _installerTool = {
    toolName = "Installer",
    runnerName = SystemName(),
    rootPath = ReadMetadata("RootPath", "/"),
    rootDirectory = ReadMetadata("directory", nil),
    invalid = true,
    success = false
  }

  -- Reset the undo history so it's ready for the tool
  pixelVisionOS:ResetUndoHistory(self)

  -- Create a global reference of the new workspace tool
  setmetatable(_installerTool, InstallerTool)

  -- Disable the back key in this tool
  --EnableBackKey(false)
  --
  --EnableAutoRun(false)

  -- Create an instance of the Pixel Vision OS
  --pixelVisionOS = PixelVisionOS:Init()

  -- Get a reference to the Editor UI
  --editorUI = pixelVisionOS.editorUI

  -- Make sure the background is masked off
  DrawRect(48, 40, 160 - 8, 8, 0, DrawMode.TilemapCache)
  DrawRect(16, 72, 208, 64, 0, DrawMode.TilemapCache)
  DrawRect(16, 168, 228 - 20, 56, 11, DrawMode.TilemapCache)

  pixelVisionOS:ChangeTitle(toolName, "toolbaricontool")

  if(ConfigureInstaller ~= nil) then

    ConfigureInstaller()

  else

    -- Get a list of all the editors
    local editorMapping = pixelVisionOS:FindEditors()

    -- Find the json editor
    textEditorPath = editorMapping["json"]

    local menuOptions =
    {
      -- About ID 1
      {name = "About", action = function() pixelVisionOS:ShowAboutModal(toolName) end, toolTip = "Learn about PV8."},
      {divider = true},
      {name = "Edit Script", enabled = textEditorPath ~= nil, action = OnEditScript, toolTip = "Edit the raw installer script file."}, -- Reset all the values
      {name = "Reset", action = OnReset, key = Keys.R, toolTip = "Revert the installer to its default state."}, -- Reset all the values
      {divider = true},
      {name = "Quit", key = Keys.Q, action = OnQuit, toolTip = "Quit the current game."}, -- Quit the current game
    }

    pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")

    -- Change the title
    pixelVisionOS:ChangeTitle(toolName, "toolbaricontool")

    rootDirectory = ReadMetadata("directory", nil)

    -- Get the target file
    targetFile = ReadMetadata("file", nil)

    if(targetFile ~= nil) then

      self:LoadInstallScript(ReadTextFile(targetFile))

      self:ConfigureToolUI()

    else

      pixelVisionOS:ChangeTitle(toolName, "toolbaricontool")



      pixelVisionOS:ShowMessageModal(toolName .. " Error", "The tool could not load without a reference to a file to edit.", 160, false,
              function()
                QuitCurrentTool()
              end
      )
    end

  end
  
  
  -- Return the draw tool data
  return _installerTool

end

function InstallerTool:Shutdown()
  
  
end





---- The Init() method is part of the game's lifecycle and called a game starts. We are going to
---- use this method to configure background color, ScreenBufferChip and draw a text box.
--function Init()
--
--  -- Disable the back key in this tool
--  EnableBackKey(false)
--
--  EnableAutoRun(false)
--
--  -- Create an instance of the Pixel Vision OS
--  pixelVisionOS = PixelVisionOS:Init()
--
--  -- Get a reference to the Editor UI
--  editorUI = pixelVisionOS.editorUI
--
--  -- Make sure the background is masked off
--  DrawRect(48, 40, 160 - 8, 8, 0, DrawMode.TilemapCache)
--  DrawRect(16, 72, 208, 64, 0, DrawMode.TilemapCache)
--  DrawRect(16, 168, 228 - 20, 56, 11, DrawMode.TilemapCache)
--
--  pixelVisionOS:ChangeTitle(toolName, "toolbaricontool")
--
--  if(ConfigureInstaller ~= nil) then
--
--    ConfigureInstaller()
--
--  else
--
--    -- Get a list of all the editors
--    local editorMapping = pixelVisionOS:FindEditors()
--
--    -- Find the json editor
--    textEditorPath = editorMapping["json"]
--
--    local menuOptions =
--    {
--      -- About ID 1
--      {name = "About", action = function() pixelVisionOS:ShowAboutModal(toolName) end, toolTip = "Learn about PV8."},
--      {divider = true},
--      {name = "Edit Script", enabled = textEditorPath ~= nil, action = OnEditScript, toolTip = "Edit the raw installer script file."}, -- Reset all the values
--      {name = "Reset", action = OnReset, key = Keys.R, toolTip = "Revert the installer to its default state."}, -- Reset all the values
--      {divider = true},
--      {name = "Quit", key = Keys.Q, action = OnQuit, toolTip = "Quit the current game."}, -- Quit the current game
--    }
--
--    pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")
--
--    -- Change the title
--    pixelVisionOS:ChangeTitle(toolName, "toolbaricontool")
--
--    rootDirectory = ReadMetadata("directory", nil)
--
--    -- Get the target file
--    targetFile = ReadMetadata("file", nil)
--
--    if(targetFile ~= nil) then
--
--      self:LoadInstallScript(ReadTextFile(targetFile))
--
--      self:ConfigureToolUI()
--
--    else
--
--      pixelVisionOS:ChangeTitle(toolName, "toolbaricontool")
--
--
--
--      pixelVisionOS:ShowMessageModal(toolName .. " Error", "The tool could not load without a reference to a file to edit.", 160, false,
--        function()
--          QuitCurrentTool()
--        end
--      )
--    end
--
--  end
--
--
--
--end


function InstallerTool:ConfigureToolUI()
  if(variables["readme"] ~= nil) then

    local wrap = WordWrap(variables["readme"], 52)
    aboutLines = SplitLines(wrap)


    if(#aboutLines < maxAboutLines) then
      maxAboutLines = #aboutLines
    end

  end

  installerRoot = variables["dir"] or "Workspace"

  -- Update file paths
  -- for i = 1, #filePaths do
  --   filePaths[i][3] = "/" .. installerRoot .. filePaths[i][1]
  --   print("file", filePaths[i][3], "to", filePaths[i][1])
  --   -- filePaths[i][3] = filePaths[i][1]
  -- end

  nameInputData = editorUI:CreateInputField({x = 48, y = 40, w = 152}, installerRoot, "Enter in a file name to this string input field.", "file")

  local startY = 168

  -- Need to see if there are enough files to display
  if(#filePaths < maxFilesToDisplay) then
    maxFilesToDisplay = #filePaths

    -- TODO disable scroller
  end

  for i = 1, maxFilesToDisplay do
    local tmpCheckbox = editorUI:CreateToggleButton({x = 16, y = startY, w = 8, h = 8}, "checkbox", "Toggles doing a clean install.")
    tmpCheckbox.onAction = function(value)

      if(Key(Keys.LeftShift) or Key(Keys.RightShift)) then

        -- Loop through all of the files
        for i = 1, #filePaths do

          -- Change all of the file values
          filePaths[i][2] = value

        end

        self:DrawFileList(fileListOffset)

      else

        -- Change a single file value
        filePaths[i + fileListOffset][2] = value
      end

    end

    startY = startY + 8

    table.insert(fileCheckBoxes, tmpCheckbox)

  end

  aboutSliderData = editorUI:CreateSlider({x = 227, y = 68, w = 16, h = 72}, "vsliderhandle", "Scroll to see more of the about text.")
  aboutSliderData.onAction = OnAboutValueChange

  editorUI:Enable(aboutSliderData, #aboutLines > maxAboutLines)

  fileSliderData = editorUI:CreateSlider({x = 224, y = 168, w = 16, h = 56}, "vsliderhandle", "Scroll to see the more files to install.")
  fileSliderData.onAction = OnFileValueChange

  editorUI:Enable(fileSliderData, #filePaths > maxFilesToDisplay)

  installButtonData = editorUI:CreateButton({x = 208, y = 32}, "installbutton", "Run the installer.")
  installButtonData.onAction = function(value)

    filesToCopy = {}

    for i = 1, #filePaths do
      if(filePaths[i][2] == true) then

        table.insert(filesToCopy, {src = filePaths[i][1], dest = filePaths[i][3]})
      end
    end

    -- Create  the install path without the last slash
    local installPath = "/Workspace" .. (nameInputData.text:lower() ~= "workspace" and "/"..nameInputData.text or "")

    pixelVisionOS:ShowMessageModal("Install Files", "Are you sure you want to install ".. #filesToCopy .." items in '".. installPath .."/'? This will overwrite any existing files and can not be undone.", 160, true,
      function()

        if(pixelVisionOS.messageModal.selectionValue == true) then

          self:OnInstall(installPath)

        end

      end
    )

  end

  cleanCheckboxData = editorUI:CreateToggleButton({x = 176, y = 56, w = 8, h = 8}, "radiobutton", "Toggles doing a clean build and removes all previous builds.")

  -- editorUI:ToggleButton(cleanCheckboxData, gameEditor:ReadMetadata("clear", "false") == "true", false)

  cleanCheckboxData.onAction = function(value)

    if(value == false) then
      -- InvalidateData()
      return
    end

    pixelVisionOS:ShowMessageModal("Warning", "Are you sure you want to do a clean install? The root directory will be removed before the installer copies over the files. This can not be undone.", 160, true,
      function()

        if(pixelVisionOS.messageModal.selectionValue == false) then

          -- Force the checkbox back into the false state
          editorUI:ToggleButton(cleanCheckboxData, false, false)

        else
          -- Force the checkbox back into the false state
          editorUI:ToggleButton(cleanCheckboxData, true, false)
        end

        -- Force the button to redraw since restoring the modal will show the old state
        editorUI:Invalidate(cleanCheckboxData)
        -- InvalidateData()
      end
    )

  end

  -- Reset list
  self:DrawFileList()
  self:DrawAboutLines()
end

function InstallerTool:LoadInstallScript(rawData)

  -- local  = ReadTextFile(path)

  biosProperties = {}
  variables = {}
  filePaths = {}

  for s in rawData:gmatch("[^\r\n]+") do

    local type = string.sub(s, 1, 1)

    if(type == "/") then

      local split = string.split(s, "|")

      if(PathExists(NewWorkspacePath(rootDirectory .. string.sub(split[1], 2)))) then
        -- print("Path", split[1], "to", split[2])

        -- Add the path to the list
        table.insert(filePaths, {split[1], true, split[2] == nil and split[1] or split[2]})
      end
    elseif(type == "$") then

      -- Need to check if this is a bios property or a variable
      if(string.sub(s, 1, 5) == "$bios") then
        local split = string.split(string.sub(s, 7, #s), "|")

        -- print("bios", split[1], split[2])

        table.insert(biosProperties, split)

      else

        local split = string.split(string.sub(s, 2, #s), "=")

        -- print("var", split[1], split[2])
        variables[split[1]] = split[2]

      end

    end

    -- print("Type", type, s)

    -- table.insert(lines, s)
  end

end

function InstallerTool:OnAboutValueChange(value)

  local offset = math.ceil((#aboutLines - maxAboutLines - 1) * value)

  self:DrawAboutLines(offset)

end

function InstallerTool:DrawAboutLines(offset)

  DrawRect(16, 64 + 8, 208, 64, 0, DrawMode.TilemapCache)
  offset = offset or 0

  for i = 1, maxAboutLines do

    local line = aboutLines[i + offset]

    line = string.rpad(line, 52, " "):upper()
    DrawText(line, 16, 64 + (i * 8), DrawMode.TilemapCache, "medium", 15, - 4)

  end

end

function InstallerTool:OnFileValueChange(value)

  local offset = math.ceil((#filePaths - maxFilesToDisplay) * value)

  self:DrawFileList(offset)

end

function InstallerTool:DrawFileList(offset)

  fileListOffset = offset or 0

  DrawRect(24, 168, 200, 56, 11, DrawMode.TilemapCache)

  for i = 1, maxFilesToDisplay do

    local file = filePaths[i + fileListOffset] or ""
    local fileName = file[1]
    local checkValue = file[2]

    editorUI:ToggleButton(fileCheckBoxes[i], checkValue, false)

    -- TODO need to check the size of the name

    if(#fileName > 49) then
      fileName = fileName:sub(1, 49 - 3) .. "..."
    end

    fileName = string.rpad(fileName, 200, " "):upper()

    DrawText(fileName, 25, 160 + (i * 8), DrawMode.TilemapCache, "small", 0, - 4)

  end

end

function InstallerTool:OnInstall(rootPath)

  if (cleanCheckboxData.selected == true) then
    local destPath = NewWorkspacePath(rootPath.."/")
    if(PathExists(destPath)) then
      Delete(destPath)
    end
  end

  installing = true

  installingTime = 0
  installingDelay = .1
  installingCounter = 0
  installingTotal = #filesToCopy

  installRoot = rootPath

  -- print("Install Root", installRoot)

end

function InstallerTool:OnInstall()

  -- print("Next step")
  -- Look to see if the modal exists
  if(installingModal == nil) then

    -- Create the model
    installingModal = InstallerModal:Init("Installing", editorUI)

    -- Open the modal
    pixelVisionOS:OpenModal(installingModal)



    -- else
    --
    --   -- If the modal exists, configure it with the new values
    --   installingModal:Configure("Installing", "Installing...", 160)
  end

  installingCounter = installingCounter + 1

  local paths = filesToCopy[installingCounter]

  if(paths ~= nil) then

    local tmpRoot = NewWorkspacePath(installRoot)

    local dest = NewWorkspacePath(string.starts(paths.dest, "../") and tmpRoot.ParentPath.Path .. paths.dest:sub(4) or installRoot .. paths.dest)

    -- Combine the root directory and path but remove the first slash from the path
    local path = NewWorkspacePath(rootDirectory .. string.sub(paths.src, 2))

    local parentPath = dest

    -- Need to make sure the directory path exists before doing the copy
    if(dest.IsDirectory == false) then
      parentPath = dest.ParentPath
    end

    -- Test if the parent path exists
    if(PathExists(parentPath) == false) then

      -- Recursively create the parent path
      CreateDirectory(parentPath)

    end
    -- print("Copying", path, dest)
    if(PathExists(path)) then


      CopyTo(path, dest)
    end

    installingModal:UpdateMessage(installingCounter, installingTotal)

  end

  if(installingCounter >= installingTotal) then
    installingDelay = .5
  end

end

function InstallerTool:OnInstallComplete()

  installing = false

  
  pixelVisionOS:CloseModal()

  -- Write to bios
  for i = 1, #biosProperties do

    local prop = biosProperties[i]
    -- print("Write to bios", prop[1], prop[2])
    WriteBiosData(prop[1], prop[2])
  end

  RebuildWorkspace()

  pixelVisionOS:ShowMessageModal("Installation Complete", installingCounter .. " files have been installed to '" .. installRoot .. "/'. Ready to exit the installer?", 160, true,

      function()
        
        if(pixelVisionOS.messageModal.selectionValue == true) then
  
          -- Create meta data with the path the workspace should load up
          local metaData = {
            overrideLastPath = variables["openPath"] or "/Workspace/"
          }

          -- Quit the tool and pass in the new metadata
          QuitCurrentTool(metaData)
        end

      end
  )

  
  -- QuitCurrentTool(metaData)
  
end

-- The self:Update() method is part of the game's life cycle. The engine calls self:Update() on every frame
-- before the Draw() method. It accepts one argument, timeDelta, which is the difference in
-- milliseconds since the last frame.
function InstallerTool:Update(timeDelta)

  ---- Convert timeDelta to a float
  --timeDelta = timeDelta / 1000
  --
  ---- This needs to be the first call to make sure all of the OS and editor UI is updated first
  --pixelVisionOS:Update(timeDelta)

  -- Only update the tool's UI when the modal isn't active
  if(pixelVisionOS:IsModalActive() == false) then

    editorUI:UpdateInputField(nameInputData)

    editorUI:UpdateButton(installButtonData)
    editorUI:UpdateButton(cleanCheckboxData)

    for i = 1, maxFilesToDisplay do
      editorUI:UpdateButton(fileCheckBoxes[i], tmpCheckbox)
    end

    editorUI:UpdateSlider(aboutSliderData)
    editorUI:UpdateSlider(fileSliderData)

  end

  if(installing == true) then


    installingTime = installingTime + timeDelta

    if(installingTime > installingDelay) then
      installingTime = 0


      OnInstallNextStep()

      if(installingCounter > installingTotal) then

        self:OnInstallComplete()

      end

    end


  end

end

-- The Draw() method is part of the game's life cycle. It is called after self:Update() and is where
-- all of our draw calls should go. We'll be using this to render sprites to the display.
--function Draw()
--
--  -- We can use the RedrawDisplay() method to clear the screen and redraw the tilemap in a
--  -- single call.
--  RedrawDisplay()
--
--  -- This is used by the OS installer to shutdown directly from the install if no OS is installed
--  if(shuttingDown == true) then
--    return
--  end
--
--  -- The UI should be the last thing to draw after your own custom draw calls
--  pixelVisionOS:Draw()
--
--end

function InstallerTool:OnQuit()

  -- Quit the tool
  QuitCurrentTool()

end

function InstallerTool:OnEditScript()


  pixelVisionOS:ShowMessageModal("Edit Script File", "You are about to leave the installer and edit the raw installer.txt file. Are you sure you want to do this?", 160, true,
    function()

      if(pixelVisionOS.messageModal.selectionValue == true) then
        -- Quit the tool
        self:EditInstallerScript()

      end

    end
  )


end

function InstallerTool:EditInstallerScript()

  local metaData = {
    directory = rootDirectory,
    file = rootDirectory .. "installer.txt",
  }

  LoadGame(textEditorPath, metaData)


end

function InstallerTool:OnReset()

  pixelVisionOS:ShowMessageModal("Reset Installer", "Do you want to reset the installer to its default values?", 160, true,
    function()
      if(pixelVisionOS.messageModal.selectionValue == true) then

        for i = 1, #filePaths do
          filePaths[i][2] = true
        end

        self:DrawFileList(fileListOffset)
      end
    end
  )

end
