--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

local canvasID = "CanvasUI"

function DrawTool:CreateCanvas()
    self.showBGColor = false
    self.lastCanvasScale = 0
    self.lastCanvasSize = 0
    self.cps = gameEditor:ColorsPerSprite()

    self.canvasData = pixelVisionOS:CreateCanvas(
        {
            x = 32,
            y = 32,
            w = 128,
            h = 128
        },
        {
            x = 128,
            y = 128
        },
        1,
        256,
        "Draw on the canvas",
        pixelVisionOS.emptyColorID
    )
    

    self.canvasData.onFirstPress = function()
            
        --print("First Press!", editorUI.inFocusUI.name, self.canvasData.inDrawMode, self.canvasData.mouseState)
        
        self:BeginUndo(data)

        self.canvasData.inDrawMode = true

        self:ForcePickerFocus(self.canvasData)

    end

    -- editorUI.collisionManager:EnableDragging(self.canvasData, .5, "SpritePicker")

    -- self.canvasData.onDropTarget = function(src, dest) self:OnCanvasDrop(src, dest) end

    self.canvasData.onAction = function() 

        if(editorUI.inFocusUI ~= nil and editorUI.inFocusUI.name == self.canvasData.name) then
            print("Release")
            self:OnSaveCanvasChanges() 
            self:EndUndo(data)
        end

    end

    self.clearBackgroundPattern = {}

    local totalTiles = 128/8 * 128/8
    local tileID = emptycolor.spriteIDs[1]
    
    for i = 1, totalTiles do
        table.insert(self.clearBackgroundPattern, tileID)
    end

end

local paletteKeys = {Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8}

-- TODO need to rename this and change other UpdateCanvas function
function DrawTool:UpdateCanvasUI()

    if(self.canvasPanelBackgroundInvalid == true) then
        self:ClearCanvasPanelBackground()
    end

    pixelVisionOS:UpdateCanvas(self.canvasData)

    local newCopyValue = self.canvasData.selectRect ~= nil

    if(self.lastCopyValue ~= newCopyValue) then
        pixelVisionOS:EnableMenuItem(CopyShortcut, newCopyValue)
        pixelVisionOS:EnableMenuItem(ClearShortcut, newCopyValue)
        pixelVisionOS:EnableMenuItem(FillShortcut, newCopyValue)
        self.lastCopyValue = newCopyValue
    end

    if(self.selectionMode == DrawingMode) then

        for i = 1, 8 do
            if(Key( paletteKeys[i], InputState.Released )) then

                local index = i -1

                if(Key( Keys.LeftShift, InputState.Down ) or Key(Keys.RightShift)) then

                    index = index + 8

                end
                -- TODO need to select color in palette
                    -- self:OnSelectPaletteColor(index)
                break

            end
        end

    end
    
end

function DrawTool:ShowCanvasPanel()

    if(self.canvasPanelActive == true) then
        return
    end

    self.canvasPanelActive = true

    pixelVisionOS:RegisterUI({name = canvasID}, "UpdateCanvasUI", self)
    
    pixelVisionOS:InvalidateCanvas(self.canvasData)

end

function DrawTool:HideCanvasPanel()

    if(self.canvasPanelActive == false) then
        return
    end

    self.canvasPanelActive = false

    pixelVisionOS:RemoveUI(canvasID)
    
end

function DrawTool:ToggleBackgroundColor(value)


    self.showBGColor = value

    self.canvasData.showBGColor = value

    if(self.usePalettes == true) then

        -- pixelVisionOS:SelectColorPage(self.paletteColorPickerData, self.paletteColorPickerData.picker.selected)

    else
        self.canvasData.emptyColorID = pixelVisionOS.emptyColorID
    end

    pixelVisionOS:InvalidateCanvas(self.canvasData)

end

function DrawTool:InvalidateCanvasPanelBackground()

    -- TODO should only need to do this when there is a switch between modes
    if(self.selectionSize.x ~= self.selectionSize) then
        -- print("Canvas is invalid")
        self.canvasPanelBackgroundInvalid = true
    end

end

function DrawTool:ClearCanvasPanelBackground()

    -- print("Canvas BG Cleared")
    
    -- Need to draw immediately since the canvas doesn't run through the UI draw queue
    DrawSprites(self.clearBackgroundPattern, 4, 4, 16, false, false, DrawMode.Tile)

    self.canvasPanelBackgroundInvalid = false
end


function DrawTool:UpdateCanvas(value, flipH, flipV)

    self:InvalidateCanvasPanelBackground()

    flipH = flipH or false
    flipV = flipV or false

    -- Save the original pixel data from the selection
    local tmpPixelData = gameEditor:ReadGameSpriteData(value, self.selectionSize.x, self.selectionSize.y, flipH, flipV)--
    
    -- print("Test", self.spriteMode)
    self.lastCanvasScale = self.selectionSize.scale--Clamp(8 * spriteSelection.scale, 4, 16)

    self.lastCanvasSize = NewPoint((8 * self.selectionSize.x), (8 * self.selectionSize.y))

    pixelVisionOS:ResizeCanvas(self.canvasData, self.lastCanvasSize, self.lastCanvasScale, tmpPixelData)

    self.originalPixelData = {}

    -- Need to loop through the pixel data and change the offset
    local total = #tmpPixelData
    for i = 1, total do

        -- TODO index the canvas colors here
        local newColor = tmpPixelData[i] - self.colorOffset

        self.originalPixelData[i] = newColor

    end

    self.lastSelection = value

end

function DrawTool:IsSpriteEmpty(pixelData)

    local total = #pixelData

    for i = 1, total do
        if(pixelData[i] ~= -1) then
            return false
        end
    end

    return true

end

function DrawTool:OnSaveCanvasChanges()

    self.canvasData.inDrawMode = false

    -- Get the raw pixel data
    local pixelData = pixelVisionOS:GetCanvasPixelData(self.canvasData)

    -- Get the canvas size
    local canvasSize = pixelVisionOS:GetCanvasSize(self.canvasData)

    -- Get the total number of pixel
    local total = #pixelData

    -- Loop through all the pixel data
    for i = 1, total do

        -- Shift the color value based on the canvas color offset
        local newColor = pixelData[i] - self.canvasData.colorOffset

        -- Set the new pixel index value
        pixelData[i] = newColor < 0 and - 1 or newColor

    end

    -- Update the spritePickerData
    if(self.spritePickerData.currentSelection > - 1) then
        pixelVisionOS:UpdateItemPickerPixelDataAt(self.spritePickerData, self.spritePickerData.currentSelection, pixelData, canvasSize.width, canvasSize.height)
    end

    -- Update the current sprite in the picker
    gameEditor:WriteSpriteData(self.spritePickerData.currentSelection, pixelData, self.selectionSize.x, self.selectionSize.y)

    -- Invalidate the sprite tool since we change some pixel data
    self:InvalidateData()

    -- Reset the canvas invalidation since we copied it
    editorUI:ResetValidation(self.canvasData)

end