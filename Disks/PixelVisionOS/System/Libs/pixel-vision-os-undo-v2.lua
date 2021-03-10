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

function PixelVisionOS:ResetUndoHistory(data)

  
  data.undoStack = {} -- Keep a stack of undo info, each one is {self, state}
  data.redoStack = {} -- Keep a stack of redo info, each one is {data, state}

  -- Add Callbacks
  data.OnUndo = function (targetData)

    pixelVisionOS:Undo(targetData)

    self:UpdateHistoryButtons(targetData)
    
  end

  data.OnRedo = function (targetData)

    pixelVisionOS:Redo(targetData)
  
    self:UpdateHistoryButtons(targetData)
  
  end

end

-- return whether the operation is undoable
function PixelVisionOS:IsUndoable(data)
  return #data.undoStack > 0
end

-- return whether the operation is redoable
function PixelVisionOS:IsRedoable(data)
  return #data.redoStack > 0
end


-- Call :BeginUndoable(data) right before doing any modification to the
-- text in the editor. It will capture the current state of the editor's
-- contents (data) and the state of the cursor, selection, etc. (state)
-- so it can be restored later.
-- NOTE: Make sure to balance each call to :BeginUndoable(data) with a call
-- to :EndUndoable(data). They can nest fine, just don't forget one.
function PixelVisionOS:BeginUndoable(data)

  if data.currentUndo then
    -- we have already stashed the data & state, just track how deep we are
    data.currentUndo.count = data.currentUndo.count + 1
  else
    -- make a new in-progress undo
    data.currentUndo = {
      count = 1, -- here is where we track nested begin/endUndoable calls
      state = data:GetState()
    }
  end
end

-- Call :EndUndoable(data) after each modification to the text in the editor.
function PixelVisionOS:EndUndoable(data)
  
  -- We might be inside several nested calls to begin/endUndoable
  data.currentUndo.count = data.currentUndo.count - 1
  -- If this was the last of the nesting
  if data.currentUndo.count == 0 then
    -- then push the undo onto the undo stack.
    table.insert(data.undoStack, data.currentUndo.state)
    -- clear the redo stack
    data.redoStack = {}
    data.currentUndo = nil
  end
end

-- Perform an undo. This will pop one entry off the undo
-- stack and restore the editor's contents & cursor state.
function PixelVisionOS:Undo(data)

  if #data.undoStack == 0 then
    -- beep?
    return
  end
  -- pull one entry from the undo stack
  local state = table.remove(data.undoStack)

  -- push a new entry onto the redo stack
  table.insert(data.redoStack, data:GetState())

  -- restore the cursor state
  self:SetState(state)

end

-- Perform a redo. This will pop one entry off the redo
-- stack and restore the editor's contents & cursor state.
function PixelVisionOS:Redo(data)
  if #data.redoStack == 0 then
    -- beep?
    return
  end
  -- pull one entry from the redo stack
  local state = table.remove(data.redoStack)
  -- push a new entry onto the undo stack
  table.insert(data.undoStack, data:GetState())
  
  -- restore the cursor state
  self:SetState(state)
end

function PixelVisionOS:UpdateHistoryButtons(data)

  self:EnableMenuItem(data.UndoShortcut, self:IsUndoable(data))
  self:EnableMenuItem(data.RedoShortcut, self:IsRedoable(data))

end

function PixelVisionOS:BeginUndo(data)
  self:BeginUndoable(data)
end

function PixelVisionOS:EndUndo(data)
  self:EndUndoable(data)
  self:UpdateHistoryButtons(data)
end

function PixelVisionOS:SetState(state)

  if(state.Action == nil) then
      return
  end

  state:Action()

end



-- function PixelVisionOS:ResetUndoHistory()
--   -- 2 stacks
--   data.undoHistory = {}
--   data.redoHistory = {}
-- end

-- -- push object to top of undo stack
-- function PixelVisionOS:AddUndoHistory(o)
--   print("Add State", dump(o))
--   o.action = #data.undoHistory
--   table.insert(data.undoHistory, o)
--   -- data.undoHistory[#data.undoHistory + 1] = o

--   -- Clear Redo
--   data.redoHistory = {}
-- end

-- -- push object to top of undo stack
-- function PixelVisionOS:AddRedoHistory(o)

--   table.insert(data.redoHistory, o)
  
-- end

-- -- return whether the operation is undoable
-- function PixelVisionOS:IsUndoable()
--   return #data.undoHistory > 0
-- end

-- -- return whether the operation is redoable
-- function PixelVisionOS:IsRedoable()
--   print("undo", dump(data.undoHistory),"\n redo", dump(data.redoHistory))
--   return #data.redoHistory > 0
-- end

-- -- undo last operation at top of stack, execute Undo() if available
-- function PixelVisionOS:Undo()
--   if #data.undoHistory > 0 then
--     local o = data.undoHistory[#data.undoHistory]

--     table.insert(data.redoHistory, o)
--     table.remove(data.undoHistory, #data.undoHistory)
    
--     return o
--   else
--     return nil
--   end
-- end

-- -- redo last undo operation and save it to the undo stack
-- -- execute Redo() if available
-- function PixelVisionOS:Redo()
--   if #data.redoHistory > 0 then
--     local o = data.redoHistory[#data.redoHistory]

--     table.insert(data.undoHistory, o)
--     table.remove(data.redoHistory, #data.redoHistory)
--     -- data.redoHistory[#data.redoHistory] = nil
--     -- data.undoHistory[#data.undoHistory + 1] = o

--     return o
--   else
--     return nil
--   end
-- end
