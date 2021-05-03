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

-- print("Enable clipboard")

function PixelVisionOS:SystemCopy(data)

    SetClipboardText(data)

    -- Enable the paste menu
    self:EnableMenuItemByName("Paste")
    
end

function PixelVisionOS:SystemPaste()

    return GetClipboardText()

end

function PixelVisionOS:ClipboardFull()

    return self.enableClipboard == false and false or GetClipboardText() ~= ""

end

function PixelVisionOS:EnableClipboard(value)

    self.enableClipboard = value

end

function PixelVisionOS:ClearClipboard()

    ClearClipboardText()

    self:EnableMenuItemByName("Paste", false)

end