--[[
  Pixel Vision 8 - Preloader Tool
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)  
  Created by Jesse Freeman (@jessefreeman)

  Please do not copy and distribute verbatim copies
  of this license document, but modifications without 
  distribiting is allowed.
]]--

-- -- mode enums
-- local Up, Down, Right, Left, A, B, Select, Start = 0, 1, 2, 3, 4, 5, 6, 7

-- animation data
local anim = {
  delay = 0,
  time = 500,
  frame = 1,
  total = 2,
  frames = {
      {
        "player-front-walk-1",
        "player-front-walk-2"
      },
      {
        "player-back-walk-1",
        "player-back-walk-2"
      },
      {
        "player-right-walk-1",
        "player-right-walk-2"
      },
      {
        "player-right-walk-1",
        "player-right-walk-2"
      }
    }
}

-- player data
local player = {
  dir = 1,
  flipH = false,
  x = 0,
  y = 0,
  tilePos = {
    col = 0, 
    row = 0
  }
}

-- map data
local map = {
  grid = {
    col = 0,
    row = 0,
    size = 0
  },
  scrollX = 0,
  scrollY = 0,
  scrollPos = {
    col = 0,
    row = 0
  },
  bounds = {
    min = 0, 
    max = 0
  }
}

local inputDelay = 200
local inputTime = 0

local message = ""

-- local input = {
--   button = {false, false, false, false},
--   mouse = {false},
--   mousePos = {x = 0, y = 0}
-- }

function Init()
  
  -- Build the screen buffer
  BackgroundColor(0)
 
  -- get the tile size we want to move the player
  map.grid.size = SpriteSize().X * 2
  map.grid.col = math.ceil(Display().X / map.grid.size)
  map.grid.rows = math.ceil(Display().Y / map.grid.size)

  print(TilemapSize( ).X /2 - map.grid.col)

  -- reset map values
  map.bounds.min = 1
  map.bounds.max = TilemapSize( ).X /2 - map.grid.col
  map.scrollY = 0
  map.scrollPos.col = 0
  map.scrollPos.row = 0

  movePlayer(1, 5)

end

function Update(timeDelta)
  
  -- timeDelta = timeDelta/1000
  ScrollPosition( map.scrollPos.col, map.scrollPos.row )
  ScrollPosition((map.scrollPos.col * map.grid.size), map.scrollY)
-- print(map.scrollX + (map.scrollPos.col * map.grid.size), map.scrollY)
  inputTime = inputTime + timeDelta

  if(inputTime > inputDelay) then

    inputTime = 0

    local dirVector = {x = 0, y = 0}

    -- capture imput
    if(Button(Buttons.Up)) then
      -- show back of player
      player.dir = 2
      player.flipH = false
      dirVector.y = -1
    end
    if (Button(Buttons.Down)) then
      -- show front of player
      player.dir = 1
      player.flipH = false
      dirVector.y = 1
    end
    if(Button(Buttons.Left)) then
      -- show side of player and toggle flip
      player.dir = 4
      player.flipH = true
      dirVector.x = -1
    end
    if (Button(Buttons.Right)) then
      -- show side of player and toggle flip
      player.dir = 3
      player.flipH = false
      dirVector.x = 1
    end

    if(dirVector.x ~= 0 or dirVector.y ~= 0) then
      movePlayer(dirVector.x, dirVector.y)
    end
  end
  -- update animation timer
  anim.delay = anim.delay + timeDelta
  
  -- see if we should change animation
  if(anim.delay > anim.time) then 
    
    -- reset the delay
    anim.delay = 0

    -- update the current frame
    anim.frame = anim.frame + 1

    -- make sure we reset the frame counter when out of anim.frames
    if(anim.frame > anim.total) then
      anim.frame = 1
    end
  end

end

function Draw()
  
  RedrawDisplay()

  DrawMetaSprite(anim.frames[player.dir][anim.frame], player.x, player.y, player.flipH, false)
  
  if(message ~= "") then
    DrawText(message, 8, 0, "message-font", 15, -4)
  end
end

function movePlayer(col, row)
  
  -- set up values to pre-calculate where the player should move to
  local nextCol = player.tilePos.col + col
  local nextRow = player.tilePos.row + row
  local nextScrollPos = {col = map.scrollPos.col, row = map.scrollPos.row }
  
  -- test the horizontal boundary
  if(nextCol < map.bounds.min) then
     nextCol = map.bounds.min
     nextScrollPos.col = nextScrollPos.col + col
  elseif nextCol >= map.bounds.max then
    nextCol = map.bounds.max-1
    nextScrollPos.col = nextScrollPos.col + col
  end

  -- test the vertical boundary
  if(nextRow < 0) then
     nextRow = 0
  elseif nextRow >= map.grid.rows then
    nextRow = map.grid.rows-1
  end
  
  -- calculate the correct map column
  local testCol = Repeat((nextCol + nextScrollPos.col) * 2, 40)

  -- calculate the correct map row
  local testRow = nextRow * 2-- + (apiBridge.displayHeight / apiBridge.spriteHeight)
  
  -- test for collision
  local testFlag = Flag(testCol, testRow)

  -- track if the player can walk
  local canWalk = false

  -- test for collision
  if(testFlag < 3) then
    canWalk = true
  end
  
  -- move the player
  if(canWalk) then

    -- set the values once collision is not detected
    player.tilePos.col = nextCol
    player.tilePos.row = nextRow
    map.scrollPos = nextScrollPos

    -- move the player
    player.x = nextCol * map.grid.size 
    player.y = nextRow * map.grid.size

    if(testFlag == -1) then
      message = ""
    elseif (testFlag == 0) then
      message = "Approaching A Castle"
    elseif (testFlag == 1) then
      message = "Approaching A Town"
    elseif (testFlag == 2) then
      message = "Approaching A Dungeon"
    end
  end
  
end

-- function math.repeate( value, length) return value - math.floor(value / length) * length end
