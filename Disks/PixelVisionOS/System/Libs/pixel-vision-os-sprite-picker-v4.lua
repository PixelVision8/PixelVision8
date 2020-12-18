function PixelVisionOS:CreateSpritePicker(rect, itemSize, columns, rows, colorOffset, spriteName, toolTip, enableDragging, draggingLabel)

  local data = self:CreateItemPicker(rect, itemSize, columns, rows, colorOffset, spriteName, toolTip, enableDragging, draggingLabel)

  data.name = "SpritePicker" .. data.name

  -- Create pagination
  data.totalPerPage = 256
  data.pageSize = 128
  data.totalPages = math.floor(data.totalItems / data.totalPerPage)
  data.maxPages = Clamp(data.totalPages, 1, 8)
  data.pageOverTime = 0
  data.pageOverDelay = .5
  data.pageOverLast = -1

  data.pagePosition = NewPoint(
    rect.x + rect.w,
    rect.y + rect.h
  )

  data.realHeight = 128

  -- Recalculate scrollScale so the hight is based on the real page height
  data.scrollScale.y = math.ceil((((data.realHeight / 8) - data.visibleRows) / data.maxScale))

  -- Shift the pagination down if there is a horizontal scroll bar
  if(data.hSlider ~= nil) then
    data.pagePosition.y = data.pagePosition.y + 16
  end

  -- Shift the pagination down if there is a horizontal scroll bar
  if(data.vSlider ~= nil) then
    data.pagePosition.x = data.pagePosition.x + 16
  end

  data.picker.onAction = function(value, doubleClick)

    -- Clear the last over id to force the over box to redraw
    data.lastSpriteID = value

    value = self:CalculateItemPickerPosition(data)

    if(data.onAction ~= nil) then
      data.onAction(value.index, doubleClick)
    end

  end

  data.pages = editorUI:CreateToggleGroup()

  data.pages.onAction = function(value)
    self:OnSpritePickerPage(data, value)
  end

  data.pageToolTipTemplate = "Select sprite page "

  self:RebuildPickerPages(data, data.totalPages)

  self:RebuildSpritePickerCache(data)

  self:OnSpritePickerPage(data, 1)

  return data

end

function PixelVisionOS:RebuildSpritePickerCache(data)

  -- Get all the sprite pixel data
  local pixelData = gameEditor:ReadGameSpriteData(0, data.columns, data.rows)

  self:SetItemPickerPixelData(data, pixelData)

  self:InvalidateItemPickerDisplay(data)
end

function PixelVisionOS:UpdateSpritePicker(data)

  editorUI:UpdateToggleGroup(data.pages)

  self:UpdateItemPicker(data)

  -- Check for dragging over a page
  if(data.dragging == true) then

    local pageButtons = data.pages.buttons

    for i = 1, #pageButtons do
      local tmpButton = pageButtons[i]

      local hitRect = tmpButton.rect

      -- TODO need to offset for size?

      if(self.editorUI.collisionManager:MouseInRect(hitRect) == true and tmpButton.enabled == true) then

        -- Test if over a new page
        if(data.pageOverLast ~= i) then

          data.pageOverLast = i
          data.pageOverTime = 0

        end

        data.pageOverTime = data.pageOverTime + self.editorUI.timeDelta

        if(data.pageOverTime > data.pageOverDelay)then
          data.pageOverTime = 0
          self:OnSpritePickerPage(data, i)

          data.pageOverLast = i
          data.pageOverTime = 0
        end

      end

    end

  else
    -- Reset page over flag
    data.pageOverLast = -1
    data.pageOverTime = 0
  end

end

function PixelVisionOS:OnSpritePickerPage(data, value)

  -- Calculate the lastStartY position
  data.lastStartY = (value - 1) * data.pageSize

  data.scrollShift.y = data.lastStartY

  -- Select the correct toggle button
  editorUI:SelectToggleButton(data.pages, value)

  if(data.vSlider ~= nil) then
  
    -- Reset the scroll bar
    editorUI:ChangeSlider(data.vSlider, 0, false)
  
  end
  
  -- Invalidate the display
  self:InvalidateItemPickerDisplay(data)

end

function PixelVisionOS:SelectSpritePickerIndex(data, value)

  -- Recalculate the position
  local pos = CalculatePosition(value, data.columns)

  -- Calculate the correct page from the position y value
  local tmpPage = math.floor((pos.y * 8) / data.pageSize)

  self:OnSpritePickerPage(data, tmpPage + 1)

  self:SelectItemPickerIndex(data, value, false, true)

  -- Invalidate the display
  self:InvalidateItemPickerDisplay(data)

end
