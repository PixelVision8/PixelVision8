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
using System.Collections.Generic;

using PixelVisionSDK;

namespace PixelVisionRunner.Parsers
{
    public class SystemParser : JsonParser
    {
        protected IEngine target;

        public SystemParser(string jsonString, IEngine target) : base(jsonString)
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
                var chipManager = target.chipManager;

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
                            ConfigurMusicChip(chipData);
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

            if (data.ContainsKey("colorsPerPage"))
                colorChip.colorsPerPage = (int) (long) data["colorsPerPage"];

            if (data.ContainsKey("maskColor"))
                colorChip.maskColor = (string) data["maskColor"];

            if (data.ContainsKey("supportedColors"))
                colorChip.supportedColors = (int) (long) data["supportedColors"];

            // Make sure we have data to parse
            if (data.ContainsKey("colors"))
            {
                // Pull out the color data
                var colors = (List<object>) data["colors"];

                var newTotal = colors.Count;
                colorChip.RebuildColorPages(newTotal);
                colorChip.Clear();
                for (var i = 0; i < newTotal; i++)
                    colorChip.UpdateColorAt(i, (string) colors[i]);
            }

            // If the data has a page count, resize the pages to match that value, even though there may be less colors than pages
            if (data.ContainsKey("pages"))
                colorChip.pages = (int) (long) data["pages"];

            if (data.ContainsKey("backgroundColor"))
                colorChip.backgroundColor = (int) (long) data["backgroundColor"];
            
            if (data.ContainsKey("debug"))
                colorChip.debugMode = Convert.ToBoolean(data["debug"]);
        }

        public void ConfigureControllerChip(Dictionary<string, object> data)
        {
        }

        public void ConfigureDisplayChip(Dictionary<string, object> data)
        {
            var displayChip = target.displayChip;

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
        }

        public void ConfigureGameChip(Dictionary<string, object> data)
        {
            var gameChip = target.gameChip;

            // loop through all data and save it to the game's memory

            if (data.ContainsKey("name"))
                gameChip.name = (string) data["name"];

            if (data.ContainsKey("description"))
                gameChip.description = (string) data["description"];

            if (data.ContainsKey("version"))
                gameChip.version = (string) data["version"];

            if (data.ContainsKey("ext"))
                gameChip.ext = (string) data["ext"];

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

        public void ConfigurMusicChip(Dictionary<string, object> data)
        {
            var musicChip = target.musicChip;

            if (musicChip == null)
                return;
            
            if (data.ContainsKey("songs"))
            {
                var songData = data["songs"] as List<object>;

                var total = songData.Count;

                musicChip.totalLoops = total;

                for (var i = 0; i < total; i++)
                {
                    var song = musicChip.CreateNewSongData("untitled");//new SfxrSongData());

                    var sngData = songData[i] as Dictionary<string, object>;
                    
                    if (sngData.ContainsKey("songName"))
                        song.songName = (string) sngData["songName"];

                    if (sngData.ContainsKey("speedInBPM"))
                        song.speedInBPM = Convert.ToInt32((long) sngData["speedInBPM"]);

                    if (sngData.ContainsKey("tracks"))
                    {
                        var tracksData = (List<object>) sngData["tracks"];
                        song.totalTracks = tracksData.Count;

                        for (var j = 0; j < song.totalTracks; j++)
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
                    
                    
                    musicChip.songDataCollection[i] = song;
                }
            }

            if (data.ContainsKey("totalTracks"))
                musicChip.totalTracks = Convert.ToInt32((long) data["totalTracks"]);

            if (data.ContainsKey("notesPerTrack"))
                musicChip.maxNoteNum = Convert.ToInt32((long) data["notesPerTrack"]);

            if (data.ContainsKey("totalLoop"))
                musicChip.totalLoops = Convert.ToInt32((long) data["totalLoop"]);
        }

        public void ConfigureSoundChip(Dictionary<string, object> data)
        {
            var soundChip = target.soundChip;
            
            if (soundChip == null)
                return;
            
            if (data.ContainsKey("totalChannels"))
                soundChip.totalChannels = (int) (long) data["totalChannels"];

            if (data.ContainsKey("totalSounds"))
                soundChip.totalSounds = (int) (long) data["totalSounds"];

            // Disabled this for now as I break out into individual files
            if (data.ContainsKey("sounds"))
            {
                var sounds = (List<object>) data["sounds"];

                var total = sounds.Count;

                soundChip.totalSounds = total;
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

            var columns = tilemapChip.columns;
            var rows = tilemapChip.rows;

            if (data.ContainsKey("columns"))
                columns = (int) (long) data["columns"];

            if (data.ContainsKey("rows"))
                rows = (int) (long) data["rows"];

            if (data.ContainsKey("totalFlags"))
                tilemapChip.totalFlags = (int) (long) data["totalFlags"];

            tilemapChip.Resize(columns, rows);
        }
    }
}