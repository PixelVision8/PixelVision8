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

function EditorUI:CreateToggleButton(rect, spriteName, toolTip, forceDraw)

  -- Use the same data as the button
  local data = self:CreateButton(rect, spriteName, toolTip, forceDraw)

  data.name = "Toggle" .. data.name

  -- Add the selected property to make this a toggle button
  data.selected = false

  data.onClick = function(tmpData)

    -- Only trigger the click action when the last pressed button name matches
    if(self.inFocusUI == tmpData) then
      self:ToggleButton(tmpData)
    end

  end

  return data

end

function EditorUI:ToggleButton(data, value, callAction)

  if(value == nil) then
    value = not data.selected
  end

  -- invert the selected value
  data.selected = value

  -- force the button to redraw itself
  data.invalid = true

  -- Call the button data's onAction method and pass the current selected state
  if(data.onAction ~= nil and callAction ~= false)then
    data.onAction(data.selected)
  end

end

function EditorUI:CreateToggleGroup(singleSelection)

  singleSelection = singleSelection == nil and true or singleSelection

  local data = self:CreateData()

  data.buttons = {}
  data.currentSelection = 0
  data.onAction = nil
  data.invalid = false
  data.hovered = 0
  data.singleSelection = singleSelection

  return data

end

-- Helper method that created a toggle button and adds it to the group
function EditorUI:ToggleGroupButton(data, rect, spriteName, toolTip, forceDraw)

  -- Create a new toggle group button
  local buttonData = self:CreateToggleButton(rect, spriteName, toolTip, forceDraw)

  -- Add the new button to the toggle group
  self:ToggleGroupAddButton(data, buttonData)

  -- Return the button data
  return buttonData

end

function EditorUI:ToggleGroupAddButton(data, buttonData, id)

  -- When adding a new button, force it to redraw
  --data.invalid = forceDraw or true

  -- Modify the hit rect to the new rect position
  buttonData.hitRect = {x = buttonData.rect.x, y = buttonData.rect.y, w = buttonData.rect.w, h = buttonData.rect.h}

  -- TODO need to replace with table insert
  -- Need to figure out where to put the button, if no id exists, find the last position in the buttons table
  id = id or #data.buttons + 1

  -- save the button data
  table.insert(data.buttons, id, buttonData)

  -- Attach a new onAction to the button so it works within the group
  buttonData.onAction = function()

    self:SelectToggleButton(data, id)

  end

  -- Invalidate the button so it redraws
  self:Invalidate(buttonData)

end

function EditorUI:ToggleGroupRemoveButton(data, id)

  if(data.currentSelection == id) then
    data.currentSelection = 0
  end

  table.remove(data.buttons, id)

  data.invalid = true

end

function EditorUI:UpdateToggleGroup(data)

  -- Exit the update if there is no is no data
  if(data == nil) then
    return
  end

  -- Set data for the total number of buttons for the loop
  local total = #data.buttons
  local btn = nil

  -- Loop through each of the buttons and update them
  for i = 1, total do

    btn = data.buttons[i]

    self:UpdateButton(btn)

  end

end

function EditorUI:SelectToggleButton(data, id, trigger)
  -- TODO need to make sure we handle multiple selections vs one at a time
  -- Get the new button to select
  local buttonData = data.buttons[id]

  -- Make sure there is button data and the button is not disabled
  if(buttonData == nil or buttonData.enabled == false)then
    return
  end

  if(id == buttonData.selected) then
    return
  end

  if(data.singleSelection == true) then
    -- Make sure that the button is selected before we disable it
    buttonData.selected = true
    self:Enable(buttonData, false)

  end

  -- Now it's time to restore the last button.
  if(data.currentSelection > 0) then

    -- Get the old button data
    buttonData = data.buttons[data.currentSelection]

    -- Make sure there is button data first, incase there wasn't a previous selection
    if(buttonData ~= nil) then

      if(data.singleSelection == true) then
        -- Reset the button's selection value to the group's disable selection value
        buttonData.selected = false

        -- Enable the button since it is no longer selected
        self:Enable(buttonData, true)

      end

    end

  end

  -- Set the current selection ID
  data.currentSelection = id

  -- Trigger the action for the selection
  if(data.onAction ~= nil and trigger ~= false) then
    data.onAction(id, buttonData.selected)
  end

end

function EditorUI:ToggleGroupCurrentSelection(data)
  return data.buttons[data.currentSelection]
end

-- TODO is anything using this?
function EditorUI:ToggleGroupSelections(data)
  local selections = {}
  local total = #data.buttons
  local buttonData = nil
  for i = 1, total do
    buttonData = data.buttons[i]
    if(buttonData ~= nil and buttonData.selected == true)then
      selections[#selections + 1] = i
    end
  end

  return selections
end

function EditorUI:ClearGroupSelections(data)

  if(data == nil) then
    return
  end

  local total = #data.buttons
  local buttonData = nil
  for i = 1, total do
    buttonData = data.buttons[i]
    if(buttonData ~= nil)then
      -- TODO this will accidentally enable disabled buttons. Need to check that they are selected and disabled
      if(buttonData.selected and buttonData.enabled == false) then
        self:Enable(buttonData, true)
      end
      buttonData.selected = false
      buttonData.invalid = true
    end
  end

  data.currentSelection = 0
  data.invalid = true
end

function EditorUI:ClearToggleGroup(data)

  -- Loop through all of the buttons and clear them
  local total = #data.buttons

  for i = 1, total do
    self:ClearButton(data.buttons[i])
  end

  -- TODO need to remove existing buttons from the tilemap
  self:ClearGroupSelections(data)

  data.buttons = {}
end
