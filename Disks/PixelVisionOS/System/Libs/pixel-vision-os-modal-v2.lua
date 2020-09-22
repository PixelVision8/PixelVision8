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

  -- Clear the previous mouse focus
  editorUI:ClearFocus()

  -- Set the active modal
  self.activeModal = modal

  -- Route the onClose to this close method
  self.activeModal.onParentClose = function()
    self:CloseModal()
  end

  self.onCloseCallback = callBack

  -- Activate the new modal
  self.activeModal:Open()

  -- Disable the menu button in the toolbar
  editorUI:Enable(self.titleBar.iconButton, false)

  self.modalCloseTime = nil

end

function PixelVisionOS:CloseModal()
  if(self.activeModal == nil) then
    return
  end

  if(self.activeModal.Close ~= nil) then
    self.activeModal:Close()
  end

  editorUI:ClearFocus()

  RestoreTilemapCache()

  self.modalCloseDelay = .15
  self.modalCloseTime = 0

  self.activeModal = nil

  -- print("Close Modal")

end

function PixelVisionOS:UpdateModal(timeDelta)

  if(self.modalCloseTime ~= nil) then

    self.modalCloseTime = self.modalCloseTime + timeDelta

    -- print("self.modalCloseTime", self.modalCloseTime)

    if(self.modalCloseTime >= self.modalCloseDelay) then

      self.modalCloseTime = nil

      -- print("Modal Closed")

      -- Trigger the callback so other objects can know when the modal is closed
      if(self.onCloseCallback ~= nil) then
        self.onCloseCallback()

        -- TODO disabled this because of a race condition when one modal is called after another
        -- self.onCloseCallback = nil
      end

      -- Enable the menu button in the toolbar
      editorUI:Enable(self.titleBar.iconButton, true)

      editorUI:Invalidate(self.titleBar)

    end
    
  end

  if(self.activeModal == nil) then
    return;
  end

  self.activeModal:Update(timeDelta)

end

function PixelVisionOS:DrawModal()

  if(self.activeModal == nil or self.activeModal.Draw == nil) then
    return;
  end

  self.activeModal:Draw()

end

function PixelVisionOS:IsModalActive()
  return self.activeModal ~= nil and self.modalCloseTime == nil
end
