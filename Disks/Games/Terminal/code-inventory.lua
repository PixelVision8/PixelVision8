--[[
  Pixel Vision 8 - New Template Script
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at https://www.gitbook.com/@pixelvision8
]]--

function Take(options, state)

  -- Look to see if the current room has an item
  if(state.currentRoom.item ~= nil) then

    local itemName = options[1] -- TODO this should probably combine multiple names

    if(state.currentRoom.item.name == itemName) then
      -- Add the item to your state.inventory
      table.insert(state.inventory, state.currentRoom.item)

      -- Remove the item from the room
      state.currentRoom.item = nil

      state.DisplayText(state.inventory[#state.inventory].take)

      return

    end

  end

  state.DisplayMessage("You can't take that.")

end

function ItemInInventory(itemName, state)

  for i = 1, #state.inventory do
    if(state.inventory[i].name == itemName) then
      return state.inventory[i]
    end
  end

  return nil

end

function RemoveItemFromInventory(itemName, state)

  for i = 1, #state.inventory do
    if(state.inventory[i].name == itemName) then
      table.remove(state.inventory, i)
    end
  end

end

function Examine(options, state)

  local itemName = options[1] -- TODO this should probably combine multiple names

  local targetItem = nil

  if(state.currentRoom.item ~= nil and state.currentRoom.item.name == itemName) then

    targetItem = state.currentRoom.item

  else

    -- Go through the state.inventory
    for i = 1, #state.inventory do
      if(state.inventory[i].name == itemName) then
        targetItem = state.inventory[i]
      end
    end

  end

  if(targetItem ~= nil) then

    state.DisplayText(targetItem.examine)

    return
  end

  state.DisplayMessage("You can't examine that.")

end

function Use(options, state)

  -- if(gameActive == true and #options > 0 and state.mapOpen == false) then

  local itemName = options[1] -- TODO this should probably combine multiple names

  -- local hasRequiredItem = true

  for i = 1, #state.inventory do
    if(state.inventory[i].name == itemName) then
      local targetItem = state.inventory[i]

      if(targetItem.needs ~= nil and ItemInInventory(targetItem.needs, state) == nil) then
        -- Clear the target item since we can't use it

        state.DisplayMessage("You need ".. targetItem.needs .. " to use this.")

        return

      else
        if(targetItem.use ~= nil) then
          local useMessage = targetItem.use(state)

          state.DisplayText(useMessage)


        else
          state.DisplayMessage("This item can't be used.")
        end

        return

      end



    end
  end

  -- end

  -- if(hasRequiredItem == false) then
  --
  -- else
  state.DisplayMessage("You can't use that.")
  -- end


end
