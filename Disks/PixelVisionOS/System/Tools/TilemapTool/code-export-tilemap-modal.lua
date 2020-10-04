--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

ExportTilemapModal = {}
ExportTilemapModal.__index = ExportTilemapModal

function ExportTilemapModal:Init(title, message, width, showCancel)

  local _exportTilemapModal = {} -- our new object
  setmetatable(_exportTilemapModal, ExportTilemapModal) -- make Account handle lookup

  _exportTilemapModal:Configure(title, message, width, showCancel)

  return _exportTilemapModal

end

function ExportTilemapModal:Configure(title, message, width, showCancel, okButtonSpriteName)

  self.showCancel = showCancel or false

  self.okButtonSpriteName = okButtonSpriteName or "ok"
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

function ExportTilemapModal:Open()

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

    local total = #self.lines
    local startX = 8
    local startY = 16
  
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

    local buttonSize = {x = 32, y = 16}

    -- TODO center ok button when no cancel button is shown
    local bX = self.showCancel == true and (self.rect.w - buttonSize.x - 8) or ((self.rect.w - buttonSize.x) * .5)

    -- snap the x value to the grid
    bX = math.floor((bX + self.rect.x) / 8) * 8

    -- Fix the button to the bottom of the window
    local bY = math.floor(((self.rect.y + self.rect.h) - buttonSize.y - 8) / 8) * 8

    local backBtnData = editorUI:CreateButton({x = bX, y = bY}, "modal".. self.okButtonSpriteName .. "button", "")

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
      bX = math.floor((bX - buttonSize.x - 8) / 8) * 8

      local cancelBtnData = editorUI:CreateButton({x = bX, y = bY}, "modalcancelbutton", "")

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

    self.firstRun = false;

  end

  for i = 1, #self.buttons do
    editorUI:Invalidate(self.buttons[i])
  end

  self.canvas:DrawPixels(self.rect.x, self.rect.y, DrawMode.TilemapCache)

end

function ExportTilemapModal:Update(timeDelta)

  for i = 1, #self.buttons do
    editorUI:UpdateButton(self.buttons[i])
  end

  editorUI:UpdateToggleGroup(self.optionGroupData)
  editorUI:UpdateToggleGroup(self.selectionGroupData)
  
  if(Key(Keys.Enter, InputState.Released)) then
    self.selectionValue = true
    self.onParentClose()
  elseif(Key(Keys.Escape, InputState.Released) and self.showCancel) then
    self.selectionValue = false
    self.onParentClose()
  end

end
