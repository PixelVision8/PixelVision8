/****************************************************************************
 * NVorbis                                                                  *
 * Copyright (C) 2014, Andrew Ward <afward@gmail.com>                       *
 *                                                                          *
 * See COPYING for license terms (Ms-PL).                                   *
 *                                                                          *
 ***************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace NVorbis.Ogg
{
    [System.Diagnostics.DebuggerTypeProxy(typeof(PacketReader.DebugView))]
    class PacketReader : IPacketProvider
    {
        class DebugView
        {
            PacketReader _reader;

            public DebugView(PacketReader reader)
            {
                if (reader == null) throw new ArgumentNullException("reader");
                _reader = reader;
            }

            public ContainerReader Container { get { return _reader._container; } }
            public int StreamSerial { get { return _reader._streamSerial; } }
            public bool EndOfStreamFound { get { return _reader._eosFound; } }

            public int CurrentPacketIndex
            {
                get
                {
                    if (_reader._current == null) return -1;
                    return Array.IndexOf(Packets, _reader._current);
                }
            }

            Packet _last, _first;
            Packet[] _packetList = new Packet[0];
            public Packet[] Packets
            {
                get
                {
                    if (_reader._last == _last && _reader._first == _first)
                    {
                        return _packetList;
                    }

                    _last = _reader._last;
                    _first = _reader._first;

                    var packets = new List<Packet>();
                    var node = _first;
                    while (node != null)
                    {
                        packets.Add(node);
                        node = node.Next;
                    }
                    _packetList = packets.ToArray();
                    return _packetList;
                }
            }
        }

        // IPacketProvider requires this, but we aren't using it
#pragma warning disable 67  // disable the "unused" warning
        public event EventHandler<ParameterChangeEventArgs> ParameterChange;
#pragma warning restore 67

        ContainerReader _container;
        int _streamSerial;
        bool _eosFound;

        Packet _first, _current, _last;

        object _packetLock = new object();

        internal PacketReader(ContainerReader container, int streamSerial)
        {
            _container = container;
            _streamSerial = streamSerial;
        }

        public void Dispose()
        {
            _eosFound = true;

            if (_container != null)
                _container.DisposePacketReader(this);
            _container = null;

            _current = null;

            if (_first != null)
            {
                var node = _first;
                _first = null;
                while (node.Next != null)
                {
                    var temp = node.Next;
                    node.Next = null;
                    node = temp;
                    node.Prev = null;
                }
                node = null;
            }

            _last = null;
        }

        internal void AddPacket(Packet packet)
        {
            lock (_packetLock)
            {
                // if we've already found the end of the stream, don't accept any more packets
                if (_eosFound) return;

                // if the packet is a resync, it cannot be a continuation...
                if (packet.IsResync)
                {
                    packet.IsContinuation = false;
                    if (_last != null) _last.IsContinued = false;
                }

                if (packet.IsContinuation)
                {
                    // if we get here, the stream is invalid if there isn't a previous packet
                    if (_last == null) throw new InvalidDataException();

                    // if the last packet isn't continued, something is wrong
                    if (!_last.IsContinued) throw new InvalidDataException();

                    _last.MergeWith(packet);
                    _last.IsContinued = packet.IsContinued;
                }
                else
                {
                    var p = packet as Packet;
                    if (p == null) throw new ArgumentException("Wrong packet datatype", "packet");

                    if (_first == null)
                    {
                        // this is the first packet to add, so just set first & last to point at it
                        _first = p;
                        _last = p;
                    }
                    else
                    {
                        // swap the new packet in to the last position (remember, we're doubly-linked)
                        _last = ((p.Prev = _last).Next = p);
                    }
                }

                if (packet.IsEndOfStream)
                {
                    SetEndOfStream();
                }
            }
        }

        internal bool HasEndOfStream
        {
            get { return _eosFound; }
        }

        internal void SetEndOfStream()
        {
            lock (_packetLock)
            {
                // set the flag...
                _eosFound = true;

                // make sure we're handling the last packet correctly
                if (_last.IsContinued)
                {
                    // last packet was a partial... spec says dump it
                    _last = _last.Prev;
                    _last.Next.Prev = null;
                    _last.Next = null;
                }
            }
        }

        public int StreamSerial
        {
            get { return _streamSerial; }
        }

        public long ContainerBits
        {
            get;
            set;
        }

        public bool CanSeek
        {
            get { return _container.CanSeek; }
        }

        // This is fast path... don't make the caller wait if we can help it...
        public DataPacket GetNextPacket()
        {
            return (_current = PeekNextPacketInternal());
        }

        public DataPacket PeekNextPacket()
        {
            return PeekNextPacketInternal();
        }

        Packet PeekNextPacketInternal()
        {
            // try to get the next packet in the sequence
            Packet curPacket;
            if (_current == null)
            {
                curPacket = _first;
            }
            else
            {
                while (true)
                {
                    lock (_packetLock)
                    {
                        curPacket = _current.Next;

                        // if we have a valid packet or we can't get any more, bail out of the loop
                        if ((curPacket != null && !curPacket.IsContinued) || _eosFound) break;
                    }

                    // we need another packet and we've not found the end of the stream...
                    _container.GatherNextPage(_streamSerial);
                }
            }

            // if we're returning a packet, prep is for use
            if (curPacket != null)
            {
                if (curPacket.IsContinued) throw new InvalidDataException("Packet is incomplete!");
                curPacket.Reset();
            }

            return curPacket;
        }

        internal void ReadAllPages()
        {
            if (!CanSeek) throw new InvalidOperationException();

            // don't hold the lock any longer than we have to
            while (!_eosFound)
            {
                _container.GatherNextPage(_streamSerial);
            }
        }

        internal DataPacket GetLastPacket()
        {
            ReadAllPages();

            return _last;
        }

        public int GetTotalPageCount()
        {
            ReadAllPages();

            // here we just count the number of times the page sequence number changes
            var cnt = 0;
            var lastPageSeqNo = 0;
            var packet = _first;
            while (packet != null)
            {
                if (packet.PageSequenceNumber != lastPageSeqNo)
                {
                    ++cnt;
                    lastPageSeqNo = packet.PageSequenceNumber;
                }
                packet = packet.Next;
            }
            return cnt;
        }

        public DataPacket GetPacket(int packetIndex)
        {
            if (!CanSeek) throw new InvalidOperationException();
            if (packetIndex < 0) throw new ArgumentOutOfRangeException("index");

            // if _first is null, something is borked
            if (_first == null) throw new InvalidOperationException("Packet reader has no packets!");

            // starting from the beginning, count packets until we have the one we want...
            var packet = _first;
            while (--packetIndex >= 0)
            {
                while (packet.Next == null)
                {
                    if (_eosFound)
                    {
                        throw new ArgumentOutOfRangeException("index");
                    }
                    _container.GatherNextPage(_streamSerial);
                }

                packet = packet.Next;
            }

            packet.Reset();
            return packet;
        }

        Packet GetLastPacketInPage(Packet packet)
        {
            if (packet != null)
            {
                var pageSeqNumber = packet.PageSequenceNumber;
                while (packet.Next != null && packet.Next.PageSequenceNumber == pageSeqNumber)
                {
                    packet = packet.Next;
                }

                if (packet != null && packet.IsContinued)
                {
                    // move to the *actual* last packet of the page... If .Prev is null, something is wrong and we can't seek anyway
                    packet = packet.Prev;
                }
            }
            return packet;
        }

        Packet FindPacketInPage(Packet pagePacket, long targetGranulePos, Func<DataPacket, DataPacket, int> packetGranuleCountCallback)
        {
            var lastPacketInPage = GetLastPacketInPage(pagePacket);
            if (lastPacketInPage == null)
            {
                return null;
            }

            // return the packet the granule position is in
            var packet = lastPacketInPage;
            do
            {
                if (!packet.GranuleCount.HasValue)
                {
                    // we don't know its length or position...

                    // if it's the last packet in the page, it gets the page's granule position. Otherwise, calc it.
                    if (packet == lastPacketInPage)
                    {
                        packet.GranulePosition = packet.PageGranulePosition;
                    }
                    else
                    {
                        packet.GranulePosition = packet.Next.GranulePosition - packet.Next.GranuleCount.Value;
                    }

                    // if it's the last packet in the stream, it might be a partial.  The spec says the last packet has to be on its own page, so if it is not assume the stream was truncated.
                    if (packet == _last && _eosFound && packet.Prev.PageSequenceNumber < packet.PageSequenceNumber)
                    {
                        packet.GranuleCount = (int)(packet.GranulePosition - packet.Prev.PageGranulePosition);
                    }
                    else if (packet.Prev != null)
                    {
                        packet.Prev.Reset();
                        packet.Reset();

                        packet.GranuleCount = packetGranuleCountCallback(packet, packet.Prev);
                    }
                    else
                    {
                        // probably the first data packet...
                        if (packet.GranulePosition > packet.Next.GranulePosition - packet.Next.GranuleCount)
                        {
                            throw new InvalidOperationException("First data packet size mismatch");
                        }
                        packet.GranuleCount = (int)packet.GranulePosition;
                    }
                }

                // we now know the granule position and count of the packet... is the target within that range?
                if (targetGranulePos <= packet.GranulePosition && targetGranulePos > packet.GranulePosition - packet.GranuleCount)
                {
                    // make sure the previous packet has a position too
                    if (packet.Prev != null && !packet.Prev.GranuleCount.HasValue)
                    {
                        packet.Prev.GranulePosition = packet.GranulePosition - packet.GranuleCount.Value;
                    }
                    return packet;
                }

                packet = packet.Prev;
            } while (packet != null && packet.PageSequenceNumber == lastPacketInPage.PageSequenceNumber);

            // couldn't find it, but maybe that's because something glitched in the file...
            // we're doing this in case there's a dicontinuity in the file...  It's not perfect, but it'll work
            if (packet != null && packet.PageGranulePosition < targetGranulePos)
            {
                packet.GranulePosition = packet.PageGranulePosition;
                return packet.Next;
            }
            return null;
        }

        public DataPacket FindPacket(long granulePos, Func<DataPacket, DataPacket, int> packetGranuleCountCallback)
        {
            // This will find which packet contains the granule position being requested.  It is basically a linear search.
            // Please note, the spec actually calls for a bisection search, but the result here should be the same.

            // don't look for any position before 0!
            if (granulePos < 0) throw new ArgumentOutOfRangeException("granulePos");

            Packet foundPacket = null;

            // determine which direction to search from...
            var packet = _current ?? _first;
            if (granulePos > packet.PageGranulePosition)
            {
                // forward search

                // find the first packet in the page the requested granule is on
                while (granulePos > packet.PageGranulePosition)
                {
                    if ((packet.Next == null || packet.IsContinued) && !_eosFound)
                    {
                        _container.GatherNextPage(_streamSerial);
                        if (_eosFound)
                        {
                            packet = null;
                            break;
                        }
                    }
                    packet = packet.Next;
                }

                foundPacket = FindPacketInPage(packet, granulePos, packetGranuleCountCallback);
            }
            else
            {
                // reverse search (or we're looking at the same page)
                while (packet.Prev != null && (granulePos <= packet.Prev.PageGranulePosition || packet.Prev.PageGranulePosition == -1))
                {
                    packet = packet.Prev;
                }

                foundPacket = FindPacketInPage(packet, granulePos, packetGranuleCountCallback);
            }

            return foundPacket;
        }

        public void SeekToPacket(DataPacket packet, int preRoll)
        {
            if (preRoll < 0) throw new ArgumentOutOfRangeException("preRoll");
            if (packet == null) throw new ArgumentNullException("granulePos");

            var op = packet as Packet;
            if (op == null) throw new ArgumentException("Incorrect packet type!", "packet");

            while (--preRoll >= 0)
            {
                op = op.Prev;
                if (op == null) throw new ArgumentOutOfRangeException("preRoll");
            }

            // _current always points to the last packet returned by PeekNextPacketInternal
            _current = op.Prev;
        }

        public long GetGranuleCount()
        {
            return GetLastPacket().PageGranulePosition;
        }
    }
}
