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

namespace NVorbis.Ogg
{
    /// <summary>
    /// Provides an <see cref="IContainerReader"/> implementation for basic Ogg files.
    /// </summary>
    public class ContainerReader : IContainerReader
    {
        Crc _crc = new Crc();
        BufferedReadStream _stream;
        Dictionary<int, PacketReader> _packetReaders;
        List<int> _disposedStreamSerials;
        long _nextPageOffset;
        int _pageCount;

        byte[] _readBuffer = new byte[65025];   // up to a full page of data (but no more!)

        long _containerBits, _wasteBits;

        /// <summary>
        /// Gets the list of stream serials found in the container so far.
        /// </summary>
        public int[] StreamSerials
        {
            get { return System.Linq.Enumerable.ToArray<int>(_packetReaders.Keys); }
        }

        /// <summary>
        /// Event raised when a new logical stream is found in the container.
        /// </summary>
        public event EventHandler<NewStreamEventArgs> NewStream;

        /// <summary>
        /// Creates a new instance with the specified file.
        /// </summary>
        /// <param name="path">The full path to the file.</param>
        public ContainerReader(string path)
            : this(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), true)
        {
        }

        /// <summary>
        /// Creates a new instance with the specified stream.  Optionally sets to close the stream when disposed.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="closeOnDispose"><c>True</c> to close the stream when <see cref="Dispose"/> is called, otherwise <c>False</c>.</param>
        public ContainerReader(Stream stream, bool closeOnDispose)
        {
            _packetReaders = new Dictionary<int, PacketReader>();
            _disposedStreamSerials = new List<int>();

            _stream = (stream as BufferedReadStream) ?? new BufferedReadStream(stream) { CloseBaseStream = closeOnDispose };
        }

        /// <summary>
        /// Initializes the container and finds the first stream.
        /// </summary>
        /// <returns><c>True</c> if a valid logical stream is found, otherwise <c>False</c>.</returns>
        public bool Init()
        {
            _stream.TakeLock();
            try
            {
                return GatherNextPage() != -1;
            }
            finally
            {
                _stream.ReleaseLock();
            }
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            // don't use _packetReaders directly since that'll change the enumeration...
            foreach (var streamSerial in StreamSerials)
            {
                _packetReaders[streamSerial].Dispose();
            }

            _nextPageOffset = 0L;
            _containerBits = 0L;
            _wasteBits = 0L;

            _stream.Dispose();
        }

        /// <summary>
        /// Gets the <see cref="IPacketProvider"/> instance for the specified stream serial.
        /// </summary>
        /// <param name="streamSerial">The stream serial to look for.</param>
        /// <returns>An <see cref="IPacketProvider"/> instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The specified stream serial was not found.</exception>
        public IPacketProvider GetStream(int streamSerial)
        {
            PacketReader provider;
            if (!_packetReaders.TryGetValue(streamSerial, out provider))
            {
                throw new ArgumentOutOfRangeException("streamSerial");
            }
            return provider;
        }

        /// <summary>
        /// Finds the next new stream in the container.
        /// </summary>
        /// <returns><c>True</c> if a new stream was found, otherwise <c>False</c>.</returns>
        /// <exception cref="InvalidOperationException"><see cref="CanSeek"/> is <c>False</c>.</exception>
        public bool FindNextStream()
        {
            if (!CanSeek) throw new InvalidOperationException();

            // goes through all the pages until the serial count increases
            var cnt = this._packetReaders.Count;
            while (this._packetReaders.Count == cnt)
            {
                _stream.TakeLock();
                try
                {
                    // acquire & release the lock every pass so we don't block any longer than necessary
                    if (GatherNextPage() == -1)
                    {
                        break;
                    }
                }
                finally
                {
                    _stream.ReleaseLock();
                }
            }
            return cnt > this._packetReaders.Count;
        }

        /// <summary>
        /// Gets the number of pages that have been read in the container.
        /// </summary>
        public int PagesRead
        {
            get { return _pageCount; }
        }

        /// <summary>
        /// Retrieves the total number of pages in the container.
        /// </summary>
        /// <returns>The total number of pages.</returns>
        /// <exception cref="InvalidOperationException"><see cref="CanSeek"/> is <c>False</c>.</exception>
        public int GetTotalPageCount()
        {
            if (!CanSeek) throw new InvalidOperationException();

            // just read pages until we can't any more...
            while (true)
            {
                _stream.TakeLock();
                try
                {
                    // acquire & release the lock every pass so we don't block any longer than necessary
                    if (GatherNextPage() == -1)
                    {
                        break;
                    }
                }
                finally
                {
                    _stream.ReleaseLock();
                }
            }

            return _pageCount;
        }

