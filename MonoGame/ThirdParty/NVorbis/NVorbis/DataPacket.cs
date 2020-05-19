﻿/****************************************************************************
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
    /// <summary>
    /// A single data packet from a logical Vorbis stream.
    /// </summary>
    public abstract class DataPacket
    {
        ulong _bitBucket;           // 8
        int _bitCount;              // 4
        int _readBits;              // 4
        byte _overflowBits;         // 1
        PacketFlags _packetFlags;   // 1
        long _granulePosition;      // 8
        long _pageGranulePosition;  // 8
        int _length;                // 4
        int _granuleCount;          // 4
        int _pageSequenceNumber;    // 4

        /// <summary>
        /// Defines flags to apply to the current packet
        /// </summary>
        [Flags]
        // for now, let's use a byte... if we find we need more space, we can always expand it...
        protected enum PacketFlags : byte
        {
            /// <summary>
            /// Packet is first since reader had to resync with stream.
            /// </summary>
            IsResync        = 0x01,
            /// <summary>
            /// Packet is the last in the logical stream.
            /// </summary>
            IsEndOfStream   = 0x02,
            /// <summary>
            /// Packet does not have all its data available.
            /// </summary>
            IsShort         = 0x04,
            /// <summary>
            /// Packet has a granule count defined.
            /// </summary>
            HasGranuleCount = 0x08,

            /// <summary>
            /// Flag for use by inheritors.
            /// </summary>
            User1           = 0x10,
            /// <summary>
            /// Flag for use by inheritors.
            /// </summary>
            User2           = 0x20,
            /// <summary>
            /// Flag for use by inheritors.
            /// </summary>
            User3           = 0x40,
            /// <summary>
            /// Flag for use by inheritors.
            /// </summary>
            User4           = 0x80,
        }

        /// <summary>
        /// Gets the value of the specified flag.
        /// </summary>
        protected bool GetFlag(PacketFlags flag)
        {
            return (_packetFlags & flag) == flag;
        }

        /// <summary>
        /// Sets the value of the specified flag.
        /// </summary>
        protected void SetFlag(PacketFlags flag, bool value)
        {
            if (value)
            {
                _packetFlags |= flag;
            }
            else
            {
                _packetFlags &= ~flag;
            }
        }

        /// <summary>
        /// Creates a new instance with the specified length.
        /// </summary>
        /// <param name="length">The length of the packet.</param>
        protected DataPacket(int length)
        {
            Length = length;
        }

        /// <summary>
        /// Reads the next byte of the packet.
        /// </summary>
        /// <returns>The next byte if available, otherwise -1.</returns>
        abstract protected int ReadNextByte();

        /// <summary>
        /// Indicates that the packet has been read and its data is no longer needed.
        /// </summary>
        virtual public void Done()
        {
        }

        /// <summary>
        /// Attempts to read the specified number of bits from the packet, but may return fewer.  Does not advance the position counter.
        /// </summary>
        /// <param name="count">The number of bits to attempt to read.</param>
        /// <param name="bitsRead">The number of bits actually read.</param>
        /// <returns>The value of the bits read.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is not between 0 and 64.</exception>
        public ulong TryPeekBits(int count, out int bitsRead)
        {
            ulong value = 0;

            if (count < 0 || count > 64) throw new ArgumentOutOfRangeException("count");
            if (count == 0)
            {
                bitsRead = 0;
                return 0UL;
            }

            while (_bitCount < count)
            {
                var val = ReadNextByte();
                if (val == -1)
                {
                    bitsRead = _bitCount;
                    value = _bitBucket;
                    _bitBucket = 0;
                    _bitCount = 0;

                    IsShort = true;

                    return value;
                }
                _bitBucket = (ulong)(val & 0xFF) << _bitCount | _bitBucket;
                _bitCount += 8;
                
                if (_bitCount > 64)
                {
                    _overflowBits = (byte)(val >> (72 - _bitCount));
                }
            }

            value = _bitBucket;

            if (count < 64)
            {
                value &= (1UL << count) - 1;
            }

            bitsRead = count;
            return value;
        }

        /// <summary>
        /// Advances the position counter by the specified number of bits.
        /// </summary>
        /// <param name="count">The number of bits to advance.</param>
        public void SkipBits(int count)
        {
            if (count == 0)
            {
                // no-op
            }
            else if (_bitCount > count)
            {
                // we still have bits left over...
                if (count > 63)
                {
                    _bitBucket = 0;
                }
                else
                {
                    _bitBucket >>= count;
                }
                if (_bitCount > 64)
                {
                    var overflowCount = _bitCount - 64;
                    _bitBucket |= (ulong)_overflowBits << (_bitCount - count - overflowCount);

                    if (overflowCount > count)
                    {
                        // ugh, we have to keep bits in overflow
                        _overflowBits >>= count;
                    }
                }

                _bitCount -= count;
                _readBits += count;
            }
            else if (_bitCount == count)
            {
                _bitBucket = 0UL;
                _bitCount = 0;
                _readBits += count;
            }
            else //  _bitCount < count
            {
                // we have to move more bits than we have available...
                count -= _bitCount;
                _readBits += _bitCount;
                _bitCount = 0;
                _bitBucket = 0;

                while (count > 8)
                {
                    if (ReadNextByte() == -1)
                    {
                        count = 0;
                        IsShort = true;
                        break;
                    }
                    count -= 8;
                    _readBits += 8;
                }

                if (count > 0)
                {
                    var temp = ReadNextByte();
                    if (temp == -1)
                    {
                        IsShort = true;
                    }
                    else
                    {
                        _bitBucket = (ulong)(temp >> count);
                        _bitCount = 8 - count;
                        _readBits += count;
                    }
                }
            }
        }

        /// <summary>
        /// Resets the bit reader.
        /// </summary>
        protected void ResetBitReader()
        {
            _bitBucket = 0;
            _bitCount = 0;
            _readBits = 0;

            IsShort = false;
        }

        /// <summary>
        /// Gets whether the packet was found after a stream resync.
        /// </summary>
        public bool IsResync
        {
            get { return GetFlag(PacketFlags.IsResync); }
            internal set { SetFlag(PacketFlags.IsResync, value); }
        }

        /// <summary>
        /// Gets the position of the last granule in the packet.
        /// </summary>
        public long GranulePosition
        {
            get { return _granulePosition; }
            set { _granulePosition = value; }
        }

        /// <summary>
        /// Gets the position of the last granule in the page the packet is in.
        /// </summary>
        public long PageGranulePosition
        {
            get { return _pageGranulePosition; }
            internal set { _pageGranulePosition = value; }
        }

        /// <summary>
        /// Gets the length of the packet.
        /// </summary>
        public int Length
        {
            get { return _length; }
            protected set { _length = value; }
        }

        /// <summary>
        /// Gets whether the packet is the last one in the logical stream.
        /// </summary>
        public bool IsEndOfStream
        {
            get { return GetFlag(PacketFlags.IsEndOfStream); }
            internal set { SetFlag(PacketFlags.IsEndOfStream, value); }
        }

        /// <summary>
        /// Gets the number of bits read from the packet.
        /// </summary>
        public long BitsRead
        {
            get { return _readBits; }
        }

        /// <summary>
        /// Gets the number of granules in the packet.  If <c>null</c>, the packet has not been decoded yet.
        /// </summary>
        public int? GranuleCount
        {
            get
            {
                if (GetFlag(PacketFlags.HasGranuleCount))
                {
                    return _granuleCount;
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    _granuleCount = value.Value;
                    SetFlag(PacketFlags.HasGranuleCount, true);
                }
                else
                {
                    SetFlag(PacketFlags.HasGranuleCount, false);
                }
            }
        }

        internal int PageSequenceNumber
        {
            get { return _pageSequenceNumber; }
            set { _pageSequenceNumber = value; }
        }

        internal bool IsShort
        {
            get { return GetFlag(PacketFlags.IsShort); }
            private set { SetFlag(PacketFlags.IsShort, value); }
        }

        /// <summary>
        /// Reads the specified number of bits from the packet and advances the position counter.
        /// </summary>
        /// <param name="count">The number of bits to read.</param>
        /// <returns>The value of the bits read.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The number of bits specified is not between 0 and 64.</exception>
        public ulong ReadBits(int count)
        {
            // short-circuit 0
            if (count == 0) return 0UL;

            int temp;
            var value = TryPeekBits(count, out temp);

            SkipBits(count);

            return value;
        }

        /// <summary>
        /// Reads the next byte from the packet.  Does not advance the position counter.
        /// </summary>
        /// <returns>The byte read from the packet.</returns>
        public byte PeekByte()
        {
            int temp;
            return (byte)TryPeekBits(8, out temp);
        }

        /// <summary>
        /// Reads the next byte from the packet and advances the position counter.
        /// </summary>
        /// <returns>The byte read from the packet.</returns>
        public byte ReadByte()
        {
            return (byte)ReadBits(8);
        }

        /// <summary>
        /// Reads the specified number of bytes from the packet and advances the position counter.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>A byte array holding the data read.</returns>
        public byte[] ReadBytes(int count)
        {
            var buf = new List<byte>(count);

            while (buf.Count < count)
            {
                buf.Add(ReadByte());
            }

            return buf.ToArray();
        }

        /// <summary>
        /// Reads the specified number of bytes from the packet into the buffer specified and advances the position counter.
        /// </summary>
        /// <param name="buffer">The buffer to read into.</param>
        /// <param name="index">The index into the buffer to start placing the read data.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or <paramref name="index"/> + <paramref name="count"/> is past the end of <paramref name="buffer"/>.</exception>
        public int Read(byte[] buffer, int index, int count)
        {
            if (index < 0 || index + count > buffer.Length) throw new ArgumentOutOfRangeException("index");
            for (int i = 0; i < count; i++)
            {
                int cnt;
                byte val = (byte)TryPeekBits(8, out cnt);
                if (cnt == 0)
                {
                    return i;
                }
                buffer[index++] = val;
                SkipBits(8);
            }
            return count;
        }

        /// <summary>
        /// Reads the next bit from the packet and advances the position counter.
        /// </summary>
        /// <returns>The value of the bit read.</returns>
        public bool ReadBit()
        {
            return ReadBits(1) == 1;
        }

        /// <summary>
        /// Retrieves the next 16 bits from the packet as a <see cref="short"/> and advances the position counter.
        /// </summary>
        /// <returns>The value of the next 16 bits.</returns>
        public short ReadInt16()
        {
            return (short)ReadBits(16);
        }

        /// <summary>
        /// Retrieves the next 32 bits from the packet as a <see cref="int"/> and advances the position counter.
        /// </summary>
        /// <returns>The value of the next 32 bits.</returns>
        public int ReadInt32()
        {
            return (int)ReadBits(32);
        }

        /// <summary>
        /// Retrieves the next 64 bits from the packet as a <see cref="long"/> and advances the position counter.
        /// </summary>
        /// <returns>The value of the next 64 bits.</returns>
        public long ReadInt64()
        {
            return (long)ReadBits(64);
        }

        /// <summary>
        /// Retrieves the next 16 bits from the packet as a <see cref="ushort"/> and advances the position counter.
        /// </summary>
        /// <returns>The value of the next 16 bits.</returns>
        public ushort ReadUInt16()
        {
            return (ushort)ReadBits(16);
        }

        /// <summary>
        /// Retrieves the next 32 bits from the packet as a <see cref="uint"/> and advances the position counter.
        /// </summary>
        /// <returns>The value of the next 32 bits.</returns>
        public uint ReadUInt32()
        {
            return (uint)ReadBits(32);
        }

        /// <summary>
        /// Retrieves the next 64 bits from the packet as a <see cref="ulong"/> and advances the position counter.
        /// </summary>
        /// <returns>The value of the next 64 bits.</returns>
        public ulong ReadUInt64()
        {
            return (ulong)ReadBits(64);
        }

        /// <summary>
        /// Advances the position counter by the specified number of bytes.
        /// </summary>
        /// <param name="count">The number of bytes to advance.</param>
        public void SkipBytes(int count)
        {
            SkipBits(count * 8);
        }
    }
}