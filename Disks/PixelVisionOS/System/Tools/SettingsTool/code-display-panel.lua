
function SettingsTool:CreateDisplayPanel()

    self.scaleInputData = pixelVisionOS:CreateInputField({x = 112, y = 200, w = 8}, Scale(), "This changes the scale of the window when not in fullscreen.", "number")
    self.scaleInputData.min = 1
    self.scaleInputData.max = 4
    self.scaleInputData.onAction = function(value) self:OnChangeScale(value) end

    -- self.stretchCheckBoxData = editorUI:CreateToggleButton({x = 128, y = 216, w = 8, h = 8}, "checkbox", "Stretch the display to fit the window.")
    
    -- self.stretchCheckBoxData.onAction = function (value)
    --     StretchScreen(value)

    --     WriteBiosData("StretchScreen", value == true and "True" or "False")
    -- end
    
    -- editorUI:ToggleButton(self.stretchCheckBoxData, StretchScreen())

    self.cropCheckBoxData = editorUI:CreateToggleButton({x = 128, y = 200, w = 8, h = 8}, "checkbox", "Enable the window to crop.")
    self.cropCheckBoxData.onAction = function (value)
        CropScreen(value)

        WriteBiosData("CropScreen", value == true and "True" or "False")

    end
    
    editorUI:ToggleButton(self.cropCheckBoxData, CropScreen())

    self.fullScreenCheckBoxData = editorUI:CreateToggleButton({x = 128, y = 192, w = 8, h = 8}, "checkbox", "Toggle full screen mode.")
    self.fullScreenCheckBoxData.hitRect = {x = 131, y = 192, w = 8, h = 8}
    self.fullScreenCheckBoxData.onAction = function(value)
        Fullscreen(value)

        WriteBiosData("FullScreen", value == true and "True" or "False")

        editorUI:Enable(self.scaleInputData, not value)

        editorUI:Enable(self.cropCheckBoxData, not value)
        editorUI:ToggleButton(self.cropCheckBoxData, value == false and CropScreen() or false, false)

        -- editorUI:Enable(self.stretchCheckBoxData, value)
        -- editorUI:ToggleButton(self.stretchCheckBoxData, value == true and StretchScreen() or false, false)

    end
    
    editorUI:ToggleButton(self.fullScreenCheckBoxData, Fullscreen())
    
    
    self.crtToggleButton = editorUI:CreateToggleButton({x = 128, y = 208, w = 8, h = 8}, "checkbox", "Toggle the CRT effect.")
    
    self.crtToggleButton.onAction = function (value)
        EnableCRT(value)

        WriteBiosData("CRT", value == true and "True" or "False")
    end
    
    editorUI:ToggleButton(self.crtToggleButton, EnableCRT())

    pixelVisionOS:RegisterUI({name = "UpdateDisplayPanel"}, "UpdateDisplayPanel", self)

  end

  function SettingsTool:OnChangeScale(value)
  Scale(tonumber(value))

  WriteBiosData("Scale", value)
end

  function SettingsTool:UpdateDisplayPanel()

    editorUI:UpdateInputField(self.scaleInputData)
    -- Update toggle groups
    editorUI:UpdateButton(self.fullScreenCheckBoxData)
    editorUI:UpdateButton(self.cropCheckBoxData)
    -- editorUI:UpdateButton(self.stretchCheckBoxData)
    editorUI:UpdateButton(self.crtToggleButton)

  end
