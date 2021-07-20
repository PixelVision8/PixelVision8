--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--


function PaintTool:CreateDropDownMenu()

  -- create a variable for the edit color modal
  self.editColorModal = EditColorModal:Init()

  -- self.resizeModal = ResizeModal:Init()

  -- Create a table with all of the menu options
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
      {name = "Select All", action = function() self:OnSelectAll() end, key = Keys.A, enabled = true, toolTip = "Learn about PV8."},

      {divider = true},
      {name = "Edit Color", action = function() self:OnEditColor() end, enabled = false, key = Keys.E, toolTip = "Learn about PV8."},
      {name = "Fill Color", action = function() self:SetFillColor() end, enabled = false, toolTip = "Learn about PV8."},
      {name = "BG Color", action = function() self:OnSetBackgroundColor() end, enabled = false, toolTip = "Learn about PV8."},
      {name = "Mask Color", action = function() self:OnEditColor(self.maskColor) end, enabled = false, toolTip = "Learn about PV8."},

      {divider = true},
      {name = "Canvas Size", action = function() self:OnResize() end, enabled = true, key = Keys.I, toolTip = "Learn about PV8."},
      {name = "Toggle BG", action = function() self:ToggleBackground() end, key = Keys.B, toolTip = "Learn about PV8."},
      {name = "Toggle Grid", action = function() self:ToggleGrid() end, key = Keys.G, toolTip = "Learn about PV8."},

      {divider = true},
      {name = "Export", action = function() self:OnExport() end, enabled = true, toolTip = "Learn about PV8."},
      -- {name = "Export Sprites", action = function() self:OnExportSprites() end, enabled = false, toolTip = "Learn about PV8."},
      -- {name = "Export Flags", action = function() self:OnExportFlags() end, enabled = false, toolTip = "Learn about PV8."},
      {name = "Save", action = function() self:OnSave() end, key = Keys.S, toolTip = "Learn about PV8."},
      {divider = true},
      {name = "Run Game", action = function() self:OnRunGame() end, key = Keys.R, toolTip = "Learn about PV8."},
      {name = "Quit", key = Keys.Q, action = function()  self:OnQuit() end, toolTip = "Quit the current game."}, -- Quit the current game
  }

  -- Create a list of menu options that are enabled when the selection tool is active
  self.selectionOptions = {
    "Cut",
    "Copy",
    "Fill",
    "Flip H",
    "Flip V",
  }

  self.colorOptions = {
    "Edit Color",
    "Outline Color",
    "Fill Color",
    "BG Color",
    "Mask Color",
  }

  -- Create the menu bar
  pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")

end

function PaintTool:OnSelectAll()
  

  if(self.imageLayerCanvas.Width * self.imageLayerCanvas.Height > 512 * 512) then
    
    local buttons =
    {
      {
        name = "modalyesbutton",
        action = function(target)
          self:SelectAll()
          target.onParentClose()
          
        end,
        key = Keys.Enter,
        tooltip = "Press 'enter' to continue"
      },
      {
        name = "modalnobutton",
        action = function(target)
          target.onParentClose()
        end,
        key = Keys.N,
        tooltip = "Press 'n' to cancel"
      }
    }
    
    pixelVisionOS:ShowMessageModal("Large Selection", "You are about to select a large area of pixels which may impact the performance of Pixel Vision 8. Do you still want to select all?", 160, buttons)
  

  else
    self:SelectAll()
  end
  
end

function PaintTool:Cut()

  -- Call copy
  self:Copy()

  -- Delete the selection
  self:Delete()

end

