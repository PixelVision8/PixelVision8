--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

NewShortcut, SaveShortcut, CutShortcut, CopyShortcut, PasteShortcut = 3, 10, 6, 7, 8

function TextTool:CreateDropDownMenu()

    local menuOptions =
    {
        -- About ID 1
        {name = "About", action = function() pixelVisionOS:ShowAboutModal(self.toolName) end, toolTip = "Learn about PV8."},
        {divider = true},
        {name = "New", action = function() self:NewFile() end, enabled = false, key = Keys.N, toolTip = "Create a new text file."}, -- Reset all the values
        {name = "Rename", action = function() self:RenameFile() end, enabled = false, key = Keys.N, toolTip = "Create a new text file."}, -- Reset all the values
        {divider = true},
        {name = "Cut", action = function() self:OnCutText() end, enabled = false, key = Keys.X, toolTip = "Cut the currently selected text."}, -- Reset all the values
        {name = "Copy", action = function() self:OnCopyText() end, enabled = false, key = Keys.C, toolTip = "Copy the currently selected text."}, -- Reset all the values
        {name = "Paste", action = function() self:OnPasteColor() end, enabled = false, key = Keys.V, toolTip = "Paste the last copied text."}, -- Reset all the values
    }

    if(self.codeMode == true) then

        table.insert(menuOptions, {divider = true})
        table.insert(menuOptions, {name = "Toggle Lines", action = function() self:ToggleLineNumbers() end, key = Keys.L, toolTip = "Toggle the line numbers for the editor."})
        table.insert(menuOptions, {name = "Run Game", action = function() self:OnRunGame() end, key = Keys.R, toolTip = "Run the code for this game."})

        SaveShortcut = SaveShortcut + 3
    end

    -- Add the last part of the menu options
    table.insert(menuOptions, {divider = true})
    table.insert(menuOptions, {name = "Save", action = function() self:OnSave() end, enabled = false, key = Keys.S, toolTip = "Save changes made to the text file."}) -- Reset all the values
    table.insert(menuOptions, {name = "Quit", key = Keys.Q, action = function() self:OnQuit() end, toolTip = "Quit the current game."}) -- Quit the current game

    pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")

end

function TextTool:OnRunGame()


    local data = {runnerType = self.extension == ".lua" and "lua" or "csharp"}

    print("runnerType", data["runnerType"], self.extension)

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

function TextTool:OnQuit()

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


function TextTool:ToggleLineNumbers()

    if(self.codeMode == false) then
        return  
    end
    
    -- TODO need to save this value to the bios
    
    self.showLines = not self.showLines
    
    WriteBiosData("ShowLinesInTextEditor", self.showLines == true and "True" or "False")

    self:InvalidateLineNumbers()

end