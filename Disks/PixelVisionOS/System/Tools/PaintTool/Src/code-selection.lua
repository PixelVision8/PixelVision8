


function PaintTool:DrawSelectionLine(canvas, x1, y1, x2, y2, shift, visibleBounds)
    
    local dx, sx = math.abs(x2-x1), x1<x2 and 1 or -1
    local dy, sy = math.abs(y2-y1), y1<y2 and 1 or -1
    local err = math.floor((dx>dy and dx or -dy)/2)
    local counter = 0

    shift = shift or 0

    while(true) do

        -- Make sure the pixel is inside of the canvas
        if(((((0 <= x1) and (x1 < canvas.Width)) and (0 <= y1)) and (y1 < canvas.Height))) then
            canvas:SetPixelAt(x1, y1, counter % 2 == shift and -17 or -2)
        end
            
        -- increment the alternating pixel counter
        counter = counter + 1

        if (x1==x2 and y1==y2) then break end
        if (err > -dx) then
            err, x1 = err-dy, x1+sx
        end
        if (err < dy) then
            err, y1 = err+dx, y1+sy
        end

        
    end

end


function PaintTool:DrawSelectionRectangle(canvas, _tmpRect, shift)

    self:DrawSelectionLine(canvas, _tmpRect.Left, _tmpRect.Top, _tmpRect.Right -1 , _tmpRect.Top, shift)

    self:DrawSelectionLine(canvas, _tmpRect.Right -1 , _tmpRect.Top, _tmpRect.Right -1 , _tmpRect.Bottom - 1, shift)

    self:DrawSelectionLine(canvas, _tmpRect.Left, _tmpRect.Bottom - 1, _tmpRect.Right -1 , _tmpRect.Bottom - 1, shift)

    self:DrawSelectionLine(canvas, _tmpRect.Left, _tmpRect.Top, _tmpRect.Left, _tmpRect.Bottom - 1, shift)
     
end