function PaintTool:OnResize()

  pixelVisionOS:RemoveUI("OnUpdateToolbar")

  local title = "Resize"
  local message = "Do you want to resize the current canvas? This process can not be undone."
  local width = 160
  local buttons =
    {
      {
        name = "modalyesbutton",
        action = function(target)

          target.onParentClose()
        end,
        key = Keys.Y,
        tooltip = "Press 'y' to resize the canvas"
      },
      {
        name = "modalnobutton",
        action = function(target)
          target.onParentClose()
        end,
        key = Keys.N,
        tooltip = "Press 'n' to cancel making changes"
      }
    }
  
  -- Look to see if the modal exists
  if(self.resizeModal == nil) then

      -- Create the model
      self.resizeModal = ResizeModal:Init(title, message, width, buttons, self.imageLayerCanvas.Width, self.imageLayerCanvas.Height)

      -- Pass a reference of the editorUI to the modal
      self.resizeModal.editorUI = self.editorUI
  -- end
  else
      -- If the modal exists, configure it with the new values
      self.resizeModal:Configure(title, message, width, buttons, self.imageLayerCanvas.Width, self.imageLayerCanvas.Height)--showCancel, okButtonSpriteName, cancelButtonSpriteName)
  end

  self:CancelCanvasSelection()

  -- TODO need to clear undo history
  pixelVisionOS:ResetUndoHistory(self)

  pixelVisionOS:UpdateHistoryButtons(self)

  -- Open the modal
  pixelVisionOS:OpenModal(self.resizeModal, function()
  
    pixelVisionOS:RegisterUI({name = "OnUpdateToolbar"}, "UpdateToolbar", self, true)
    
    local newWidth = tonumber(self.resizeModal.colInputData.text) * 8
    local newHeight = tonumber(self.resizeModal.rowInputData.text) * 8

    if(self.imageLayerCanvas.Width == newWidth and self.imageLayerCanvas.Height == newHeight) then
      return
    end

    self.imageLayerCanvas:Resize(newWidth, newHeight, true)
    
    self.backgroundLayerCanvas:Resize(newWidth, newHeight)
    self.tmpLayerCanvas:Resize(newWidth, newHeight)
    self.flagLayerCanvas:Resize(newWidth, newHeight)
    self.gridCanvas:Resize(newWidth, newHeight)

    self:InvalidateBackground()
    self:InvalidateGrid()
    self:InvalidateCanvas()

    self:ChangeScale( self.scaleValues[self.scaleMode])

  end
  )

end

function PaintTool:UpdateResize()


end

function PaintTool:OnExportColors()

  
  
  local dest = NewWorkspacePath(self.rootDirectory).AppendFile("colors.png")

  if(PathExists(dest)) then

    pixelVisionOS:ShowSaveModal("A Color File Exists", "Looks like a 'colors.png' file already exits? Do you want to replace it with the current image's colors?", 160,
      -- Accept
      function(target)
        self:ExportColors(dest)
        self.exporting = false
        target.onParentClose()
      end,
      -- Decline
      function (target)
        
        self:ExportColors(UniqueFilePath(dest))
        self.exporting = false
        target.onParentClose()
        
      end,
      -- Cancel
      function(target)
        self.exporting = false
        target.onParentClose()
      end
    )
  else

    self:ExportColors(dest)
    self.exporting = false

  end

end

function PaintTool:ExportColors(dest)
  
  local colorIndexes = {}

  for i = 1, 256 do
    table.insert(colorIndexes, i-1)
  end

  local image = NewImage(8, 32, colorIndexes, self:GameColors())

  -- Save image to the workspace
  SaveImage(dest, image, Color( self.maskColor ))

  -- Display a message that the save was successful
  pixelVisionOS:DisplayMessage("Saving a new '".. dest.EntityName .. "' file.")

end

