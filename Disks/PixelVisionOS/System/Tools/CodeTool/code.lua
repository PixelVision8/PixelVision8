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

-- Figure out which tool to load
if(NewWorkspacePath(ReadMetadata("file", nil)).GetExtension() == ".png") then
  LoadScript("code-image-tool")
else
  LoadScript("code-text-tool")
end

-- Create an global instance of the Pixel Vision OS
_G["pixelVisionOS"] = PixelVisionOS:Init()

local currentTool = nil

function Init()
  --print("Init")

  -- Disable the back key in this tool
  EnableBackKey(false)
  EnableAutoRun(false)

  -- Update background
  BackgroundColor(5)

  -- pixelVisionOS.messageBar.clearColorID = 2

  -- Create new tool instance based on the file extension
  currentTool = CreateTool()

end

function Update(timeDelta)

  pixelVisionOS:Update(timeDelta/ 1000)

end

function Draw()

  RedrawDisplay()

  pixelVisionOS:Draw()
  
  -- TODO should this be registered instead of hard coded?
  currentTool:Draw()

end

function Shutdown()

  currentTool:Shutdown()

end


