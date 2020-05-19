using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace UnitTests
{
    class ForwardOnlyStream : Stream
    {
        Stream _baseStream;

        public ForwardOnlyStream(Stream baseStream)
        {
            _baseStream = baseStream;
        }

        public override bool CanRead
        {
            get { return _baseStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return _baseStream.CanWrite; }
        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override long Length
        {
            get { return _baseStream.Length; }
        }

        public override long Position
        {
            get
            {
                return _baseStream.Position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _baseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _baseStream.Write(buffer, offset, count);
        }
    }
}
