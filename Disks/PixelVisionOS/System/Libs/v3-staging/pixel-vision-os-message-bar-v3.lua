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
    delay = 0,
    time = 0,
    maxChars = maxChars,
    currentMessage = "",
    mode = 1,
    modes = {
      Empty = 1,
      Message = 2,
      Help = 3
    }
  }

  -- Create a text label to display the message with
  data.messageTextData = self.editorUI:CreateText({x = x, y = y}, "", self.editorUI.theme.message.font, self.editorUI.theme.message.spacing, DrawMode.TilemapCache, clearColorID or BackgroundColor())

  return data

end

function PixelVisionOS:UpdateMessageBar(data)

  self.editorUI:UpdateText(data.messageTextData)

  if(data.delay == -1) then

    if(self.editorUI.collisionManager.hovered == -1) then
      -- If _messageBar delay is set to 0 then clear it
      if(data.currentMessage ~= "") then
        self:ClearMessage(data)
        data.delay = 0
        self.editorUI:UpdateText(data.messageTextData)
      end
    end

  elseif(data.delay > 0) then

    data.time = data.time + self.editorUI.timeDelta


    if(data.time > data.delay) then
      self:ClearMessage(data)
    end
  end
  self.editorUI:UpdateText(data.messageTextData)

end

function PixelVisionOS:DisplayMessage(text, delay, data)

  -- My default, we want to use the built in message bar but can accept an alternative one
  data = data or self.messageBar

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

  local length = data.maxChars - #data.currentMessage + 1

  -- Set the text and pad it with spaces to make sure it always clears correctly
  self.editorUI:ChangeText(data.messageTextData, string.rpad(string.upper(data.currentMessage), data.maxChars, " "))

end

function PixelVisionOS:DisplayToolTip(text, override, data)

  -- My default, we want to use the built in message bar but can accept an alternative one
  data = data or self.messageBar

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
