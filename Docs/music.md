Pixel Vision 8 allows the playback of music at runtime. A song is a collection of patterns. Each pattern can contain up to 32 notes across 5 channels. The total amount of channels is determined by the `SoundChip`’s `totalChannels` property in the `data.json` file. A song can contain up to 100 patterns. All of this is stored in the `music.json` file.

## Music Data

All of the game’s songs and patterns data are stored in the `music.json` file. This file is broken up into two parts. The top containing the song data and the bottom which contains the pattern data. 

Patterns are the building block for songs and contain the playback speed property called `speedInBPM`, as well as an array of tracks. Each track references a sound effect ID to use as the instrument for playback. Each instrument will play but based on the channel, the `waveType` may be overridden. Each pattern track maps back to the Sound Chip’s channels. So if channel `0` is set to `Square`, any instrument played on this channel will be forced to use the `Square` waveform.

Each track contains an array of up to 32 notes. These notes are mapped to MIDI values. Here is a list of the supported MIDI values and their corresponding notes at an octave of 1.

| Note | MIDI |
|------|------|
| C    | 24   |
| C\#  | 25   |
| D    | 26   |
| D\#  | 27   |
| E    | 28   |
| F    | 29   |
| F\#  | 30   |
| G    | 31   |
| G\#  | 32   |
| A    | 33   |
| A\#  | 34   |
| B    | 35   |

If you want to move up or down an octave simply add or subtract 12 to the MIDI value.

Songs are simply a collection of patterns. You can think of them almost like a playlist. Here is an example of a truncated `music.json` file:

```json
{
  "MusicChip": {
    "version": "v2",
    "songs": [
      {
        "songName": "Song1",
        "start": 0,
        "end": 5,
        "patterns": [0,1,2, ... ]
      },
      ...
    ],
    "patterns": [
      {
        "patternName": "PV8",
        "speedInBPM": 320,
        "tracks": [
          {
            "sfxID": 0,
            "notes":[36,0,0, ... ]
          },
          {
            "sfxID": 1,
            "notes":[40,0,0, ... ]
          },
          {
            "sfxID": 2,
            "notes":[48,0,0, ... ]
          },
          {
            "sfxID": 3,
            "notes":[36,0,0, ... ]
          }
        ],
      ...
      }
    ]
  }
}
```

Each song block contains a `start` and `end` position as well as an array of `patterns`. Since each song contains 100 patterns, the `end` position will define where a song should stop playing. Each `patterns` array defaults all of its values to the first pattern which is ID `0` if you do not define other pattern IDs when it is loaded into memory.