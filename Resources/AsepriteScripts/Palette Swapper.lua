----------------------------------------------------------------------
-- A customizable toolbar that can be useful in touch-like devices
-- (e.g. on a Microsoft Surface).
--
-- Feel free to add new commands and modify it as you want.
----------------------------------------------------------------------

local dlg = Dialog("Palette Swapper")
dlg
:entry{ label = "Prefix", id = "prefix", text = "ActionButton"}
:separator()
:button{text = "Disabled", onclick = function()

  LoadPalette("C:\\Users\\JesseFreeman\\Documents\\Projects\\PixelVision8Builder\\Resources\\UI\\Palettes\\"..dlg.data.prefix.."Disabled.ase")

end}
:button{text = "Up", onclick = function()

  LoadPalette("C:\\Users\\JesseFreeman\\Documents\\Projects\\PixelVision8Builder\\Resources\\UI\\Palettes\\"..dlg.data.prefix.."Up.ase")

end}
:button{text = "Over", onclick = function()

  LoadPalette("C:\\Users\\JesseFreeman\\Documents\\Projects\\PixelVision8Builder\\Resources\\UI\\Palettes\\"..dlg.data.prefix.."Over.ase")

end}
:button{text = "Down", onclick = function()

  LoadPalette("C:\\Users\\JesseFreeman\\Documents\\Projects\\PixelVision8Builder\\Resources\\UI\\Palettes\\"..dlg.data.prefix.."Down.ase")

end}
:button{text = "Selected Up", onclick = function()

  LoadPalette("C:\\Users\\JesseFreeman\\Documents\\Projects\\PixelVision8Builder\\Resources\\UI\\Palettes\\"..dlg.data.prefix.."SelectedUp.ase")

end}
:button{text = "Selected Over", onclick = function()

  LoadPalette("C:\\Users\\JesseFreeman\\Documents\\Projects\\PixelVision8Builder\\Resources\\UI\\Palettes\\"..dlg.data.prefix.."SelectedOver.ase")

end}
:separator()
:button{text = "Default Color", onclick = function()

  LoadPalette("C:\\Users\\JesseFreeman\\Documents\\Projects\\PixelVision8Builder\\Resources\\UI\\Palettes\\DefaultUI.ase")

end}
:button{text = "Color Map", onclick = function()

  LoadPalette("C:\\Users\\JesseFreeman\\Documents\\Projects\\PixelVision8Builder\\Resources\\UI\\Palettes\\ColorMapUI.ase")

end}
:show{wait = false}

function LoadPalette(path)

  -- local newPalette = Palette{ fromFile = path}
  local palette = app.sprites[1]:loadPalette(path)

end
