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
using Microsoft.Xna.Framework;
using PixelVision8.Engine;
using PixelVision8.Engine.Utils;

namespace PixelVision8.Runner.Parsers
{
    public class SystemParser : JsonParser
    {
        protected IEngine target;

        public SystemParser(IEngine target, string jsonString = "") : base(jsonString)
        {
            this.target = target;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();
            steps.Add(ApplySettings);
        }

        public virtual void ApplySettings()
        {
            if (target != null)
            {
                var chipManager = target;

                foreach (var entry in data)
                {
                    var fullName = entry.Key;
                    var split = fullName.Split('.');
                    var chipName = split[split.Length - 1];
                    var chipData = entry.Value as Dictionary<string, object>;

                    switch (chipName)
                    {
                        case "ColorChip":
                            ConfigureColorChip(chipData);
                            break;
                        case "ControllerChip":
                            ConfigureControllerChip(chipData);
                            break;
                        case "DisplayChip":
                            ConfigureDisplayChip(chipData);
                            break;
                        case "FontChip":
                            ConfigureFontChip(chipData);
                            break;
                        case "GameChip":
                        case "LuaGameChip":
                        case "LuaToolChip":
                            ConfigureGameChip(chipData);
                            break;
                        case "MusicChip":
                            ConfigureMusicChip(chipData);
                            break;
                        case "SoundChip":
                            ConfigureSoundChip(chipData);
                            break;
                        case "SpriteChip":
                            ConfigureSpriteChip(chipData);
                            break;
                        case "TilemapChip":
                            ConfigureTilemapChip(chipData);
                            break;
                    }
                }

                // Removed any active chips not reserialized
                chipManager.RemoveInactiveChips();
            }

            currentStep++;
        }

        public void ConfigureColorChip(Dictionary<string, object> data)
        {
            var colorChip = target.colorChip;

            // Flag chip to export
            colorChip.export = true;

            // Force the color chip to clear itself
            colorChip.Clear();

//            if (data.ContainsKey("colorsPerPage"))
//                colorChip.colorsPerPage = (int) (long) data["colorsPerPage"];

            if (data.ContainsKey("maskColor"))
                colorChip.maskColor = (string) data["maskColor"];

            if (data.ContainsKey("maxColors"))
                colorChip.maxColors = (int) (long) data["maxColors"];

            // Make sure we have data to parse
            if (data.ContainsKey("colors"))
            {
                // Pull out the color data
                var colors = (List<object>) data["colors"];

                var newTotal = colors.Count;
                colorChip.total = newTotal;
                colorChip.Clear();
                for (var i = 0; i < newTotal; i++)
                    colorChip.UpdateColorAt(i, (string) colors[i]);
            }


            // TODO this is deprecated and only in for legacy support
            // If the data has a page count, resize the pages to match that value, even though there may be less colors than pages
            if (data.ContainsKey("pages"))
                colorChip.total = (int) (long) data["pages"] * 64;

            if (data.ContainsKey("total"))
                colorChip.total = (int) (long) data["total"];

            if (data.ContainsKey("backgroundColor"))
                colorChip.backgroundColor = (int) (long) data["backgroundColor"];

            if (data.ContainsKey("debug"))
                colorChip.debugMode = Convert.ToBoolean(data["debug"]);

            if (data.ContainsKey("unique"))
                colorChip.unique = Convert.ToBoolean(data["unique"]);

//            if (data.ContainsKey("paletteMode"))
//                colorChip.paletteMode = Convert.ToBoolean(data["paletteMode"]);
        }

        public void ConfigureControllerChip(Dictionary<string, object> data)
        {
            // TODO does this chip need to be configured?
        }

        public void ConfigureDisplayChip(Dictionary<string, object> data)
        {
            var displayChip = target.displayChip;

            // Flag chip to export
            displayChip.export = true;

            var _width = displayChip.width;
            var _height = displayChip.height;

            if (data.ContainsKey("width"))
                _width = (int) (long) data["width"];

            if (data.ContainsKey("height"))
                _height = (int) (long) data["height"];

            if (data.ContainsKey("overscanX"))
                displayChip.overscanX = (int) (long) data["overscanX"];

            if (data.ContainsKey("overscanY"))
                displayChip.overscanY = (int) (long) data["overscanY"];

            if (data.ContainsKey("layers"))
                displayChip.layers = (int) (long) data["layers"];

            displayChip.ResetResolution(_width, _height);
        }

