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

-- Creating a composite component (Picker, ToggleGroup, Buttons)
function PixelVisionOS:CreateColorPicker(rect, tileSize, total, totalPerPage, maxPages, colorOffset, spriteName, toolTip, modifyPages, enableDragging, dragBetweenPages)

  local columns = rect.w / tileSize.x
  local rows = rect.h / tileSize.y

  -- Create the generic UI data for the component
  local data = self:CreateItemPicker(rect, tileSize, columns, rows, 0, spriteName, toolTip, enableDragging, "ColorPicker")

  -- Modify the name to a ColorPicker
  data.name = "ColorPicker" .. data.name

  -- Fix color offset
  data.altColorOffset = colorOffset
  data.overItemDrawArgs[9] = 0

  -- Create pagination
  data.totalPerPage = data.visibleColumns * data.visibleRows
  data.pageSize = totalPerPage
  data.totalPages = 0--math.floor(data.totalItems / data.totalPerPage)
  data.maxPages = maxPages
  data.pageOverTime = 0
  data.pageOverDelay = .5
  data.pageOverLast = -1
  data.total = total
  data.dragBetweenPages = dragBetweenPages

  -- Shift picker color for selection and over
  -- data.picker.colorOffset = PaletteOffset( 0 )

  -- Set the picker sprites
  -- data.picker.cachedMetaSpriteIds.up = FindMetaSpriteId(?)

  data.lastSelectedPage = 1
  -- Stop the picker from drawing the selection
  data.picker.drawSelected = false
  data.selectedPixelDrawArgs = {
    nil,
    0,
    0,
    8,
    8,
    false,
    false,
    DrawMode.Sprite,
    0
  }

  self:ConfigureEmptyDragColorPickerSprites(data)

  -- Need to clean up selection on action callback
  data.picker.onAction = function(value, doubleClick)

    -- Clear the last over id to force the over box to redraw
    data.lastOverID = -1

    value = self:CalculateItemPickerPosition(data)

    self:SelectItemPickerIndex(data, value.index, true, false)

    data.selectedPixelDrawArgs[1] = self:ReadItemPickerOverPixelData(data, value.x, value.y)

    if(data.onAction ~= nil) then
      data.onAction(value.index, doubleClick)
    end

  end

  data.pagePosition = NewPoint(
    rect.x + rect.w,
    rect.y + rect.h
  )

  data.realHeight = rect.h

  -- Recalculate scrollScale so the hight is based on the real page height
  data.scrollScale.y = math.ceil((((data.realHeight / 8) - data.visibleRows) / data.maxScale))

  -- Shift the pagination down if there is a horizontal scroll bar
  if(data.hSlider ~= nil) then
    data.pagePosition.y = data.pagePosition.y + 16
  end

  data.pages = editorUI:CreateToggleGroup()

  data.pages.onAction = function(value)

    self:OnColorPickerPage(data, value)

  end

  -- TODO need to pad the pages
  data.pageToolTipTemplate = toolTip .. " page "

  data.emptyColorSpriteId = MetaSprite(FindMetaSpriteId("emptycolor")).Sprites[i].Id


  local pixelData = Sprite(data.emptyColorSpriteId)

  local tmpCanvas = NewCanvas(16, 16)

  tmpCanvas.SetPattern(pixelData, 8, 8)

  tmpCanvas.FloodFill(0, 0)

  data.emptyPixelData = tmpCanvas.GetPixels()

  self:ColorPickerVisiblePerPage(data, totalPerPage)

  return data

end

function PixelVisionOS:ConfigureEmptyDragColorPickerSprites(data, spriteID, width)

  -- TODO need to cancel this if the ctrl key is down
  local scale = width or data.itemSize.x / 8

  -- Update the emptySpriteList
  -- Need to resize empty sprite block
  local emptySpriteList = {}
  local totalSprites = scale * scale
  
  for i = 1, totalSprites do
    table.insert(emptySpriteList, spriteID or data.emptyColorSpriteId)
  end

  --  TODO disabled these for testing
  -- data.emptyDrawArgs[1] = emptySpriteList
  -- data.emptyDrawArgs[4] = scale

end

function PixelVisionOS:ColorPickerVisiblePerPage(data, value)

  data.visiblePerPage = value

  self:InvalidateColorPickerCache(data)

  self:InvalidateItemPickerDisplay(data)

end

function PixelVisionOS:RebuildColorPickerCache(data)

  -- Recalculate the number of pages
  self:RebuildPickerPages(data)

  -- TODO needed to do a hack here to keep the canvas from crashing if totalPages is 0

  -- Resize the canvas based on the total pages
  data.canvas.Resize(data.canvas.width, data.totalPages == 0 and 1 or (data.totalPages * 64) * 16)

  -- Clear the canvas
  data.canvas.Clear()

  -- Set up the values for the building steps
  data.nextCacheIndex = 1
  data.nextLoopTotal = 128
  data.nextLoopBlock = data.nextLoopTotal
  data.nextCacheTotal = data.totalPerPage * data.totalPages

  -- Delay between build loops
  data.buildCacheDelay = .05
  data.buildCacheTime = 0

  -- flag the cache to build on the update loop
  data.buildingCache = true

