--
-- Copyright (c) 2017, Jesse Freeman. All rights reserved.
--
-- Licensed under the Microsoft Public License (MS-PL) License.
-- See LICENSE file in the project root for full license information.
--
-- Based on PixelVisionOS by Dave Yang https://github.com/daveyang/PixelVisionOS/blob/master/PixelVisionOS.lua under MIT License
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

function PixelVisionOS:ResetUndoHistory()
  -- 2 stacks
  self.undos = {}
  self.redos = {}
end

-- push object to top of undo stack
function PixelVisionOS:AddUndoHistory(o)
  self.undos[#self.undos + 1] = o
  self.redos = {}
end

-- pop top-most object from undo stack and save it to redo stack
-- return the object, or nil if nothing to undo
function PixelVisionOS:DeleteUndoHistory()
  if #self.undos >= 1 then
    local o = self.undos[#self.undos]
    self.undos[#self.undos] = nil
    self.redos[#self.redos + 1] = o
    return o
  else
    return nil
  end
end

-- return whether the operation is undoable
function PixelVisionOS:IsUndoable()
  return #self.undos >= 1
end

-- return whether the operation is redoable
function PixelVisionOS:IsRedoable()
  return #self.redos >= 1
end

-- undo last operation at top of stack, execute Undo() if available
function PixelVisionOS:Undo()
  local o = self:DeleteUndoHistory()

  return o
end

-- redo last undo operation and save it to the undo stack
-- execute Redo() if available
function PixelVisionOS:Redo()
  if #self.redos >= 1 then
    local o = self.redos[#self.redos]
    self.redos[#self.redos] = nil
    self.undos[#self.undos + 1] = o

    return o
  else
    return nil
  end
end