        /// <summary>
        /// Gets whether the container supports seeking.
        /// </summary>
        public bool CanSeek
        {
            get { return _stream.CanSeek; }
        }

        /// <summary>
        /// Gets the number of bits in the container that are not associated with a logical stream.
        /// </summary>
        public long WasteBits
        {
            get { return _wasteBits; }
        }


        // private implmentation bits
        class PageHeader
        {
            public int StreamSerial { get; set; }
            public PageFlags Flags { get; set; }
            public long GranulePosition { get; set; }
            public int SequenceNumber { get; set; }
            public long DataOffset { get; set; }
            public int[] PacketSizes { get; set; }
            public bool LastPacketContinues { get; set; }
            public bool IsResync { get; set; }
        }

        PageHeader ReadPageHeader(long position)
        {
            // set the stream's position
            _stream.Seek(position, SeekOrigin.Begin);

            // header
            // NB: if the stream didn't have an EOS flag, this is the most likely spot for the EOF to be found...
            if (_stream.Read(_readBuffer, 0, 27) != 27) return null;

            // capture signature
            if (_readBuffer[0] != 0x4f || _readBuffer[1] != 0x67 || _readBuffer[2] != 0x67 || _readBuffer[3] != 0x53) return null;

            // check the stream version
            if (_readBuffer[4] != 0) return null;

            // start populating the header
            var hdr = new PageHeader();

            // bit flags
            hdr.Flags = (PageFlags)_readBuffer[5];

            // granulePosition
            hdr.GranulePosition = BitConverter.ToInt64(_readBuffer, 6);

            // stream serial
            hdr.StreamSerial = BitConverter.ToInt32(_readBuffer, 14);

            // sequence number
            hdr.SequenceNumber = BitConverter.ToInt32(_readBuffer, 18);

            // save off the CRC
            var crc = BitConverter.ToUInt32(_readBuffer, 22);

            // start calculating the CRC value for this page
            _crc.Reset();
            for (int i = 0; i < 22; i++)
            {
                _crc.Update(_readBuffer[i]);
            }
            _crc.Update(0);
            _crc.Update(0);
            _crc.Update(0);
            _crc.Update(0);
            _crc.Update(_readBuffer[26]);

            // figure out the length of the page
            var segCnt = (int)_readBuffer[26];
            if (_stream.Read(_readBuffer, 0, segCnt) != segCnt) return null;

            var packetSizes = new List<int>(segCnt);

            int size = 0, idx = 0;
            for (int i = 0; i < segCnt; i++)
            {
                var temp = _readBuffer[i];
                _crc.Update(temp);

                if (idx == packetSizes.Count) packetSizes.Add(0);
                packetSizes[idx] += temp;
                if (temp < 255)
                {
                    ++idx;
                    hdr.LastPacketContinues = false;
                }
                else
                {
                    hdr.LastPacketContinues = true;
                }

                size += temp;
            }
            hdr.PacketSizes = packetSizes.ToArray();
            hdr.DataOffset = position + 27 + segCnt;

            // now we have to go through every byte in the page
            if (_stream.Read(_readBuffer, 0, size) != size) return null;
            for (int i = 0; i < size; i++)
            {
                _crc.Update(_readBuffer[i]);
            }

            if (_crc.Test(crc))
            {
                _containerBits += 8 * (27 + segCnt);
                ++_pageCount;
                return hdr;
            }
            return null;
        }

        PageHeader FindNextPageHeader()
        {
            var startPos = _nextPageOffset;

            var isResync = false;
            PageHeader hdr;
            while ((hdr = ReadPageHeader(startPos)) == null)
            {
                isResync = true;
                _wasteBits += 8;
                _stream.Position = ++startPos;

                var cnt = 0;
                do
                {
                    var b = _stream.ReadByte();
                    if (b == 0x4f)
                    {
                        if (_stream.ReadByte() == 0x67 && _stream.ReadByte() == 0x67 && _stream.ReadByte() == 0x53)
                        {
                            // found it!
                            startPos += cnt;
                            break;
                        }
                        else
                        {
                            _stream.Seek(-3, SeekOrigin.Current);
                        }
                    }
                    else if (b == -1)
                    {
                        return null;
                    }
                    _wasteBits += 8;
                } while (++cnt < 65536);    // we will only search through 64KB of data to find the next sync marker.  if it can't be found, we have a badly corrupted stream.
                if (cnt == 65536) return null;
            }
            hdr.IsResync = isResync;

            _nextPageOffset = hdr.DataOffset;
            for (int i = 0; i < hdr.PacketSizes.Length; i++)
            {
                _nextPageOffset += hdr.PacketSizes[i];
            }

            return hdr;
        }

