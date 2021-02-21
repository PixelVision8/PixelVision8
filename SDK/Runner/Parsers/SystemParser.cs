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

using Microsoft.Xna.Framework;
using PixelVision8.Player;
using PixelVision8.Player.Audio;
using PixelVision8.Runner;
using System;
using System.Collections.Generic;

namespace PixelVision8.Runner
{
    public class SystemParser : JsonParser
    {
        protected PixelVision Target;

        public SystemParser(string filePath, IFileLoader fileLoadHelper, PixelVision target) : base(filePath,
            fileLoadHelper)
        {
            Target = target; // as PixelVisionEngine;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();
            Steps.Add(ApplySettings);
        }

        public virtual void ApplySettings()
        {
            if (Target != null)
            {
                var chipManager = Target;

                foreach (var entry in Data)
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
                        case "MetaSprites":
                            ConfigureMetaSprites(chipData);
                            break;
                    }
                }

                // Removed any active chips not reserialized
                // chipManager.RemoveInactiveChips();
            }

            StepCompleted();
        }

        public void ConfigureColorChip(Dictionary<string, object> data)
        {
            var colorChip = Target.ColorChip;

            // Flag chip to export
            //colorChip.export = true;

            // Force the color chip to clear itself
            //            colorChip.Clear();

            //            if (data.ContainsKey("colorsPerPage"))
            //                colorChip.colorsPerPage = (int) (long) data["colorsPerPage"];

            if (data.ContainsKey("maskColor")) colorChip.MaskColor = (string) data["maskColor"];

            // if (data.ContainsKey("maxColors")) colorChip.maxColors = (int)(long)data["maxColors"];

            // Force the color chip to have 256 colors
            colorChip.Total = 256;

            // Make sure we have data to parse
            if (data.ContainsKey("colors"))
            {
                colorChip.Clear();

                // Pull out the color data
                var colors = (List<object>) data["colors"];

                // Clear the colors
                colorChip.Clear();

                // Add all the colors from the data
                for (var i = 0; i < colors.Count; i++) colorChip.UpdateColorAt(i, (string) colors[i]);
            }


            // TODO this is deprecated and only in for legacy support
            // If the data has a page count, resize the pages to match that value, even though there may be less colors than pages
            //            if (data.ContainsKey("pages"))
            //                colorChip.total = (int) (long) data["pages"] * 64;
            //
            //            if (data.ContainsKey("total"))
            //                colorChip.total = (int) (long) data["total"];

            if (data.ContainsKey("backgroundColor")) colorChip.BackgroundColor = (int) (long) data["backgroundColor"];

            if (data.ContainsKey("debug")) colorChip.DebugMode = Convert.ToBoolean(data["debug"]);

            // if (data.ContainsKey("unique")) colorChip.unique = Convert.ToBoolean(data["unique"]);

            //            if (data.ContainsKey("paletteMode"))
            //                colorChip.paletteMode = Convert.ToBoolean(data["paletteMode"]);
        }

        public void ConfigureControllerChip(Dictionary<string, object> data)
        {
            // TODO does this chip need to be configured?
        }

        public void ConfigureDisplayChip(Dictionary<string, object> data)
        {
            var displayChip = Target.DisplayChip;

            // Flag chip to export
            //displayChip.export = true;

            var _width = displayChip.Width;
            var _height = displayChip.Height;

            if (data.ContainsKey("width")) _width = (int) (long) data["width"];

            if (data.ContainsKey("height")) _height = (int) (long) data["height"];

            // if (data.ContainsKey("overscanX")) displayChip.OverscanX = (int) (long) data["overscanX"];

            // if (data.ContainsKey("overscanY")) displayChip.OverscanY = (int) (long) data["overscanY"];

            // if (data.ContainsKey("layers")) displayChip.layers = (int) (long) data["layers"];

            displayChip.ResetResolution(_width, _height);
        }

        public void ConfigureFontChip(Dictionary<string, object> data)
        {
            var fontChip = Target.FontChip;

            if (data.ContainsKey("pages")) fontChip.Pages = (int) (long) data["pages"];

            if (data.ContainsKey("unique")) fontChip.Unique = Convert.ToBoolean(data["unique"]);

            // fontChip.Resize(fontChip.pageWidth, fontChip.pageHeight * fontChip.pages);
        }

        public void ConfigureGameChip(Dictionary<string, object> data)
        {
            var gameChip = Target.GameChip;


            // Flag chip to export
            //gameChip.export = true;

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

            if (data.ContainsKey("maxSize")) gameChip.maxSize = (int) (long) data["maxSize"];

            if (data.ContainsKey("saveSlots")) gameChip.SaveSlots = (int) (long) data["saveSlots"];

            if (data.ContainsKey("lockSpecs")) gameChip.lockSpecs = Convert.ToBoolean(data["lockSpecs"]);

            if (data.ContainsKey("savedData"))
                foreach (var entry in data["savedData"] as Dictionary<string, object>)
                {
                    var name = entry.Key;
                    var value = entry.Value as string;
                    gameChip.WriteSaveData(name, value);
                }


            // TODO need to look for MetaSprite properties
            if (data.ContainsKey("totalMetaSprites")) gameChip.TotalMetaSprites((int) (long) data["totalMetaSprites"]);

            if (data.ContainsKey("metaSprites"))
            {
                // Console.WriteLine("ContainsKey  metaSprites");

                var metaSprites = data["metaSprites"] as List<object>;

                var total = MathHelper.Clamp(metaSprites.Count, 0, gameChip.TotalMetaSprites());

                for (var i = 0; i < total; i++)
                {
                    var metaSprite = gameChip.MetaSprite(i);
                    var spriteData = metaSprites[i] as Dictionary<string, object>;

                    if (spriteData.ContainsKey("name")) metaSprite.Name = spriteData["name"] as string;

                    if (spriteData.ContainsKey("sprites"))
                        if (spriteData["sprites"] is List<object> childSprites)
                        {
                            var subTotal = childSprites.Count;
                            for (var j = 0; j < subTotal; j++)
                            {
                                var childData = childSprites[j] as Dictionary<string, object>;

                                metaSprite.AddSprite(
                                    childData.ContainsKey("id") ? (int) (long) childData["id"] : 0,
                                    childData.ContainsKey("x") ? (int) (long) childData["x"] : 0,
                                    childData.ContainsKey("y") ? (int) (long) childData["y"] : 0,
                                    childData.ContainsKey("flipH") && Convert.ToBoolean(childData["flipH"]),
                                    childData.ContainsKey("flipV") && Convert.ToBoolean(childData["flipV"]),
                                    childData.ContainsKey("colorOffset") ? (int) (long) childData["colorOffset"] : 0
                                );
                            }
                        }
                }

                // var total = metaSprites.Length;
                // for (int i = 0; i < UPPER; i++)
                // {
                //     
                //         var metaSprite = gameChip.MetaSprite()
                //
                //         // TODO need a way to parse this out
                //         Console.WriteLine("Found Sprites");
                //         // var name = entry.Key;
                //         // var value = entry.Value as string;
                //         // gameChip.WriteSaveData(name, value);
                //     }
            }
        }

        public void ConfigureMusicChip(Dictionary<string, object> data)
        {
            var musicChip = Target.MusicChip;

            if (musicChip == null) return;

            // Flag chip to export
            //musicChip.export = true;

            var patternKey = "songs";
            //            var patternNameKey = "songName";

            // Configure chip before parsing song data
            if (data.ContainsKey("totalSongs")) musicChip.totalSongs = Convert.ToInt32((long) data["totalSongs"]);
            if (data.ContainsKey("notesPerTrack")) musicChip.maxNoteNum = Convert.ToInt32((long) data["notesPerTrack"]);

            if (data.ContainsKey("totalPatterns")) musicChip.TotalLoops = Convert.ToInt32((long) data["totalPatterns"]);

            // TODO remove legacy property
            if (data.ContainsKey("totalLoop")) musicChip.TotalLoops = Convert.ToInt32((long) data["totalLoop"]);

            if (data.ContainsKey("version") && (string) data["version"] == "v2")

            {
                patternKey = "patterns";
                //                patternNameKey = "patternName";


                // TODO build song playlist

                // Look for songs
                if (data.ContainsKey("songs"))
                {
                    // Get the list of song data
                    var songsData = data["songs"] as List<object>;
                    var total = Math.Min(songsData.Count, musicChip.totalSongs);

                    // Change the total songs to match the songs in the data
                    // musicChip.totalSongs = total;

                    // Loop through each of teh 
                    for (var i = 0; i < total; i++)
                    {
                        var songData = songsData[i] as Dictionary<string, object>;
                        var song = musicChip.songs[i];

                        if (songData.ContainsKey("songName")) song.name = songData["songName"] as string;

                        if (songData.ContainsKey("patterns"))
                        {
                            var patternData = (List<object>) songData["patterns"];
                            var totalPatterns = patternData.Count;
                            song.patterns = new int[totalPatterns];
                            for (var j = 0; j < totalPatterns; j++) song.patterns[j] = (int) (long) patternData[j];
                        }

                        if (songData.ContainsKey("start")) song.start = Convert.ToInt32((long) songData["start"]);

                        if (songData.ContainsKey("end")) song.end = Convert.ToInt32((long) songData["end"]);
                    }
                }
            }

            //            if (data.ContainsKey("totalTracks"))
            //                musicChip.totalTracks = Convert.ToInt32((long) data["totalTracks"]);


            if (data.ContainsKey(patternKey))
            {
                var patternData = data[patternKey] as List<object>;

                var total = Math.Min(patternData.Count, musicChip.TotalLoops);

                //                musicChip.totalLoops = total;

                for (var i = 0; i < total; i++)
                {
                    var song = musicChip.CreateNewTrackerData("untitled"); //new SfxrSongData());

                    var sngData = patternData[i] as Dictionary<string, object>;

                    //                    if (sngData.ContainsKey(patternNameKey))
                    //                        song.songName = (string) sngData[patternNameKey];

                    if (sngData.ContainsKey("speedInBPM"))
                        song.speedInBPM = Convert.ToInt32((long) sngData["speedInBPM"]);

                    if (sngData.ContainsKey("tracks"))
                    {
                        var tracksData = (List<object>) sngData["tracks"];
                        //                        song.totalTracks = tracksData.Count;

                        var trackCount = MathHelper.Clamp(tracksData.Count, 0, musicChip.totalTracks);

                        for (var j = 0; j < trackCount; j++)
                        {
                            var trackData = tracksData[j] as Dictionary<string, object>;

                            var track = song.tracks[j];

                            if (track != null && trackData != null)
                            {
                                if (trackData.ContainsKey("SfxId"))
                                    track.sfxID = Convert.ToInt32((long) trackData["SfxId"]);

                                if (trackData.ContainsKey("notes"))
                                {
                                    var noteData = (List<object>) trackData["notes"];
                                    var totalNotes = noteData.Count;
                                    track.notes = new int[totalNotes];
                                    for (var k = 0; k < totalNotes; k++) track.notes[k] = (int) (long) noteData[k];
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
            var soundChip = Target.SoundChip as SfxrSoundChip;

            if (soundChip == null) return;

            // Flag chip to export
            //soundChip.export = true;

            if (data.ContainsKey("totalChannels")) soundChip.TotalChannels = (int) (long) data["totalChannels"];

            if (data.ContainsKey("totalSounds")) soundChip.TotalSounds = (int) (long) data["totalSounds"];

            if (data.ContainsKey("channelTypes"))
            {
                var types = (List<object>) data["channelTypes"];

                for (var i = 0; i < types.Count; i++)
                    // Make sure we are only changing channels that exist
                    if (i < soundChip.TotalChannels)
                        soundChip.ChannelType(i, (WaveType) Convert.ToInt32(types[i]));
            }

            // Disabled this for now as I break out into individual files
            if (data.ContainsKey("sounds"))
            {
                var sounds = (List<object>) data["sounds"];

                var total = MathHelper.Clamp(sounds.Count, 0, soundChip.TotalSounds);

                for (var i = 0; i < total; i++)
                {
                    var soundData = soundChip.ReadSound(i);
                    if (soundData != null)
                    {
                        var sndData = sounds[i] as Dictionary<string, object>;

                        if (sndData.ContainsKey("name")) soundData.name = sndData["name"] as string;

                        var tmpSettings = "";
                        var newProps = new string[24];

                        if (sndData.ContainsKey("settings")) tmpSettings = sndData["settings"] as string;

                        // If this this is version 1 we need to convert the settings to 24 value mode
                        if (!data.ContainsKey("version"))
                        {
                            // Remap old format to new format
                            var values = tmpSettings.Split(',');


                            // TODO need to remap the wavs 
                            // waveType
                            newProps[0] = values[0];

                            // attackTime
                            newProps[1] = values[2];

                            // sustainTime
                            newProps[2] = values[3];

                            // sustainPunch 
                            newProps[3] = values[4];

                            // decayTime
                            newProps[4] = values[5];

                            //startFrequency
                            newProps[5] = values[7];

                            // minFrequency
                            newProps[6] = values[8];

                            //slide
                            newProps[7] = values[9];

                            // deltaSlide 
                            newProps[8] = values[10];

                            // vibratoDepth
                            newProps[9] = values[11];

                            // vibratoSpeed 
                            newProps[10] = values[12];

                            // changeAmount
                            newProps[11] = values[16];

                            //changeSpeed
                            newProps[12] = values[17];

                            // squareDuty
                            newProps[13] = values[20];

                            // dutySweep
                            newProps[14] = values[21];

                            // repeatSpeed 
                            newProps[15] = values[22];

                            // phaserOffset 
                            newProps[16] = values[23];

                            // phaserSweep 
                            newProps[17] = values[24];

                            // lpFilterCutoff
                            newProps[18] = values[25];

                            //lpFilterCutoffSweep
                            newProps[19] = values[26];

                            // lpFilterResonance
                            newProps[20] = values[27];

                            //hpFilterCutoff
                            newProps[21] = values[28];

                            // hpFilterCutoffSweep
                            newProps[22] = values[29];

                            // masterVolume 
                            newProps[23] = values[1];

                            tmpSettings = string.Join(",", newProps);
                        }

                        soundData.param = tmpSettings;
                    }
                }
            }
        }

        public void ConfigureSpriteChip(Dictionary<string, object> data)
        {
            var spriteChip = Target.SpriteChip;
            var displayChip = Target.DisplayChip;

            // Flag chip to export
            //spriteChip.export = true;

            if (data.ContainsKey("maxSpriteCount")) displayChip.MaxDrawRequests = (int) (long) data["maxSpriteCount"];

            // if (data.ContainsKey("spriteWidth")) SpriteChip.DefaultSpriteSize = (int) (long) data["spriteWidth"];
            //
            // if (data.ContainsKey("spriteHeight")) SpriteChip.DefaultSpriteSize = (int) (long) data["spriteHeight"];

            if (data.ContainsKey("cps")) spriteChip.ColorsPerSprite = (int) (long) data["cps"];

            if (data.ContainsKey("pages")) spriteChip.Pages = (int) (long) data["pages"];

            if (data.ContainsKey("unique")) spriteChip.Unique = Convert.ToBoolean(data["unique"]);

            // spriteChip.Resize(spriteChip.pageWidth, spriteChip.pageHeight * spriteChip.pages);
        }

        public void ConfigureTilemapChip(Dictionary<string, object> data)
        {
            var tilemapChip = Target.TilemapChip;

            // Flag chip to export
            //tilemapChip.export = true;

            var columns = tilemapChip.Columns;
            var rows = tilemapChip.Rows;

            if (data.ContainsKey("columns")) columns = (int) (long) data["columns"];

            if (data.ContainsKey("rows")) rows = (int) (long) data["rows"];

            // if (data.ContainsKey("totalFlags")) tilemapChip.totalFlags = (int) (long) data["totalFlags"];

            if (data.ContainsKey("autoImport")) tilemapChip.autoImport = Convert.ToBoolean(data["autoImport"]);

            tilemapChip.Resize(columns, rows);
        }

        // Meta Sprite Json Example
        // {
        //     "MetaSprites": {
        //         "version": "v1",
        //         "total": 96,
        //         "collections": [{
        //             "name": "tileset_1",
        //             "spriteIDs": [0, 1, 24, 25],
        //             "sprites": [{
        //                 "id": 0,
        //                 "x": 0,
        //                 "y": 0,
        //                 "flipH": false,
        //                 "flipV": false,
        //                 "colorOffset": 0
        //
        //             }],
        //             "width": 2,
        //             "colorOffset": 0
        //         }]
        //     }
        // }
        public void ConfigureMetaSprites(Dictionary<string, object> data)
        {
            var gameChip = Target.GameChip;

            // Prepare to parse v1 of the MetaSprite json template/
            if (data.ContainsKey("version") && (string) data["version"] == "v1")
            {
                if (data.ContainsKey("total"))
                    gameChip.TotalMetaSprites(Convert.ToInt32((long) data["total"]));

                var spriteWidth = data.ContainsKey("spriteWidth")
                    ? Convert.ToInt32((long) data["spriteWidth"])
                    : gameChip.SpriteSize().X;
                var spriteHeight = data.ContainsKey("spriteHeight")
                    ? Convert.ToInt32((long) data["spriteHeight"])
                    : gameChip.SpriteSize().Y;

                // Look for songs
                if (data.ContainsKey("collections"))
                {
                    // Get the list of song data
                    var collections = data["collections"] as List<object>;
                    var total = Math.Min(collections.Count, gameChip.TotalMetaSprites());

                    // Loop through each of teh 
                    for (var i = 0; i < total; i++)
                    {
                        var collectionData = collections[i] as Dictionary<string, object>;
                        var metaSprite = gameChip.MetaSprite(i);

                        // TODO this is redundant
                        metaSprite.SpriteWidth = spriteWidth;
                        metaSprite.SpriteHeight = spriteHeight;

                        if (collectionData.ContainsKey("name")) metaSprite.Name = collectionData["name"] as string;

                        // Test to see if the basic sprite data exists, IDs and Width.
                        if (collectionData.ContainsKey("spriteIDs") && collectionData.ContainsKey("width"))
                        {
                            var width = Convert.ToInt32((long) collectionData["width"]);

                            var spriteData = (List<object>) collectionData["spriteIDs"];
                            var totalSprites = spriteData.Count;

                            for (var j = 0; j < totalSprites; j++)
                            {
                                if (Convert.ToInt32((long) spriteData[j]) > -1)
                                {
                                    var pos = Utilities.CalculatePosition(j, width);

                                    metaSprite.AddSprite(Convert.ToInt32((long) spriteData[j]), pos.X * spriteWidth,
                                        pos.Y * spriteHeight);
                                }
                            }

                            collections[i] = collectionData;
                        }
                        // Test to see if the more advanced sprite data exists
                        else if (collectionData.ContainsKey("sprites"))
                        {
                            // TODO this is where we need to manually create each sprite in the collection
                        }
                    }
                }
            }
        }
    }

    public partial class Loader
    {
        [FileParser("data.png", FileFlags.System)]
        public void ParseSystem(string file, PixelVision engine)
        {
            // if (!string.IsNullOrEmpty(files[0]))
            // {
            // var fileContents = Encoding.UTF8.GetString(ReadAllBytes(file));

            var jsonParser = new SystemParser(file, _fileLoadHelper, engine);

            jsonParser.CalculateSteps();

            while (jsonParser.Completed == false) jsonParser.NextStep();
            // }
        }
    }
}