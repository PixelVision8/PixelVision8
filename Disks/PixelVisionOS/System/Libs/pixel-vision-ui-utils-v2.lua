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

local absindex = function(len, i)
  return i < 0 and (len + i + 1) or i
end

string.starts = function(String, Start)
  return string.sub(String, 1, string.len(Start)) == Start
end

string.ends = function(String, End)
  return End == '' or string.sub(String, - string.len(End)) == End
end

table.clone = function(src)
  return {table.unpack(src)}
end

string.rpad = function(str, len, char)
  if char == nil then char = ' ' end
  return str .. string.rep(char, len - #str)
end

string.lpad = function(str, len, char)
  if char == nil then char = ' ' end
  return string.rep(char, len - #str) .. str
end

string.trunc = function(str, len, char)

  if char == nil then char = '...' end
  if(#str > len) then
    str = string.sub(str, 1, len - #char) .. char
  end

  return str
end

string.split = function(string, delimiter)
  if delimiter == nil then
    delimiter = "%s"
  end
  local t = {} ; i = 1
  for str in string.gmatch(string, "([^"..delimiter.."]+)") do
    t[i] = str
    i = i + 1
  end
  return t
end

function math.round(num, numDecimalPlaces)
  local mult = 10^(numDecimalPlaces or 0)
  return math.floor(num * mult + 0.5) / mult
end

function math.index(x, y, width)
  return x + y * width
end

function math.pos(index, width)
  return index % width, math.floor(index / width)
end

function table.find(f, l) -- find element v of l satisfying f(v)
  for _, v in ipairs(l) do
    if f(v) then
      return v
    end
  end
  return nil
end

table.indexOf = function( t, object )
  if "table" == type( t ) then
    for i = 1, #t do
      if object == t[i] then
        return i
      end
    end
    return - 1
  else
    error("table.indexOf expects table for first argument, " .. type(t) .. " given")
  end
end

function dump(o)
  if type(o) == 'table' then
    local s = '{ '
    for k, v in pairs(o) do
      if type(k) ~= 'number' then k = '"'..k..'"' end
      s = s .. '['..k..'] = ' .. dump(v) .. ','
    end
    return s .. '} '
  else
    return tostring(o)
  end
end

function ExplodeSettings(str)
  local div = ","

  local pos, arr = 0, {}
  for st, sp in function() return string.find(str, div, pos, true) end do
    table.insert(arr, tonumber(string.sub(str, pos, st - 1)))
    pos = sp + 1
  end
  table.insert(arr, tonumber(string.sub(str, pos)))
  return arr
end

function PrintSprite(pixels, colorOffset, w, h)

  w = w or 8
  h = h or 8
  colorOffset = colorOffset or 0

  print("Sprite " .. string.format("%02d", w).. " x " .. string.format("%02d", h))

  for r = 1, h+1 do
    local row = ""
    
    for c = 1, w do
      if(r > 1) then
        -- print(c, r, (c-1) + (r-2) * w)
        local pixel = pixels[((c-1) + (r-2) * w) + 1] 
        -- print("px", dump(pixels))
        row = row .. (pixel == -1 and "-1" or string.format("%02d", (pixel + colorOffset))) .. "  "
      else
        row = row .. "C" .. string.format("%02d", c) .. " "
      end
    end

    if(r > 1) then
      print("R" .. string.format("%02d", r-1) .. "   " .. row)
    else
      print("     " .. row)
    end

  end

end
