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

function EditorUI:CreateCanvas(rect, size, scale, colorOffset, toolTip, emptyColorID, forceDraw)

  -- Create the button's default data
  local data = self:CreateData(rect, nil, toolTip, forceDraw)

  -- Customize the default name by adding Button to it
  data.name = "Canvas" .. data.name

  data.showPixelSelection = true
  data.pixelSelectionSize = 1
  data.borderOffset = 2
  data.gridSize = 8
  data.showBGColor = false
  data.selectionCounter = 0
  data.selectionDelay = .2
  data.selectionTime = 0
  data.selectRect = nil

  -- Create a selection canvas
  data.selectionCanvas = NewCanvas(data.rect.w, data.rect.h)
  data.selectionCanvas:SetStroke({0}, 1, 1)

  local spriteData = _G["pixelselection1x"]

  data.overDrawArgs = {spriteData.spriteIDs, 0, 0, spriteData.width, false, false, DrawMode.Sprite, 36, true, false}


  data.onClick = function(tmpData)


    self:CanvasRelease(tmpData, true)

  end

  data.triggerOnFirstPress = function(tmpData)

    self:CanvasPress(tmpData, true)

    -- Trigger fill here since it only happens on the fist press
    if(data.tool == "fill") then

      -- TODO need to set this one on a timer

      -- Update the fill color
      data.paintCanvas:SetPattern({data.brushColor + data.colorOffset}, 1, 1)

      data.paintCanvas:FloodFill(data.startPos.x, data.startPos.y)

      self:Invalidate(data)
    end

    if(data.onFirstPress ~= nil) then
      data.onFirstPress()
    end

  end

  data.penCanErase = false
  data.colorOffset = colorOffset
  data.emptyColorID = emptyColorID or - 1
  data.fill = false
  data.currentCursorID = 1
  -- Default color is 0
  data.brushColor = 0
  data.overColor = -1

  self:ResizeCanvas(data, size, scale)


  -- Clear the background so the mask color shows through
  -- DrawRect(rect.x, rect.y, rect.w, rect.h, emptyColorID, DrawMode.TilemapCache)

  -- data.currentTool = 1 -- default tool

  self:ResetValidation(data)

  return data

end

function EditorUI:ChangeCanvasTool(data, toolName, cursorID)

  -- print("Change canvas tool", toolName)

  data.tool = toolName

  -- TODO change the cursor
  if(data.tool == "pen") then

    data.currentCursorID = cursorID or 6
    self:ResetCanvasStroke(data)

  elseif(data.tool == "eraser") then

    data.currentCursorID = cursorID or 7

    ReplaceColor(54, data.emptyColorID)

  else
    data.currentCursorID = 8
    self:ResetCanvasStroke(data)


  end

  -- data.showPixelSelection = data.tool ~= "eyedropper" and data.tool ~= "fill"


  -- TODO change the drawing tool

end

function EditorUI:SetCanvasPixels(data, pixelData)
  data.paintCanvas:SetPixels(pixelData)
end

