MessageModal = {}
MessageModal.__index = MessageModal

function MessageModal:Init(title, message, width, showCancel, okLabel, cancelLabel)

  local _messageModal = {} -- our new object
  setmetatable(_messageModal, MessageModal) -- make Account handle lookup

  _messageModal:Configure(title, message, width, showCancel, okLabel, cancelLabel)

  return _messageModal

end

function MessageModal:Configure(title, message, width, showCancel, okLabel, cancelLabel)
  self.showCancel = showCancel or false

  -- Reset the modal so it redraws correctly when opened
  self.firstRun = nil

  width = width or 96

  -- Need to calculate the height ahead of time
  -- Draw message text
  local wrap = WordWrap(message, (width / 4) - 4)
  self.lines = SplitLines(wrap)

  height = #self.lines * 8 + 42

  -- Make sure width and height are on the grid
  width = math.floor(width / 8) * 8
  height = math.floor(height / 8) * 8

  local displaySize = Display()

  self.title = title or "Message Modal"

  self.rect = NewRect(
    math.floor(((displaySize.x - width) * .5) / 8) * 8,
    math.floor(((displaySize.y - height) * .5) / 8) * 8,
    width,
    height
  )

  self.selectionValue = false

  self.okLabel = okLabel or " OK "

  self.cancelLabel = cancelLabel or " CANCLE "

end

function MessageModal:Open(pixelVisionOS)

  if(self.firstRun == nil) then

    -- Get references to the editor UI
    self.editorUI = pixelVisionOS.editorUI

    self.canvas = pixelVisionOS:GenerateModalCanvas(self.rect, self.title)

    local theme = self.editorUI.theme
    local total = #self.lines
    local startX = 8
    local startY = 16

    -- Customize the text button to use the pop up background
    ReplaceColor( theme.textButton.background, theme.modal.background )

    -- We want to render the text from the bottom of the screen so we offset it and loop backwards.
    for i = 1, total do
      self.canvas:DrawText(self.lines[i]:upper(), startX, (startY + ((i - 1) * 8)), theme.modal.messageFont, theme.modal.messageColor, theme.modal.messageSpacing)
    end

    self.buttons = {}

    local buttonSize = {x = 32, y = 16}

    -- Fix the button to the bottom of the window
    local bY = math.floor(((self.rect.y + self.rect.height) - buttonSize.y - 8) / 8) * 8

    local backBtnData = self.editorUI:CreateTextButton({x = 0, y = bY}, self.okLabel, "")

    backBtnData.onAction = function()

      -- Set value to true when ok is pressed
      self.selectionValue = true

      if(self.onParentClose ~= nil) then
        self.onParentClose()
      end
    end

    table.insert(self.buttons, backBtnData)

    if(self.showCancel) then

      -- Offset the bX value and snap to the grid
      -- bX = math.floor((bX - buttonSize.x - 8) / 8) * 8

      local cancelBtnData = self.editorUI:CreateTextButton({x = 0, y = bY}, self.cancelLabel, "")

      cancelBtnData.onAction = function()

        -- Set value to true when cancel is pressed
        self.selectionValue = false

        -- Close the panel
        if(self.onParentClose ~= nil) then
          self.onParentClose()
        end
      end

      table.insert(self.buttons, cancelBtnData)

    end

    local nextX = self.rect.x + self.rect.width - 8

    local totalButtons = #self.buttons

    -- If there is one button, center it
    if(totalButtons == 1) then
      local tmpButton = self.buttons[1]
      nextX = self.rect.x + ((self.rect.width - tmpButton.rect.width) * .5)

      -- snap the x value to the grid
      tmpButton.rect.x = math.floor(nextX / 8) * 8
    else

      -- Lay out buttons
      for i = 1, #self.buttons do
        local tmpButton = self.buttons[i]
        nextX = math.floor((nextX - tmpButton.rect.width - 8) / 8) * 8
        tmpButton.rect.x = nextX
      end
    end
    self.firstRun = false;

  end

  for i = 1, #self.buttons do
    self.editorUI:Invalidate(self.buttons[i])
  end

  self.canvas:DrawPixels(self.rect.x, self.rect.y, DrawMode.TilemapCache)

end

function MessageModal:Update(timeDelta)

  for i = 1, #self.buttons do
    self.editorUI:UpdateButton(self.buttons[i])
  end

  if(Key(Keys.Enter, InputState.Released)) then
    self.selectionValue = true
    self.onParentClose()
  elseif(Key(Keys.Escape, InputState.Released) and self.showCancel) then
    self.selectionValue = false
    self.onParentClose()
  end

end
