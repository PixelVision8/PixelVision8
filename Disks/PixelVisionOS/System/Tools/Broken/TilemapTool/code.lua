--[[
	Pixel Vision 8 - Tilemap Tool
	Copyright (C) 2016, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- Load sprites and libs
LoadScript("sb-sprites")
LoadScript("pixel-vision-os-v2")
LoadScript("code-tilemap-tool")

-- Create an global instance of the Pixel Vision OS
_G["pixelVisionOS"] = PixelVisionOS:Init()

local tilemapTool = nil

function Init()

    -- Disable the back key in this tool
    EnableBackKey(false)
    EnableAutoRun(false)

    -- Update background
    BackgroundColor(5)
    
    -- Create new workspace tool instance
    tilemapTool = TilemapTool:Init()

end

function Update(timeDelta)

    pixelVisionOS:Update(timeDelta/ 1000)
    
    -- TODO this is for debugging until each panel is self contained
    tilemapTool:Update()

end

function Draw()

    RedrawDisplay()

    pixelVisionOS:Draw()

end

function Shutdown()

    tilemapTool:Shutdown()

end