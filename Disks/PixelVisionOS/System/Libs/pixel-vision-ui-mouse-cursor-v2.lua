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

MouseCursor = {}
MouseCursor.__index = MouseCursor

-- TODO this should be set up like all of the other UI components and not its own object
function MouseCursor:Init()

    -- Create a new object for the instance and register it
    local _mouseCursor = {
        cursorID = -1,
        animationTime = 0,
        animationDelay = .2,
        animationFrame = 0,
        lock = false,
        pos = NewPoint(-1, -1)
    }

    setmetatable(_mouseCursor, MouseCursor)

    -- This defines which set of data to use when drawing the cursor
    
    -- Reference data for each of the different mouse cursors
    _mouseCursor.cursors = {
        -- Pointer 1
        {
            spriteData = "cursorpointer",
            offset = {
                x = 0,
                y = -1
            }
        },
        -- Hand (for interaction) 2
        {
            spriteData = "cursorhand",
            offset = {
                x = -6,
                y = -1
            }
        },
        -- Input 3
        {
            spriteData = "cursortext",
            offset = {
                x = -4,
                y = -8
            }
        },

        -- Help (for showing tool tips) 4
        {
            spriteData = "cursorhelp",
            offset = {
                x = -2,
                y = -3
            }
        },
        -- Wait 5
        {
            spriteData = "cursorwait1",
            offset = {
                x = -2,
                y = -3
            },
            animated = true,
            frames = 10,
            spriteName = "cursorwait1"
        },
        -- Pencil 6
        {
            spriteData = "cursorpen",
            offset = {
                x = 0,
                y = -15
            }
        },
        -- Eraser 7
        {
            spriteData = "cursoreraser",
            offset = {
                x = 0,
                y = -15
            }
        },
        -- Cross 8
        {
            spriteData = "cursorcross",
            offset = {
                x = -8,
                y = -8
            }
        },
        -- Move Hand 9
        {
            spriteData = "cursorhandmove",
            offset = {
                x = -8,
                y = -8
            }
        },
    }


    _mouseCursor:SetCursor(1)
    -- Return the new instance of the editor ui
    
    return _mouseCursor

end

function MouseCursor:Update(timeDelta, collisionState)

    -- save the current mouse position
    self.pos.x = collisionState.mousePos.x
    self.pos.y = collisionState.mousePos.y

    if(self.cursorData ~= nil and self.cursorData.animated == true) then

        self.animationTime = self.animationTime + timeDelta

        if(self.animationTime > self.animationDelay) then

            self.animationTime = 0
            self.animationFrame = Repeat(self.animationFrame, self.cursorData.frames - 1) + 1

            self.cursorData.spriteData = _G[self.cursorData.spriteName .. tostring(self.animationFrame)]

        end

    end
    
end

function MouseCursor:Draw()

    -- Need to make sure the mouse is not off screen before drawing it
    if(self.pos.x < 0 or self.pos.y < 0) then
        return
    end

    -- Make sure the data isn't undefined
    if(self.cursorData ~= nil) then

        DrawMetaSprite(
            FindMetaSpriteId((self.cursorID == 2 and MouseButton(0)) and "cursorhanddown" or self.cursorData.spriteData),
            self.pos.x + self.cursorData.offset.x,
            self.pos.y + self.cursorData.offset.y,
            false,
            false,
            10 -- This forces a layer higher than the default DrawModes
        )
           
    end

end

function MouseCursor:SetCursor(id, lock)

    -- Check for unlock flag
    if(lock == false) then
        self.lock = false
    end

    if(self.cursorID ~= id and self.lock ~= true) then

        self.lock = lock

        self.cursorID = id

        self.animationTime = 0
        self.animationFrame = 1

        -- get the current sprite data for the current cursor
        self.cursorData = self.cursors[self.cursorID]

    end

end
