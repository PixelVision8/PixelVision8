
function SettingsTool:CreateShortcutPanel()

    self.shortcutFields = {
        pixelVisionOS:CreateInputField({x = 176, y = 200, w = 8}, "", "ScreenShot"),
        pixelVisionOS:CreateInputField({x = 200, y = 200, w = 8}, "", "Record"),
        pixelVisionOS:CreateInputField({x = 224, y = 200, w = 8}, "", "Restart"),
    }
    
    self.usedShortcutKeys = {}
    
    for i = 1, #self.shortcutFields do
        local field = self.shortcutFields[i]
        field.pattern = "number"
        field.type = field.toolTip
    
        -- Read the shortcut keys from the bios
        local keyValue = self:ConvertKeyCodeToChar(tonumber(ReadBiosData(field.type .. "Key", tostring(49 + i))))
    
        -- Test if the value is nill and set a default value
        if(keyValue == nil) then
            keyValue = self:ConvertKeyCodeToChar(49 + i) -- first value will be 50 which converts to key 2
        end

        editorUI:ChangeInputField(field, keyValue)
    
        -- Save used keys
        self.usedShortcutKeys[field.type] = keyValue
    
        -- Create a new tooltip
        field.toolTip = "Remap the " .. field.type .. " key."
    
        field.captureInput = function(targetData)
        
            local inputText = self:ValidateInput(self:OnCaptureKey(), targetData, self.usedShortcutKeys)
        
            -- Validate the input before returning it to the input field
            return editorUI:ValidateInputFieldText(targetData, inputText)
    
        end

        field.onAction = function(value)

            -- Save used keys
            self.usedShortcutKeys[field.type] = value

            -- Save the new key map value
            self:RemapKey(field.type .. "Key", self:ConvertKeyToKeyCode(value))
        
            -- Let the user know the key has been saved
            pixelVisionOS:DisplayMessage("Setting '"..value.."' as the shortcut key.")
        
            RefreshActionKeys()
    
        end
    
    end

    pixelVisionOS:RegisterUI({name = "UpdateShortcutPanel"}, "UpdateShortcutPanel", self)

end

function SettingsTool:UpdateShortcutPanel()

for i = 1, #self.shortcutFields do
    editorUI:UpdateInputField(self.shortcutFields[i])
end

end

function SettingsTool:ResetShortcuts()

    for i = 1, #self.shortcutFields do
        local field = self.shortcutFields[i]
        
        local keyValue = self:ConvertKeyCodeToChar(49 + i) -- first value will be 50 which converts to key 2
        
        editorUI:ChangeInputField(field, tonumber(keyValue))
    end

end