function EditorUI:UpdateCanvas(data, hitRect)

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
    self:RedrawCanvas(data)

    -- Shouldn't update the button if its disabled
    return

  end

  -- Make sure we don't detect a collision if the mouse is down but not over this button
  if(self.collisionManager.mouseDown and data.inFocus == false) then
    -- See if the button needs to be redrawn.
    self:RedrawCanvas(data)
    return
  end

  -- If the hit rect hasn't been overridden, then use the buttons own hit rect
  if(hitRect == nil) then
    hitRect = data.hitRect or data.rect
  end

  local overrideFocus = (data.inFocus == true and self.collisionManager.mouseDown)

  local inRect = self.collisionManager:MouseInRect(hitRect)

  -- Ready to test finer collision if needed
  if(inRect == true or overrideFocus) then

    -- Draw a selection rect on top of everything
    -- if(data.selectRect ~= nil) then

    --   data.selectionTime = data.selectionTime + self.timeDelta

    --   if(data.selectionTime > data.selectionDelay) then
    --     data.selectionCounter = data.selectionCounter + 1
    --     if(data.selectionCounter > 1) then
    --       data.selectionCounter = 0
    --     end

    --     data.selectionTime = 0
        

    --   -- data.selectionCanvas:Clear()

    --   -- -- local lastCenteredValue = data.selectionCanvas:DrawCentered()

    --   -- -- data.selectionCanvas:DrawCentered(false)

    --   -- -- Change the stroke to a single pixel
      

    --   -- -- data.selectionCanvas:LinePattern(2, data.selectionCounter)

    --   -- data.selectionCanvas:DrawSquare(data.selectRect.x, data.selectRect.y, data.selectRect.width, data.selectRect.height, false)

    --   -- -- Loop through pixels and invert if needed

    --   -- -- Draw the canvas to the display
    --   -- data.selectionCanvas:DrawPixels(data.rect.x, data.rect.y, DrawMode.UI, 1, -1, data.emptyColorID)

      
    --   -- data.selectionCanvas:LinePattern(1, 0)

    --   -- data.selectionCanvas:DrawCentered(lastCenteredValue)

    -- end
    
    
    -- end

    -- If we are in the collision area, set the focus
    self:SetFocus(data, data.currentCursorID)

    local tmpPos = NewPoint(
      Clamp(self.collisionManager.mousePos.x - data.rect.x, 0, data.rect.w - 1),
      Clamp(self.collisionManager.mousePos.y - data.rect.y, 0, data.rect.h - 1)
    )

    -- Modify scale
    tmpPos.x = tmpPos.x / data.scale
    tmpPos.y = tmpPos.y / data.scale

    if(data.showPixelSelection) then

      local position = 
      {
        x = math.floor((self.collisionManager.mousePos.x - data.rect.x) / data.gridSize),
        y = math.floor((self.collisionManager.mousePos.y - data.rect.y) / data.gridSize),
      }

      data.toolTip = "Over pixel (" .. string.lpad(tostring(position.x + 1), 2, "0") .. "," .. string.lpad(tostring(position.y + 1), 2, "0") ..")"

      if(data.tool == "eyedropper") then

        local tmpColor = data.paintCanvas:ReadPixelAt(tmpPos.x, tmpPos.y) - data.colorOffset

        -- TODO this is duplicated below

        -- Update over color if using eye picker
        if(pixelVisionOS.paletteMode == true) then

          -- TODO this is hard coded to look for a palette color picker

          -- Shift the value to offset for the palette
          tmpColor = tmpColor + PaletteOffset(paletteColorPickerData.pages.currentSelection - 1)

        end

        ReplaceColor(54, tmpColor + 256)

      end


      -- Only update the over rect if the mouse is over the canvas
      if(inRect == true and data.tool ~= "select") then

        -- Update over rect position
        data.overDrawArgs[2] = (position.x * data.gridSize) + data.rect.x - data.borderOffset
        data.overDrawArgs[3] = (position.y * data.gridSize) + data.rect.y - data.borderOffset

        -- Draw over rect
        self:NewDraw("DrawSprites", data.overDrawArgs)
      end

    end

    -- Check to see if the button is pressed and has an onAction callback
    if(self.collisionManager.mouseReleased == true) then

      -- Click the button
      data.onClick(data)
      data.firstPress = true

    elseif(self.collisionManager.mouseDown) then

      data.mouseState = "dragging"


      if(data.firstPress ~= false) then

        -- Save start position
        data.startPos = NewPoint(tmpPos.x, tmpPos.y)

        -- if(data.triggerOnFirstPress ~= nil) then
        -- Call the onPress method for the button
        data.triggerOnFirstPress(data)
        -- end

        data.mouseState = "pressed"

        -- Change the flag so we don't trigger first press again
        data.firstPress = false



      end

      self:DrawOnCanvas(data, tmpPos)

    end

  else

    if(data.inFocus == true) then
      data.firstPress = true
      -- If we are not in the button's rect, clear the focus
      self:ClearFocus(data)

    end

  end

  -- Capture keys to switch between different tools and options
  if( Key(Keys.Back, InputState.Released) ) then

    if(self.selectRect ~= nil) then

      -- Remove the pixel data from the temp canvas's selection
      self.tmpPaintCanvas:Clear()

      -- Change the stroke to a single pixel of white
      self.tmpPaintCanvas:SetStroke({1}, 1, 1)

      -- Change the stroke to a single pixel of white
      self.tmpPaintCanvas:SetPattern({1}, 1, 1)

      -- Draw a square to mask off the selected area on the main canvas
      self.tmpPaintCanvas:DrawSquare(self.selectRect.x, self.selectRect.y, self.selectRect.width, self.selectRect.height, true)

      -- Clear the selection
      self.selectRect = nil

      -- Merge the pixel data from the tmp canvas into the main canvas before it renders
      self.paintCanvas:Merge(self.tmpPaintCanvas, 0, true)

    end

  end



  -- Make sure we don't need to redraw the button.
  self:RedrawCanvas(data)

end