        bool AddPage(PageHeader hdr)
        {
            // get our packet reader (create one if we have to)
            PacketReader packetReader;
            if (!_packetReaders.TryGetValue(hdr.StreamSerial, out packetReader))
            {
                packetReader = new PacketReader(this, hdr.StreamSerial);
            }

            // save off the container bits
            packetReader.ContainerBits += _containerBits;
            _containerBits = 0;

            // get our flags prepped
            var isContinued = hdr.PacketSizes.Length == 1 && hdr.LastPacketContinues;
            var isContinuation = (hdr.Flags & PageFlags.ContinuesPacket) == PageFlags.ContinuesPacket;
            var isEOS = false;
            var isResync = hdr.IsResync;

            // add all the packets, making sure to update flags as needed
            var dataOffset = hdr.DataOffset;
            var cnt = hdr.PacketSizes.Length;
            foreach (var size in hdr.PacketSizes)
            {
                var packet = new Packet(this, dataOffset, size)
                    {
                        PageGranulePosition = hdr.GranulePosition,
                        IsEndOfStream = isEOS,
                        PageSequenceNumber = hdr.SequenceNumber,
                        IsContinued = isContinued,
                        IsContinuation = isContinuation,
                        IsResync = isResync,
                    };
                packetReader.AddPacket(packet);

                // update the offset into the stream for each packet
                dataOffset += size;

                // only the first packet in a page can be a continuation or resync
                isContinuation = false;
                isResync = false;

                // only the last packet in a page can be continued or flagged end of stream
                if (--cnt == 1)
                {
                    isContinued = hdr.LastPacketContinues;
                    isEOS = (hdr.Flags & PageFlags.EndOfStream) == PageFlags.EndOfStream;
                }
            }

            // if the packet reader list doesn't include the serial in question, add it to the list and indicate a new stream to the caller
            if (!_packetReaders.ContainsKey(hdr.StreamSerial))
            {
                _packetReaders.Add(hdr.StreamSerial, packetReader);
                return true;
            }
            else
            {
                // otherwise, indicate an existing stream to the caller
                return false;
            }
        }

        int GatherNextPage()
        {
            while (true)
            {
                // get our next header
                var hdr = FindNextPageHeader();
                if (hdr == null)
                {
                    return -1;
                }
                
                // if it's in a disposed stream, grab the next page instead
                if (_disposedStreamSerials.Contains(hdr.StreamSerial)) continue;
                
                // otherwise, add it
                if (AddPage(hdr))
                {
                    var callback = NewStream;
                    if (callback != null)
                    {
                        var ea = new NewStreamEventArgs(_packetReaders[hdr.StreamSerial]);
                        callback(this, ea);
                        if (ea.IgnoreStream)
                        {
                            _packetReaders[hdr.StreamSerial].Dispose();
                            continue;
                        }
                    }
                }
                return hdr.StreamSerial;
            }
        }

        // packet reader bits...
        internal void DisposePacketReader(PacketReader packetReader)
        {
            _disposedStreamSerials.Add(packetReader.StreamSerial);
            _packetReaders.Remove(packetReader.StreamSerial);
        }

        internal int PacketReadByte(long offset)
        {
            _stream.TakeLock();
            try
            {
                _stream.Position = offset;
                return _stream.ReadByte();
            }
            finally
            {
                _stream.ReleaseLock();
            }
        }

        internal void PacketDiscardThrough(long offset)
        {
            _stream.TakeLock();
            try
            {
                _stream.DiscardThrough(offset);
            }
            finally
            {
                _stream.ReleaseLock();
            }
        }

        internal void GatherNextPage(int streamSerial)
        {
            if (!_packetReaders.ContainsKey(streamSerial)) throw new ArgumentOutOfRangeException("streamSerial");

            int nextSerial;
            do
            {
                _stream.TakeLock();
                try
                {
                    if (_packetReaders[streamSerial].HasEndOfStream) break;

                    nextSerial = GatherNextPage();
                    if (nextSerial == -1)
                    {
                        foreach (var reader in _packetReaders)
                        {
                            if (!reader.Value.HasEndOfStream)
                            {
                                reader.Value.SetEndOfStream();
                            }
                        }
                        break;
                    }
                }
                finally
                {
                    _stream.ReleaseLock();
                }
            } while (nextSerial != streamSerial);
        }
    }
}
