--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2016, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- Load sprites and libs
-- LoadScript("sb-sprites")
LoadScript("pixel-vision-os-v2")
LoadScript("code-draw-tool")

-- Create an global instance of the Pixel Vision OS
_G["pixelVisionOS"] = PixelVisionOS:Init()

function Init()
  
  -- Disable the back key in this tool
  EnableBackKey(false)
  EnableAutoRun(false)

  -- Update background
  BackgroundColor(5)

  -- Create new workspace tool instance
  drawTool = DrawTool:Init()

end

function Update(timeDelta)

  pixelVisionOS:Update(timeDelta/ 1000)

end

function Draw()

  RedrawDisplay()

  pixelVisionOS:Draw()

end

function Shutdown()

  drawTool:Shutdown()

end