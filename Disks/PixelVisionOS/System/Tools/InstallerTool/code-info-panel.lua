--[[
	Pixel Vision 8 - Debug Tool
	Copyright (C) 2016, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

--local self.maxAboutLines = 8
--local self.aboutLines = {}
local panelID = "InfoPanelUI"

function InstallerTool:CreateInfoPanel()
  print("CreateInfoPanel")
  self.maxAboutLines = 0
  --self.aboutLines = {}
  
  if(self.variables["readme"] ~= nil) then

    local wrap = WordWrap(self.variables["readme"], 52)
    self.aboutLines = SplitLines(wrap)

    print(dump(self.aboutLines))

    if(#self.aboutLines < self.maxAboutLines) then
      self.maxAboutLines = #self.aboutLines
    end

  end

  installerRoot = self.variables["dir"] or "Workspace"

  nameInputData = editorUI:CreateInputField({x = 48, y = 40, w = 152}, installerRoot, "Enter in a file name to this string input field.", "file")

  aboutSliderData = editorUI:CreateSlider({x = 227, y = 68, w = 16, h = 72}, "vsliderhandle", "Scroll to see more of the about text.")
  aboutSliderData.onAction = function() self:OnAboutValueChange() end

  editorUI:Enable(aboutSliderData, #self.aboutLines > self.maxAboutLines)

  installButtonData = editorUI:CreateButton({x = 208, y = 32}, "installbutton", "Run the installer.")
  installButtonData.onAction = function(value)

    -- Create  the install path without the last slash
    local installPath = "/Workspace" .. (nameInputData.text:lower() ~= "workspace" and "/"..nameInputData.text or "")

    pixelVisionOS:ShowMessageModal("Install Files", "Are you sure you want to install ".. #filesToCopy .." items in '".. installPath .."/'? This will overwrite any existing files and can not be undone.", 160, true,
      function()

        if(pixelVisionOS.messageModal.selectionValue == true) then

          self:OnInstall(installPath)

        end

      end
    )

  end

  cleanCheckboxData = editorUI:CreateToggleButton({x = 176, y = 56, w = 8, h = 8}, "radiobutton", "Toggles doing a clean build and removes all previous builds.")

  -- editorUI:ToggleButton(cleanCheckboxData, gameEditor:ReadMetadata("clear", "false") == "true", false)

  cleanCheckboxData.onAction = function(value)

    if(value == false) then
      -- InvalidateData()
      return
    end

    pixelVisionOS:ShowMessageModal("Warning", "Are you sure you want to do a clean install? The root directory will be removed before the installer copies over the files. This can not be undone.", 160, true,
      function()

        if(pixelVisionOS.messageModal.selectionValue == false) then

          -- Force the checkbox back into the false state
          editorUI:ToggleButton(cleanCheckboxData, false, false)

        else
          -- Force the checkbox back into the false state
          editorUI:ToggleButton(cleanCheckboxData, true, false)
        end

        -- Force the button to redraw since restoring the modal will show the old state
        editorUI:Invalidate(cleanCheckboxData)
        -- InvalidateData()
      end
    )

  end

  self:DrawAboutLines()

  pixelVisionOS:RegisterUI({name = panelID}, "UpdateInfoPanel", self)

end

function InstallerTool:OnAboutValueChange(value)

  local offset = math.ceil((#self.aboutLines - self.maxAboutLines - 1) * value)

  self:DrawAboutLines(offset)

end

function InstallerTool:DrawAboutLines(offset)

  DrawRect(16, 64 + 8, 208, 64, 0, DrawMode.TilemapCache)
  offset = offset or 0

  for i = 1, self.maxAboutLines do

    local line = self.aboutLines[i + offset]

    line = string.rpad(line, 52, " "):upper()
    DrawText(line, 16, 64 + (i * 8), DrawMode.TilemapCache, "medium", 15, - 4)

  end

end

-- The self:Update() method is part of the game's life cycle. The engine calls self:Update() on every frame
-- before the Draw() method. It accepts one argument, timeDelta, which is the difference in
-- milliseconds since the last frame.
function InstallerTool:UpdateInfoPanel()

    editorUI:UpdateInputField(nameInputData)

    editorUI:UpdateButton(installButtonData)
    editorUI:UpdateButton(cleanCheckboxData)

    editorUI:UpdateSlider(aboutSliderData)
    editorUI:UpdateSlider(fileSliderData)

end
