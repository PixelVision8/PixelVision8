function EditorUI:CreateChipPicker(rect, itemWidth, itemHeight, total, spriteName, toolTip)

  -- Create the button's default data
  local data = self:CreateData(rect, spriteName, toolTip, forceDraw)

  data.isDragging = false
  data.buttonOffset = 0

  data.topButton = self:CreateButton({x = data.rect.x, y = data.rect.y}, "selectionpaneltopbutton", "This is the picker back button.")

  data.topButton.onAction = function() self:OnChipPickerBack(data) end


  data.bottomButton = self:CreateButton({x = data.rect.x, y = data.rect.y + data.rect.h - 16}, "selectionpanelbottombutton", "This is the picker back button.")

  data.bottomButton.onAction = function() self:OnChipPickerNext(data) end

  local button1 = self:CreateButton({x = data.rect.x + 8, y = data.rect.y + 16}, "chippv8gpuicon", "Click to edit the cart chip.")

  local button2 = self:CreateButton({x = data.rect.x + 8, y = data.rect.y + 16 + 32}, "chipfamigpuicon", "Click to edit the cart chip.")

  local button3 = self:CreateButton({x = data.rect.x + 8, y = data.rect.y + 16 + 64}, "chipmastrgpuicon", "Click to edit the cart chip.")

  local button4 = self:CreateButton({x = data.rect.x + 8, y = data.rect.y + 16 + 96}, "chipgboygpuicon", "Click to edit the cart chip.")

  data.buttons = { button1, button2, button3, button4}

  for i = 1, #data.buttons do

    data.buttons[i].onPress = function()
      self:ChipPickerButtonPress(data, i)
    end

    data.buttons[i].onAction = function()
      self:ChipPickerButtonRelease(data, i)
    end

  end

  return data

end

function EditorUI:ConfigureChipPicker(data, options)
-- print("Configure Picker")
  -- if(data.lastType ~= type) then
    -- Reset offset
    data.buttonOffset = 0

    data.options = options

    self:RedrawChipPickerButtons(data)

    data.lastType = type

  -- end

end

function EditorUI:RedrawChipPickerButtons(data)

  -- Clear background first

  DrawRect(8, 32, 32, 128, BackgroundColor(), DrawMode.TilemapCache)

  local offset = data.buttonOffset

  for i = 1, #data.buttons do

    local button = data.buttons[i]
    button.spriteName = data.options[i + offset]["spriteName"] .. "icon"
    button.toolTip = "Drag the " .. data.options[i + offset]["name"] .. " " .. data.options[i + offset]["type"] .. " chip to the empty socket."
    self:RebuildMetaSpriteCache(button)

  end

  self:UpdateChipPickerNavButtons(data)

end

function EditorUI:UpdateChipPickerNavButtons(data)

  if(#data.options <= #data.buttons) then

    self:Enable(data.topButton, false)
    self:Enable(data.bottomButton, false)

  else

    local offset = data.buttonOffset + #data.buttons

    -- TODO need to figure this out
    self:Enable(data.topButton, data.buttonOffset > 0)
    self:Enable(data.bottomButton, (data.buttonOffset + 4) < #data.options)

  end

end

function EditorUI:ChipPickerButtonPress(data, id)

  -- print("Button", data.name, id, "pressed")

  data.currentButtonDown = id

  -- data.dragTime = 0
  data.isDragging = true

  local button = data.buttons[id]

  DrawRect(button.rect.x, button.rect.y, button.rect.w, button.rect.h, BackgroundColor(), DrawMode.TilemapCache)

end

function EditorUI:ChipPickerButtonRelease(data, id)

  -- print("Button", data.name, id, "release")

  data.currentButtonDown = nil

  -- data.dragTime = 0
  data.isDragging = false

  self:Invalidate(data.buttons[id])


end

function EditorUI:ClearChipPickerSelection(data)

  if(data.currentButtonDown ~= nil) then
    self:Invalidate(data.buttons[data.currentButtonDown])

    data.currentButtonDown = nil
    data.isDragging = false

  end

end

function EditorUI:OpenChipPicker(data)

  data.open = true

  DrawMetaSprite(FindMetaSpriteId("selectionpanel"), data.rect.x, data.rect.y + 16, false, false, DrawMode.TilemapCache)

  self:Invalidate(data.topButton)
  self:Invalidate(data.bottomButton)

  for i = 1, #data.buttons do
    self:Invalidate(data.buttons[i])
  end

end

function EditorUI:CloseChipPicker(data)
  data.open = false
  DrawRect(data.rect.x + 1, data.rect.y, data.rect.w - 1, data.rect.h + 16, BackgroundColor(), DrawMode.TilemapCache)
end

function EditorUI:UpdateChipPicker(data)

  if(data.open == true) then
    self:UpdateButton(data.topButton)
    self:UpdateButton(data.bottomButton)

    for i = 1, #data.buttons do
      self:UpdateButton(data.buttons[i])
    end

    if(data.isDragging and self.collisionManager.mouseDown == false) then
      data.isDragging = false
      self:Invalidate(data.buttons[data.currentButtonDown])
    end

  end

end

function EditorUI:DrawChipPicker(data)

  if(data.isDragging) then

    local button = data.buttons[data.currentButtonDown]

    local spriteData = FindMetaSpriteId(button.spriteName .. "up")

    -- if(spriteData ~= nil) then

      DrawMetaSprite(spriteData, self.collisionManager.mousePos.x - 16, self.collisionManager.mousePos.y - 16, false, false, DrawMode.UI)

    -- end
  end

end

function EditorUI:OnChipPickerBack(data)

  data.buttonOffset = data.buttonOffset - 1

  self:RedrawChipPickerButtons(data)

end

function EditorUI:OnChipPickerNext(data)

  data.buttonOffset = data.buttonOffset + 1

  self:RedrawChipPickerButtons(data)

end


function EditorUI:ChipPickerSelectedData(data)

  -- local offset = data.offset
  local index = data.currentButtonDown
  return data.options[data.currentButtonDown + data.buttonOffset]

end
