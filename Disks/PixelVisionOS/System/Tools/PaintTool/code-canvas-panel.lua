--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

function PaintTool:CreateCanvasPanel()

  self.canvasPanel = {
      name = "canvasPanel"
  }

  self.tmpLayerCanvas = nil
  self.viewportRect = NewRect(8, 48+8, 224, 160-8)
  self.boundaryRect = NewRect(0,0,0,0)
  self.displayInvalid = true
  self.gridSize = 8
  self.snapToGrid = true
  self.mousePos = NewPoint()
  
  -- self.scale = 1
  self.scaledViewport = NewRect()
  self.scaleValues = {.5, 1, 2, 4, 8}--, 8}
  self.scaleMode = 1 -- TODO need to read last scale

  -- Change the pointer to a hand when inside of the component
  self.focusCursor = 2

  self.overCanvas = NewCanvas( 8, 8 )
  self.overCanvas.SetStroke(0, 1);

  -- self.colorOffset = colorOffset
  self.emptyColorID = emptyColorID or - 1

  -- Create empty pixel data we'll use when erasing tiles
  self.emptyPixelData = {}
  
  for i = 1, 64 do
    self.emptyPixelData[i] = self.emptyColorID
  end

  self.fill = false
  self.currentCursorID = 1

  -- Default color is 0
  self.brushColor = 0
  self.brushColorOffset = 0

  self.brushCanvas = NewCanvas( 8, 8 )
  self.brushMaskRect = NewRect( 0, 0, self.brushCanvas.Width, self.brushCanvas.Height )


  self.overColor = -1
  
  self.defaultStrokeWidth = 1
  -- Get the image pixels
  local pixelData = self.image.GetPixels()

  self.backgroundMode = 1

  -- Used to track the pixel shift in the selection outline
  self.selectionTime = 0
  self.selectionDelay = .2
  self.selectionShift = 0

  -- Create a new canvas
  self.imageLayerCanvas = NewCanvas(math.ceil(self.image.width/8) * 8, math.ceil(self.image.height/8) * 8)
  
  -- Copy the modified image pixel data over to the new canvas
  self.imageLayerCanvas.SetPixels(0, 0, self.image.Width, self.image.Height, pixelData)

  -- Create new background canvas
  self.backgroundLayerCanvas = NewCanvas( self.imageLayerCanvas.Width, self.imageLayerCanvas.Height )

  -- Invalidate the background so it renders
  self:InvalidateBackground()
  
  -- Create the tmp layer for drawing shapes and other effects into
  self.tmpLayerCanvas = NewCanvas(self.imageLayerCanvas.Width, self.imageLayerCanvas.Height)
  -- self.tmpLayerCanvas.Clear();

  -- TODO need to parse any flag image or json file in the game project
  self.flagLayerCanvas = NewCanvas( self.imageLayerCanvas.Width, self.imageLayerCanvas.Height )
  -- self.flagLayerCanvas.Clear()

  
  self.vSliderData = editorUI:CreateSlider({x = 235-3, y = 44+3 + 8, w = 10, h = 193-24 - 7 - 8}, "vsliderhandle", "Scroll text vertically.")
  self.vSliderData.onAction = function(value) self:OnVerticalScroll(value) end
  
  self.hSliderData = editorUI:CreateSlider({ x = 4+3, y = 211-3, w = 233-7, h = 10}, "hsliderhandle", "Scroll text horizontally.", true)
  self.hSliderData.onAction = function(value) self:OnHorizontalScroll(value) end
  
  self.sizeBtnData = editorUI:CreateButton({x = 232, y = 208}, "scalemode0", "Pick the scale mode.")
  self.sizeBtnData.onAction = function() self:OnNextZoom() end


  self.currentCanvasLayer = nil

  self:OnNextZoom()

  self:InvalidateCanvas()

  pixelVisionOS:RegisterUI({name = "OnUpdateCanvasPanel"}, "UpdateCanvasPanel", self)
  pixelVisionOS:RegisterUI({name = "OnDrawCanvasPanel"}, "DrawCanvasPanel", self)

  self.onClick = function()
     
      -- Trigger the canvas release
      self:CanvasRelease(true)

      print("Release", self.tool, self.mergerTmpLayer)

      -- If the state was invalidated save the undo
      if(self.undoValid == true) then

        print("Save Undo")

        -- Get the saved state
        pixelVisionOS:BeginUndo(self)

        -- Save the undo state
        pixelVisionOS:EndUndo(self)

      end

  end

  self.triggerOnFirstPress = function()

    print("Press", self.tool, self.mergerTmpLayer)

    -- Save the canvas state to use for undo on release
    self:StoreUndoSnapshot()

    -- Check to make sure the press happens when the canvas actually has focus to avoid trigger when closing button menu over canvas
    if(editorUI.inFocusUI.name ~= self.canvasPanel.name or self.optionMenuOpen == true) then
      return
    end
    
    -- Reset the canvas stroke before we start drawing
    self:ResetCanvasStroke()

    -- TODO should this be on release?
    -- Trigger fill here since it only happens on the fist press
    if(self.tool == "fill") then

      -- TODO need to defer this to the draw function

      -- Update the fill color
      self.imageLayerCanvas:SetPattern({self.brushColor}, 1, 1)

      self.imageLayerCanvas:FloodFill(self.startPos.x, self.startPos.y)

      -- No need to merge the tmp layer since we are drawing directly into the image layer
      self.mergerTmpLayer = false

      self:InvalidateCanvas()

      self:InvalidateUndo()

    end

  end
  
