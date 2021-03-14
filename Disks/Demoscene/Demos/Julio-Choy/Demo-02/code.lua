-- We'll be creating several moving fireballs, so we need a table to keep track of them.
local fireballs = {}

-- The fireballs will change colors as they bounce around, so we'll make a list of colors for the fireballs to use.
local fireColors = {9, 13, 14, 15}

-- Each fireball will be a 10 x 10 pixel square.
local fireballSize = 10

-- Since the fireballs need to bounce off of the game's boundaries, we'll need a reference to the display's dimensions.
local width, height

function Init()

  -- Display() gives us an object with data of the current display, which we'll need for collision checking later.
  local display = Display()

  -- We store the display's width and height values in these variables to use later.
  width = display.x
  height = display.y

  -- This loop will create 10 fireballs.
  for i = 1, 10 do

    -- We make a fireball object, with a random (x, y) position, a random color picked from fireColors, and 2 variables for the x and y velocity.
    local fireball = {x = math.random(width), y = math.random(height), dx = 1, dy = 1, colorIndex = math.random(#fireColors - 1)}

    -- We add the fireball object to the fireballs table to draw and update later.
    table.insert(fireballs, fireball)

  end

end

function Update(timeDelta)

  -- We iterate through every fireball object in the fireballs table.
  for k, v in pairs(fireballs) do

    -- The fireballs need to move, so we'll change its x and y positions with dx and dy.
    v.x +  = v.dx
    v.y +  = v.dy

    -- We'll have the fireball change colors every time they change directions, so we make a boolean to check this condition.
    local velocityChanged = false

    -- Check if the fireball is touching the right side of the display. Note that since the origin of the fireball is the top-left corner, we need to check if its right side (x + the width) is touching the wall.
    if(v.x + fireballSize > width) then

      -- Change the fireball's x velocity to -1 (left).
      v.dx = -1

      -- The velocity has changed, so we set this flag to true.
      velocityChanged = true

      -- Check if the fireball is touching the left side of the display.
    elseif(v.x < 0) then

      -- Change the fireball's x velocity to 1 (right).
      v.dx = 1

      -- The velocity has changed, so we set this flag to true.
      velocityChanged = true

    end

    -- Check if the fireball is touching the bottom side of the display. Note that since the origin of the fireball is the top-left corner, we need to check if its bottom side (y + the height) is touching the wall.
    if(v.y + fireballSize > height) then

      -- Change the fireball's y velocity to -1 (up).
      v.dy = -1

      -- The velocity has changed, so we set this flag to true.
      velocityChanged = true

      -- Check if the fireball is touching the top side of the display.
    elseif(v.y < 0) then

      -- Change the fireball's y velocity to 1 (down).
      v.dy = 1

      -- The velocity has changed, so we set this flag to true.
      velocityChanged = true

    end

    -- Check if the velocity has changed at any point during this frame.
    if(velocityChanged) then

      -- Increment the fireball's current color index from fireColors by 1.
      v.colorIndex +  = 1

      -- Since we'll use 2 colors from fireColors, at colorIndex and colorIndex+1, we want to limit colorIndex from 1 to the number of colors in fireColors - 1 to prevent an off-by-one error.
      if(v.colorIndex > #fireColors - 1) then

        -- Set the fireball's color to the first color in fireColors.
        v.colorIndex = 1

      end

    end

  end

end

function Draw()

  RedrawDisplay()

  -- To add a cool trail-like effect, instead of redrawing the display each time, we can draw 1000 squares of the same color as the background color, at random positions every Draw() call.
  for i = 1, 1000 do
    DrawRect(math.random(width), math.random(height), 2, 2, 0)
  end

  -- Iterate through every fireball object in fireballs
  for k, v in pairs(fireballs) do

    -- Draw the fireball as two parts: the first part is drawn using its x, y, size, and colorIndex values, the second part is drawn 2 pixels smaller, offset based on the velocity, and using the color next to the one used by the first part.
    DrawRect(v.x, v.y, fireballSize, fireballSize, fireColors[v.colorIndex])
    DrawRect(v.x + v.dx, v.y + v.dy, fireballSize - 2, fireballSize - 2, fireColors[v.colorIndex + 1])

  end

end
