--[[
	Pixel Vision 8 - Paint Tool
	Copyright (C) 2021, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

LoadScript("pixel-vision-os-v2")
LoadScript("code-paint-tool")

-- Create an global instance of the Pixel Vision OS
_G["pixelVisionOS"] = PixelVisionOS:Init()

local currentTool = nil

function Init()
  
  -- Disable the back key in this tool
  EnableBackKey(false)
  EnableAutoRun(false)

  -- Update background
  BackgroundColor(5)

  -- Create new tool instance based on the file extension
  currentTool = CreateTool()

end

function Update(timeDelta)

  pixelVisionOS:Update(timeDelta/ 1000)

end

function Draw()

  RedrawDisplay()

  pixelVisionOS:Draw()
  
end

function Shutdown()

  currentTool:Shutdown()

end


