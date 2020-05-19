using System;
using System.Runtime.InteropServices;

namespace StbImageWriteSharp
{
	internal static unsafe class CRuntime
	{
		public const long DBL_EXP_MASK = 0x7ff0000000000000L;
		public const int DBL_MANT_BITS = 52;
		public const long DBL_SGN_MASK = -1 - 0x7fffffffffffffffL;
		public const long DBL_MANT_MASK = 0x000fffffffffffffL;
		public const long DBL_EXP_CLR_MASK = DBL_SGN_MASK | DBL_MANT_MASK;

		public static void* malloc(ulong size)
		{
			return malloc((long) size);
		}

		public static void* malloc(long size)
		{
			var ptr = Marshal.AllocHGlobal((int) size);

			return ptr.ToPointer();
		}

		public static void memcpy(void* a, void* b, long size)
		{
			var ap = (byte*) a;
			var bp = (byte*) b;
			for (long i = 0; i < size; ++i)
			{
				*ap++ = *bp++;
			}
		}

		public static void memcpy(void* a, void* b, ulong size)
		{
			memcpy(a, b, (long) size);
		}

		public static void memmove(void* a, void* b, long size)
		{
			void* temp = null;

			try
			{
				temp = malloc(size);
				memcpy(temp, b, size);
				memcpy(a, temp, size);
			}

			finally
			{
				if (temp != null)
				{
					free(temp);
				}
			}
		}

		public static void memmove(void* a, void* b, ulong size)
		{
			memmove(a, b, (long) size);
		}

		public static int memcmp(void* a, void* b, long size)
		{
			var result = 0;
			var ap = (byte*) a;
			var bp = (byte*) b;
			for (long i = 0; i < size; ++i)
			{
				if (*ap != *bp)
				{
					result += 1;
				}

				ap++;
				bp++;
			}

			return result;
		}

		public static int memcmp(void* a, void* b, ulong size)
		{
			return memcmp(a, b, (long) size);
		}

		public static int memcmp(byte* a, byte[] b, ulong size)
		{
			fixed (void* bptr = b)
			{
				return memcmp(a, bptr, (long) size);
			}
		}

		public static void free(void* a)
		{
			var ptr = new IntPtr(a);
			Marshal.FreeHGlobal(ptr);
		}

		public static void memset(void* ptr, int value, long size)
		{
			byte* bptr = (byte*) ptr;
			var bval = (byte) value;
			for (long i = 0; i < size; ++i)
			{
				*bptr++ = bval;
			}
		}

		public static void memset(void* ptr, int value, ulong size)
		{
			memset(ptr, value, (long) size);
		}

		public static uint _lrotl(uint x, int y)
		{
			return (x << y) | (x >> (32 - y));
		}

		public static void* realloc(void* a, long newSize)
		{
			if (a == null)
			{
				return malloc(newSize);
			}

			var ptr = new IntPtr(a);
			var result = Marshal.ReAllocHGlobal(ptr, new IntPtr(newSize));

			return result.ToPointer();
		}

		public static void* realloc(void* a, ulong newSize)
		{
			return realloc(a, (long) newSize);
		}

		public static int abs(int v)
		{
			return Math.Abs(v);
		}

		/// <summary>
		/// This code had been borrowed from here: https://github.com/MachineCognitis/C.math.NET
		/// </summary>
		/// <param name="number"></param>
		/// <param name="exponent"></param>
		/// <returns></returns>
		public static double frexp(double number, int* exponent)
		{
			var bits = BitConverter.DoubleToInt64Bits(number);
			var exp = (int) ((bits & DBL_EXP_MASK) >> DBL_MANT_BITS);
			*exponent = 0;

			if (exp == 0x7ff || number == 0D)
				number += number;
			else
			{
				// Not zero and finite.
				*exponent = exp - 1022;
				if (exp == 0)
				{
					// Subnormal, scale number so that it is in [1, 2).
					number *= BitConverter.Int64BitsToDouble(0x4350000000000000L); // 2^54
					bits = BitConverter.DoubleToInt64Bits(number);
					exp = (int) ((bits & DBL_EXP_MASK) >> DBL_MANT_BITS);
					*exponent = exp - 1022 - 54;
				}

				// Set exponent to -1 so that number is in [0.5, 1).
				number = BitConverter.Int64BitsToDouble((bits & DBL_EXP_CLR_MASK) | 0x3fe0000000000000L);
			}

			return number;
		}
	}
}