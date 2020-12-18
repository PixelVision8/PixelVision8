getPath = function(str)
  return str:match("(.*[/\\])")
end

local filePath = getPath(app.activeSprite.filename)
local counter = 0

if(filePath == nil or filePath == "") then

  app.alert{title = "Error: No File Path", text = "You'll need to save this to the computer first.", buttons = "OK"}

else

  local dlg = Dialog("Export Sprites")
  dlg:entry{ label = "Path", id = "path", text = filePath}


  dlg:separator()

  dlg:check{ id = "useFolder",
    label = "Sub Folder",
    text = "SpriteBuilder",
    selected = true
  }

  dlg:separator()

  dlg:button{ text = "&Cancel", onclick = function() dlg:close() end }
  dlg:button{ text = "&Export", onclick =
    function()

      local path = dlg.data.path

      if(dlg.data.useFolder) then

        path = path .. "Export" .. path:sub(-1)

      end

      Export(path)

      dlg:close()

      app.alert{title = "Export Completed", text = "Exported ".. counter .. " sprites to '"..path.."'."}

    end
  }

  dlg:show{ wait = false}

  function Export(path)
    -- print("Export", path)
    -- local counter = 0
    local sprite = app.activeSprite
    for _, layer in ipairs(sprite.layers) do


      SaveLayer(layer, path)


    end

    SaveSpriteFile(path)

  end

  function SaveSpriteFile(path)

    if #app.sprites < 1 then
      return app.alert "You should have at least one sprite opened"
    end

    local bounds = Rectangle()
    for i,sprite in ipairs(app.sprites) do
      bounds = bounds:union(sprite.bounds)
    end

    local function getTitle(filename)
      return filename:match("^.+/(.+)$")
    end

    local newSprite = Sprite(bounds.width, bounds.height)
    local firstLayer = newSprite.layers[1]
    newSprite:deleteCel(newSprite.cels[1])
    for i,sprite in ipairs(app.sprites) do
      if sprite ~= newSprite then
        while #newSprite.frames < #sprite.frames do
          newSprite:newEmptyFrame()
        end
        local newLayer = newSprite:newLayer()
        newLayer.name = "sprites"
        for j,frame in ipairs(sprite.frames) do
          local cel = newSprite:newCel(newLayer, frame)
          cel.image:drawSprite(sprite, frame)
        end
      end
    end
    newSprite:deleteLayer(firstLayer)
    app.activeFrame = 1

    local cel = newSprite.cels[1]
    cel.image.spec.colorMode = ColorMode.RGB
    cel.image:saveAs(path .. cel.layer.name .. '.png')

    app.command.CloseFile(false)
  end

  function SaveLayer(layer, path)

    if(layer.isVisible == false) then
      return
    end

    if(layer.isGroup) then

      for _, layer in ipairs(layer.layers) do
        SaveLayer(layer, path)
      end

    else

      local cel = layer.cels[1]

      if cel then

        local bounds = cel.bounds
        cel.image.spec.colorMode = ColorMode.RGB

        if(cel.layer.name == "tilemap" or cel.layer.name == "sprites" or cel.layer.name == "colors" or cel.layer.name == "colormap") then
          -- Do nothing
        else
          path = path .. "SpriteBuilder/"
        end

        -- print(cel.layer.name, bounds.width, bounds.height)
        cel.image:saveAs(path .. cel.layer.name .. '.png')
        counter = counter + 1
      end

    end

  end

end
