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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.UI;
using PixelVision8.Player;

namespace PixelVision8.Runner
{
    // using Debug = UnityEngine.Debug;

    /// <summary>
    ///     The Runner will work just like any other Unity GameObject. By extending MonoBehavior,
    ///     we can attach the runner to a GameObject in the scene and leverage its own lifecycle
    ///     to run Pixel Vision 8.
    /// </summary>
    public partial class GameRunner : MonoBehaviour
    {

        public string RootPath;

        // protected virtual bool RunnerActive => IsActive;
        // private TimeSpan _elapsedTime = TimeSpan.Zero;
        private int _frameCounter;
        protected int TimeDelta;
        protected PixelVision TmpEngine;
        public PixelVision ActiveEngine { get; protected set; }

        //   The Runner represents the bridge between a native platform and the Pixel Vision 8 
        //   Engine. A Runner is responsible for managing an instance of the PixelVisionEngine. 
        //   It also calls Update() and Draw() on the engine, converts the DisplayChip's 
        //   pixel data into a Texture and supplies input data from the native platform. In this 
        //   example, we'll use Unity to build out a simple Runner and load up one of the demo games.

        // To display our game, we'll need a reference to a RawImage from Unity. We are using a 
        // RawImage so that we can leverage some of Unity's new UI scaling options to keep the 
        // display at a fixed aspect ratio no matter what the screen resolution is at.
        public RawImage displayTarget;

        // To make this work, you'll need to create a new scene. Add a Canvas Component to it. 
        // Change the Canvas Scaler to scale with screen, and the reference Resolution should 
        // be 256 x 240.  It should also match the screen height. Next, add an Image called 
        // PlayWindow. Set its color to black and make it stretch to fill its parent. This Image 
        // will be our background outside of the game's display. Finally, add a Raw Image as a 
        // child of the Image we just created. Here you'll set it also to scale to fill its parent 
        // container and add an Aspect Ratio Fitter component with its Aspect Mode set to Fit In Parent. 
        // You can pass this RawImage into the runner to see the game's display when everything is working.

        // Now that we are storing a reference of the RawImage, we'll also need Texture for it. We'll draw 
        // the DisplayChip's pixel data into this Texture. We'll also set this Texture as the RawImage's 
        // source so we can see it in Unity.
        protected Texture2D renderTexture;


        /// <summary>
        ///     We'll use the Start method to configure our PixelVisionEngin and load a game.
        /// </summary>
        public virtual void Start()
        {

            if(RootPath == String.Empty || RootPath == null)
            {
                RootPath = Application.streamingAssetsPath;
            }

            // Pixel Vision 8 doesn't have a frame per second lock. It's up to the runner to 
            // determine what that cap should be. Here we'll use Unity's Application.targetFrameRate 
            // to lock it at 60 FPS.
            Application.targetFrameRate = 60;

            // By changing Unity's Cursor.visible property to false we'll be able to hide the mouse 
            // while the game is running.
            Cursor.visible = false;

            // // Before we set up the PixelVisionEngine we'll want to configure the renderTexture. 
            // // We'll create a new 256 x 240 Texture2D instance and set it as the displayTarget.texture.
            // renderTexture = new Texture2D(256, 240, TextureFormat.ARGB32, false) {filterMode = FilterMode.Point};
            // displayTarget.texture = renderTexture;

            // By setting the Texture2D filter mode to Point, we ensure that it will look crisp at any size. 
            // Since the Texture will be scaled based on the resolution, we want it always to look pixel perfect.

            Debug.Log("Root Path " + RootPath);

        }
        
        /// <summary>
        ///     It's important that we call the PixelVision8's Update() method on each frame. To do this, we'll use the
        ///     GameObject's own Update() call.
        /// </summary>
        public virtual void Update()
        {
            // Debug.Log("Update " + Time.deltaTime);
    //        if (preloading)
    //            Debug.Log("Loading Percent " + loadService.percent);

            // Before trying to update the PixelVisionEngine instance, we need to make sure it exists. The guard clause protects us from throwing an 
            // error when the Runner loads up and starts before we've had a chance to instantiate the new engine instance.
            // if (activeEngine == null)
            //     return;

            // activeEngine.Update(Time.deltaTime);

            // It's important that we pass in the Time.deltaTime to the PixelVisionEngine. It is passed along to any Chip that registers itself with 
            // the ChipManager to be updated. The ControlsChip, GamesChip, and others use this time delta to synchronize their actions based on the 
            // current framerate.
            
            // Before trying to update the PixelVisionEngine instance, we need to make sure it exists. The guard clause protects us from throwing an 
            // error when the Runner loads up and starts before we've had a chance to instantiate the new engine instance.
            if (ActiveEngine == null) return;

            // _elapsedTime += gameTime.ElapsedGameTime;

            // if (_elapsedTime > TimeSpan.FromSeconds(1))
            // {
            //     _elapsedTime -= TimeSpan.FromSeconds(1);

            //     // Make sure the game chip has the current fps value which is incremented in the draw call
            //     ActiveEngine.FPS = _frameCounter;

            //     _frameCounter = 0;
            // }

            // if (RunnerActive)
            // {
                TimeDelta = (int)(Time.deltaTime * 1000);

                // It's important that we pass in the Time.deltaTime to the PixelVisionEngine. It is passed along to any Chip that registers itself with 
                // the ChipManager to be updated. The ControlsChip, GamesChip, and others use this time delta to synchronize their actions based on the 
                // current frame rate.
                ActiveEngine.Update(TimeDelta);
            // }
        }

