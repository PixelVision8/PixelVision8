--[[
	Pixel Vision 8 - Debug Tool
	Copyright (C) 2016, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- Create table to store the workspace tool logic
InfoTool = {}
InfoTool.__index = InfoTool

LoadScript("code-drop-down-menu")
LoadScript("code-game-info-panel")
LoadScript("code-build-panel")
LoadScript("code-library-panel")
LoadScript("code-runner-panel")
LoadScript("code-installer-modal")
LoadScript("pixel-vision-os-progress-modal-v2")

function InfoTool:Init()

    -- Create a new table for the instance with default properties
    local _infoTool = {
    
        toolName = "Info Editor",
        runnerName = SystemName(),
        rootPath = ReadMetadata("RootPath", "/"),
        rootDirectory = ReadMetadata("directory", nil),
        targetFile = ReadMetadata("file", nil),
        invalid = true,
        maxFilesToDisplay = 6,
        fileCheckboxes = {},
        buildFlagCheckboxes = {},
        buildSettingsCheckboxes = {},
        filePaths = {},
        SaveShortcut = 5,
        -- Master list of shared library files
        sharedLibFiles = {},
        buildFlagLabels = {
            "buildWin",
            "buildMac",
            "buildLinux",
            "buildExtras",
            "buildFullscreen"
        },
        buildTemplatePaths = {
            {name = "Windows", path = NewWorkspacePath("/Workspace/System/Runners/Runner Windows.pvr")},
            {name = "Mac", path = NewWorkspacePath("/Workspace/System/Runners/Runner Mac.pvr")},
            {name = "Linux", path = NewWorkspacePath("/Workspace/System/Runners/Runner Linux.pvr")}
        }
        
    }

    -- Create a global reference of the new workspace tool
    setmetatable(_infoTool, InfoTool)


    

    if(_infoTool.targetFile ~= nil) then

        local pathSplit = string.split(_infoTool.rootDirectory, "/")
        _infoTool.toolTitle = pathSplit[#pathSplit] .. "/data.json"

        _infoTool:CreateDropDownMenu()
        
        _infoTool:CreateGameInfoPanelPanel()
        _infoTool:CreateLibraryInfoPanel()
        _infoTool:CreateBuildInfoPanel()
        _infoTool:CreateRunnerInfoPanel()

        _infoTool:ResetDataValidation()

    else

        pixelVisionOS:ChangeTitle(_infoTool.toolName, "toolbaricontool")

        DrawRect(48, 40, 160, 8, 0, DrawMode.TilemapCache)
        DrawRect(16, 72, 208, 64, 0, DrawMode.TilemapCache)
        DrawRect(16, 168, 228, 56, 11, DrawMode.TilemapCache)

        pixelVisionOS:ShowMessageModal(_infoTool.toolName .. " Error", "The tool could not load without a reference to a file to edit.", 160, false,
            function()
                QuitCurrentTool()
            end
        )

    end
    
    -- Return the draw tool data
    return _infoTool

end

function InfoTool:Shutdown()

  -- Save the current session ID
  WriteSaveData("sessionID", SessionID())

  if(targetFile ~= nil) then
    
    WriteSaveData("targetFile", self.targetFile)
    
  end

end


function InfoTool:InvalidateData()

    -- Only everything if it needs to be
    if(self.invalid == true)then
        return
    end

    pixelVisionOS:ChangeTitle(self.toolTitle .."*", "toolbaricontool")
    -- pixelVisionOS:EnableActionButton(1, true)

    pixelVisionOS:EnableMenuItem(SaveShortcut, true)

    self.invalid = true

end

function InfoTool:ResetDataValidation()

    -- Only everything if it needs to be
    if(self.invalid == false)then
        return
    end

    pixelVisionOS:ChangeTitle(self.toolTitle, "toolbaricontool")
    self.invalid = false

    pixelVisionOS:EnableMenuItem(SaveShortcut, false)
    -- pixelVisionOS:EnableActionButton(1, false)
end

function OnSave()

    local flags = {SaveFlags.Meta}

    local includeString = ""

    for i = 1, #filePaths do

        local file = filePaths[i]

        if(file.selected == true) then
            includeString = includeString .. file.name .. ","
        end

    end

    gameEditor:WriteMetadata("clear", tostring(cleanCheckboxData.selected))

    -- Write the includeString and remove the last comma from the end

    gameEditor:WriteMetadata("includeLibs", includeString:sub(1, - 2))

    -- Add the build flags
    for i = 1, #buildFlagCheckboxes do
        gameEditor:WriteMetadata(buildFlagLabels[i], tostring(buildFlagCheckboxes[i].selected))
    end

    -- TODO need to save music and sounds when those are broken out
    gameEditor:Save(rootDirectory, flags)

    -- Display that the data was saved and reset invalidation
    pixelVisionOS:DisplayMessage("The game's 'data.json' file has been updated.", 5)

    ResetDataValidation()

end