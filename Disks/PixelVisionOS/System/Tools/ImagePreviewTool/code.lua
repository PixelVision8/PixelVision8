--[[
	Pixel Vision 8 - Debug Tool
	Copyright (C) 2016, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- Load in the editor framework script to access tool components
LoadScript("sb-sprites")
LoadScript("pixel-vision-os-v2")

local toolName = "Image Preview"
local debugMode = false
local pixelVisionOS = nil
local editorUI = nil
local invalid = true
local rootDirectory = nil

local image = nil
local imageCanvas = nil
local viewportRect = NewRect(0, 0, 224, 184)
local boundaryRect = NewRect(0,0,0,0)
local displayInvalid = true

function Init()

    BackgroundColor(5)

    -- Disable the back key in this tool
    EnableBackKey(false)

    EnableAutoRun(false)

    -- Create an instance of the Pixel Vision OS
    pixelVisionOS = PixelVisionOS:Init()

    -- Get a reference to the Editor UI
    editorUI = pixelVisionOS.editorUI

    rootDirectory = ReadMetadata("directory", nil)

    -- Get the target file
    targetFile = ReadMetadata("file", nil)

    -- targetFile = "/Workspace/Games/GGSystem/code.lua"
    if(targetFile ~= nil) then

        -- Load the image
        image = ReadImage(NewWorkspacePath(targetFile))

        -- TODO need to copy out colors from the image  
        local imageColors = image.colors
        local totalColors = math.max(8, #imageColors)
        
        -- Get the last color index for the offset
        local colorOffset = TotalColors()

        -- Double the color memory
        ResizeColorMemory(512)

        -- Create the canvas to display the image color palette
        colorMemoryCanvas = NewCanvas(8, totalColors / 8)
        local pixels = {}

        -- Add the image colors to the color chip
        for i = 1, totalColors do
            Color(colorOffset + (i-1), imageColors[i])

            local index = i + 255
            table.insert(pixels, index)
        end

            -- TODO each pixel should be set above
        colorMemoryCanvas:SetPixels(pixels)
        
        -- Get the image pixels
        local pixelData = image.GetPixels()

        -- TODO we may not need to do this if we just offset the canvas when drawing?
        -- Shift colors
        for i = 1, #pixelData do
            pixelData[i] = pixelData[i] + colorOffset
        end

        -- Create a new canvas
        imageCanvas = NewCanvas(image.width, image.height)

        -- Copy the modified image pixel data over to the new canvas
        imageCanvas.SetPixels(pixelData)

        -- The image is loaded at this point
        toolLoaded = true
        
        -- Create tool title from path
        local pathSplit = string.split(targetFile, "/")
        toolTitle = pathSplit[#pathSplit - 1] .. "/" .. pathSplit[#pathSplit]


        -- Configure the menu
        ConfigureMenu()

        CreateViewport()

        -- Reset Tool Validation
        ResetDataValidation()

    else

        pixelVisionOS:ChangeTitle(toolName, "toolbaricontool")

        pixelVisionOS:ShowMessageModal(toolName .. " Error", "The tool could not load without a reference to a file to edit.", 160, false,
            function()
                QuitCurrentTool()
            end
        )
    end

end

function ConfigureMenu()

    -- pixelVisionOS:ImportColorsFromGame()

    local menuOptions = 
    {
        -- About ID 1
        {name = "About", action = function() pixelVisionOS:ShowAboutModal(toolName) end, toolTip = "Learn about PV8."},

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

function CreateViewport()
    
    -- Setup viewport
    viewportRect.Width = math.min(imageCanvas.width, viewportRect.Width)
    viewportRect.Height = math.min(imageCanvas.height, viewportRect.Height)

    -- Calculate the boundary for scrolling
    boundaryRect.Width = imageCanvas.width - viewportRect.Width
    boundaryRect.Height = imageCanvas.height - viewportRect.Height

    if(boundaryRect.Height > 0) then
        vSliderData = editorUI:CreateSlider({x = 235, y = 20, w = 10, h = 193}, "vsliderhandle", "Scroll text vertically.")
        vSliderData.onAction = OnVerticalScroll
    end

    if(boundaryRect.Width > 0) then
        hSliderData = editorUI:CreateSlider({ x = 4, y = 211, w = 233, h = 10}, "hsliderhandle", "Scroll text horizontally.", true)
        hSliderData.onAction = OnHorizontalScroll
    end

    InvalidateDisplay()
    
end

function InvalidateDisplay()
    displayInvalid = true
end

function ResetMapValidation()
    displayInvalid = false
end

function InvalidateData()

    -- Only everything if it needs to be
    if(invalid == true)then
        return
    end

    pixelVisionOS:ChangeTitle(toolTitle .."*", "toolbariconfile")

    invalid = true

end

function ResetDataValidation()

    -- Only everything if it needs to be
    if(invalid == false)then
        return
    end

    pixelVisionOS:ChangeTitle(toolTitle, "toolbariconfile")
    invalid = false

end

function OnHorizontalScroll(value)

    -- TODO this is wrong but works when I use ABS... need to fix it
    viewportRect.X = math.abs(math.floor(((viewportRect.Width - boundaryRect.Width) - viewportRect.Width) * value))

    InvalidateDisplay()
end

function OnVerticalScroll(value)

    viewportRect.Y = math.abs(math.floor(((viewportRect.Height - boundaryRect.Height) - viewportRect.Height) * value))

    InvalidateDisplay()
end



function Update(timeDelta)

    -- Convert timeDelta to a float
    timeDelta = timeDelta / 1000

    -- This needs to be the first call to make sure all of the editor UI is updated first
    pixelVisionOS:Update(timeDelta)

    -- We only want to run this when a modal isn't active. Mostly to stop the tool if there is an error modal on load
    if(pixelVisionOS:IsModalActive() == false) then

        -- -- Only update the tool's UI when the modal isn't active
        if(targetFile ~= nil and toolLoaded == true) then

            -- Update the slider
            editorUI:UpdateSlider(vSliderData)

            -- Update the slider
            editorUI:UpdateSlider(hSliderData)

        end

    end

end

function Draw()


    RedrawDisplay()

    if(displayInvalid == true and pixelVisionOS:IsModalActive() == false) then

        -- Draw the pixel data in the upper left hand cornver of the tool's window
        imageCanvas:DrawPixels(8, 24, DrawMode.TilemapCache, 1, -1, -1, 0, viewportRect)

        if(debugMode) then
            colorMemoryCanvas:DrawPixels(8, 24, DrawMode.UI, 3)
        end
    end

    -- The ui should be the last thing to update after your own custom draw calls
    pixelVisionOS:Draw()

end

function OnSavePNG(color, sprite, tilemap)

    local label = "none"
    local menuID = 0
    if(color) then
        label = "colors"
        menuID = 3
    elseif(sprite) then
        label = "sprites"
        menuID = 4
    elseif(tilemap) then
        label = "tilemap"
        menuID = 5
    end

    pixelVisionOS:ShowMessageModal("Export " .. label, "This will override any existing file in the directory. Do you want to do this?", 160, true,
        function()
            if(pixelVisionOS.messageModal.selectionValue == true) then
                -- Save changes

                local flags = {}

                if(color == true) then

                    -- Add the color map flag
                    table.insert(flags, SaveFlags.Colors)

                end

                if(sprite == true) then

                    -- Add the color map flag
                    table.insert(flags, SaveFlags.Sprites)

                end

                if(tilemap == true) then

                    -- Add the color map flag
                    table.insert(flags, SaveFlags.Tilemap)

                end

                -- This will save the system data, the colors and color-map
                gameEditor:Save(rootDirectory, flags)

                pixelVisionOS:EnableMenuItem(menuID, false)

            end

        end
    )


end
