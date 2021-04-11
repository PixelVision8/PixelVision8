--[[
  Pixel Vision 8 - Preloader Tool
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  Please do not copy and distribute verbatim copies
  of this license document, but modifications without
  distributing is allowed.
]]--

-- Debugger values
local connectDebugger = true
local debuggerTime = 0
local debuggerDelay = 10

local currentAnimation = {}

-- Game boot animation frames
local insertAnimation = {
    "loadingframe1",
    "loadingframe2",
    "loadingframe3",
    "loadingframe4",
    "loadingframe5",
    "loadingframe6",
    "loadingframe7",
    "loadingframe8",
    "loadingframe9",
    "loadingframe9",
    "loadingframe10",
    "loadingframe11",
    "loadingframe12",
    "loadingframe13",
    "loadingframe14",
    "loadingframe15",
    "loadingframe16",
    "loadingframe17",
    "loadingframe18",
    "loadingframe17",
    "loadingframe19",
    "loadingframe20",
    "loadingframe18",
    "loadingframe19",
    "loadingframe18",
    "loadingframe19",
    
}

local loadAnimation = {
    "loadingframe17",
    "loadingframe18",
    "loadingframe17",
    "loadingframe19",
    "loadingframe20",
    "loadingframe18",
    "loadingframe19",
    "loadingframe18",
    "loadingframe19",
}

local ejectAnimation = {
    "loadingframe18",
    "loadingframe19",
    "loadingframe18",
    "loadingframe20",
    "loadingframe19",
    "loadingframe20",
    "loadingframe19",
    "loadingframe18",
    "loadingframe17",
    "loadingframe16",
    "loadingframe15",
    "loadingframe14",
    "loadingframe13",
    'loadingframe12',
    "loadingframe11",
    "loadingframe10",
    "loadingframe10",
    "loadingframe18",
    "loadingframe19",
    "loadingframe18",
    "loadingframe20",
    "loadingframe19",
    "loadingframe20",
    "loadingframe19",
}

-- Animation properties
local animDelay = .04
local animTime = 0
local readDelay = .04
local readTime = readDelay
local frame = 1
local offset = 10
local percent = 0
local startDelay = .08
local delayTime = 0
local preloading = false
local preloadComplete = false
local playSounds = true
local showDebugger = false

-- We are going to reduce the count by 6 to start preloading once the disk insertion animation is done
local totalFrames = 0
local loopKeyframe = 0

local runnerType = "none"

function Init()

    -- CreateMetaSprites()

    if(EnableAutoRun ~= nil) then
        EnableAutoRun(false)
    end

    if(EnableBackKey ~= nil) then
        EnableBackKey(false)
    end

    playSounds = ReadBiosData("PlaySystemSounds", "True") == "True"

    -- Set the background an rebuild the screen buffer
    BackgroundColor(tonumber(ReadBiosData("DefaultBackgroundColor", "5")))

    if(ReadMetadata("showEjectAnimation") == "true") then
        currentAnimation = ejectAnimation
        mode = "ejecting"
        totalFrames = #currentAnimation
        loopKeyframe = totalFrames - 2

        -- Start the unload process
        StartUnload()

    elseif(ReadMetadata("showDiskAnimation") == "true") then
        mode = "inserting"
        currentAnimation = insertAnimation
        totalFrames = #currentAnimation
        loopKeyframe = totalFrames - 7
    else
        currentAnimation = loadAnimation
        mode = "loading"
        totalFrames = #currentAnimation
        loopKeyframe = totalFrames - 7
    end

    if(connectDebugger == true) then
        showDebugger = ReadMetadata( "showDebugger", "false" ) == "true"
    end

end

function Update(timeDelta)

    -- Convert timeDelta to a float
    timeDelta = timeDelta / 1000

    -- Calculate start delay
    delayTime = delayTime + timeDelta

    -- Test for the start delay before beginning the animation
    if(delayTime < startDelay) then
        return
    end

    -- Increase frame animation
    animTime = animTime + timeDelta

    -- If preloading is set to true, read the percentage
    if(preloading == true) then

        percent = ReadPreloaderPercent()

        if(connectDebugger == true and showDebugger == false) then
            if(Key(Keys.LeftShift, InputState.Down)) then
                showDebugger = true
            end
        end

        -- If the percent is 100, call PreloaderComplete()
        if(percent >= 100) then

            if(preloadComplete == false) then
                preloadComplete = true
                delayTime = 0
                frame = 1
            elseif(connectDebugger == true and showDebugger) then

                -- check to see if the debugger has been connected yet
                debuggerTime = debuggerTime + timeDelta

                if(debuggerTime > debuggerDelay or Key(Keys.Escape) or DebuggerAttached()) then
                    connectDebugger = false
                    return
                end

            else
                PreloaderComplete()
            end

        end

    end

    -- Test to see if animation is greater than delay
    if(animTime > animDelay and preloadComplete == false) then

        -- Reset time
        animTime = 0

        -- Increment frame by 1
        frame = frame + 1

        if(mode == "ejecting") then
        
            if(frame == 15) then
            
                EndUnload()
                -- Once disk is ejected, it needs to load the next game
                mode = "loading"
            end
            
        end

        if((mode == "inserting" and frame > 17) or (mode == "ejecting" and frame < 8) or (mode == "loading") ) then
            readTime = readTime + timeDelta

            if(readTime > readDelay and playSounds) then
                PlaySound(1)
                readTime = 0
            end
        end

        -- Test to see if we are out of frames
        if(frame >= totalFrames) then

            -- Restart the loop 6 frames from the end
            frame = loopKeyframe

            -- If we are not preloading, trigger that process
            if(preloading == false) then

                -- Start the engine's preloader
                StartNextPreload()

                -- Change the preloading flag
                preloading = true

                -- Make the totalFrames include the last frames so it loops properly
                totalFrames = #currentAnimation

            end

        end

    end

end

function Draw()

    -- Copy over screen buffer
    RedrawDisplay()

    -- Get current sprite data
    -- local spriteId = FindMetaSpriteId(currentAnimation[frame])

    -- if(sprite ~= nil) then
        -- Draw sprite to the display
    DrawMetaSprite(FindMetaSpriteId(currentAnimation[frame]), 112, 96, false, false, DrawMode.Sprite, 0)
    -- end

    if(preloadComplete == true and showDebugger == true) then

        local countdown = debuggerDelay - math.ceil(debuggerTime)

        DrawText(string.format("WAITING FOR DEBUGGER %02d", countdown), 5 * 8, 27 * 8, DrawMode.Sprite, "large", 15)

    elseif((preloading == true or mode == "loading") and mode ~= "ejecting") then

        DrawText(string.format("LOADING %03d%s", percent, "%"), offset * 8, 27 * 8, DrawMode.Sprite, "large", 15)

    end

    runnerType = ReadMetadata( "runnerType", "none" )

    if(runnerType == "csharp") then runnerType = "csharp" end

    -- if(_G["runner"..runnerType] ~= nil) then
        DrawMetaSprite(FindMetaSpriteId("runner"..runnerType), 116, 72)
    -- end

end