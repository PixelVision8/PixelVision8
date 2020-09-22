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

CollisionManager = {}
CollisionManager.__index = CollisionManager

function CollisionManager:Init()

  -- Create a new object for the instance and register it
  local manager = {}
  setmetatable(manager, CollisionManager)

  -- Get a reference to the sprite size on startup
  manager.spriteSize = SpriteSize()
  manager.currentFocus = nil

  -- Current state
  manager.active = nil
  manager.hovered = nil
  manager.mousePos = {x = -1, y = -1, c = -1, r = -1}
  manager.mouseDown = false
  manager.mouseReleased = false

  -- Past state
  manager.lastHovered = nil
  manager.lastActive = nil
  manager.lastMousePos = {x = -1, y = -1, c = -1, r = -1}
  manager.lastMouseDown = false

  manager.scrollPos = {x = 0, y = 0}
  manager.ignoreScrollPos = false

  -- Hold a string for a tool tip. Read by the editor ui component
  manager.toolTip = nil
  manager.overrideMessage = false

  manager.dragTargets = {}

  -- manager.focus = nil

  return manager

end

function CollisionManager:Update(timeDelta)

  -- Copy current values over to old state
  self.lastHovered = self.hovered
  self.lastActive = self.active
  self.lastMousePos = self.mousePos

  -- Update the current mouse position and button state
  local mousePointer = MousePosition()

  self.mousePos.x = mousePointer.x
  self.mousePos.y = mousePointer.y

  -- Before clearing hover, test to see if the mouse is still down
  self.mouseDown = MouseButton(0, InputState.Down)
  self.mouseReleased = MouseButton(0, InputState.Released)

  -- Calculate the current mouse column
  self.mousePos.c = math.floor(self.mousePos.x / self.spriteSize.x)

  -- Calculate the current mouse row
  self.mousePos.r = math.floor(self.mousePos.y / self.spriteSize.y)

  -- Save the current value of the mouse down for the next frame
  self.lastMouseDown = self.mouseDown

  if(self.currentDragSource ~= nil) then

    -- Check to see if the mouse was released
    if(self.mouseReleased) then

      -- Call the on end drag function
      if(self.currentDragSource.onEndDrag ~= nil) then
        self.currentDragSource.onEndDrag(self.currentDragSource)
      end

      -- If the mouse is still down, we need to handle the drag
    else

      -- Increase the drag delay
      self.dragTime = self.dragTime + timeDelta

      -- Set the drag value equal to true if the time is greater than the delay
      self.currentDragSource.dragging = self.dragTime > self.currentDragSource.dragDelay

    end

  end

end

function CollisionManager:MouseInRect(rect)

  -- Return false if there is not rect to test
  if(rect == nil) then
    return false
  end

  -- Test for collision in a rect {x,y,w,h}
  return self.mousePos.x >= rect.x and self.mousePos.y >= rect.y and
  self.mousePos.x < (rect.x + rect.w) and self.mousePos.y < (rect.y + rect.h)
end

function CollisionManager:StartDrag(source)

  -- If drag delay is set to -1, dragging is disabled
  if(source.dragDelay == -1) then
    return
  end

  self.currentDragSource = source
  self.dragTime = 0
end

function CollisionManager:EndDrag(source)

  if(source.dragging == true) then

    -- Look for drop targets
    for i = 1, #self.dragTargets do

      local dest = self.dragTargets[i]

      -- Look for a collision with the dest
      if(self:MouseInRect(dest.hitRect ~= nil and dest.hitRect or dest.rect)) then

        if(dest.onDropTarget ~= nil) then
          dest.onDropTarget(source, dest)
          break
        end

      end

    end

    if(self.currentDragSourceToolTip ~= nil) then
      -- Restore the previous tooltip
      source.toolTip = self.currentDragSourceToolTip
    end

    source.dragging = false

  end
  -- Clear drag state
  self.currentDragSourceToolTip = nil
  self.currentDragSource = nil
  self.dragTime = 0


end

function CollisionManager:EnableDragging(target, dragDelay, type)
  target.dragDelay = dragDelay or .5
  target.dragging = false

  target.onStartDrag = function(data)
    self:StartDrag(target)
  end

  target.onEndDrag = function(data)
    self:EndDrag(target)
  end

  self:RegisterDragTarget(target, type)

end

function CollisionManager:DisableDragging(target)

  target.onStartDrag = nil

  target.onEndDrag = nil

  self:RemoveDragTarget(target)

end

function CollisionManager:RegisterDragTarget(target, type)

  target.dropType = type

  table.insert(self.dragTargets, target)

end

function CollisionManager:RemoveDragTarget(target)
  -- TODO this needs to be tested
  for i = 1, #self.dragTargets do
    if(self.dragTargets[i].name == target.name) then
      -- print("Remove", i, self.dragTargets[i].name)
      table.remove(self.dragTargets, i)
      return
    end
  end
end
