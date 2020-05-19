using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NVorbis
{
    public partial class BufferedReadStream
    {
    }
}
namespace TestHarness
{
    public class BufferedReadStream : NVorbis.BufferedReadStream
    {
        public BufferedReadStream(Stream baseStream)
            : base(baseStream)
        {
        }

        public BufferedReadStream(Stream baseStream, bool minimalRead)
            : base(baseStream, minimalRead)
        {
        }

        public BufferedReadStream(Stream baseStream, int initialSize, int maxSize)
            : base(baseStream, initialSize, maxSize)
        {
        }

        public BufferedReadStream(Stream baseStream, int initialSize, int maxSize, bool minimalRead)
            : base(baseStream, initialSize, maxSize, minimalRead)
        {
        }
    }
}