        public void ConfigureFontChip(Dictionary<string, object> data)
        {
            // TODO does this chip need to be parsed?
        }

        public void ConfigureGameChip(Dictionary<string, object> data)
        {
            var gameChip = target.gameChip;

            // Flag chip to export
            gameChip.export = true;

            // loop through all data and save it to the game's memory

//            if (data.ContainsKey("name"))
//                gameChip.name = (string) data["name"];
//
//            if (data.ContainsKey("description"))
//                gameChip.description = (string) data["description"];
//
//            if (data.ContainsKey("version"))
//                gameChip.version = (string) data["version"];
//
//            if (data.ContainsKey("ext"))
//                gameChip.ext = (string) data["ext"];

            if (data.ContainsKey("maxSize"))
                gameChip.maxSize = (int) (long) data["maxSize"];

            if (data.ContainsKey("saveSlots"))
                gameChip.saveSlots = (int) (long) data["saveSlots"];

            if (data.ContainsKey("lockSpecs"))
                gameChip.lockSpecs = Convert.ToBoolean(data["lockSpecs"]);


            if (data.ContainsKey("savedData"))
                foreach (var entry in data["savedData"] as Dictionary<string, object>)
                {
                    var name = entry.Key;
                    var value = entry.Value as string;
                    gameChip.WriteSaveData(name, value);
                }
        }

        public void ConfigureMusicChip(Dictionary<string, object> data)
        {
            var musicChip = target.musicChip;

            if (musicChip == null)
                return;

            // Flag chip to export
            musicChip.export = true;

            var patternKey = "songs";
            var patternNameKey = "songName";

            if (data.ContainsKey("version") && (string) data["version"] == "v2")

            {
                patternKey = "patterns";
                patternNameKey = "patternName";


                // TODO build song playlist

                // Look for songs
                if (data.ContainsKey("songs"))
                {
                    // Get the list of song data
                    var songsData = data["songs"] as List<object>;
                    var total = songsData.Count;

                    // Change the total songs to match the songs in the data
                    musicChip.totalSongs = total;

                    // Loop through each of teh 
                    for (var i = 0; i < total; i++)
                    {
                        var songData = songsData[i] as Dictionary<string, object>;
                        var song = musicChip.songs[i];

                        if (songData.ContainsKey("songName"))
                            song.name = songData["songName"] as string;

                        if (songData.ContainsKey("patterns"))
                        {
                            var patternData = (List<object>) songData["patterns"];
                            var totalPatterns = patternData.Count;
                            song.patterns = new int[totalPatterns];
                            for (var j = 0; j < totalPatterns; j++)
                                song.patterns[j] = (int) (long) patternData[j];
                        }

                        if (songData.ContainsKey("start"))
                            song.start = Convert.ToInt32((long) songData["start"]);

                        if (songData.ContainsKey("end"))
                            song.end = Convert.ToInt32((long) songData["end"]);
                    }
                }
            }

            if (data.ContainsKey("totalTracks"))
                musicChip.totalTracks = Convert.ToInt32((long) data["totalTracks"]);

            if (data.ContainsKey("notesPerTrack"))
                musicChip.maxNoteNum = Convert.ToInt32((long) data["notesPerTrack"]);

            if (data.ContainsKey("totalSongs"))
                musicChip.totalSongs = Convert.ToInt32((long) data["totalSongs"]);

            if (data.ContainsKey("totalPatterns"))
                musicChip.totalLoops = Convert.ToInt32((long) data["totalPatterns"]);

            // TODO remove legacy property
            if (data.ContainsKey("totalLoop"))
                musicChip.totalLoops = Convert.ToInt32((long) data["totalLoop"]);


            if (data.ContainsKey(patternKey))
            {
                var patternData = data[patternKey] as List<object>;

                var total = patternData.Count;

//                musicChip.totalLoops = total;

                for (var i = 0; i < total; i++)
                {
                    var song = musicChip.CreateNewTrackerData("untitled"); //new SfxrSongData());

                    var sngData = patternData[i] as Dictionary<string, object>;

                    if (sngData.ContainsKey(patternNameKey))
                        song.songName = (string) sngData[patternNameKey];

                    if (sngData.ContainsKey("speedInBPM"))
                        song.speedInBPM = Convert.ToInt32((long) sngData["speedInBPM"]);

                    if (sngData.ContainsKey("tracks"))
                    {
                        var tracksData = (List<object>) sngData["tracks"];
//                        song.totalTracks = tracksData.Count;

                        var trackCount = MathHelper.Clamp(tracksData.Count, 0, song.totalTracks);

                        for (var j = 0; j < trackCount; j++)
                        {
                            var trackData = tracksData[j] as Dictionary<string, object>;

                            var track = song.tracks[j];

                            if (track != null && trackData != null)
                            {
                                if (trackData.ContainsKey("sfxID"))
                                    track.sfxID = Convert.ToInt32((long) trackData["sfxID"]);

                                if (trackData.ContainsKey("notes"))
                                {
                                    var noteData = (List<object>) trackData["notes"];
                                    var totalNotes = noteData.Count;
                                    track.notes = new int[totalNotes];
                                    for (var k = 0; k < totalNotes; k++)
                                        track.notes[k] = (int) (long) noteData[k];
                                }
                            }
                        }
                    }


                    musicChip.trackerDataCollection[i] = song;
                }
            }

//            }
        }

