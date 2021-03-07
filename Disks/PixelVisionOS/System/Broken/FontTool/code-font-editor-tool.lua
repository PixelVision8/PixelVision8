--[[
	Pixel Vision 8 - Debug Tool
	Copyright (C) 2016, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- Create table to store the workspace tool logic
FontEditorTool = {}
FontEditorTool.__index = FontEditorTool

LoadScript("code-drop-down-menu")
LoadScript("code-editor-panel")
LoadScript("code-canvas-panel")

function FontEditorTool:Init()

    -- Create a new table for the instance with default properties
    local _fontEditorTool = {
    
        toolName = "Font Editor",
        runnerName = SystemName(),
        rootPath = ReadMetadata("RootPath", "/"),
        rootDirectory = ReadMetadata("directory", nil),
        targetFile = ReadMetadata("file", nil),
        invalid = true,
        imageLoaded = false,
        characters = {
            " ", "!", "\"", "#", "$", "%", "&", "'", "(" ,")" ,"*" ,"+",
            ", ", "-", ".", "/", "0", "1", "2", "3", "4", "5", "6", "7",
            "8", "9", ":", ";", "<", "=", ">", "?", "@", "A", "B", "C",
            "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P",
            "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "[", "\\",
            "]", "^", "_", "`", "a", "b", "c", "d", "e", "f", "g", "h",
            "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t",
            "u", "v", "w", "x", "y", "z", "{", "|", "}", "~"
        },
        charOffset = 32
    }

    -- Save the total char count
    _fontEditorTool.totalChars = #_fontEditorTool.characters

    pixelVisionOS:ResetUndoHistory(_fontEditorTool)
    
    -- Create a global reference of the new workspace tool
    setmetatable(_fontEditorTool, FontEditorTool)

    -- Used if there is an error
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

    if(_fontEditorTool.targetFile ~= nil) then

        -- print("targetFile", targetFile)

        local pathSplit = string.split(_fontEditorTool.targetFile, "/")

        -- if(SessionID() == ReadSaveData("sessionID", "") and rootDirectory == ReadSaveData("rootDirectory", "")) then
        --   print("Reloading session")
        -- else
        --
        -- end

        _fontEditorTool.fontName = pathSplit[#pathSplit] or _fontEditorTool.lastFont

        if(gameEditor:LoadFont(_fontEditorTool.rootDirectory .. _fontEditorTool.fontName) == false) then

            pixelVisionOS:ChangeTitle(_fontEditorTool.toolName, "toolbaricontool")

            pixelVisionOS:ShowMessageModal(_fontEditorTool.toolName .. " Error", "The tool could not load without a reference to a file to edit.", 160, buttons)

            return

        end

        pixelVisionOS:RegisterUI({name = "PreloadUpdateLoop"}, "PreloadUpdate", _fontEditorTool)

    else

        -- Patch background when loading fails

        -- Left panel
        DrawRect(8, 40, 128, 128, 0, DrawMode.TilemapCache)

        DrawRect(152, 40, 96, 8, 0, DrawMode.TilemapCache)

        DrawRect(152, 152, 96, 24, BackgroundColor(), DrawMode.TilemapCache)

        DrawRect(8, 192, 240, 16, 0, DrawMode.TilemapCache)
        
        pixelVisionOS:ChangeTitle(_fontEditorTooltoolName, "toolbaricontool")

        pixelVisionOS:ShowMessageModal(toolName .. " Error", "The tool could not load without a reference to a file to edit.", 160, false,
            function()
                QuitCurrentTool()
            end
        )

    end

    -- Return the draw tool data
    return _fontEditorTool

end

function FontEditorTool:PreloadUpdate()

    
    if(self.imageLoaded == false) then

        local percent = ReadPreloaderPercent()
        
        pixelVisionOS:DisplayMessage("Loading font " .. percent .. "%")

        -- If preloading is done, exit the loading loop
        if(percent >= 100) then
            self.imageLoaded = true

            pixelVisionOS:RemoveUI("PreloadUpdateLoop")

            self:OnFontLoaded()

        end

    end

end

function FontEditorTool:OnFontLoaded()

    self:CreateDropDownMenu()

    self:CreateCanvasPanel()

    self:CreateEditorPanel()

    self:OnEnterChar(" ")

    self:DrawSampleText()

    self:ResetDataValidation()

    print("Font loaded")
    -- _G["itempickerover"] = {spriteIDs = charselection.spriteIDs, width = charselection.width, colorOffset = 28}

    -- _G["itempickerselectedup"] = {spriteIDs = charselection.spriteIDs, width = charselection.width, colorOffset = (_G["itempickerover"].colorOffset + 2)}

    -- canvasData = pixelVisionOS:CreateCanvas(
    --     {
    --         x = 8,
    --         y = 40,
    --         w = 128,
    --         h = 128
    --     },
    --     {
    --         x = 128,
    --         y = 128
    --     },
    --     1,
    --     0, -- Forcing this to be white
    --     "Draw on the canvas",
    --     0
    -- )

    -- -- canvasData.overDrawArgs[8] = 0

    -- pixelVisionOS:CanvasBrushColor(canvasData, WHITE)

    -- canvasData.onPress = function()

    --     -- Calculate the position in the canvas
    --     local tmpPos = NewPoint(
    --         Clamp(editorUI.collisionManager.mousePos.x - canvasData.rect.x, 0, canvasData.rect.w - 1) / canvasData.scale,
    --         Clamp(editorUI.collisionManager.mousePos.y - canvasData.rect.y, 0, canvasData.rect.h - 1) / canvasData.scale
    --     )

    --     -- See what color is under the mouse by reading the pixel
    --     local tmpColor = canvasData.paintCanvas:ReadPixelAt(tmpPos.x, tmpPos.y)

    --     -- Change the tool bases on what color the mouse is over
    --     pixelVisionOS:ChangeCanvasTool(canvasData, tmpColor == 0 and "pen" or "eraser", 6)

    --     canvasData.inDrawMode = true

    --     UpdateHistory(pixelVisionOS:GetCanvasPixelData(canvasData))
    -- end

    -- canvasData.onAction = OnSaveCanvasChanges

    -- pixelVisionOS:ChangeCanvasPixelSize(canvasData, 1)

    -- charPicker = editorUI:CreatePicker(
    --     {x = 152, y = 72, w = 96, h = 64 },
    --     8,
    --     8,
    --     95,
    --     "itempicker",
    --     "Pick a char"
    -- )

    -- charPicker.onAction = OnSelectChar

    -- for i = 1, #characters do

    --     -- Lua loops start at 1 but we need to start at 0
    --     DrawFontCharacter(i - 1, rect)

    -- end


    -- spacingStepper = editorUI:CreateStringStepper({x = 200, y = 160}, 16, "0", {"-4", "-3", "-2", "-1", "0", "1", "2"}, "", "Change the length of the pattern.")

    -- -- TODO This is a hack becuse entering in numbers by hand could break the return value... example 2 becomes 20.
    -- spacingStepper.inputField.highlighterTheme.disabled = 15
    -- editorUI:Enable(spacingStepper.inputField, false)

    -- spacingStepper.onInputAction = function(value) DrawSampleText(value) end

    -- charStepper = editorUI:CreateStringStepper({x = 152, y = 160}, 8, "A", characters, "", "Change the length of the pattern.")

    -- charStepper.onInputAction = OnEnterChar

    -- -- local pathSplit = string.split(rootDirectory, "/")



    -- -- Update title with file path
    -- -- toolTitle = pathSplit[#pathSplit - 1] .. "/" .. fontName

    -- fontNameInputData = editorUI:CreateInputField({x = 152, y = 40, w = 96}, string.split(fontName, ".")[1], "Give the font a name.", "file")
    -- -- fontNameInputData.min = 0
    -- fontNameInputData.onAction = OnFontNameChange

    -- -- editorUI:Enable(fontNameInputData, false)

    -- -- TODO need to save last selection and restore it
    -- OnEnterChar(" ")

    -- DrawSampleText()

    -- ResetDataValidation()

    -- toolLoaded = true

end

function FontEditorTool:InvalidateData()

    -- Only everything if it needs to be
    if(self.invalid == true)then
        return
    end

    pixelVisionOS:ChangeTitle(self.toolTitle .."*", "toolbariconfile")

    self.invalid = true

    pixelVisionOS:EnableMenuItem(self.SaveShortcut, true)


end

function FontEditorTool:ResetDataValidation()

    -- Only everything if it needs to be
    if(self.invalid == false)then
        return
    end

    -- Update the title with the current font name
    self:UpdateTitleText()

    pixelVisionOS:ChangeTitle(self.toolTitle, "toolbariconfile")
    self.invalid = false

    pixelVisionOS:EnableMenuItem(self.SaveShortcut, false)


end

function FontEditorTool:UpdateTitleText()

    local pathSplit = string.split(self.targetFile, "/")

    -- Update title with file path
    self.toolTitle = pathSplit[#pathSplit - 1] .. "/" .. self.fontName

end

function FontEditorTool:Shutdown()

    WriteSaveData("sessionID", SessionID())
    WriteSaveData("rootDirectory", self.rootDirectory)

end