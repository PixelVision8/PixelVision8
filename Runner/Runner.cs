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

using PixelVisionRunner.Chips;
using PixelVisionRunner.Services;
using PixelVisionRunner.Utils;
using PixelVisionSDK;
using PixelVisionSDK.Chips;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace PixelVisionRunner
{
    public class Runner
    {
        private IEngine engine;

        private SimplePromise<Stream> openPv8;

        private IDisplayTarget displayTarget;

        private IColorFactory colorFactory;

        private LoadService loadService;

        private IInputFactory inputFactory;

        public Runner(
            SimplePromise<Stream> openPv8,
            IDisplayTarget displayTarget,
            ITextureFactory textureFactory,
            IColorFactory colorFactory,
            IInputFactory inputFactory)
        {
            this.openPv8 = openPv8;
            this.displayTarget = displayTarget;
            this.loadService = new LoadService(textureFactory, colorFactory);
            this.colorFactory = colorFactory;
            this.inputFactory = inputFactory;
        }

        public void Initialize(IEnumerable<string> addOnChips = null)
        {
            var chips = new[]
            {
                typeof(ColorChip).FullName,
                typeof(SpriteChip).FullName,
                typeof(TilemapChip).FullName,
                typeof(FontChip).FullName,
                typeof(ControllerChip).FullName,
                typeof(DisplayChip).FullName,
                typeof(ControllerChip).FullName,
                typeof(LuaGameChip).FullName,
                typeof(SfxrMusicChip).FullName,
                typeof(SfxrSoundChip).FullName,
            };

            if (addOnChips != null)
                chips.Concat(addOnChips);

            engine = new PixelVisionEngine(chips.ToArray());

            engine.chipManager.AddService(typeof(LuaService).FullName, new LuaService());

            openPv8
                .Then(LoadGame)
                .Then(StartEngine)
                .Execute();
        }

        public void Update(float delta)
        {
            if (engine == null)
                return;

            engine.Update(delta);
        }

        public void Draw()
        {
            if (engine == null)
                return;

            engine.Draw();

            var pixelMap = engine.displayChip.displayPixels;

            var colors = engine.colorChip.colors;

            var backgroundColor = engine.colorChip.backgroundColor;

            var pixelColors = pixelMap.Select(p =>
                (p < 0 || p >= colors.Length)
                    ? colors[backgroundColor]
                    : colors[p]
                )
                .Select(c => colorFactory.Create(c.r, c.g, c.b))
                .ToArray();

//            displayTarget.Render(pixelMap, backgroundColor);//pixelColors);
        }

        private Unit LoadGame(Stream gameContent)
        {
            var flags = SaveFlags.System;
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

            loadService.ParseFiles(ExtractZipFromMemoryStream(gameContent), engine, flags);
            loadService.LoadAll();

            return Unit.Value;
        }

        private Unit StartEngine(Unit _)
        {
            ResetResolution(engine.displayChip.width, engine.displayChip.height);
            ConfigureInput();
            engine.RunGame();
            return Unit.Value;
        }

        private static Dictionary<string, byte[]> ExtractZipFromMemoryStream(Stream stream)
        {
            var zip = ZipStorer.Open(stream, FileAccess.Read);

            var dir = zip.ReadCentralDir();

            var files = new Dictionary<string, byte[]>();

            // Look for the desired file
            foreach (var entry in dir)
            {
                var fileBytes = new byte[0];
                zip.ExtractFile(entry, out fileBytes);

                files.Add(entry.ToString(), fileBytes);
            }

            zip.Close();

            return files;
        }

        private void ResetResolution(int width, int height, bool fullScreen = false)
        {
            engine.displayChip.ResetResolution(width, height);
            displayTarget.ResetResolution(width, height, fullScreen);
        }

        private void ConfigureInput()
        {
            var activeControllerChip = engine.controllerChip;

            activeControllerChip.RegisterKeyInput(inputFactory.CreateKeyInput());

            var buttons = Enum.GetValues(typeof(Buttons)).Cast<Buttons>();
            foreach (var button in buttons)
            {
                activeControllerChip.UpdateControllerKey(0, inputFactory.CreateButtonBinding(0, button));
                activeControllerChip.UpdateControllerKey(1, inputFactory.CreateButtonBinding(1, button));
            }

            activeControllerChip.RegisterMouseInput(inputFactory.CreateMouseInput());
        }
    }
}