        public void ConfigureSoundChip(Dictionary<string, object> data)
        {
            var soundChip = target.soundChip;

            if (soundChip == null)
                return;

            // Flag chip to export
            soundChip.export = true;

            if (data.ContainsKey("totalChannels"))
                soundChip.totalChannels = (int) (long) data["totalChannels"];

            if (data.ContainsKey("totalSounds"))
                soundChip.totalSounds = (int) (long) data["totalSounds"];


            // Disabled this for now as I break out into individual files
            if (data.ContainsKey("sounds"))
            {
                var sounds = (List<object>) data["sounds"];

                var total = MathHelper.Clamp(sounds.Count, 0, soundChip.totalSounds);

                for (var i = 0; i < total; i++)
                {
                    var soundData = soundChip.ReadSound(i);
                    if (soundData != null)
                    {
                        var sndData = sounds[i] as Dictionary<string, object>;

                        if (sndData.ContainsKey("name"))
                            soundData.name = sndData["name"] as string;

                        if (sndData.ContainsKey("settings"))
                            soundData.UpdateSettings(sndData["settings"] as string);
                    }
                }
            }
        }

        public void ConfigureSpriteChip(Dictionary<string, object> data)
        {
            var spriteChip = target.spriteChip;

            // Flag chip to export
            spriteChip.export = true;

            if (data.ContainsKey("maxSpriteCount"))
                spriteChip.maxSpriteCount = (int) (long) data["maxSpriteCount"];

            if (data.ContainsKey("spriteWidth"))
                spriteChip.width = (int) (long) data["spriteWidth"];

            if (data.ContainsKey("spriteHeight"))
                spriteChip.height = (int) (long) data["spriteHeight"];

            if (data.ContainsKey("cps"))
                spriteChip.colorsPerSprite = (int) (long) data["cps"];

            if (data.ContainsKey("pages"))
                spriteChip.pages = (int) (long) data["pages"];

            if (data.ContainsKey("unique"))
                spriteChip.unique = Convert.ToBoolean(data["unique"]);

            spriteChip.Resize(spriteChip.pageWidth, spriteChip.pageHeight * spriteChip.pages);
        }

        public void ConfigureTilemapChip(Dictionary<string, object> data)
        {
            var tilemapChip = target.tilemapChip;

            // Flag chip to export
            tilemapChip.export = true;

            var columns = tilemapChip.columns;
            var rows = tilemapChip.rows;

            if (data.ContainsKey("columns"))
                columns = (int) (long) data["columns"];

            if (data.ContainsKey("rows"))
                rows = (int) (long) data["rows"];

            if (data.ContainsKey("totalFlags"))
                tilemapChip.totalFlags = (int) (long) data["totalFlags"];

            if (data.ContainsKey("autoImport"))
                tilemapChip.autoImport = Convert.ToBoolean(data["autoImport"]);

            tilemapChip.Resize(columns, rows);
        }
    }
}