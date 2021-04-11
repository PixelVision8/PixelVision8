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

    self.tmpPaintCanvas = nil
    self.viewportRect = NewRect(8, 48+8, 224, 160-8)
    self.boundaryRect = NewRect(0,0,0,0)
    self.displayInvalid = true
    self.gridSize = 8
    self.snapToGrid = false -- TODO snapping to grid is very buggy

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
    self.fill = false
    self.currentCursorID = 1

    -- Default color is 0
    self.brushColor = 0
    self.overColor = -1
    
    self.brushColorID = 253
    self.defaultStrokeWidth = 1
    -- Get the image pixels
    local pixelData = self.image.GetPixels()

    -- Create a new canvas
    self.imageCanvas = NewCanvas(math.ceil(self.image.width/8) * 8, math.ceil(self.image.height/8) * 8)
    self.imageCanvas:Clear()
    
     -- Copy the modified image pixel data over to the new canvas
    self.imageCanvas.SetPixels(0, 0, self.image.Width, self.image.Height, pixelData)
    
    
    self.tmpPaintCanvas = NewCanvas(self.viewportRect.width, self.viewportRect.height)
    self.tmpPaintCanvas.Clear(-1);
    
    self.vSliderData = editorUI:CreateSlider({x = 235-3, y = 44+3 + 8, w = 10, h = 193-24 - 7 - 8}, "vsliderhandle", "Scroll text vertically.")
    self.vSliderData.onAction = function(value) self:OnVerticalScroll(value) end
    
    self.hSliderData = editorUI:CreateSlider({ x = 4+3, y = 211-3, w = 233-7, h = 10}, "hsliderhandle", "Scroll text horizontally.", true)
    self.hSliderData.onAction = function(value) self:OnHorizontalScroll(value) end
    
    self.sizeBtnData = editorUI:CreateButton({x = 232, y = 208}, "scalemode0", "Pick the scale mode.")
    self.sizeBtnData.onAction = function() self:OnNextZoom() end

    self:OnNextZoom()

    self:InvalidateCanvas()

    pixelVisionOS:RegisterUI({name = "OnUpdateCanvasPanel"}, "UpdateCanvasPanel", self)
    pixelVisionOS:RegisterUI({name = "OnDrawCanvasPanel"}, "DrawCanvasPanel", self)

    self.onClick = function()
        print("On Click")

        self:CanvasRelease(true)

    end

    self.triggerOnFirstPress = function()

        print("First Press")
        
        --self:CanvasPress(tmpData, true)
    
        -- Trigger fill here since it only happens on the fist press
        if(self.tool == "fill") then
    
          -- TODO need to set this one on a timer
    
          -- Update the fill color
          self.tmpPaintCanvas:SetPattern({self.brushColor}, 1, 1)
    
          self.tmpPaintCanvas:FloodFill(self.startPos.x, self.startPos.y)
    
          editorUI:Invalidate(self)
        end
        
        -- Trigger first press callback
        if(self.onFirstPress ~= nil) then
    
          self.onFirstPress()
    
          editorUI:Invalidate(self)
          
        end
    
      end
    

end

function PaintTool:InvalidateCanvas()
    self.displayInvalid = true
end

function PaintTool:IResetCanvasValidation()
    self.displayInvalid = false
end

