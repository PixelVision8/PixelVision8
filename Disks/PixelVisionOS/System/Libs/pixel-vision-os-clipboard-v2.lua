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

    self.clipboardContents = data

end

function PixelVisionOS:SystemPaste()

    autoClear = autoClear or true

    local data = self.clipboardContents

    return data

end

function PixelVisionOS:ClipboardFull()

    return self.enableClipboard == false and false or self.clipboardContents ~= nil

end

function PixelVisionOS:EnableClipboard(value)

    self.enableClipboard = value

end

function PixelVisionOS:ClearClipboard()

    self.clipboardContents = nil

end