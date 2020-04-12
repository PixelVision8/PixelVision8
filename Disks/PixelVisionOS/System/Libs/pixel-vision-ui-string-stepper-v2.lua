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

function EditorUI:CreateStringStepper(rect, inputWidth, value, options, spriteNamePrefix, toolTip)

  spriteNamePrefix = spriteNamePrefix or ""

  local data = self:CreateData(rect, nil, toolTip, forceDraw)

  data.stepperValue = 1

  data.options = options
  data.currentOption = 1


  data.backButton = self:CreateButton({x = rect.x, y = rect.y}, spriteNamePrefix .. "stepperback", "Decrease the value.")

  data.backButton.onAction = function()
    self:DecreaseStringStepper(data, true)
  end

  data.inputField = self:CreateInputField({x = rect.x + 16, y = rect.y + 8, w = inputWidth}, "", toolTip, "string")

  data.inputField.onAction = function(value)

    self:UpdateStepperButtons(data, data.currentOption, 0, #data.options)

    if(data.onInputAction ~= nil) then
      data.onInputAction(value)
    end

  end

  data.nextButton = self:CreateButton({x = data.inputField.rect.x + data.inputField.rect.w, y = rect.y}, spriteNamePrefix .. "steppernext", "Increase the value.")
  data.nextButton.onAction = function()
    self:IncreaseStringStepper(data, true)
  end


  data.inputField.onValidate = function (value)

    for i = 1, #data.options do

      if(data.options[i] == value) then

        data.currentOption = i

        return value
      end

    end

    return data.inputField.defaultValue

  end

  self:ChangeStringStepperValue(data, value, false)

  return data

end

function EditorUI:GetStringStepperValue(data)
  return data.inputField.text
end

function EditorUI:IncreaseStringStepper(data)

  local nextID = data.currentOption + data.stepperValue

  if(nextID > #data.options) then
    nextID = #data.options
  end

  self:ChangeStringStepperValue(data, data.options[nextID], action)


end

function EditorUI:DecreaseStringStepper(data, action)

  local nextID = data.currentOption - data.stepperValue

  if(nextID < 0) then
    nextID = 0
  end

  self:ChangeStringStepperValue(data, data.options[nextID], action)

end

function EditorUI:ChangeStringStepperValue(data, value, action, updateButtons)

  self:ChangeInputField(data.inputField, value, action)

  -- TODO need to find the value
  if(updateButtons ~= false) then
    self:UpdateStepperButtons(data, data.currentOption, 1, #data.options)
  end
end

function EditorUI:RefreshStringStepper(data)

  self:UpdateStepperButtons(data, data.currentOption, 1, #data.options)

end
