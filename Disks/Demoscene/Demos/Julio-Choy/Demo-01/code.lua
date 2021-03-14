-- We'll be generating several planets and moons to draw and animate later, so we're making two tables to keep track of them.
local planets = {}
local moons = {}

-- This variable will make a moon orbit 16 pixels away from its planet.
local moonOrbitRadius = 16

-- The angle speed determines how fast a moon will rotate around its planet, and 1 degree / frame is a decent speed.
local angleSpeed = 1

function Init()

  -- This for loop will create 4 planets and a few moons for each planet.
  for i = 1, 4 do

    -- We're making a planet object with a random x and y position.
    local planet = {x = math.random(50, 200), y = math.random(50, 200)}

    -- We'll add the planet object to the planets table in order to draw them later.
    table.insert(planets, planet)

    -- We'll add 1 to 5 moons for this planet, making it a random amount for each planet.
    local numberOfMoons = math.random(1, 5)

    -- This loop will create each moon object for this planet.
    for j = 1, numberOfMoons do

      -- The moon will orbit the planet, so we'll need an origin to rotate around, and an angle and (x, y) offsets to work with polar coordinates.

      -- The angles for each moon will be evenly spaced out in (360 / numberOfMoons) increments.
      local moon = {x = planet.x, y = planet.y, angle = j * (360 / numberOfMoons), offsetX = 0, offsetY = 0}

      -- We'll add the moon object to the moons table in order to draw and update them later.
      table.insert(moons, moon)

    end

  end

end

function Update(timeDelta)

  -- This for loop iterates through every entry in the moons table, giving us the key and value to work with in each step.
  for k, v in pairs(moons) do

    -- The angle of this moon object will increase by angleSpeed degrees every time Update() runs.
    v.angle +  = angleSpeed

    -- We don't want the angle to keep increasing infinitely, so we'll use the modulo operation to keep its value to 0 - 359.
    v.angle % = 360

    -- These offsets are the (x, y) representation of the moon object's polar coordinates, calculated by using the formula x = radius * cosine(angle), y = radius * sine(angle). And since math.cos() and math.sin() use radians instead of degrees, we'll need math.rad() to convert the angle to radians
    v.offsetX = moonOrbitRadius * math.cos(math.rad(v.angle))
    v.offsetY = moonOrbitRadius * math.sin(math.rad(v.angle))

  end

end

function Draw()

  RedrawDisplay()

  ChangeSizeMode(SpriteSizes.Mode4)

  -- We iterate through each planet object in the planets table and draw them using DrawSpriteBlock() because the planet sprite is made up of a 2 x 2 square of sprites, so we need to draw that block of sprites together.
  for k, v in pairs(planets) do
    DrawSprite(0, v.x - 4, v.y - 4)
  end

  -- We iterate through each moon object in the moons table and draw them using DrawSprite() because the moon sprite is just a single sprite block.
  ChangeSizeMode(SpriteSizes.Mode1)

  for k, v in pairs(moons) do
    DrawSprite(2, v.x + v.offsetX, v.y + v.offsetY)
  end

end
