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

function EditorUI:CreateText(rect, text, font, colorOffset, spacing, drawMode)

  local data = self:CreateData(rect)
  data.text = ""
  data.font = font or "large-bold"
  data.colorOffset = colorOffset or 0
  data.spacing = spacing or 0
  data.drawMode = DrawMode.TilemapCache
  data.charSize = SpriteSize()

  -- After the component's data is set, update the text
  self:ChangeText(data, text)

  return data

end

function EditorUI:UpdateText(data)

  -- Exit out of update if there is nothing to update
  if(data == nil) then
    return
  end

  local drawArguments = nil
  -- Test to see if we should draw the component
  if(data.invalid == true or DrawMode.Sprite == true) then

    -- We want to render the text from the bottom of the screen so we offset it and loop backwards.
    for i = 1, data.totalLines do

      drawArguments = {
        data.lines[i], -- text (1)
        data.rect.x, -- x (2)
        data.drawMode == DrawMode.Tile and data.tiles.r + (i - 1) or data.rect.y + ((i - 1) * data.charSize.y), -- y (3)
        data.drawMode, -- drawMode (4)
        data.font, -- font (5)
        data.colorOffset, -- colorOffset (6)
        data.spacing, -- spacing (7)
      }

      -- Push a draw call into the UI's draw queue
      self:NewDraw("DrawText", drawArguments)

    end

    if(data.invalid == true) then
      -- We only want to reset the validation if it's invalid since sprite text will keep drawing
      self:ResetValidation(data)

    end
  end

end

function EditorUI:ChangeText(data, text)

  -- If the text is the same, don't update the text component and exit out of the method
  if(data.text == text) then
    return
  end

  self:Invalidate(data)

  -- Save the text on the draw arguments
  data.text = text

  -- We are going to render the message in a box as tiles. To do this, we need to wrap the
  -- text, then split it into lines and draw each line.
  local wrap = WordWrap(data.text, data.tiles.w)
  data.lines = SplitLines(wrap)
  data.totalLines = #data.lines

end