end

function PaintTool:ChangeCanvasLayer(name)

  -- name = name or "image"

  -- print("Change layer", name)

  -- if(name == "image") then

  --   self.

  -- elseif(name == "tmp") then

  -- end


end

function PaintTool:InvalidateCanvas(mergeTmpLayer)
  self.displayInvalid = true
end

function PaintTool:IResetCanvasValidation()
  self.displayInvalid = false
end

function PaintTool:UpdateCanvasPanel(timeDelta)

  -- Update the slider
  editorUI:UpdateSlider(self.vSliderData)

  -- Update the slider
  editorUI:UpdateSlider(self.hSliderData)

  -- Don't draw when the option menu is open
  if(self.optionMenuOpen == true) then
    return
  end

  -- -- Make sure we don't detect a collision if the mouse is down but not over this button
  if(editorUI.collisionManager.mouseDown and self.canvasPanel.inFocus == false) then
  --     -- See if the button needs to be redrawn.
  --     self:InvalidateCanvas()
      return
  end

  -- Change the scale
  if(Key(Keys.OemMinus, InputState.Released)) then

    -- Zoom out if the crtl key is down
    if((Key(Keys.LeftControl) or Key(Keys.RightControl)) and self.scaleMode > 1) then

      self:OnNextZoom(true)

    else

      -- TODO each tool should have its own specific thickness
      self.defaultStrokeWidth = math.max(self.defaultStrokeWidth - 1, 1)

      self:InvalidateBrushPreview()

    end

  elseif(Key(Keys.OemPlus, InputState.Released)) then
  
    -- Zoom in if the crtl key is down
    if((Key(Keys.LeftControl) or Key(Keys.RightControl)) and self.scaleMode <= #self.scaleValues) then

      self:OnNextZoom()

    else

      -- TODO each tool should have its own specific thickness
      self.defaultStrokeWidth = math.min(self.defaultStrokeWidth + 1, 4)

      self:InvalidateBrushPreview()

    end

  elseif(Key(Keys.Escape, InputState.Released)) then
    
    if(self.selectRect ~= nil) then
      self:CancelCanvasSelection()
    end

  elseif(Key(Keys.Delete) or Key(Keys.Back)) then
    
    if(self.selectRect ~= nil) then

      self:Delete()

    end
  end

  editorUI:UpdateButton(self.sizeBtnData)

  local overrideFocus = (self.canvasPanel.inFocus == true and editorUI.collisionManager.mouseDown)

  if(self.viewportRect.Contains(editorUI.mouseCursor.pos) == true or overrideFocus) then

    -- TODO need to adjust for scroll and scale
    self.mousePos.X = math.floor((editorUI.collisionManager.mousePos.x - self.viewportRect.X)/ self.scale) + self.scaledViewport.X
    self.mousePos.Y = math.floor((editorUI.collisionManager.mousePos.y - self.viewportRect.Y)/ self.scale) + self.scaledViewport.Y
    
    if(self.selectRect ~= nil) then
      
      -- If the mouse is inside of the select rect, change the icon
      if(self.selectRect:Contains(self.mousePos) == true and self.selectionState ~= "resize") then

        -- Change the cursor to the drag hand
        self.currentCursorID = 9

      else

        -- Reset the cursor when you leave the selection area
        self.currentCursorID = self.defaultCursorID
        
      end

    else

      -- Reset the cursor when you leave the selection area
      self.currentCursorID = self.defaultCursorID

    end

    -- TODO the mouse calculation logic needs to be cleaned up since it's duplicated all over the place
    -- Adjust mouse position inside of the canvas with scale
    local tmpX = ((self.mousePos.X - self.scaledViewport.X))
    local tmpY = ((self.mousePos.Y - self.scaledViewport.Y))

    -- Change the cursor to the arrow if we are out of the image bounds but still inside of the viewport
    if(pixelVisionOS:IsModalActive() ~= true and (tmpX < 0 or tmpY < 0 or tmpX > (self.scaledViewport.Width - 1) or tmpY > self.scaledViewport.Height - 1)) then
      self.currentCursorID = 1
    end
    -- print(editorUI.mouseCursor.pos, self.mousePos.X, self.mousePos.Y, self.viewportRect, self.scaledViewport)

    -- Force a focus change to up date the cursor
    editorUI:SetFocus(self.canvasPanel, self.currentCursorID)

    if(self.canvasPanel.inFocus == true) then

      -- Check to see if the button is pressed and has an onAction callback
      if(editorUI.collisionManager.mouseReleased == true) then

          -- Click the button
          self.onClick()
          self.firstPress = true
  
      elseif(editorUI.collisionManager.mouseDown) then
  
        self.mouseState = "dragging"

        if(self.firstPress ~= false) then

          -- Save start position
          self.startPos = NewPoint(self.mousePos.X, self.mousePos.Y)
  
          self.triggerOnFirstPress()
          -- end
  
          self.mouseState = "pressed"
  
          -- Change the flag so we don't trigger first press again
          self.firstPress = false

        end

        self:DrawOnCanvas(self.mousePos)
  
      end

      self.mCol = math.floor((editorUI.mouseCursor.pos.X - self.viewportRect.X + self.scaledViewport.X)/self.gridSize)
      self.mRow = math.floor((editorUI.mouseCursor.pos.Y - self.viewportRect.Y + self.scaledViewport.Y)/self.gridSize)
    
    end
      
  else
      
    if(self.canvasPanel.inFocus == true) then
        editorUI:ClearFocus(self.canvasPanel)
    end

  end

  -- Trigger click if released out of bounds to save drawing
  if(editorUI.collisionManager.mouseReleased == true and self.mouseState == "dragging") then
      -- TODO not wired up to anything right now
      -- Click the button
      self.onClick(self)
      self.firstPress = true
      
  end
  
  -- end

end

function PaintTool:OnHorizontalScroll(value)

  self.scaledViewport.X = math.abs(math.floor(((self.scaledViewport.Width - self.boundaryRect.Width) - self.scaledViewport.Width) * value))

  self:InvalidateCanvas()
end

function PaintTool:OnVerticalScroll(value)

  self.scaledViewport.Y = math.abs(math.floor(((self.scaledViewport.Height - self.boundaryRect.Height) - self.scaledViewport.Height) * value))

  self:InvalidateCanvas()

end

function PaintTool:SnapToGrid(value, scale)

  scale = scale or 8
  
  return self.snapToGrid == true and math.floor(value / scale) * scale or value
end


function PaintTool:DrawCanvasPanel()

  --  Render a preview of the brush if the mouse is inside of the canvas
  if(self.canvasPanel.inFocus == true and self.showBrushPreview) then

    -- Adjust mouse position inside of the canvas with scale
    local tmpX = ((self.mousePos.X - self.scaledViewport.X) * self.scale)
    local tmpY = ((self.mousePos.Y - self.scaledViewport.Y) * self.scale)
    
    -- Calculate scroll offset
    local scrollXOffset = (self.scaledViewport.X % 8)
    local scrollYOffset = (self.scaledViewport.Y % 8)
    
    -- We don't need to clip or snap the brush to the grid in color mode
    if(self.pickerMode ~= ColorMode) then

      -- Adjust the tmp X and Y position to account for the scroll offset
      tmpX = tmpX + (scrollXOffset * self.scale)
      tmpY = tmpY + (scrollYOffset * self.scale)
      
      -- Snap tmp X and Y to the grid
      tmpX = self:SnapToGrid(tmpX, self.scale * 8) - (scrollXOffset * self.scale)
      tmpY = self:SnapToGrid(tmpY, self.scale * 8) - (scrollYOffset * self.scale)
      
    end
      -- Calculate horizontal clipping
      if(tmpX < 0) then

        -- If the brush is too far left, shift the X, width, and tmpX position
        self.brushMaskRect.X = scrollXOffset
        self.brushMaskRect.Width = self.brushCanvas.Width - self.brushMaskRect.X
        tmpX = tmpX + (scrollXOffset * self.scale)

      elseif((tmpX/self.scale) + 8 > self.scaledViewport.Width) then
        
        -- If the brush is too far right, reset the X and shift the width
        self.brushMaskRect.X = 0
        self.brushMaskRect.Width = self.brushCanvas.Width - (((tmpX/self.scale) + 8) - self.scaledViewport.Width)

      else

        -- Reset the X and Width
        self.brushMaskRect.X = 0
        self.brushMaskRect.Width = self.brushCanvas.Width

      end

      -- Calculate vertical clipping
      if(tmpY < 0) then

        -- If the brush is too far up, shift the Y, height, and tmpY position
        self.brushMaskRect.Y = scrollYOffset
        self.brushMaskRect.Height = self.brushCanvas.Height - self.brushMaskRect.Y
        tmpY = tmpY + (scrollYOffset * self.scale)

      elseif((tmpY/self.scale) + 8 > self.scaledViewport.Height) then
        
        -- If the brush is too far down, reset the Y and shift the height
        self.brushMaskRect.Y = 0
        self.brushMaskRect.Height = self.brushCanvas.Height - (((tmpY/self.scale) + 8) - self.scaledViewport.Height)

      else

        -- Reset the Y and height
        self.brushMaskRect.Y = 0
        self.brushMaskRect.Height = self.brushCanvas.Height

      end

      -- Clamp brush mask so we don't get an error if the entire brush is out of bounds
      self.brushMaskRect.Width = Clamp( self.brushMaskRect.Width, 0, self.brushCanvas.Width )
      self.brushMaskRect.Height = Clamp( self.brushMaskRect.Height, 0, self.brushCanvas.Height )

    -- end

    -- Draw the brush
    self.brushCanvas:DrawPixels(
      -- Shift the tmpX and tmpY position over by the viewportRect's poisiton
      tmpX + self.viewportRect.X,
      tmpY + self.viewportRect.Y,
      DrawMode.UI, 
      self.scale,
      -1,
      -1,
      self.brushColorOffset,
      self.brushMaskRect
    )

  end


  if(self.displayInvalid == true or self.selectRect ~= nil) then
    
    -- Check if we need to fill the area below the selection
    if(self.fillRect == true) then
        
      for i = 1, #self.selectedPixelData do
        self.selectedPixelData[i] = self.fillRectColor
      end
      -- We fill the are first because there is no need to clear
      -- self.imageLayerCanvas:Clear(self.fillRectColor, self.selectRect.X, self.selectRect.Y, self.selectRect.Width, self.selectRect.Height)
    
      self.fillRect = false

      -- Clear the selected pixels since we just filled the area
      -- self.selectedPixelData = nil

      -- self:CancelCanvasSelection()

    end

    if(self.selectRect ~= nil) then

      -- Check to see if the state has been set to canceled
      if(self.selectionState == "canceled") then
      
        
        
        if(self.selectedPixelData ~= nil) then
          
          -- Check to see if the selection is ignoring transparency
          if(self.selectionUsesMaskColor == true) then
      
            self.imageLayerCanvas:Clear(-1, self.selectRect.X, self.selectRect.Y, self.selectRect.Width, self.selectRect.Height)
      
          end
          
          -- Draw pixel data to the image
          self.imageLayerCanvas:MergePixels(self.selectRect.X, self.selectRect.Y, self.selectRect.Width, self.selectRect.Height, self.selectedPixelData)
        
          -- Clear the pixel data
          self.selectedPixelData = nil
        
        end
        
      
        -- Clear the selection state
        self.selectionState = "none"
      
        -- Clear the selection rect
        self.selectRect = nil
      
        -- Reset the mask flag
        self.selectionUsesMaskColor = false
      
        -- Force the display to clear the tmp layer canvas
        self.tmpLayerCanvas:Clear()
        -- print("Clear Tmp Layer - Cancel selection")
  

        -- Invalidate the canvas and the selection
        self:InvalidateUndo()

      else

      
        -- Check for the clear flag
        if(self.clearArea == true) then

          -- If the area wasn't being filled, clear it with the mask color
          self.imageLayerCanvas:Clear(-1, self.selectRect.X, self.selectRect.Y, self.selectRect.Width, self.selectRect.Height)

          -- TODO should we use the mask color here?
          
          self.clearArea = false

        end

        -- Increment selection timer
        self.selectionTime = self.selectionTime + editorUI.timeDelta

        -- print("self.selectionTime", self.selectionTime)
        if(self.selectionTime > self.selectionDelay) then
          self.selectionShift = self.selectionShift == 0 and 1 or 0
          self.selectionTime = 0
          -- print("Change Shift")
        end

        -- Clear the tmp layer
        self.tmpLayerCanvas:Clear()
        -- print("Clear Tmp Layer - Redraw selection")
        if(self.selectedPixelData ~= nil) then

          -- Check to see if the selection is ignoring transparency
          if(self.selectionUsesMaskColor == true) then

            self.tmpLayerCanvas:Clear(self.maskColor, self.selectRect.X, self.selectRect.Y, self.selectRect.Width, self.selectRect.Height)
            -- print("Clear Tmp Layer - Redraw Selection mask")
          end
          
          self.tmpLayerCanvas:MergePixels(self.selectRect.X, self.selectRect.Y, self.selectRect.Width, self.selectRect.Height, self.selectedPixelData)

        end
        
        self:DrawSelectionRectangle(self.tmpLayerCanvas, self.selectRect, self.selectionShift)

      end
      -- self:InvalidateCanvas()

    end
    
    if(self.clearBG) then
        self:ClearBackground()
        self.clearBG = false
    end

    -- Redraw the background of the canvas
    self.backgroundLayerCanvas:DrawPixels(self.viewportRect.X, self.viewportRect.Y, DrawMode.TilemapCache, self.scale, -1, self.maskColor, 0, self.scaledViewport)
    

    -- print("self.mouseState", self.mouseState)
    
    if(self.mergerTmpLayer == true and self.mouseState == "released") then

      -- print("Start Undo")
      --   -- TODO we can optimize this by passing in a rect for the area to merge
      self.imageLayerCanvas:MergeCanvas(self.tmpLayerCanvas, 0, true)

      -- print("End undo")
      -- self.tmpLayerCanvas:Clear()
      self.mergerTmpLayer = false

      -- -- Invalidate the canvas and the selection
      -- self:InvalidateUndo()
      
    end

    -- Draw the pixel data in the upper left hand corner of the tool's window
    self.imageLayerCanvas:DrawPixels(self.viewportRect.X, self.viewportRect.Y, DrawMode.TilemapCache, self.scale, self.maskColor, -1, self.colorOffset, self.scaledViewport)

    -- Only draw the flag layer when we need to
    if(self.pickerMode == FlagMode) then
      self.flagLayerCanvas:DrawPixels(self.viewportRect.X, self.viewportRect.Y, DrawMode.TilemapCache, self.scale, -1, self.emptyColorID, 0, self.scaledViewport)
    end

    -- Only draw the temp layer when we need to
    if(self.drawTmpLayer == true) then
      self.tmpLayerCanvas:DrawPixels(self.viewportRect.X, self.viewportRect.Y, DrawMode.TilemapCache, self.scale, -1, self.emptyColorID, self.colorOffset, self.scaledViewport)
    end
    
    self.displayInvalid = false

  end

  -- Reset the mouse state at the end of the draw cycle if it was released on this frame
  if(self.mouseState == "released") then
    self.mouseState = "up"
  end

end

function PaintTool:ClearTmpLayer()

  -- TODO need to rout all draw calls through APIs to make sure they are correctly invalidating and not being called multiple times in a single frame

end


function PaintTool:OnNextZoom(reverse)

  -- Loop backwards through the button sizes
  if(Key(Keys.LeftShift) or reverse == true) then
      self.scaleMode = self.scaleMode - 1

      if(self.scaleMode < 1) then
          self.scaleMode = 1
      end

      -- TODO disable zoom out menu

  else
      self.scaleMode = self.scaleMode + 1

      if(self.scaleMode > #self.scaleValues) then
          self.scaleMode = #self.scaleValues
      end

      -- TODO disable zoom in menu

  end

  self:ChangeScale( self.scaleValues[self.scaleMode])

end

function PaintTool:ChangeScale(value)
 
  self.scale = value--Clamp(value, 1, 8)

  local imageWidth = math.floor(self.imageLayerCanvas.width * self.scale)
  local imageHeight = math.floor(self.imageLayerCanvas.height * self.scale)

  local viewWidth = math.floor(self.viewportRect.Width / self.scale)
  local viewHeight = math.floor(self.viewportRect.Height / self.scale)

  self.scaledViewport.Width = Clamp(viewWidth, 1, math.max(imageWidth, self.imageLayerCanvas.width)) --math.min(self.viewportRect.Width, math.min(self.tmpLayerCanvas.width * self.scale, math.ceil(self.viewportRect.Width / self.scale)))
  self.scaledViewport.Height = Clamp(viewHeight, 1, math.max(imageHeight, self.imageLayerCanvas.height))--, self.viewportRect.Height) --math.min(self.viewportRect.Height, math.min(self.tmpLayerCanvas.height * self.scale, math.ceil(self.viewportRect.Height / self.scale)))


  -- print("self.scaledViewport", dump(self.scaledViewport))
  -- Calculate the boundary for scrolling
  self.boundaryRect.Width = self.imageLayerCanvas.width - self.scaledViewport.Width
  self.boundaryRect.Height = self.imageLayerCanvas.height - self.scaledViewport.Height

  editorUI:Enable(self.hSliderData, self.boundaryRect.Width > 0)

  editorUI:Enable(self.vSliderData, self.boundaryRect.Height > 0)

  -- TODO enable and disable menu


  self.clearBG = false

  if(self.hSliderData.enabled == true) then
      local oldValue = self.hSliderData.value
      self.hSliderData.value = -1
      editorUI:ChangeSlider(self.hSliderData, oldValue, true)
  else
      editorUI:ChangeSlider(self.hSliderData, 0, true)
      self.clearBG = true
  end

  if(self.vSliderData.enabled == true) then
      local oldValue = self.vSliderData.value
      self.vSliderData.value = -1
      editorUI:ChangeSlider(self.vSliderData, oldValue, true)
  else
      editorUI:ChangeSlider(self.vSliderData, 0, true)
      self.clearBG = true
  end

  self.gridSize = 8 * self.scale


  self.overCanvas.Resize(self.gridSize + 6, self.gridSize + 6)
  self.overCanvas.Clear(15)
  self.overCanvas.DrawRectangle(0, 0, self.overCanvas.Width, self.overCanvas.Height);
  self.overCanvas.DrawRectangle(2, 2, self.overCanvas.Width-4, self.overCanvas.Height-4);

  self:InvalidateCanvas()

  editorUI:RebuildMetaSpriteCache(self.sizeBtnData, "scalemode"..(self.scaleMode - 1))

  pixelVisionOS:DisplayMessage("Image scale " .. (self.scale * 100) .. "%")

end

function PaintTool:ClearBackground()

  -- TODO these values should be cached

  local cols = self.viewportRect.Width / 8
  
  local total = cols * (self.viewportRect.Height / 8)
  
  local spriteId = MetaSprite( "emptycolor" ).Sprites[1].Id

  for i = 1, total do
      
      local pos = CalculatePosition( i-1, cols )

      DrawSprite(spriteId, self.viewportRect.X + (pos.X * 8), self.viewportRect.Y + (pos.Y * 8), false, false, DrawMode.TilemapCache)
      
  end

end

function PaintTool:ChangeCanvasTool(toolName, cursorID)

  print("Change Tool", toolName)

  -- Clear the selection when changing tools/
  if(self.selectRect ~= nil) then
    self:CancelCanvasSelection()
  end
  

  if(toolName == "circlefill") then

      toolName = "circle"
      self.fill = true

  elseif(toolName == "rectanglefill") then

      toolName = "rectangle"
      self.fill = true

  else
      self.fill = false
  end

  self.showBrushPreview = (toolName == "pen" or toolName == "eraser")

  self.tool = toolName

  self.drawTmpLayer = self.tool == "select" or self.toolButtons[3].selected == true

  -- TODO need to add in support for hand, pointer, etc

  -- TODO this should use the tool button data for the pointer
  if(self.tool == "pointer") then
  
    self.currentCursorID = cursorID or 1

  elseif(self.tool == "hand") then

    self.currentCursorID = cursorID or 9
  
  elseif(self.tool == "pen") then

    if(self.pickerMode == ColorMode and self.brushColor < 0) then
      self:OnPickerSelection(1)
    end

    self.currentCursorID = cursorID or 6
    
  elseif(self.tool == "eraser") then

    if(self.pickerMode == ColorMode and self.brushColor > -1) then
      self:OnPickerSelection(0)
    end

    self.currentCursorID = cursorID or 7

  else
    
    self.currentCursorID = 8
    
  end

  -- Save the new cursor for tools that need to restore
  self.defaultCursorID = self.currentCursorID

  self:InvalidateBrushPreview()

end

function PaintTool:CancelCanvasSelection()

  -- Clear the selection state
  self.selectionState = "canceled"

  self:InvalidateUndo()
  
  -- Invalidate the display so it redraws on the next frame
  self:InvalidateCanvas()

  -- Disable all of the selection related menu options
  for i = 1, #self.selectionOptions do
    pixelVisionOS:EnableMenuItemByName(self.selectionOptions[i], false)
  end

end

function PaintTool:SelectAll()
  print("Select All")

  -- Look for any selected pixel data so we don't accidentally clear it with the new selection
  if(self.selectRect ~= nil and self.selectedPixelData ~= nil) then
    
    -- Draw pixel data to the image
    self.imageLayerCanvas:MergePixels(self.selectRect.X, self.selectRect.Y, self.selectRect.Width, self.selectRect.Height, self.selectedPixelData)
  
  end

  self:OnSelectTool("select")
  
  self:InvalidateUndo()
  
  self.selectionState = "resize"

  self.selectRect = NewRect( 0, 0, self.imageLayerCanvas.Width, self.imageLayerCanvas.Height )
  
  self.selectedPixelData = nil
  
  self.onClick()

end

-- Use this to perform a click action on a button. It's used internally when a mouse click is detected.
function PaintTool:CanvasRelease(callAction)

  print("Canvas Release")

  -- Check to see if a selection was just resized
  if(self.selectRect ~= nil and self.selectionState == "resize") then

    self:ClampSelectionToBounds()

    -- Test to see if the selection is valid
    if(self.selectRect.Width == 0 or self.selectRect.Height == 0) then

      -- Clear any pixel data
      self.selectedPixelData = nil

      -- Clear the selection values
      self:CancelCanvasSelection()

    else

      -- Make sure we don't already have selected pixel data
      if(self.selectedPixelData == nil) then

        -- Cut the pixels
        self.selectedPixelData = self:CutPixels()

      end

    end

    -- Clear all of the selection related menu options
    for i = 1, #self.selectionOptions do
      pixelVisionOS:EnableMenuItemByName(self.selectionOptions[i], true)
    end

  end

  -- Reset the selection state
  self.selectionState = "none"

  -- Clear the start position
  self.startPos = nil
  self.lastPenX = nil
  self.lastPenY = nil

  -- Clear the mouse state
  self.mouseState = self.mouseState == "released" and "up" or "released"

  print("Release", self.mouseState)
  -- if(self.mergerTmpLayer == true ) then

  -- --   -- TODO we can optimize this by passing in a rect for the area to merge
  --   self.imageLayerCanvas:MergeCanvas(self.tmpLayerCanvas, 0, true)

  --   self.mergerTmpLayer = false
    
  -- end

  -- Clear the canvas
  -- self.tmpLayerCanvas:Clear()

  self:InvalidateCanvas()

  self:InvalidateSprites()
  
end

function PaintTool:ClampSelectionToBounds()

  -- Clamp X and Y values
  self.selectRect.X = math.max(self.selectRect.X, 0)
  self.selectRect.Y = math.max(self.selectRect.Y, 0)

  -- Clamp the Width
  if(self.selectRect.X + self.selectRect.Width > self.imageLayerCanvas.Width) then
    self.selectRect.Width = self.selectRect.Width - ((self.selectRect.X + self.selectRect.Width) - self.imageLayerCanvas.Width)
  end

  -- Clamp the Height
  if(self.selectRect.Y + self.selectRect.Height > self.imageLayerCanvas.Height) then
    self.selectRect.Height = self.selectRect.Height - ((self.selectRect.Y + self.selectRect.Height) - self.imageLayerCanvas.Height)
  end

end

function PaintTool:ResetCanvasStroke()

  -- print("ResetCanvasStroke")

  if(self.tool == "pen") then
    
    self.imageLayerCanvas:SetStroke(self.brushColor, self.defaultStrokeWidth)

  elseif(self.tool == "eraser") then

    self.imageLayerCanvas:SetStroke(self.emptyColorID, self.defaultStrokeWidth)

  else

    -- Change the stroke to a single pixel
    self.tmpLayerCanvas:SetStroke(self.brushColor, self.defaultStrokeWidth)--realBrushColor, )
    self.tmpLayerCanvas:SetPattern({self.brushColor}, 1, 1)

  end

