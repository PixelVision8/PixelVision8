--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

SaveShortcut = 5

function InfoTool:CreateDropDownMenu()

    -- Find the json editor
    self.textEditorPath = pixelVisionOS:FindEditors()["json"]

   local menuOptions = 
   {
     -- About ID 1
     {name = "About", action = function() pixelVisionOS:ShowAboutModal(self.toolName) end, toolTip = "Learn about PV8."},
     {divider = true},
     {name = "Edit JSON", enabled = self.textEditorPath ~= nil, action = function() self:OnEditJSON() end, toolTip = "Edit the raw info file's json data."}, -- Reset all the values
     {name = "Reset", action = function() self:OnReset() end, key = Keys.R, toolTip = "Revert the installer to its default state."}, -- Reset all the values
     {name = "Save", key = Keys.S, action = function() self:OnSave() end, toolTip = "Save changes."},
     {divider = true}, -- Reset all the values
     {name = "Quit", key = Keys.Q, action = function() self:OnQuit() end, toolTip = "Quit the current game."}, -- Quit the current game
   }

   pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")

end

function InfoTool:OnEditJSON()

    if(self.invalid == true) then

    pixelVisionOS:ShowSaveModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before you edit the raw data file?", 160,
        -- Accept
        function(target)
        self:OnSave()
        self:EditJSON()
        end,
        -- Decline
        function (target)
        self:EditJSON()
        end,
        -- Cancel
        function(target)
        target.onParentClose()
        end
    )

    else
        -- Quit the tool
        self:EditJSON()
    end

end

function InfoTool:EditJSON()

  local metaData = {
      directory = self.rootDirectory,
      file = self.rootDirectory .. "info.json",
  }

  LoadGame(self.textEditorPath, metaData)

end

function InfoTool:OnQuit()

  if(self.invalid == true) then

    pixelVisionOS:ShowSaveModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before you quit?", 160,
      -- Accept
      function(target)
        self:OnSave()
        QuitCurrentTool()
      end,
      -- Decline
      function (target)
        -- Quit the tool
        QuitCurrentTool()
      end,
      -- Cancel
      function(target)
        target.onParentClose()
      end
    )

  else
      -- Quit the tool
      QuitCurrentTool()
  end

end

function InfoTool:OnSave()

    local flags = {SaveFlags.Meta}

    local includeString = ""

    for i = 1, #self.filePaths do

        local file = self.filePaths[i]

        if(file.selected == true) then
            includeString = includeString .. file.name .. ","
        end

    end

    -- gameEditor:WriteMetadata("clear", tostring(self.cleanCheckboxData.selected))
    
    gameEditor:WriteMetadata("runnerType", self.runnerType)

    gameEditor:WriteMetadata("includeLibs", includeString:sub(1, - 2))

    -- Add the build flags
    for i = 1, #self.buildFlagCheckboxes do
        gameEditor:WriteMetadata(self.buildFlagLabels[i], tostring(self.buildFlagCheckboxes[i].selected))
    end

    gameEditor:Save(self.rootDirectory, flags)

    -- Display that the data was saved and reset invalidation
    pixelVisionOS:DisplayMessage("The game's 'data.json' file has been updated.", 5)

    self:ResetDataValidation()

end
