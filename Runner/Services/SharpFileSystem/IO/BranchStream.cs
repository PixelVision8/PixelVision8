using System.IO;

namespace SharpFileSystem.IO
{
    public class BranchStream : Stream
    {
        private readonly Stream _readStream;
        private readonly Stream _stream;
        private readonly Stream _writeStream;

        public BranchStream(Stream stream, Stream writeStream, Stream readStream)
        {
            _stream = stream;
            _writeStream = writeStream;
            _readStream = readStream;
        }

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length => _stream.Length;

        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readBytes = _stream.Read(buffer, offset, count);
            _readStream.Write(buffer, offset, readBytes);
            _readStream.Flush();
            return readBytes;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
            _writeStream.Write(buffer, offset, count);
            _writeStream.Flush();
        }

        public override void Flush()
        {
            _stream.Flush();
            _writeStream.Flush();
        }

        public override void Close()
        {
            _stream.Close();
            _readStream.Close();
            _writeStream.Close();
        }
    }
}