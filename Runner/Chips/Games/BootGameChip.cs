//namespace PixelVisionSDK.Chips
//{
//    public class BootGameChip: GameChip
//    {
////        -- Get references to boot animation sprite data
////local bootSprites = {
////  logoframe01,
////  logoframe02,
////  logoframe03,
////  logoframe04,
////  logoframe05,
////  logoframe06,
////  logoframe07,
////  logoframe08,
////  logoframe09,
////  logoframe10,
////  logoframe11,
////  logoframe12,
////  logoframe13,
////  logoframe14,
////  logoframe15,
////  logoframe15,
////  logoframe15,
////  logoframe15,
////  logoframe15,
////  logoframe15
////}
////
////-- Animation properties
////local animDelay = .1;
////local time = 0;
////local frame = 1;
////local done = false;
////local nextScreenDelay = .3
////local nextScreenTime = 0
////local startDelay = .5
////local ready = false
////local bottomBorder = 224 + 8
////-- local shortcutDelay = 1
////-- local shortcutTime = 0
////-- local checkShortcuts = true
////
////function Init()
////
////  EnableAutoRun(false)
////  --EnableBackKey(false)
////
////  -- Set the default background color
////  BackgroundColor(5)
////
////  local display = Display(false)
////
////  -- We are going to render the message in a box as tiles. To do this, we need to wrap the
////  -- text, then split it into lines and draw each line.
////  local wrap = WordWrap(message, (display.x / 8) - 3)
////  local lines = SplitLines(wrap)
////  local total = #lines
////  local startY = (240 + 16) / 8--((display.y * 2) + 16) / 8 --(((display.y * 2) / 8) - 7) - total -
////
////  --176 448
////
////  local runnerName = SystemName()
////  local runnerVer = "/"..SystemVersion() -- TODO we don't have a V char so use / instead
////
////  local labelWidth = (#runnerName * 8) + (#runnerVer * 4) + 16
////
////  local startX = 256 - labelWidth
////
////  DrawText(runnerName, startX, 225, DrawMode.TilemapCache, "large-bold", 11)
////
////  DrawText(runnerVer, startX + (#runnerName * 8) + 8, 225, DrawMode.TilemapCache, "small", 11, - 4)
////
////  -- DrawText("Runner Version")
////
////  -- We want to render the text from the bottom of the screen so we offset it and loop backwards.
////  for i = 1, total do
////    DrawText(lines[i], 1, startY + (i - 1), DrawMode.Tile, "large-bold", 15)
////  end
////
////  -- Replace the tile with a logo and rest the color offset to 0 (since the font was set to 15)
////  Tile(1, startY, logosmall.spriteIDs[1], 0)
////
////end
////
////function Update(timeDelta)
////
////  -- Track time of animation
////  time = time + timeDelta
////
////  shortcutTime = time + timeDelta
////
////  -- if(shortcutTime > shortcutDelay) then
////  --   checkShortcuts = false
////  -- end
////  --
////  -- if(checkShortcuts == true) then
////
////  -- end
////
////  -- Test to see if we are ready to display the boot animation
////  if(ready == false) then
////
////    -- If the current time is less than the start delay exit the Update method
////    if(time < startDelay) then
////      return
////
////    else
////      -- If the time has passed, reset time for the next frame and change ready to true
////      time = 0
////      ready = true
////
////      -- Play the boot song
////      PlayPattern(0, false)
////
////    end
////
////  end
////
////  -- If animation is past delay
////  if(time > animDelay) then
////
////    -- Reset animation time value
////    time = 0
////
////    if(done == true)then
////
////      local newScrollY = ScrollPosition().y
////
////      if(newScrollY < bottomBorder) then
////
////        newScrollY = newScrollY + math.floor(500 * timeDelta)
////
////        if(newScrollY > bottomBorder) then
////          newScrollY = bottomBorder
////        end
////
////        -- scroll background to new position
////        ScrollPosition(0, newScrollY)
////
////        -- Check that we are in boot mode
////      else -- if(editorBridge.mode == 2) then
////
////        nextScreenTime = nextScreenTime + timeDelta
////
////        if(nextScreenTime > nextScreenDelay) then
////          -- Trigger BootDone
////          BootDone()
////
////        end
////
////      end
////
////    else
////
////      -- Test to see if we are done
////      if(frame < #bootSprites) then
////
////        KeyPressCheck()
////
////        -- Not done with animation, go to next frame
////        frame = frame + 1
////        local sprite = bootSprites[frame]
////        UpdateTiles(10, 12, sprite.width, sprite.spriteIDs)
////
////      elseif(done == false) then
////
////        -- If frames are over total sprites, we are done
////        done = true
////
////      end
////    end
////  end
////end
////
////function Draw()
////
////
////
////  -- Redraw the entire display
////  RedrawDisplay()
////
////  -- Draw top border
////  DrawSprites(topborder.spriteIDs, 0, 0, topborder.width, false, false, DrawMode.Sprite, 0, false, false)
////
////  -- Draw bottom border
////  DrawSprites(bottomborder.spriteIDs, 0, 232, bottomborder.width, false, false, DrawMode.Sprite, 0, false, false)
////
////  -- Mask off the bottom of the screen so you can see the scrolling
////  DrawRect(0, 240, 256, 8, 0, DrawMode.UI)
////end
////
////function KeyPressCheck()
////
////  if(invalid == true) then
////    return
////  end
////
////  if(Key(Keys.F)) then
////    Fullscreen(not Fullscreen())
////    InvalidateKeys()
////  else
////    if(Key(Keys.Alpha1)) then
////      Scale(1)
////      InvalidateKeys()
////    elseif(Key(Keys.Alpha2)) then
////      Scale(2)
////      InvalidateKeys()
////    elseif(Key(Keys.Alpha3)) then
////      Scale(3)
////      InvalidateKeys()
////    elseif(Key(Keys.Alpha4)) then
////      Scale(4)
////      InvalidateKeys()
////    end
////  end
////
////end
////
////function InvalidateKeys()
////  invalid = true
////end
//    }
//}