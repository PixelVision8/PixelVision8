ProgressModal = {}
ProgressModal.__index = ProgressModal
-- print("Modal loaded")
function ProgressModal:Init(title, editorUI)

  -- print("Create installer modal")

  local _progressModal = {} -- our new object
  setmetatable(_progressModal, ProgressModal) -- make Account handle lookup

  _progressModal.editorUI = editorUI
  _progressModal:Configure(title)

  return _progressModal

end

function ProgressModal:Configure(title)

  -- self.showCancel = showCancel or false

  -- Reset the modal so it redraws correctly when opened
  self.firstRun = nil

  local width = 96 + 32
  local height = 48 + 16 + 16


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

function ProgressModal:Open()

  if(self.firstRun == nil) then

    -- Draw the black background
    self.canvas:SetStroke({5}, 1, 1)
    self.canvas:SetPattern({0}, 1, 1)
    self.canvas:DrawSquare(0, 0, self.canvas.width - 1, self.canvas.height - 1, true)

    -- Draw the brown background
    self.canvas:SetStroke({12}, 1, 1)
    self.canvas:SetPattern({11}, 1, 1)
    self.canvas:DrawSquare(3, 9, self.canvas.width - 4, self.canvas.height - 4, true)

    local tmpX = (self.canvas.width - (#self.title * 4)) * .5

    self.canvas:DrawText(self.title:upper(), tmpX, 1, "small", 15, - 4)

    -- draw highlight stroke
    self.canvas:SetStroke({15}, 1, 1)
    self.canvas:DrawLine(3, 9, self.canvas.width - 5, 9)
    self.canvas:DrawLine(3, 9, 3, self.canvas.height - 5)

    self.buttons = {}

  end

  for i = 1, #self.buttons do
    self.editorUI:Invalidate(self.buttons[i])
  end

  -- self:UpdateMessage(message)

  self.canvas:DrawPixels(self.rect.x, self.rect.y, DrawMode.TilemapCache)

end

function ProgressModal:UpdateMessage(message, percent)

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

  startY = startY + 4 + 8
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

function ProgressModal:Close()
  --   -- print("Modal Close")
  --   -- if(self.onParentClose ~= nil) then
  --   --   self.onParentClose()
  --   -- end
end
--
function ProgressModal:Update(timeDelta)
  --   --
  --   -- if(self.completed == true) then
  --   --   self.editorUI:UpdateButton(self.backBtnData)
  --   -- end
  --
end
--
function ProgressModal:Draw()
  --
end

function OnExport(soundID, showWarning)

  soundID = soundID or currentID

  local workspacePath = NewWorkspacePath(rootDirectory).AppendDirectory("Wavs").AppendDirectory("Sounds")

  if(showWarning == true) then



    pixelVisionOS:ShowMessageModal("Export Sound", "Do you want to export this sound effect to '"..workspacePath.Path.."'?", 200, true,
      function()
        if(pixelVisionOS.messageModal.selectionValue == true) then
          gameEditor:ExportSFX(soundID, workspacePath)
        end

      end
    )
  else
    gameEditor:ExportSFX(soundID, workspacePath)
  end

end

function OnExportAll()

  local workspacePath = NewWorkspacePath(rootDirectory).AppendDirectory("Wavs").AppendDirectory("Sounds")

  pixelVisionOS:ShowMessageModal("Export All Sounds", "Do you want to export all of the sound effects to '"..workspacePath.Path.."'?", 200, true,
    function()
      if(pixelVisionOS.messageModal.selectionValue == true) then

        installing = true

        installingTime = 0
        installingDelay = .1
        installingCounter = 0
        installingTotal = totalSounds

        installRoot = rootPath

      end

    end
  )

end

function OnInstallNextStep()

  -- print("Next step")
  -- Look to see if the modal exists
  if(installingModal == nil) then

    -- Create the model
    installingModal = ProgressModal:Init("Exporting", editorUI)

    -- Open the modal
    pixelVisionOS:OpenModal(installingModal)

  end

  OnExport(installingCounter, false)

  installingCounter = installingCounter + 1

  local message = "Exporting sound effect "..string.lpad(tostring(installingCounter), string.len(tostring(installingTotal)), "0") .. " of " .. installingTotal .. ".\n\n\nDo not restart or shut down Pixel Vision 8."

  local percent = (installingCounter / installingTotal)

  installingModal:UpdateMessage(message, percent)

  if(installingCounter >= installingTotal) then
    installingDelay = .5
  end

end

function OnInstallComplete()

  installing = false

  pixelVisionOS:CloseModal()

end