function EditorUI:DrawOnCanvas(data, mousePos, toolID)

  -- Get the start position for a new drawing
  if(data.startPos ~= nil) then

    -- Test for the data.tool and perform a draw action
    if(data.tool == "pen") then

      if(data.penCanErase == true) then

        local overColorID = data.paintCanvas:ReadPixelAt(mousePos.x, mousePos.y) - data.colorOffset

        if(overColorID > 0) then
          data.tmpPaintCanvas:SetStroke({data.emptyColorID}, 1, 1)
        else
          self:ResetCanvasStroke(data)
        end

        data.tmpPaintCanvas:SetStrokePixel(mousePos.x, mousePos.y)

      else

        self:ResetCanvasStroke(data)

        data.tmpPaintCanvas:DrawLine(data.startPos.x, data.startPos.y, mousePos.x, mousePos.y)

      end


      data.startPos = NewPoint(mousePos.x, mousePos.y)

      self:Invalidate(data)

    elseif(data.tool == "eraser") then

      -- Change the stroke the empty color
      data.tmpPaintCanvas:SetStroke({data.emptyColorID}, 1, 1)

      data.tmpPaintCanvas:DrawLine(data.startPos.x, data.startPos.y, mousePos.x, mousePos.y)
      data.startPos = NewPoint(mousePos.x, mousePos.y)

      self:Invalidate(data)

    elseif(data.tool == "line") then

      data.tmpPaintCanvas:Clear()

      self:ResetCanvasStroke(data)

      data.tmpPaintCanvas:DrawLine(data.startPos.x, data.startPos.y, mousePos.x, mousePos.y, data.fill)

      -- force the paint canvas to redraw
      data.paintCanvas:Invalidate()

      self:Invalidate(data)

    elseif(data.tool == "box") then


      data.tmpPaintCanvas:Clear()

      self:ResetCanvasStroke(data)

      data.tmpPaintCanvas:DrawSquare(data.startPos.x, data.startPos.y, mousePos.x, mousePos.y, data.fill)

      -- force the paint canvas to redraw
      data.paintCanvas:Invalidate()

      self:Invalidate(data)

    elseif(data.tool == "select") then

      print("data.mouseState" , data.mouseState)

    -- if(data.selectRect ~= nil and data.firstPress == false) then

    --   if(data.selectRect:Contains(mousePos)) then

    --     print("Move")

    --     -- TODO see if the selection was moved
    --     -- TODO move the selection with the mouse
        
      
    --   else
    --     data.selectRect = nil
    --   end

    -- else

      
    --   print(data.startPos.x, data.startPos.y, mousePos.x, mousePos.y, data.scale)

        
    --   data.selectRect = NewRect(data.startPos.x, data.startPos.y, mousePos.x, mousePos.y)
    
    --   -- clear selection if it's not big enough
    --   if(math.abs(data.selectRect.x - data.selectRect.width) <= 2 or math.abs(data.selectRect.y - data.selectRect.width) <= 2) then
    --     data.selectRect = nil

    -- end

      -- Save start position
      -- if(data.selectRect == nil) then
      
      -- else
      --
      -- end
      --
      -- data.selectRect.width = mousePos.x - data.selectRect.x
      -- data.selectRect.height = mousePos.y - data.selectRect.y

      -- print("Rect", data.selectRect.x, data.selectRect.y, data.selectRect.width, data.selectRect.height)

      

        -- data.tmpPaintCanvas:Clear()

      -- end

    elseif(data.tool == "circle") then

      data.tmpPaintCanvas:Clear()

      self:ResetCanvasStroke(data)

      data.tmpPaintCanvas:DrawEllipse(data.startPos.x, data.startPos.y, mousePos.x, mousePos.y, data.fill)

      -- force the paint canvas to redraw
      data.paintCanvas:Invalidate()

      self:Invalidate(data)

    elseif(data.tool == "eyedropper") then

      data.overColor = data.paintCanvas:ReadPixelAt(mousePos.x, mousePos.y) - data.colorOffset

    elseif(data.tool == "select") then

      -- print("select", data.startPos.x, data.startPos.y)

    end


  end

  -- end

end

function EditorUI:ResetCanvasStroke(data)

  local tmpColor = data.brushColor

  local realBrushColor = tmpColor + data.colorOffset

  -- Change the stroke to a single pixel
  data.tmpPaintCanvas:SetStroke({realBrushColor}, 1, 1)

  -- Check to see if we are in palete mode
  if(pixelVisionOS.paletteMode == true) then

    -- TODO this is hard coded to look for a palette color picker

    -- Shift the value to offset for the palette
    tmpColor = tmpColor + PaletteOffset(paletteColorPickerData.pages.currentSelection - 1)

  end

  ReplaceColor(54, tmpColor + 256)

end

function EditorUI:InvalidateCanvas(data)

  data.paintCanvas:Invalidate()

end