function PaintTool:OnExport()

  -- Configure the title and message
  local title = "Export"
  local message = "It's important to note that performing this optimization may break any places where you have hardcoded references to sprite IDs. You will have the option to apply the optimization after the sprites are processed. \n\nDo you want to perform the following?\n\n"


  local callbacks = {}

  message = message .. "#  Colors - A new colors.png file\n"
  table.insert(callbacks, function () self:OnExportColors() end)

  if(self.canExportSprites == true) then
    message = message .. "#  Sprites - An optimized sprites.png file\n"
    table.insert(callbacks, function () self:OnExportSprites() end)
  end

  if(self.canExportFlags == true) then
    message = message .. "#  Flags - A new flags.png template file\n"
    table.insert(callbacks, function () self:OnExportFlags() end)
  end

  -- Create the new warning model
  local warningModal = OptionModal:Init(title, message .. "\n", 216, true)

  self.exportQueue = {}
  self.exporting = false

  pixelVisionOS:OpenModal( warningModal,
    
        function()

          local selections = editorUI:ToggleGroupSelections(warningModal.optionGroupData)

          for i = 1, #selections do
            
            table.insert(self.exportQueue, callbacks[selections[i]])

          end

          if(#self.exportQueue > 0) then
            
            self.currentExportIndex = 1
            
            pixelVisionOS:RegisterUI({name = "OnUpdateExport"}, "UpdateExport", self)
--        
          end

        end
    )
end

function PaintTool:UpdateExport()

  if(self.currentExportIndex >= #self.exportQueue) then

    self.exporting = false

    pixelVisionOS:RemoveUI("OnUpdateExport")

  end

  if(self.exporting == false) then

    self.exporting = true

    self.exportQueue[self.currentExportIndex]()

    self.currentExportIndex = self.currentExportIndex + 1

  end

end

-- TODO need to make this reusable

function PaintTool:OnExportSprites()
  local dest = NewWorkspacePath(self.rootDirectory).AppendFile("sprites.png")

  if(PathExists(dest)) then

    pixelVisionOS:ShowSaveModal("A Sprite File Exists", "Looks like a 'sprites.png' file already exits? Do you want to replace it with the current sprites?", 160,
      -- Accept
      function(target)
        self:ExportSprites(dest)
        self.exporting = false
        target.onParentClose()
      end,
      -- Decline
      function (target)
        
        self:ExportSprites(UniqueFilePath(dest))
        self.exporting = false
        target.onParentClose()
        
      end,
      -- Cancel
      function(target)
        self.exporting = false
        target.onParentClose()
      end
    )
  else

    self:ExportSprites(dest)
    self.exporting = false

  end

end

function PaintTool:ExportSprites(dest)

  local maskColor = Color( self.maskColor )

  local systemColors = {}

  for i = 1, 256 do
    table.insert(systemColors, i < self.colorOffset and maskColor or Color(i-1))
  end

  local spritesImage = NewImage(self.spriteCanvas.Width, self.spriteCanvas.Height, self.spriteCanvas.Pixels, systemColors)

  SaveImage(dest, spritesImage, maskColor)

  -- Display a message that the save was successful
  pixelVisionOS:DisplayMessage("Saving a new '".. dest.EntityName .. "' file.")

end

function PaintTool:OnExportFlags()

  local dest = NewWorkspacePath(self.rootDirectory).AppendFile("flags.png")

  if(PathExists(dest)) then

    pixelVisionOS:ShowSaveModal("A Flags File Exists", "Looks like a 'flags.png' file already exits? Do you want to replace it with the current flag layer?", 160,
      -- Accept
      function(target)
        self:ExportFlags(dest)
        self.exporting = false
        target.onParentClose()
      end,
      -- Decline
      function (target)
        
        self:ExportFlags(UniqueFilePath(dest))
        self.exporting = false
        target.onParentClose()
        
      end,
      -- Cancel
      function(target)
        self.exporting = false
        target.onParentClose()
      end
    )
  else

    self:ExportFlags(dest)
    self.exporting = false

  end

end

function PaintTool:ExportFlags(dest)

  local maskColor = Color( self.maskColor )

  local flagSpriteImage = NewImage(self.flagCanvas.Width, self.flagCanvas.Height, self.flagCanvas.Pixels, self:SystemColors())

  SaveImage(dest, flagSpriteImage, maskColor)

  -- Display a message that the save was successful
  pixelVisionOS:DisplayMessage("Saving a new '".. dest.EntityName .. "' file.")

end

function PaintTool:Delete()

  local srcCanvas = self.pickerMode == FlagMode and self.flagLayerCanvas or self.imageLayerCanvas

  -- TODO this needs to look at the right canvas based on the mode (image vs flag)
  srcCanvas:Clear(-1, self.selectRect.X, self.selectRect.Y, self.selectRect.Width, self.selectRect.Height )
      
  -- Clear the select pixel data so it doesn't get saved back to the canvas
  self.selectedPixelData = nil

  self:CancelCanvasSelection()

end

function PaintTool:Copy()

  -- cut pixel data from selection and clear pixels
  -- local pixelData = self.imageLayerCanvas:GetPixels(self.selectRect.X, self.selectRect.Y, self.selectRect.Width, self.selectRect.Height)

  local tmpPixels = ""

  for i = 1, #self.selectedPixelData.Pixels do
    tmpPixels = tmpPixels .. self.selectedPixelData.Pixels[i] .. ","
  end

  -- TODO should save mode
  -- Convert to a string
  local data = self.pickerMode .. "PixelData-" .. self.selectRect.X.. "," .. self.selectRect.Y .. "," .. self.selectRect.Width .. "," .. self.selectRect.Height .. ":" .. tmpPixels


  -- print("Copy data", #data, "#tmpPixels", #tmpPixels, #string.split(tmpPixels, ","))

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
  
  -- print("paste data", #data, "#tmpPixels", #tmpPixels, self.selectRect.Width * self.selectRect.Height)

  -- print("Pixels", total, self.selectRect.Width * self.selectRect.Height, split[2])
  
  -- Make sure the total equals the selection rect's dimensions
  if(total == self.selectRect.Width * self.selectRect.Height) then
    
    if(self.selectedPixelData ~= nil) then

      self:CancelCanvasSelection()

    end

    self.selectedPixelData = NewCanvas( self.selectRect.Width, self.selectRect.Height)

    for i = 1, total do
      self.selectedPixelData.Pixels[i-1] = tonumber(test[i])
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
  self.selectedPixelData:Flip(true, false)
end

function PaintTool:FlipV()
  self.selectedPixelData:Flip(false, true)
end

function PaintTool:OnRunGame()


  local parentPath = self.targetFilePath.ParentPath

  if(self.invalid == true) then

      pixelVisionOS:ShowSaveModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before running the game?", 160,
        -- Accept
        function(target)
          self:OnSave()
          LoadGame(parentPath.Path)
        end,
        -- Decline
        function (target)
          LoadGame(parentPath.Path)
        end,
        -- Cancel
        function(target)
          target.onParentClose()
        end
      )

  else
      -- Quit the tool
      LoadGame(parentPath.Path)
  end

end

function PaintTool:OnQuit()

  if(self.invalid == true) then

    pixelVisionOS:ShowSaveModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before you quit?", 160,
      -- Accept
      function(target)
        self:OnSave()
        QuitCurrentTool()
      end,
      -- Decline
      function (target)
        QuitCurrentTool()
      end,
      -- Cancel
      function(target)
        target.onParentClose()
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

  -- Calculate save area to sample from accounting for off screen changes
  local tmpX = math.max(0, self.scaledViewport.X - self.scaledViewport.Width)
  local tmpY = math.max(0, self.scaledViewport.Y - self.scaledViewport.Height)
  local tmpW = math.min(width, self.scaledViewport.Width * 3)
  local tmpH = math.min(height, self.scaledViewport.Height * 3)

  -- Get the pixels from the canvas
  local pixels = srcCanvas:GetPixels(tmpX, tmpY, tmpW, tmpH)

  
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
        srcCanvas:SetPixels(tmpX, tmpY, tmpW, tmpH, pixels)

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

  -- We'll invalidate the tool's data since any time an undo action can happen, the canvas has changed
  self:InvalidateData()
  
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

  self.backgroundMode = self.backgroundMode + 1

  if(self.backgroundMode > 3) then
    self.backgroundMode = 1
  end

  self:InvalidateBackground()

end

function PaintTool:ToggleGrid()

  self.gridMode = not self.gridMode

  self:InvalidateCanvas()
  self:InvalidateGrid()

end

function PaintTool:SwapToolColors()
  local currentColors = {}
  local colorIndex = 0

  for i = 1, self.totalColors do
    
    -- Calculate the color index accounting for the Lua's 1 based arrays
    colorIndex = i-1

    -- Save the current color
    currentColors[i] = Color(colorIndex)
    
    -- Change the color to the default editor colors
    Color(colorIndex, self.toolColors[i])

  end

  return currentColors

end

function PaintTool:RestoreColors(currentColors)

  -- Restore previous colors before the modal was opened
  for i = 1, self.totalColors do
    Color(i-1, currentColors[i])
  end

end

function PaintTool:OnEditColor(colorId)

  pixelVisionOS:RemoveUI("OnUpdateToolbar")

  -- Get the color Id from what is passed in or the picker
  colorId = colorId or self.currentState.selectedId  + self.colorOffset

  -- Read the color HEX from memory before editing using the color offset
  local currentColor = Color(colorId)

  -- Save all of the colors before opening the modal
  local currentColors = self:SwapToolColors()
  

  -- Open the modal
  pixelVisionOS:OpenModal(self.editColorModal,
      function()

        -- TODO need to read the modified color

        self:RestoreColors(currentColors)

        self:InvalidateColors()

        -- Register the update loop
        pixelVisionOS:RegisterUI({name = "OnUpdateToolbar"}, "UpdateToolbar", self, true)

        if(self.editColorModal.selectionValue == true and currentColor ~= "#" .. self.editColorModal.colorHexInputData.text) then

          Color(colorId, "#" .. self.editColorModal.colorHexInputData.text)

          self:InvalidateData()
          
          return
        
        end

      end,
      -- Optional arguments
      currentColor, currentColors, self.maskColor, string.format("EDIT COLOR %03d", colorId) 
  )

end

function PaintTool:SetFillColor() 
  -- TODO need to wire this up
  -- print("Change outline color")

  self.fillColor = self.currentState.selectedId

end

function PaintTool:OnSetBackgroundColor()

  self.backgroundColorId = self.currentState.selectedId

  self:InvalidateBackground()
  -- print(self.currentState.selectedId)
  -- TODO need to wire this up
  -- print("Change BG color")
end