end

function PixelVisionOS:OnNextColorPickerCacheStep(data)

  for i = data.nextCacheIndex, data.nextLoopTotal do

    -- Lua loops start at 1 but we need to start at 0
    local index = i - 1

    self:DrawColorPickerColorItem(data, index)

    if(i > data.nextCacheTotal) then
      data.buildingCache = false

      self:OnColorPickerPage(data, data.lastSelectedPage)
      data.lastOverID = -1
      self:InvalidateItemPickerDisplay(data)
      return
    end

  end

  data.nextLoopTotal = data.nextLoopTotal + data.nextLoopBlock

  self:InvalidateItemPickerDisplay(data)

end

function PixelVisionOS:ColorPickerChangeColor(data, index, color)


  if(data.onChange ~= nil) then

    data.onChange(index, color)

  end

  self:DrawColorPickerColorItem(data, index- data.altColorOffset)

  self:InvalidateItemPickerDisplay(data)

end

function PixelVisionOS:InvalidateColorPickerPage(data)

  data.invalidPage = true
  
end

function PixelVisionOS:DrawColorPickerColorItem(data, id)

  local pos = CalculatePosition(id, data.columns)
    x = pos.x * data.itemSize.x
    y = pos.y * data.itemSize.y

    
  if(data.onDrawColor ~= nil) then 
    -- print("Draw Color", id)
    data.onDrawColor(data, id, x ,y)
  else
    data.canvas.DrawSprites(emptycolor.spriteIDs, x, y, emptycolor.width, false, false)
  end

end

function PixelVisionOS:UpdateColorPicker(data)

  if(data.buildingCache == true ) then

    data.buildCacheTime = data.buildCacheTime + editorUI.timeDelta

    if(data.buildCacheTime >= data.buildCacheDelay) then
      self:OnNextColorPickerCacheStep(data)
      data.buildCacheTime = 0
    end
  end

  -- Rebuild the pixel cache if a change has happened
  if(data.invalidPixelCache == true) then
    self:RebuildColorPickerCache(data)

    data.invalidPixelCache = false
  end

  if(data.invalidPage == true) then 

    self:OnColorPickerPage(data, data.currentPage)
    data.invalidPage = false
    
  end

  editorUI:UpdateToggleGroup(data.pages)

  if(data.picker.selected > - 1 and data.dragging ~= true) then

    -- Draw selection sprites when we are not dragging
    -- self.editorUI:NewDraw("DrawSprites", data.picker.selectedDrawArgs)
    -- TODO draw selection", data.picker.cachedMetaSpriteIds.up, data.picker.colorOffset)
    DrawMetaSprite(
      data.picker.cachedMetaSpriteIds.up,
      data.picker.selectedPos.x,
      data.picker.selectedPos.y,
      false, 
      false, 
      DrawMode.Sprite, 
      data.picker.colorOffset
    )

  end

  self:UpdateItemPicker(data)

  -- Check for dragging over a page
  if(data.dragging == true and data.dragBetweenPages == true) then

    local pageButtons = data.pages.buttons

    for i = 1, #pageButtons do
      local tmpButton = pageButtons[i]

      local hitRect = tmpButton.rect

      if(self.editorUI.collisionManager:MouseInRect(hitRect) == true and tmpButton.enabled == true) then

        -- Test if over a new page
        if(data.pageOverLast ~= i) then

          data.pageOverLast = i
          data.pageOverTime = 0

        end

        data.pageOverTime = data.pageOverTime + self.editorUI.timeDelta

        if(data.pageOverTime > data.pageOverDelay)then
          data.pageOverTime = 0
          self:OnColorPickerPage(data, i)

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

function PixelVisionOS:OnColorPickerPage(data, value)

  -- print("OnColorPickerPage", data.name, value)

  value = value or 0

  value = Clamp(value, 1, 16)

  -- Calculate the lastStartY position
  data.lastStartY = (value - 1) * (data.visibleRows * data.itemSize.y)

  data.scrollShift.y = data.lastStartY

  -- Select the correct toggle button
  editorUI:SelectToggleButton(data.pages, value)

  data.lastSelectedPage = value

  -- Reset the scroll bar
  -- editorUI:ChangeSlider(data.vSlider, 0, false)

  local maxTotal = value * data.totalPerPage

  local newTotal = math.min(data.totalPerPage, data.visiblePerPage)

  if(maxTotal > data.total) then
    newTotal = newTotal - (maxTotal - data.total)
  end

  data.picker.total = newTotal

  if(data.currentPage ~= value) then
    -- Invalidate the display
    self:InvalidateItemPickerDisplay(data)

  end

  data.currentPage = value

  if(data.onPageAction ~= nil) then
    data.onPageAction(value)
  end