        public virtual void ActivateEngine(PixelVision engine)
        {
            if (engine == null) return;

            // Make the loaded engine active
            ActiveEngine = engine;

            ActiveEngine.ResetGame();

            // After loading the game, we are ready to run it.
            ActiveEngine.RunGame();
        }

        public virtual void RunGame()
        {
            ActivateEngine(TmpEngine);
        }

        /// <summary>
        ///     In Unity we can use the LateUpdate() method on the MonoBehavior class to synchronize when the PixelVision8 engine
        ///     should draw.
        /// </summary>
        public virtual void LateUpdate()
        {

            Draw();
            // Just like before, we use a guard clause to keep the Runner from throwing errors if no PixelVision8 engine exists.
            // if (activeEngine == null)
            //     return;

            // // Here we are checking that the PixelVisionEngine is actually running. If a game is not loaded there is nothing to render so we would 
            // // exit out of this call.
            // if (!activeEngine.running)
            //     return;

            // // Now it's time to call the PixelVisionEngine's Draw() method. This Draw() call propagates throughout all of the Chips that have 
            // // registered themselves as being able to draw such as the GameChip and the DisplayChip.
            // activeEngine.Draw();

            // // The first part of rendering Pixel Vision 8's DisplayChip is to get all of the current pixel data during the current frame. Each 
            // // Integer in this Array contains an ID we can use to match up to the cached colors we created when setting up the Runner.
            // var pixelData = activeEngine.displayChip.displayPixels; //.displayPixelData;
            // var total = pixelData.Length;
            // int colorRef;

            // // Need to make sure we are using the latest colors.
            // if (activeEngine.colorChip.invalid)
            //     CacheColors();

            // // We also want to cache the ScreenBufferChip's background color. The background color is an ID that references one of the ColorChip's colors.
            // var bgColor = activeEngine.colorChip.backgroundColor;

            // // The cachedTransparentColor is what shows when a color ID is out of range. Pixel Vision 8 doesn't support transparency, so this 
            // // color shows instead. Here we test to see if the bgColor is an ID within the length of the bgColor variable. If not, we set it to 
            // // Unity's default magenta color. If the bgColor is within range, we'll use that for transparency.
            // cacheTransparentColor = bgColor > cachedColors.Length || bgColor < 0 ? Color.magenta : cachedColors[activeEngine.colorChip.backgroundColor];

            // // Now it's time to loop through all of the DisplayChip's pixel data.
            // for (var i = 0; i < total; i++)
            // {
            //     // Here we get a reference to the color we are trying to look up from the pixelData array. Then we compare that ID to what we 
            //     // have in the cachedPixels. If the color is out of range, we use the cachedTransparentColor. If the color exists in the cache we use that.
            //     colorRef = pixelData[i];

            //     // Replace transparent colors with bg for next pass
            //     if (colorRef == -1)
            //         pixelData[i] = bgColor;

            //     cachedPixels[i] = colorRef < 0 || colorRef >= totalCachedColors ? cacheTransparentColor : cachedColors[colorRef];

            //     // As you can see, we are using a protected field called cachedPixels. When we call ResetResolution, we resize this array to make sure that 
            //     // it matches the length of the DisplayChip's pixel data. By keeping a reference to this Array and updating each color instead of rebuilding 
            //     // it, we can significantly increase the render performance of the Runner.
            // }

            // // At this point, we have all the color data we need to update the renderTexture. We'll set the cachedPixels on the renderTexture and call 
            // // Apply() to re-render the Texture.
            // renderTexture.SetPixels(cachedPixels);
            // renderTexture.Apply();
        }

        /// <summary>
        ///     The ResetResolution() method manages Unity-specific logic we need to make sure that our rednerTexture displays
        ///     correctly in its UI container
        ///     as well as making sure the cachedPixel array matches up to DisplayChip's pixel data length.
        /// </summary>
        // protected virtual void ResetResolution(int width, int height, bool fullScreen = false)
        // {
        //     // The first thing we need to do is resize the DisplayChip's own resolution.
        //     activeEngine.displayChip.ResetResolution(width, height);

        //     Screen.fullScreen = fullScreen;

