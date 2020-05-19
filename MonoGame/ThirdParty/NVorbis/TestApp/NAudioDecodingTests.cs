using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestApp
{
    static class NAudioDecodingTests
    {
        static void TestNAudio()
        {
            using (var vorbis = new NVorbis.NAudioSupport.VorbisWaveReader(@"..\..\..\TestFiles\2test.ogg"))
            using (var converter = new NAudio.Wave.Wave32To16Stream(vorbis))
            {
                NAudio.Wave.WaveFileWriter.CreateWaveFile(@"..\..\..\TestFiles\2test_naudio.wav", converter);
            }
        }

        static void TestRaw()
        {
            using (var vorbis = new NVorbis.VorbisReader(@"..\..\..\TestFiles\2test.ogg"))
            using (var outFile = System.IO.File.Create(@"..\..\..\TestFiles\2test.wav"))
            using (var writer = new System.IO.BinaryWriter(outFile))
            {
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(0);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(18);
                writer.Write((short)1); // PCM format
                writer.Write((short)vorbis.Channels);
                writer.Write(vorbis.SampleRate);
                writer.Write(vorbis.SampleRate * vorbis.Channels * 2);  // avg bytes per second
                writer.Write((short)(2 * vorbis.Channels)); // block align
                writer.Write((short)16); // bits per sample
                writer.Write((short)0); // extra size

                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Flush();
                var dataPos = outFile.Position;
                writer.Write(0);

                var buf = new float[vorbis.SampleRate / 10 * vorbis.Channels];
                int count;
                while ((count = vorbis.ReadSamples(buf, 0, buf.Length)) > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var temp = (int)(32767f * buf[i]);
                        if (temp > 32767)
                        {
                            temp = 32767;
                        }
                        else if (temp < -32768)
                        {
                            temp = -32768;
                        }
                        writer.Write((short)temp);
                    }
                }
                writer.Flush();

                writer.Seek(4, System.IO.SeekOrigin.Begin);
                writer.Write((int)(outFile.Length - 8L));

                writer.Seek((int)dataPos, System.IO.SeekOrigin.Begin);
                writer.Write((int)(outFile.Length - dataPos - 4L));

                writer.Flush();
            }
        }
    }
}
