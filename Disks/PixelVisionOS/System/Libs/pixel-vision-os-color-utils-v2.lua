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

function PixelVisionOS:ImportColorsFromGame()

    -- Resize the tool's color memory to 512 so it can store the tool and game colors
    gameEditor:ResizeToolColorMemory()

    -- We'll save the game's mask color
    self.maskColor = gameEditor:MaskColor()

    self.colorsPerSprite = gameEditor:ColorsPerSprite()

    -- Games are capped at 256 colors
    self.totalColors = 256

    self.emptyColorID = self.totalColors - 1

    -- The color offset is the first position where a game's colors are stored in the tool's memory
    self.colorOffset = self.totalColors

    -- Clear all the tool's colors
    for i = 1, self.totalColors do
        local index = i - 1
        Color(index + self.colorOffset, self.maskColor)
    end

    -- Set the color mode
    self.paletteMode = gameEditor:ReadMetadata("paletteMode", "false") == "true"

    -- Calculate the total available system colors based on the palette mode
    self.totalSystemColors = self.paletteMode and self.totalColors / 2 or self.totalColors

    -- There are always 128 total palette colors in memory
    self.totalPaletteColors = 0

    -- We display 64 system colors per page
    self.systemColorsPerPage = 64

    -- We display 16 palette colors per page
    self.paletteColorsPerPage = Clamp(gameEditor:ColorsPerSprite(), 2, 16)

    -- Get all of the game's colors
    local gameColors = gameEditor:Colors()

    -- Create a table for all of the system colors so we can track unique colors
    self.systemColors = {}

    local counter = 0

    -- Loop through all of the system colors and add them to the tool
    for i = 1, self.totalSystemColors do

        -- Calculate the color's index
        local index = i - 1

        -- get the game color at the current index
        local color = gameColors[i]

        local ignoreColor = false

        if(color == self.maskColor or table.indexOf(self.systemColors, color) ~= -1) then
            ignoreColor = true
        end

        -- Look to see if we have the system color or if its not the mask color
        if(ignoreColor == false) then

            -- Reset the index to the last ID of the system color's array
            index = #self.systemColors

            -- Add the system color to the table
            table.insert(self.systemColors, color)

            -- Save the game's color to the tool's memory
            Color(index + self.colorOffset, color)

            counter = counter + 1

            if(counter > gameEditor:MaximumColors()) then

                break

            end
        end

    end

    if(self.paletteMode == true) then

        self.totalPaletteColors = 128

        for i = 129, self.totalColors do

            local index = i - 1

            -- get the game color at the current index
            local color = gameColors[i]

            local colorID = table.indexOf(self.systemColors, color)

            local paletteIndex = (i - 129) % 16

            -- Mask off any colors outside of the palette
            if(paletteIndex >= self.colorsPerSprite) then
                color = self.maskColor

                -- Set any masked colors in a palette to the fist system color
            elseif(colorID == -1 and paletteIndex < self.colorsPerSprite) then
                color = self.maskColor

            end

            Color(index + self.colorOffset, color)

        end

    end

    self.totalSystemColors = #self.systemColors
    -- Add up the palette colors

end



function PixelVisionOS:CopyToolColorsToGameMemory()

    -- Clear the game's colors
    gameEditor:ClearColors()

    -- Copy over all the new system colors from the tool's memory
    for i = 1, self.totalColors do

        -- Calculate the index of the color
        local index = i - 1

        -- Read the color from the tool's memory starting at the system color offset
        local newColor = Color(index + self.colorOffset)

        -- Set the game's color to the tool's color
        gameEditor:Color(index, newColor)

    end

end
