SaveShortcut = 4
UndoShortcut = 10
RedoShortcut = 11
CopyShortcut = 12
PasteShortcut = 13
BGColorShortcut = 14 
FlipH = 16
FlipV = 17
QuitShortcut = 19

function TilemapTool:CreateDropDownMenu()

    local menuOptions =
    {
        -- About ID 1
        {name = "About", action = function() pixelVisionOS:ShowAboutModal(self.toolName) end, toolTip = "Learn about PV8."},
        
        {divider = true},
        {name = "Edit Sprites", enabled = spriteEditorPath ~= nil, action = function() self:OnEditSprites() end, toolTip = "Open the sprite editor."},
        --{name = "Export PNG", action = function() self:OnPNGExport() end, enabled = true, toolTip = "Generate a 'tilemap.png' file."}, -- Reset all the values
        {name = "Save", action = function() self:OnSave() end, enabled = false, key = Keys.S, toolTip = "Save changes made to the tilemap.json file."}, -- Reset all the values
        {name = "Export", action = function() self:OnTilemapBuilder() end, toolTip = "Allows you to export the current tilemap in different formats."}, -- Reset all the values

        {divider = true},
        {name = "Auto Load", action = function() self:ToggleAutoLoad() end, toolTip = "Enable or disable the game from auto-loading a tilemap."}, -- Reset all the values
        {name = "Shift Colors", action = function() self:ShiftColorOffset() end, toolTip = "Fix tiles that are not set to a palette color offer value."}, -- Reset all the values
        
        {divider = true},
        {name = "Undo", action = function() self:OnUndo() end, enabled = false, key = Keys.Z, toolTip = "Undo the last action."}, -- Reset all the values
        {name = "Redo", action = function() self:OnRedo() end, enabled = false, key = Keys.Y, toolTip = "Redo the last undo."}, -- Reset all the values
        {name = "Copy", action = function() self:OnCopyTile() end, enabled = false, key = Keys.C, toolTip = "Copy the currently selected tile."}, -- Reset all the values
        {name = "Paste", action = function() self:OnPasteTile() end, enabled = false, key = Keys.V, toolTip = "Paste the last copied tile."}, -- Reset all the values
        
        {divider = true},
        {name = "BG Color", action = function() self:ToggleBackgroundColor(not self.showBGColor) end, key = Keys.B, toolTip = "Toggle background color."},
        {name = "Toggle Layer", action = function() self:ChangeMode(not self.flagModeActive) end, key = Keys.L, toolTip = "Toggle flag mode for collision."},
        
        {divider = true},
        {name = "Flip H", action = function() self:OnFlipH() end, enabled = false, key = Keys.H, toolTip = "Flip the current tile horizontally."}, -- Reset all the values
        {name = "Flip V", action = function() self:OnFlipV() end, enabled = false, key = Keys.G, toolTip = "Flip the current tile vertically."}, -- Reset all the values
        
        {divider = true},
        {name = "Quit", key = Keys.Q, action = function() self:OnQuit() end, toolTip = "Quit the current game."}, -- Quit the current game
    }

    if(PathExists(NewWorkspacePath(self.rootDirectory).AppendFile("code.lua"))) then
        table.insert(menuOptions, #menuOptions, {name = "Run Game", action = OnRunGame, key = Keys.R, toolTip = "Run the code for this game."})
    end

    pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")
    
end

function TilemapTool:ToggleAutoLoad()
    -- TODO need to read the meta data file and toggle the auto load flag
end
--
--function TilemapTool:ShiftColorOffset()
--    -- TODO need to loop through all of the tiles and if the color offset is under 128, shift it
--end

function TilemapTool:ShiftColorOffset()

    self:ResetProcessTiles()

    self.tilesPerLoop = 16
    self.totalTilesToProcess = self.mapSize.x * self.mapSize.y
    --self.prevColorIndex = prevValue
    --self.newColorIndex = newValue

    -- The action to preform on each step of the sprite progress loop
    self.onTileProcessAction = function()

        local pos = CalculatePosition(self.currentParsedTileID, self.mapSize.x)
        -- Get the sprite data
        self.tmpPixelData = gameEditor:Tile(pos.x, pos.y)

        if(self.tmpPixelData.colorOffset < pixelVisionOS.colorOffset) then
            
            -- TODO update the color offset
            -- TODO Draw the tile to the layer cache
            
        end
        ---- Loop through each of the tiles pixel
        --for j = 1, #self.tmpPixelData do
        --
        --    -- Get the current color
        --    local color = self.tmpPixelData[j]
        --
        --    -- Test if the color is the old color
        --    if(color == self.prevColorIndex) then
        --
        --        -- update the sprite's pixel to the new value
        --        self.tmpPixelData[j] = self.newColorIndex
        --
        --    end
        --
        --end

        -- Save the modified sprite data
        --gameEditor:Tile(self.currentParsedTileID, self.tmpPixelData)



        -- Return true to let the progress loop know this was executed fully
        return true

    end

    pixelVisionOS:OpenModal(self.progressModal,
            function()

                self:CompleteProcessTiles()

            end
    )

    pixelVisionOS:RegisterUI({name = "ProgressUpdate"}, "UpdateTileProgress", self, true)


end

function TilemapTool:OnRunGame()
    -- TODO should this ask to launch the game first?

    if(self.invalid == true) then

        pixelVisionOS:ShowMessageModal("Unsaved Changes", "You have unsaved changes. You will lose those changes if you run the game now?", 160, true,
                function()
                    if(pixelVisionOS.messageModal.selectionValue == true) then
                        LoadGame(NewWorkspacePath(self.rootDirectory))
                    end

                end
        )

    else

        LoadGame(NewWorkspacePath(self.rootDirectory))

    end

end

function TilemapTool:ToggleBackgroundColor(value)

    self.showBGColor = value

    self.tilePickerData.showBGColor = value

    pixelVisionOS:InvalidateItemPickerDisplay(self.tilePickerData)

    -- DrawRect(self.viewport.x, self.viewport.y, self.viewport.w, self.viewport.h, pixelVisionOS.emptyColorID, DrawMode.TilemapCache)

end

function TilemapTool:OnSave()

    -- TODO need to save all of the colors back to the game

    -- This will save the system data, the colors and color-map
    gameEditor:Save(self.rootDirectory, {SaveFlags.System, SaveFlags.Colors, SaveFlags.Tilemap})-- SaveFlags.ColorMap, SaveFlags.FlagColors})

    -- Display a message that everything was saved
    pixelVisionOS:DisplayMessage("Your changes have been saved.", 5)

    -- Need to fix the extension if we switched from a png to a json file
    if(string.ends(toolTitle, "png")) then

        -- Rewrite the extension
        toolTitle = string.split(toolTitle, ".")[1] .. ".json"

    end

    -- Clear the validation
    self:ResetDataValidation()

