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

function EditorUI:CreateInputArea(rect, text, toolTip, font, colorOffset, spacing)

  font = font or "medium"
  spacing = spacing or -4
  colorOffset = colorOffset or 15

  local data = self:CreateTextEditor(rect, text, toolTip, font, colorOffset, spacing)

  data.name = "InputArea" .. data.name
  data.drawMode = DrawMode.Tile
  data.editing = false
  data.onValidate = nil
  data.toolTip = toolTip
  data.onAction = nil
  data.scrollValue = {h = 0, v = 0}
  data.totalLines = 0
  data.maxLineWidth = 0



  if(text ~= nil) then
    self:ChangeInputArea(data, text)
  end

  return data

end



function EditorUI:UpdateInputArea(data)

  -- Test to see if the enabled state has changed
  if(data.lastEnabledState ~= data.enabled) then
    -- print(data.name, "Enabled state changed", data.enabled)

    -- Save the new state for the next frame
    data.lastEnabledState = data.enabled

    -- Invalidate the display
    self:TextEditorInvalidateBuffer(data)
  end

  self:TextEditorUpdate(data, self.timeDelta)

end

function EditorUI:EditInputArea(data, value)

  self:EditTextEditor(data, value)

end

function EditorUI:ChangeInputArea(data, text, trigger)

  self:TextEditorImport(data, text)

  if(trigger ~= false and data.onAction ~= nil) then
    data.onAction(self:TextEditorExport(data))
  end

end
