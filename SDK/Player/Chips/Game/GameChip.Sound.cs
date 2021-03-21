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

namespace PixelVision8.Player
{
    public partial class GameChip
    {
        protected SoundChip SoundChip => Player.SoundChip;

        #region Sound

        /// <summary>
        ///     This method plays back a sound on a specific channel. The SoundChip has a limit of
        ///     active channels so playing a sound effect while another was is playing on the same
        ///     channel will cancel it out and replace with the new sound.
        /// </summary>
        /// <param name="id">
        ///     The ID of the sound in the SoundCollection.
        /// </param>
        /// <param name="channel">
        ///     The channel the sound should play back on. Channel 0 is set by default.
        /// </param>
        public void PlaySound(int id, int channel = 0)
        {
            SoundChip.PlaySound(id, channel);
        }
        
        /// <summary>
        ///     Use StopSound() to stop any sound playing on a specific channel.
        /// </summary>
        /// <param name="channel">The channel ID to stop a sound on.</param>
        public void StopSound(int channel = 0)
        {
            SoundChip.StopSound(channel);
        }

        /// <summary>
        ///     Returns a bool if the channel is playing a sound.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public bool IsChannelPlaying(int channel)
        {
            return SoundChip.IsChannelPlaying(channel);
        }

        #endregion
    }
}