-- This example shows how to create a new sprite stacking all the
-- other sprites each in one layer.

if #app.sprites < 1 then
  return app.alert "You should have at least one sprite opened"
end

local result = app.alert{ title = "Add Sprites To Layers",
  text = "Do you want to add all of the open sprites to this document?",
buttons = {"Cancel", "No", "Yes"}}

if result == 1 then
  return
end


local bounds = Rectangle()
for i, sprite in ipairs(app.sprites) do
  bounds = bounds:union(sprite.bounds)
end

local function getTitle(filename)
  return filename:match("^.+\\(.+)$"):sub(0, - 5)
end

local newSprite = nil
local firstLayer = nil

if(result == 2) then
  newSprite = Sprite(bounds.width, bounds.height)
  firstLayer = newSprite.layers[1]
  newSprite:deleteCel(newSprite.cels[1])

elseif(result == 3) then

  newSprite = app.activeSprite

end
local counter = 0

for i, sprite in ipairs(app.sprites) do

  if sprite ~= newSprite then
    while #newSprite.frames < #sprite.frames do
      newSprite:newEmptyFrame()
    end
    local newLayer = newSprite:newLayer()
    newLayer.name = getTitle(sprite.filename)

    -- print("Copy", newLayer.name, sprite.bounds.width, sprite.bounds.height)
    for j, frame in ipairs(sprite.frames) do
      local cel = newSprite:newCel(newLayer, frame, Image(sprite.bounds.width, sprite.bounds.height))
      cel.image:drawSprite(sprite, frame)
      counter = counter + 1
    end
  end
end

if(firstLayer ~= nil) then newSprite:deleteLayer(firstLayer) end

app.activeFrame = 1

app.alert ("Added '".. counter .."' sprites to '"..app.activeSprite.filename.."'.")
