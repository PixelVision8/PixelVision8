

//using UnityEngine;

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PixelVision8.Engine.Chips;

namespace GifEncoder
{
    // TODO this uses the old Unity code, need to update with raw data directly from the display
	public class AnimatedGifEncoder
	{
		protected int width; // image size
		protected int height;
		protected int repeat = 0; // no repeat
		protected int delay; // frame delay (hundredths)
		protected FileStream fs;

		protected Color[] currentFramePixels; // current frame pixels
		protected byte[] pixels; // BGR byte array from frame
        protected byte[] visiblePixels;
		protected byte[] indexedPixels; // converted frame indexed to palette
		protected int colorDepth; // number of bit planes
		protected byte[] colorTab; // RGB palette
		protected bool[] usedEntry = new bool[256]; // active palette entries
		protected int palSize; // color table size (bits-1)
		protected bool firstFrame = true;
		protected bool sizeSet; // if false, get size from first frame
		protected int sample = 10; // default sample interval for quantizer
        protected NeuQuant nq;

        public AnimatedGifEncoder(FileStream fileStream)
        {
	        fs = fileStream;//new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            WriteString("GIF89a"); // header
        }

        /// <summary>
        /// Sets the delay time between each frame, or changes it for subsequent frames (applies to last frame added).
        /// </summary>
        /// <param name="ms">delay time in milliseconds</param>
		public void SetDelay(int ms) 
		{
			delay = (int)Math.Round(ms / 10.0f);
		}
        	
		public void BuildPalette(DisplayChip frame)
		{
			if (!sizeSet)
			{
				// use first frame's size
				width = frame.visibleBounds.Width;
				height = frame.visibleBounds.Height;
				sizeSet = true;
			}

			currentFramePixels = frame.VisiblePixels();
			GetImagePixels(); // convert to correct format if necessary
//            this.AnalyzePixels(); // build color table & map pixels
			
			if (firstFrame)
			{
				// HEY YOU - THIS IS IMPORTANT!
				//
				// Unlike the code this project is built on, we generate a palette only once, using the data from the
				// first frame. This speeds up encoding (building the palette is slow) and makes it so that subsequent
				// frames use the same colors and look more consistent from frame to frame, but has an undesirable
				// side-effect of getting confused when radically different colors show up in later frames. This wasn't
				// a problem with Infinifactory, but if your game uses a deliberately reduced color palette you might
				// want to switch back to using per-frame palettes. 
				//
				// This is, of course, left as an exercise for the reader.

				// initialize quantizer and create reduced palette
				nq = new NeuQuant(pixels, pixels.Length, sample);
				colorTab = nq.Process();
                
				// create the buffer for the displayed pixel data:
				visiblePixels = new byte[pixels.Length];
			}
		}
		
        /// <summary>
        /// Adds a frame to the animated GIF. If this is the first frame, it will be used to specify the size and color palette of the GIF.
        /// </summary>
        public void AddFrame(DisplayChip frame) 
		{
//            if (!this.sizeSet)
//            {
//                // use first frame's size
//                this.width = frame.width;
//                this.height = frame.height;
//                this.sizeSet = true;
//            }
//            this.currentFramePixels = frame.GetPixels32();
			currentFramePixels = frame.VisiblePixels();
            GetImagePixels(); // convert to correct format if necessary
            AnalyzePixels(); // build color table & map pixels
            if (firstFrame)
            {
                WriteLSD(); // logical screen descriptior
                WritePalette(); // global color table
                if (repeat >= 0)
                {
                    // use NS app extension to indicate reps
                    WriteNetscapeExt();
                }
            }
            WriteGraphicCtrlExt(); // write graphic control extension
            WriteImageDesc(); // image descriptor
            WritePixels(); // encode and write pixel data
            firstFrame = false;
		}
	
		/// <summary>
        /// Flushes any pending data and closes output file.
		/// </summary>
		public void Finish() 
		{
            fs.WriteByte(0x3b); // gif trailer
            fs.Flush();
            fs.Dispose();
		}

        /// <summary>
        /// Cancels the encoding process and deletes the temporary file.
        /// </summary>
        public void Cancel()
        {
            string filePath = fs.Name;
            Finish();
            File.Delete(filePath);
        }
	
