using System;
using System.Runtime.InteropServices;

namespace StbImageSharp
{
#if !STBSHARP_INTERNAL
	public
#else
	internal
#endif
	class ImageResult
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public ColorComponents SourceComp { get; set; }
		public ColorComponents Comp { get; set; }
		public byte[] Data { get; set; }

		internal unsafe static ImageResult FromResult(byte* result, int width, int height, ColorComponents comp, ColorComponents req_comp)
		{
			if (result == null)
			{
				throw new InvalidOperationException(StbImage.LastError);
			}

			var image = new ImageResult
			{
				Width = width,
				Height = height,
				SourceComp = comp,
				Comp = req_comp == ColorComponents.Default ? comp : req_comp
			};

			// Convert to array
			image.Data = new byte[width * height * (int)image.Comp];
			Marshal.Copy(new IntPtr(result), image.Data, 0, image.Data.Length);

			return image;
		}

		public unsafe static ImageResult FromMemory(byte[] bytes, ColorComponents requiredComponents = ColorComponents.Default)
		{
			byte* result = null;

			try
			{
				int x, y, comp;
				fixed (byte* b = bytes)
				{
					result = StbImage.stbi_load_from_memory(b, bytes.Length, &x, &y, &comp, (int)requiredComponents);
				}

				return FromResult(result, x, y, (ColorComponents)comp, requiredComponents);
			}
			finally
			{
				if (result != null)
				{
					CRuntime.free(result);
				}
			}
		}
	}
}