--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- SaveShortcut = 5

function FontEditorTool:CreateDropDownMenu()

    self.ClearShortcut = 3
    self.SaveShortcut = 4
    self.RevertShortcut = 5
    self.UndoShortcut = 7
    self.RedoShortcut = 8
    self.CopyShortcut = 9
    self.PasteShortcut = 10

    local menuOptions = 
    {
        {name = "About", action = function() pixelVisionOS:ShowAboutModal(self.toolName) end, toolTip = "Learn more about this tool."},
        {divider = true},
        {name = "Clear", action = function() self:OnClear() end, enabled = false, key = Keys.D, toolTip = "Clear the currently selected character."}, -- Reset all the values
        {name = "Save", action = function() self:OnSave() end, key = Keys.S, enabled = false, toolTip = "Save changes made to the font file."}, -- Reset all the values

        {name = "Revert", action = function() self:OnRevert() end, enabled = false, toolTip = "Revert the character to its previous state."}, -- Reset all the values
        {divider = true},
        {name = "Undo", action = function() self:OnUndo() end, enabled = false, key = Keys.Z, toolTip = "Undo the last action."}, -- Reset all the values
        {name = "Redo", action = function() self:OnRedo() end, enabled = false, key = Keys.Y, toolTip = "Redo the last undo."}, -- Reset all the values
        {name = "Copy", action = function() self:OnCopySprite() end, enabled = false, key = Keys.C, toolTip = "Copy the currently selected sprite."}, -- Reset all the values
        {name = "Paste", action = function() self:OnPasteSprite() end, enabled = false, key = Keys.V, toolTip = "Paste the last copied sprite."}, -- Reset all the values

        {divider = true},

        {name = "Quit", key = Keys.Q, action = function() self:OnQuit() end, toolTip = "Quit the current game."}, -- Quit the current game
    }

    -- TODO need to test if this is a game and not a one off file
    if(PathExists(NewWorkspacePath(self.rootDirectory).AppendFile("code.lua"))) then
        table.insert(menuOptions, #menuOptions, {name = "Run Game", action = function() self:OnRunGame() end, key = Keys.R, toolTip = "Run the code for this game."})
    end

    pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")

end

function FontEditorTool:OnClear()

    -- TODO add new buttons

    pixelVisionOS:ShowMessageModal("Clear Sprite", "Do you want to clear all of the pixel data for the current sprite?", 160, true,
        function()
            if(pixelVisionOS.messageModal.selectionValue == true) then
                -- Save changes
                ClearSprite()

            end

        end
    )

end

function FontEditorTool:ClearSprite()

    -- TODO need to link this up to the size
    -- get the total number of pixels in the current sprite selection
    local total = 8 * 8


    -- TODO we should calculate an empty sprite when changing sizes instead of doing it over and over again on clear sprite

    -- Create an empty table for the pixel data
    tmpPixelData = {}

    -- Loop through the total pixels and set them to -1
    for i = 1, total do
        tmpPixelData[i] = - 1
    end

    -- Find the currents sprite index
    -- local index = pixelVisionOS:CalculateRealSpriteIndex(spritePickerData)

    -- Update the currently selected sprite
    gameEditor:FontSprite(currentCharID, tmpPixelData)

    -- Select the current sprite to update the canvas
    -- pixelVisionOS:SelectSpritePickerSprite(spritePickerData, index)

    -- Redraw the sprite picker page
    DrawFontCharacter(currentCharID)

    OnSelectChar(currentCharID)

    DrawSampleText()

    -- Invalidate the tool's data
    InvalidateData()

    pixelVisionOS:EnableMenuItem(RevertShortcut, true)
    pixelVisionOS:EnableMenuItem(ClearShortcut, false)

end

function FontEditorTool:OnRevert()

    pixelVisionOS:ShowMessageModal("Clear Sprite", "Do you want to revert the sprite's pixel data to it's original state?", 160, true,
        function()
            if(pixelVisionOS.messageModal.selectionValue == true) then
                -- Save changes
                RevertSprite()

            end

        end
    )

end

-- TODO This should be automatically added to the tool when resetting the history

function FontEditorTool:OnUndo()

    pixelVisionOS:Undo(self)

    self:UpdateHistoryButtons()
end

function FontEditorTool:OnRedo()

    pixelVisionOS:Redo(self)

    self:UpdateHistoryButtons()

end

function FontEditorTool:BeginUndo()
    pixelVisionOS:BeginUndoable(self)
end

function FontEditorTool:EndUndo()
    pixelVisionOS:EndUndoable(self)
    self:UpdateHistoryButtons()
end

function FontEditorTool:UpdateHistoryButtons()

    pixelVisionOS:EnableMenuItem(self.UndoShortcut, pixelVisionOS:IsUndoable(self))
    pixelVisionOS:EnableMenuItem(self.RedoShortcut, pixelVisionOS:IsRedoable(self))

end

function FontEditorTool:SetState(state)

    if(state.Action == nil) then
        return
    end

    state:Action()

end

function FontEditorTool:GetState()

    local pixelData = pixelVisionOS:GetCanvasPixelData(self.canvasData)

    local selectedChar = self.charStepper.inputField.text

    local state = {
        
        Action = function()

            if(pixelData ~= nil) then

                self:OnEnterChar(selectedChar)
                
                -- TODO need to figure out how to optimize this
                pixelVisionOS:SetCanvasPixels(self.canvasData, pixelData)
                
                self:OnSaveCanvasChanges()

            end

        end
    }

    return state

end

function FontEditorTool:OnRunGame()

    if(self.invalid == true) then

        pixelVisionOS:ShowSaveModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before running the game?", 160,
          -- Accept
          function(target)
            self:OnSave()
            LoadGame(NewWorkspacePath(self.rootDirectory))
          end,
          -- Decline
          function (target)
            LoadGame(NewWorkspacePath(self.rootDirectory))
          end,
          -- Cancel
          function(target)
            target.onParentClose()
          end
        )

    else
        -- Quit the tool
        LoadGame(NewWorkspacePath(self.rootDirectory))
    end

