--[[
  Pixel Vision 8 - New Template Script
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at https://www.gitbook.com/@pixelvision8
]]--

function CreateInputField(rect)

  local data = {}

  data.text = ""
  data.maxChars = math.floor(rect.w / 8)
  -- data.cursorCol = 0
  data.blinkTime = 0
  data.blinkDelay = .4
  data.blink = false
  data.blinkChar = "_"
  data.inputTime = 0
  data.inputDelay = .1
  data.editing = true
  data.rect = rect
  data.spriteSize = {x = 8, y = 8}
  data.actions = {}

  return data

end


function UpdateInputField(data, timeDelta)

  -- Save the timeDelta on the input field
  data.timeDelta = timeDelta / 1000

  if(data.editing == true) then
    -- self:InputAreaKeyCapture(data)

    -- if we are in edit mode, we need to update the cursor blink time
    data.blinkTime = data.blinkTime + data.timeDelta

    if(data.blinkTime > data.blinkDelay) then
      data.blinkTime = 0
      data.blink = not data.blink
    end

    -- Capture the text input from the last frame
    -- local lastInput = InputString()
    --
    -- if(lastInput ~= "") then

    CaptureInput(data)

    -- end

    KeyCapture(data)

  end

  -- Redraw the display
  DrawInputField(data)

end

function DrawInputField(data)

  -- TODO need to add some kind of invalidation so we are not redrawing text constantly

  if(data.blink == true and data.editing == true) then

    local tmpX = data.rect.x + (#data.text * data.spriteSize.x)

    if(tmpX < data.rect.w) then
      DrawText(data.blinkChar, tmpX, data.rect.y, DrawMode.Sprite, "large", 15)
    end

  end

  if(data.invalid) then

    -- Clear the line
    DrawRect(data.rect.x, data.rect.y, data.rect.w, 8, 0, DrawMode.TilemapCache)

    -- Draw the text to the display
    DrawText(data.text, data.rect.x, data.rect.y, DrawMode.TilemapCache, "large", 15)

    data.invalid = false
  end

end

function KeyCapture(data)

  data.inputTime = data.inputTime + data.timeDelta

  if(data.inputTime > data.inputDelay) then

    data.inputTime = 0

    if(Key(Keys.Back)) then

      data.text = data.text:sub(0, #data.text - 1)

      data.invalid = true

    elseif(Key(Keys.Enter)) then

      SubmitText(data)

    end

  end

end

function CaptureInput(data)

  local lastInput = InputString()

  if(lastInput ~= "") then

    -- Loop through each of the characters to insert them
    for i = 1, #lastInput do

      if(#data.text < data.maxChars) then

        local char = lastInput:sub(i, i):lower()

        -- Look for any special characters in the char
        if(char:match("[^%w%s]") == nil) then

          -- Add the text to the display
          data.text = data.text .. char

          -- Invalidate the input field
          data.invalid = true

        end

      end


    end

  end

end

function ClearInputField(data)

  DrawRect(data.rect.x, data.rect.y, data.rect.w, 8, 0, DrawMode.TilemapCache)

  data.text = ""
end

function SubmitText(data)

  if(data.text == "") then
    return
  end

  local words = {}
  for word in data.text:gmatch("%w+") do table.insert(words, word) end

  print("Parse text - total words", #words)

  local actionName = words[1]

  if(data.actions[actionName] ~= nil) then
    print("Found action"..actionName)

    -- Remove the first word
    table.remove(words, 1)

    data.actions[actionName](words)

  end

  ClearInputField(data)

end

function RegisterAction(data, actionName, func)

  data.actions[actionName] = func

end
