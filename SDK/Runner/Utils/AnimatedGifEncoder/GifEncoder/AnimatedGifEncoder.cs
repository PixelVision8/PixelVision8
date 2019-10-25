using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;

//using UnityEngine;

namespace GifEncoder
{
    public class AnimatedGifEncoder
    {
        protected int width; // image size
        protected int height;
        protected int repeat = 0; // no repeat
        protected int delay; // frame delay (hundredths)
        public MemoryStream fs;

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



        public AnimatedGifEncoder()
        {
            fs = new MemoryStream();
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

        public void CreatePalette(DisplayChip displayChip, ColorChip colorChip)
        {

            var allColors = colorChip.colors;
            var uniqueColors = new List<Color>()
            {
                ColorUtils.HexToColor(colorChip.maskColor),
                Color.Black,
                Color.White
            };

            for (int i = 0; i < allColors.Length; i++)
            {
                if (uniqueColors.IndexOf(allColors[i]) == -1)
                {
                    uniqueColors.Add(allColors[i]);
                }
            }

            currentFramePixels = uniqueColors.ToArray();
            width = currentFramePixels.Length;
            height = 1;

            GetImagePixels(); // convert to correct format if necessary

            // Force this to analize the system color palette
            nq = new NeuQuant(pixels, pixels.Length, sample);
            colorTab = nq.Process();


            currentFramePixels = displayChip.VisiblePixels();
            width = displayChip.visibleBounds.Width;
            height = displayChip.visibleBounds.Height;
            sizeSet = true;

            GetImagePixels();
            // create the buffer for the displayed pixel data:
            visiblePixels = new byte[pixels.Length];

            //	        AnalyzePixels(); // build color table & map pixels

            colorDepth = (int)Math.Log(NeuQuant.PaletteSize + 1, 2);
            palSize = colorDepth - 1;
            //	        
            WriteLSD(); // logical screen descriptior
            WritePalette(); // global color table
            if (repeat >= 0)
            {
                // use NS app extension to indicate reps
                WriteNetscapeExt();
            }
        }

        private List<Color[]> frameData = new List<Color[]>();
        private BackgroundWorker exportWorker;
        public bool exporting = false;
        public bool exportingDone = false;
        public byte[] bytes;

        /// <summary>
        /// Adds a frame to the animated GIF. If this is the first frame, it will be used to specify the size and color palette of the GIF.
        /// </summary>
        public void AddFrame(DisplayChip displayChip)
        {
            frameData.Add(displayChip.VisiblePixels());




            //            WritePixels(); // encode and write pixel data


        }



        /// <summary>
        /// Analyzes image colors and creates color map.
        /// </summary>
        private void AnalyzePixels()
        {

            int nPix = pixels.Length / 3;
            indexedPixels = new byte[nPix];
            // map image pixels to new palette
            for (int i = 0; i < nPix; i++)
            {
                int r = i * 3 + 0;
                int g = r + 1;
                int b = g + 1;

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
                    Color pixel = currentFramePixels[y * width + x];
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
            for (int i = 0; i < frameData.Count; i++)
            {
                firstFrame = i == 0;

                currentFramePixels = frameData[i];
                GetImagePixels(); // convert to correct format if necessary
                AnalyzePixels(); // build color table & map pixels

                WriteGraphicCtrlExt(); // write graphic control extension
                WriteImageDesc(); // image descriptor

                LZWEncoder encoder = new LZWEncoder(width, height, indexedPixels, colorDepth);
                encoder.Encode(fs);


            }

        }

        private void WriteShort(int value)
        {
            fs.WriteByte(Convert.ToByte(value & 0xff));
            fs.WriteByte(Convert.ToByte((value >> 8) & 0xff));
        }

        private void WriteString(String s)
        {
            char[] chars = s.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                fs.WriteByte((byte)chars[i]);
            }
        }

        /// <summary>
        /// Flushes any pending data and closes output file.
        /// </summary>
        public void Finish()
        {
            StartExport();
            //	        WritePixels();
            //			
            //	        fs.WriteByte(0x3b); // gif trailer
            //	        fs.Flush();
            //	        fs.Close();
        }

        public void StartExport()
        {

            exportWorker = new BackgroundWorker();

            // TODO need a way to of locking this.

            exportWorker.WorkerSupportsCancellation = true;
            exportWorker.WorkerReportsProgress = true;


            //                Console.WriteLine("Start export " + exportService.totalSteps + " steps");

            exportWorker.DoWork += WorkerExportSteps;
            //            bgw.ProgressChanged += WorkerLoaderProgressChanged;
            exportWorker.RunWorkerCompleted += WorkerExporterCompleted;

            //            bgw.WorkerReportsProgress = true;
            exportWorker.RunWorkerAsync();

            exporting = true;
        }

        private void WorkerExportSteps(object sender, DoWorkEventArgs e)
        {
            //            var result = e.Result;

            for (int i = 0; i < frameData.Count; i++)
            {
                firstFrame = i == 0;

                currentFramePixels = frameData[i];
                GetImagePixels(); // convert to correct format if necessary
                AnalyzePixels(); // build color table & map pixels

                WriteGraphicCtrlExt(); // write graphic control extension
                WriteImageDesc(); // image descriptor

                LZWEncoder encoder = new LZWEncoder(width, height, indexedPixels, colorDepth);
                encoder.Encode(fs);

                Thread.Sleep(1);
                //		        exportWorker.ReportProgress((int) (percent * 100), i);
            }

            //            }
        }

        public void WorkerExporterCompleted(object sender, RunWorkerCompletedEventArgs e)
        {



            fs.WriteByte(0x3b); // gif trailer

            //            bytes = fs.ReadAllBytes();

            //            fs.Flush();
            //            
            //            
            //            
            //            
            //            fs.Close();

            exporting = false;
            exportingDone = true;
        }
    }

}