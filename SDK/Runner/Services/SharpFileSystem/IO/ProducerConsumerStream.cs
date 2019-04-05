using System;
using System.IO;
using System.Threading;

namespace SharpFileSystem.IO
{
    public class ProducerConsumerStream : Stream
    {
        private readonly CircularBuffer<byte> _buffer = new CircularBuffer<byte>(4096);
        private bool _closed;
        private bool _isWritingStalled;


        private readonly object _readLocker = new object();
        private readonly object _writeLocker = new object();

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        private bool IsWriteable => WriteableCount > 0;

        private long WriteableCount => _buffer.Capacity - _buffer.Size;

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Close()
        {
            _closed = true;
            lock (_readLocker)
            {
                Monitor.Pulse(_readLocker);
            }

            lock (_writeLocker)
            {
                Monitor.Pulse(_writeLocker);
            }

            base.Close();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            while (true)
                lock (_readLocker)
                {
                    var readCount = _buffer.Get(buffer, offset, count);
                    if (readCount == 0)
                    {
                        if (_closed)
                            return 0;
                        Monitor.Wait(_readLocker);
                        continue;
                    }

                    if (_isWritingStalled)
                        lock (_writeLocker)
                        {
                            Monitor.Pulse(_writeLocker);
                        }

                    return readCount;
                }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (_readLocker)
            {
                var writeCount = Math.Min((int) WriteableCount, count - offset);
                while (offset < count)
                {
                    if (!IsWriteable)
                    {
                        _isWritingStalled = true;
                        lock (_writeLocker)
                        {
                            Monitor.Exit(_readLocker);
                            Monitor.Wait(_writeLocker);
                            Monitor.Enter(_readLocker);
                        }

                        _isWritingStalled = false;
                        if (_closed)
                            break;
                    }

                    _buffer.Put(buffer, offset, writeCount);
                    offset += writeCount;
                    writeCount = Math.Min((int) WriteableCount, count - offset);

                    Monitor.Pulse(_readLocker);
                }
            }
        }
    }
}