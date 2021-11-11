
Pixel Vision 8 supports playing sound effects at runtime. These SFX files are based on [Sfxr](http://www.drpetter.se/project_sfxr.html) and are stored in the `sounds.json` file. Each game can store a specific amount of sound effects and each one can be played back on one of the available sound channels. Each channel can be configured to playback any supported waveform or can be locked to only playback a specific type in order to create a sound system that closely resembles older 8-bit system limitations.

## Sound Data

Pixel Vision 8 sound effects are stored in the `sounds.json` file. This file defines all of the available sound effects that your game can playback at runtime. Each sound effect is stored as a 24 value comma-delimited string. Here is a mapping of each sound property.

| Order | Property            | Notes                                                                                    |
|-------|---------------------|------------------------------------------------------------------------------------------|
| 1     | waveType            | Supports: Square \(0\), Saw \(1\), Sine \(2\), Noise \(3\), Triangle \(4\), Sample \(5\) |
| 2     | attackTime          | Range 0 to 1                                                                             |
| 3     | sustainTime         | Range 0 to 1                                                                             |
| 4     | sustainPunch        | Range 0 to 1                                                                             |
| 5     | decayTime           | Range 0 to 1                                                                             |
| 6     | startFrequency      | Range 0 to 1                                                                             |
| 7     | minFrequency        | Range 0 to 1                                                                             |
| 8     | slide               | Range \-1 to 1                                                                           |
| 9     | deltaSlide          | Range \-1 to 1                                                                           |
| 10    | vibratoDepth        | Range 0 to 1                                                                             |
| 11    | vibratoSpeed        | Range 0 to 1                                                                             |
| 12    | changeAmount        | Range \-1 to 1                                                                           |
| 13    | changeSpeed         | Range 0 to 1                                                                             |
| 14    | squareDuty          | Range 0 to 1                                                                             |
| 15    | dutySweep           | Range \-1 to 1                                                                           |
| 16    | repeatSpeed         | Range 0 to 1                                                                             |
| 17    | phaserOffset        | Range \-1 to 1                                                                           |
| 18    | phaserSweep         | Range \-1 to 1                                                                           |
| 19    | lpFilterCutoff      | Range 0 to 1                                                                             |
| 20    | lpFilterCutoffSweep | Range \-1 to 1                                                                           |
| 21    | lpFilterResonance   | Range 0 to 1                                                                             |
| 22    | hpFilterCutoff      | Range 0 to 1                                                                             |
| 23    | hpFilterCutoffSweep | Range \-1 to 1                                                                           |
| 24    | masterVolume        | Range 0 to 1                                                                             |

You can use any Sfxr sound generator to create a compatible 24 parameter string file for each sound effect.

Each sound effect is stored inside of the `SoundChip`’s `sounds` array property. Here is an example of how the file is structured:

```json
{
  "SoundChip": {
    "sounds": [
      {
        "name": "Melody",
        "settings": "0,.05,,.2,,.2,.3,.1266,,,,,,,,,,,,,,,,,,1,,,,,,"
      }
    ]
  }
}
```

This file will contain the maximum number of sound effects your game can support which is defined in the `data.json` file. You can change this cap by modifying the `totalSounds `property of the `SoundChip`.

Finally, you can configure the sound channels in the data.json file. The `SoundChip` contains a property called `channelType` with an array of values that map to each wave type. 

```json
"channelTypes": [-1, -1, -1, -1, -1]
```

Setting a channel to `-1` will support playing any wave type. If you set it to a specific wave type ID, that channel will override the sound effect’s `waveType `property. Here is an example of how to configure the channels to match up to a NES’s specs:

```json
"channelTypes": [0, 0, 4, 3, 5]
```

In this configuration, the `SoundChip` will enforce channels 1 & 2 as `Square`, channel 3 as `Triangle`, channel 4 as `Noise`, and channel 5 as `Sample` which allows you to only play `.wav` files. It’s important to understand the connection between the sound effect data’s `waveTyp`e property and the `SoundChip`’s `channelTypes` to correctly playback each sound effect as expected.



