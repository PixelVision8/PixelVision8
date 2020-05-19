using System;
using System.Runtime.InteropServices;

namespace StbImageSharp
{
	internal static unsafe class CRuntime
	{
		public static void* malloc(ulong size)
		{
			return malloc((long) size);
		}

		public static void* malloc(long size)
		{
			var ptr = Marshal.AllocHGlobal((int) size);

			MemoryStats.Allocated();

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
			if (a == null)
			{
				return;
			}

			var ptr = new IntPtr(a);
			Marshal.FreeHGlobal(ptr);
			MemoryStats.Freed();
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

		public static void SetArray<T>(T[] data, T value)
		{
			for (var i = 0; i < data.Length; ++i)
			{
				data[i] = value;
			}
		}
	}
}