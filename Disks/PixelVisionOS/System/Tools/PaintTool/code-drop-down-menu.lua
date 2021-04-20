--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--


function PaintTool:CreateDropDownMenu()

  local menuOptions = 
  {
      -- About ID 1
      {name = "About", action = function() pixelVisionOS:ShowAboutModal(self.toolName) end, toolTip = "Learn about PV8."},

      {divider = true},
      {name = "Undo", action = function() self:OnUndo() end, key = Keys.Z, enabled = false, toolTip = "Learn about PV8."},
      {name = "Redo", action = function() self:OnRedo() end, key = Keys.Y, enabled = false, toolTip = "Learn about PV8."},

      {divider = true},
      {name = "Cut", action = function() self:Cut() end, key = Keys.X, enabled = false, toolTip = "Learn about PV8."},
      {name = "Copy", action = function() self:Copy() end, key = Keys.C, enabled = false, toolTip = "Learn about PV8."},
      {name = "Paste", action = function() self:Paste() end, key = Keys.V, enabled = false, toolTip = "Learn about PV8."},
      {name = "Fill", action = function() self:FillCanvasSelection() end, key = Keys.F, enabled = false, toolTip = "Learn about PV8."},
      {name = "Flip H", action = function() self:FlipH() end, key = Keys.H, enabled = false, toolTip = "Learn about PV8."},
      {name = "Flip V", action = function() self:FlipV() end, key = Keys.J, enabled = false, toolTip = "Learn about PV8."},
      {name = "Select All", action = function() self:SelectAll() end, key = Keys.A, enabled = true, toolTip = "Learn about PV8."},

      -- {divider = true},
      -- {name = "Line Thicker", action = function()  end, toolTip = "Learn about PV8."},
      -- {name = "Line Thinner", action = function()  end, toolTip = "Learn about PV8."},
      
      {divider = true},
      {name = "Edit Color", action = function()  end, toolTip = "Learn about PV8."},
      {name = "Outline Color", action = function()  end, toolTip = "Learn about PV8."},
      {name = "Fill Color", action = function()  end, toolTip = "Learn about PV8."},
      {name = "BG Color", action = function()  end, toolTip = "Learn about PV8."},
      {name = "Mask Color", action = function()  end, toolTip = "Learn about PV8."},

      {divider = true},
      {name = "Canvas Size", action = function()  end, key = Keys.I, toolTip = "Learn about PV8."},
      -- {name = "Color Mode", action = function()  end, toolTip = "Learn about PV8."},
      -- {name = "Sprite Mode", action = function()  end, toolTip = "Learn about PV8."},
      -- {name = "Flag Mode", action = function()  end, toolTip = "Learn about PV8."},
      {name = "Toggle BG", action = function() self:ToggleBackground() end, key = Keys.B, toolTip = "Learn about PV8."},
      -- {divider = true},
      
      -- {name = "Zoom In", action = function()  end, toolTip = "Learn about PV8."},
      -- {name = "Zoom Out", action = function()  end, toolTip = "Learn about PV8."},
      
      {divider = true},
      {name = "Export Colors", action = function()  end, toolTip = "Learn about PV8."},
      {name = "Export Sprites", action = function()  end, toolTip = "Learn about PV8."},
      {name = "Export Flags", action = function()  end, toolTip = "Learn about PV8."},

      {divider = true},
      {name = "Run Game", action = function()  end, key = Keys.R, toolTip = "Learn about PV8."},
      {name = "Save", action = function()  end, key = Keys.S, toolTip = "Learn about PV8."},
      {name = "Quit", key = Keys.Q, action = QuitCurrentTool, toolTip = "Quit the current game."}, -- Quit the current game
  }

  self.selectionOptions = {
    "Cut",
    "Copy",
    "Fill",
    "Flip H",
    "Flip V",
  }

  pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")

end

function PaintTool:Cut()

  -- Call copy
  self:Copy()

  -- Delete the selection
  self:Delete()

end

function PaintTool:Delete()

  -- TODO this needs to look at the right canvas based on the mode (image vs flag)
  self.imageLayerCanvas:Clear(-1, self.selectRect.X, self.selectRect.Y, self.selectRect.Width, self.selectRect.Height )
      
  -- Clear the select pixel data so it doesn't get saved back to the canvas
  self.selectedPixelData = nil

  self:CancelCanvasSelection()

end

