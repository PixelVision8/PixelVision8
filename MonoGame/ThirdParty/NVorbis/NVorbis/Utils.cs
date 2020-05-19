/****************************************************************************
 * NVorbis                                                                  *
 * Copyright (C) 2014, Andrew Ward <afward@gmail.com>                       *
 *                                                                          *
 * See COPYING for license terms (Ms-PL).                                   *
 *                                                                          *
 ***************************************************************************/
namespace NVorbis
{
    static class Utils
    {
        static internal int ilog(int x)
        {
            int cnt = 0;
            while (x > 0)
            {
                ++cnt;
                x >>= 1;    // this is safe because we'll never get here if the sign bit is set
            }
            return cnt;
        }

        static internal uint BitReverse(uint n)
        {
            return BitReverse(n, 32);
        }

        static internal uint BitReverse(uint n, int bits)
        {
            n = ((n & 0xAAAAAAAA) >> 1) | ((n & 0x55555555) << 1);
            n = ((n & 0xCCCCCCCC) >> 2) | ((n & 0x33333333) << 2);
            n = ((n & 0xF0F0F0F0) >> 4) | ((n & 0x0F0F0F0F) << 4);
            n = ((n & 0xFF00FF00) >> 8) | ((n & 0x00FF00FF) << 8);
            return ((n >> 16) | (n << 16)) >> (32 - bits);
        }

        // make it so we can twiddle bits in a float...
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        struct FloatBits
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            public float Float;
            [System.Runtime.InteropServices.FieldOffset(0)]
            public uint Bits;
        }

        static internal float ClipValue(float value, ref bool clipped)
        {
            /************
             * There is some magic happening here... IEEE 754 single precision floats are built such that:
             *   1) The only difference between x and -x is the sign bit (31)
             *   2) If x is further from 0 than y, the bitwise value of x is greater than the bitwise value of y (ignoring the sign bit)
             * 
             * With those assumptions, we can just look for the bitwise magnitude to be too large...
             */

            FloatBits fb;
            fb.Bits = 0;
            fb.Float = value;

            // as a courtesy to those writing out 24-bit and 16-bit samples, our full scale is 0.99999994 instead of 1.0
            if ((fb.Bits & 0x7FFFFFFF) > 0x3f7fffff) // 0x3f7fffff == 0.99999994f
            {
                clipped = true;
                fb.Bits = 0x3f7fffff | (fb.Bits & 0x80000000);
            }
            return fb.Float;
        }

        static internal float ConvertFromVorbisFloat32(uint bits)
        {
            // do as much as possible with bit tricks in integer math
            var sign = ((int)bits >> 31);   // sign-extend to the full 32-bits
            var exponent = (double)((int)((bits & 0x7fe00000) >> 21) - 788);  // grab the exponent, remove the bias, store as double (for the call to System.Math.Pow(...))
            var mantissa = (float)(((bits & 0x1fffff) ^ sign) + (sign & 1));  // grab the mantissa and apply the sign bit.  store as float

            // NB: We could use bit tricks to calc the exponent, but it can't be more than 63 in either direction.
            //     This creates an issue, since the exponent field allows for a *lot* more than that.
            //     On the flip side, larger exponent values don't seem to be used by the Vorbis codebooks...
            //     Either way, we'll play it safe and let the BCL calculate it.

            // now switch to single-precision and calc the return value
            return mantissa * (float)System.Math.Pow(2.0, exponent);
        }

        // this is a no-allocation way to sum an int queue
        static internal int Sum(System.Collections.Generic.Queue<int> queue)
        {
            var value = 0;
            for (int i = 0; i < queue.Count; i++)
            {
                var temp = queue.Dequeue();
                value += temp;
                queue.Enqueue(temp);
            }
            return value;
        }
    }
}
