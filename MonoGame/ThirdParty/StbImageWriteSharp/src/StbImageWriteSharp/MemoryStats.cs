using System.Threading;

namespace StbImageWriteSharp
{
#if !STBSHARP_INTERNAL
	public
#else
	internal
#endif
	static class MemoryStats
	{
		private static int _allocations;

		public static int Allocations
		{
			get
			{
				return _allocations;
			}
		}

		internal static void Allocated()
		{
			Interlocked.Increment(ref _allocations);
		}

		internal static void Freed()
		{
			Interlocked.Decrement(ref _allocations);
		}
	}
}
