-- To animate the typed characters, we'll need to store them in a table.
local characters = {}

-- We'll store the typed text in a string variable, and we'll limit the length to 28 characters.
local message = ""
local maxCharLength = 28

-- There are 2 states in this demo, one is the typing state, the other is the animating state.
local isAnimating = false

-- This will be the top-center origin where the characters animate from.
local messageX = 124
local messageY = 24

function Update(timeDelta)

  -- If we're in the animating state.
  if(isAnimating) then

    -- Iterate through every character.
    for k, v in pairs(characters) do

      -- Make the x offset in a sine-wave path with an angle offset for every character.
      v.offsetX = 124 * - math.sin(math.rad(v.offsetDelta))

      -- Increase the angle offset so that the character moves along the sine-wave path over time.
      v.offsetDelta +  = 2

    end

    -- Set the top position of the characters' origin based on how far along the sine wave the first character is. 720 is used to make the characters move along the sine wave for more cycles.
    messageY = 248 * characters[1].offsetDelta / 720

    -- If the top-most part of the animating characters has passed the bottom of the screen.
    if(messageY > 240) then

      -- Move the origin back to the top.
      messageY = 24

      -- Stop animating the characters, so switch back to the typing state.
      isAnimating = false

      for k, v in pairs(characters) do

        -- The table of characters needs to be cleared so that the next time the animation happens, the old characters aren't used.
        characters[k] = nil

      end

    end

  else

    -- Check if the typed message isn't longer than maxCharLength and if a character was just typed using InputString().
    if(string.len(message) < maxCharLength) then

      -- Concatenate the typed string to the message variable.
      message = message .. InputString()

    end

    -- If backspace was just released.
    if(Key(Keys.Back, InputState.Released)) then

      -- The delete operation should happen only if the message isn't empty.
      if(string.len(message) > 0) then

        -- Delete the last typed character, using string.sub() to get the substring excluding the last character.
        message = string.sub(message, 1, string.len(message) - 1)

      end

    end

    -- If there's a non-empty message to animate, and if the player presses enter.
    if(string.len(message) > 0 and Key(Keys.Enter, InputState.Released)) then

      -- Run this for loop for every character in the typed message.
      for i = 1, #message do

        -- Add an object that contains the message character, an initial position offset, and an initial angle offset, to the characters table.
        table.insert(characters, {char = message:sub(i, i), offsetX = 8, offsetY = i * 2, offsetDelta = i * 15})

      end

      -- We need to clear the message string so that after the animation phase is done, the message variable we save to won't have the old characters.
      message = ""

      -- Start the animation phase.
      isAnimating = true

    end

  end

end

function Draw()

  RedrawDisplay()

  -- We'll draw a prompt for the player to know to start typing
  DrawText("Type something!", 2, 0, DrawMode.Tile, "large", 15)

  -- If we're in the animation phase
  if(isAnimating) then

    -- Run this for loop for every character in the typed message
    for i = 1, #characters do

      -- TODO I added this check to see if character is off the bottom of the screen
      if(messageY + characters[i].offsetY < 244) then

        -- Draw the individual character as a sprite, using its position and offset, and choosing a color based on the current index
        DrawText(characters[i].char, messageX + characters[i].offsetX, messageY + characters[i].offsetY, DrawMode.Sprite, "large", i % 15 + 1)

      end

    end

  else

    -- Draw the text that's being typed by the player
    DrawText(message, 16, 16, DrawMode.Sprite, "large", 15)

  end

end