function PaintTool:UpdateCanvasPanel(timeDelta)

    -- If the button has data but it's not enabled exit out of the update
    if(self.canvasPanel.enabled == false) then

        -- If the button is disabled but still in focus we need to remove focus
        if(self.canvasPanel.inFocus == true) then
            editorUI:ClearFocus(self.canvasPanel)
        end

        -- See if the button needs to be redrawn.
        self:InvalidateCanvas()

        -- Shouldn't update the button if its disabled
        return

    end

    -- -- Make sure we don't detect a collision if the mouse is down but not over this button
    -- if(editorUI.collisionManager.mouseDown and self.canvasPanel.inFocus == false) then
    --     -- See if the button needs to be redrawn.
    --     self:InvalidateCanvas()
    --     return
    -- end

    -- Change the scale
    if(Key(Keys.OemMinus, InputState.Released) and self.scaleMode > 1) then
        self:OnNextZoom(true)
    elseif(Key(Keys.OemPlus, InputState.Released) and self.scaleMode <= #self.scaleValues) then
        self:OnNextZoom()
    end

    -- -- Only update the tool's UI when the modal isn't active
    -- if(self.targetFile ~= nil and self.toolLoaded == true) then

        -- Update the slider
        editorUI:UpdateSlider(self.vSliderData)

        -- Update the slider
        editorUI:UpdateSlider(self.hSliderData)

    -- end

    editorUI:UpdateButton(self.sizeBtnData)

    local overrideFocus = (self.canvasPanel.inFocus == true and editorUI.collisionManager.mouseDown)

    if(self.viewportRect.Contains(editorUI.mouseCursor.pos) == true or overrideFocus) then

        if(self.canvasPanel.inFocus ~= true and editorUI.inFocusUI == nil) then
            editorUI:SetFocus(self.canvasPanel, self.currentCursorID)
        end

        if(self.canvasPanel.inFocus == true) then

            -- TODO need to adjust for scroll and scale
            local tmpPos = NewPoint(
                math.floor((editorUI.collisionManager.mousePos.x - self.viewportRect.x ) / self.scale) + self.scaledViewport.X,
                math.floor((editorUI.collisionManager.mousePos.y - self.viewportRect.y)/ self.scale) + self.scaledViewport.Y
            )

            -- print("Mouse Pos", dump(editorUI.collisionManager.mousePos), "adjusted", tmpPos, self.scaledViewport.X)

            -- Check to see if the button is pressed and has an onAction callback
            if(editorUI.collisionManager.mouseReleased == true) then

                -- Click the button
                self.onClick()
                self.firstPress = true
        
            elseif(editorUI.collisionManager.mouseDown) then
        
                self.mouseState = "dragging"
        
        
                if(self.firstPress ~= false) then

                    -- Save start position
                    self.startPos = NewPoint(tmpPos.X, tmpPos.Y)
            
                    self.triggerOnFirstPress()
                    -- end
            
                    self.mouseState = "pressed"
            
                    -- Change the flag so we don't trigger first press again
                    self.firstPress = false

                end
        
                self:DrawOnCanvas(tmpPos)
        
            end

            self.mCol = math.floor((editorUI.mouseCursor.pos.X - self.viewportRect.X + self.scaledViewport.X)/self.gridSize)
            self.mRow = math.floor((editorUI.mouseCursor.pos.Y - self.viewportRect.Y + self.scaledViewport.Y)/self.gridSize)
        
        end
        -- print("Preview", self.mCol, self.mRow)

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

    -- Convert the value percent to a number
    value = math.abs(math.floor(((self.scaledViewport.Width - self.boundaryRect.Width) - self.scaledViewport.Width) * value))
    
    self.scaledViewport.X = self:SnapToGrid(value)--math.floor(math.abs(math.floor(((self.scaledViewport.Width - self.boundaryRect.Width) - self.scaledViewport.Width) * value))/self.gridSize) * self.gridSize

    self:InvalidateCanvas()
end

function PaintTool:OnVerticalScroll(value)

    value = math.abs(math.floor(((self.scaledViewport.Height - self.boundaryRect.Height) - self.scaledViewport.Height) * value))

    self.scaledViewport.Y = self:SnapToGrid(value)--math.floor(math.abs(math.floor(((self.scaledViewport.Height - self.boundaryRect.Height) - self.scaledViewport.Height) * value))/self.gridSize) * self.gridSize

    self:InvalidateCanvas()

end

function PaintTool:SnapToGrid(value, offset)


    -- self.scale) + self.scaledViewport.X

    -- local gridSize = self.scale * 8
    return self.snapToGrid == true and math.floor(value / self.gridSize)* self.gridSize or value
end



