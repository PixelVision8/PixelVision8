--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

NewShortcut, SaveShortcut, CutShortcut, CopyShortcut, PasteShortcut = 3, 10, 6, 7, 8

function ImageTool:CreateDropDownMenu()

  local menuOptions = 
  {
      -- About ID 1
      {name = "About", action = function() pixelVisionOS:ShowAboutModal(self.toolName) end, toolTip = "Learn about PV8."},

      {divider = true},
      {name = "Quit", key = Keys.Q, action = QuitCurrentTool, toolTip = "Quit the current game."}, -- Quit the current game
  }

  local editorMapping = pixelVisionOS:FindEditors()

  local addLastDivider = false

  -- Only add these if the version of PV8 supports drawing tools
  if(editorMapping["colors"] ~= nil) then

      table.insert(menuOptions, #menuOptions, {name = "Toggle Palette", enabled = true, action = function() debugMode = not debugMode end, toolTip = "Shows a preview of the color palette."})

      table.insert(menuOptions, #menuOptions, {divider = true})

      table.insert(menuOptions, #menuOptions, {name = "Save Colors", enabled = true, action = function() OnSavePNG(true, false, false) end, toolTip = "Create a 'color-map.png' file."})

      addLastDivider = true
  end

  if(editorMapping["sprites"] ~= nil) then

      table.insert(menuOptions, #menuOptions, {name = "Save Sprites", enabled = true, action = function() OnSavePNG(false, true, false) end, toolTip = "Create a 'sprite.png' file."})
      addLastDivider = true
  end

  if(editorMapping["tilemap"] ~= nil) then

      table.insert(menuOptions, #menuOptions, {name = "Save Tilemap", enabled = true, action = function() OnSavePNG(false, false, true) end, toolTip = "Create a 'tilemap.json' file."})
      addLastDivider = true
  end

  if(addLastDivider == true) then
      table.insert(menuOptions, #menuOptions, {divider = true})
  end

  pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")

end

function ImageTool:OnRunGame()


    local data = {runnerType = self.extension == ".lua" and "lua" or "csharp"}

    -- print("runnerType", data["runnerType"], self.extension)

    -- if(self.codeMode == true) then
    --    data["codeFile"] = _textTool.targetFile
    -- end

    local parentPath = self.targetFilePath.ParentPath

    if(self.invalid == true) then

        pixelVisionOS:ShowMessageModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before running the game?", 160, true,
                function()
                    if(pixelVisionOS.messageModal.selectionValue == true) then
                        -- Save changes
                        self:OnSave()

                    end

                    -- TODO should check that this is a game directory or that this file is at least a code.lua file
                    LoadGame(parentPath.Path, data)
                end
        )

    else
        -- Quit the tool
        LoadGame(parentPath.Path, data)
    end

end

function ImageTool:OnQuit()

      if(self.invalid == true) then

        pixelVisionOS:ShowMessageModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before you quit?", 160, true,
          function()
            if(pixelVisionOS.messageModal.selectionValue == true) then
              -- Save changes
              self:OnSave()

            end

            -- Quit the tool
            QuitCurrentTool()
          end
        )

      else
        -- Quit the tool
        QuitCurrentTool()
      end

end


function ImageTool:ToggleLineNumbers()

    if(self.codeMode == false) then
        return  
    end
    
    -- TODO need to save this value to the bios
    
    self.showLines = not self.showLines
    
    WriteBiosData("ShowLinesInTextEditor", self.showLines == true and "True" or "False")

    self:InvalidateLineNumbers()

end