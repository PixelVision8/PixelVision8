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

function MusicTool:CreateSongSlider()

  -- Song ID Input field
  self.songSliderData = editorUI:CreateSlider({x = 12, y = 58, w = 232, h = 16}, "hsliderhandle", "This is a horizontal slider.", true)
  self.songSliderData.onAction = function(value) self:OnSongScroll(value) end

  -- Force the slider to have a different start value so it can be updated correctly when the first song loads up
  self.songSliderData.value = -1

  pixelVisionOS:RegisterUI({name = "UpdateSongSliderLoop"}, "UpdateSongSlider", self)

end

function MusicTool:UpdateSongSlider()

  for i = 1, self.totalSongFields do

    editorUI:UpdateSongInputField(self.songInputFields[i], editorUI.timeDelta)

  end
  
  editorUI:UpdateSlider(self.songSliderData)
  
end

function MusicTool:ChangeSongSlider(value)

  editorUI:ChangeSlider(self.songSliderData, value)

end

function EditorUI:UpdateSongInputField(data, dt)

  local overrideFocus = (data.inFocus == true and self.collisionManager.mouseDown)

  local doubleClickDelay = .45

  -- TODO this should be only happen when in focus
  local cx = self.collisionManager.mousePos.c - data.tiles.c
  local cy = self.collisionManager.mousePos.r - data.tiles.r

  -- Ready to test finer collision if needed
  if(self.collisionManager:MouseInRect(data.rect) == true or overrideFocus) then

    if(data.enabled == true) then

      -- If the button wasn't in focus before, reset the timer since it's about to get focus
      if(data.inFocus == false) then
        -- print(data.name, "Reset focus")
        data.doubleClickTime = 0
        data.doubleClickActive = false
      end

      data.doubleClickTime = data.doubleClickTime + self.timeDelta


      if(data.doubleClickActive and data.doubleClickTime > doubleClickDelay) then
        data.doubleClickActive = false
      end

      if(data.inFocus == false) then
        -- Set focus
        self:SetFocus(data, data.editing == true and 3 or 2)
      end

      self:TextEditorMouseMoved(data, cx, cy)

      if(self.collisionManager.mouseReleased == true and data.editing == false) then

        -- self:EditTextEditor(data, true)
        if(data.doubleClickActive and data.doubleClickTime < doubleClickDelay) then

          -- TODO should this be edited on double click?

        else
          self:SelectSongInputField(data, true)

        end

        data.doubleClickTime = 0
        data.doubleClickActive = true

      end

    else

      -- If the mouse is not in the rect, clear the focus
      if(data.inFocus == true) then
        self:ClearFocus(data)
      end

    end

  else
    -- If the mouse isn't over the component clear the focus
    self:ClearFocus(data)

  end

  if(data.editing == true and self.collisionManager.mouseDown == true) then
    self:EditTextEditor(data, false)
  end

  -- Only redraw the line if the buffer isn't about to redraw
  if(data.invalidateBuffer == false) then
    self:TextEditorDrawLine(data)
  end

  -- Redraw the display
  self:TextEditorDrawBuffer(data)
end

function EditorUI:SelectSongInputField(data, value, triggerAction)

  data.selected = value

  if(value == true) then
    self:TextEditorSelectAll(data)
  else
    self:TextEditorDeselect(data)
  end

  if(data.onSelected ~= nil and value == true and triggerAction ~= false) then
    data.onSelected(data.index)
  end
end

function MusicTool:OnSongScroll(value)

  if(value ~= nil) then
    local totalPatterns = #self.currentSongPatterns - self.totalSongFields
    self.songScrollOffset = Clamp(value * totalPatterns, 0, totalPatterns)
  end

  editorUI:ClearGroupSelections(self.songEndButtons)

  -- Clear music selection
  local realIndex = self.currentSelectedSong - self.songScrollOffset

  if(realIndex > 0 and realIndex < self.totalSongFields) then
    editorUI:SelectSongInputField(self.songInputFields[realIndex], false)
  end

  local enabled = false
  local field = nil
  local index = 0

  for i = 1, self.totalSongFields do

    index = i + self.songScrollOffset

    self.songEndButtons.buttons[i].toolTip = "Click to end the song at pattern index '".. tostring(index - 1) .. "'."

    self.songInputFields[i].toolTip = "Click to select the song's pattern at index '".. index .. "'."

    if(i == 1) then

      if(index == 1) then
        DrawMetaSprite(FindMetaSpriteId("songstart"), 16 / 8, 48 / 8, false, false, DrawMode.Tile)
      end

      editorUI:Enable(self.firstSongEndButton, index ~= 1)

    end

    if(index == self.songEndPos) then

      -- TODO this is off by 1
      editorUI:SelectToggleButton(self.songEndButtons, i + 1, false)

    end

    local tmpValue = gameEditor:SongPatternAt(self.currentSongID, index - 1)

    enabled = index >= self.songStartPos and index <= self.songEndPos
    field = self.songInputFields[i]

    editorUI:ChangeInputField(field, tostring(tmpValue), false)

    editorUI:SelectSongInputField(self.songInputFields[i], self.currentSelectedSong == (i + self.songScrollOffset), false)

    editorUI:Enable(field, enabled)

  end

end

function MusicTool:OnSelectSongField(value)

  -- TODO need to make sure this value is within the range

  -- print("self.songScrollOffset", self.songScrollOffset)

  if(self.currentSelectedSong ~= nil and self.currentSelectedSong ~= (value + 1)) then

    -- TODO need to calculate real index

    local realIndex = self.currentSelectedSong - self.songScrollOffset


    if(realIndex > 0 and realIndex < self.totalSongFields) then
      -- print("Deselect", self.currentSelectedSong, realIndex)

      editorUI:SelectSongInputField(self.songInputFields[realIndex], false)
    end

  end

  -- Account for 0 index value
  self.currentSelectedSong = (value + 1) + self.songScrollOffset

  local realIndex = self.currentSelectedSong - self.songScrollOffset

  if (realIndex < 1 or realIndex > self.totalSongFields) then
    local totalPatterns = #self.currentSongPatterns - self.totalSongFields

    local scroll = (self.currentSelectedSong - 1) / totalPatterns
    local scrollX = math.floor(scroll * self.songSliderData.size) + 11
    self.songSliderData.handleX = scrollX
    self:OnSongScroll(scroll)

    realIndex = self.currentSelectedSong - self.songScrollOffset
  end

  self:LoadLoop(tonumber(self.songInputFields[realIndex].text))

  self:OnSongScroll()

end