function PaintTool:DrawCanvasPanel()

    if(self.displayInvalid == true and pixelVisionOS:IsModalActive() == false) then

        if(self.clearBG) then
            -- print("Refresh background")
            self:ClearBackground()
            self.clearBG = false
        
        end
        
        -- Draw the pixel data in the upper left hand cornver of the tool's window
        self.imageCanvas:DrawPixels(self.viewportRect.X, self.viewportRect.Y, DrawMode.TilemapCache, self.scale, -1, self.maskColor, self.colorOffset, self.scaledViewport)

        self.tmpPaintCanvas:DrawPixels(self.viewportRect.X, self.viewportRect.Y, DrawMode.TilemapCache, self.scale, -1, self.emptyColorID, self.colorOffset, NewRect(0, 0, self.scaledViewport.Width, self.scaledViewport.Height))
        
        
        self.displayInvalid = false

    end

    -- if(self.canvasPanel.inFocus == true) then

    --     -- print(editorUI.mouseCursor.pos)

    --     local tmpX = self.mCol * self.gridSize
    --     local tmpY = self.mRow * self.gridSize
        
    --     if(tmpX < (self.tmpPaintCanvas.width * self.scale) and tmpY < (self.tmpPaintCanvas.height * self.scale)) then

    --         -- TODO If dragging, snap this to the mouse position

    --         tmpX = tmpX + self.viewportRect.X - self.scaledViewport.X
    --         tmpY = tmpY + self.viewportRect.Y- self.scaledViewport.Y

    --         self.overCanvas:DrawPixels( tmpX - 3 , tmpY - 3, DrawMode.UI )

    --         self.tmpPaintCanvas:DrawPixels(tmpX, tmpY, DrawMode.SpriteAbove, self.scale, -1, self.maskColor, 0, NewRect( self.mCol * 8  + self.scaledViewport.X, self.mRow * 8   + self.scaledViewport.Y, 8, 8 ))

    --     else
    --         editorUI:ClearFocus(self.canvasPanel)
    --     end
    -- end


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

    local imageWidth = math.floor(self.imageCanvas.width * self.scale)
    local imageHeight = math.floor(self.imageCanvas.height * self.scale)

    local viewWidth = math.floor(self.viewportRect.Width / self.scale)
    local viewHeight = math.floor(self.viewportRect.Height / self.scale)

    self.scaledViewport.Width = Clamp(viewWidth, 1, math.max(imageWidth, self.imageCanvas.width)) --math.min(self.viewportRect.Width, math.min(self.tmpPaintCanvas.width * self.scale, math.ceil(self.viewportRect.Width / self.scale)))
    self.scaledViewport.Height = Clamp(viewHeight, 1, math.max(imageHeight, self.imageCanvas.height))--, self.viewportRect.Height) --math.min(self.viewportRect.Height, math.min(self.tmpPaintCanvas.height * self.scale, math.ceil(self.viewportRect.Height / self.scale)))

    -- Calculate the boundary for scrolling
    self.boundaryRect.Width = self.imageCanvas.width - self.scaledViewport.Width
    self.boundaryRect.Height = self.imageCanvas.height - self.scaledViewport.Height

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

    -- Find the next sprite for the button
    local spriteName = "scalemode"..(self.scaleMode - 1)

    -- Change sprite button graphic
    self.sizeBtnData.cachedMetaSpriteIds = {
        up = FindMetaSpriteId(spriteName .. "up"),
        down = FindMetaSpriteId(spriteName .. "down") ~= -1 and FindMetaSpriteId(spriteName .. "down") or FindMetaSpriteId(spriteName .. "selectedup"),
        over = FindMetaSpriteId(spriteName .. "over"),
        selectedup = FindMetaSpriteId(spriteName .. "selectedup"),
        selectedover = FindMetaSpriteId(spriteName .. "selectedover"),
        selecteddown = FindMetaSpriteId(spriteName .. "selecteddown") ~= -1 and FindMetaSpriteId(spriteName .. "selecteddown") or FindMetaSpriteId(spriteName .. "selectedover"),
        disabled = FindMetaSpriteId(spriteName .. "disabled"),
        empty = FindMetaSpriteId(spriteName .. "empty") -- used to clear the sprites
    }

    editorUI:Invalidate(self.sizeBtnData)

    pixelVisionOS:DisplayMessage("Image scale " .. (self.scale * 100) .. "%")

end

function PaintTool:ClearBackground()
    
    -- TODO need to find only the area we need to redraw to reduce flicker

    local cols = self.viewportRect.Width / 8
    
    local total = cols * (self.viewportRect.Height / 8)

    local spriteId = MetaSprite( "emptycolor" ).Sprites[1].Id

    for i = 1, total do
        
        local pos = CalculatePosition( i-1, cols )

        DrawSprite(spriteId, self.viewportRect.X + (pos.X * 8), self.viewportRect.Y + (pos.Y * 8), false, false, DrawMode.TilemapCache)
        
    end

end

