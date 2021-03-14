-- We'll be drawing several squares that make up the pattern, so we need a table to store them.
local squares = {}

-- This will be the center of the pattern.
local centerX = 132
local centerY = 124

-- This value will be the radius from the center of the pattern, which will change over time.
local mainRadius = 1

function Init()

  -- There will be 60 squares drawn in each "arm" of the pattern.
  for i = 1, 60 do

    -- There will be 6 "arms" in the pattern.
    for j = 1, 6 do

      -- We'll make a square object with a position, radius from the center, angle from the center, an angle offset for the main angle, and a color based on how far along the "arm" this square is.
      local square = {x = 0, y = 0, r = i * 2, angle = j * (360 / 6), angleOffset = 360 * i / 30, col = math.floor(15 * (i / 60)) + 1}

      -- Add the square to the squares list to update and draw them later.
      table.insert(squares, square)

    end

  end

end

function Update(timeDelta)

  -- Iterate through each square object.
  for k, v in pairs(squares) do

    -- Increment the angle offset by 2, and use modulo to keep the value within 0 - 359.
    v.angleOffset +  = 2
    v.angleOffset % = 360

    -- Increment the main angle at a slower rate, and also keep it within 0 - 359.
    v.angle +  = 0.5
    v.angle % = 360

    -- We'll use this variable to make the main angle's offset fluctuate between -45 and 45 degrees using math.cos().
    local da = 45 * math.cos(math.rad(v.angleOffset))

    -- Get the (x, y) representation of the polar coordinates of this square, adding in the fluctuating main radius and the fluctuating angle offset.
    v.x = (v.r + mainRadius) * math.cos(math.rad(v.angle + da))
    v.y = (v.r + mainRadius) * math.sin(math.rad(v.angle + da))

    -- Make the main radius fluctuate between -32 and 32 using math.sin().
    mainRadius = 32 * math.sin(math.rad(v.angleOffset))

  end

end

function Draw()

  Clear()
  
  -- Iterate through each square object.
  for k, v in pairs(squares) do

    -- Draw the square using its position and color.
    DrawRect(centerX + v.x, centerY + v.y, 6, 6, v.col, DrawMode.Sprite)

  end

end