end

-- TODO need to wire up the history logic with the new undo system

function TilemapTool:UpdateHistory(tiles)

    -- if(#tiles == 0) then
    --     return
    -- end

    -- local historyAction = {
    --     -- sound = settingsString,
    --     Action = function()

    --         local total = #tiles

    --         for i = 1, total do

    --             local tile = tiles[i]

    --             pixelVisionOS:OnChangeTile(self.tilePickerData, tile.col, tile.row, tile.spriteID, tile.colorOffset, tile.flag)

    --         end

    --     end
    -- }

    -- pixelVisionOS:AddUndoHistory(historyAction)

    -- -- We only want to update the buttons in some situations
    -- -- if(updateButtons ~= false) then
    -- self:UpdateHistoryButtons()
    -- end

end

function TilemapTool:OnUndo()

    -- local action = pixelVisionOS:Undo()

    -- if(action ~= nil and action.Action ~= nil) then
    --     action.Action()
    -- end

    -- self:UpdateHistoryButtons()
end

function TilemapTool:OnRedo()

    -- local action = pixelVisionOS:Redo()

    -- if(action ~= nil and action.Action ~= nil) then
    --     action.Action()
    -- end

    -- self:UpdateHistoryButtons()
end

function TilemapTool:UpdateHistoryButtons()

    -- pixelVisionOS:EnableMenuItem(UndoShortcut, pixelVisionOS:IsUndoable())
    -- pixelVisionOS:EnableMenuItem(RedoShortcut, pixelVisionOS:IsRedoable())

end

function TilemapTool:ClearHistory()

    -- Reset history
    -- pixelVisionOS:ResetUndoHistory()
    -- self:UpdateHistoryButtons()

end

function TilemapTool:OnCopyTile()

end

function TilemapTool:OnPasteTile()

end