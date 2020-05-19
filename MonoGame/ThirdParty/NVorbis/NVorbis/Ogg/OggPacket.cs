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
using System.Text;
using System.IO;

namespace NVorbis.Ogg
{
    class Packet : DataPacket
    {
        long _offset;                       // 8
        int _length;                        // 4
        int _curOfs;                        // 4
        Packet _mergedPacket;               // IntPtr.Size
        Packet _next;                       // IntPtr.Size
        Packet _prev;                       // IntPtr.Size
        ContainerReader _containerReader;   // IntPtr.Size

        internal Packet Next
        {
            get { return _next; }
            set { _next = value; }
        }
        internal Packet Prev
        {
            get { return _prev; }
            set { _prev = value; }
        }
        internal bool IsContinued
        {
            get { return GetFlag(PacketFlags.User1); }
            set { SetFlag(PacketFlags.User1, value); }
        }
        internal bool IsContinuation
        {
            get { return GetFlag(PacketFlags.User2); }
            set { SetFlag(PacketFlags.User2, value); }
        }

        internal Packet(ContainerReader containerReader, long streamOffset, int length)
            : base(length)
        {
            _containerReader = containerReader;

            _offset = streamOffset;
            _length = length;
            _curOfs = 0;
        }

        internal void MergeWith(NVorbis.DataPacket continuation)
        {
            var op = continuation as Packet;

            if (op == null) throw new ArgumentException("Incorrect packet type!");

            Length += continuation.Length;

            if (_mergedPacket == null)
            {
                _mergedPacket = op;
            }
            else
            {
                _mergedPacket.MergeWith(continuation);
            }

            // per the spec, a partial packet goes with the next page's granulepos.  we'll go ahead and assign it to the next page as well
            PageGranulePosition = continuation.PageGranulePosition;
            PageSequenceNumber = continuation.PageSequenceNumber;
        }

        internal void Reset()
        {
            _curOfs = 0;
            ResetBitReader();

            if (_mergedPacket != null)
            {
                _mergedPacket.Reset();
            }
        }

        protected override int ReadNextByte()
        {
            if (_curOfs == _length)
            {
                if (_mergedPacket == null) return -1;

                return _mergedPacket.ReadNextByte();
            }

            var b = _containerReader.PacketReadByte(_offset + _curOfs);
            if (b != -1)
            {
                ++_curOfs;
            }
            return b;
        }

        public override void Done()
        {
            if (_mergedPacket != null)
            {
                _mergedPacket.Done();
            }
            else
            {
                _containerReader.PacketDiscardThrough(_offset + _length);
            }
        }
    }
}
