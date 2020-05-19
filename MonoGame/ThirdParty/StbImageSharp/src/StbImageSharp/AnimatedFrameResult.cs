namespace StbImageSharp
{
#if !STBSHARP_INTERNAL
	public
#else
	internal
#endif
	class AnimatedFrameResult : ImageResult
	{
		public int Delay
		{
			get; set;
		}
	}
}
