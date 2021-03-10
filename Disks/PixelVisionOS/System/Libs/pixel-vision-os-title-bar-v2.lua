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

function PixelVisionOS:CreateTitleBar(x, y, title)

    local data = {} -- our new object

    data.pos = {
        x = x or 0,
        y = y or 0
    }

    data.invalid = true
    data.textColorOffset = 15
    data.font = "medium"
    data.fontSpacing = -4
    data.lastTimeStamp = ""
    data.timeDelay = .3
    data.time = .3
    data.invalid = true
    data.showTimeDivider = true
    data.title = "Untitled"
    data.debugTime = ReadBiosData("DebugTime") == "True"
    data.charWidth = 4
    data.timeTextTemplate = (data.debugTime and "SAT" or "%a") .. "         1985"
    data.timeOffsetX = Display().x - 72 -- time text chars * charWidth + offset (8)

    -- Create the time mask sprite data
    data.timeMask = {}

    for i = 1, 8 * 4 do
        data.timeMask[i] = 0
    end

    self.editorUI:Invalidate(data)

    DrawMetaSprite(FindMetaSpriteId("titlebarbackground"), 0, 0, false, false, DrawMode.TilemapCache)

    -- Fix scope for lamda functions below
    local this = self

    -- Create mute button
    data.iconButton = self.editorUI:CreateButton({x = 8, y = 0}, "pv8toolbaricon", "Options and shortcuts for this tool.")

    data.iconButton.hitRect = {x = 8, y = 0, w = 13, h = 11}
    data.iconButton.onPress = function()
        -- print("Show menu")
        this.titleBar.menu.showMenu = true
    end
    data.iconButton.onAction = function(value)
        this.titleBar.menu.showMenu = false
        data.iconButton.toolTip = this.titleBar.menu.defaultToolTip
    end

    -- Disable the button but default until the tool creates an option menu
    self.editorUI:Enable(data.iconButton, false)

    

    -- Create mute button
    data.muteBtnData = self.editorUI:CreateButton({x = 172, y = 0}, "", "Toggle systme wide mute.")
    data.muteBtnData.hitRect = {x = data.muteBtnData.rect.x, y = data.muteBtnData.rect.y, w = 8, h = 11}

    data.muteBtnData.onAction = function()
        Mute(not Mute())
        data.muteInvalid = true
        -- this.lastMuteValue = nil
    end

    data.muteBtnData.selected = Mute()
    data.muteInvalid = true

    return data

end