end

function FontEditorTool:OnQuit()

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

local copiedSpriteData = nil

function FontEditorTool:OnCopySprite()

    copiedSpriteData = gameEditor:FontSprite(currentCharID)

    pixelVisionOS:EnableMenuItem(PasteShortcut, true)

end

function FontEditorTool:OnPasteSprite()

    if(copiedSpriteData == nil) then
        return
    end

    -- local index = pixelVisionOS:CalculateRealSpriteIndex(spritePickerData)

    gameEditor:FontSprite(currentCharID, copiedSpriteData)

    copiedSpriteData = nil

    DrawFontCharacter(currentCharID)

    OnSelectChar(currentCharID)

    DrawSampleText()

    InvalidateData()

    pixelVisionOS:EnableMenuItem(RevertShortcut, false)
    pixelVisionOS:EnableMenuItem(PasteShortcut, false)

end

function FontEditorTool:OnSave()

    -- TODO need to save all of the colors back to the game

    -- local currentFontName =

    -- Write the font name to the save file so it will reload correctly
    --  WriteSaveData("fontName", currentFontName == fontName and "none" or currentFontName)
    --
    -- Update the font name
    --fontName = currentFontName

    local oldFontName = self.fontName

    -- Save the font and get the new font name back (in case there is a name conflict in the folder)
    self.fontName = gameEditor:SaveFont(self.fontNameInputData.text)


    print("font names", oldFontName, self.fontName, self.targetFile)
    
    -- Save the font back to the meta data
    WriteMetadata("file", self.rootDirectory .. self.fontName)

    -- Update the input field to show any changes to the font name
    editorUI:ChangeInputField(self.fontNameInputData, string.split(self.fontName, ".")[1], false)

    if(oldFontName ~= self.fontName) then

        local buttons = 
        {
            {
                name = "modalokbutton",
                action = function(target)
                    target.onParentClose()
                    self:ResetDataValidation()
print("Remove and retaret", self.targetFile)
                    -- if(PathExists())
                end,
                key = Keys.Enter,
                tooltip = "Press 'enter' to accept"
            }
        }
        
        pixelVisionOS:ShowMessageModal(self.toolName .. " Warning", "The font was renamed from '".. string.split(oldFontName, ".")[1] .."' to '" .. string.split(self.fontName, ".")[1] .."'.", 160, buttons)
        
    else
        -- Display a message that everything was saved
        pixelVisionOS:DisplayMessage("Your changes to '" .. string.split(fontName, ".")[1] .. "' have been saved.", 5)
        -- Clear the validation
        ResetDataValidation()
    end



end