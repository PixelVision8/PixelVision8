--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

function InfoTool:CreateDropDownMenu()

    -- Get a list of all the editors
    local editorMapping = pixelVisionOS:FindEditors()

    -- Find the json editor
    self.textEditorPath = editorMapping["json"]

   local menuOptions = 
   {
     -- About ID 1
     {name = "About", action = function() pixelVisionOS:ShowAboutModal(toolName) end, toolTip = "Learn about PV8."},
     {divider = true},
     {name = "Edit JSON", enabled = self.textEditorPath ~= nil, action = function() self:OnEditJSON() end, toolTip = "Edit the raw info file's json data."}, -- Reset all the values
     {name = "Reset", action = function() self:OnReset() end, key = Keys.R, toolTip = "Revert the installer to its default state."}, -- Reset all the values
     {name = "Save", key = Keys.S, action = function() self:OnSave() end, toolTip = "Save changes."},
     {divider = true}, -- Reset all the values
     {name = "Quit", key = Keys.Q, action = function() self:OnQuit() end, toolTip = "Quit the current game."}, -- Quit the current game
   }

   pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")

end
