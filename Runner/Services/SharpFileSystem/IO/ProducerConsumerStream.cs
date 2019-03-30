using System;
using System.IO;
using System.Threading;

namespace SharpFileSystem.IO
{
    public class ProducerConsumerStream : Stream
    {
        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }



        object _readLocker = new object();
        object _writeLocker = new object();
        bool _closed = false;
        bool _isWritingStalled = false;

        private CircularBuffer<byte> _buffer = new CircularBuffer<byte>(4096);

        bool IsWriteable
        {
            get { return WriteableCount > 0; }
        }

        long WriteableCount
        {
            get { return _buffer.Capacity - _buffer.Size; }
        }

        public ProducerConsumerStream()
        {
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
            {
                lock (_readLocker)
                {
                    int readCount = _buffer.Get(buffer, offset, count);
                    if (readCount == 0)
                    {
                        if (_closed)
                            return 0;
                        Monitor.Wait(_readLocker);
                        continue;
                    }

                    if (_isWritingStalled)
                    {
                        lock (_writeLocker)
                        {
                            Monitor.Pulse(_writeLocker);
                        }
                    }
                    return readCount;
                }
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock(_readLocker)
            {
                int writeCount = Math.Min((int)WriteableCount, count - offset);
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
                    writeCount = Math.Min((int)WriteableCount, count - offset);

                    Monitor.Pulse(_readLocker);
                }
            }
        }
    }
}
