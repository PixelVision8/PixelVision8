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
// Jesse Freeman - @JesseFreeman
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany
// 

using PixelVisionSDK.Utils;

namespace PixelVisionSDK.Chips
{
    public class DisplayChip : AbstractChip, IDraw
    {
        protected readonly TextureData textureData = new TextureData(0, 0);
        protected int _height = 240;
        protected int _maxSpriteCount = 64;
        protected int _width = 256;
        protected int currentSprites;
        protected int[] tmpBufferData;

        /// <summary>
        ///     Sets the total number of sprite draw calls for the display.
        /// </summary>
        public int maxSpriteCount
        {
            get { return _maxSpriteCount; }
            set { _maxSpriteCount = value; }
        }

        /// <summary>
        ///     This toggles wrap mode on the display. If pixel data is draw past
        ///     the end of the display it will appear on the opposite side. There is
        ///     a slight performance hit for this.
        /// </summary>
        public bool wrapMode
        {
            get { return textureData.wrapMode; }
            set { textureData.wrapMode = value; }
        }

        /// <summary>
        ///     Returns the display's <see cref="width" />
        /// </summary>
        public int width
        {
            get { return _width; }
        }

        /// <summary>
        ///     Returns the display's <see cref="height" />
        /// </summary>
        public int height
        {
            get { return _height; }
        }

        /// <summary>
        /// </summary>
        public bool paused { get; set; }

        /// <summary>
        ///     A flag to define if the display auto clears after each frame.
        /// </summary>
        public bool autoClear { get; set; }

        /// <summary>
        ///     Returns the raw pixel data that represents what the display should look
        ///     like. Use this data to render to the display.
        /// </summary>
        public int[] displayPixelData
        {
            get { return textureData.GetPixels(); }
        }

        /// <summary>
        /// </summary>
        public void Draw()
        {
            //TODO this should run through all the draw calls and composite them to the textureData


            // Reset the sprite counter after a draw
            currentSprites = 0;

            //            if (_rawImage)
            //            {
            //                //Debug.Log("Post Render");
            //                CopyBuffer();
            //            }
        }

        /// <summary>
        ///     This clears the display. It will write a background color from the
        ///     <see cref="ScreenBufferChip" /> into the internal
        ///     screenBufferData or us 0 if no <see cref="ScreenBufferChip" /> is
        ///     found.
        /// </summary>
        public void Clear()
        {
            // TODO this is a strange dependancy I need to look into. Throws an error on startup
            textureData.Clear(engine.screenBufferChip != null ? engine.screenBufferChip.backgroundColor : 0);
        }


        public void CopyScreenBlockBuffer()
        {
            //TODO optimize this, it's the most expensive method in the engine

            var bufferChip = engine.screenBufferChip;

            if (bufferChip == null)
                return;

            if (bufferChip.invalid)
            {
                bufferChip.ReadScreenData(_width, _height, tmpBufferData);

                bufferChip.ResetInvalidation();
            }

            textureData.MergePixels(0, 0, _width, _height, tmpBufferData, false, engine.screenBufferChip.backgroundColor);
        }

        /// <summary>
        ///     Resets the display chip and clears all of the pixel data.
        /// </summary>
        public override void Reset()
        {
            Clear();
        }

        /// <summary>
        ///     Returns a bool if the Display has enough draw calls left to
        ///     render a sprite.
        /// </summary>
        /// <returns></returns>
        public bool CanDraw()
        {
            return currentSprites < maxSpriteCount;
        }

        /// <summary>
        ///     Creates a new draw by copying the supplied pixel data over
        ///     to the Display's TextureData.
        /// </summary>
        /// <param name="pixelData"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="flipH"></param>
        /// <param name="flipV"></param>
        /// <param name="flipY"></param>
        /// <param name="layerOrder"></param>
        /// <param name="masked"></param>
        public void NewDrawCall(int[] pixelData, int x, int y, int width, int height, bool flipH, bool flipV, bool flipY,
            int layerOrder = 0,
            bool masked = false)
        {
            var drawCalls = width / engine.spriteChip.width * (height / engine.spriteChip.height);

            //currentSprites += drawCalls;

            if (currentSprites + drawCalls > maxSpriteCount)
                return;

            currentSprites += drawCalls;

            //TODO need to add in layer merge logic, -1 is behind, 0 is normal, 1 is above

            layerOrder = layerOrder.Clamp(-1, 1);

            // flip y coordinate space
            if (flipY)
                y = _height - engine.spriteChip.height - y;

            if (pixelData != null)
            {
                if (flipH || flipV)
                    SpriteChipUtil.FlipSpriteData(ref pixelData, width, height, flipH, flipV);

                textureData.MergePixels(x, y, width, height, pixelData);
            }
        }

        /// <summary>
        ///     Changes the resolution of the display.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="updateScaler"></param>
        public void ResetResolution(int width, int height)
        {
            _width = width;
            _height = height;

            // Resize data structures
            textureData.Resize(_width, _height);

            tmpBufferData = new int[_width * _height];
        }

        /// <summary>
        ///     This configures the DisplayChip. It registers itself as the default
        ///     <see cref="DisplayChip" /> for the engine, gets a reference to the
        ///     engine's renderTarget, sets <see cref="autoClear" /> and
        ///     <see cref="wrapMode" /> to true and
        ///     finally resets the resolution to its default value
        ///     of 256 x 240.
        /// </summary>
        public override void Configure()
        {
            //Debug.Log("Pixel Data Renderer: Configure ");
            engine.displayChip = this;

            // Get the target raw image from the engine
            //target = engine.renderTarget;

            // TODO Need to set the display from the engine
            //maxSpriteCount = 64;
            autoClear = true;
            wrapMode = true;
            ResetResolution(256, 240);
        }

        public override void Deactivate()
        {
            base.Deactivate();
            engine.displayChip = null;
        }
    }
}