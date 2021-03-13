--[[
	Pixel Vision 8 - Debug Tool
	Copyright (C) 2016, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- Load sprites and libs
-- LoadScript("sb-sprites")
LoadScript("pixel-vision-os-v2")
LoadScript("code-music-tool")

local musicTool = nil

-- Create an global instance of the Pixel Vision OS
_G["pixelVisionOS"] = PixelVisionOS:Init()

function Init()
  
  -- Disable the back key in this tool
  EnableBackKey(false)
  EnableAutoRun(false)

  -- Update background
  BackgroundColor(5)

  -- Create new workspace tool instance
  musicTool = MusicTool:Init()

end

function Update(timeDelta)

  pixelVisionOS:Update(timeDelta/ 1000)

end

function Draw()

  RedrawDisplay()

  -- musicTool:Draw()

  pixelVisionOS:Draw()

end

function Shutdown()

  musicTool:Shutdown()

end


