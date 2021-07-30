MessageModal = {}
MessageModal.__index = MessageModal

function MessageModal:Init(title, message, width, buttons, options, singleSelection)

  local _messageModal = {} -- our new object
  setmetatable(_messageModal, MessageModal) -- make Account handle lookup

  _messageModal:Configure(title, message, width, buttons, options, singleSelection)

  return _messageModal

end

function MessageModal:Configure(title, message, width, buttons, options, singleSelection)

  if(options ~= nil) then

    message = message .. "\n"

    local box = singleSelection == true and "* " or "# "

    for i = 1, #options do
        
        message = message .. "\n" .. box .. options[i]

    end

    message = message .. "\n\n"

  end

  if(buttons == nil) then
    buttons = 
    {
      {
        name = "modalokbutton",
        action = function(target)

          -- if(target.onParentClose ~= nil) then
            target.onParentClose()
          -- end
          
        end,
        key = Keys.Enter,
        size = NewPoint(32, 16),
        tooltip = "Press 'enter' to close",
      }
    }
  end

  -- Reset the modal so it redraws correctly when opened
  self.firstRun = true

  width = width or 96

  -- Need to calculate the height ahead of time
  local wrap = WordWrap(message, (width / 4) - 4)
  self.lines = SplitLines(wrap)
  
  height = #self.lines * 8 + 42

  -- Make sure width and height are on the grid
  width = math.floor(width / 8) * 8
  height = math.floor(height / 8) * 8

  -- Create the canvas for the modal background
  self.canvas = NewCanvas(width, height)

  local displaySize = Display()

  self.title = title or "Message Modal"

  self.rect = {
    x = math.floor(((displaySize.x - width) * .5) / 8) * 8,
    y = math.floor(((displaySize.y - height) * .5) / 8) * 8,
    w = width,
    h = height
  }

  local total = #self.lines
  local startX = 8
  local startY = 16

  self.buttonData = {}


  self.selectionGroupData = editorUI:CreateToggleGroup(true)

    self.optionGroupData = editorUI:CreateToggleGroup(false)
    self.optionGroupData.onAction = function(value)

      local selected  = self.optionGroupData.buttons[value].selected
      
      print("Click value", value, selected)
      
      if(Key(Keys.LeftShift) or Key(Keys.RightShift)) then

        for i = 1, #self.optionGroupData.buttons do
            
          editorUI:ToggleButton(self.optionGroupData.buttons[i], value, false)

        end

      end

    end

    self.buttons = {}

    -- We want to render the text from the bottom of the screen so we offset it and loop backwards.
    for i = 1, total do

      local newY = (startY + ((i - 1) * 8))

      local firstChar = string.sub(self.lines[i], 0, 1)

      -- # are check boxes
      if(firstChar == "#") then
        editorUI:ToggleGroupButton(self.optionGroupData, {x = startX + self.rect.x, y = newY + self.rect.y, w = 8, h = 8}, "checkbox", "Select" .. string.sub(self.lines[i], 3), true)

      -- * are radio buttons
      elseif(firstChar == "*") then
        editorUI:ToggleGroupButton(self.selectionGroupData, {x = startX + self.rect.x, y = newY + self.rect.y, w = 8, h = 8}, "radiobutton", "Select" .. string.sub(self.lines[i], 3), true)
      end

      self.canvas:DrawText(self.lines[i]:upper(), startX, newY, "medium", 0, - 4)
    
    end


  local tmpButton = nil

  local total = #buttons

  local bX = self.rect.w
  local btnPadding = 8
  
  -- If total is 1, just center the button
  if(total == 1) then

    local width =  buttons.size == nil and 32 or buttons[1].size.x

    bX = ((bX - width) * .5) + (width + btnPadding)

  end

  for i = 1, total do
    
    -- Get a reference to the button properties
    tmpButton = buttons[i]
  
    -- Make sure there is a default button size
    if(tmpButton.size == nil) then
      tmpButton.size = {x = 32, y = 16}
    end

    bX = bX - (tmpButton.size.x + btnPadding)
    
    -- Fix the button to the bottom of the window
    local bY = math.floor(((self.rect.y + self.rect.h) - tmpButton.size.y - 8) / 8) * 8

    local tmpBtnData = editorUI:CreateButton({x = bX + self.rect.x, y = bY}, tmpButton.name, tmpButton.tooltip or "")

    if(tmpButton.key ~= nil and tmpButton.action ~= nil) then

      tmpBtnData.key = tmpButton.key
      tmpBtnData.onAction = function() buttons[i].action(self) end

    end

    table.insert(self.buttonData, tmpBtnData)

  end

  self.selectionValue = false

end

function MessageModal:Open()

  if(self.firstRun == true) then

    pixelVisionOS:CreateModalChrome(self.canvas, self.title, self.lines)

    self.firstRun = false;

  end

  for i = 1, #self.buttonData do
    editorUI:Invalidate(self.buttonData[i])
  end

  self.canvas:DrawPixels(self.rect.x, self.rect.y, DrawMode.TilemapCache)

end

function MessageModal:Update(timeDelta)

  local tmpBtn = nil

  for i = 1, #self.buttonData do

    tmpBtn = self.buttonData[i]
    
    editorUI:UpdateButton(tmpBtn)

    if(tmpBtn.key ~= nil and tmpBtn.onAction ~= nil and Key(tmpBtn.key, InputState.Released) == true ) then
        tmpBtn.onAction(self)
    end

  end

  editorUI:UpdateToggleGroup(self.optionGroupData)
  editorUI:UpdateToggleGroup(self.selectionGroupData)

end