-- This is a helper for changing the text on the title bar
function PixelVisionOS:ChangeTitle(text, titleIconName)

    DrawRect(30, 0, 140, 8, 0, DrawMode.TilemapCache)

    local maxChars = 34
    if(#text > maxChars) then
        text = text:sub(0, maxChars - 3) .. "..."
    else
        text = string.rpad(text, maxChars, "")
    end

    self.titleBar.titleIcon = FindMetaSpriteId(titleIconName)-- and _G[titleIconName].spriteIDs[1] or nil
    self.titleBar.title = string.upper(text)
    self.editorUI:Invalidate(self.titleBar)

end

function PixelVisionOS:CreateTitleBarMenu(items, toolTip)

    -- Get a reference to the iconButton
    local iconButton = self.titleBar.iconButton

    self.editorUI:Enable(iconButton, true)
    iconButton.toolTip = toolTip

    -- TODO Button draws should always happen on the first frame? (This breaks in tilemap tool since it loads in wait mode)
    -- Force this to draw just incase the tool loads up in wait mode
    self.editorUI:UpdateButton(iconButton)

    local data = {
        options = items,
        menuSelection = -1,
        showMenu = false,
        defaultToolTip = toolTip,
        shortcuts = {}
    }

    -- Get the total number of options
    local totalOptions = #data.options

    local itemHeight = 9
    local dividerHeight = 4

    local tmpW = 92
    local tmpH = 0

    for i = 1, totalOptions do

        local tmpOption = data.options[i]

        tmpOption.height = tmpOption.divider == true and dividerHeight or itemHeight

        tmpOption.y = tmpH

        tmpH = tmpH + tmpOption.height

    end

    -- Create menu canvas
    local canvas = NewCanvas(tmpW, tmpH + 10+ 2)

    -- Set the canvas stroke to be 2 x 2 pixels wide
    canvas:SetStroke(0, 2)

    -- Create a solid background pattern
    canvas:SetPattern({12}, 1, 1)

    -- Draw background and border
    canvas:DrawRectangle(0, 0, canvas.width - 8, canvas.height - 8, true)
    --canvas.wrap = false

    --canvas:Draw()

    local tmpCanvas = NewCanvas(canvas.width - 12, itemHeight)
    --tmpCanvas.wrap = false

    local pos = NewPoint(6, 9)

    for i = 1, totalOptions do

        local option = data.options[i]

        if(option.key ~= nil) then
            table.insert(data.shortcuts, option.key)
        end

        local tmpX, tmpY = 2, option.y + 2

        -- Create up pixel data
        tmpCanvas:Clear()

        -- Draw the up state
        self:DrawTitleBarMenuItem(tmpCanvas, option, 14)

        canvas:MergePixels(tmpX, tmpY, tmpCanvas.width, tmpCanvas.height, tmpCanvas:GetPixels(), false, false, 0, true)

        if(option.divider ~= true) then
            -- Create over pixel data
            tmpCanvas:Clear(14)

            option.rect = NewRect(
                tmpX + pos.x,
                tmpY + pos.y - 2,
                tmpCanvas.width,
                tmpCanvas.height
            )

            -- Draw the over state
            self:DrawTitleBarMenuItem(tmpCanvas, option)

            -- Save the over state
            option.overPixelData = 
            {
                tmpCanvas:GetPixels(),
                tmpX + pos.x,
                tmpY + pos.y,
                tmpCanvas.width,
                tmpCanvas.height,
                false,
                false,
                DrawMode.SpriteAbove
            }
        end

    end

    data.canvas = canvas

    data.menuDrawArgs = {
        canvas.GetPixels(),
        pos.x,
        pos.y,
        canvas.width,
        canvas.height,
        false,
        false,
        DrawMode.SpriteAbove,
        0
    }

    -- adjust the rect to match the correct layout position
    -- Save the menu values to the data object
    data.rect = NewRect(pos.x - 2, pos.y + 2, data.menuDrawArgs[4] - 4, data.menuDrawArgs[5] - 4)

    -- Save data back to the title bar data object
    self.titleBar.menu = data

    -- The menu is create so reset its validation
    self.editorUI:ResetValidation(data)

    return data

end

function PixelVisionOS:DrawTitleBarMenuItem(canvas, option, bgColor2)

    local bgColor = 14
    bgColor2 = option.enabled == false and 11 or bgColor2

    local divColor = 5
    local t1Color = option.enabled == false and 11 or 0
    local t2Color = t2Color or 12

    if(option.divider == true) then

        canvas:SetStroke(divColor, 1)
        local y = 2
        canvas:DrawLine(0, y, canvas.width, y)

    else

        canvas:DrawText(option.name:upper(), 4, 0, "medium", t1Color, -4)

        if(option.key ~= nil) then

            canvas:SetStroke(bgColor2 or bgColor, 1)
            if(bgColor2 ~= nil) then
                canvas:SetPattern({bgColor2}, 1, 1)
            end

            local tmpX = canvas.width - 16
            local tmpY = 1

            canvas:DrawRectangle(tmpX, tmpY, 12, 7, true)
            
            canvas:DrawText(("^" .. tostring(option.key)):upper(), tmpX + 2, tmpY - 1, "small", t2Color, -4)
            
        end

    end

end

function PixelVisionOS:UpdateTitleBar(data, timeDelta)

    -- Keep track of time passed since last frame
    data.time = data.time + timeDelta

    -- Enable and disable elements based on the status of any active modals
    if(self:IsModalActive() == true) then
        self.editorUI:Enable(data.muteBtnData, false)
    else
        self.editorUI:Enable(data.muteBtnData, true)
    end

    -- Only update these buttons if the mouse is not in wait mode
    if(self.editorUI.mouseCursor.cursorID ~= 5) then

        -- Update buttons
        self.editorUI:UpdateButton(data.iconButton)
        self.editorUI:UpdateButton(data.muteBtnData)

    end

    self:DrawTitleBar(data)

    local menuData = self.titleBar.menu

    if(menuData ~= nil) then

        -- Loop through all the options and see what needs to be done

        if(menuData.showMenu ~= false) then

            -- First, we want to test that the iconButton hasn't lost focus
            if(data.iconButton.inFocus == false) then

                menuData.showMenu = false
                -- Restore default tooltip

                data.iconButton.toolTip = menuData.defaultToolTip

                if(menuData.menuSelection > 0) then
                    local option = menuData.options[menuData.menuSelection]

                    if(option.action ~= nil) then
                        option.action()
                    end
                end

            end

        end

        -- local mousePos = MousePosition()

        -- If we are showing the menu, reset the selction and tooltip
        if(menuData.showMenu == true) then

            -- Reset mouse selection while we loop through all the options
            menuData.menuSelection = -1

            -- Clear the icon button's tooltip
            data.iconButton.toolTip = ""

        end

        for i = 1, #menuData.options do

            local option = menuData.options[i]

            -- First, check to see if there is a shortcut key
            if(option.key ~= nil and option.enabled ~= false and self:IsModalActive() == false) then

                -- Either the left or right control key needs to be down
                local triggerShortcut = Key(Keys.LeftControl) or Key(Keys.RightControl)

                -- If you can trigger the shortcut, check to see if a key was released
                if(triggerShortcut and Key(option.key, InputState.Released)) then

                    -- Call the option's action function
                    if(option.action ~= nil) then
                        option.action()

                        -- Exit the for loop
                        break

                    end

                end

            end
            if(option.enabled ~= false) then
                -- Test for collision
                if(menuData.showMenu == true and option.rect ~= nil and option.rect.Contains(editorUI.mouseCursor.pos.x, editorUI.mouseCursor.pos.y)) then

                    -- Update menu selection
                    menuData.menuSelection = i

                    -- Update the icon button's tool tip
                    data.iconButton.toolTip = option.toolTip

                    if(option.subMenuOptions ~= nil) then

                        menuData.showSubMenu = i
                        print("has sub menu", option.subMenuOptions ~= nil, option.rect.x)

                    end

                    break

                end
            end

        end

    end

    if(data.time > data.timeDelay) then

        local newTimeStamp = data.debugTime == true and "08:00AM" or string.upper(CurrentTime())

        if(newTimeStamp ~= data.lastTimeStamp) then

            DrawRect( 200, 0, 32, 8, 0, DrawMode.TilemapCache )

            DrawText(newTimeStamp, 200, 1, DrawMode.TilemapCache, data.font, data.textColorOffset, data.fontSpacing)

            data.lastTimeStamp = newTimeStamp

        end

        if(data.showTimeDivider == true) then
            data.showTimeDivider = false
        else
            data.showTimeDivider = true
        end
        data.time = 0
    end

end

function PixelVisionOS:EnableMenuItemByName( name, value)

    local options = self.titleBar.menu.options

    local total = #options

    for i = 1, total do

        local option = options[i]

        if(option.name == name) then

            if(option.enabled ~= value) then
                option.enabled = value
                self.editorUI:Invalidate(self.titleBar.menu)
            end

        end

    end

end

function PixelVisionOS:EnableMenuItem( id, value)

    local menuData = self.titleBar.menu

    if(menuData.options[id].enabled ~= value) then

        local option = self.titleBar.menu.options[id]

        menuData.options[id].enabled = value

        self.editorUI:Invalidate(menuData)

    end

end

function PixelVisionOS:DrawTitleBar(data)

    local menuData = self.titleBar.menu

    -- Redraw the menu bar
    if(menuData ~= nil and menuData.invalid == true) then

        menuData = self:CreateTitleBarMenu(menuData.options, menuData.defaultToolTip)

        self.editorUI:ResetValidation(menuData)

    end

    if(data.invalid == true) then
        
        DrawText(
            string.upper(os.date(data.timeTextTemplate)),
            data.timeOffsetX,
            1,
            DrawMode.TilemapCache,
            data.font,
            data.textColorOffset,
            data.fontSpacing
        )

        -- Draw title icon
        if(data.titleIcon ~= nil) then
            DrawMetaSprite(
                data.titleIcon,
                23,
                2,
                false,
                false,
                DrawMode.TilemapCache
            )
            
        end

        DrawText( 
            data.title,
            23 + 8,
            1,
            DrawMode.TilemapCache,
            data.font,
            data.textColorOffset,
            data.fontSpacing
        )

        data.lastTimeStamp = ""

        -- Reset the titlebar validation
        self.editorUI:ResetValidation(data)
    end

    if(data.showTimeDivider == true) then

        DrawPixels( 
            data.timeMask,
            208,
            0,
            4,
            8,
            false,
            false,
            DrawMode.Sprite,
            0
        )

    end

    if(data.muteInvalid == true) then

        DrawMetaSprite(
            FindMetaSpriteId(Mute() and "titlebarvolumeoff" or "titlebarvolumeon"),
            data.muteBtnData.rect.x,
            data.muteBtnData.rect.y,
            false,
            false,
            DrawMode.TilemapCache
        )
       
        data.muteInvalid = false

    end

    local menuData = self.titleBar.menu

    if(menuData ~= nil and menuData.showMenu ~= false) then

        menuData.canvas:DrawPixels(menuData.menuDrawArgs[2], menuData.menuDrawArgs[3], DrawMode.UI)

        if(menuData.menuSelection > 0) then
            
            local pixelData = menuData.options[menuData.menuSelection].overPixelData

            if(pixelData ~= nil)then
                
                DrawPixels(
                    pixelData[1],
                    pixelData[2],
                    pixelData[3],
                    pixelData[4],
                    pixelData[5],
                    false,
                    false,
                    DrawMode.SpriteAbove
                )
                
            end

        end

    end


end