function PaintTool:Copy()

  -- cut pixel data from selection and clear pixels
  -- local pixelData = self.imageLayerCanvas:GetPixels(self.selectRect.X, self.selectRect.Y, self.selectRect.Width, self.selectRect.Height)

  local tmpPixels = ""

  for i = 1, #self.selectedPixelData do
    tmpPixels = tmpPixels .. self.selectedPixelData[i] .. ","
  end

  -- TODO should save mode
  -- Convert to a string
  local data = self.pickerMode .. "PixelData-" .. self.selectRect.X.. "," .. self.selectRect.Y .. "," .. self.selectRect.Width .. "," .. self.selectRect.Height .. ":" .. tmpPixels


  print("Copy data", #data, "#tmpPixels", #tmpPixels, #string.split(tmpPixels, ","))

  -- Save to clipboard
  pixelVisionOS:SystemCopy(data)

end

function PaintTool:Paste()

  local data = pixelVisionOS:SystemPaste()

  
    
  -- Separate the type from the string
  local split = string.split(data, "-")

  -- Save the type
  local type = split[1]

  -- Only continue if the type ends with PixelData
  if(string.ends(type, "PixelData") == false) then
    return
  end

  -- Split the position from the pixel data
  split = string.split(split[2], ":")

  local pos = string.split(split[1], ",")

  -- Only continue if there are 4 position values
  if(#pos ~= 4) then
    return
  end

  -- Create a new selection from position values
  self.selectRect = NewRect(tonumber(pos[1]), tonumber(pos[2]), tonumber(pos[3]), tonumber(pos[4]))

  -- Get the pixels from the string
  -- local tmpPixels = string.split(split[2], ",")


  local test = string.split(data, ":")[2]


  

  local tmpPixels = string.split(test, ",")

  -- Save the total pixels
  local total = #tmpPixels
  
  print("paste data", #data, "#tmpPixels", #tmpPixels, self.selectRect.Width * self.selectRect.Height)

  -- print("Pixels", total, self.selectRect.Width * self.selectRect.Height, split[2])
  
  -- Make sure the total equals the selection rect's dimensions
  if(total == self.selectRect.Width * self.selectRect.Height) then
    
    if(self.selectedPixelData ~= nil) then

      self:CancelCanvasSelection()

    end

    self.selectedPixelData = {}

    for i = 1, total do
      table.insert(self.selectedPixelData, tonumber(test[i]))
    end
  
    -- self:ClampSelectionToBounds()
    
    -- Change the mode based on the
    self:ChangeMode(type:sub(0, - 10))

    -- self.selecti/onState = "none"
    
  else

    -- Clear the selection if the data is not valid
    self:CancelCanvasSelection()

  end





  -- print("Test", )

  -- split = string.split(split[2], ":")

  -- local pos = string.split(split[1], ",") -- TODO need to remove last ) from string

  -- print(type, dump(pos))



  -- TODO check for mode pixelData, spriteData, flagData - split("(")

  -- TODO parse our size and verify - split(")")[1] split(",")

  -- TODO parse our pixel data - split(":") split(",")

  -- TODO go to correct mode (layer)

  -- TODO create the new selection

  -- TODO set pixel data in the selection

end

function PaintTool:FlipH()

end

function PaintTool:FlipV()

end

function PaintTool:OnRunGame()


    local data = {runnerType = self.extension == ".lua" and "lua" or "csharp"}

    -- print("runnerType", data["runnerType"], self.extension)

    -- if(self.codeMode == true) then
    --    data["codeFile"] = _textTool.targetFile
    -- end

    local parentPath = self.targetFilePath.ParentPath

    if(self.invalid == true) then

        pixelVisionOS:ShowMessageModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before running the game?", 160, true,
                function()
                    if(pixelVisionOS.messageModal.selectionValue == true) then
                        -- Save changes
                        self:OnSave()

                    end

                    -- TODO should check that this is a game directory or that this file is at least a code.lua file
                    LoadGame(parentPath.Path, data)
                end
        )

    else
        -- Quit the tool
        LoadGame(parentPath.Path, data)
    end

end

function PaintTool:OnQuit()

      if(self.invalid == true) then

        pixelVisionOS:ShowMessageModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before you quit?", 160, true,
          function()
            if(pixelVisionOS.messageModal.selectionValue == true) then
              -- Save changes
              self:OnSave()

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

function PaintTool:StoreUndoSnapshot()

  -- Select the correct canvas based on image or flag mode
  local srcCanvas = self.pickerMode == FlagMode and self.flagLayerCanvas or self.imageLayerCanvas

  -- Get the size of the canvas
  local width = srcCanvas.Width
  local height = srcCanvas.Height

  -- Get the pixels from the canvas
  local pixels = srcCanvas:GetPixels()

  
  -- TODO save brush state


  -- TODO save picker state
  local pickerMode = self.pickerMode
  
  local selection = nil
  local selectionPixelData = nil

  if(self.selectRect ~= nil) then
    -- Save the selection rect
    selection = NewRect( self.selectRect.X, self.selectRect.Y, self.selectRect.Width, self.selectRect.Height )
    selectionPixelData = self.selectedPixelData

  end

  self.undoState = {
      
      Action = function()

        -- Restore the selection state
        -- if(selection ~= nil) then
          self.selectRect = selection
          self.selectedPixelData = selectionPixelData
        -- else
        --   self
        -- end

        -- Check the size of the image layer vs the last saved state
        if(srcCanvas.Width ~= width or srcCanvas.Height ~= height) then
          self:ResizeCanvas(width, height)
        end

        -- Restore the pixel data
        srcCanvas:SetPixels(pixels)

        -- Restore picker mode
        self:ChangeMode(pickerMode)

        -- Force the tmp canvas to clear
        self.tmpLayerCanvas:Clear()

        -- Force the canvas to redraw
        self:InvalidateCanvas()

      end

  }

end

function PaintTool:InvalidateUndo(canvas, selection)

  -- TODO need to optimize this by using the canvas and selection flags
  self.canvasInvalid = canvas or true
  self.selectionInvalid = selection or true

  self.undoValid = true
  
end

function PaintTool:ResetUndoValidation()
  self.undoValid = false
end

function PaintTool:GetState()

  -- If the previous state is invalid, return the current state
  if(self.undoValid == false) then
    self:StoreUndoSnapshot()
  end
  
  -- Reset the state validation
  self:ResetUndoValidation()

  -- Return the last saved state
  return self.undoState

end

function PaintTool:ResizeCanvas(width, height)

  -- TODO need to resize all of the canvases

  self.imageLayerCanvas:Resize(width, height)
  self.tmpLayerCanvas:Resize(width, height)

end

function PaintTool:ToggleBackground()

  print("Toggle BG")
  self.backgroundMode = self.backgroundMode + 1

  if(self.backgroundMode > 3) then
    self.backgroundMode = 1
  end

  self:InvalidateBackground()

end