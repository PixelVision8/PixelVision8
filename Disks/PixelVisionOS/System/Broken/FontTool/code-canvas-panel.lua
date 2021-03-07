LoadScript("pixel-vision-os-canvas-v4")

function FontEditorTool:CreateCanvasPanel()
    
    self.tools = {"pen", "eraser"}

    self.canvasData = pixelVisionOS:CreateCanvas(
        {
            x = 8,
            y = 40,
            w = 128,
            h = 128
        },
        {
            x = 128,
            y = 128
        },
        1,
        0, -- Forcing this to be white
        "Draw on the canvas",
        0
    )

    -- canvasData.overDrawArgs[8] = 0

    -- TODO need to reconnect this
    pixelVisionOS:CanvasBrushColor(self.canvasData, 15)

    self.canvasData.onFirstPress = function()
            
        print("First Press!", editorUI.inFocusUI.name, self.canvasData.inDrawMode, self.canvasData.mouseState)
        
        self:BeginUndo()

        -- self.canvasData.inDrawMode = true

        -- self:ForcePickerFocus(self.canvasData)

    end

    self.canvasData.onAction = function() 

        if(editorUI.inFocusUI ~= nil and editorUI.inFocusUI.name == self.canvasData.name) then
            self:OnSaveCanvasChanges()
            self:EndUndo()
        end

    end

    -- self.canvasData.onPress = function()

    --     -- Calculate the position in the canvas
    --     local tmpPos = NewPoint(
    --         Clamp(editorUI.collisionManager.mousePos.x - self.canvasData.rect.x, 0, self.canvasData.rect.w - 1) / self.canvasData.scale,
    --         Clamp(editorUI.collisionManager.mousePos.y - self.canvasData.rect.y, 0, self.canvasData.rect.h - 1) / self.canvasData.scale
    --     )

    --     -- See what color is under the mouse by reading the pixel
    --     local tmpColor = self.canvasData.paintCanvas:ReadPixelAt(tmpPos.x, tmpPos.y)

    --     print("tmpColor", tmpColor)
    --     -- Change the tool bases on what color the mouse is over
    --     pixelVisionOS:ChangeCanvasTool(self.canvasData, tmpColor == 0 and "pen" or "eraser", 6)

    --     self.canvasData.inDrawMode = true

    --     UpdateHistory(pixelVisionOS:GetCanvasPixelData(self.canvasData))
    -- end

    -- self.canvasData.onAction = function() self:OnSaveCanvasChanges() end

    pixelVisionOS:ChangeCanvasPixelSize(self.canvasData, 128/8)

    pixelVisionOS:RegisterUI({name = "CanvasPanelUpdateLoop"}, "CanvasPanelUpdate", self)

end

function FontEditorTool:CanvasPanelUpdate()
    
    pixelVisionOS:UpdateCanvas(self.canvasData)

    -- Calculate the position in the canvas
    -- local tmpPos = NewPoint(
    --     Clamp(editorUI.collisionManager.mousePos.x - self.canvasData.rect.x, 0, self.canvasData.rect.w - 1) / self.canvasData.scale,
    --     Clamp(editorUI.collisionManager.mousePos.y - self.canvasData.rect.y, 0, self.canvasData.rect.h - 1) / self.canvasData.scale
    -- )

    -- -- See what color is under the mouse by reading the pixel
    -- local tmpColor = self.canvasData.paintCanvas:ReadPixelAt(tmpPos.x, tmpPos.y)

    -- print("tmpColor", tmpColor)
    
    if(IsExporting()) then
        pixelVisionOS:DisplayMessage("Saving " .. tostring(ReadExportPercent()).. "% complete.", 2)
    end

end

function FontEditorTool:OnSaveCanvasChanges()

    print("Save Canvas")
    local pixelData = pixelVisionOS:GetCanvasPixelData(self.canvasData)
    -- local canvasSize = editorUI:GetCanvasSize(canvasData)

    local total = #pixelData

    for i = 1, total do
        -- if(pixelData[i] > - 1) then
        --   pixelData[i] = 0
        -- end
        pixelData[i] = pixelData[i] - self.WHITE
    end

    -- Update the current sprite in the picker
    gameEditor:FontSprite(self.currentCharID, pixelData)

    self:DrawFontCharacter(self.currentCharID)

    -- Invalidate the sprite tool since we change some pixel data
    self:InvalidateData()

    -- Reset the canvas invalidation since we copied it
    editorUI:ResetValidation(self.canvasData)

    -- end

    pixelVisionOS:EnableMenuItem(self.RevertShortcut, true)

    self:DrawSampleText()

end