function PaintTool:ChangeCanvasTool(toolName, cursorID)


    if(toolName == "circlefill") then

        toolName = "circle"
        self.fill = true

    elseif(toolName == "rectanglefill") then

        toolName = "rectangle"
        self.fill = true

    else
        self.fill = false
    end

    print("Change Tool", toolName)

    self.tool = toolName
  
    -- Clear the selection when changing tools/
    if(self.tool ~= "selection" and self.selectRect ~= nil) then
      self:CancelCanvasSelection()
    end
  
    -- TODO change the cursor
    if(self.tool == "pen") then
  
      self.currentCursorID = cursorID or 6
      self:ResetCanvasStroke()
  
    elseif(self.tool == "eraser") then
  
      self.currentCursorID = cursorID or 7
  
    --   ReplaceColor(self.brushColorID, self.emptyColorID)
  
    else
      
      self.currentCursorID = 8
      self:ResetCanvasStroke()
  
    end
  
    -- Save the new cursor for tools that need to restore
    self.defaultCursorID = self.currentCursorID
  
  end

  function PaintTool:CancelCanvasSelection(mergeSelection, action)

    -- if(mergeSelection ~= false and data.selectedPixelData ~= nil) then
    --   data.tmpPaintCanvas:SetPixels(data.selectRect.Left, data.selectRect.Top, data.selectedPixelData.size.Width, data.selectedPixelData.size.Height, data.selectedPixelData.pixelData)
    -- end
   
    -- data.selectedPixelData = nil
    -- data.selectionState = "none"
    -- data.selectRect = nil
    
    -- if(action ~= false) then
    --   --Fire a release event
    --   self:CanvasRelease(data, true)
    -- end
  
  end

  -- Use this to perform a click action on a button. It's used internally when a mouse click is detected.
function PaintTool:CanvasRelease(callAction)

    print("Canvas Release")

    -- Clear the start position
    self.startPos = nil
  
    -- data.mouseState = data.mouseState == "released" and "up" or "released"
  
    -- -- Merge the pixel data from the tmp canvas into the main canvas before it renders
    -- data.tmpPaintCanvas:MergeCanvas(self.tmpPaintCanvas, 0, true)
  
    -- TODO we can optimize this by passing in a rect for the area to merge
    self.imageCanvas:MergeCanvas(self.tmpPaintCanvas, 0, true)

    -- -- Clear the canvas
    self.tmpPaintCanvas:Clear()
    
    -- if(data.selectRect ~= nil and (data.selectRect.Width == 0 or data.selectRect.Height == 0)) then
    --   data.selectRect = nil
    -- end
  
    -- if(data.selectionCanvas.invalid == true) then
  
    --   data.selectionCanvas:ResetValidation()
  
    -- end
  
    -- local oldPixelData = nil
  
    -- if(data.selectedPixelData ~= nil) then
  
    --   oldPixelData = data.tmpPaintCanvas:GetPixels()
  
    --  --TODO need to test for a special key down and toggle ignoring transparency
    --  data.ignoreMaskColor = true
      
    --   data.tmpPaintCanvas:SetPixels(data.selectRect.Left, data.selectRect.Top, data.selectedPixelData.size.Width, data.selectedPixelData.size.Height, data.selectedPixelData.pixelData)
      
  
    -- end

    -- -- trigger the canvas action callback
    -- if(data.onAction ~= nil and callAction ~= false) then

    -- -- Trigger the onAction call back and pass in the double click value if the button is set up to use it
    -- data.onAction()

    -- end

    -- if(oldPixelData ~= nil) then
    --     data.tmpPaintCanvas:SetPixels(oldPixelData)
    --     --data.tmpPaintCanvas:Invalidate()
    -- end

end

function PaintTool:ResetCanvasStroke()

    print("ResetCanvasStroke")

    local tmpColor = self.brushColor
  
    -- local realBrushColor = tmpColor + self.colorOffset
  
    -- Change the stroke to a single pixel
    self.tmpPaintCanvas:SetStroke(self.brushColor, self.defaultStrokeWidth)--realBrushColor, )
    self.tmpPaintCanvas:SetPattern({self.brushColor}, 1, 1)
  
    -- ReplaceColor(self.brushColorID, tmpColor + self.colorOffset)
  
  end

