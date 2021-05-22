//   
// Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by Pixel Vision 8 are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman - @JesseFreeman
// Christina-Antoinette Neofotistou @CastPixel
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany
//

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace PixelVision8.Player
{
    public class HighlighterTheme
    {
        int Text = 15;
        int Selection = 0;
        int Keyword = 14;
        int Number = 6;  
        int Comment = 5;
        int String = 11; 
        int Api = 7; 
        int Callback = 9;
        int Escape = 15; 
        int Disabled = 5; 
        int SelectionBackground = 11;
    }

    public class Theme
    {
        public int bg = 0;
        public int cursor = 0;
        public int cursorBG = 15;
    }

    internal class Selection
    {
        int sxs;
        int sys;
        int sxe;
        int sye;

        public Selection(int sxs, int sys, int sxe, int sye)
        {
            this.sxs = sxs;
            this.sys = sys;
            this.sxe = sxe;
            this.sye = sye;
        }
    }

    public class TextEdtor : UIEntity
    {
        private int lastCX = 0;
        public bool editable = true;
        private int endOfLineOffset = 1;

        public Rectangle Viewport { get; }
        public Func<string> CaptureInput { get; }

        private Dictionary<Keys, Action> keyMap;
        private Theme theme;
        private int cx;
        private int cy;
        private int vx;
        private int vy;
        private int spacing;
        private string font;
        private Point charSize = new Point(Constants.SpriteSize, Constants.SpriteSize);
        private bool colorize;
        private bool autoDeselect = true;
        private List<string> buffer = new List<string>();
        private int stimer = 0;
        private float stime = 0.1f;
        private Point sflag = new Point();
        private int btimer = 0;
        private float btime = 0.5f;
        private bool bflag = true;
        private bool mflag;
        private int cursorColor;
        private float inputDelay = .10f;
        private Point cursorPos = new Point();
        

        private bool invalidateLine = true;
        private bool invalidateBuffer = true;
        private bool invalidText = true;
        private int lastKeyCounter = 0;
        private string tabChar = "  ";
        private string lastKey = "";
        private int colorOffset;

        private Selection selection;
        
        public TextEdtor(UIBuilder uiBuilder, Rectangle rect, string name = "", string font = "large", int spacing = 0, string tooltip = "") : base(uiBuilder, rect, "", name, tooltip, autoSize: false, rebuildSpriteCache: false, drawMode: DrawMode.TilemapCache)
        {

            Viewport = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
            theme = new Theme();

            cx = 1;
            cy = 1;
            
            vx = 1; 
            vy = 1;
//
            // undoStack = {} -- Keep a stack of undo info, each one is {data, state}
            // redoStack = {} -- Keep a stack of redo info, each one is {data, state}

            this.spacing = spacing;
            this.font = font;
            charSize.X -= Math.Abs(spacing);

            CaptureInput = new Func<string>(_CaptureInput);

            keyMap = new Dictionary<Keys, Action>()
            {
                {Keys.Enter, new Action(OnReturn)},
                {Keys.Left, new Action(OnLeft)},
                {Keys.Right, new Action(OnRight)},
                {Keys.Up, new Action(OnUp)},
                {Keys.Down, new Action(OnDown)},
                {Keys.Delete, new Action(OnDelete)},
                {Keys.Back, new Action(OnBackspace)},
                {Keys.Home, new Action(OnHome)},
                {Keys.PageUp, new Action(OnPageUp)},
                {Keys.PageDown, new Action(OnPageDown)},
                {Keys.Tab, new Action(OnTab)},

            };

        }

        private string _CaptureInput()
        {
            return _gameChip.InputString();
        }

        private void OnReturn()
        {
            if(selection != null)
                TextEditorDeleteSelection();
            
            TextEditorInsertNewLine();
        }

        private void OnLeft()
        {
            // self:TextEditorDeselect(targetData)
            // local flag = false
            // targetData.cx = targetData.cx - 1
            // if targetData.cx < 1 then
            //     if targetData.cy > 1 then
            //     targetData.cy = targetData.cy - 1
            //     targetData.cx = targetData.buffer[targetData.cy]:len() + 1
            //     flag = true
            //     end
            // end
            // self:TextEditorResetCursorBlink(targetData)
            // self:TextEditorCheckPosition(targetData)
        }

        private void OnRight()
        {
            // self:TextEditorDeselect(targetData)
            // local flag = false
            // targetData.cx = targetData.cx + 1
            // if targetData.cx > targetData.buffer[targetData.cy]:len() + 1 then
            //     if targetData.buffer[targetData.cy + 1] then
            //     targetData.cy = targetData.cy + 1
            //     targetData.cx = 1
            //     flag = true
            //     end
            // end
            // self:TextEditorResetCursorBlink(targetData)
            // self:TextEditorCheckPosition(targetData)
        }

        private void OnShiftUp()
        {
            // --in case we want to reduce shift selection
            // if targetData.cy == 1 then
            //     --we stay in buffer
            //     return
            // end
            // if targetData.sxs then
            //     --there is an existing selection to update
            //     targetData.cy = targetData.cy - 1
            //     self:TextEditorCheckPosition(targetData)
            //     targetData.sye = targetData.cy
            //     targetData.sxe = math.min(targetData.cx, #targetData.buffer[targetData.cy])
            // else
            //     targetData.sxs = targetData.cx
            //     targetData.sys = targetData.cy
            //     targetData.cy = targetData.cy - 1
            //     self:TextEditorCheckPosition(targetData)
            //     targetData.sye = targetData.cy
            //     targetData.sxe = math.min(targetData.cx, #targetData.buffer[targetData.cy])
            // end
            // self:TextEditorInvalidateBuffer(targetData)
        }

        private void OnShiftDown()
        {
            // --last line check, we do not go further than buffer
            // if #targetData.buffer == targetData.cy then
            //     return
            // end

            // if targetData.sxs then
            //     targetData.cy = targetData.cy + 1
            //     self:TextEditorCheckPosition(targetData)
            //     targetData.sye = targetData.cy
            //     targetData.sxe = math.min(targetData.cx, #targetData.buffer[targetData.cy])
            // else
            //     targetData.sxs = targetData.cx
            //     targetData.sys = targetData.cy
            //     targetData.cy = targetData.cy + 1
            //     self:TextEditorCheckPosition(targetData)
            //     targetData.sye = targetData.cy
            //     targetData.sxe = math.min(targetData.cx, #targetData.buffer[targetData.cy])
            // end
            // self:TextEditorInvalidateBuffer(targetData)
        }

        private void OnShiftRight()
        {
            // --last line check, we do not go further than buffer
            // if #targetData.buffer == targetData.cy and targetData.cx == #targetData.buffer[targetData.cy] then
            //     return
            // end
            // local originalcx, originalcy = targetData.cx, targetData.cy
            // targetData.cx = targetData.cx + 1

            // if targetData.cx > targetData.buffer[targetData.cy]:len() + 1 then
            //     if targetData.buffer[targetData.cy + 1] then
            //     targetData.cy = targetData.cy + 1
            //     targetData.cx = 1
            //     end
            // end
            // self:TextEditorCheckPosition(targetData)

            // if targetData.sxs then
            //     targetData.sye = targetData.cy
            //     targetData.sxe = math.min(targetData.cx, #targetData.buffer[targetData.cy])
            // else
            //     targetData.sxs = originalcx
            //     targetData.sys = originalcy
            //     targetData.sye = targetData.cy
            //     targetData.sxe = math.min(targetData.cx, #targetData.buffer[targetData.cy])
            // end

            // self:TextEditorInvalidateBuffer(targetData)
        }

        private void OnShiftLeft()
        {
            // --last line check, we do not go further than buffer
            // if 0 == targetData.cy and targetData.cx <= 1 then
            //     return
            // end
            // local originalcx, originalcy = targetData.cx, targetData.cy
            // targetData.cx = targetData.cx - 1

            // if targetData.cx < 1 then
            //     if targetData.cy > 1 then
            //     targetData.cy = targetData.cy - 1
            //     targetData.cx = targetData.buffer[targetData.cy]:len() + 1
            //     end
            // end
            // self:TextEditorCheckPosition(targetData)

            // if targetData.sxs then
            //     targetData.sye = targetData.cy
            //     targetData.sxe = math.min(targetData.cx, #targetData.buffer[targetData.cy])
            // else
            //     targetData.sxs = originalcx
            //     targetData.sys = originalcy
            //     targetData.sye = targetData.cy
            //     targetData.sxe = math.min(targetData.cx, #targetData.buffer[targetData.cy])
            // end

            // self:TextEditorInvalidateBuffer(targetData)
        }

        private void OnUp()
        {
            // self:TextEditorDeselect(targetData)
            // targetData.cy = targetData.cy - 1
            // self:TextEditorResetCursorBlink(targetData)
            // self:TextEditorCheckPosition(targetData)
        }

        private void OnDown()
        {
            // self:TextEditorDeselect(targetData)
            // targetData.cy = targetData.cy + 1
            // self:TextEditorResetCursorBlink(targetData)
            // self:TextEditorCheckPosition(targetData)
        }

        private void OnBackspace()
        {
            // -- if targetData.readonly then _systemMessage("The file is readonly !", 1, 9, 4) return end
            // if targetData.sxs then self:TextEditorDeleteSelection(targetData) return end
            // if targetData.cx == 1 and targetData.cy == 1 then return end
            // local lineChange
            // targetData.cx, targetData.cy, lineChange = self:TextEditorDeleteCharAt(targetData, targetData.cx - 1, targetData.cy)
            // self:TextEditorResetCursorBlink(targetData)
            // self:TextEditorCheckPosition(targetData)-- or lineChange then self:TextEditorDrawBuffer(targetData) else self:TextEditorDrawLine(targetData) end
            
        }

        private void OnDelete()
        {
            // -- if targetData.readonly then _systemMessage("The file is readonly !", 1, 9, 4) return end
            // if targetData.sxs then self:TextEditorDeleteSelection(targetData) return end
            // local lineChange
            // targetData.cx, targetData.cy, lineChange = self:TextEditorDeleteCharAt(targetData, targetData.cx, targetData.cy)
            // self:TextEditorResetCursorBlink(targetData)
            // self:TextEditorCheckPosition(targetData)-- or lineChange then self:TextEditorDrawBuffer(targetData) else self:TextEditorDrawLine(targetData) end
            
        }

        private void OnHome()
        {
           TextEditorGotoLineStart();
        }

        private void OnEnd()
        {
            TextEditorGotoLineEnd();
        }

        private void OnPageUp()
        {

        }

        private void OnPageDown()
        {

        }

        private void OnTab()
        {

        }

        private void OnSelectAll()
        {

        }


        public void TextEditorGotoLineStart()
        {
            TextEditorDeselect();
            cx = 1;
            TextEditorResetCursorBlink();

            if (TextEditorCheckPosition())
            {
                TextEditorDrawBuffer();
            }
            else 
            {
                TextEditorDrawLine();
            }

        }

        public void TextEditorGotoLineEnd()
        {
            TextEditorDeselect();
            cx = buffer[cy].Length + 1;
            TextEditorResetCursorBlink();
            if (TextEditorCheckPosition())
            {
                TextEditorDrawBuffer();
            }
            else
            {
                TextEditorDrawLine();
            } 

        }

        public void TextEditorDeselect()
        {
        
            if (selection != null)//sxs then
            {
                selection = null;
                TextEditorInvalidateBuffer();
            }
            //     --print(name, "Deselect")
            //     sxs, sys, sxe, sye = nil, nil, nil, nil
            //     self:TextEditorInvalidateBuffer(data)
            // end
        }

        public void TextEditorResetCursorBlink()
        {
            btimer = 0;
            bflag = true;
        }

        // public void TextEditorInvalidateBuffer()
        // {
        //     invalidateBuffer = true;
        // }    

        public bool TextEditorCheckPosition()
        {
            var flag = false;// --Flag if the whole buffer requires redrawing

            // Clamp the y position between 1 and the length of the buffer
            cy = _gameChip.Clamp(cy, 1, buffer.Count);

            // if cy > tiles.h + vy - 1 then --Passed the screen to the bottom
            //     vy = cy - (tiles.h - 1); flag = true
            // elseif cy < vy then --Passed the screen to the top
            //     if cy < 1 then cy = 1 end
            //     vy = cy; flag = true
            // end

            // --X position checking--
            // if buffer[cy]:len() < cx - 1 then cx = buffer[cy]:len() + endOfLineOffset end --Passed the end of the line !

            // cx = Clamp(cx, 1, buffer[cy]:len() + endOfLineOffset)

            // if cx > tiles.w + (vx - 1) then --Passed the screen to the right
            //     vx = cx - (tiles.w - 1); flag = true
            // elseif cx < vx then --Passed the screen to the left
            //     if cx < 1 then cx = 1 end
            //     vx = cx; flag = true
            // end

            if(flag)
            {
                TextEditorInvalidateBuffer();
            }

            return flag;
            // end
        }

        public void TextEditorDrawBuffer()
        {
            // if invalidateBuffer == false then return end

            // self:TextEditorResetBufferValidation()

            // local vbuffer = lume.slice(buffer, vy, vy + tiles.h - 1) --Visible buffer
            // local cbuffer = (colorize and highlighter ~= nil) and highlighter:highlightLines(vbuffer, vy) or vbuffer

            // DrawRect( 
            //     rect.x,
            //     rect.y,
            //     rect.w,
            //     rect.h,
            //     theme.bg,
            //     DrawMode.TilemapCache
            // )

            // for k, l in ipairs(cbuffer) do

            //     -- Draw the line first for the background
            //     self:TextEditorMoveCursor(, - (vx - 2) - 1, k, - 1)
            //     self:TextEditorDrawColoredTextAtCursor(, l)

            //     local sxs, sys, sxe, sye = self:TextEditorGetOrderedSelect()

            //     if sxs and vy + k - 1 >= sys and vy + k - 1 <= sye then --Selection
            //     self:TextEditorMoveCursor(, - (vx - 2) - 1, k, highlighterTheme.selection)
            //     local linelen, skip = vbuffer[k]:len(), 0

            //     if vy + k - 1 == sys then --Selection start
            //         skip = sxs - 1
            //         self:TextEditorMoveCursor(, skip - (vx - 2) - 1)
            //         linelen = linelen - skip
            //     end

            //     if vy + k - 1 == sye then --Selection end
            //         linelen = sxe - skip
            //     end

            //     if vy + k - 1 < sye then --Not the end of the selection
            //         linelen = linelen + 1
            //     end

            //     -- Highlight start
            //     local hs = vx + cursorPos.x

            //     local he = hs + linelen - 1

            //     local char = buffer[vy + k - 1]:sub(hs, he)

            //     -- print("highlight", hs, he)
            //     if(char == "") then
            //         char = " "
            //     end

            //     -- DrawRect( hs, cursorPos.y * 8, he, 8, 1, DrawMode.TilemapCache )
            //     self:TextEditorDrawCharactersAtCursor(, char)

            //     end
            // end
            // end
        }

        public void TextEditorDrawLine()
        {
            // -- If the line hasn't been invalidated don't render it
            // if data.invalidateLine == false then return end

            // self:TextEditorResetLineValidation(data)

            // -- If there is a selection we want to draw the buffer instead of the line
            // if(data.sxs) then

            //     self:TextEditorInvalidateBuffer(data)

            //     return

            // end

            // -- get the line's new width to see if it's larger than the max counter
            // data.maxLineWidth = math.max(#data.buffer[data.cy], data.maxLineWidth)

            // -- Reset validation
            // if data.cy - data.vy < 0 or data.cy - data.vy > data.tiles.h - 1 then return end
            // local cline, colateral
            // if (data.colorize and highlighter ~= nil) then
            //     cline, colateral = highlighter:highlightLine(data.buffer[data.cy], data.cy)
            // end
            // if not cline then cline = data.buffer[data.cy] end

            // local y = ((data.cy - data.vy + 1) * (data.charSize.y))

            // DrawRect( 
            //     data.rect.x,
            //     data.rect.y + y - data.charSize.Y,
            //     data.rect.w,
            //     data.charSize.y,
            //     data.theme.bg,
            //     DrawMode.TilemapCache
            // )
            
            // self:TextEditorMoveCursor(data, - (data.vx - 2) - 1, y / data.charSize.Y, data.theme.bg)
            // if not colateral then
            //     self:TextEditorDrawColoredTextAtCursor(data, cline)
            // else
            //     self:TextEditorInvalidateBuffer(data)
            // end

            // end
        }

        public void TextEditorInvalidateLine()
        {
            invalidateLine = true;
        }
        
        public void TextEditorResetLineValidation()
        {
            invalidateLine = false;
        }
        
        public void TextEditorInvalidateBuffer()
        {
            invalidateBuffer = true;
        }
        
        public void TextEditorResetBufferValidation()
        {
            invalidateBuffer = false;
        }
        
        public void TextEditorInvalidateText()
        {
            invalidText = true;
        }
        
        public void TextEditorResetTextValidation()
        {
            invalidText = false;
        }
        
        public void TextEditorGetOrderedSelect()
        {
        //           if sxs then
        //     if sye < sys then
        //     return sxe, sye, sxs, sys
        //     elseif sye == sys and sxe < sxs then
        //     return sxe, sys, sxs, sye
        //     else
        //     return sxs, sys, sxe, sye
        //     end
        // else
        //     return false
        // end
        // end
        }
    

        public void TextEditorInsertNewLine()
        {
            // self:TextEditorBeginUndoable(data)
            // local newLine = buffer[cy]:sub(cx, - 1)
            // buffer[cy] = buffer[cy]:sub(0, cx - 1)
            // local snum = string.find(buffer[cy].."a", "%S") --Number of spaces
            // snum = snum and snum - 1 or 0
            // newLine = string.rep(" ", snum)..newLine
            // cx, cy = snum + 1, cy + 1
            // if cy > #buffer then
            //     table.insert(buffer, newLine)
            // else
            //     buffer = lume.concat(lume.slice(buffer, 0, cy - 1), {newLine}, lume.slice(buffer, cy, - 1)) --Insert between 2 different lines
            // end

            // self:TextEditorInvalidateBuffer(data)

            // self:TextEditorResetCursorBlink(data)
            // self:TextEditorCheckPosition(data)
            // -- self:TextEditorDrawBuffer(data)
            // -- self:TextEditorDrawLineNum(data)
            // self:TextEditorEndUndoable(data)
            // self:TextEditorInvalidateText(data)
            // end
        }
            


        public void TextEditorDeleteSelection()
        {
            // if not sxs then return end --If not selection just return back.
            // local sxs, sys, sxe, sye = self:TextEditorGetOrderedSelect(data)

            // self:TextEditorBeginUndoable(data)
            // local lnum, slength = sys, sye + 1
            // while lnum < slength do
            //     if lnum == sys and lnum == sye then --Single line selection
            //     buffer[lnum] = buffer[lnum]:sub(1, sxs - 1) .. buffer[lnum]:sub(sxe + 1, - 1)
            //     lnum = lnum + 1
            //     elseif lnum == sys then
            //     buffer[lnum] = buffer[lnum]:sub(1, sxs - 1)
            //     lnum = lnum + 1
            //     elseif lnum == slength - 1 then
            //     buffer[lnum - 1] = buffer[lnum - 1] .. buffer[lnum]:sub(sxe + 1, - 1)
            //     buffer = lume.concat(lume.slice(buffer, 1, lnum - 1), lume.slice(buffer, lnum + 1, - 1))
            //     slength = slength - 1
            //     else --Middle line
            //     buffer = lume.concat(lume.slice(buffer, 1, lnum - 1), lume.slice(buffer, lnum + 1, - 1))
            //     slength = slength - 1
            //     end
            // end
            // cx, cy = sxs, sys
            // self:TextEditorCheckPosition(data)
            // self:TextEditorDeselect(data)
            // self:TextEditorInvalidateBuffer(data)
            // self:TextEditorEndUndoable(data)
            // self:TextEditorInvalidateText(data)
            // end
        }

    }

}