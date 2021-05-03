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

LoadScript("pixel-vision-ui-v2")

-- Pixel Vision OS Components
LoadScript("pixel-vision-os-title-bar-v2")
LoadScript("pixel-vision-os-message-bar-v2")
LoadScript("pixel-vision-os-modal-v2")
LoadScript("pixel-vision-os-message-modal-v5")
LoadScript("pixel-vision-os-color-utils-v3")
LoadScript("pixel-vision-os-undo-v2")
LoadScript("pixel-vision-os-clipboard-v2")
LoadScript("pixel-vision-os-export-game-v1")
LoadScript("pixel-vision-os-input-v1")
LoadScript("pixel-vision-os-version")

function PixelVisionOS:Init()

    -- Get a global reference to the Editor UI
    _G["editorUI"] = EditorUI:Init()

    -- Create a new object for the instance and register it
    local _pixelVisionOS = {
        editorUI = editorUI,
        version = _G["PixelVisionOSVersion"] or "v2.6",
        uiComponents = {},
        uiTotal = 0, 
        displayFPS = false
    }
    setmetatable(_pixelVisionOS, PixelVisionOS)

    -- Create new title bar instance
    _pixelVisionOS.titleBar = _pixelVisionOS:CreateTitleBar(0, 0)

    -- Create message bar instance
    _pixelVisionOS.messageBar = _pixelVisionOS:CreateMessageBar(7, 230, 60)

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

    -- Loop through all of the registered UI and update them
    for i = 1, self.uiTotal do

        -- Get a reference to the UI data
        local ref = self.uiComponents[i]

        if(ref ~= nil) then

            -- Only update UI when the modal is not active
            if((ref.cycle == "update" and pixelVisionOS:IsModalActive() == false) or (ref.cycle == "update" and pixelVisionOS:IsModalActive() == true and ref.ignoreModal == true) ) then

                -- Call the UI scope's update and pass back in the UI data
                ref.uiScope[ref.uiUpdate](ref.uiScope, ref.uiData)

            end

        end

    end

end

function PixelVisionOS:Draw()

    if(self.editorUI.inFocusUI ~= nil and self.editorUI.inFocusUI.toolTip ~= nil) then
        self:DisplayToolTip(self.editorUI.inFocusUI.toolTip)
    else
        self:ClearToolTip()
    end

    -- Loop through all of the registered UI and update them
    for i = 1, self.uiTotal do

        -- Get a reference to the UI data
        local ref = self.uiComponents[i]

        if(ref ~= nil) then

            -- Only update UI when the modal is not active
            if((pixelVisionOS:IsModalActive() == false or ref.ignoreModal) and ref.cycle == "draw") then

                -- Call the UI scope's update and pass back in the UI data
                ref.uiScope[ref.uiUpdate](ref.uiScope, ref.uiData)

            end

        end

    end

    -- We manually call draw on the message bar since it can be updated at any point outside of its own update call
    self:DrawMessageBar(self.messageBar)


    
    -- Draw modals on top
    self:DrawModal()

    --if(self.displayFPS == true) then

    local fps = ReadFPS()

    local color = 7

    if(fps < 30) then
        color = 9
    elseif(fps <= 40 and fps >= 30) then
        color = 14
    end

    DrawText(tostring(fps), Display().x - 10, Display().y - 10, DrawMode.Sprite, "medium", color, -4)
    --end

    -- Draw the editor UI
    self.editorUI:Draw()


end



function PixelVisionOS:ShowAboutModal(toolTitle, optionalText, width)

    width = width or 160

    optionalText = optionalText or ""

    local message = "Copyright (c) 2018, Jesse Freeman. All rights reserved. Licensed under the Microsoft Public License (MS-PL) License.\n\n"

    self:ShowMessageModal("About " .. toolTitle .. " " .. self.version, message .. optionalText, width)

end

function PixelVisionOS:ShowMessageModal(title, message, width, buttons) --showCancel, onCloseCallback, okButtonSpriteName, cancelButtonSpriteName)

    -- Look to see if the modal exists
    if(self.messageModal == nil) then

        -- Create the model
        self.messageModal = MessageModal:Init(title, message, width, buttons)

        -- Pass a reference of the editorUI to the modal
        self.messageModal.editorUI = self.editorUI
    -- end
    else
        -- If the modal exists, configure it with the new values
        self.messageModal:Configure(title, message, width, buttons)--showCancel, okButtonSpriteName, cancelButtonSpriteName)
    end

    -- Open the modal
    self:OpenModal(self.messageModal, onCloseCallback)

end