end

function PaintTool:DrawOnCanvas(mousePos)

  -- TODO some tools should snap to grid when holding down shift

  if(self.pickerMode ~= ColorMode) then
    
  --   print("Snap to grid")
  --   -- TODO need to snap values to the grid
    self.startPos.X = self:SnapToGrid(self.startPos.X)
    self.startPos.Y = self:SnapToGrid(self.startPos.Y)

    mousePos.X = self:SnapToGrid(mousePos.X)
    mousePos.Y = self:SnapToGrid(mousePos.Y)

  end

  

  -- print("self.startPos", self.startPos, "mousePos", mousePos)

  -- Get the start position for a new drawing
  if(self.startPos ~= nil) then

      -- Test for the data.tool and perform a draw action
      if(self.tool == "pen" or self.tool == "eraser") then

        local targetCanvas = self.pickerMode == FlagMode and self.flagLayerCanvas or self.imageLayerCanvas

        if(self.lastPenX ~= self.startPos.X or self.lastPenY ~= self.startPos.Y) then

          if(self.pickerMode == ColorMode) then

            targetCanvas:DrawLine(self.startPos.x, self.startPos.y, mousePos.x, mousePos.y)

          else
     
            targetCanvas:MergePixels(self.startPos.x, self.startPos.y, self.brushCanvas.Width, self.brushCanvas.Height, self.brushCanvas:GetPixels(), false, false, 0, false)

          end

          self.lastPenX = self.startPos.X
          self.lastPenY = self.startPos.Y

          self:InvalidateCanvas()

        end

        self.startPos = NewPoint(mousePos.x, mousePos.y)

        -- Don't merge tmp layer since we are not using it
        self.mergerTmpLayer = false

        -- Invalidate the canvas and the selection
        self:InvalidateUndo()

      elseif(self.tool == "line") then

        -- Clear tmp layer before drawing the line on this frame
        self.tmpLayerCanvas:Clear()
        
        -- print("Clear Tmp Layer - Line")
        
        -- Draw the line on the tmp canvas
        self.tmpLayerCanvas:DrawLine(self.startPos.x, self.startPos.y, mousePos.x, mousePos.y, self.fill)

        -- Need to merge tmp layer after drawing
        self.mergerTmpLayer = true

        -- Invalidate the canvas
        self:InvalidateCanvas()

        -- Invalidate the canvas and the selection
        self:InvalidateUndo()

      elseif(self.tool == "rectangle") then

        -- Invalidate the canvas and the selection
        self:InvalidateUndo()

        self.tmpLayerCanvas:Clear()
        -- print("Clear Tmp Layer - Rectangle")
        -- self:ResetCanvasStroke()

        -- TODO this is fixed in the canvas
        self.tmpLayerCanvas:DrawRectangle(
            math.min(self.startPos.x, mousePos.x), 
            math.min(self.startPos.y, mousePos.y),
            math.abs(mousePos.x - self.startPos.x)+ 1,
            math.abs(mousePos.y - self.startPos.y) + 1, 
            self.fill
        )

        -- Need to merge tmp layer after drawing
        self.mergerTmpLayer = true

        self:InvalidateCanvas()

      elseif(self.tool == "circle") then

        -- Invalidate the canvas and the selection
        self:InvalidateUndo()

        self.tmpLayerCanvas:Clear()

        -- self:ResetCanvasStroke()

        -- TODO this is fixed in the canvas
        self.tmpLayerCanvas:DrawEllipse(
            math.min(self.startPos.x, mousePos.x),
            math.min(self.startPos.y, mousePos.y),
            math.abs(mousePos.x - self.startPos.x)+ 1,
            math.abs(mousePos.y - self.startPos.y) + 1,
            self.fill
        )

        -- Need to merge tmp layer after drawing
        self.mergerTmpLayer = true

        self:InvalidateCanvas()

      elseif(self.tool == "select") then

        if(self.mouseState == "pressed") then

          if(self.selectRect == nil) then

            self.selectionState = "new"

            self.selectRect = NewRect(self.startPos.x, self.startPos.y, 0, 0)

          else
            
            if(self.selectRect:Contains(mousePos) == true) then

                self.selectionState = "newmove"

                self.moveOffset = NewPoint(self.selectRect.X - mousePos.X, self.selectRect.Y - mousePos.Y)

            else

              -- Cancel the selection
              self:CancelCanvasSelection(self)
                
            end

          end

        elseif(self.mouseState == "dragging")  then

          if(self.selectRect ~= nil) then

            self:InvalidateUndo()
            
            if(self.selectionState == "new" or self.selectionState == "resize") then

              self.selectionState = "resize"

              -- print("resize", data.selectRect, mousePos, , )

              self.selectRect.X = math.min(self.startPos.X, mousePos.X)
              self.selectRect.Y = math.min(self.startPos.Y, mousePos.Y)
              self.selectRect.Width = Clamp(math.abs(mousePos.X - self.startPos.X), 0, self.imageLayerCanvas.width)
              self.selectRect.Height = Clamp(math.abs(mousePos.Y - self.startPos.Y), 0, self.imageLayerCanvas.height)

            else

              editorUI.cursorID = 2

              self.selectRect.X = mousePos.X + self.moveOffset.X --  data.selectionSize.X
              self.selectRect.Y = mousePos.Y + self.moveOffset.Y --  data.selectionSize.Y

              self.selectionState = "move"

            end
          end
          

        end

        -- No need to merge tmp layer since the selection tool will do this manually
        self.mergerTmpLayer = false

      -- print("Selection", dump(self.selectRect))


      elseif(self.tool == "eyedropper") then
        
        -- Need to merge tmp layer after drawing
        self.mergerTmpLayer = false

        -- Check to see if we are in color mode
        if(self.pickerMode == ColorMode) then

          -- Save reference to the previous color
          local oldColor = self.overColor
          
          -- Save the new color under the mouse 
          self.overColor = self.imageLayerCanvas:ReadPixelAt(mousePos.x, mousePos.y)


          if(oldColor ~= self.overColor) then

            -- Select the color and offset by 1 to account for the mask color in the first position
            self:OnPickerSelection(self.overColor + 1)

          end
          -- print("self.overColor", self.overColor)
      -- if(self.overColor > pixelVisionOS.colorsPerSprite) then
      --     self.overColor = -1
        end

      -- TODO should we display this value or that it is out of bounds?

      end

  end

