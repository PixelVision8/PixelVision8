ToolPickerModal = {}
ToolPickerModal.__index = ToolPickerModal

function ToolPickerModal:Init(pos, buttons)

  local _toolPickerModal = {}
  setmetatable(_toolPickerModal, ToolPickerModal)

  _toolPickerModal:Configure(pos, buttons)

  return _toolPickerModal

end

function ToolPickerModal:Configure(pos, buttons)


    self.selection = -1

--   if(buttons == nil) then
--     buttons = 
--     {
--       {
--         name = "pointer",
--         tooltip = "Press 'enter' to close",
--       },
--       {
--         name = "line",
--         tooltip = "Press 'enter' to close",
--       }
--     }
--   end

  -- Reset the modal so it redraws correctly when opened
  self.firstRun = true

  self.buttonData = {}

  local tmpButton = nil

  local total = #buttons

  local bX = pos.X
  local bY = pos.Y

  for i = 1, total do

    -- Get a reference to the button properties
    tmpButton = buttons[i]

    bY = bY + 16

    local tmpBtnData = editorUI:CreateButton({x = bX, y = bY}, tmpButton.name, tmpButton.tooltip or "")

    tmpBtnData.hitRect = {x = tmpBtnData.rect.x, y = tmpBtnData.rect.y, w = 16, h = 16}

    -- if(tmpButton.key ~= nil and tmpButton.action ~= nil) then

    --   tmpBtnData.key = tmpButton.key
      tmpBtnData.onPress = function() self.selection = tmpBtnData.spriteName end

    -- end

    table.insert(self.buttonData, tmpBtnData)

  end

end

function ToolPickerModal:Open()

  if(self.firstRun == true) then

    self.firstRun = false;

  end

  for i = 1, #self.buttonData do
    editorUI:Invalidate(self.buttonData[i])
  end

end

function ToolPickerModal:Update(timeDelta)

  local tmpBtn = nil

  for i = 1, #self.buttonData do

    tmpBtn = self.buttonData[i]
    
    editorUI:UpdateButton(tmpBtn)

  end

  if(MouseButton( 0, InputState.Released )) then
    self.onParentClose()
  end

end
