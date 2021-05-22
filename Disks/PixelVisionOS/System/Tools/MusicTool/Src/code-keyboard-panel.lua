function MusicTool:CreateKeyboardPanel()

    self.blackKeyMap = {
        1, 3, 6, 8, 10, 13, 15, 18, 20, 22
    }

    self.whiteKeyMap = {
        0, 2, 4, 5, 7, 9, 11, 12, 14, 16, 17, 19, 21, 23, 24
    }

    self.keys = {}

    self.keyPositions = {
        {
            rect = {x = 16, y = 184},
            spriteName = "leftwhitekey",
            note = self.whiteKeyMap[1],
            hitRect = self.whiteKeyHitRect,
            key = Keys.Z
        },
        {
            rect = {x = 32, y = 184},
            spriteName = "centerwhitekey",
            note = self.whiteKeyMap[2],
            hitRect = self.whiteKeyHitRect,
            key = Keys.X
        },
        {
            rect = {x = 48, y = 184},
            spriteName = "rightwhitekey",
            note = self.whiteKeyMap[3],
            hitRect = self.whiteKeyHitRect,
            key = Keys.C
        },
        {
            rect = {x = 24, y = 184},
            spriteName = "blackkey",
            note = self.blackKeyMap[1],
            hitRect = self.blackKeyHitRect,
            key = Keys.S
        },
        {
            rect = {x = 40, y = 184},
            spriteName = "blackkey",
            note = self.blackKeyMap[2],
            hitRect = self.blackKeyHitRect,
            key = Keys.D
        },

        {
            rect = {x = 64, y = 184},
            spriteName = "leftwhitekey",
            note = self.whiteKeyMap[4],
            hitRect = self.whiteKeyHitRect,
            key = Keys.V
        },
        {
            rect = {x = 80, y = 184},
            spriteName = "centerwhitekey",
            note = self.whiteKeyMap[5],
            hitRect = self.whiteKeyHitRect,
            key = Keys.B
        },
        {
            rect = {x = 96, y = 184},
            spriteName = "centerwhitekey",
            note = self.whiteKeyMap[6],
            hitRect = self.whiteKeyHitRect,
            key = Keys.N
        },
        {
            rect = {x = 112, y = 184},
            spriteName = "rightwhitekey",
            note = self.whiteKeyMap[7],
            hitRect = self.whiteKeyHitRect,
            key = Keys.M
        },
        {
            rect = {x = 72, y = 184},
            spriteName = "blackkey",
            note = self.blackKeyMap[3],
            hitRect = self.blackKeyHitRect,
            key = Keys.G
        },
        {
            rect = {x = 88, y = 184},
            spriteName = "blackkey",
            note = self.blackKeyMap[4],
            hitRect = self.blackKeyHitRect,
            key = Keys.H
        },
        {
            rect = {x = 104, y = 184},
            spriteName = "blackkey",
            note = self.blackKeyMap[5],
            hitRect = self.blackKeyHitRect,
            key = Keys.J
        },

        -- next octave

        {
            rect = {x = 16 + 112, y = 184},
            spriteName = "leftwhitekey",
            note = self.whiteKeyMap[8],
            hitRect = self.whiteKeyHitRect,
            key = Keys.Q
        },
        {
            rect = {x = 32 + 112, y = 184},
            spriteName = "centerwhitekey",
            note = self.whiteKeyMap[9],
            hitRect = self.whiteKeyHitRect,
            key = Keys.W
        },
        {
            rect = {x = 48 + 112, y = 184},
            spriteName = "rightwhitekey",
            note = self.whiteKeyMap[10],
            hitRect = self.whiteKeyHitRect,
            key = Keys.E
        },
        {
            rect = {x = 24 + 112, y = 184},
            spriteName = "blackkey",
            note = self.blackKeyMap[6],
            hitRect = self.blackKeyHitRect,
            key = Keys.D2
        },
        {
            rect = {x = 40 + 112, y = 184},
            spriteName = "blackkey",
            note = self.blackKeyMap[7],
            hitRect = self.blackKeyHitRect,
            key = Keys.D3
        },

        {
            rect = {x = 64 + 112, y = 184},
            spriteName = "leftwhitekey",
            note = self.whiteKeyMap[11],
            hitRect = self.whiteKeyHitRect,
            key = Keys.R
        },
        {
            rect = {x = 80 + 112, y = 184},
            spriteName = "centerwhitekey",
            note = self.whiteKeyMap[12],
            hitRect = self.whiteKeyHitRect,
            key = Keys.T
        },
        {
            rect = {x = 96 + 112, y = 184},
            spriteName = "centerwhitekey",
            note = self.whiteKeyMap[13],
            hitRect = self.whiteKeyHitRect,
            key = Keys.Y
        },
        {
            rect = {x = 112 + 112, y = 184},
            spriteName = "rightwhitekey",
            note = self.whiteKeyMap[14],
            hitRect = self.whiteKeyHitRect,
            key = Keys.U
        },
        {
            rect = {x = 72 + 112, y = 184},
            spriteName = "blackkey",
            note = self.blackKeyMap[8],
            hitRect = self.blackKeyHitRect,
            key = Keys.D5
        },
        {
            rect = {x = 88 + 112, y = 184},
            spriteName = "blackkey",
            note = self.blackKeyMap[9],
            hitRect = self.blackKeyHitRect,
            key = Keys.D6
        },
        {
            rect = {x = 104 + 112, y = 184},
            spriteName = "blackkey",
            note = self.blackKeyMap[10],
            hitRect = self.blackKeyHitRect,
            key = Keys.D7
        },
        {
            rect = {x = 240, y = 184},
            spriteName = "lastwhitekey",
            note = self.whiteKeyMap[15],
            hitRect = self.whiteKeyHitRect,
            key = Keys.I -- TODO this needs to be mapped correctly?
        },
    }
  
      for i = 1, #self.keyPositions do
  
        local data = self.keyPositions[i]
        -- Create keyboard
        local keyButtonData = editorUI:CreateButton(data.rect, data.spriteName, "Midi note '".. data.note .."'.")
        keyButtonData.inputKey = data.key
  
        table.insert(self.disableWhenPlaying, keyButtonData)
  
        keyButtonData.hitRect = {x = keyButtonData.rect.x + data.hitRect.x, y = keyButtonData.rect.y + data.hitRect.y, w = data.hitRect.w, h = data.hitRect.h}
  
        keyButtonData.onAction = function()

            pixelVisionOS:BeginUndo(self)

            self:SetMidiNote(data.note)

            pixelVisionOS:EndUndo(self)

        end
  
        table.insert(self.keys, keyButtonData)
  
      end

      -- keyboard
    self.previousOctaveButtonData = editorUI:CreateButton({x = 0, y = 200 + 8}, "leftoctave", "Move up an octave.")
    self.previousOctaveButtonData.onAction = function(value) self:PreviousOctave(value) end

    table.insert(self.disableWhenPlaying, self.previousOctaveButtonData)

    self.nextOctaveButtonData = editorUI:CreateButton({x = 0, y = 176 + 8}, "rightoctave", "Move down an octave.")
    self.nextOctaveButtonData.onAction = function(value) self:NextOctave(value) end

    table.insert(self.disableWhenPlaying, self.nextOctaveButtonData)

    pixelVisionOS:RegisterUI({name = "UpdateKeyboardPanelLoop"}, "UpdateKeyboardPanel", self)
  
  end
  
  function MusicTool:UpdateKeyboardPanel()
  
    editorUI:UpdateButton(self.previousOctaveButtonData)
    editorUI:UpdateButton(self.nextOctaveButtonData)

    -- Draw note octave
    DrawText("C" .. self.octave, 16 + 2, 184, DrawMode.UI, "small", 12, - 4)
    DrawText("C" .. (self.octave + 1), 128 + 2, 184, DrawMode.UI, "small", 12, - 4)
    DrawText("C" .. (self.octave + 2), 240 + 2, 184, DrawMode.UI, "small", 12, - 4)

    local controlDown = (Key(Keys.LeftControl) == true or Key(Keys.RightControl) == true)

    local total = #self.keys
    
    for i = 1, total do
      local key = self.keys[i]

      editorUI:UpdateButton(key)

      if(key.inputKey ~= nil and controlDown == false and editorUI.editingInputField == false) then
        if(Key(key.inputKey, InputState.Released)) then
          editorUI:Invalidate(key)
          key.onAction()
        elseif(Key(key.inputKey)) then
          editorUI:RedrawButton(key, "over")
        end
      end

    end

  end

   -- Select the next set of octaves
function MusicTool:NextOctave()
    local nextID = self.octave + 1
  
    if(nextID > self.octaveRange.y) then
      nextID = self.octaveRange.x
    end
  
    self:SelectOctave(nextID)
  end
  
  -- Select the previous set of octaves
  function MusicTool:PreviousOctave()
    local nextID = self.octave - 1
  
    if(nextID < self.octaveRange.x) then
      nextID = self.octaveRange.y
    end
  
    self:SelectOctave(nextID)
  end