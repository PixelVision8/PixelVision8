using System.IO;

namespace SharpFileSystem.IO
{
    public class BranchStream: Stream
    {
        private Stream _stream;
        private Stream _writeStream;
        private Stream _readStream;

        public override bool CanRead
        {
            get { return _stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _stream.CanWrite; }
        }

        public override long Length
        {
            get { return _stream.Length; }
        }

        public override long Position
        {
            get { return _stream.Position; }
            set { _stream.Position = value; }
        }

        public BranchStream(Stream stream, Stream writeStream, Stream readStream)
        {
            _stream = stream;
            _writeStream = writeStream;
            _readStream = readStream;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readBytes = _stream.Read(buffer, offset, count);
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
