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


    pixelVisionOS:RegisterUI({name = gameInfoPanelID}, "GameInfoPanelUpdate", self)

end 

function InfoTool:GameInfoPanelUpdate()
    
    
    
end