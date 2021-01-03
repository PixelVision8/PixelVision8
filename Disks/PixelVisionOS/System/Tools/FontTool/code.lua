--[[
	Pixel Vision 8 - Debug Tool
	Copyright (C) 2016, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- API Bridge
LoadScript("sb-sprites")
LoadScript("pixel-vision-os-v2")
LoadScript("pixel-vision-os-color-picker-v3")
LoadScript("pixel-vision-os-sprite-picker-v4")
LoadScript("pixel-vision-os-canvas-v3")
LoadScript("code-characters")

local toolName = "Color Tool"
local colorOffset = 50
local imageLoaded = false
local toolLoaded = false
local fontName = "untitled"
local currentCharID = -1
local characterWindow = {x = 152, y = 72, w = 96, h = 64}
local originalPixelData = nil
local WHITE = 15

local ClearShortcut, SaveShortcut, RevertShortcut, UndoShortcut, RedoShortcut, CopyShortcut, PasteShortcut = 3, 4, 5, 7, 8, 9, 10

local tools = {"pen", "eraser"}

local message = "The quick brown fox jumps over the lazy dog."

function InvalidateData()

    -- Only everything if it needs to be
    if(invalid == true)then
        return
    end

    pixelVisionOS:ChangeTitle(toolTitle .."*", "toolbariconfile")

    invalid = true

    pixelVisionOS:EnableMenuItem(SaveShortcut, true)


end

function ResetDataValidation()

    -- Only everything if it needs to be
    if(invalid == false)then
        return
    end

    -- Update the title with the current font name
    UpdateTitleText()

    pixelVisionOS:ChangeTitle(toolTitle, "toolbariconfile")
    invalid = false

    pixelVisionOS:EnableMenuItem(SaveShortcut, false)


end

local copiedSpriteData = nil

function OnCopySprite()

    copiedSpriteData = gameEditor:FontSprite(currentCharID)

    pixelVisionOS:EnableMenuItem(PasteShortcut, true)

end

function OnPasteSprite()

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

function OnRevert()

    pixelVisionOS:ShowMessageModal("Clear Sprite", "Do you want to revert the sprite's pixel data to it's original state?", 160, true,
        function()
            if(pixelVisionOS.messageModal.selectionValue == true) then
                -- Save changes
                RevertSprite()

            end

        end
    )

end

function RevertSprite()

    gameEditor:FontSprite(currentCharID, originalPixelData)

    -- Redraw the sprite picker page
    DrawFontCharacter(currentCharID)

    OnSelectChar(currentCharID)

    DrawSampleText()

    -- Invalidate the tool's data
    InvalidateData()

    pixelVisionOS:EnableMenuItem(RevertShortcut, false)
    pixelVisionOS:EnableMenuItem(ClearShortcut, not IsEmpty(originalPixelData))

end


function OnClear()

    pixelVisionOS:ShowMessageModal("Clear Sprite", "Do you want to clear all of the pixel data for the current sprite?", 160, true,
        function()
            if(pixelVisionOS.messageModal.selectionValue == true) then
                -- Save changes
                ClearSprite()

            end

        end
    )

end

function ClearSprite()

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

function Init()

    BackgroundColor(5)

    -- Disable the back key in this tool
    EnableBackKey(false)
    EnableAutoRun(false)

    -- Create an global instance of the Pixel Vision OS
    _G["pixelVisionOS"] = PixelVisionOS:Init()
    
    -- -- Create an instance of the Pixel Vision OS
    -- pixelVisionOS = PixelVisionOS:Init()

    -- -- Get a reference to the Editor UI
    -- editorUI = pixelVisionOS.editorUI

    rootDirectory = ReadMetadata("directory", nil)

    -- Get the target file
    targetFile = ReadMetadata("file", nil)

    if(targetFile ~= nil) then

        -- print("targetFile", targetFile)

        local pathSplit = string.split(targetFile, "/")

        -- if(SessionID() == ReadSaveData("sessionID", "") and rootDirectory == ReadSaveData("rootDirectory", "")) then
        --   print("Reloading session")
        -- else
        --
        -- end

        fontName = pathSplit[#pathSplit] or lastFont

        if(gameEditor:LoadFont(rootDirectory .. fontName) == false) then

            pixelVisionOS:ChangeTitle(toolName, "toolbaricontool")

            pixelVisionOS:ShowMessageModal(toolName .. " Error", "The tool was not able to load the correct file", 160, false,
                function()
                    QuitCurrentTool()
                end
            )

        end

    else

        -- Patch background when loading fails

        -- Left panel
        DrawRect(8, 40, 128, 128, 0, DrawMode.TilemapCache)

        DrawRect(152, 40, 96, 8, 0, DrawMode.TilemapCache)

        DrawRect(152, 152, 96, 24, BackgroundColor(), DrawMode.TilemapCache)

        DrawRect(8, 192, 240, 16, 0, DrawMode.TilemapCache)
        --
        -- DrawRect(136, 164, 3, 9, BackgroundColor(), DrawMode.TilemapCache)
        -- DrawRect(248, 180, 3, 9, BackgroundColor(), DrawMode.TilemapCache)
        -- DrawRect(136, 220, 3, 9, BackgroundColor(), DrawMode.TilemapCache)


        pixelVisionOS:ChangeTitle(toolName, "toolbaricontool")

        pixelVisionOS:ShowMessageModal(toolName .. " Error", "The tool could not load without a reference to a file to edit.", 160, false,
            function()
                QuitCurrentTool()
            end
        )

    end

end


function OnFontLoaded()

    -- If data loaded activate the tool
    -- if(success == true) then

    -- gameEditor:StartLoading()

    local menuOptions = 
    {
        {name = "About", action = nil, toolTip = "Learn more about this tool."},
        {divider = true},
        {name = "Clear", action = OnClear, enabled = false, key = Keys.D, toolTip = "Clear the currently selected character."}, -- Reset all the values
        {name = "Save", action = OnSave, key = Keys.S, enabled = false, toolTip = "Save changes made to the font file."}, -- Reset all the values

        {name = "Revert", action = OnRevert, enabled = false, toolTip = "Revert the character to its previous state."}, -- Reset all the values
        {divider = true},
        {name = "Undo", action = OnUndo, enabled = false, key = Keys.Z, toolTip = "Undo the last action."}, -- Reset all the values
        {name = "Redo", action = OnRedo, enabled = false, key = Keys.Y, toolTip = "Redo the last undo."}, -- Reset all the values
        {name = "Copy", action = OnCopySprite, enabled = false, key = Keys.C, toolTip = "Copy the currently selected sprite."}, -- Reset all the values
        {name = "Paste", action = OnPasteSprite, enabled = false, key = Keys.V, toolTip = "Paste the last copied sprite."}, -- Reset all the values

        {divider = true},

        {name = "Quit", key = Keys.Q, action = OnQuit, toolTip = "Quit the current game."}, -- Quit the current game
    }

    if(PathExists(NewWorkspacePath(rootDirectory).AppendFile("code.lua"))) then
        table.insert(menuOptions, #menuOptions, {name = "Run Game", action = OnRunGame, key = Keys.R, toolTip = "Run the code for this game."})
    end

    pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")

    -- The first thing we need to do is rebuild the tool's color table to include the game's system and game colors.

    -- ImportColorsFromGame()

    -- selectionPixelData = colorselection

    _G["itempickerover"] = {spriteIDs = charselection.spriteIDs, width = charselection.width, colorOffset = 28}

    _G["itempickerselectedup"] = {spriteIDs = charselection.spriteIDs, width = charselection.width, colorOffset = (_G["itempickerover"].colorOffset + 2)}

    canvasData = pixelVisionOS:CreateCanvas(
        {
            x = 8,
            y = 40,
            w = 128,
            h = 128
        },
        {
            x = 128,
            y = 128
        },
        1,
        0, -- Forcing this to be white
        "Draw on the canvas",
        0
    )

    -- canvasData.overDrawArgs[8] = 0

    pixelVisionOS:CanvasBrushColor(canvasData, WHITE)

    canvasData.onPress = function()

        -- Calculate the position in the canvas
        local tmpPos = NewPoint(
            Clamp(editorUI.collisionManager.mousePos.x - canvasData.rect.x, 0, canvasData.rect.w - 1) / canvasData.scale,
            Clamp(editorUI.collisionManager.mousePos.y - canvasData.rect.y, 0, canvasData.rect.h - 1) / canvasData.scale
        )

        -- See what color is under the mouse by reading the pixel
        local tmpColor = canvasData.paintCanvas:ReadPixelAt(tmpPos.x, tmpPos.y)

        -- Change the tool bases on what color the mouse is over
        pixelVisionOS:ChangeCanvasTool(canvasData, tmpColor == 0 and "pen" or "eraser", 6)

        canvasData.inDrawMode = true

        UpdateHistory(pixelVisionOS:GetCanvasPixelData(canvasData))
    end

    canvasData.onAction = OnSaveCanvasChanges

    pixelVisionOS:ChangeCanvasPixelSize(canvasData, 1)

    charPicker = editorUI:CreatePicker(
        {x = 152, y = 72, w = 96, h = 64 },
        8,
        8,
        95,
        "itempicker",
        "Pick a char"
    )

    charPicker.onAction = OnSelectChar

    for i = 1, #characters do

        -- Lua loops start at 1 but we need to start at 0
        DrawFontCharacter(i - 1, rect)

    end


    spacingStepper = editorUI:CreateStringStepper({x = 200, y = 152}, 16, "0", {"-4", "-3", "-2", "-1", "0", "1", "2"}, "", "Change the length of the pattern.")

    -- TODO This is a hack becuse entering in numbers by hand could break the return value... example 2 becomes 20.
    spacingStepper.inputField.highlighterTheme.disabled = 15
    editorUI:Enable(spacingStepper.inputField, false)

    spacingStepper.onInputAction = function(value) DrawSampleText(value) end

    charStepper = editorUI:CreateStringStepper({x = 152, y = 152}, 8, "A", characters, "", "Change the length of the pattern.")

    charStepper.onInputAction = OnEnterChar

    -- local pathSplit = string.split(rootDirectory, "/")



    -- Update title with file path
    -- toolTitle = pathSplit[#pathSplit - 1] .. "/" .. fontName

    fontNameInputData = editorUI:CreateInputField({x = 152, y = 40, w = 96}, string.split(fontName, ".")[1], "Give the font a name.", "file")
    -- fontNameInputData.min = 0
    fontNameInputData.onAction = OnFontNameChange

    -- editorUI:Enable(fontNameInputData, false)

    -- TODO need to save last selection and restore it
    OnEnterChar(" ")

    DrawSampleText()

    ResetDataValidation()

    toolLoaded = true

end

function UpdateTitleText()

    local pathSplit = string.split(targetFile, "/")

    -- fontName = fontNameInputData.text .. ".font.png"

    -- Update title with file path
    toolTitle = pathSplit[#pathSplit - 1] .. "/" .. fontName

end

function OnSaveCanvasChanges()

    local pixelData = pixelVisionOS:GetCanvasPixelData(canvasData)
    -- local canvasSize = editorUI:GetCanvasSize(canvasData)

    local total = #pixelData

    for i = 1, total do
        -- if(pixelData[i] > - 1) then
        --   pixelData[i] = 0
        -- end
        pixelData[i] = pixelData[i] - WHITE
    end

    -- Update the current sprite in the picker
    gameEditor:FontSprite(currentCharID, pixelData)

    DrawFontCharacter(currentCharID)

    if(canvasData.invalid == true) then

        -- Invalidate the sprite tool since we change some pixel data
        InvalidateData()

        -- Reset the canvas invalidation since we copied it
        editorUI:ResetValidation(canvasData)

    end

    pixelVisionOS:EnableMenuItem(RevertShortcut, true)

    DrawSampleText()

end

function DrawFontCharacter(id )

    local pos = CalculatePosition(id, math.floor(characterWindow.w / 8))

    pixelData = gameEditor:FontSprite(id)

    if(IsEmpty(pixelData)) then

        pixelData = Sprite(_G["fontchar" .. id].spriteIDs[1])

    else

        for j = 1, #pixelData do
            if(pixelData[j] < 0) then
                pixelData[j] = colorOffset
            else
                -- Convert the pixel to white
                pixelData[j] = WHITE--pixelData[j] + colorOffset
            end
        end

    end
    -- need to replace transparent color


    local x = (pos.x * 8) + characterWindow.x
    local y = (pos.y * 8) + characterWindow.y

    DrawPixels(pixelData, x, y, 8, 8, false, false, DrawMode.TilemapCache)

end

function IsEmpty(spriteData)

    for i = 1, #spriteData do

        if(spriteData[i] > - 1) then
            return false
        end
    end

    return true

end


function OnEnterChar(value)

    local charID = table.indexOf(characters, value)

    if(charID ~= -1) then
        editorUI:SelectPicker(charPicker, charID - 1)
    end


    OnSelectChar(charID - 1)

end

function OnSelectChar(value)

    if(currentCharID == value) then
        return
    end

    local lastCharID = currentCharID

    local visibleWidth = math.floor(charPicker.rect.w / 8)

    local pos = CalculatePosition(value, visibleWidth)

    currentCharID = CalculateIndex(pos.x, pos.y, visibleWidth)

    local char = characters[currentCharID + 1]
    --
    -- print("Char ID", value, char)

    -- editorUI:ChangeInputField(chart, value, false)

    editorUI:ChangeStringStepperValue(charStepper, char, false)
    --
    -- UpdateCanvas(value)


    -- Save the original pixel data from the selection
    local tmpPixelData = gameEditor:FontSprite(currentCharID)

    if(lastCharID ~= currentCharID) then
        -- Clear the original pixel data
        
        -- Need to loop through the pixel data and change the offset
        local total = #tmpPixelData
        for i = 1, total do

            if(tmpPixelData[i] > - 1) then

                -- Convert the pixel data to white
                tmpPixelData[i] = WHITE
            else
                tmpPixelData[i] = -1
            end

        end

    end

    local scale = 16 -- TODO need to get the real scale
    local size = NewPoint(8, 8)

    -- TODO simulate selecting the first sprite
    pixelVisionOS:ResizeCanvas(canvasData, size, scale, tmpPixelData)

    -- editorUI:CanvasBrushColor(canvasData, WHITE)

    pixelVisionOS:ChangeCanvasTool(canvasData, tools[1])

    pixelVisionOS:EnableMenuItem(RevertShortcut, false)

    -- Only enable the clear menu when the sprite is not empty
    pixelVisionOS:EnableMenuItem(ClearShortcut, not IsEmpty(tmpPixelData))


    pixelVisionOS:EnableMenuItem(CopyShortcut, true)

    ClearHistory()
    -- print("OnSelectChar", value)

end

function DrawSampleText(spacing)

    local rect = NewRect(8, 192, 240, 16)

    DrawRect(rect.x, rect.y, rect.width, rect.height, 0, DrawMode.TilemapCache)

    spacing = spacing or tonumber(editorUI:GetStringStepperValue(spacingStepper))
    
    local charWidth = 8 + spacing

    local wrap = WordWrap(message, math.floor(rect.width / charWidth))
    local lines = SplitLines(wrap)
    local total = #lines
    local startY = (rect.y + rect.height / 8) - total

    local charOffset = 32

    -- Make the text white (based on the tool's own colors)
    local colorOffset = 15

    -- We want to render the text from the bottom of the screen so we offset it and loop backwards.
    for i = total, 1, - 1 do

        local line = lines[i]

        -- Calculated once per line
        local y = ((i - 1) * 8) + rect.y

        for j = 1, #line do
            local charID = string.byte (line, j) - charOffset

            pixelData = gameEditor:FontSprite(charID)

            if(IsEmpty(pixelData) and charID ~= 0) then

                pixelData = Sprite(_G["missingchar"].spriteIDs[1])

            end

            local x = ((j - 1) * charWidth) + rect.x

            DrawPixels(pixelData, x, y, 8, 8, false, false, DrawMode.TilemapCache, WHITE)

        end

    end

end

function OnSave()

    -- TODO need to save all of the colors back to the game

    -- local currentFontName =

    -- Write the font name to the save file so it will reload correctly
    --  WriteSaveData("fontName", currentFontName == fontName and "none" or currentFontName)
    --
    -- Update the font name
    --fontName = currentFontName

    local oldFontName = fontName

    -- Save the font and get the new font name back (in case there is a name conflict in the folder)
    fontName = gameEditor:SaveFont(fontNameInputData.text)

    -- Save the font back to the meta data
    WriteMetadata("file", rootDirectory .. fontName)

    -- Update the input field to show any changes to the font name
    editorUI:ChangeInputField(fontNameInputData, string.split(fontName, ".")[1], false)

    if(oldFontName ~= fontName) then

        pixelVisionOS:ShowMessageModal(toolName .. " Warning", "The font was renamed from '".. string.split(oldFontName, ".")[1] .."' to '" .. string.split(fontName, ".")[1] .."'.", 160, false,
            function(value)
                -- Clear the validation
                ResetDataValidation()

            end
        )
    else
        -- Display a message that everything was saved
        pixelVisionOS:DisplayMessage("Your changes to '" .. string.split(fontName, ".")[1] .. "' have been saved.", 5)
        -- Clear the validation
        ResetDataValidation()
    end



end

function OnFontNameChange(value)

    if(fontName ~= value .. ".font.png") then
        InvalidateData()
        -- print("Rename Font", value)
    end

end

function Update(timeDelta)

    -- Convert timeDelta to a float
    timeDelta = timeDelta / 1000

    -- This needs to be the first call to make sure all of the editor UI is updated first
    pixelVisionOS:Update(timeDelta)

    -- We only want to run this when a modal isn't active. Mostly to stop the tool if there is an error modal on load
    if(pixelVisionOS:IsModalActive() == false) then


        if(imageLoaded == false) then

            local percent = ReadPreloaderPercent()
            pixelVisionOS:DisplayMessage("Loading font " .. percent .. "%")

            -- If preloading is done, exit the loading loop
            if(percent >= 100) then
                imageLoaded = true

                OnFontLoaded()

            end

        end

        if(targetFile ~= nil and toolLoaded == true) then

            editorUI:UpdatePicker(charPicker)
            editorUI:UpdateInputField(fontNameInputData)
            pixelVisionOS:UpdateCanvas(canvasData)
            editorUI:UpdateStepper(spacingStepper)
            editorUI:UpdateStepper(charStepper)

            if(IsExporting()) then
                pixelVisionOS:DisplayMessage("Saving " .. tostring(ReadExportPercent()).. "% complete.", 2)
            end

        end

    end

end

function Draw()

    RedrawDisplay()

    -- The ui should be the last thing to update after your own custom draw calls
    pixelVisionOS:Draw()

end

function Shutdown()

    -- print("Writing", "fontName", fontName)
    -- Save the current session ID
    WriteSaveData("sessionID", SessionID())
    WriteSaveData("rootDirectory", rootDirectory)
    -- WriteSaveData("tab", tostring(colorTabBtnData.currentSelection))
    -- WriteSaveData("selected", CalculateRealIndex(systemColorPickerData.picker.selected))

end

function OnSpriteBuilder()

    local count = gameEditor:RunSpriteBuilder(rootDirectory)

    if(count > 0) then
        pixelVisionOS:DisplayMessage(count .. " sprites were generated in the 'sb-sprites.lua' file.")
    else
        pixelVisionOS:DisplayMessage("No sprites were found in the 'SpriteBuilder' folder.")
    end

end

function OnQuit()

    if(invalid == true) then

        pixelVisionOS:ShowMessageModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before you quit?", 160, true,
            function()
                if(pixelVisionOS.messageModal.selectionValue == true) then
                    -- Save changes
                    OnSave()

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

function UpdateHistory(pixelData)

    -- local historyAction = {
    --     -- sound = settingsString,
    --     Action = function()

    --         canvasData.paintCanvas:SetPixels(pixelData)

    --         OnSaveCanvasChanges()


    --     end
    -- }

    -- pixelVisionOS:AddUndoHistory(historyAction)

    -- -- We only want to update the buttons in some situations
    -- -- if(updateButtons ~= false) then
    -- UpdateHistoryButtons()
    -- end

end

-- local historyPos = 1



function OnRunGame()
    -- TODO should this ask to launch the game first?

    if(invalid == true) then

        pixelVisionOS:ShowMessageModal("Unsaved Changes", "You have unsaved changes. You will lose those changes if you run the game now?", 160, true,
            function()
                if(pixelVisionOS.messageModal.selectionValue == true) then
                    LoadGame(NewWorkspacePath(rootDirectory))
                end

            end
        )

    else

        LoadGame(NewWorkspacePath(rootDirectory))

    end

end

-- TODO reconnect undo history

function OnUndo()

    -- local action = pixelVisionOS:Undo()

    -- if(action ~= nil and action.Action ~= nil) then
    --     action.Action()
    -- end

    -- UpdateHistoryButtons()
end

function OnRedo()

    -- local action = pixelVisionOS:Redo()

    -- if(action ~= nil and action.Action ~= nil) then
    --     action.Action()
    -- end

    -- UpdateHistoryButtons()
end

function UpdateHistoryButtons()

    -- pixelVisionOS:EnableMenuItem(UndoShortcut, pixelVisionOS:IsUndoable())
    -- pixelVisionOS:EnableMenuItem(RedoShortcut, pixelVisionOS:IsRedoable())

end

function ClearHistory()

    -- Reset history
    -- pixelVisionOS:ResetUndoHistory()
    -- UpdateHistoryButtons()

end