		/// <summary>
        /// Analyzes image colors and creates color map.
		/// </summary>
		private void AnalyzePixels() 
		{
//            if (this.firstFrame)
//            {
//                // HEY YOU - THIS IS IMPORTANT!
//                //
//                // Unlike the code this project is built on, we generate a palette only once, using the data from the
//                // first frame. This speeds up encoding (building the palette is slow) and makes it so that subsequent
//                // frames use the same colors and look more consistent from frame to frame, but has an undesirable
//                // side-effect of getting confused when radically different colors show up in later frames. This wasn't
//                // a problem with Infinifactory, but if your game uses a deliberately reduced color palette you might
//                // want to switch back to using per-frame palettes. 
//                //
//                // This is, of course, left as an exercise for the reader.
//
//                // initialize quantizer and create reduced palette
//                this.nq = new NeuQuant(this.pixels, this.pixels.Length, this.sample);
//                this.colorTab = nq.Process();
//                
//                // create the buffer for the displayed pixel data:
//                this.visiblePixels = new byte[this.pixels.Length];
//            }

			int nPix = pixels.Length / 3;
			indexedPixels = new byte[nPix];
			// map image pixels to new palette
			for (int i = 0; i < nPix; i++) 
			{
                int r = i * 3 + 0;
                int g = r + 1;
                int b = g + 1;
                
                // HEY YOU - THIS IS IMPORTANT!
                //
                // As you may know, animated GIFs aren't exactly the most efficient way to encode video; notably,
                // they lack the interframe compression found in real video formats. However, we can do something
                // similar by taking advantage of the GIF format's ability to "build on" a previous frame by
                // specifying pixels as transparent and allowing the previous frame's image to show through in those
                // locations. If a pixel doesn't change much from a previous pixel it will be replaced with the
                // transparent color, and if enough of those pixels are used in a frame it will compress much better
                // than if you had all the original color data for the frame. 
                // 
                // In a game like Infinifactory, where the recording camera is locked and only a small portion of
                // the scene is animated, this reduced the size of GIFs to about 1/3 of their original size.

                const int ChangeDelta = 3;
                bool pixelRequired = firstFrame ||
                    Math.Abs(pixels[r] - visiblePixels[r]) > ChangeDelta ||
                    Math.Abs(pixels[g] - visiblePixels[g]) > ChangeDelta ||
                    Math.Abs(pixels[b] - visiblePixels[b]) > ChangeDelta;

                int index;
                if (pixelRequired)
                {
                    visiblePixels[r] = pixels[r];
                    visiblePixels[g] = pixels[g];
                    visiblePixels[b] = pixels[b];
                    index = nq.Map(pixels[r], pixels[g], pixels[b]);
                }
                else
                {
                    index = NeuQuant.PaletteSize;
                }

                usedEntry[index] = true;
                indexedPixels[i] = (byte)index;
			}
            colorDepth = (int)Math.Log(NeuQuant.PaletteSize + 1, 2);
            palSize = colorDepth - 1;
		}
	
		/// <summary>
        /// Extracts image pixels into byte array "pixels", flipping vertically
		/// </summary>
		private void GetImagePixels() 
		{
            pixels = new byte[3 * currentFramePixels.Length];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = currentFramePixels[(y) * width + x];
                    pixels[y * width * 3 + x * 3 + 0] = pixel.R;
                    pixels[y * width * 3 + x * 3 + 1] = pixel.G;
                    pixels[y * width * 3 + x * 3 + 2] = pixel.B;
                }
            }
		}
	
		/// <summary>
        /// Writes Graphic Control Extension.
		/// </summary>
        private void WriteGraphicCtrlExt() 
		{
			fs.WriteByte(0x21); // extension introducer
			fs.WriteByte(0xf9); // GCE label
			fs.WriteByte(4); // data block size

            if (firstFrame)
            {
                fs.WriteByte(0x00); // packed fields: disposal = 0, transparent = 0
            }
            else
            {
                fs.WriteByte(0x05); // packed fields: disposal = 1, transparent = 1
            }
			
            WriteShort(delay); // delay x 1/100 sec
            fs.WriteByte((byte)NeuQuant.PaletteSize); // transparent color index
			fs.WriteByte(0); // block terminator
		}
	
        private void WriteImageDesc()
		{
			fs.WriteByte(0x2c); // image separator
			WriteShort(0); // image position x,y = 0,0
			WriteShort(0);
			WriteShort(width); // image size
			WriteShort(height);
            fs.WriteByte(0); // packed fields
		}
	
        private void WriteLSD()  
		{
			// logical screen size
            WriteShort(width);
            WriteShort(height);

			// packed fields
            fs.WriteByte(Convert.ToByte(0x80 | // 1   : global color table flag = 1 (gct used)
				0x70 | // 2-4 : color resolution = 7
				0x00 | // 5   : gct sort flag = 0
                palSize)); // 6-8 : gct size

            fs.WriteByte(0); // background color index
            fs.WriteByte(0); // pixel aspect ratio - assume 1:1
		}

        private void WriteNetscapeExt()
		{
            fs.WriteByte(0x21); // extension introducer
            fs.WriteByte(0xff); // app extension label
            fs.WriteByte(11); // block size
            WriteString("NETSCAPE" + "2.0"); // app id + auth code
            fs.WriteByte(3); // sub-block size
            fs.WriteByte(1); // loop sub-block id
            WriteShort(repeat); // loop count (extra iterations, 0=repeat forever)
            fs.WriteByte(0); // block terminator
		}

        private void WritePalette()
		{
            fs.Write(colorTab, 0, colorTab.Length);
            int n = (3 * (NeuQuant.PaletteSize + 1)) - colorTab.Length;
			for (int i = 0; i < n; i++) 
			{
                fs.WriteByte(0);
			}
		}

        private void WritePixels()
		{
            LZWEncoder encoder = new LZWEncoder(width, height, indexedPixels, colorDepth);
            encoder.Encode(fs);
		}
	
		private void WriteShort(int value)
		{
            fs.WriteByte(Convert.ToByte(value & 0xff));
            fs.WriteByte(Convert.ToByte((value >> 8) & 0xff));
		}
	
        private void WriteString(string s)
		{
			char[] chars = s.ToCharArray();
			for (int i = 0; i < chars.Length; i++) 
			{
                fs.WriteByte((byte)chars[i]);
			}
		}
	}
}