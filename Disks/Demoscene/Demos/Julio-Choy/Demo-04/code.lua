-- The moving particles in this demo will have an acceleration and maximum speed.
local acceleration = 0.2
local maxSpeed = 4

-- There will be a single static object that the particles will be pulled towards.
local staticObject = {x = 132, y = 128}

-- We'll need to store the particles in a table to iterate through later.
local movingObjects = {}

-- We'll have a mouse click change the static object's position, so we'll need a reference to the mouse's position for later.
local mousePos = {x = 0, y = 0}

-- This function creates a new moving particle at the given position. We can later use this function to create a moving particle every time the user left clicks somewhere on the screen.
function createMovingObject(xPos, yPos)

  -- Create a new object with position, velocity, and acceleration properties.
  local obj = {x = xPos, y = yPos, velX = 0, velY = 0, accelX = 0, accelY = 0}

  -- Add the new object to the movingObjects table.
  table.insert(movingObjects, obj)

end

function Init()

  -- We'll start the demo by creating one moving object at (100, 100).
  createMovingObject(100, 100)

end

function Update(timeDelta)

  -- We call MousePosition() to get the current position of the mouse.
  mousePos = MousePosition()

  -- If the user left clicks.
  if(MouseButton(0, InputState.Released)) then

    -- Create a new moving object at the current mouse position.
    createMovingObject(mousePos.x, mousePos.y)

  end

  -- If the user right clicks.
  if(MouseButton(1, InputState.Released)) then

    -- Move the static object to the current mouse position.
    staticObject.x = mousePos.x
    staticObject.y = mousePos.y

  end

  -- Iterate through every particle in movingObjects.
  for k, v in pairs(movingObjects) do

    -- If the particle is to the left of the static object, set its x acceleration to the right.
    if(v.x < staticObject.x) then
      v.accelX = acceleration
    end

    -- If the particle is to the right of the static object, set its x acceleration to the left.
    if(v.x > staticObject.x) then
      v.accelX = -acceleration
    end

    -- If the particle is above the static object, set its y acceleration downwards.
    if(v.y < staticObject.y) then
      v.accelY = acceleration
    end

    -- If the particle is below the static object, set its y acceleration upwards.
    if(v.y > staticObject.y) then
      v.accelY = -acceleration
    end

    -- Update the particle's position using its velocity.
    v.x +  = v.velX
    v.y +  = v.velY

    -- Update the particle's velocity using its acceleration.
    v.velX +  = v.accelX
    v.velY +  = v.accelY

    -- Limit the particle's max velocity from the right.
    if(v.velX > maxSpeed) then
      v.velX = maxSpeed
    end

    -- Limit the particle's max velocity from the left.
    if(v.velX < - maxSpeed) then
      v.velX = -maxSpeed
    end

    -- Limit the particle's velocity from below.
    if(v.velY > maxSpeed) then
      v.velY = maxSpeed
    end

    -- Limit the particle's velocity from above.
    if(v.velY < - maxSpeed) then
      v.velY = -maxSpeed
    end

  end

end

function Draw()

  RedrawDisplay()

  -- Draw the static object.
  DrawSprite(1, staticObject.x, staticObject.y)

  -- Iterate through every moving particle and draw it.
  for k, v in pairs(movingObjects) do
    DrawSprite(2, v.x, v.y)
  end

  -- Draw a crosshair sprite at the current mouse position.
  DrawSprite(0, mousePos.x, mousePos.y)

end