end


function PixelVisionOS:SelectColorPickerIndex(data, value)

  if(data.picker.enabled == false) then
    return
  end
  
  -- Recalculate the position
  local pos = CalculatePosition(value, data.columns)

  -- Calculate the correct page from the position y value
  local tmpPage = math.floor((pos.y * 8) / data.pageSize)

  -- print(data.name, value, tmpPage, dump(pos),data.pageSize)

  self:OnColorPickerPage(data, tmpPage + 1)

  self:SelectItemPickerIndex(data, value, false, true)

  -- Invalidate the display
  self:InvalidateItemPickerDisplay(data)

end

function PixelVisionOS:ChangeColorPickerTotal(data, value, invalidateCache)

  data.total = value

  if(invalidateCache == true) then
    self:InvalidateColorPickerCache(data)
  end

end

function PixelVisionOS:InvalidateColorPickerCache(data)

  data.invalidPixelCache = true
end

-- TODO maybe move this to the item picker file?
function PixelVisionOS:RebuildPickerPages(data, totalPages)

  -- If the total colors are 0, make the total pages 0 too
  if(data.total == 0 or totalPages == 0) then

    data.totalPages = 0

  else

    -- If there are colors, calculate the correct number of pages
    data.totalPages = Clamp(totalPages ~= nil and totalPages or math.ceil(data.total / data.totalPerPage), 1, data.maxPages)

  end

  -- local totalPages = data.totalPages
  local position = data.pagePosition
  local maxPages = data.maxPages or 10
  local toolTipTemplate = data.pageToolTipTemplate or "Select page "

  -- Clear all the existing buttons
  editorUI:ClearToggleGroup(data.pages)

  -- Need to shift the offset to the left
  local tmpPosX = position.x - (maxPages * 8)

  local rect = {x = 0, y = 0, w = 8, h = 16}

  -- Create new pagination buttons
  for i = 1, maxPages do

    local pageID = data.totalPages - (maxPages - (i - 1)) + 1

    local offsetX = ((i - 1) * 8) + tmpPosX

    rect = {x = offsetX, y = position.y, w = 8, h = 16}
    rect.x = offsetX
    rect.y = position.y

    if(pageID <= 0) then
      -- DrawSprites(pagebuttonempty.spriteIDs, rect.x, rect.y, pagebuttonempty.width, false, false, DrawMode.TilemapCache)
    else
      editorUI:ToggleGroupButton(data.pages, rect, "pagebutton" .. tostring(pageID - 1), toolTipTemplate .. tostring(pageID-1))
    end
  end

  rect = {x = rect.x, y = position.y, w = 8, h = 16}

  if(data.totalPages > 0) then
    DrawMetaSprite(FindMetaSpriteId("pickerbottompageedge"), rect.x + 8, rect.y, false, false, DrawMode.TilemapCache)
  else
    DrawMetaSprite(FindMetaSpriteId("pickerbottompage"), rect.x + 8, rect.y, false, false, DrawMode.TilemapCache)
  end

end

function PixelVisionOS:SelectColorPickerColor(data, value)

  self:SelectItemPickerIndex(data, value)

  -- Calculate the correct page
  local page = math.floor(value / (data.totalPerPage)) + 1

  -- Select the right page
  self:SelectColorPage(data, page)

end

function PixelVisionOS:SelectColorPage(data, value)
  self.editorUI:SelectToggleButton(data.pages, value)
end

function PixelVisionOS:RefreshColorPickerColor(data, colorID)

  self:DrawColorPickerColorItem(data, colorID)
  self:InvalidateItemPickerDisplay(data)

end

function PixelVisionOS:AddNewColorToPicker(data)

  local newTotal = data.total + 1

  self:ChangeColorPickerTotal(data, newTotal)

  self:DrawColorPickerColorItem(data, data.total - 1)

  self:RebuildPickerPages(data)

end

-- Removes the last color from the picker and updates the cache
function PixelVisionOS:RemoveColorFromPicker(data)

  local newTotal = data.total - 1

  self:ChangeColorPickerTotal(data, newTotal)

  self:DrawColorPickerColorItem(data, data.total)

  self:RebuildPickerPages(data)

end

function PixelVisionOS:RemoveColorPicker(data)
  data.visiblePerPage = 0

  self:RebuildPickerPages(data, 0)

  DrawRect(data.rect.x, data.rect.y, data.rect.w, data.rect.h, 0, DrawMode.TilemapCache)
end

function PixelVisionOS:EnableColorPicker(data, pickerEnabled, pagesEnabled)

  self.editorUI:Enable(data.picker, pickerEnabled)
  self.editorUI:Enable(data.pages, pagesEnabled)

end
