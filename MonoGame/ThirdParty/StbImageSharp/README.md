# StbImageSharp
[![NuGet](https://img.shields.io/nuget/v/StbImageSharp.svg)](https://www.nuget.org/packages/StbImageSharp/) [![Build status](https://ci.appveyor.com/api/projects/status/c9eh0e4c70ki26fy?svg=true)](https://ci.appveyor.com/project/RomanShapiro/stbimagesharp)

StbImageSharp is C# port of the stb_image.h, which in its turn is C library to load images in JPG, PNG, BMP, TGA, PSD and GIF formats.

It is important to note, that this project is **port**(not **wrapper**). Original C code had been ported to C#. Therefore StbImageSharp doesnt require any native binaries.

The porting hasn't been done by hand, but using [Sichem](https://github.com/rds1983/Sichem), which is the C to C# code converter utility.

# Usage
StbImageSharp exposes API similar to stb_image.h. However that API is complicated and deals with raw unsafe pointers.

Thus several utility classes had been made to wrap that functionality.

'ImageStreamLoader' class wraps the call to 'stbi_load_from_callbacks' method.

It could be used following way:
```c#
ImageStreamLoader loader = new ImageStreamLoader();
using (Stream stream = File.Open(path, FileMode.Open)) 
{
	ImageResult image = loader.Read(stream, ColorComponents.RedGreenBlueAlpha);
}
```

Or 'ImageResult.FromMemory' method wraps 'stbi_load_from_memory':
```c# 
byte[] buffer = File.ReadAllBytes(path);
ImageResult image = ImageResult.FromMemory(buffer, ColorComponents.RedGreenBlueAlpha);
```

Both code samples will try to load an image (JPG/PNG/BMP/TGA/PSD/GIF) located at 'path'. It'll throw Exception on failure.

If you are writing MonoGame application and would like to convert that data to the Texture2D. It could be done following way:
```c#
Texture2D texture = new Texture2D(GraphicsDevice, image.Width, image.Height, false, SurfaceFormat.Color);
texture.SetData(image.Data);
```

Or if you are writing WinForms app and would like StbSharp resulting bytes to be converted to the Bitmap. The sample code is:
```c#
byte[] data = image.Data;
// Convert rgba to bgra
for (int i = 0; i < x*y; ++i)
{
	byte r = data[i*4];
	byte g = data[i*4 + 1];
	byte b = data[i*4 + 2];
	byte a = data[i*4 + 3];


	data[i*4] = b;
	data[i*4 + 1] = g;
	data[i*4 + 2] = r;
	data[i*4 + 3] = a;
}

// Create Bitmap
Bitmap bmp = new Bitmap(_loadedImage.Width, _loadedImage.Height, PixelFormat.Format32bppArgb);
BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, _loadedImage.Width, _loadedImage.Height), ImageLockMode.WriteOnly,
	bmp.PixelFormat);

Marshal.Copy(data, 0, bmpData.Scan0, bmpData.Stride*bmp.Height);
bmp.UnlockBits(bmpData);
```

# License
Public Domain

# Credits
* [stb](https://github.com/nothings/stb)