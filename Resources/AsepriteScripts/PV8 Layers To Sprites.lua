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

        path = path .. "SpriteBuilder" .. path:sub(-1)

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
        -- print(cel.layer.name, bounds.width, bounds.height)
        cel.image:saveAs(path .. cel.layer.name .. '.png')
        counter = counter + 1
      end

    end

  end

end
