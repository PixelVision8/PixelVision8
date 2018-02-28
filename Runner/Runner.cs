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

using PixelVisionRunner.Services;
using PixelVisionSDK;
using System.Collections.Generic;
using System.Diagnostics;

namespace PixelVisionRunner
{
    public class Runner
    {
        public IEngine activeEngine;
        protected IFileSystem fileSystem;
        public LoadService loadService;

        public Runner(ITextureFactory textureFactory)
        {
            loadService = new LoadService(textureFactory);
        }

        public void ActivateEngine(IEngine engine)
        {
            if (engine == null)
                return;
        
            // Make the loaded engine active
            activeEngine = engine;

            // After loading the game, we are ready to run it.
            activeEngine.RunGame();
        }

        /// <summary>
        ///     It's important that we call the PixelVision8's Update() method on each frame. To do this, we'll use the
        ///     GameObject's own Update() call.
        /// </summary>
        public virtual void Update(float timeDelta)
        {
            if (activeEngine == null)
                return;
        
            // Before trying to update the PixelVisionEngine instance, we need to make sure it exists. The guard clause protects us from throwing an 
            // error when the Runner loads up and starts before we've had a chance to instantiate the new engine instance.

            activeEngine.Update(timeDelta);

            // It's important that we pass in the Time.deltaTime to the PixelVisionEngine. It is passed along to any Chip that registers itself with 
            // the ChipManager to be updated. The ControlsChip, GamesChip, and others use this time delta to synchronize their actions based on the 
            // current framerate.
        
        }

        /// <summary>
        ///     In Unity we can use the LateUpdate() method on the MonoBehavior class to synchronize when the PixelVision8 engine
        ///     should draw.
        /// </summary>
        public virtual void Draw()
        {
            if (activeEngine == null)
                return;
        
            // Now it's time to call the PixelVisionEngine's Draw() method. This Draw() call propagates throughout all of the Chips that have 
            // registered themselves as being able to draw such as the GameChip and the DisplayChip.
            activeEngine.Draw();

        }

        private IEngine tmpEngine;

        protected bool displayProgress;
        
        public virtual void ProcessFiles(IEngine tmpEngine, Dictionary<string, byte[]> files, bool displayProgress = false)
        {
            this.displayProgress = displayProgress;
            
            this.tmpEngine = tmpEngine;
            
            ParseFiles(files);

            if (!displayProgress)
            {
                loadService.LoadAll();
                RunGame();
            }
        }
        
        public virtual bool PreloaderNextStep()
        {

            var timeElapsed = 0L;
            var batchedSteps = 0;

            while (timeElapsed < 15L && loadService.completed == false)
            {

                var watch = Stopwatch.StartNew();

                loadService.NextParser();

                watch.Stop();

                timeElapsed = watch.ElapsedMilliseconds;
                batchedSteps++;
            }

            if (loadService.completed && !displayProgress)
            {
                PreloaderComplete();
                return true;
            }

            return false;

        }

        public virtual void PreloaderComplete()
        {
            RunGame();
        }
        
        protected virtual void ParseFiles(Dictionary<string, byte[]> files, SaveFlags? flags = null)
        {
            if (!flags.HasValue)
            {
                flags = SaveFlags.System;
                flags |= SaveFlags.Code;
                flags |= SaveFlags.Colors;
                flags |= SaveFlags.ColorMap;
                flags |= SaveFlags.Sprites;
                flags |= SaveFlags.Tilemap;
                flags |= SaveFlags.TilemapFlags;
                flags |= SaveFlags.Fonts;
                flags |= SaveFlags.Sounds;
                flags |= SaveFlags.Music;
                flags |= SaveFlags.SaveData;
            }
        
            loadService.ParseFiles(files, tmpEngine, flags.Value);

        }
        
        public virtual void RunGame()
        {

            ActivateEngine(tmpEngine);

        }
        
    }
}