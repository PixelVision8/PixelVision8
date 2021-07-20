ResizeModal = {}
ResizeModal.__index = ResizeModal

function ResizeModal:Init(title, message, width, buttons, imageWidth, imageHeight)

  local _resizeModal = {} -- our new object
  setmetatable(_resizeModal, ResizeModal) -- make Account handle lookup

  _resizeModal:Configure(title, message, width, buttons, imageWidth, imageHeight)

  return _resizeModal

end

function ResizeModal:Configure(title, message, width, buttons, imageWidth, imageHeight)


  self.imageWidth = math.ceil(imageWidth / 8)
  self.imageHeight = math.ceil(imageHeight / 8)

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
  local height = #self.lines * 8 + 42 + 8

  -- Make sure width and height are on the grid
  width = math.floor(width / 8) * 8
  height = math.floor(height / 8) * 8 + 24

  -- Create the canvas for the modal background
  self.canvas = NewCanvas(width, height)

  self.title = title or "Message Modal"

  self.rect = {
    x = math.floor(((Display().X - self.canvas.Width) * .5) / 8) * 8,
    y = math.floor(((Display().Y - self.canvas.Height) * .5) / 8) * 8,
    w = width,
    h = height
  }

  -- self.selectionValue = false

  self.buttonData = {}

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
    
    local tmpBtnData = editorUI:CreateButton({x = bX + self.rect.x, y = self.rect.y + self.rect.h - 24}, tmpButton.name, tmpButton.tooltip or "")

    if(tmpButton.key ~= nil and tmpButton.action ~= nil) then

      tmpBtnData.key = tmpButton.key
      tmpBtnData.onAction = function() buttons[i].action(self) end

    end

    table.insert(self.buttonData, tmpBtnData)

  end

end

function ResizeModal:Open()

  if(self.firstRun == true) then

    pixelVisionOS:CreateModalChrome(self.canvas, self.title, self.lines)

    local offset = #self.lines * 8 + 12
    self.canvas:DrawMetaSprite("resizemodal", 32, offset, false, false)

    -- TODO need to get the size of the image
    self.colInputData = editorUI:CreateInputField({x = self.rect.x + 40, y = self.rect.y + offset + 16, w = 24}, tostring(self.imageWidth), "Width of canvas in columns.", "number")
    
    self.colInputData.min = 1
    self.colInputData.max = 256 -- TODO should this be limited to 128 to help with performance?

    -- TODO need to get the size of the image
    self.rowInputData = editorUI:CreateInputField({x = self.rect.x + 88 , y = self.rect.y + offset + 16, w = 24}, tostring(self.imageHeight), "Height of canvas in rows.", "number")
    
    self.rowInputData.min = 1
    self.rowInputData.max = 256 -- TODO should this be limited to 128 to help with performance?

    self.firstRun = false;

  end

  for i = 1, #self.buttonData do
    editorUI:Invalidate(self.buttonData[i])
  end

  self.canvas:DrawPixels(self.rect.x, self.rect.y, DrawMode.TilemapCache)

end

function ResizeModal:Update(timeDelta)

  local tmpBtn = nil

  for i = 1, #self.buttonData do

    tmpBtn = self.buttonData[i]
    
    editorUI:UpdateButton(tmpBtn)
    editorUI:UpdateInputField(self.colInputData)
    editorUI:UpdateInputField(self.rowInputData)
    
    if(self.colInputData.editing == false and self.rowInputData.editing == false) then

      if(tmpBtn.key ~= nil and tmpBtn.onAction ~= nil and Key(tmpBtn.key, InputState.Released) == true ) then
          tmpBtn.onAction(self)
      end

    end

  end

end
