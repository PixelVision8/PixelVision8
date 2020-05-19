/****************************************************************************
 * NVorbis                                                                  *
 * Copyright (C) 2014, Andrew Ward <afward@gmail.com>                       *
 *                                                                          *
 * See COPYING for license terms (Ms-PL).                                   *
 *                                                                          *
 ***************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;

namespace NVorbis
{
    partial class StreamReadBuffer : IDisposable
    {
        class StreamWrapper
        {
            internal Stream Source;
            internal object LockObject = new object();
            internal long EofOffset = long.MaxValue;
            internal int RefCount = 1;
        }

        static Dictionary<Stream, StreamWrapper> _lockObjects = new Dictionary<Stream, StreamWrapper>();

        internal StreamReadBuffer(Stream source, int initialSize, int maxSize, bool minimalRead)
        {
            StreamWrapper wrapper;
            lock(_lockObjects)
            {
                if (!_lockObjects.TryGetValue(source, out wrapper))
                {
                    _lockObjects.Add(source, new StreamWrapper { Source = source });
                    wrapper = _lockObjects[source];
    
                    if (source.CanSeek)
                    {
                        // assume that this is a quick operation
                        wrapper.EofOffset = source.Length;
                    }
                }
                else
                {
                    wrapper.RefCount++;
                }
            }

            // make sure our initial size is a power of 2 (this makes resizing simpler to understand)
            initialSize = 2 << (int)Math.Log(initialSize - 1, 2);

            // make sure our max size is a power of 2 (in this case, just so we report a "real" number)
            maxSize = 1 << (int)Math.Log(maxSize, 2);

            _wrapper = wrapper;
            _data = new byte[initialSize];
            _maxSize = maxSize;
            _minimalRead = minimalRead;

            _savedBuffers = new List<SavedBuffer>();
        }

        public void Dispose()
        {
            lock( _lockObjects )
            {
                if (--_wrapper.RefCount == 0)
                {
                    _lockObjects.Remove(_wrapper.Source);
                }
            }
        }

        StreamWrapper _wrapper;
        int _maxSize;

        byte[] _data;
        long _baseOffset;
        int _end;
        int _discardCount;

        bool _minimalRead;

        // we're locked already when we enter, so we can do whatever we need to do without worrying about it...
        class SavedBuffer
        {
            public byte[] Buffer;
            public long BaseOffset;
            public int End;
            public int DiscardCount;
            public long VersionSaved;
        }
        long _versionCounter;
        List<SavedBuffer> _savedBuffers;

        /// <summary>
        /// Gets or Sets whether to limit reads to the smallest size possible.
        /// </summary>
        public bool MinimalRead
        {
            get { return _minimalRead; }
            set { _minimalRead = value; }
        }

        /// <summary>
        /// Gets or Sets the maximum size of the buffer.  This is not a hard limit.
        /// </summary>
        public int MaxSize
        {
            get { return _maxSize; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("Must be greater than zero.");

                var newMaxSize = 1 << (int)Math.Ceiling(Math.Log(value, 2));

                if (newMaxSize < _end)
                {
                    if (newMaxSize < _end - _discardCount)
                    {
                        // we can't discard enough bytes to satisfy the buffer request...
                        throw new ArgumentOutOfRangeException("Must be greater than or equal to the number of bytes currently buffered.");
                    }

                    CommitDiscard();
                    var newBuf = new byte[newMaxSize];
                    Buffer.BlockCopy(_data, 0, newBuf, 0, _end);
                    _data = newBuf;
                }
                _maxSize = newMaxSize;
            }
        }

        /// <summary>
        /// Gets the offset of the start of the buffered data.  Reads to offsets before this are likely to require a seek.
        /// </summary>
        public long BaseOffset
        {
            get { return _baseOffset + _discardCount; }
        }

        /// <summary>
        /// Gets the number of bytes currently buffered.
        /// </summary>
        public int BytesFilled
        {
            get { return _end - _discardCount; }
        }

        /// <summary>
        /// Gets the number of bytes the buffer can hold.
        /// </summary>
        public int Length
        {
            get { return _data.Length; }
        }

        internal long BufferEndOffset
        {
            get
            {
                if (_end - _discardCount > 0)
                {
                    // this is the base offset + discard bytes + buffer max length (though technically we could go a little further...)
                    return _baseOffset + _discardCount + _maxSize;
                }
                // if there aren't any bytes in the buffer, we can seek wherever we want
                return _wrapper.Source.Length;
            }
        }

        /// <summary>
        /// Reads the number of bytes specified into the buffer given, starting with the offset indicated.
        /// </summary>
        /// <param name="offset">The offset into the stream to start reading.</param>
        /// <param name="buffer">The buffer to read to.</param>
        /// <param name="index">The index into the buffer to start writing to.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The number of bytes read.</returns>
        public int Read(long offset, byte[] buffer, int index, int count)
        {
            if (offset < 0L) throw new ArgumentOutOfRangeException("offset");
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (index < 0 || index + count > buffer.Length) throw new ArgumentOutOfRangeException("index");
            if (count < 0) throw new ArgumentOutOfRangeException("count");
            if (offset >= _wrapper.EofOffset) return 0;

            var startIdx = EnsureAvailable(offset, ref count, false);

            Buffer.BlockCopy(_data, startIdx, buffer, index, count);

            return count;
        }

        internal int ReadByte(long offset)
        {
            if (offset < 0L) throw new ArgumentOutOfRangeException("offset");
            if (offset >= _wrapper.EofOffset) return -1;

            int count = 1;
            var startIdx = EnsureAvailable(offset, ref count, false);
            if (count == 1)
            {
                return _data[startIdx];
            }
            return -1;
        }


        int EnsureAvailable(long offset, ref int count, bool isRecursion)
        {
            // simple... if we're inside the buffer, just return the offset (FAST PATH)
            if (offset >= _baseOffset && offset + count < _baseOffset + _end)
            {
                return (int)(offset - _baseOffset);
            }

            // not so simple... we're outside the buffer somehow...

            // let's make sure the request makes sense
            if (count > _maxSize)
            {
                throw new InvalidOperationException("Not enough room in the buffer!  Increase the maximum size and try again.");
            }

            // make sure we always bump the version counter when a change is made to the data in the "live" buffer
            ++_versionCounter;

            // can we satisfy the request with a saved buffer?
            if (!isRecursion)
            {
                for (int i = 0; i < _savedBuffers.Count; i++)
                {
                    var tempS = _savedBuffers[i].BaseOffset - offset;
                    if ((tempS < 0 && _savedBuffers[i].End + tempS > 0) || (tempS > 0 && count - tempS > 0))
                    {
                        SwapBuffers(_savedBuffers[i]);
                        return EnsureAvailable(offset, ref count, true);
                    }
                }
            }

            // look for buffers we need to drop due to age...
            while (_savedBuffers.Count > 0 && _savedBuffers[0].VersionSaved + 25 < _versionCounter)
            {
                _savedBuffers[0].Buffer = null;
                _savedBuffers.RemoveAt(0);
            }

            // if we have to seek back, we're doomed...
            if (offset < _baseOffset && !_wrapper.Source.CanSeek)
            {
                throw new InvalidOperationException("Cannot seek before buffer on forward-only streams!");
            }

            // figure up the new buffer parameters...
            int readStart;
            int readEnd;
            CalcBuffer(offset, count, out readStart, out readEnd);

            // fill the buffer...
            // if we did a reverse seek, there will be data still in end of the buffer...  Make sure to fill everything between
            count = FillBuffer(offset, count, readStart, readEnd);

            return (int)(offset - _baseOffset);
        }

        void SaveBuffer()
        {
            _savedBuffers.Add(
                new SavedBuffer
                {
                    Buffer = _data,
                    BaseOffset = _baseOffset,
                    End = _end,
                    DiscardCount = _discardCount,
                    VersionSaved = _versionCounter
                }
            );

            _data = null;
            _end = 0;
            _discardCount = 0;
        }

        void CreateNewBuffer(long offset, int count)
        {
            SaveBuffer();

            _data = new byte[Math.Min(2 << (int)Math.Log(count - 1, 2), _maxSize)];
            _baseOffset = offset;
        }

        void SwapBuffers(SavedBuffer savedBuffer)
        {
            _savedBuffers.Remove(savedBuffer);
            SaveBuffer();
            _data = savedBuffer.Buffer;
            _baseOffset = savedBuffer.BaseOffset;
            _end = savedBuffer.End;
            _discardCount = savedBuffer.DiscardCount;
        }

        void CalcBuffer(long offset, int count, out int readStart, out int readEnd)
        {
            readStart = 0;
            readEnd = 0;
            if (offset < _baseOffset)
            {
                // try to overlap the end...
                if (offset + _maxSize <= _baseOffset)
                {
                    // nope...
                    if (_baseOffset - (offset + _maxSize) > _maxSize)
                    {
                        // it's probably best to cache this buffer for a bit
                        CreateNewBuffer(offset, count);
                    }
                    else
                    {
                        // don't worry about caching...
                        EnsureBufferSize(count, false, 0);
                    }
                    _baseOffset = offset;
                    readEnd = count;
                }
                else
                {
                    // we have at least some overlap
                    readEnd = (int)(offset - _baseOffset);
                    EnsureBufferSize(Math.Min((int)(offset + _maxSize - _baseOffset), _end) - readEnd, true, readEnd);
                    readEnd = (int)(offset - _baseOffset) - readEnd;
                }
            }
            else
            {
                // try to overlap the beginning...
                if (offset >= _baseOffset + _maxSize)
                {
                    // nope...
                    if (offset - (_baseOffset + _maxSize) > _maxSize)
                    {
                        CreateNewBuffer(offset, count);
                    }
                    else
                    {
                        EnsureBufferSize(count, false, 0);
                    }
                    _baseOffset = offset;
                    readEnd = count;
                }
                else
                {
                    // we have at least some overlap
                    readEnd = (int)(offset + count - _baseOffset);
                    var ofs = Math.Max(readEnd - _maxSize, 0);
                    EnsureBufferSize(readEnd - ofs, true, ofs);
                    readStart = _end;
                    // re-pull in case EnsureBufferSize had to discard...
                    readEnd = (int)(offset + count - _baseOffset);
                }
            }
        }

        void EnsureBufferSize(int reqSize, bool copyContents, int copyOffset)
        {
            byte[] newBuf = _data;
            if (reqSize > _data.Length)
            {
                if (reqSize > _maxSize)
                {
                    if (_wrapper.Source.CanSeek || reqSize - _discardCount <= _maxSize)
                    {
                        // lose some of the earlier data...
                        var ofs = reqSize - _maxSize;
                        copyOffset += ofs;
                        reqSize = _maxSize;
                    }
                    else
                    {
                        throw new InvalidOperationException("Not enough room in the buffer!  Increase the maximum size and try again.");
                    }
                }
                else
                {
                    // find the new size
                    var size = _data.Length;
                    while (size < reqSize)
                    {
                        size *= 2;
                    }
                    reqSize = size;
                }

                // if we discarded some bytes above, don't resize the buffer unless we have to...
                if (reqSize > _data.Length)
                {
                    newBuf = new byte[reqSize];
                }
            }

            if (copyContents)
            {
                // adjust the position of the data
                if ((copyOffset > 0 && copyOffset < _end) || (copyOffset == 0 && newBuf != _data))
                {
                    // copy forward
                    Buffer.BlockCopy(_data, copyOffset, newBuf, 0, _end - copyOffset);

                    // adjust our discard count
                    if ((_discardCount -= copyOffset) < 0) _discardCount = 0;
                }
                else if (copyOffset < 0 && -copyOffset < _end)
                {
                    // copy backward
                    // be clever... if we're moving to a new buffer or the ranges don't overlap, just use a block copy
                    if (newBuf != _data || _end <= -copyOffset)
                    {
                        Buffer.BlockCopy(_data, 0, newBuf, -copyOffset, Math.Max(_end, Math.Min(_end, _data.Length + copyOffset)));
                    }
                    else
                    {
                        // this shouldn't happen often, so we can get away with a full buffer refill
                        _end = copyOffset;
                    }

                    // adjust our discard count
                    _discardCount = 0;
                }
                else
                {
                    _end = copyOffset;
                    _discardCount = 0;
                }

                // adjust our markers
                _baseOffset += copyOffset;
                _end -= copyOffset;
                if (_end > newBuf.Length) _end = newBuf.Length;
            }
            else
            {
                _discardCount = 0;
                // we can't set _baseOffset since our caller hasn't told us what it should be...
                _end = 0;
            }

            _data = newBuf;
        }

        int FillBuffer(long offset, int count, int readStart, int readEnd)
        {
            var readOffset = _baseOffset + readStart;
            var readCount = readEnd - readStart;

            lock (_wrapper.LockObject)
            {
                readCount = PrepareStreamForRead(readCount, readOffset);

                ReadStream(readStart, readCount, readOffset);

                // check for full read...
                if (_end < readStart + readCount)
                {
                    count = Math.Max(0, (int)(_baseOffset + _end - offset));
                }
                else if (!_minimalRead && _end < _data.Length)
                {
                    // try to finish filling the buffer
                    readCount = _data.Length - _end;
                    readCount = PrepareStreamForRead(readCount, _baseOffset + _end);
                    _end += _wrapper.Source.Read(_data, _end, readCount);
                }
            }
            return count;
        }

        int PrepareStreamForRead(int readCount, long readOffset)
        {
            if (readCount > 0 && _wrapper.Source.Position != readOffset)
            {
                if (readOffset < _wrapper.EofOffset)
                {
                    if (_wrapper.Source.CanSeek)
                    {
                        _wrapper.Source.Position = readOffset;
                    }
                    else
                    {
                        // ugh, gotta read bytes until we've reached the desired offset
                        var seekCount = readOffset - _wrapper.Source.Position;
                        if (seekCount < 0)
                        {
                            // not so fast... we can't seek backwards.  This technically shouldn't happen, but just in case...
                            readCount = 0;
                        }
                        else
                        {
                            while (--seekCount >= 0)
                            {
                                if (_wrapper.Source.ReadByte() == -1)
                                {
                                    // crap... we just threw away a bunch of bytes for no reason
                                    _wrapper.EofOffset = _wrapper.Source.Position;
                                    readCount = 0;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    readCount = 0;
                }
            }
            return readCount;
        }

        void ReadStream(int readStart, int readCount, long readOffset)
        {
            while (readCount > 0 && readOffset < _wrapper.EofOffset)
            {
                var temp = _wrapper.Source.Read(_data, readStart, readCount);
                if (temp == 0)
                {
                    break;
                }
                readStart += temp;
                readOffset += temp;
                readCount -= temp;
            }

            if (readStart > _end)
            {
                _end = readStart;
            }
        }

        /// <summary>
        /// Tells the buffer that it no longer needs to maintain any bytes before the indicated offset.
        /// </summary>
        /// <param name="offset">The offset to discard through.</param>
        public void DiscardThrough(long offset)
        {
            var count = (int)(offset - _baseOffset);
            _discardCount = Math.Max(count, _discardCount);

            if (_discardCount >= _data.Length) CommitDiscard();
        }

        void CommitDiscard()
        {
            if (_discardCount >= _data.Length || _discardCount >= _end)
            {
                // we have been told to discard the entire buffer
                _baseOffset += _discardCount;
                _end = 0;
            }
            else
            {
                // just discard the first part...
                Buffer.BlockCopy(_data, _discardCount, _data, 0, _end - _discardCount);
                _baseOffset += _discardCount;
                _end -= _discardCount;
            }
            _discardCount = 0;
        }
    }
}
