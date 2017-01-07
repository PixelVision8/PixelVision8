//  
// Copyright (c) Jesse Freeman. All rights reserved.  
// 
// Licensed under the Microsoft Public License (MS-PL) License. 
// See LICENSE file in the project root for full license information. 
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman
// 

using System.Collections.Generic;
using System.Text;
using PixelVisionSDK.Engine.Chips.Data;
using PixelVisionSDK.Engine.Utils;

namespace PixelVisionSDK.Engine.Chips.Graphics.Display
{
    /// <summary>
    ///     The <see cref="ScreenBufferChip" /> represents the background layer of
    ///     the renderer. A TileMap can be loaded into the ScreenBuffer to allow for
    ///     displaying larger levels than the screen's own size. Also the
    ///     ScreenBuffer allows you ways to render static elements
    ///     like UI or text that don't change often but would be too expensive to
    ///     render as sprites.
    /// </summary>
    public class ScreenBufferChip : AbstractChip
    {
        protected int _scrollX;
        protected int _scrollY;
        protected int[] tmpPixelData;
        //TODO this causes a bug in the editor where it should be 256 x 244 need to resize it somehow
        protected TextureData tmpTextureData = new TextureData(1, 1);

        /// <summary>
        ///     The width of the area to sample from in the ScreenBufferChip. If
        ///     width of the view port is larger than the <see cref="TextureData" />
        ///     it will wrap.
        /// </summary>
        public int viewPortHeight = 240;

        /// <summary>
        ///     This represents the x position on the screen where the
        ///     ScreenBufferChip's view port should be rendered to on the display. 0
        ///     is the left of the screen.
        /// </summary>
        public int viewPortOffsetX;

        /// <summary>
        ///     This represents the y position on the screen where the
        ///     ScreenBufferChip's view port should be rendered to on the display. 0
        ///     is the top of the screen.
        /// </summary>
        public int viewPortOffsetY;

        /// <summary>
        ///     The height of the area to sample from in the ScreenBufferChip. If
        ///     width of the view port is larger than the <see cref="TextureData" />
        ///     it will wrap.
        /// </summary>
        public int viewPortWidth = 256;

        /// <summary>
        ///     The <see cref="invalid" /> flag is set to true when
        ///     something has changed in the screen buffer. This is usually changed
        ///     when the buffer's Clear(), UpdatePixelDataAt() and
        ///     MergePixelDataAt() methods are called. The <see cref="invalid" />
        ///     value can only be reset manually via the ResetInvalidation() method.
        /// </summary>
        public bool invalid { get; private set; }

        /// <summary>
        ///     This value is used for horizontally scrolling the ScreenBufferChip.
        ///     The <see cref="scrollX" /> field represents starting x position of
        ///     the <see cref="TextureData" /> to sample from. 0 is the left of the
        ///     screen;
        /// </summary>
        public int scrollX
        {
            get { return _scrollX; }
            set
            {
                if (value > viewPortWidth)
                    value = MathUtil.Repeat(value, tmpTextureData.width);


                if (_scrollX == value)
                    return;

                _scrollX = value;

                Invalidate();
            }
        }

        /// <summary>
        ///     This value is used for vertically scrolling the ScreenBufferChip.
        ///     The <see cref="scrollY" /> field represents starting y position of
        ///     the <see cref="TextureData" /> to sample from. 0 is the top of the
        ///     screen;
        /// </summary>
        public int scrollY
        {
            get { return _scrollY; }
            set
            {
                if (value > viewPortHeight)
                    value = MathUtil.Repeat(value, tmpTextureData.height);

                if (_scrollY == value)
                    return;

                _scrollY = value;
                Invalidate();
            }
        }

        //TODO shouldn't this be on the color class and not here?
        /// <summary>
        ///     The background color reference to use when rendering transparent in
        ///     the ScreenBufferTexture.
        /// </summary>
        public int backgroundColor { get; set; }

        /// <summary>
        ///     This will set the ScreenBufferData as <see cref="invalid" />
        ///     notifying other chips that a change has been made to the
        ///     ScreenBufferChip's data.
        /// </summary>
        public void Invalidate()
        {
            invalid = true;
        }

        /// <summary>
        ///     This will reset the <see cref="invalid" /> flag and should be called
        ///     when data has been sampled from the buffer after a previous
        ///     invalidation.
        /// </summary>
        public void ResetInvalidation()
        {
            invalid = false;
        }

        /// <summary>
        ///     This will automatically read pixel data from a TileMapChip and
        ///     render it to the ScreenBufferChip's TextureData. This is an
        ///     expensive method, so only call it when a game is loading or you have
        ///     a break in the action.
        /// </summary>
        public void RefreshScreenBlock()
        {
            if (engine.tileMapChip == null)
                return;
            engine.tileMapChip.ConvertToTextureData(tmpTextureData);
            Invalidate();
        }

        /// <summary>
        ///     This method allows you to draw raw pixel data into the
        ///     ScreenBufferChip's TextureData. You can draw pixel data at any x,
        ///     <paramref name="y" /> position but the data will not wrap around the
        ///     internal TextureData.
        /// </summary>
        /// <param name="x">
        ///     The x position where to start drawing pixel data. 0 is the left of
        ///     the screen.
        /// </param>
        /// <param name="y">
        ///     The y position where to start drawing pixel data. 0 is the top of
        ///     the sceen.
        /// </param>
        /// <param name="width">The width of the pixel data to draw.</param>
        /// <param name="height">The height of the pixel data to draw.</param>
        /// <param name="pixelData">
        ///     The actual pixel data values as anint array.
        /// </param>
        public void UpdatePixelDataAt(int x, int y, int width, int height, int[] pixelData)
        {
            y = tmpTextureData.height - height - y;

            tmpTextureData.SetPixels(x, y, width, height, pixelData);

            Invalidate();
        }

