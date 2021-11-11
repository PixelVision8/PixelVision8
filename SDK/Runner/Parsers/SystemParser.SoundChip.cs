using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelVision8.Player;

namespace PixelVision8.Runner
{
    public partial class SystemParser
    {
        [ChipParser("SoundChip")]
        public void ConfigureSoundChip(Dictionary<string, object> data)
        {
            var soundChip = Target.SoundChip;// as SfxrSoundChip;

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
    }
}