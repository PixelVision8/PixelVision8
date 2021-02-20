--
-- Copyright (c) 2017, Jesse Freeman. All rights reserved.
--
-- Licensed under the Microsoft Public License (MS-PL) License.
-- See LICENSE file in the project root for full license information.
--
-- Contributors
-- --------------------------------------------------------
-- This is the official list of Pixel Vision 8 contributors:
--
-- Jesse Freeman - @JesseFreeman
-- Christina-Antoinette Neofotistou - @CastPixel
-- Christer Kaitila - @McFunkypants
-- Pedro Medeiros - @saint11
-- Shawn Rakowski - @shwany
--

function PixelVisionOS:CreateMessageBar(x, y, maxChars, clearColorID)

  local data = {
    textColorOffset = 15,
    font = "medium",
    delay = 0,
    time = 0,
    pos = {x = x or 0, y = y or 0},
    maxChars = maxChars,
    offset = -4,
    currentMessage = "",
    mode = 1,
    invalid = false,
    clearColorID = clearColorID or 5,
    modes = {
      Empty = 1,
      Message = 2,
      Help = 3
    },
    length = 0
  }

  data.textDrawArgs = {
    "",
    data.pos.x,
    data.pos.y,
    DrawMode.TilemapCache,
    data.font,
    data.textColorOffset,
    data.offset
  }

  data.clearDrawArgs = {
    data.pos.x,
    data.pos.y,
    data.maxChars * 4,
    8,
    data.clearColorID,
    DrawMode.TilemapCache
  }

  return data

end

function PixelVisionOS:UpdateMessageBar(data)

  if(data.delay == -1) then

    if(editorUI.collisionManager.hovered == -1) then
      -- If _messageBar delay is set to 0 then clear it
      if(data.currentMessage ~= "") then
        self:ClearMessage(data)
        data.delay = 0
      end
    end

    return;
  end

  if(data.delay == 0) then
    return
  end

  data.time = data.time + editorUI.timeDelta

  if(data.delay > 0) then
    if(data.time > data.delay) then
      self:ClearMessage(data)
    end
  end

end

-- Unlike other components, the message bar is manually drawn last since its message can be updated outside of the update loop by other components
function PixelVisionOS:DrawMessageBar(data)
  if(data.invalid == true) then
    
    self.length = data.maxChars - #data.currentMessage + 1

    if(self.length < 0) then
      self.length = 0
    end

    data.textDrawArgs[1] = string.upper(data.currentMessage)

    editorUI:NewDraw("DrawRect", data.clearDrawArgs)
    editorUI:NewDraw("DrawText", data.textDrawArgs)

    editorUI:ResetValidation(data)
  end
end

function PixelVisionOS:DisplayMessage(text, delay, data)

  -- My default, we want to use the built in message bar but can accept an alternative one
  data = data or self.messageBar

  -- If no messageBar exists, exit out of the method
  if(data == nil) then
    return
  end

  if(text == data.currentMessage) then
    return
  end

  if(#text > data.maxChars) then
    text = string.sub(text, 1, data.maxChars)
  end

  data.currentMessage = text

  data.delay = delay or 2
  data.time = 0
  data.mode = data.modes.Message
  editorUI:Invalidate(data)

end


function PixelVisionOS:DisplayToolTip(text, override, data)

  -- My default, we want to use the built in message bar but can accept an alternative one
  data = data or self.messageBar

  -- If no messageBar exists, exit out of the method
  if(data == nil) then
    return
  end

  if(text == data.currentMessage) then
    return
  end

  if(data.delay <= 0 or override == true) then
    self:DisplayMessage(text, - 1, data)
    data.mode = data.modes.Help
  end
end

function PixelVisionOS:ClearMessage(data)

  -- My default, we want to use the built in message bar but can accept an alternative one
  data = data or self.messageBar

  -- If no message bar exists, exit out of the method
  if(data == nil) then
    return
  end

  self:DisplayMessage("", 0, data)
  data.mode = data.modes.Empty
end

function PixelVisionOS:ClearToolTip(data)

  -- My default, we want to use the built in message bar but can accept an alternative one
  data = data or self.messageBar

  if(data.mode == data.modes.Help) then
    self:ClearMessage(data)
  end

end
