--
-- Copyright (c) 2017, Jesse Freeman. All rights reserved.
--
-- Licensed under the Microsoft Public License (MS-PL) License.
-- See LICENSE file in the project root for full license information.
--
-- Contributors
-- --------------------------------------------------------
-- This is the official list of Pixel Vision 8 contributors:
--
-- Jesse Freeman - @JesseFreeman
-- Christina-Antoinette Neofotistou - @CastPixel
-- Christer Kaitila - @McFunkypants
-- Pedro Medeiros - @saint11
-- Shawn Rakowski - @shwany
--

EditorUI = {}
EditorUI.__index = EditorUI

-- Core Framework UI Components
LoadScript("pixel-vision-ui-utils-v2")
LoadScript("pixel-vision-ui-collision-manager-v2")
LoadScript("pixel-vision-ui-slider-v4")
LoadScript("pixel-vision-ui-knob-v3")
LoadScript("pixel-vision-ui-button-v3")
LoadScript("pixel-vision-ui-toggle-group-v2")
LoadScript("pixel-vision-ui-text-editor-v3")
LoadScript("pixel-vision-ui-input-field-v3")
LoadScript("pixel-vision-ui-input-area-v3")
LoadScript("pixel-vision-ui-mouse-cursor-v2")
LoadScript("pixel-vision-ui-picker-v3")
LoadScript("pixel-vision-ui-number-stepper-v2")
LoadScript("pixel-vision-ui-string-stepper-v2")

function EditorUI:Init()

    -- Create a new object for the instance and register it
    local _editorUI = {}
    setmetatable(_editorUI, EditorUI)

    -- Track the current frame's time delta
    _editorUI.timeDelta = 0

    -- Get a reference of the sprite size
    _editorUI.spriteSize = SpriteSize()

    -- Create collision manager instance
    _editorUI.collisionManager = CollisionManager:Init()

    -- Create mouse cursor instance
    _editorUI.mouseCursor = MouseCursor:Init()

    _editorUI.focus = nil

    _editorUI.cursorID = 1
    _editorUI.clearCursor = false

    _editorUI.refreshDelay = .1
    _editorUI.refreshTime = 0

    -- _editorUI.drawCalls = {}
    -- _editorUI.drawCallTotal = 0

    _editorUI.codeEditorClipboardValue = nil

    -- Return the new instance of the editor ui
    return _editorUI

end

function EditorUI:Update(timeDelta)

    -- We need to store the last time delta to sync up all of the UI components
    self.timeDelta = timeDelta

    -- Update the refresh time counter
    self.refreshTime = self.refreshTime + self.timeDelta

    -- Reset the refresh counter
    if(self.refreshTime > self.refreshDelay) then
        self.refreshTime = 0

        -- Delay the mouse cursor by the refresh rate so it doesn't flicker
        self.mouseCursor:SetCursor(self.cursorID)

    end

    -- Update the collision manager first since the other components need a reference to the collision state
    self.collisionManager:Update(self.timeDelta)

    -- Update the mouse cursor
    self.mouseCursor:Update(self.timeDelta, self.collisionManager)

end

function EditorUI:Draw()

    -- Draw the mouse cursor. This should be the last UI draw call so it is always on top.
    self.mouseCursor:Draw()

end

function EditorUI:Shutdown()

end

function EditorUI:CreateData(rect, spriteName, toolTip, forceDraw)

    -- TODO this should use the NewRect() Api and not a rect object
    local data = {
        rect = rect,
        spriteName = spriteName,
        invalid = forceDraw or true,
        enabled = true,
        toolTip = toolTip,
        inFocus = false,
    }

    -- Create a base name for the UI data
    data.name = "UI"

    -- If there is a position for the component, calculate the tile map position
    if(data.rect ~= nil) then

        -- We need to make sure there is a width and height on the rect to calculate the tile dimensions
        if(data.rect.w == nil) then
            data.rect.w = 0
        end

        if(data.rect.h == nil) then
            data.rect.h = 0
        end

        -- print(dump(data.rect.x / self.spriteSize.x))
        -- Calculate tile dimensions
        data.tiles = {
            c = math.floor(data.rect.x / self.spriteSize.x),
            r = math.floor(data.rect.y / self.spriteSize.y),
            w = math.ceil(data.rect.w / self.spriteSize.x),
            h = math.ceil(data.rect.h / self.spriteSize.y)
        }

        -- If the component has a position, append the tile column and row to the name to make it more unique
        data.name = data.name .. ":"..data.tiles.c..","..data.tiles.r

    end

    
    self:RebuildMetaSpriteCache(data)

    return data

end

function EditorUI:RebuildMetaSpriteCache(data, spriteName)

    -- Look for the sprite name or use the one that is provided
    spriteName = spriteName or data.spriteName

    -- If a sprite name is provided then look for the correct sprite states
    if(spriteName ~= nil) then

        -- Make sure the sprite name is up to date
        data.spriteName = spriteName

        -- Find meta sprites for each of the button states
        data.cachedMetaSpriteIds = {
            up = FindMetaSpriteId(spriteName .. "up"),
            down = FindMetaSpriteId(spriteName .. "down" ~= nil and spriteName .. "down" or spriteName .. "selectedup"),
            over = FindMetaSpriteId(spriteName .. "over"),
            selectedup = FindMetaSpriteId(spriteName .. "selectedup"),
            selectedover = FindMetaSpriteId(spriteName .. "selectedover"),
            selecteddown = FindMetaSpriteId(spriteName .. "selecteddown" ~= nil and spriteName .. "selecteddown" or spriteName .. "selectedover"),
            disabled = FindMetaSpriteId(spriteName .. "disabled"),
            empty = FindMetaSpriteId(spriteName .. "empty") -- used to clear the sprites
        }

    end

    -- Invalidate the button so it redraws on the next frame
    self:Invalidate(data)

end

function EditorUI:Invalidate(data)
    data.invalid = true
end

function EditorUI:ResetValidation(data)
    data.invalid = false
end

function EditorUI:Enable(data, value)
    data.enabled = value
    self:Invalidate(data)
end

function EditorUI:SetFocus(data, cursor)

    -- Don't set focus if the mouse was just released
    if(self.collisionManager.mouseReleased == true) then
        return
    end

    -- Update the cursor no matter what
    self.cursorID = cursor or 2

    -- Only do the collision testing if the focus is not already set to true
    if(data.inFocus == true) then
        return
    end

    if(self.collisionManager.mouseDown and self.inFocusUI ~= nil) then

        if(self.inFocusUI.name ~= data.name) then
            return
        end

    end

    -- Check to see if the passed in component is in focus
    data.inFocus = true

    -- Set the current component's data to be in focus in the editor UI
    self.inFocusUI = data

end

function EditorUI:ClearFocus(data)

    self.lastInFocusUI = self.inFocusUI

    -- Clear all focus if no data is provided
    if(data == nil) then
        self.cursorID = 1
        if(self.inFocusUI ~= nil) then
            self.inFocusUI.inFocus = false
            self.inFocusUI = nil
        end
        return
    end

    -- See if the component is in focus
    if(data.inFocus == false) then
        return
    end

    -- Set the component to not be in focus
    data.inFocus = false

    -- Return the cursor back to the pointer
    self.cursorID = 1

    -- Clear the focus of the last UI object
    if(self.inFocusUI ~= nil) then
        self.inFocusUI.inFocus = false
    end

    self.inFocusUI = nil

end