        /// <summary>
        ///     This method will merge pixel data into the ScreenBufferChip's
        ///     TextureData. Use this when you want to maintain transparancy and
        ///     fill in the empty pixel data with data from the existing
        ///     ScreenBufferChip's TextureData.
        /// </summary>
        /// <param name="x">
        ///     The x position where to start drawing pixel data. 0 is the left of
        ///     the screen.
        /// </param>
        /// <param name="y">
        ///     The y position where to start drawing pixel data. 0 is the top of
        ///     the sceen.
        /// </param>
        /// <param name="width">The width of the pixel data to draw.</param>
        /// <param name="height">The height of the pixel data to draw.</param>
        /// <param name="pixelData">
        ///     The actual pixel data values as anint array.
        /// </param>
        public void MergePixelDataAt(int x, int y, int width, int height, int[] pixelData)
        {
            //TODO this is hardcoded

            x *= 8;
            y *= 8;

            y = tmpTextureData.height - height - y;

            tmpTextureData.MergePixels(x, y, width, height, pixelData);

            Invalidate();
        }

        /// <summary>
        ///     Returns all of the pixel <paramref name="data" /> in the
        ///     ScreenBufferChip.
        /// </summary>
        /// <param name="data">
        ///     Supply anint array to be populated with pixel
        ///     data values.
        /// </param>
        /// <param name="width">Returns the width of the pixel data.</param>
        /// <param name="height">Returns the height of the pixel data.</param>
        public void ReadBlockData(int[] data, out int width, out int height)
        {
            tmpTextureData.CopyPixels(data);
            width = tmpTextureData.width;
            height = tmpTextureData.height;
        }

        /// <summary>
        ///     Clears the ScreenBufferChip's <see cref="TextureData" /> with the
        ///     background color value.
        /// </summary>
        public void Clear()
        {
            tmpTextureData.Clear(backgroundColor);
            Invalidate();
        }

        /// <summary>
        ///     This configures the ScreenBufferChip. It sets itself as the engine's
        ///     default buffer chip, enables wrap mode on the
        ///     internal TextureData, resets the view port
        ///     dimensions and clears the internal pixel data.
        /// </summary>
        public override void Configure()
        {
            engine.screenBufferChip = this;
            tmpTextureData.wrapMode = true;
            backgroundColor = -1;
            //ppu.screenBlockRam = this;
            viewPortOffsetX = 0;
            viewPortOffsetY = 0;
            scrollX = 0;
            scrollY = 0;
            tmpPixelData = new int[engine.spriteChip.width*engine.spriteChip.textureHeight];
            Clear();
            //backgroundColor = -1;
            // Creates a default Screen Block 
            //CreateScreenBlock();
        }

        //TODO need to optimize this
        /// <summary>
        ///     Reads the view port of the ScreenBufferChip. This is used by the
        ///     display. It uses the <see cref="scrollX" /> and <see cref="scrollY" />
        ///     values to determine where to start sampling data from.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="pixelData"></param>
        /// <param name="wrap"></param>
        public void ReadScreenData(int width, int height, int[] pixelData, bool wrap = false)
        {
            var tmpScrollY = tmpTextureData.height - height - scrollY;
            tmpTextureData.GetPixels(scrollX, tmpScrollY, width, height, pixelData);
        }

        /// <summary>
        ///     A quick method to replace a spefic sprite from the tile map to the
        ///     screen buffer.
        /// </summary>
        /// <param name="column">The column of the tile in the TileMap.</param>
        /// <param name="row">The row of the tile in the TileMap.</param>
        public void RevertCachedBlockAt(int column, int row)
        {
            int spriteID, paletteID, flag, index;

            var map = engine.tileMapChip;

            map.ReadTileAt(column, row, out spriteID, out paletteID, out flag);

            PosUtil.CalculateIndex(column, row, map.columns, out index);
            engine.spriteChip.ReadSpriteAt(index, tmpPixelData);
            UpdatePixelDataAt(column, row, engine.spriteChip.width, engine.spriteChip.height, tmpPixelData);

            Invalidate();
        }

        /// <summary>
        ///     This converts the ScreenBuffer's settings into a JSON string. It
        ///     stores the <see cref="viewPortWidth" /> and viewPortHeight.
        /// </summary>
        /// <param name="sb"></param>
        public override void CustomSerializedData(StringBuilder sb)
        {
            sb.Append("\"backgroundColor\":");
            sb.Append(backgroundColor);
            sb.Append(",");

            // Width Value
            sb.Append("\"viewPortWidth\":");
            sb.Append(viewPortWidth);
            sb.Append(",");

            // Height Value
            sb.Append("\"viewPortHeight\":");
            sb.Append(viewPortHeight);
        }

        /// <summary>
        ///     This converts values from a Dictionary with a sting as the key and
        ///     an object as the value into values the
        ///     <see cref="ScreenBufferChip" /> can use.
        /// </summary>
        /// <param name="data">
        ///     A Dictionary with a sting as the key and an object.
        /// </param>
        public override void DeserializeData(Dictionary<string, object> data)
        {
            if (data.ContainsKey("backgroundColor"))
                backgroundColor = (int) (long) data["backgroundColor"];

            if (data.ContainsKey("viewPortWidth"))
                viewPortWidth = (int) (long) data["viewPortWidth"];

            if (data.ContainsKey("viewPortHeight"))
                viewPortHeight = (int) (long) data["viewPortHeight"];
        }

        public override void Deactivate()
        {
            base.Deactivate();
            engine.screenBufferChip = null;
        }
    }
}