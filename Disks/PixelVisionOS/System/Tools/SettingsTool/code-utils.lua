-- Converts a key code into a char character
function SettingsTool:ConvertKeyCodeToChar(keyCode)

  local value = -1

  for i = 1, #KeyCodeMap do
    if(KeyCodeMap[i].keyCode == keyCode) then
      return KeyCodeMap[i].char
    end
  end

  return value

end

function SettingsTool:ConvertKeyToKeyCode(key)

  local keyCode = -1

  for i = 1, #KeyCodeMap do

    if(KeyCodeMap[i].char == key) then
      return KeyCodeMap[i].keyCode
    end

  end

  return keyCode

end

function SettingsTool:ValidateInput(key, field, useKeys)

  local key = self:OnCaptureKey()

  -- Check to see if the key is not empty
  if(key ~= "") then

    -- Look for duplicate keys
    if(self:CheckActionKeyDoups(key, useKeys) == true) then

      editorUI:EditInputArea(field, false)

      editorUI:ChangeInputField(field, field.previousValue, false)

      -- TODO this used to show the key but it would require adding sprites to small font
      pixelVisionOS:DisplayMessage("The key is already being used.", 2)

      return ""

    end

  end

  -- Return the new key to the input field
  return key
end

function SettingsTool:OnCaptureKey()

  -- Show blinking light for controller
  -- if(blinkActive) then
  --   self:DrawBlinkSprite()
  -- end

  local total = #KeyCodeMap

  for i = 1, total do

    -- TODO need to test to see if the keyCode is already assigned
    local key = KeyCodeMap[i]

    if(Key(key.keyCode)) then
      return key.char
    end

  end

  return ""

end

function SettingsTool:OnCaptureButton()

  -- Show blinking light for controller
--   if(blinkActive) then
--     DrawBlinkSprite()
--   end

  local total = #ButtonsCodes

  for i = 1, total do

    -- TODO need to test to see if the keyCode is already assigned
    local button = ButtonsCodes[i]

    if(Button(button.keyCode)) then
      return button.char
    end

  end

  return ""

end

function SettingsTool:ConvertButtonCodeToChar(keyCode)

  local value = -1

  for i = 1, #ButtonsCodes do
    if(ButtonsCodes[i].keyCode == keyCode) then
      return ButtonsCodes[i].char
    end
  end

  return value

end

function SettingsTool:ConvertButtonToKeyCode(key)

  local keyCode = -1

  for i = 1, #ButtonsCodes do

    if(ButtonsCodes[i].char == key) then
      return ButtonsCodes[i].keyCode
    end

  end

  return keyCode

end


function SettingsTool:CheckActionKeyDoups(key, keyMap)

  local value = false

  for k, v in pairs(keyMap) do

    if(key == v) then
      return true
    end

  end

  return value

end

function SettingsTool:RemapKey(keyName, keyCode)

  -- Make sure that the key code is valid
  if (keyCode == -1) then
    return false
  end

  -- print("Write Bios", keyName, tostring(keyCode))

  -- Save the new mapped key to the bios
  WriteBiosData(keyName, tostring(keyCode));


  -- print("Read Bios", ReadBiosData(keyName), self:ConvertKeyCodeToChar(ReadBiosData(keyName)))

  self.usedKeysInvalid = true

  self:GetUsedKeys()

  return true

end