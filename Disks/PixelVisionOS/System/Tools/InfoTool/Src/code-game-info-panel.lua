--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

local gameInfoPanelID = "GameInfoPanel"

function InfoTool:CreateGameInfoPanelPanel()
	
	local pathSplit = string.split(self.targetFile, "/")

    -- Update title with file path
    self.toolTitle = pathSplit[#pathSplit - 1] .. "/" .. pathSplit[#pathSplit]

    if(self.rootDirectory ~= nil) then

        -- Load only the game data we really need
        self.success = gameEditor:Load(self.rootDirectory, {SaveFlags.Meta})

    end

    -- Attempt to load a custom icon
    pixelVisionOS:LoadCustomIcon(NewWorkspacePath(self.rootDirectory).AppendFile("icon.png"))
    
    -- Draw the custom icon to the info panel
    DrawMetaSprite(FindMetaSpriteId("filecustomiconup"), 12, 35, false, false, DrawMode.TilemapCache)

    local name = gameEditor:ReadMetadata("name", "untitled")

    local description = gameEditor:ReadMetadata("description", "")

    self.nameInputData = pixelVisionOS:CreateInputField({x = 48, y = 40, w = 160}, name, "Enter in a file name to this string input field.", "file")

    self.nameInputData.onAction = function(value)

        gameEditor:WriteMetadata("name", value)

        self:InvalidateData()

    end

    self.inputAreaData = pixelVisionOS:CreateInputArea({x = 16, y = 72, w = 208, h = 56}, description, "Change the game description.")
    -- self.inputAreaData.wrap = false
    self.inputAreaData.editable = true
    self.inputAreaData.autoDeselect = false
    -- self.inputAreaData.colorize = codeMode
    
    self.inputAreaData.onAction = function(value)
    
       gameEditor:WriteMetadata("description", value)
    
       self:InvalidateData()
    end
    
    -- Prepare the input area for scrolling
    self.inputAreaData.scrollValue = {x = 0, y = 0}
    
    self.vSliderData = editorUI:CreateSlider({x = 235 - 8, y = 73 - 5, w = 10, h = 56 + 9}, "vsliderhandle", "Scroll text vertically.")
    self.vSliderData.onAction = function(value) self:OnVerticalScroll(value) end
    
    self.hSliderData = editorUI:CreateSlider({ x = 16 + 4 - 8, y = 136 - 5, w = 208 + 9, h = 10}, "hsliderhandle", "Scroll text horizontally.", true)
    self.hSliderData.onAction = function(value) self:OnHorizontalScroll(value) end
    

    pixelVisionOS:RegisterUI({name = gameInfoPanelID}, "GameInfoPanelUpdate", self)

end 

function InfoTool:GameInfoPanelUpdate()

	editorUI:UpdateInputField(self.nameInputData)

	editorUI:UpdateInputArea(self.inputAreaData)

	-- Check to see if we should show the horizontal slider
	local showVSlider = #self.inputAreaData.buffer > self.inputAreaData.tiles.h

	-- Test if we need to show or hide the slider
	if(self.vSliderData.enabled ~= showVSlider) then
		editorUI:Enable(self.vSliderData, showVSlider)
	end

	if(self.vSliderData.enabled == true) then
		self.inputAreaData.scrollValue.y = (self.inputAreaData.vy - 1) / (#self.inputAreaData.buffer - self.inputAreaData.tiles.h)

		if(self.vSliderData.value ~= self.inputAreaData.scrollValue.y) then

			-- InvalidateLineNumbers()

			editorUI:ChangeSlider(self.vSliderData, self.inputAreaData.scrollValue.y, false)
		end

	end

	-- Update the slider
	editorUI:UpdateSlider(self.vSliderData)

	-- Check to see if we should show the vertical slider
	local showHSlider = self.inputAreaData.maxLineWidth > self.inputAreaData.tiles.w

	-- Test if we need to show or hide the slider
	if(self.hSliderData.enabled ~= showHSlider) then
		editorUI:Enable(self.hSliderData, showHSlider)
	end

	if(self.hSliderData.enabled == true) then
		self.inputAreaData.scrollValue.x = (self.inputAreaData.vx - 1) / ((self.inputAreaData.maxLineWidth + 1) - self.inputAreaData.tiles.w)

		if(self.hSliderData.value ~= self.inputAreaData.scrollValue.x) then
			
			editorUI:ChangeSlider(self.hSliderData, self.inputAreaData.scrollValue.x, false)
		end

	end

	-- Update the slider
	editorUI:UpdateSlider(self.hSliderData)

end

function InfoTool:OnHorizontalScroll(value)

    local charPos = math.ceil(((self.inputAreaData.maxLineWidth + 1) - (self.inputAreaData.tiles.w)) * value) + 1

    if(self.inputAreaData.vx ~= charPos) then
        self.inputAreaData.vx = charPos
        editorUI:TextEditorInvalidateBuffer(self.inputAreaData)
    end

end

function InfoTool:OnVerticalScroll(value)

    -- print("value", value)

    local line = math.ceil((#self.inputAreaData.buffer - (self.inputAreaData.tiles.h - 1)) * value)
    if(self.inputAreaData.vy ~= line) then
        self.inputAreaData.vy = Clamp(line, 1, #self.inputAreaData.buffer)

        editorUI:TextEditorInvalidateBuffer(self.inputAreaData)
    end

end