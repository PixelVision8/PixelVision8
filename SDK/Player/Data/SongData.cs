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
using System.Text;
// using PixelVision8.Runner;

namespace PixelVision8.Player
{
    public class SongData : AbstractData
    {
        public int _end = 1;
        private int[] _patterns = new int[100];
        public int _start;


        public int currentPos = -1;
        public string name = "Untitled";
        public int totalPatternsPerSong = 100;

        public int[] patterns
        {
            get => _patterns;
            set
            {
                if (value.Length > totalPatternsPerSong) Array.Resize(ref value, totalPatternsPerSong);

                _patterns = value;

                // TODO need to change start

                end = _patterns.Length - 1;

                // TODO end should match up with the length of the song before it is resized or the end if it is bigger

                // TODO should we make a copy of the value?
            }
        }

        public int start
        {
            get => _start;
            // Make sure the start is always less than the end position or the total patterns per song
            set => _start = Utilities.Clamp(value, 0, Math.Min(_end, totalPatternsPerSong) - 1);
        }

        public int end
        {
            get => _end;
            // Always make sure the end position is greater than the start position and less than the total patterns per song
            set => _end = Utilities.Clamp(value, start + 1, totalPatternsPerSong);
        }

        //        public SongData(int[] patterns, int start, int end, bool loops = true)
        //        {
        //            
        //        }

        public int NextPattern()
        {
            currentPos++;

            if (AtEnd()) currentPos = 0;

            //            Console.WriteLine("Load pattern " + currentPos);
            return _patterns[currentPos];
        }

        public bool AtEnd()
        {
            //            Console.WriteLine("End "+ end + " " + currentPos);

            return currentPos >= end;
        }

        public void Rewind()
        {
            currentPos = -1;

            //            return NextPattern();
        }

        public int SeekTo(int index)
        {
            currentPos = Utilities.Clamp(index, -1, _patterns.Length - 1);

            return currentPos;
        }

        public void UpdatePatternAt(int index, int id)
        {
            _patterns[index] = id;
        }

    }
}