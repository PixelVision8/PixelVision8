using System;
using System.Collections.Generic;
using PixelVision8.Player;

namespace PixelVision8.Runner
{
    public partial class SystemParser
    {
        [ChipParser("MusicChip")]
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

                        var trackCount = Utilities.Clamp(tracksData.Count, 0, musicChip.totalTracks);

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
    }
}