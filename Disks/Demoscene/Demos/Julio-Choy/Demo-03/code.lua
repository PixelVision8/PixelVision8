-- This demo will allow the user to "erase" away frost that accumulates on the screen, so we need an "eraser" object, with an (x, y) position, size, and speed for moving around.
local blobX = 132
local blobY = 128
local blobSize = 20
local blobSpeed = 2

-- We also want the user to be able to toggle the frost effect on and off, so we have this boolean for that.
local frostToggle = true

-- The frost effect will consist of many small squares of these 3 specific colors.
local snowColors = {2, 3, 15}

-- We'll need to use the display's dimensions for later calculations, so we create these variables.
local width, height

function Init()

  -- We'll change the background color to a less darker color, closer to a stone-like color.
  BackgroundColor(5)

  -- The background color doesn't update to the new one that was set unless this is called.
  Clear()

  -- Since the eraser will move around the screen, we want to keep its position within bounds, so we'll need the display's dimensions to use for calculations later.
  local display = Display()
  width = display.x
  height = display.y

end

function Update(timeDelta)

  -- The eraser will need x and y velocities.
  local dx = 0
  local dy = 0

  -- If left is pressed, make the x velocity towards the left.
  if(Key(Keys.Left)) then
    dx +  = -blobSpeed
  end

  -- If right is pressed, make the x velocity towards the right.
  if(Key(Keys.Right)) then
    dx +  = blobSpeed
  end

  -- If up is pressed, make the y velocity upwards.
  if(Key(Keys.Up)) then
    dy +  = -blobSpeed
  end

  -- If down is pressed, make the y velocity downwards.
  if(Key(Keys.Down)) then
    dy +  = blobSpeed
  end

  -- After calculating the net x and y velocities, change the eraser's position accordingly.
  blobX +  = dx
  blobY +  = dy

  -- We want to keep the eraser's position within the display's boundaries, so we use modulo to do so.
  blobX % = width
  blobY % = height

  -- If Z was just released, toggle the frost effect on/off.
  if(Key(Keys.Z, InputState.Released)) then
    frostToggle = not frostToggle
  end

end

function Draw()

  RedrawDisplay()
  
  -- We'll draw some text to instruct the player how to move around.
  DrawText("Use arrow keys to move.", 24, 32, DrawMode.Sprite, "large", 6)

  -- We'll also tell the user that Z toggles the frost effect on and off.
  DrawText("Press Z to toggle the frost.", 24, 64, DrawMode.Sprite, "large", 6)

  -- Check if the frost effect flag is on.
  if(frostToggle) then

    -- Do the following 15 times every Draw() call.
    for i = 1, 15 do

      -- Draw a 2x2 square at a random position and with a random color from snowColors.
      DrawRect(math.random(width), math.random(height), 2, 2, snowColors[math.random(#snowColors)])

    end

  end

  -- The "eraser" will essentially be a square area where 20 squares are drawn within it every Draw() call.
  for i = 1, 20 do

    -- Draw a 3x3 square at a random position within the eraser's area, using the same color as the background.
    DrawRect(math.random(blobX, blobX + blobSize), math.random(blobY, blobY + blobSize), 3, 3, 5)

  end

end
