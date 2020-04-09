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

function EditorUI:CreateTextButton(rect, text, toolTip, colorOffset)

    -- Create the button's default data
    local data = self:CreateData(rect, nil, toolTip)

    data.doubleClick = false
    data.doubleClickTime = 0
    data.doubleClickDelay = .45
    data.doubleClickActive = false

    -- By default, we don't want buttons to redraw the background
    data.redrawBackground = false
    data.bgColorOverride = nil

    -- Customize the default name by adding Button to it
    data.name = "TextButton" .. data.name

    -- Internal CallBacks (These can be re-mapped when needed)

    -- On click
    data.onClick = function(tmpData)

        -- Only trigger the click action when the last pressed button name matches
        if(self.currentButtonDown == tmpData.name) then
            self:ClickButton(tmpData, true, tmpData.doubleClickActive and tmpData.doubleClickTime < tmpData.doubleClickDelay)

            tmpData.doubleClickTime = 0
            tmpData.doubleClickActive = true
            tmpData.doubleClick = true
        end
    end

    -- On First Press (Called when the button)
    data.onFirstPress = function(tmpData)

        -- Save the name of the button that was just pressed
        self.currentButtonDown = tmpData.name

        self:PressButton(tmpData, true)
    end

    -- On Redraw
    data.onRedraw = function(tmpData)
        self:RedrawTextButton(tmpData)
    end

    local sprites = BuildTextButton(text)

    data.colorOffset = colorOffset

    if(sprites ~= nil) then

        -- Update the UI tile width and height
        data.tiles.w = #sprites / 2
        data.tiles.h = 2

        -- Update the rect width and height with the new sprite size
        data.rect.w = data.tiles.w * self.spriteSize.x
        data.rect.h = data.tiles.h * self.spriteSize.y

        data.cachedSpriteData = {
            disabled = {sprites = sprites, width = #sprites, colorOffset = data.colorOffset},
            up = {sprites = sprites, width = #sprites, colorOffset = data.colorOffset + 16},
            over = {sprites = sprites, width = #sprites, colorOffset = data.colorOffset + 16 + 16},
            down = {sprites = sprites, width = #sprites, colorOffset = data.colorOffset + 16 + 16 + 16},
            selectedup = {sprites = sprites, width = #sprites, colorOffset = data.colorOffset + 16 + 16 + 16},
            selectedover = {sprites = sprites, width = #sprites, colorOffset = data.colorOffset + 16 + 16 + 16 + 16},
            -- selecteddown = _G[spriteName .. "selecteddown"] ~= nil and _G[spriteName .. "selecteddown"] or _G[spriteName .. "selectedover"],

            -- empty = _G[spriteName .. "empty"] -- used to clear the sprites
        }

        spriteData = data.cachedSpriteData.up or data.cachedSpriteData.disabled

        -- Cache the tile draw arguments for rendering
        data.spriteDrawArgs = {data.sprites, 0, 0, spriteData.width, false, false, DrawMode.Sprite, 0, false, false}
        data.tileDrawArgs = {data.sprites, data.rect.x, data.rect.y, spriteData.width, false, false, DrawMode.TilemapCache, 0}
        data.bgDrawArgs = {data.rect.x, data.rect.y, data.rect.w, data.rect.h, BackgroundColor(), DrawMode.TilemapCache}

    end

    return data

end

function EditorUI:UpdateTextButton(data, hitRect)

    -- Make sure we have data to work with and the component isn't disabled, if not return out of the update method
    if(data == nil) then
        return
    end

    -- If the button has data but it's not enabled exit out of the update
    if(data.enabled == false) then

        -- If the button is disabled but still in focus we need to remove focus
        if(data.inFocus == true) then
            self:ClearFocus(data)
        end

        -- See if the button needs to be redrawn.
        data.onRedraw(data)
        -- Shouldn't update the button if its disabled
        return

    end

    -- Make sure we don't detect a collision if the mouse is down but not over this button
    if(self.collisionManager.mouseDown and data.inFocus == false) then
        -- See if the button needs to be redrawn.
        -- self:RedrawTextButton(data)
        data.onRedraw(data)
        return
    end

    -- If the hit rect hasn't been overridden, then use the buttons own hit rect
    if(hitRect == nil) then
        hitRect = data.hitRect or data.rect
    end

    local overrideFocus = (data.inFocus == true and self.collisionManager.mouseDown)

    -- Ready to test finer collision if needed
    if(self.collisionManager:MouseInRect(hitRect) == true or overrideFocus) then

        if(data.doubleClick == true) then

            -- If the button wasn't in focus before, reset the timer since it's about to get focus
            if(data.inFocus == false) then
                data.doubleClickTime = 0
                data.doubleClickActive = false
            end

            data.doubleClickTime = data.doubleClickTime + self.timeDelta
            if(data.doubleClickActive and data.doubleClickTime > data.doubleClickDelay) then
                data.doubleClickActive = false
            end
        end

        -- If we are in the collision area, set the focus
        self:SetFocus(data)

        -- calculate the correct button over state
        local state = self.collisionManager.mouseDown and "down" or "over"

        if(data.selected == true) then
            state = "selected" .. state
        end

        local spriteData = data.cachedSpriteData ~= nil and data.cachedSpriteData[state] or nil

        if(spriteData ~= nil and data.spriteDrawArgs ~= nil) then

            -- -- Sprite Data
            -- data.spriteDrawArgs[1] = spriteData.spriteIDs
            --
            -- -- X pos
            -- data.spriteDrawArgs[2] = data.rect.x
            --
            -- -- Y pos
            -- data.spriteDrawArgs[3] = data.rect.y
            --
            -- -- Color Offset
            -- data.spriteDrawArgs[8] = spriteData.colorOffset or 0
            --
            -- self:NewDraw("DrawSprites", data.spriteDrawArgs)

            self:DrawTextButton(data.cachedSpriteData[state].sprites, data.rect.x, data.rect.y, DrawMode.Sprite, data.cachedSpriteData[state].colorOffset)

        end

        -- Check to see if the button is pressed and has an onAction callback
        if(self.collisionManager.mouseReleased == true) then

            -- Click the button
            data.onClick(data)
            data.firstPress = true
        elseif(self.collisionManager.mouseDown) then

            if(data.firstPress ~= false) then

                -- Call the onPress method for the button
                data.onFirstPress(data)

                -- Change the flag so we don't trigger first press again
                data.firstPress = false
            end
        end

    else

        if(data.inFocus == true) then
            data.firstPress = true
            -- If we are not in the button's rect, clear the focus
            self:ClearFocus(data)

        end

    end

    data.onRedraw(data)

end

function EditorUI:RedrawTextButton(data, stateOverride)

    if(data == nil) then
        return
    end



    -- If the button changes state we need to redraw it to the tilemap
    if(data.invalid == true or stateOverride ~= nil) then

        print("Draw")

        -- The default state is up
        local state = "up"

        if(stateOverride ~= nil) then
            state = stateOverride
        else

            -- If the button is selected, we will use the selected up state
            if(data.selected == true) then
                state = "selected" .. state
            end

            -- Test to see if the button is disabled. If there is a disabled sprite data, we'll change the state to disabled. By default, always use the up state.
            if(data.enabled == false and data.cachedSpriteData["disabled"] ~= nil and data.selected ~= true) then --_G[spriteName .. "disabled"] ~= nil) then
                state = "disabled"

            end

        end

        -- Test to see if the sprite data exist before updating the tiles
        if(data.cachedSpriteData ~= nil and data.cachedSpriteData[state] ~= nil and data.tileDrawArgs ~= nil) then

            print("spriteData", dump(data.cachedSpriteData[state]))

            self:DrawTextButton(data.cachedSpriteData[state].sprites, data.rect.x, data.rect.y, DrawMode.TilemapCache, data.cachedSpriteData[state].colorOffset)
            --
            -- -- Update the tile draw arguments
            -- data.tileDrawArgs[1] = data.cachedSpriteData[state].spriteIDs
            --
            -- -- Color offset
            -- data.tileDrawArgs[8] = data.cachedSpriteData[state].colorOffset or 0
            --
            -- if(data.redrawBackground == true) then
            --
            --     -- Make sure we always have the current BG color
            --     data.bgDrawArgs[5] = data.bgColorOverride ~= nil and data.bgColorOverride or BackgroundColor()
            --
            --     self:NewDraw("DrawRect", data.bgDrawArgs)
            --
            -- end
            --
            -- self:NewDraw("DrawSprites", data.tileDrawArgs)

        end

        self:ResetValidation(data)

    end

end

function EditorUI:ButtonTester(text, x, y, drawMode, colorOffset)

    local sprites = self:BuildTextButton(text)

    self:DrawTextButton(sprites, x, y, drawMode, colorOffset)

end

function EditorUI:BuildTextButton(text)

    -- Default button sprites
    local sprites = {
        textbuttonleft,
        textbuttonright
    }

    local total = #text

    for i = 1, total do

        local spriteData = nil
        local char = string.sub(text, i, i)

        if(char == " ") then
            spriteData = textbuttonmiddle
        else
            spriteData = _G["textbutton" .. char:lower()]
        end

        if(spriteData ~= nil) then
            table.insert(sprites, #sprites, spriteData)
        end
    end

    return sprites

end

function EditorUI:DrawTextButton(sprites, x, y, drawMode, colorOffset, shiftX)

    shiftX = shiftX or 0

    local total = #sprites

    if(#sprites % 2 > 0) then
        x = x + shiftX
    end

    for i = 1, total do
        local spriteData = sprites[i]

        DrawSprites( spriteData.spriteIDs, x + ((i - 1) * 4), y, spriteData.width, false, false, drawMode, colorOffset )

    end

end
