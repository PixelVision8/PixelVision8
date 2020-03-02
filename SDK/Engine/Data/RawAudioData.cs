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

namespace PixelVision8.Engine
{
    public class RawAudioData : AbstractData
    {
        public float[] data;

        public RawAudioData(int samples = 0, int channels = 1, int frequency = 22050)
        {
            this.samples = samples;
            this.channels = channels;
            this.frequency = frequency;
            data = new float[samples];
        }

        public int samples { get; set; }
        public int channels { get; }

        public int frequency { get; }

        public void SetData(float[] data, int offsetSamples = 0)
        {
            var total = data.Length;
            for (var i = 0; i < total; i++)
            {
                var index = i + offsetSamples;

                if (index < samples) this.data[index] = data[i];
            }
        }

        public void Resize(int samples)
        {
            Array.Resize(ref data, samples);
            this.samples = samples;
        }

        // CONVERT TO WAV

        /// <summary>
        ///     Returns a ByteArray of the wave in the form of a .wav file, ready to be saved out
        /// </summary>
        /// <param name="__sampleRate">Sample rate to generate the .wav data at (44100 or 22050, default 44100)</param>
        /// <param name="__bitDepth">Bit depth to generate the .wav at (8 or 16, default 16)</param>
        /// <returns>Wave data (in .wav format) as a byte array</returns>
        public virtual byte[] GenerateWav()
        {
            var __sampleRate = 22050u;
            var __bitDepth = 8u;

            var soundLength = Convert.ToUInt32(samples);
            if (__bitDepth == 16) soundLength *= 2;

            if (__sampleRate == 22050) soundLength /= 2;

            var fileSize = 36 + soundLength;
            var blockAlign = __bitDepth / 8;
            var bytesPerSec = __sampleRate * blockAlign;

            // The file size is actually 8 bytes more than the fileSize
            var wav = new byte[fileSize + 8];

            var bytePos = 0;

            // Header

            // Chunk ID "RIFF"
            writeUintToBytes(wav, ref bytePos, 0x52494646, Endian.BIG_ENDIAN);

            // Chunck Data Size
            writeUintToBytes(wav, ref bytePos, fileSize, Endian.LITTLE_ENDIAN);

            // RIFF Type "WAVE"
            writeUintToBytes(wav, ref bytePos, 0x57415645, Endian.BIG_ENDIAN);

            // Format Chunk

            // Chunk ID "fmt "
            writeUintToBytes(wav, ref bytePos, 0x666D7420, Endian.BIG_ENDIAN);

            // Chunk Data Size
            writeUintToBytes(wav, ref bytePos, 16, Endian.LITTLE_ENDIAN);

            // Compression Code PCM
            writeShortToBytes(wav, ref bytePos, 1, Endian.LITTLE_ENDIAN);
            // Number of channels
            writeShortToBytes(wav, ref bytePos, 1, Endian.LITTLE_ENDIAN);
            // Sample rate
            writeUintToBytes(wav, ref bytePos, __sampleRate, Endian.LITTLE_ENDIAN);
            // Average bytes per second
            writeUintToBytes(wav, ref bytePos, bytesPerSec, Endian.LITTLE_ENDIAN);
            // Block align
            writeShortToBytes(wav, ref bytePos, (short) blockAlign, Endian.LITTLE_ENDIAN);
            // Significant bits per sample
            writeShortToBytes(wav, ref bytePos, (short) __bitDepth, Endian.LITTLE_ENDIAN);

            // Data Chunk

            // Chunk ID "data"
            writeUintToBytes(wav, ref bytePos, 0x64617461, Endian.BIG_ENDIAN);
            // Chunk Data Size
            writeUintToBytes(wav, ref bytePos, soundLength, Endian.LITTLE_ENDIAN);

            // Write data as bytes
            var sampleCount = 0;
            var bufferSample = 0f;
            for (var i = 0; i < data.Length; i++)
            {
                bufferSample += data[i];
                sampleCount++;

                if (sampleCount == 2)
                {
                    bufferSample /= sampleCount;
                    sampleCount = 0;

                    writeBytes(wav, ref bytePos, new[] {(byte) (Math.Round(bufferSample * 127f) + 128)},
                        Endian.LITTLE_ENDIAN);

                    bufferSample = 0f;
                }
            }

            return wav;
        }

        /// <summary>
        ///     Writes a short (Int16) to a byte array.
        ///     This is an aux function used when creating the WAV data.
        /// </summary>
        /// <param name="__bytes"></param>
        /// <param name="__position"></param>
        /// <param name="__newShort"></param>
        /// <param name="__endian"></param>
        protected void writeShortToBytes(byte[] __bytes, ref int __position, short __newShort, Endian __endian)
        {
            writeBytes(__bytes, ref __position,
                new byte[2] {(byte) ((__newShort >> 8) & 0xff), (byte) (__newShort & 0xff)}, __endian);
        }

        /// <summary>
        ///     Writes a uint (UInt32) to a byte array.
        ///     This is an aux function used when creating the WAV data.
        /// </summary>
        /// <param name="__bytes"></param>
        /// <param name="__position"></param>
        /// <param name="__newUint"></param>
        /// <param name="__endian"></param>
        protected void writeUintToBytes(byte[] __bytes, ref int __position, uint __newUint, Endian __endian)
        {
            writeBytes(__bytes, ref __position,
                new byte[4]
                {
                    (byte) ((__newUint >> 24) & 0xff), (byte) ((__newUint >> 16) & 0xff),
                    (byte) ((__newUint >> 8) & 0xff), (byte) (__newUint & 0xff)
                }, __endian);
        }

        /// <summary>
        ///     Writes any number of bytes into a byte array, at a given position.
        ///     This is an aux function used when creating the WAV data.
        /// </summary>
        /// <param name="__bytes"></param>
        /// <param name="__position"></param>
        /// <param name="__newBytes"></param>
        /// <param name="__endian"></param>
        protected void writeBytes(byte[] __bytes, ref int __position, byte[] __newBytes, Endian __endian)
        {
            // Writes __newBytes to __bytes at position __position, increasing the position depending on the length of __newBytes
            for (var i = 0; i < __newBytes.Length; i++)
            {
                __bytes[__position] = __newBytes[__endian == Endian.BIG_ENDIAN ? i : __newBytes.Length - i - 1];
                __position++;
            }
        }

        protected enum Endian
        {
            BIG_ENDIAN,
            LITTLE_ENDIAN
        }
    }
}