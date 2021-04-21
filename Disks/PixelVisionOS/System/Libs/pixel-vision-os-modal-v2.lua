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
function PixelVisionOS:OpenModal(modal, callBack, ...)

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
  self.activeModal:Open( ... )

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

  -- TODO  need to restore this on the next draw call, not immediately
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


function PixelVisionOS:CreateModalChrome(canvas, title, messageLines)

  -- Draw the black background
  canvas:SetStroke(5, 1)
  canvas:SetPattern({0}, 1, 1)
  canvas:DrawRectangle(0, 0, canvas.width, canvas.height, true)

  -- Draw the brown background
  canvas:SetStroke(12, 1)
  canvas:SetPattern({11}, 1, 1)
  canvas:DrawRectangle(3, 9, canvas.width - 6, canvas.height - 12, true)

  local tmpX = (canvas.width - (#title * 4)) * .5

  canvas:DrawText(title:upper(), tmpX, 1, "small", 15, - 4)

  -- draw highlight stroke
  canvas:SetStroke(15, 1)
  canvas:DrawLine(3, 9, canvas.width - 5, 9)
  canvas:DrawLine(3, 9, 3, canvas.height - 5)

  if(messageLines ~= nil) then
      local total = #messageLines
      local startX = 8
      local startY = 16

      -- We want to render the text from the bottom of the screen so we offset it and loop backwards.
      for i = 1, total do
      canvas:DrawText(messageLines[i], startX, (startY + ((i - 1) * 8)), "medium", 0, - 4)
      end
  end

end