end

function PaintTool:CutPixels(clearArea)

  if(self.selectRect == nil) then
    return
  end

  -- Check if the shift key is down
  if(Key(Keys.LeftShift) or Key(Keys.RightShift)) then
   
    -- Set the flag to use the mask color when copying the selection pixel data
    self.selectionUsesMaskColor = true

  end

  -- Set the flag to clear the area below the cut pixels
  self.clearArea = clearArea or true

  -- Return the pixel data
  return self.imageLayerCanvas:GetPixels(self.selectRect.X, self.selectRect.Y, self.selectRect.Width, self.selectRect.Height)

end

function PaintTool:FillCanvasSelection(colorID)

  -- pixelVisionOS:BeginUndo(self)
  
  if(self.selectRect == nil) then
    return
  end

  self.fillRect = true
  self.fillRectColor = colorID or self.brushColor

  -- self:InvalidateCanvas()

end

function PaintTool:InvalidateBackground()

  -- Mode 1 will render the transparent canvas texture for the background
  if(self.backgroundMode == 1) then

    self.backgroundLayerCanvas:SetStroke(-1, 0)
    self.backgroundLayerCanvas:SetPattern(Sprite(MetaSprite("emptymaskcolor").Sprites[1].Id), 8, 8)
    self.backgroundLayerCanvas:DrawRectangle(0, 0, self.backgroundLayerCanvas.Width, self.backgroundLayerCanvas.Height, true)

  -- Mode 2 will render the background color
  elseif(self.backgroundMode == 2) then

    self.backgroundLayerCanvas:Clear(self.backgroundColorId + self.colorOffset)

  else

    -- Use the mask color as the default background    
    self.backgroundLayerCanvas:Clear(-1)

  end

  -- Invalidate the canvas to force it to redraw
  self:InvalidateCanvas()

end