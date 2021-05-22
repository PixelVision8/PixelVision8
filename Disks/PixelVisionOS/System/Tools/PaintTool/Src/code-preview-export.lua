function PaintTool:OnSavePNG(color, sprite, tilemap)

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