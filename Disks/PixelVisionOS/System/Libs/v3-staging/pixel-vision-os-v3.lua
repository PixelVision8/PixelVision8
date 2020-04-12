--
-- Copyright (c) 2017, Jesse Freeman. All rights reserved.
--
-- Licensed under the Microsoft Public License (MS-PL) License.
-- See LICENSE file in the project root for full license information.
--
-- Contributors
-- --------------------------------------------------------
-- This is the official list of Pixel Vision 8 contributors:
--
-- Jesse Freeman - @JesseFreeman
-- Christina-Antoinette Neofotistou - @CastPixel
-- Christer Kaitila - @McFunkypants
-- Pedro Medeiros - @saint11
-- Shawn Rakowski - @shwany
--

PixelVisionOS = {}
PixelVisionOS.__index = PixelVisionOS

LoadScript("pixel-vision-ui-v3")
LoadScript("pixel-vision-os-theme-v1")

-- Pixel Vision OS Components
LoadScript("pixel-vision-os-title-bar-v3")
LoadScript("pixel-vision-os-message-bar-v3")
LoadScript("pixel-vision-os-modal-v3")
LoadScript("pixel-vision-os-message-modal-v3")
LoadScript("pixel-vision-os-color-utils-v2")
LoadScript("pixel-vision-os-undo-v2")
LoadScript("pixel-vision-os-version")

function PixelVisionOS:Init()
  -- Create a new object for the instance and register it
  local _pixelVisionOS = {}
  setmetatable(_pixelVisionOS, PixelVisionOS)

  _pixelVisionOS.editorUI = EditorUI:Init()

  -- Create new title bar instance
  _pixelVisionOS.titleBar = _pixelVisionOS:CreateTitleBar(0, 0)

  -- Create message bar instance
  _pixelVisionOS.messageBar = _pixelVisionOS:CreateMessageBar(7, 230, 60)

  _pixelVisionOS.version = _G["PixelVisionOSVersion"] or "v2.6"

  return _pixelVisionOS

end

function PixelVisionOS:Update(timeDelta)

  -- Update the editorUI
  self.editorUI:Update(timeDelta)

  -- Update the title bar
  self:UpdateTitleBar(self.titleBar, timeDelta)

  -- Update message bar
  self:UpdateMessageBar(self.messageBar)

  -- if(self:IsModalActive() == true) then
  self:UpdateModal(timeDelta)
  -- end

end

function PixelVisionOS:Draw()

  if(self.editorUI.inFocusUI ~= nil and self.editorUI.inFocusUI.toolTip ~= nil) then

    self:DisplayToolTip(self.editorUI.inFocusUI.toolTip)

  else
    self:ClearToolTip()
    -- clear tool tip message

  end

  -- Draw the editor UI
  self.editorUI:Draw()

  -- Draw modals on top
  self:DrawModal()

end



function PixelVisionOS:ShowAboutModal(toolTitle, optionalText, width)

  width = width or 160

  optionalText = optionalText or ""

  local message = "Copyright (c) 2018, Jesse Freeman. All rights reserved. Licensed under the Microsoft Public License (MS-PL) License.\n\n"

  self:ShowMessageModal("About " .. toolTitle .. " " .. self.version, message .. optionalText, width, false, nil, " CLOSE ")

end

function PixelVisionOS:ShowMessageModal(title, message, width, showCancel, onCloseCallback, okLabel, cancelLabel)

  -- Look to see if the modal exists
  if(self.messageModal == nil) then

    -- Create the model
    self.messageModal = MessageModal:Init(title, message, width, showCancel, okLabel, cancelLabel)

  else

    -- If the modal exists, configure it with the new values
    self.messageModal:Configure(title, message, width, showCancel, okLabel, cancelLabel)
  end

  -- Open the modal
  self:OpenModal(self.messageModal, onCloseCallback)

end

function PixelVisionOS:FindEditors()

  local editors = {}

  local paths = 
  {
    NewWorkspacePath("/PixelVisionOS/Tools/"),
  }

  -- Add disk paths
  local disks = DiskPaths();

  for k, v in pairs(disks) do
    local diskPath = NewWorkspacePath(v).AppendDirectory("System").AppendDirectory("Tools")

    table.insert(paths, diskPath)

  end

  local total = #paths

  for i = 1, total do

    local path = paths[i];

    if (PathExists(path)) then

      local folders = GetEntities(path);

      for i = 1, #folders do

        local folder = folders[i]

        if (folder.IsDirectory) then

          if (self:ValidateGameInDir(folder)) then

            local jsonData = ReadJson(folder.AppendFile("info.json"))

            if (jsonData["editType"] ~= nil) then
              --     {
              local split = string.split(jsonData["editType"], ",")
              --
              local totalTypes = #split
              for j = 1, totalTypes do

                local key = split[j];

                editors[key] = folder.Path

              end
            end
          end
        end
      end
    end

  end

  return editors

end

function PixelVisionOS:ValidateGameInDir(workspacePath, requiredFiles)

  if (PathExists(workspacePath) == false) then
    return false
  end

  requiredFiles = requiredFiles or {"data.json", "info.json"}

  -- if(extraFiles ~= nil) then
  --   for i = 1, #extraFiles do
  --     table.insert(requiredFiles, extraFiles[i])
  --   end
  -- end

  local flag = 0

  local total = #requiredFiles

  for i = 1, total do
    if(PathExists(workspacePath.AppendFile(requiredFiles[i])) == true) then
      flag = flag + 1
    end
  end

  return flag == total

end
