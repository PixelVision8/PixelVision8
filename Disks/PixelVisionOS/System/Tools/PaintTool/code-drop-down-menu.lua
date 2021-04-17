--[[
	Pixel Vision 8 - Draw Tool
	Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
	Created by Jesse Freeman (@jessefreeman)

	Please do not copy and distribute verbatim copies
	of this license document, but modifications without
	distributing is allowed.
]]--

NewShortcut, SaveShortcut, CutShortcut, CopyShortcut, PasteShortcut = 3, 10, 6, 7, 8

function PaintTool:CreateDropDownMenu()

  local menuOptions = 
  {
      -- About ID 1
      {name = "About", action = function() pixelVisionOS:ShowAboutModal(self.toolName) end, toolTip = "Learn about PV8."},

      {divider = true},
      {name = "Undo", action = function() self:OnUndo() end, key = Keys.Z, toolTip = "Learn about PV8."},
      {name = "Redo", action = function() self:OnRedo() end, key = Keys.Y, toolTip = "Learn about PV8."},

      {divider = true},
      {name = "Cut", action = function()  end, key = Keys.X, toolTip = "Learn about PV8."},
      {name = "Copy", action = function()  end, key = Keys.C, toolTip = "Learn about PV8."},
      {name = "Paste", action = function()  end, key = Keys.V, toolTip = "Learn about PV8."},
      {name = "Flip H", action = function()  end, key = Keys.H, toolTip = "Learn about PV8."},
      {name = "Flip V", action = function()  end, key = Keys.J, toolTip = "Learn about PV8."},

      {divider = true},
      {name = "Line Thicker", action = function()  end, toolTip = "Learn about PV8."},
      {name = "Line Thinner", action = function()  end, toolTip = "Learn about PV8."},
      {name = "Fill", action = function()  end, toolTip = "Learn about PV8."},
      
      {divider = true},
      {name = "Edit Color", action = function()  end, toolTip = "Learn about PV8."},
      {name = "Outline Color", action = function()  end, toolTip = "Learn about PV8."},
      {name = "Fill Color", action = function() self:FillCanvasSelection(self.brushColor) end, toolTip = "Learn about PV8."},
      {name = "BG Color", action = function()  end, toolTip = "Learn about PV8."},
      {name = "Mask Color", action = function()  end, toolTip = "Learn about PV8."},

      {divider = true},
      {name = "Canvas Size", action = function()  end, key = Keys.I, toolTip = "Learn about PV8."},
      -- {name = "Color Mode", action = function()  end, toolTip = "Learn about PV8."},
      -- {name = "Sprite Mode", action = function()  end, toolTip = "Learn about PV8."},
      -- {name = "Flag Mode", action = function()  end, toolTip = "Learn about PV8."},
      {name = "Toggle BG", action = function()  end, key = Keys.I, toolTip = "Learn about PV8."},
      -- {divider = true},
      
      -- {name = "Zoom In", action = function()  end, toolTip = "Learn about PV8."},
      -- {name = "Zoom Out", action = function()  end, toolTip = "Learn about PV8."},
      
      {divider = true},
      {name = "Export Colors", action = function()  end, toolTip = "Learn about PV8."},
      {name = "Export Sprites", action = function()  end, toolTip = "Learn about PV8."},
      {name = "Export Flags", action = function()  end, toolTip = "Learn about PV8."},

      {divider = true},
      {name = "Run Game", action = function()  end, key = Keys.R, toolTip = "Learn about PV8."},
      {name = "Save", action = function()  end, key = Keys.S, toolTip = "Learn about PV8."},
      {name = "Quit", key = Keys.Q, action = QuitCurrentTool, toolTip = "Quit the current game."}, -- Quit the current game
  }

  self.RedoShortcut = 4
  self.UndoShortcut = 3

  -- local editorMapping = pixelVisionOS:FindEditors()

  -- local addLastDivider = false

  -- -- Only add these if the version of PV8 supports drawing tools
  -- if(editorMapping["colors"] ~= nil) then

  --     table.insert(menuOptions, #menuOptions, {name = "Toggle Palette", enabled = true, action = function() debugMode = not debugMode end, toolTip = "Shows a preview of the color palette."})

  --     table.insert(menuOptions, #menuOptions, {divider = true})

  --     table.insert(menuOptions, #menuOptions, {name = "Save Colors", enabled = true, action = function() OnSavePNG(true, false, false) end, toolTip = "Create a 'color-map.png' file."})

  --     addLastDivider = true
  -- end

  -- if(editorMapping["sprites"] ~= nil) then

  --     table.insert(menuOptions, #menuOptions, {name = "Save Sprites", enabled = true, action = function() OnSavePNG(false, true, false) end, toolTip = "Create a 'sprite.png' file."})
  --     addLastDivider = true
  -- end

  -- if(editorMapping["tilemap"] ~= nil) then

  --     table.insert(menuOptions, #menuOptions, {name = "Save Tilemap", enabled = true, action = function() OnSavePNG(false, false, true) end, toolTip = "Create a 'tilemap.json' file."})
  --     addLastDivider = true
  -- end

  -- if(addLastDivider == true) then
  --     table.insert(menuOptions, #menuOptions, {divider = true})
  -- end

  pixelVisionOS:CreateTitleBarMenu(menuOptions, "See menu options for this tool.")

end

function PaintTool:OnRunGame()


    local data = {runnerType = self.extension == ".lua" and "lua" or "csharp"}

    -- print("runnerType", data["runnerType"], self.extension)

    -- if(self.codeMode == true) then
    --    data["codeFile"] = _textTool.targetFile
    -- end

    local parentPath = self.targetFilePath.ParentPath

    if(self.invalid == true) then

        pixelVisionOS:ShowMessageModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before running the game?", 160, true,
                function()
                    if(pixelVisionOS.messageModal.selectionValue == true) then
                        -- Save changes
                        self:OnSave()

                    end

                    -- TODO should check that this is a game directory or that this file is at least a code.lua file
                    LoadGame(parentPath.Path, data)
                end
        )

    else
        -- Quit the tool
        LoadGame(parentPath.Path, data)
    end

end

function PaintTool:OnQuit()

      if(self.invalid == true) then

        pixelVisionOS:ShowMessageModal("Unsaved Changes", "You have unsaved changes. Do you want to save your work before you quit?", 160, true,
          function()
            if(pixelVisionOS.messageModal.selectionValue == true) then
              -- Save changes
              self:OnSave()

            end

            -- Quit the tool
            QuitCurrentTool()
          end
        )

      else
        -- Quit the tool
        QuitCurrentTool()
      end

end

function PaintTool:GetState()

  -- Select the correct canvas based on image or flag mode
  local srcCanvas = self.pickerMode == FlagMode and self.flagLayerCanvas or self.imageLayerCanvas

  -- Get the size of the canvas
  local width = srcCanvas.Width
  local height = srcCanvas.Height

  -- Get the pixels from the canvas
  local pixels = srcCanvas:GetPixels()

  
  -- TODO save brush state


  -- TODO save picker state
  local pickerMode = self.pickerMode
  
  -- Save the selection rect
  local selection = self.selectRect

  local state = {
      
      Action = function()

        -- Restore the selection state
        if(selection ~= nil) then
          self.selection = NewRect( selection.X, selection.Y, selection.Width, selection.Height )
        end

        -- Check the size of the image layer vs the last saved state
        if(srcCanvas.Width ~= width or srcCanvas.Height ~= height) then
          self:ResizeCanvas(width, height)
        end

        -- Restore the pixel data
        srcCanvas:SetPixels(pixels)

        -- Restore picker mode
        self:ChangeMode(pickerMode)

        -- Force the canvas to redraw
        self:InvalidateCanvas()

      end

  }

  return state

end

function PaintTool:ResizeCanvas(width, height)

  -- TODO need to resize all of the canvases

  self.imageLayerCanvas:Resize(width, height)
  self.tmpLayerCanvas:Resize(width, height)

end