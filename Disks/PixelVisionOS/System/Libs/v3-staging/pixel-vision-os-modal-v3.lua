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
function PixelVisionOS:OpenModal(modal, callBack)

  SaveTilemapCache()

  self.closeDelay = .2
  self.closeTime = -1

  -- Clear the previous mouse focus
  self.editorUI:ClearFocus()

  -- Set the active modal
  self.activeModal = modal

  -- Route the onClose to this close method
  self.activeModal.onParentClose = function()
    self:CloseModal()
  end

  self.onCloseCallback = callBack

  -- Activate the new modal and pass in a reference to pixelVisionOS
  self.activeModal:Open(self)

  -- Disable the menu button in the toolbar
  self.editorUI:Enable(self.titleBar.iconButton, false)

end

function PixelVisionOS:CloseModal()
  if(self.activeModal ~= nil) then

    if(self.activeModal.Close ~= nil) then
      self.activeModal:Close()
    end

    self.editorUI:ClearFocus()

    RestoreTilemapCache()

    -- TODO need to clear any previous message

  end

  self.activeModal = nil

  -- Trigger the callback so other objects can know when the modal is closed
  if(self.onCloseCallback ~= nil) then
    self.onCloseCallback()

    self.onCloseCallback = nil
  end

  -- Enable the menu button in the toolbar
  self.editorUI:Enable(self.titleBar.iconButton, true)

  self.editorUI:Invalidate(self.titleBar)

end

function PixelVisionOS:UpdateModal(deltaTime)

  if(self.activeModal == nil) then
    return;
  end

  self.activeModal:Update(deltaTime)

end

function PixelVisionOS:DrawModal()

  if(self.activeModal == nil or self.activeModal.Draw == nil) then
    return;
  end

  self.activeModal:Draw()

end

function PixelVisionOS:IsModalActive()
  return self.activeModal ~= nil
end

function PixelVisionOS:GenerateModalCanvas(rect, title)

  local theme = self.editorUI.theme.modal

  canvas = NewCanvas(rect.width, rect.height)

  -- Draw the black background
  canvas:SetStroke({theme.outerBorder}, 1, 1)
  canvas:SetPattern({theme.border}, 1, 1)
  canvas:DrawSquare(0, 0, canvas.width - 1, canvas.height - 1, true)

  -- Draw the brown background
  canvas:SetStroke({theme.innerBorderBottom}, 1, 1)
  canvas:SetPattern({theme.background}, 1, 1)
  canvas:DrawSquare(3, 9, canvas.width - 4, canvas.height - 4, true)

  local tmpX = (canvas.width - (#title * 4)) * .5

  canvas:DrawText(title:upper(), tmpX, 1, theme.titleFont, theme.titleColor, theme.titleSpacing)

  -- draw highlight stroke
  canvas:SetStroke({theme.innerBorderTop}, 1, 1)
  canvas:DrawLine(3, 9, canvas.width - 5, 9)
  canvas:DrawLine(3, 9, 3, canvas.height - 5)

  return canvas

end
