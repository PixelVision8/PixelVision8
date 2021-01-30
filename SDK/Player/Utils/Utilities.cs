using System;

namespace PixelVision8.Player
{
    public static partial class Utilities
    {
        /// <summary>
        ///     Returns Ceil value as an int.
        /// </summary>
        /// <param name="value"></param>
        public static int CeilToInt(float value)
        {
            return (int) Math.Ceiling(value);
        }

        /// <summary>
        ///     Returns Floor value as an int.
        /// </summary>
        /// <param name="value"></param>
        public static int FloorToInt(float value)
        {
            return (int) Math.Floor(value);
        }

        /// <summary>
        ///     Repeats a value based on the max. When the value is greater than the max, it starts
        ///     over at 0 plus the remaining value.
        /// </summary>
        /// <param name="val">
        ///     The value to repeat.
        /// </param>
        /// <param name="max">
        ///     The maximum the value can be.
        /// </param>
        /// <returns>
        ///     Returns an int that is never less than 0 or greater than the max.
        /// </returns>
        public static int Repeat(int val, int max)
        {
            return (int) (val - Math.Floor(val / (float) max) * max);
        }

        public static int Clamp(int value, int min, int max)
        {
            value = value > max ? max : value;
            value = value < min ? min : value;
            return value;
        }

        // from https://stackoverflow.com/questions/16365870/distance-between-two-points-without-using-the-square-root
        public static int Sqrt(int x)
        {
            int s, t;

            s = 1;
            t = x;
            while (s < t)
            {
                s <<= 1;
                t >>= 1;
            } //decide the value of the first tentative

            do
            {
                t = s;
                s = (x / s + s) >> 1; //x1=(N / x0 + x0)/2 : recurrence formula
            } while (s < t);

            return t;
        }
    }
}