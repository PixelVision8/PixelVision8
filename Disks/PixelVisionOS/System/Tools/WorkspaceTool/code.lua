    --[[
	Pixel Vision 8 - Debug Tool
	Copyright (C) 2016, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

-- new code modules
-- LoadScript("sb-sprites")
LoadScript("pixel-vision-os-v2")
LoadScript("code-workspace-tool")

-- Create an global instance of the Pixel Vision OS
_G["pixelVisionOS"] = PixelVisionOS:Init()

-- Get all of the available editors
local editorMapping = {}

-- The Init() method is part of the game's lifecycle and called a game starts. We are going to
-- use this method to configure background color, ScreenBufferChip and draw a text box.
function Init()

    -- Disable the back key in this tool
    EnableBackKey(false)
    EnableAutoRun(false)

    -- Update background
    BackgroundColor(tonumber(ReadBiosData("DefaultBackgroundColor", "5")))

    -- Create new workspace tool instance
    workspaceTool = WorkspaceTool:Init()

end

-- The Update() method is part of the game's life cycle. The engine calls Update() on every frame
-- before the Draw() method. It accepts one argument, timeDelta, which is the difference in
-- milliseconds since the last frame.
function Update(timeDelta)

    
    -- Convert timeDelta to a float
    timeDelta = timeDelta / 1000

    if(workspaceTool.shuttingDown == true) then
        return
    end

    -- This needs to be the first call to make sure all of the OS and editor UI is updated first
    pixelVisionOS:Update(timeDelta)


end


-- The Draw() method is part of the game's life cycle. It is called after Update() and is where
-- all of our draw calls should go. We'll be using this to render sprites to the display.
function Draw()

    -- We can use the RedrawDisplay() method to clear the screen and redraw the tilemap in a
    -- single call.
    RedrawDisplay()

    -- OLD Code

    if(workspaceTool.shuttingDown == true) then

        local runnerName = SystemName()

        if(workspaceTool.shutdownScreen ~= true) then

            BackgroundColor(0)

            DrawRect(0, 0, 256, 480, 0, DrawMode.TilemapCache)

            local startX = math.floor((32 - #runnerName) * .5)
            DrawText(runnerName:upper(), startX, 10, DrawMode.Tile, "large", 15)
            DrawText("IS READY FOR SHUTDOWN.", 5, 11, DrawMode.Tile, "large", 15)

            if(Fullscreen() == true) then 

                local closeString = string.format("Press %s to close", OperatingSystem() == "Windows" and "Alt + F4" or "Command + Q")

                DrawText(closeString:upper(), (Display().X - (#closeString*4)) * .5, Display().Y - 16, DrawMode.TilemapCache, "small", 15, -4)
               
            end
            
            workspaceTool.shutdownScreen = true
        end

        return
    end

    -- The UI should be the last thing to draw after your own custom draw calls
    pixelVisionOS:Draw()

end

function Shutdown()

    workspaceTool:Shutdown()

end