        //     // We need to make sure our displayTarget, which is our RawImage in the Unity scene,  exists before trying to update it. 
        //     if (displayTarget != null)
        //     {
        //         // The first thing we'll do to update the displayTarget recalculate the correct aspect ratio. Here we get a reference 
        //         // to the AspectRatioFitter component then set the aspectRatio property to the value of the width divided by the height. 
        //         var fitter = displayTarget.GetComponent<AspectRatioFitter>();
        //         fitter.aspectRatio = (float) width / height;

        //         // Next we need to update the CanvasScaler's referenceResolution value.
        //         var canvas = displayTarget.canvas;
        //         var scaler = canvas.GetComponent<CanvasScaler>();
        //         scaler.referenceResolution = new Vector2(width, height);

        //         // Now we can resize the redenerTexture to also match the new resolution.
        //         renderTexture.Resize(width, height);
                
        //         // At this point, the Unity-specific UI is correctly configured. The CanvasScaler and AspectRetioFitter will ensure that 
        //         // the Texture we use to show the DisplayChip's pixel data will always maintain it's aspect ratio no matter what the game's 
        //         // real resolution is.

        //         // Now it's time to resize our cahcedPixels array. We calculate the total number of pixels by multiplying the width by the 
        //         // height. We'll use this array to make sure we have enough pixels to correctly render the DisplayChip's own pixel data.
        //         var totalPixels = width * height;
        //         Array.Resize(ref cachedPixels, totalPixels);

        //         // The last this we need to do is make sure that all of the cachedPixels are not transparent. Since Pixel Vision 8 doesn't 
        //         // support transparency it's important to make sure we can modify these colors before attempting to render the DisplayChip's pixel data.
        //         for (var i = 0; i < totalPixels; i++)
        //             cachedPixels[i].a = 1;

        //         var overscanXPixels = (width - activeEngine.displayChip.overscanXPixels) / (float) width;
        //         var overscanYPixels = (height - activeEngine.displayChip.overscanYPixels) / (float) height;
        //         var offsetY = 1 - overscanYPixels;
        //         displayTarget.uvRect = new UnityEngine.Rect(0, offsetY, overscanXPixels, overscanYPixels);

        //         // When copying over the DisplayChip's pixel data to the cachedPixels, we only focus on the RGB value. While we could reset the 
        //         // alpha during that step, it would also slow down the renderer. Since Pixel Vision 8 simply ignores the alpha value of a color, 
        //         // we can just do this once when changing the resolution and help speed up the Runner.
        //     }
        // }


            protected bool _resolutionInvalid = true;
            public DisplayTarget DisplayTarget;

        /// <summary>
            ///     Scale the resolution.
            /// </summary>
            /// <param name="scale"></param>
            /// <param name="fullScreen"></param>
            public virtual int Scale(int? scale = null)
            {
                if (scale.HasValue)
                {
                    DisplayTarget.MonitorScale = scale.Value;

                    InvalidateResolution();
                }

                return DisplayTarget.MonitorScale;
            }

            public virtual bool Fullscreen(bool? value = null)
            {
                if (value.HasValue)
                {
                    DisplayTarget.Fullscreen = value.Value;

                    InvalidateResolution();
                }

                return
                    DisplayTarget.Fullscreen;
            }

            public virtual void ConfigureDisplayTarget()
            {
                // Create the default display target
                DisplayTarget = new DisplayTarget(512, 480, displayTarget);
                // {
                //     GraphicsManager = Graphics
                // };
            }

            public void InvalidateResolution()
            {
                _resolutionInvalid = true;
            }

            public void ResetResolutionValidation()
            {
                _resolutionInvalid = false;
            }

            public virtual void ResetResolution()
            {
                DisplayTarget.ResetResolution(ActiveEngine.DisplayChip.Width, ActiveEngine.DisplayChip.Height);
            }

            protected void Draw()
            {
                if (ActiveEngine == null) return;

                // _frameCounter++;

                // Clear with black and draw the runner.
                // Graphics.GraphicsDevice.Clear(Color.Black);

                // Now it's time to call the PixelVisionEngine's Draw() method. This Draw() call propagates throughout all of the Chips that have 
                // registered themselves as being able to draw such as the GameChip and the DisplayChip.

                // Only call draw if the window has focus
                // if (RunnerActive) 
                ActiveEngine.Draw();

                if (ActiveEngine.ColorChip.Invalid)
                {
                    // Make sure the color palette doesn't need to rebuild itself
                    DisplayTarget.RebuildColorPalette(
                        ActiveEngine.ColorChip.HexColors, 
                        ActiveEngine.ColorChip.BackgroundColor,
                        ActiveEngine.ColorChip.MaskColor,
                        ActiveEngine.ColorChip.DebugMode
                    );
                    
                    ActiveEngine.ColorChip.ResetValidation();
                }

                if (_resolutionInvalid)
                {
                    ResetResolution();
                    ResetResolutionValidation();
                }
                
                DisplayTarget.Render(ActiveEngine.DisplayChip.Pixels);

                
            }

    }
}