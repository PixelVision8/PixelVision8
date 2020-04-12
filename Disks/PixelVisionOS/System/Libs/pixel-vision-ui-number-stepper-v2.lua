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

function EditorUI:CreateNumberStepper(rect, inputWidth, value, min, max, spriteNamePrefix, toolTip, colorOffset)

  spriteNamePrefix = spriteNamePrefix or ""

  local data = self:CreateData(rect, nil, toolTip, forceDraw)

  data.backButton = self:CreateButton({x = rect.x, y = rect.y}, spriteNamePrefix .. "stepperback", "Decrease the value.")

  data.backButton.onAction = function()
    self:DecreaseNumberStepper(data, true)
  end

  data.inputField = self:CreateInputField({x = rect.x + 16, y = rect.y + 8, w = inputWidth}, "", "Enter a value between ".. min .. " and " .. max .. ".", "number", nil, colorOffset)
  data.inputField.min = min
  data.inputField.max = max
  data.inputField.onAction = function(value)

    self:UpdateStepperButtons(data, tonumber(data.inputField.text), data.inputField.min, data.inputField.max)

    if(data.onInputAction ~= nil) then
      data.onInputAction(value)
    end

  end

  data.nextButton = self:CreateButton({x = data.inputField.rect.x + data.inputField.rect.w, y = rect.y}, spriteNamePrefix .. "steppernext", "Increase the value.")
  data.nextButton.onAction = function()
    self:IncreaseNumberStepper(data, true)
  end

  data.stepperValue = 1

  self:ChangeNumberStepperValue(data, value, false)

  return data

end

function EditorUI:UpdateStepper(data)

  self:UpdateInputField(data.inputField)
  self:UpdateButton(data.backButton)
  self:UpdateButton(data.nextButton)

end

function EditorUI:IncreaseNumberStepper(data)

  local nextID = tonumber(data.inputField.text) + data.stepperValue

  if(nextID > data.inputField.max) then
    nextID = data.inputField.max
  end

  self:ChangeNumberStepperValue(data, nextID, action)


end

function EditorUI:DecreaseNumberStepper(data, action)

  local nextID = tonumber(data.inputField.text) - data.stepperValue

  if(nextID < data.inputField.min) then
    nextID = data.inputField.min
  end

  self:ChangeNumberStepperValue(data, nextID, action)

end

function EditorUI:ChangeNumberStepperValue(data, value, action, updateButtons)

  self:ChangeInputField(data.inputField, tostring(value), action)

  if(updateButtons ~= false) then
    self:UpdateStepperButtons(data, tonumber(data.inputField.text), data.inputField.min, data.inputField.max)
  end

end

function EditorUI:GetNumberStepperValue(data)
  return tonumber(data.inputField.text)
end

function EditorUI:UpdateStepperButtons(data, value, min, max)

  if(data == nil or value == nil or data.enabled == false) then
    return
  end

  self:Enable(data.backButton, value > min)
  self:Enable(data.nextButton, value < max)

end

function EditorUI:EnableStepper(data, value)

  self:Enable(data, value)
  self:Enable(data.backButton, value)
  self:Enable(data.inputField, value)
  self:Enable(data.nextButton, value)

end

function EditorUI:RefreshNumberStepper(data)

  self:UpdateStepperButtons(data, tonumber(data.inputField.text), data.inputField.min, data.inputField.max)

end
