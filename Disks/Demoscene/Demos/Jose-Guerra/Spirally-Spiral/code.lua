--[[
  Pixel Vision 8 - Spirally Spiral
    https://twitter.com/guerragames/status/974361013679247360

  Created by Jose Guerra (@guerragames)
    Ported to PV8 by Jesse Freeman (@jessefreeman)

]]--

x, y = 0, 0
t = 0
title = "FPS "

-- -- The Init() method is part of the game's lifecycle and called a game starts. We are going to
-- -- use this method to configure background color, ScreenBufferChip and draw a text box.
function Init()

  -- Get the display size
  display = Display()

  -- Create a new canvas for drawing into
  canvas = NewCanvas(display.x, display.y)

  -- Change the stroke to a single pixel
  canvas:SetStroke(7, 1)

end

-- The Update() method is part of the game's life cycle. The engine calls Update() on every frame
-- before the Draw() method. It accepts one argument, timeDelta, which is the difference in
-- milliseconds since the last frame.
function Update(timeDelta)

  -- Increase t on every frame
  t +  = .0005

end

-- The Draw() method is part of the game's life cycle. It is called after Update() and is where
-- all of our draw calls should go. We'll be using this to render sprites to the display.
function Draw()

  -- We can use the RedrawDisplay() method to clear the screen and redraw the tilemap in a
  -- single call.
  Clear()

  -- Clear the canvas
  canvas:Clear()

  -- Reset the x and y values back to 0
  x, y = 0, 0


  for i = 0, 1, .1 do
    b = i
    for a = 0, 1, .01 do
      r = 96 * a
      c = .05 * (.8 * sin(t * 8 + cos(a * (5 * sin(t)))))
      b +  = c
      n = r * cos(b + t * 2)
      m = r * sin(b + t * 2)
      if(a > 0) then

        -- Draw a line on the canvas
        canvas:DrawLine(64 + x, 64 + y, 64 + n, 64 + m)

      end
      x, y = n, m
    end
  end

  -- Draw the canvas to the UI layer.
  canvas:DrawPixels(0, 0, DrawMode.UI)



  DrawText(title .. ReadFPS(), 5, 5, DrawMode.SpriteAbove, "medium", 1, - 4)
  DrawText(title .. ReadFPS(), 4, 4, DrawMode.SpriteAbove, "medium", 10, - 4)

end

-- Math APIs (Copied from Pico8 Shim)
function sin(x)
  return - math.sin((x or 0) * math.pi * 2)
end

function cos(x)
  return math.cos((x or 0) * math.pi * 2)
end
