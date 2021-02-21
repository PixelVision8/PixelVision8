NewFileModal = {}
NewFileModal.__index = NewFileModal

function NewFileModal:Init()

  local _renameModal = {} -- our new object
  setmetatable(_renameModal, NewFileModal) -- make Account handle lookup

  local width = 224
  local height = 72

  _renameModal.canvas = NewCanvas(width, height)

  local displaySize = Display()

  _renameModal.title = "Rename File"

  _renameModal.rect = {
    x = math.floor(((displaySize.x - width) * .5) / 8) * 8,
    y = math.floor(((displaySize.y - height) * .5) / 8) * 8,
    w = width,
    h = height
  }

  _renameModal.currentSelection = 1
  _renameModal.message = message

  _renameModal.editable = true


  return _renameModal

end

function NewFileModal:SetText(title, inputText, message, editable)

  self.editable = editable

  self.title = title
  -- self.message = description
  self.defaultText = inputText

  self.selectionValue = false

  local wrap = WordWrap(message, (self.rect.w / 4) - 4)
  self.lines = SplitLines(wrap)

end

function NewFileModal:GetText()
  return self.inputField.text
end

function NewFileModal:Open()

  self.keyDelay = .2
  self.keyTime = 0

  self.canvas:Clear()
  -- Save a snapshot of the TilemapCache

  -- Draw the black background
  self.canvas:SetStroke(0, 1)
  self.canvas:SetPattern({0}, 1, 1)
  self.canvas:DrawRectangle(0, 0, self.canvas.width, self.canvas.height, true)

  -- Draw the brown background
  self.canvas:SetStroke(12, 1)
  self.canvas:SetPattern({11}, 1, 1)
  self.canvas:DrawRectangle(2, 8, self.canvas.width - 4, self.canvas.height - 10, true)

  local tmpX = (self.canvas.width - (#self.title * 4)) * .5

  self.canvas:DrawText(self.title:upper(), tmpX, 0, "small", 15, - 4)

  self.canvas:SetStroke(15, 1)
  self.canvas:DrawLine(2, 8, self.canvas.width - 4, 8)
  self.canvas:DrawLine(2, 8, 2, self.canvas.height - 4)

  self.buttons = {}

  local backBtnData = self.editorUI:CreateButton({x = self.rect.x + 184, y = self.rect.y + 48}, "modalokbutton", "Accept the changes.")

  backBtnData.onAction = function()

    -- Set value to true when ok is pressed
    self.selectionValue = true

    if(self.onParentClose ~= nil) then
      self.onParentClose()
    end
  end

  local cancelBtnData = self.editorUI:CreateButton({x = self.rect.x + 144, y = self.rect.y + 48}, "modalcancelbutton", "Cancel renaming.")

  cancelBtnData.onAction = function()

    -- Set value to true when cancel is pressed
    self.selectionValue = false

    -- Close the panel
    if(self.onParentClose ~= nil) then
      self.onParentClose()
    end
  end

  table.insert(self.buttons, backBtnData)
  table.insert(self.buttons, cancelBtnData)

  -- local spriteData = renameinputfield

  -- self.canvas:DrawSprites(spriteData.spriteIDs, 8, 16 + 8, spriteData.width)
  self.canvas:DrawMetaSprite(FindMetaSpriteId("renameinputfield"), 8, 16 + 8)
  
  self.inputField = self.editorUI:CreateInputField({x = self.rect.x + 16, y = self.rect.y + 32, w = 192}, "Untitled", "Enter a new filename.", "file")

  local startX = 16
  local startY = 16

  local total = #self.lines

  -- We want to render the text from the bottom of the screen so we offset it and loop backwards.
  for i = 1, total do
    self.canvas:DrawText(self.lines[i]:upper(), startX, (startY + ((i - 1) * 8)), "medium", 0, - 4)
  end

  for i = 1, #self.buttons do
    self.editorUI:Invalidate(self.buttons[i])
  end

  self.canvas:DrawPixels(self.rect.x, self.rect.y, DrawMode.TilemapCache)

  if(self.inputField ~= nil) then
    self.editorUI:ChangeInputField(self.inputField, self.defaultText, false)

  end

  self.editorUI:Enable(self.inputField, self.editable)

end

function NewFileModal:ShutdownTextField()
  self.editorUI:EditInputArea(self.inputField, false)
  self.editorUI:ResetValidation(self.inputField)
end

function NewFileModal:Update(timeDelta)

  for i = 1, #self.buttons do
    self.editorUI:UpdateButton(self.buttons[i])
  end

  self.editorUI:UpdateInputField(self.inputField)

  if(self.inputField.editing == false) then
    -- print("Key Input")
    self.keyTime = self.keyTime + timeDelta

    if((self.keyTime > self.keyDelay)) then
      if(Key(Keys.Enter, InputState.Released)) then
        self.selectionValue = true
        self.onParentClose()
      elseif(Key(Keys.Escape, InputState.Released)) then
        self.selectionValue = false
        self.onParentClose()
      end
    end
  else
    self.keyTime = 0
  end

end
