function PaintTool:ColorCheck()
    
    if(string.ends(self.targetFile, "colors.png") == false) then

        local messagePath = ""

        -- Get the current path where the file is being loaded from
        local currentPath = NewWorkspacePath(self.rootDirectory)

        -- If the current path is a Sprites folder, move up a directory
        if(currentPath.EntityName == "Sprites") then
            currentPath = currentPath.ParentPath;
        end

        print("Dir", currentPath, self.useDefaultColors, self.colorsPath, pixelVisionOS:ValidateGameInDir(currentPath))

        -- Check to see if the current path is a valid game
        if(pixelVisionOS:ValidateGameInDir(currentPath)) then
        
            -- Look for a colors.png file
            if(PathExists(currentPath.AppendFile("colors.png"))) then

                -- Set the colors path to colors.png file
                self.colorsPath = currentPath.ParentPath.AppendFile("colors.png").Path

                -- Update the message to match the colors.png path
                messagePath = currentPath.ParentPath.EntityName

            else

                -- Update the message path to be PV8
                messagePath = "PixelVision8"

                -- Set the color path to default
                self.colorsPath = "default"

            end

        end

        if(self.useDefaultColors and (self.colorsPath == "default" or  self.colorsPath == "none")) then
          self.colorsPath = ""
        end
    
        if(self.colorsPath ~= "") then
    
          self.importColorMessage = "The " .. self.toolName .. " has detected another color option:\n\n'" .. messagePath .. "/colors.png'\n\nDo you want to use these colors instead and replace the sprite colors?"
    
        end
    
    end

    pixelVisionOS:RegisterUI({name = "OnDelayColorImport"}, "DelayColorImport", self)

end

function PaintTool:DelayColorImport()

    -- print("DelayColorImport")

    if(self.colorsPath ~= "") then

        -- Clear color flag
        self.colorsPath = ""

        self:OnImportColors(self.colorsPath)

    end

    pixelVisionOS:RemoveUI("OnDelayColorImport")

end

function PaintTool:OnImportColors(path)

    local buttons =
    {
      {
        name = "modalyesbutton",
        action = function(target)
          self:ImportColorsFrom(path)
          target.onParentClose()
          self:ChangeMode(ColorMode)
        end,
        key = Keys.Enter,
        tooltip = "Press 'enter' to save"
      },
      {
        name = "modalnobutton",
        action = function(target)
          target.onParentClose()
        end,
        key = Keys.N,
        tooltip = "Press 'n' to not save"
      }
    }
    
    pixelVisionOS:ShowMessageModal("Import Colors", self.importColorMessage, 160, buttons)
  
  
  end

  function PaintTool:ImportColorsFrom(path)

    local colors = nil

    if(path ~= nil and path ~= "") then
      
        if(path ~= "default") then
            
          local filePath = NewWorkspacePath(path)

          if(PathExists(filePath)) then

              local colorsFile = ReadImage(filePath)

              -- Get a reference to the image's colors 
              colors = colorsFile.colors

          end

        end
    
    end

    if(colors == nil) then
      colors = self.defaultColors
    end

    self.image.RemapColors(colors)

    self.imageLayerCanvas.SetPixels(0, 0, self.image.Width, self.image.Height, self.image.GetPixels())

    self:ImportColors(colors)
    
  end
  
  function PaintTool:ImportColors(colors)

    
    
    if(colors == nil) then
      colors = self.defaultColors
    end

    -- TODO need a way to remap colors
    -- We need to save the default tool colors to use when showing the color editor modal
    self.cachedToolColors = {}
    
    local tmpColorIndex, tmpRefIndex = 0, 0
  
    -- Loop through all of the system colors
    for i = 1, self.totalColors do
  
      -- Calculate the real color index
      tmpColorIndex = i-1
  
      -- Get the color accounting for Lua's 1 based arrays
      self.cachedToolColors[i] = Color(tmpColorIndex)
  
      -- Test to see if the color is after the colorOffset
      if(i > self.colorOffset) then
        
        -- Map the index back to the image's colors
        tmpRefIndex = i - self.colorOffset
  
        -- Set the new color from the image or to the default mask
        Color(tmpColorIndex, tmpRefIndex <= #colors and colors[tmpRefIndex] or MaskColor())
  
      end
     
    end
  
    self:InvalidateColors()

  end

  function PaintTool:SystemColors()
    
    local systemColors = {}

    for i = 1, self.colorOffset do
      table.insert(systemColors, Color(i-1))
    end

    return systemColors
    
  end

  function PaintTool:GameColors()
    
    local colors = {}

    -- Read colors from memory
    for i = self.colorOffset, 256 do
      table.insert(colors, Color( i))
    end
    
    -- Pad colors to 256
    if(#colors < 256) then
      
      local maskHex = Color( self.maskColor )
      local total = 256 - #colors

      for i = 1, total do
        table.insert(colors, maskHex)
      end

    end

    return colors

  end

  function PaintTool:UniqueColorsIds()

    -- Create a table to store the unique colors
    local uniqueColorIds = {}

    -- Loop through all of the colors and pull out the unique colors
    for i = self.colorOffset, 255 do
        
        -- Get the color stating at the colorOffset
        local tmpColor = Color(i)

        -- Make sure it is not transparent
        if(tmpColor ~= MaskColor()) then
            
            -- Add to the unique color table
            table.insert(uniqueColorIds, i)

        end

    end

    return uniqueColorIds
      
  end

  function PaintTool:UniqueColors()

    -- Create a table to store the unique colors
    local uniqueColors = {}

    -- Loop through all of the colors and pull out the unique colors
    for i = self.colorOffset, 255 do
        
        -- Get the color stating at the colorOffset
        local tmpColor = Color(i)

        -- Make sure it is not transparent
        if(tmpColor ~= MaskColor()) then
            
            -- Add to the unique color table
            table.insert(uniqueColors, tmpColor)

        end

    end

    return uniqueColors
      
  end