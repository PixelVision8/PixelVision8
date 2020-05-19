using System;
using System.IO;
using System.Runtime.InteropServices;

namespace StbImageWriteSharp
{
#if !STBSHARP_INTERNAL
	public
#else
    internal
#endif
	unsafe class ImageWriter
	{
		private Stream _stream;
		private byte[] _buffer = new byte[1024];

		private int WriteCallback(void* context, void* data, int size)
		{
			if (data == null || size <= 0)
			{
				return 0;
			}

			if (_buffer.Length < size)
			{
				_buffer = new byte[size * 2];
			}

			var bptr = (byte*)data;

			Marshal.Copy(new IntPtr(bptr), _buffer, 0, size);

			_stream.Write(_buffer, 0, size);

			return size;
		}

		private void CheckParams(byte[] data, int width, int height, ColorComponents components)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}

			if (width <= 0)
			{
				throw new ArgumentOutOfRangeException("width");
			}

			if (height <= 0)
			{
				throw new ArgumentOutOfRangeException("height");
			}

			int requiredDataSize = width * height * (int)components;
			if (data.Length < requiredDataSize)
			{
				throw new ArgumentException(
					string.Format("Not enough data. 'data' variable should contain at least {0} bytes.", requiredDataSize));
			}
		}

		public void WriteBmp(void* data, int width, int height, ColorComponents components, Stream dest)
		{
			try
			{
				_stream = dest;
				StbImageWrite.stbi_write_bmp_to_func(WriteCallback, null, width, height, (int)components, data);
			}
			finally
			{
				_stream = null;
			}
		}

		public void WriteBmp(byte[] data, int width, int height, ColorComponents components, Stream dest)
		{
			CheckParams(data, width, height, components);

			fixed (byte* b = &data[0])
			{
				WriteBmp(b, width, height, components, dest);
			}
		}

		public void WriteTga(void* data, int width, int height, ColorComponents components, Stream dest)
		{
			try
			{
				_stream = dest;
				StbImageWrite.stbi_write_tga_to_func(WriteCallback, null, width, height, (int)components, data);
			}
			finally
			{
				_stream = null;
			}
		}

		public void WriteTga(byte[] data, int width, int height, ColorComponents components, Stream dest)
		{
			CheckParams(data, width, height, components);

			fixed (byte* b = &data[0])
			{
				WriteTga(b, width, height, components, dest);
			}
		}

		public void WriteHdr(byte[] data, int width, int height, ColorComponents components, Stream dest)
		{
			CheckParams(data, width, height, components);

			try
			{
				_stream = dest;
				var f = new float[data.Length];
				for (var i = 0; i < data.Length; ++i)
				{
					f[i] = data[i] / 255.0f;
				}

				fixed (float* fptr = f)
				{
					StbImageWrite.stbi_write_hdr_to_func(WriteCallback, null, width, height, (int)components, fptr);
				}
			}
			finally
			{
				_stream = null;
			}
		}

		public void WritePng(void* data, int width, int height, ColorComponents components, Stream dest)
		{
			try
			{
				_stream = dest;

				StbImageWrite.stbi_write_png_to_func(WriteCallback, null, width, height, (int)components, data,
				   width * (int)components);
			}
			finally
			{
				_stream = null;
			}
		}

		public void WritePng(byte[] data, int width, int height, ColorComponents components, Stream dest)
		{
			CheckParams(data, width, height, components);

			fixed (byte* b = &data[0])
			{
				WritePng(data, width, height, components, dest);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="components"></param>
		/// <param name="dest"></param>
		/// <param name="quality">Should be from 1 to 100</param>
		public void WriteJpg(void* data, int width, int height, ColorComponents components, Stream dest, int quality)
		{
			try
			{
				_stream = dest;

				StbImageWrite.stbi_write_jpg_to_func(WriteCallback, null, width, height, (int)components, data, quality);
			}
			finally
			{
				_stream = null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="components"></param>
		/// <param name="dest"></param>
		/// <param name="quality">Should be from 1 to 100</param>
		public void WriteJpg(byte[] data, int width, int height, ColorComponents components, Stream dest, int quality)
		{
			CheckParams(data, width, height, components);

			fixed (byte* b = &data[0])
			{
				WriteJpg(b, width, height, components, dest, quality);
			}
		}
	}
}