function EditorUI:RedrawCanvas(data)


  if(data == nil) then
    return
  end

  local bgColor = data.showBGColor and gameEditor:BackgroundColor() + 256 or data.emptyColorID

  if(data.paintCanvas.invalid == true) then

    -- Draw the final canvas to the display on each frame
    data.paintCanvas:DrawPixels(data.rect.x, data.rect.y, DrawMode.TilemapCache, data.scale, bgColor, data.emptyColorID)

    data.paintCanvas:ResetValidation()

  end

  if(data.tmpPaintCanvas.invalid == true) then

    -- Draw the tmp layer on top of everything since it has the active drawing's pixel data
    data.tmpPaintCanvas:DrawPixels(data.rect.x, data.rect.y, DrawMode.TilemapCache, data.scale, bgColor, data.emptyColorID)

    -- data.tmpPaintCanvas:ResetValidation()

  end

  if(data.selectionCanvas.invalid == true) then
    
    if(data.selectRect  ~= nil) then

      data.selectionCanvas:Clear()

      -- Loop through pixels and invert if needed

      -- Draw the canvas to the display
      

      data.selectionCanvas:DrawSquare(data.selectRect.x, data.selectRect.y, data.selectRect.width, data.selectRect.height, false)

      data.selectionCanvas:DrawPixels(data.rect.x, data.rect.y, DrawMode.UI, 1, -1, data.emptyColorID)

    end

  end

end

-- Use this to perform a click action on a button. It's used internally when a mouse click is detected.
function EditorUI:CanvasRelease(data, callAction)

  -- print("Canvas Release")


  -- Clear the start position
  data.startPos = nil

  data.mouseState = data.mouseState == "released" and "up" or "released"

  -- Return if the selection rect is nil
  -- if(data.selectRect ~= nil) then
  --   return
  -- end

  if(data.tmpPaintCanvas.invalid == true) then

    -- Merge the pixel data from the tmp canvas into the main canvas before it renders
    data.paintCanvas:MergeCanvas(data.tmpPaintCanvas, 0, true)

    -- Clear the canvas
    data.tmpPaintCanvas:Clear()

    -- Normally clearing the canvas invalidates it but se want to reset it until its drawn in again
    data.tmpPaintCanvas:ResetValidation()

  end

  if(data.selectionCanvas.invalid == true) then

    
    data.selectionCanvas:ResetValidation()

  end

  -- trigger the canvas action callback
  if(data.onAction ~= nil and callAction ~= false) then

    -- Trigger the onAction call back and pass in the double click value if the button is set up to use it
    data.onAction()

  end

end

function EditorUI:CanvasPress(data, callAction)

  -- print("onPress", "Update canvas")

  data.tmpPaintCanvas:Invalidate()

  if(data.onPress ~= nil and callAction ~= false) then

    -- Trigger the onPress
    data.onPress()

  end

end

function EditorUI:ResizeCanvas(data, size, scale, pixelData)

  -- data.canvas = NewCanvas(rect.w, rect.h)

  -- Create a new canvas for drawing into
  data.mergedCanvas = NewCanvas(size.x, size.y)

  -- Create a new canvas for drawing into
  data.paintCanvas = NewCanvas(size.x, size.y)

  -- Create a temporary canvas
  data.tmpPaintCanvas = NewCanvas(size.x, size.y)

  -- Set scale for calculation
  data.scale = scale or 1

  -- TODO need to copy pixel data over to the canvas
  if(pixelData ~= nil) then

    local total = #pixelData

    -- print("Total Pixels", total)
    for i = 1, total do

      local color = pixelData[i]

      pixelData[i] = color == -1 and data.emptyColorID or (color + data.colorOffset)

    end

    data.paintCanvas:SetPixels(0, 0, size.x, size.y, pixelData);

  end

end

function EditorUI:GetCanvasSize(data)

  return NewRect(data.rect.x, data.rect.y, data.paintCanvas.width, data.paintCanvas.height)
end

function EditorUI:ToggleCanvasFill(data, value)

  data.fill = value or not data.fill

  return data.fill

end

function EditorUI:ToggleCanvasCentered(data, value)

  value = value or not data.tmpPaintCanvas:DrawCentered()

  data.tmpPaintCanvas:DrawCentered(value)

  return value

end

function EditorUI:CanvasBrushColor(data, value)

  -- print("Value", value)

  data.brushColor = value

  self:ResetCanvasStroke(data)

end

function EditorUI:GetCanvasPixelData(data)

  -- TODO should this subtract the color offset?

  return data.paintCanvas:GetPixels()

end

function EditorUI:ClearCanvas(data)

  data.paintCanvas:Clear()

end

function EditorUI:ChangeCanvasPixelSize(data, size)

  data.pixelSelectionSize = size
  -- data.borderOffset = 2
  local spriteData = _G["pixelselection" .. tostring(size) .."x"]

  data.overDrawArgs[1] = spriteData.spriteIDs
  data.overDrawArgs[4] = spriteData.width

  -- Not sure why but we need to offset this value
  if(size == 4) then
    data.borderOffset = 3
  else
    data.borderOffset = 2
  end

  -- Calculate the gridSize TODO this is off because we don't support scale 3 so clamping
  data.gridSize = Clamp((3 - data.pixelSelectionSize) * 8, 4, 16)

end