function PixelVisionOS:ShowSaveModal(title, message, width, onAccept, onDecline, onCancel) --showCancel, onCloseCallback, okButtonSpriteName, cancelButtonSpriteName)

    local buttons = 
    {
      {
        name = "modalyesbutton",
        action = onAccept,
        key = Keys.Enter,
        tooltip = "Press 'enter' to save"
      },
      {
        name = "modalnobutton",
        action = onDecline,
        key = Keys.N,
        tooltip = "Press 'n' to not save"
      }
    }

    if(onCancel ~= nil) then
        table.insert(
            buttons, 
            {
                name = "modalcancelbutton",
                action = onCancel,
                key = Keys.Escape,
                tooltip = "Press 'escape' to cancel"
            }
        )
    end

    -- Look to see if the modal exists
    if(self.messageModal == nil) then

        -- Create the model
        self.messageModal = MessageModal:Init(title, message, width, buttons)

        -- Pass a reference of the editorUI to the modal
        self.messageModal.editorUI = self.editorUI
    -- end
    else
        -- If the modal exists, configure it with the new values
        self.messageModal:Configure(title, message, width, buttons)--showCancel, okButtonSpriteName, cancelButtonSpriteName)
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

    local flag = 0

    local total = #requiredFiles

    for i = 1, total do
        if(PathExists(workspacePath.AppendFile(requiredFiles[i])) == true) then
            flag = flag + 1
        end
    end

    return flag == total

end

function PixelVisionOS:RegisterUI(data, updateCall, scope, ignoreModal, cycle)

    scope = scope or self
    ignoreModal = ignoreModal or false
    cycle = cycle or "update"
  
    -- Try to remove an existing instance of the component
    self:RemoveUI(data.name)
  
    table.insert(self.uiComponents, {uiData = data, uiUpdate = updateCall, uiScope = scope, ignoreModal = ignoreModal, cycle = cycle})
  
    self.uiTotal = #self.uiComponents
  
    -- Return an instance of the component
    return data
  
  end
  
  function PixelVisionOS:RemoveUI(name)
  
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
  
    -- print("Remove", removeItem, "total", self.uiTotal)
  
    for i = 1, #self.uiComponents do
      print("Left over", self.uiComponents[i].uiData.name)
    end
  
  end

-- Shared function to help load a custom icon into memory
function PixelVisionOS:LoadCustomIcon(iconWorkspacePath, iconUpName, iconSelectedName)

    iconUpName = iconUpName or "filecustomiconup"
    iconSelectedName = iconSelectedName or "filecustomiconselectedup"
    
    -- We'll return this value at the end of the function
    local success = false
    
    
    -- Look to see if the file exisits
    if(PathExists(iconWorkspacePath)) then
        
        -- Build a list of system colors to use as a reference from the default OS colors
        local systemColors = {}
        for i = 1, 16 do
            table.insert(systemColors, Color(i-1))
        end

        -- Create a new image and supply the system colors
        local customIcon = ReadImage(iconWorkspacePath, "#FF00FF", systemColors)

        -- Create an array with the two icon meta sprite names
        local spriteNames = {iconUpName, iconSelectedName}

        local spriteMap = {
            {0, 1, 2,
                6, 7, 8,
                12, 13, 14},
            {3, 4, 5,
                9, 10, 11,
                15, 16, 17},
        }

        local cps = ColorsPerSprite()

        for i = 1, #spriteNames do
            
            local metaSprite = MetaSprite(FindMetaSpriteId(spriteNames[i]))

            for j = 1, #metaSprite.Sprites do
                Sprite(metaSprite.Sprites[j].Id, customIcon.GetSpriteData(spriteMap[i][j]), cps)
            end

        end

        success = true

    end

    return success

end

function PixelVisionOS:OpenPath(path)

    print("Open Workspace Tool and go to", path)
    local metaData = {
        overrideLastPath = path,
    }
    
    -- TODO this is not working
    -- LoadGame("/PixelVisionOS/Tools/WorkspaceTool/", metaData)

end

function PixelVisionOS:LoadError(toolName, bgColor)

    DrawRect( 1, 11, 254, 227, bgColor or BackgroundColor(), DrawMode.TilemapCache)

    self:ChangeTitle(toolName, "toolbariconfile")

    self:CreateTitleBarMenu({}, "See menu options for this tool.")

    local buttons =
      {
        {
          name = "modalokbutton",
          action = function(target)
            QuitCurrentTool()
          end,
          key = Keys.Enter,
          tooltip = "Press 'enter' to quit the tool"
        }
      }
      
      pixelVisionOS:ShowMessageModal(toolName .. " Error", "The tool could not load without a reference to a file to edit.", 160, buttons)


end

