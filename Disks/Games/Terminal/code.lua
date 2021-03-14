--[[
  Pixel Vision 8 - New Template Script
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at https://www.gitbook.com/@pixelvision8
]]--

LoadScript("code-utils")
LoadScript("code-display-text")
LoadScript("code-simple-input-field")
LoadScript("code-map-generator")
LoadScript("code-room-decorator")
LoadScript("code-message-bar")
LoadScript("code-inventory")
LoadScript("code-combat")

-- This this is an empty game, we will the following text. We combined two sets of fonts into
-- the default.font.png. Use uppercase for larger characters and lowercase for a smaller one.
local message = ">TERMINAL\n\n\nTerminal is the empty shell of a text adventure game I am going to try to build in 48 hours, maybe 72 if I run into problems completing the compo.\n\n\nVisit 'pixelvision8.com' to learn about my Fantasy Console."

-- Holder for the input field
local inputField = nil
local textDisplay = nil

-- This is the default game state. It is empty at start with gameActive set to false
local gameState = {
  gameActive = false
}

-- The Init() method is part of the game's lifecycle and called a game starts. We are going to
-- use this method to configure background color, ScreenBufferChip and draw a text box.
function Init()

  -- Here we are manually changing the background color
  BackgroundColor(0)

  inputField = CreateInputField({x = 8, y = 224, w = 240})

  textDisplay = CreateTextDisplay({x = 8, y = 8, w = 240, h = 200})


  RegisterAction(inputField, "open", Open)
  RegisterAction(inputField, "close", Close)

  RegisterAction(inputField, "start", Start)

  -- Register go
  RegisterAction(inputField, "go",
    function(options)
      Go(options, gameState)
    end
  )
  RegisterAction(inputField, "take",
    function(options)

      if(gameState.gameActive == false) then
        DisplayMessage("Start a game first.")
      elseif(gameState.mapOpen == true) then
        DisplayMessage("Close map first.")
      elseif(gameState.terminalOpen == true) then
        DisplayMessage("Close the terminal first.")
      elseif(#options > 0) then
        Take(options, gameState)
      end

    end
  )
  RegisterAction(inputField, "examine",
    function(options)

      if(gameState.gameActive == false) then
        DisplayMessage("Start a game first.")
      elseif(gameState.mapOpen == true) then
        DisplayMessage("Close map first.")
      elseif(gameState.terminalOpen == true) then
        DisplayMessage("Close the terminal first.")
      elseif(#options > 0) then
        Examine(options, gameState)
      end

    end
  )

  RegisterAction(inputField, "use",
    function(options)

      if(gameState.gameActive == false) then
        DisplayMessage("Start a game first.")
      elseif(gameState.mapOpen == true) then
        DisplayMessage("Close map first.")
      elseif(gameState.terminalOpen == true) then
        DisplayMessage("Close the terminal first.")
      elseif(#options > 0) then
        Use(options, gameState)
      end

    end
  )

  RegisterAction(inputField, "look",
    function(options)

      if(gameState.gameActive == false) then
        DisplayMessage("Start a game first.")
      elseif(gameState.mapOpen == true) then
        DisplayMessage("Close map first.")
      elseif(gameState.terminalOpen == true) then
        DisplayMessage("Close the terminal first.")
      elseif(#options > 0) then
        Look(options, gameState)
      end

    end
  )


  FirstScreen()

end

-- The Update() method is part of the game's life cycle. The engine calls Update() on every frame
-- before the Draw() method. It accepts one argument, timeDelta, which is the difference in
-- milliseconds since the last frame.
function Update(timeDelta)

  if(textDisplay.drawText == true) then

    UpdateTextDisplay(textDisplay, timeDelta)

  elseif(messageText ~= "") then

    UpdateMessage(timeDelta)

  else

    -- If the display is not drawing text, update the input field
    UpdateInputField(inputField, timeDelta)

  end

end

-- The Draw() method is part of the game's life cycle. It is called after Update() and is where
-- all of our draw calls should go. We'll be using this to render sprites to the display.
function Draw()

  RedrawDisplay()

  DrawTextDisplay(textDisplay)

  -- PreviewMap(map, 10, 150)

end

function FirstScreen()

  -- Clear the display
  ClearDisplay()

  -- Clear the input field
  ClearInputField(inputField)

  -- Draw text to the display
  DisplayText(textDisplay, message)

end

function ClearDisplay()
  -- DrawRect(0, 0, 256, 240, 0, DrawMode.TilemapCache)
end

function ShowMap()

  gameState.mapOpen = true

  -- Clear the display
  ClearDisplay()

  local mapString = "   MAP           STATS\n\n"
  local rooms = gameState.map.roomIndex

  local totalTiles = gameState.map.width * gameState.map.height
  local totalRooms = #rooms
  local statCounter = 1

  for i = 1, totalTiles do

    local roomValue = "#"

    for j = 1, totalRooms do

      local room = rooms[j]

      if(room.id == i) then

        if(room.id == gameState.currentRoom.id) then
          roomValue = "@"
        elseif(room.lastRoom == true) then
          roomValue = "X"
        else
          roomValue = room.visited == true and " " or "#"
        end

      end

    end

    mapString = mapString .. roomValue


    if(i % gameState.map.width == 0) then

      if(statCounter <= #gameState.stats) then
        mapString = mapString .. "         " .. gameState.stats[statCounter].name:upper() .. " " .. string.lpad(tostring(gameState.stats[statCounter].value), 2, "0")
        statCounter = statCounter + 1
      end

      mapString = mapString .. "\n"
    end

  end

  mapString = mapString .. "\n\nINVENTORY\n\n"

  for i = 1, #gameState.inventory do
    mapString = mapString .. string.lpad(tostring(i), 2, "0") .. " " .. gameState.inventory[i].name:upper() .. "\n"
  end

  -- Draw text to the display
  DisplayText(textDisplay, mapString, true)

end

function Open(options)

  if(gameState.gameActive == true) then
    local action = options[1]

    if(action == "map" and gameState.mapOpen == false) then
      ShowMap()
      return
    elseif(action == "terminal" and gameState.currentRoom.terminal ~= nil) then
      ShowTerminal()
      return
    end

    -- TODO need to test if the room has anything to open


  end
  -- TODO return error
  DisplayMessage("You can not open that.")

end

function Close(options)

  if(gameState.gameActive == true) then

    local action = options[1]

    if(action == "map" and gameState.mapOpen == true) then

      gameState.mapOpen = false
      EnterRoom(gameState.currentRoom)

      return
    elseif(action == "terminal" and gameState.terminalOpen == true) then

      gameState.terminalOpen = false

      EnterRoom(gameState.currentRoom)

      return

    end

  end

  -- TODO return error
  DisplayMessage("You can not close that.")


end

function ShowTerminal()

  gameState.terminalOpen = true

  gameState.DisplayText(gameState.currentRoom.terminal)

end

function Start()

  if(gameState.gameActive == true) then
    return
  end

  -- Create game state
  gameState = {}
  gameState.gameActive = true
  -- Configure game
  gameState.map = nil
  gameState.currentRoom = nil
  gameState.inventory = {}
  gameState.stats = {
    {name = "health", value = 0},
    {name = "rooms ", value = 0},
    {name = "steps ", value = 0},
    {name = "kills ", value = 0},
    {name = "saved ", value = 0},
  }
  gameState.debug = false
  gameState.mapOpen = false

  -- Create map
  gameState.map = NewMap(3, 2)
  DecorateRooms(gameState.map)

  -- This exposes the display text to any function referencing the game state
  gameState.DisplayText = function(text)

    -- Clear the display first
    ClearDisplay()

    -- Display text
    DisplayText(textDisplay, text)

  end

  -- Expose the display message function to the game state
  gameState.DisplayMessage = function(text, delay)

    DisplayMessage(text, delay)

  end


  -- Enter the first room
  EnterRoom(gameState.map.firstRoom)

end

function EnterRoom(room)

  if(gameState.currentRoom ~= nil) then
    -- Flag the room we are leaving as visited
    gameState.currentRoom.visited = true
  end

  gameState.currentRoom = room

  -- Display room title and message
  local message = gameState.currentRoom.title .. "\n\n\n" ..
  GetMessage(gameState.currentRoom)

  -- Draw text to the display and clear the previous text
  DisplayText(textDisplay, message, true)

end

function Go(options)



  if(gameState.gameActive == true and gameState.mapOpen == false) then

    local validDirections = {"north", "east", "south", "west"}

    local direction = options[1]

    if(table.indexOf(validDirections, direction ) ~= -1) then

      local roomID = gameState.currentRoom[direction]

      -- find the room
      for i = 1, #gameState.map.roomIndex do
        if(gameState.map.roomIndex[i].id == roomID) then
          EnterRoom(gameState.map.roomIndex[i])

          return
        end
      end

    end

  end

  if(gameState.gameActive == false) then
    DisplayMessage("Start a game first.")
  elseif(gameState.mapOpen == true) then
    DisplayMessage("Close the map before moving.")
  else
    DisplayMessage("You can't go that way.")
  end

end

function Look(options, state)

  if(options[1] == "at") then

    if(state.currentRoom.itemsOfInterest ~= nil and #options > 1) then
      for i = 1, #state.currentRoom.itemsOfInterest do
        if(state.currentRoom.itemsOfInterest[i].name == options[2]) then
          DisplayText(textDisplay, state.currentRoom.itemsOfInterest[i].message)
          return
        end
      end
    end

  elseif(options[1] == "around") then

    -- TODO should tell the player about items and itemsOfInterest

    local message = GetMessage(gameState.currentRoom)

    -- Draw text to the display and clear the previous text
    DisplayText(textDisplay, message)
    return

  end

  DisplayMessage("You can't look at that.")

end
