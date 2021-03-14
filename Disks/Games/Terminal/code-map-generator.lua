--[[
  Pixel Vision 8 - New Template Script
  Based on TMaze v0.5 by AndyGFX
  Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
  Created by Jesse Freeman (@jessefreeman)

  This project was designed to display some basic instructions when you create
  a new game.  Simply delete the following code and implement your own Init(),
  Update() and Draw() logic.

  Learn more about making Pixel Vision 8 games at https://www.gitbook.com/@pixelvision8
]]--

function NewMap(w, h)

  local data = {
    width = w,
    height = h,
    maze = {}
  }

  ClearMap(data)

  GenerateMap(data)

  CleanUpMap(data)

  FindRooms(data)

  MapToString(data)

  print("Raw Map\n" ..dump(data.roomIndex))

  return data

end



function ClearMap(data)

  for i = 1, data.height, 1 do
    data.maze[i] = {}
    for j = 1, data.width, 1 do
      data.maze[i][j] = {}
      data.maze[i][j].left = 0
      data.maze[i][j].right = 0
      data.maze[i][j].up = 0
      data.maze[i][j].down = 0
      data.maze[i][j].visited = 0
    end
  end
end

function GenerateMap(data)
  -- ClearMap(data)

  for i = 1, data.height do
    for j = 1, data.width do
      if ((i == data.height) and (j == data.width)) then
        -- do nothing
      elseif (i == data.height) then
        data.maze[i][j].right = 1
        data.maze[i][j + 1].left = 1
      elseif (j == data.width) then
        data.maze[i][j].down = 1
        data.maze[i + 1][j].up = 1
      elseif (math.floor(math.random(0, 2)) == 0) then
        data.maze[i][j].right = 1
        data.maze[i][j + 1].left = 1
      else
        data.maze[i][j].down = 1
        data.maze[i + 1][j].up = 1
      end
    end
  end
end

function CleanUpMap(data)

  -- Fix the size of the map
  data.width = (data.width * 2) + 1
  data.height = (data.height * 2) + 1

  data.mapTable = {}

  for i = 1, data.width * data.height do
    table.insert(data.mapTable, i, 0)
  end

  for i = 0, #data.maze * 2, 1 do
    data.mapTable[CalculateIndex(0, i, data.width) + 1] = 1
  end

  for j = 1, #data.maze[1] * 2, 1 do
    data.mapTable[CalculateIndex(j, 0, data.height) + 1] = 1
  end

  for i = 1, #data.maze do
    for j = 1, #data.maze[1] do

      data.mapTable[ CalculateIndex(j * 2, i * 2, data.width) + 1] = 1

      if data.maze[i][j].right == 0 then
        data.mapTable[CalculateIndex(j * 2, (i * 2) - 1, data.width) + 1] = 1
      end
      if data.maze[i][j].down == 0 then
        data.mapTable[CalculateIndex((j * 2) - 1, i * 2, data.width) + 1] = 1
      end
    end
  end

end

function MapToString(data)

  data.mapString = ""

  for i = 1, #data.mapTable do

    local tmpValue = data.mapTable[i]

    data.mapString = data.mapString .. (tmpValue == 1 and "  " or string.lpad(tostring(i), 2, "0"))

    data.mapString = data.mapString .. " "

    if(i % data.width == 0) then
      data.mapString = data.mapString .. "\n"
    end

  end

end

function FindRooms(data)

  data.roomIndex = {}

  for i = 1, #data.mapTable do
    local roomValue = data.mapTable[i]
    if(roomValue == 0) then

      local pos = CalculatePosition(i, data.width)

      local room = {
        id = i,
        pos = pos
      }

      local tmpValue = 1
      local tmpIndex = -1
      -- Top

      tmpIndex = CalculateIndex(pos.x, pos.y - 1, data.width)
      if(data.mapTable[tmpIndex] == 0) then
        room.north = tmpIndex
      end

      -- Right

      tmpIndex = CalculateIndex(pos.x + 1, pos.y, data.width)
      if(data.mapTable[tmpIndex] == 0) then
        room.east = tmpIndex
      end

      -- Bottom

      tmpIndex = CalculateIndex(pos.x, pos.y + 1, data.width)
      if(data.mapTable[tmpIndex] == 0) then
        room.south = tmpIndex
      end

      -- Left

      tmpIndex = CalculateIndex(pos.x - 1, pos.y, data.width)
      if(data.mapTable[tmpIndex] == 0) then
        room.west = tmpIndex
      end

      table.insert(data.roomIndex, room)

    end

  end


end

function PreviewMap(data, ox, oy)
  white = 7
  for i = 0, #data.maze * 2, 1 do DrawPixels({14}, ox + 0, oy + i, 1, 1);end
  for j = 1, #data.maze[1] * 2, 1 do DrawPixels({14}, ox + j, oy + 0, 1, 1); end

  for i = 1, #data.maze do
    for j = 1, #data.maze[1] do
      DrawPixels({14}, ox + j * 2, oy + i * 2, 1, 1);
      if data.maze[i][j].right == 0 then
        DrawPixels({14}, ox + j * 2, oy + i * 2 - 1, 1, 1);
      end
      if data.maze[i][j].down == 0 then
        DrawPixels({14}, ox + j * 2 - 1, oy + i * 2, 1, 1);
      end
    end
  end
end
