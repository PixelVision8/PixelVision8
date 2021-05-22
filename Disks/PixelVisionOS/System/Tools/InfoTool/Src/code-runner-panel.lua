--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

local runnerInfoPanelID = "RunnerInfoPanel"

function InfoTool:CreateRunnerInfoPanel()

    self.runnerTypes = {"lua", "csharp"}

    -- Get runner setting

    local selectedRunnerID = Clamp(table.indexOf(self.runnerTypes, gameEditor:ReadMetadata("runnerType", "lua")), 1, 2)

    self.runnerToggleGroup = editorUI:CreateToggleGroup(true)

    

    -- C# Button
    editorUI:ToggleGroupButton(self.runnerToggleGroup, {x = 200, y = 176, w = 8, h = 8}, "radiobutton", "Use the C# runner.")

    -- Lua Button
    editorUI:ToggleGroupButton(self.runnerToggleGroup, {x = 200, y = 176 + 8, w = 8, h = 8}, "radiobutton", "Use the Lua runner.")

    editorUI:SelectToggleButton(self.runnerToggleGroup, selectedRunnerID);

    pixelVisionOS:RegisterUI({name = runnerInfoPanelID}, "RunnerInfoPanelUpdate", self)

    self.runnerToggleGroup.onAction = function(value)

        self.runnerType = self.runnerTypes[value]
        self:InvalidateData()

    end

end

function InfoTool:RunnerInfoPanelUpdate()

    editorUI:UpdateToggleGroup(self.runnerToggleGroup)

end