InstallerModal = {}
InstallerModal.__index = InstallerModal

function InstallerModal:Init(title, editorUI)

  local _installerModal = {} -- our new object
  setmetatable(_installerModal, InstallerModal) -- make Account handle lookup

  _installerModal.editorUI = editorUI
  _installerModal:Configure(title)

  return _installerModal

end

function InstallerModal:Configure(title)

  -- self.showCancel = showCancel or false

  -- Reset the modal so it redraws correctly when opened
  self.firstRun = nil

  local width = 96 + 32
  local height = 48 + 16


  -- Make sure width and height are on the grid
  width = math.floor(width / 8) * 8
  height = math.floor(height / 8) * 8

  self.canvas = NewCanvas(width, height)

  local displaySize = Display()

  self.title = title or "Message Modal"

  self.rect = {
    x = math.floor(((displaySize.x - width) * .5) / 8) * 8,
    y = math.floor(((displaySize.y - height) * .5) / 8) * 8,
    w = width,
    h = height
  }

  self.selectionValue = false

end

function InstallerModal:Open()

  if(self.firstRun == nil) then

    -- Draw the black background
    self.canvas:SetStroke(5, 1)
    self.canvas:SetPattern({0}, 1, 1)
    self.canvas:DrawSquare(0, 0, self.canvas.width - 1, self.canvas.height - 1, true)

    -- Draw the brown background
    self.canvas:SetStroke(12, 1)
    self.canvas:SetPattern({11}, 1, 1)
    self.canvas:DrawSquare(3, 9, self.canvas.width - 4, self.canvas.height - 4, true)

    local tmpX = (self.canvas.width - (#self.title * 4)) * .5

    self.canvas:DrawText(self.title:upper(), tmpX, 1, "small", 15, - 4)

    -- draw highlight stroke
    self.canvas:SetStroke(15, 1)
    self.canvas:DrawLine(3, 9, self.canvas.width - 5, 9)
    self.canvas:DrawLine(3, 9, 3, self.canvas.height - 5)

    self.buttons = {}

  end

  for i = 1, #self.buttons do
    self.editorUI:Invalidate(self.buttons[i])
  end

  -- self:UpdateMessage(message)

  self.canvas:DrawPixels(self.rect.x, self.rect.y, DrawMode.TilemapCache)

  self.editorUI.mouseCursor:SetCursor(5, true)

end

function InstallerModal:UpdateMessage(currentItem, total)

  local message = "Installing item "..string.lpad(tostring(currentItem), string.len(tostring(total)), "0") .. " of " .. installingTotal .. ".\n\n\nDo not restart or shut down Pixel Vision 8."

  local percent = (currentItem / total)

  -- Need to calculate the height ahead of time
  -- Draw message text
  local wrap = WordWrap(message, (self.rect.w / 4) - 4)
  self.lines = SplitLines(wrap)

  -- Draw message text
  local total = #self.lines
  local startX = 8
  local startY = 16

  -- Clear the canvas where we are going to redraw the text
  self.canvas:Clear(11, startX, startY, self.canvas.width - 16, (total - 1) * 8)

  -- We want to render the text from the bottom of the screen so we offset it and loop backwards.
  for i = 1, total do
    self.canvas:DrawText(self.lines[i]:upper(), startX, (startY + ((i - 1) * 8)), "medium", 0, - 4)
  end

  local width = self.canvas.width - 16

  startY = startY + 4
  -- Background
  self.canvas:Clear(0, startX, startY + 8, width, 8)

  if(percent > 1 ) then
    percent = 1
  end

  width = width * percent

  -- Progress
  self.canvas:Clear(6, startX + 1, startY + 9, width - 2, 6)


  self.canvas:DrawPixels(self.rect.x, self.rect.y, DrawMode.TilemapCache)

  -- self.editorUI:Invalidate(self.buttons[1])
end

function InstallerModal:Close()
  self.editorUI.mouseCursor:SetCursor(1, false)
end
--
function InstallerModal:Update(timeDelta)
  --   --
  --   -- if(self.completed == true) then
  --   --   self.editorUI:UpdateButton(self.backBtnData)
  --   -- end
  --
end
--
function InstallerModal:Draw()
  --
end
