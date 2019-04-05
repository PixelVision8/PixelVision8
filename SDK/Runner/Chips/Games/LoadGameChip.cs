//namespace PixelVisionSDK.Chips
//{
//    public class LoadGameChip : GameChip
//    {
////        --[[
////	Pixel Vision 8 - Preloader Tool
////	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
////	Created by Jesse Freeman (@jessefreeman)
////
////	Please do not copy and distribute verbatim copies
////	of this license document, but modifications without
////	distributing is allowed.
////]]--
////
////LoadScript("sb-sprites")
////
////-- Game boot animation frames
////local currentAnimation = {
////  loadingframe1,
////  loadingframe2,
////  loadingframe3,
////  loadingframe4,
////  loadingframe5,
////  loadingframe6,
////  loadingframe7,
////  loadingframe8,
////  loadingframe9,
////  loadingframe10,
////  loadingframe11,
////  loadingframe12,
////  loadingframe13,
////  loadingframe14,
////  loadingframe15,
////  loadingframe16,
////  loadingframe17,
////  loadingframe18,
////  loadingframe19,
////  loadingframe20,
////
////  loadingframe21,
////  loadingframe22,
////  loadingframe23,
////  loadingframe24,
////  loadingframe25,
////  loadingframe26,
////}
////
////-- Animation properties
////local animDelay = .04
////local animTime = 0
////local frame = 1
////local loopAnimation = false
////local message = "LOADING    %"
////local offset = 10
////local percent = 0
////local startDelay = .5
////local delayTime = 0
////local preloading = false
////local preloadComplete = false
////
////-- We are going to reduce the count by 6 to start preloading once the disk insertion animation is done
////local totalFrames = #currentAnimation - 6
////local loopKeyframe = totalFrames
////
////function Init()
////
////  EnableAutoRun(false)
////  --EnableBackKey(false)
////
////  -- Set the background an rebuild the screen buffer
////  BackgroundColor(5)
////
////  local runnerName = SystemName()
////  local runnerVer = SystemVersion() -- TODO we don't have a V char so use / instead
////
////  local labelWidth = #runnerName + #runnerVer + 2
////
////  local startX = math.floor((32 / 2) - (labelWidth / 2))
////
////  -- DrawSprite(logosmall.spriteIDs[1], startX, 1, DrawMode.Tile)
////  DrawSprite(logosmall.spriteIDs[1], startX, 1, false, false, DrawMode.Tile)
////
////  startX = startX + 1
////
////  DrawText(runnerName, startX, 1, DrawMode.Tile, "large-bold", 15)
////
////  DrawText(runnerVer, startX + #runnerName + 1, 1, DrawMode.Tile, "large-bold", 15)
////
////
////  if(ReadMetaData("showDiskAnimation") == "false") then
////    frame = 18
////  end
////
////end
////
////function Update(timeDelta)
////
////  -- Calculate start delay
////  delayTime = delayTime + timeDelta
////
////  -- Test for the start delay before beginning the animation
////  if(delayTime < startDelay) then
////    return
////  end
////
////  -- Increase frame animation
////  animTime = animTime + timeDelta
////
////  -- If preloading is set to true, read the percentage
////  if(preloading == true) then
////    percent = ReadPreloaderPercent()
////
////    -- If the percent is 100, call PreloaderComplete()
////    if(percent >= 100) then
////
////      if(preloadComplete == false) then
////        preloadComplete = true
////        delayTime = 0
////        frame = 1
////      else
////        PreloaderComplete()
////
////      end
////
////    end
////
////  end
////
////  -- Test to see if animation is greater than delay
////  if(animTime > animDelay and preloadComplete == false) then
////
////    -- Reset time
////    animTime = 0
////
////    -- Increment frame by 1
////    frame = frame + 1
////
////    -- Test to see if we are out of frames
////    if(frame >= totalFrames) then
////
////      -- Restart the loop 6 frames from the end
////      frame = loopKeyframe
////
////      -- If we are not preloading, trigger that process
////      if(preloading == false) then
////
////        -- Display the message
////        DrawText(message, offset, 27, DrawMode.Tile, "large-bold", 15)
////
////        -- Set the offset for the number sprites
////        offset = offset + (#message - 4)
////
////        -- Start the engine's preloader
////        StartNextPreload()
////
////        -- Change the preloading flag
////        preloading = true
////
////        -- Make the totalFrames include the last frames so it loops properly
////        totalFrames = #currentAnimation
////
////      end
////
////    end
////
////  end
////
////end
////
////function Draw()
////
////  -- Copy over screen buffer
////  RedrawDisplay()
////
////  -- Get current sprite data
////  local sprite = currentAnimation[frame]
////
////  if(sprite ~= nil) then
////    -- Draw sprite to the display
////    DrawSprites(sprite.spriteIDs, 112, 80 + 16, sprite.width, false, false, DrawMode.Sprite, 0)
////  end
////
////  if(preloading == true) then
////
////    -- Draw percent as sprites
////    local percentString = string.rpad(tostring(percent), 3, "0")
////    DrawText(percentString, offset * 8, 27 * 8, DrawMode.Sprite, "large-bold", 15)
////
////  end
////
////end
////
////string.rpad = function(str, len, char)
////  if char == nil then char = ' ' end
////  return string.rep(char, len - #str) .. str
////end
//
//    }
//}