function PaintTool:DrawOnCanvas(mousePos)

   
    -- print("self.startPos", self.startPos, "mousePos", mousePos)

    -- Get the start position for a new drawing
    if(self.startPos ~= nil) then

        -- Test for the data.tool and perform a draw action
        if(self.tool == "pen") then

            self:ResetCanvasStroke()

            self.tmpPaintCanvas:DrawLine(self.startPos.x, self.startPos.y, mousePos.x, mousePos.y)

            self.startPos = NewPoint(mousePos.x, mousePos.y)

            self:InvalidateCanvas()

        elseif(self.tool == "eraser") then

            -- Change the stroke the empty color
            self.tmpPaintCanvas:SetStroke(self.emptyColorID, self.defaultStrokeWidth)

            self.tmpPaintCanvas:DrawLine(self.startPos.x, self.startPos.y, mousePos.x, mousePos.y)
            self.startPos = NewPoint(mousePos.x, mousePos.y)

            self:InvalidateCanvas()

        elseif(self.tool == "line") then

            self.tmpPaintCanvas:Clear()

            self:ResetCanvasStroke()

            self.tmpPaintCanvas:DrawLine(self.startPos.x, self.startPos.y, mousePos.x, mousePos.y, self.fill)

            -- force the paint canvas to redraw
            --data.tmpPaintCanvas:Invalidate()

            self:InvalidateCanvas()

        elseif(self.tool == "rectangle") then


            self.tmpPaintCanvas:Clear()

            self:ResetCanvasStroke()

            -- TODO this is fixed in the canvas
            self.tmpPaintCanvas:DrawRectangle(
                math.min(self.startPos.x, mousePos.x), 
                math.min(self.startPos.y, mousePos.y),
                math.abs(mousePos.x - self.startPos.x)+ 1,
                math.abs(mousePos.y - self.startPos.y) + 1, 
                self.fill
            )

            self:InvalidateCanvas()

        elseif(self.tool == "circle") then

            self.tmpPaintCanvas:Clear()

            self:ResetCanvasStroke()

            -- TODO this is fixed in the canvas
            self.tmpPaintCanvas:DrawEllipse(
                math.min(self.startPos.x, mousePos.x),
                math.min(self.startPos.y, mousePos.y),
                math.abs(mousePos.x - self.startPos.x)+ 1,
                math.abs(mousePos.y - self.startPos.y) + 1,
                self.fill
            )

            self:InvalidateCanvas()

        -- elseif(data.tool == "select") then

        -- if(data.mouseState == "pressed") then

        --   if(data.selectRect == nil) then

        --       data.selectionState = "new"

        --       data.selectRect = NewRect(data.startPos.x, data.startPos.y, 0, 0)

        --   else
            
        --     if(data.selectRect:Contains(mousePos) == true) then

        --       data.selectionState = "newmove"

        --       data.moveOffset = NewPoint(data.selectRect.X - mousePos.X, data.selectRect.Y - mousePos.Y)

        --       if(data.selectedPixelData == nil) then
                
        --         data.selectedPixelData = self:CutPixels(data)
        --       end

        --     else

        --       self:CancelCanvasSelection(data)
                
        --     end

        --   end

        elseif(self.mouseState == "dragging")  then

        --   if(data.selectRect ~= nil) then
        --     if(data.selectionState == "new" or data.selectionState == "resize") then

        --       data.selectionState = "resize"

        --       -- print("resize", data.selectRect, mousePos, , )

        --       data.selectRect.X = math.min(data.startPos.X, mousePos.X)
        --       data.selectRect.Y = math.min(data.startPos.Y, mousePos.Y)
        --       data.selectRect.Width = Clamp(math.abs(mousePos.X - data.startPos.X), 0, data.tmpPaintCanvas.width)
        --       data.selectRect.Height = Clamp(math.abs(mousePos.Y - data.startPos.Y), 0, data.tmpPaintCanvas.height)

        --     else


        --       editorUI.cursorID = 2

        --       data.selectRect.X = mousePos.X + data.moveOffset.X --  data.selectionSize.X
        --       data.selectRect.Y = mousePos.Y + data.moveOffset.Y --  data.selectionSize.Y
            

        --       data.selectionState = "move"

        --     end
        --   end

        -- end



        elseif(self.tool == "eyedropper") then

            -- TODO this doesn't appear to do anything
            self.overColor = self.tmpPaintCanvas:ReadPixelAt(mousePos.x, mousePos.y) - self.colorOffset

        -- if(self.overColor > pixelVisionOS.colorsPerSprite) then
        --     self.overColor = -1
        -- end

        -- TODO should we display this value or that it is out of bounds?

        end

    end

end

