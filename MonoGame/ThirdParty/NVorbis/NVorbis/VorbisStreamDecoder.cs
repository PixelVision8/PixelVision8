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

namespace NVorbis
{
    class VorbisStreamDecoder : IVorbisStreamStatus, IDisposable
    {
        internal int _upperBitrate;
        internal int _nominalBitrate;
        internal int _lowerBitrate;

        internal string _vendor;
        internal string[] _comments;

        internal int _channels;
        internal int _sampleRate;
        internal int Block0Size;
        internal int Block1Size;

        internal VorbisCodebook[] Books;
        internal VorbisTime[] Times;
        internal VorbisFloor[] Floors;
        internal VorbisResidue[] Residues;
        internal VorbisMapping[] Maps;
        internal VorbisMode[] Modes;

        int _modeFieldBits;

        #region Stat Fields

        internal long _glueBits;
        internal long _metaBits;
        internal long _bookBits;
        internal long _timeHdrBits;
        internal long _floorHdrBits;
        internal long _resHdrBits;
        internal long _mapHdrBits;
        internal long _modeHdrBits;
        internal long _wasteHdrBits;

        internal long _modeBits;
        internal long _floorBits;
        internal long _resBits;
        internal long _wasteBits;

        internal long _samples;

        internal int _packetCount;

        internal System.Diagnostics.Stopwatch _sw = new System.Diagnostics.Stopwatch();

        #endregion

        IPacketProvider _packetProvider;
        DataPacket _parameterChangePacket;

        List<int> _pagesSeen;
        int _lastPageSeen;

        bool _eosFound;

        object _seekLock = new object();

        internal VorbisStreamDecoder(IPacketProvider packetProvider)
        {
            _packetProvider = packetProvider;
            _packetProvider.ParameterChange += SetParametersChanging;

            _pagesSeen = new List<int>();
            _lastPageSeen = -1;
        }

        internal bool TryInit()
        {
            // try to process the stream header...
            if (!ProcessStreamHeader(_packetProvider.PeekNextPacket()))
            {
                return false;
            }

            // seek past the stream header packet
            _packetProvider.GetNextPacket().Done();

            // load the comments header...
            var packet = _packetProvider.GetNextPacket();
            if (!LoadComments(packet))
            {
                throw new InvalidDataException("Comment header was not readable!");
            }
            packet.Done();

            // load the book header...
            packet = _packetProvider.GetNextPacket();
            if (!LoadBooks(packet))
            {
                throw new InvalidDataException("Book header was not readable!");
            }
            packet.Done();

            // get the decoding logic bootstrapped
            InitDecoder();

            return true;
        }

        void SetParametersChanging(object sender, ParameterChangeEventArgs e)
        {
            _parameterChangePacket = e.FirstPacket;
        }

        public void Dispose()
        {
            if (_packetProvider != null)
            {
                var temp = _packetProvider;
                _packetProvider = null;
                temp.ParameterChange -= SetParametersChanging;
                temp.Dispose();
            }
        }

        #region Header Decode

        void ProcessParameterChange(DataPacket packet)
        {
            _parameterChangePacket = null;

            // try to do a stream header...
            var wasPeek = false;
            var doFullReset = false;
            if (ProcessStreamHeader(packet))
            {
                packet.Done();
                wasPeek = true;
                doFullReset = true;
                packet = _packetProvider.PeekNextPacket();
                if (packet == null) throw new InvalidDataException("Couldn't get next packet!");
            }

            // try to do a comment header...
            if (LoadComments(packet))
            {
                if (wasPeek)
                {
                    _packetProvider.GetNextPacket().Done();
                }
                else
                {
                    packet.Done();
                }
                wasPeek = true;
                packet = _packetProvider.PeekNextPacket();
                if (packet == null) throw new InvalidDataException("Couldn't get next packet!");
            }

            // try to do a book header...
            if (LoadBooks(packet))
            {
                if (wasPeek)
                {
                    _packetProvider.GetNextPacket().Done();
                }
                else
                {
                    packet.Done();
                }
            }

            ResetDecoder(doFullReset);
        }

        bool ProcessStreamHeader(DataPacket packet)
        {
            if (!packet.ReadBytes(7).SequenceEqual(new byte[] { 0x01, 0x76, 0x6f, 0x72, 0x62, 0x69, 0x73 }))
            {
                // don't mark the packet as done... it might be used elsewhere
                _glueBits += packet.Length * 8;
                return false;
            }

            if (!_pagesSeen.Contains((_lastPageSeen = packet.PageSequenceNumber))) _pagesSeen.Add(_lastPageSeen);

            _glueBits += 56;

            var startPos = packet.BitsRead;

            if (packet.ReadInt32() != 0) throw new InvalidDataException("Only Vorbis stream version 0 is supported.");

            _channels = packet.ReadByte();
            _sampleRate = packet.ReadInt32();
            _upperBitrate = packet.ReadInt32();
            _nominalBitrate = packet.ReadInt32();
            _lowerBitrate = packet.ReadInt32();

            Block0Size = 1 << (int)packet.ReadBits(4);
            Block1Size = 1 << (int)packet.ReadBits(4);

            if (_nominalBitrate == 0)
            {
                if (_upperBitrate > 0 && _lowerBitrate > 0)
                {
                    _nominalBitrate = (_upperBitrate + _lowerBitrate) / 2;
                }
            }

            _metaBits += packet.BitsRead - startPos + 8;

            _wasteHdrBits += 8 * packet.Length - packet.BitsRead;

            return true;
        }

        bool LoadComments(DataPacket packet)
        {
            if (!packet.ReadBytes(7).SequenceEqual(new byte[] { 0x03, 0x76, 0x6f, 0x72, 0x62, 0x69, 0x73 }))
            {
                return false;
            }

            if (!_pagesSeen.Contains((_lastPageSeen = packet.PageSequenceNumber))) _pagesSeen.Add(_lastPageSeen);

            _glueBits += 56;

            _vendor = Encoding.UTF8.GetString(packet.ReadBytes(packet.ReadInt32()));

            _comments = new string[packet.ReadInt32()];
            for (int i = 0; i < _comments.Length; i++)
            {
                _comments[i] = Encoding.UTF8.GetString(packet.ReadBytes(packet.ReadInt32()));
            }

            _metaBits += packet.BitsRead - 56;
            _wasteHdrBits += 8 * packet.Length - packet.BitsRead;

            return true;
        }

        bool LoadBooks(DataPacket packet)
        {
            if (!packet.ReadBytes(7).SequenceEqual(new byte[] { 0x05, 0x76, 0x6f, 0x72, 0x62, 0x69, 0x73 }))
            {
                return false;
            }

            if (!_pagesSeen.Contains((_lastPageSeen = packet.PageSequenceNumber))) _pagesSeen.Add(_lastPageSeen);

            var bits = packet.BitsRead;

            _glueBits += packet.BitsRead;

            // get books
            Books = new VorbisCodebook[packet.ReadByte() + 1];
            for (int i = 0; i < Books.Length; i++)
            {
                Books[i] = VorbisCodebook.Init(this, packet, i);
            }

            _bookBits += packet.BitsRead - bits;
            bits = packet.BitsRead;

            // get times
            Times = new VorbisTime[(int)packet.ReadBits(6) + 1];
            for (int i = 0; i < Times.Length; i++)
            {
                Times[i] = VorbisTime.Init(this, packet);
            }

            _timeHdrBits += packet.BitsRead - bits;
            bits = packet.BitsRead;

            // get floor
            Floors = new VorbisFloor[(int)packet.ReadBits(6) + 1];
            for (int i = 0; i < Floors.Length; i++)
            {
                Floors[i] = VorbisFloor.Init(this, packet);
            }

            _floorHdrBits += packet.BitsRead - bits;
            bits = packet.BitsRead;

            // get residue
            Residues = new VorbisResidue[(int)packet.ReadBits(6) + 1];
            for (int i = 0; i < Residues.Length; i++)
            {
                Residues[i] = VorbisResidue.Init(this, packet);
            }

            _resHdrBits += packet.BitsRead - bits;
            bits = packet.BitsRead;

            // get map
            Maps = new VorbisMapping[(int)packet.ReadBits(6) + 1];
            for (int i = 0; i < Maps.Length; i++)
            {
                Maps[i] = VorbisMapping.Init(this, packet);
            }

            _mapHdrBits += packet.BitsRead - bits;
            bits = packet.BitsRead;

            // get mode settings
            Modes = new VorbisMode[(int)packet.ReadBits(6) + 1];
            for (int i = 0; i < Modes.Length; i++)
            {
                Modes[i] = VorbisMode.Init(this, packet);
            }

            _modeHdrBits += packet.BitsRead - bits;

            // check the framing bit
            if (!packet.ReadBit()) throw new InvalidDataException();

            ++_glueBits;

            _wasteHdrBits += 8 * packet.Length - packet.BitsRead;

            _modeFieldBits = Utils.ilog(Modes.Length - 1);

            return true;
        }

        #endregion

        #region Data Decode

        float[] _prevBuffer;
        RingBuffer _outputBuffer;
        Queue<int> _bitsPerPacketHistory;
        Queue<int> _sampleCountHistory;
        int _preparedLength;
        internal bool _clipped = false;

        Stack<DataPacket> _resyncQueue;

        long _currentPosition;
        long _reportedPosition;

        VorbisMode _mode;
        bool _prevFlag, _nextFlag;
        bool[] _noExecuteChannel;
        VorbisFloor.PacketData[] _floorData;
        float[][] _residue;
        bool _isParameterChange;

        void InitDecoder()
        {
            _currentPosition = 0L;

            _resyncQueue = new Stack<DataPacket>();

            _bitsPerPacketHistory = new Queue<int>();
            _sampleCountHistory = new Queue<int>();

            ResetDecoder(true);
        }

        void ResetDecoder(bool isFullReset)
        {
            // this is called when:
            //  - init (true)
            //  - parameter change w/ stream header (true)
            //  - parameter change w/o stream header (false)
            //  - the decoder encounters a "hiccup" in the data stream (false)
            //  - a seek happens (false)

            // save off the existing "good" data
            if (_preparedLength > 0)
            {
                SaveBuffer();
            }
            if (isFullReset)
            {
                _noExecuteChannel = new bool[_channels];
                _floorData = new VorbisFloor.PacketData[_channels];

                _residue = new float[_channels][];
                for (int i = 0; i < _channels; i++)
                {
                    _residue[i] = new float[Block1Size];
                }

                _outputBuffer = new RingBuffer(Block1Size * 2 * _channels);
                _outputBuffer.Channels = _channels;
            }
            else
            {
                _outputBuffer.Clear();
            }
            _preparedLength = 0;
        }

        void SaveBuffer()
        {
            var buf = new float[_preparedLength * _channels];
            ReadSamples(buf, 0, buf.Length);
            _prevBuffer = buf;
        }

        bool UnpackPacket(DataPacket packet)
        {
            // make sure we're on an audio packet
            if (packet.ReadBit())
            {
                // we really can't do anything... count the bits as waste
                return false;
            }

            // get mode and prev/next flags
            var modeBits = _modeFieldBits;
            _mode = Modes[(int)packet.ReadBits(_modeFieldBits)];
            if (_mode.BlockFlag)
            {
                _prevFlag = packet.ReadBit();
                _nextFlag = packet.ReadBit();
                modeBits += 2;
            }
            else
            {
                _prevFlag = _nextFlag = false;
            }

            if (packet.IsShort) return false;

            var startBits = packet.BitsRead;

            var halfBlockSize = _mode.BlockSize / 2;

            // read the noise floor data (but don't decode yet)
            for (int i = 0; i < _channels; i++)
            {
                _floorData[i] = _mode.Mapping.ChannelSubmap[i].Floor.UnpackPacket(packet, _mode.BlockSize, i);
                _noExecuteChannel[i] = !_floorData[i].ExecuteChannel;

                // go ahead and clear the residue buffers
                Array.Clear(_residue[i], 0, halfBlockSize);
            }

            // make sure we handle no-energy channels correctly given the couplings...
            foreach (var step in _mode.Mapping.CouplingSteps)
            {
                if (_floorData[step.Angle].ExecuteChannel || _floorData[step.Magnitude].ExecuteChannel)
                {
                    _floorData[step.Angle].ForceEnergy = true;
                    _floorData[step.Magnitude].ForceEnergy = true;
                }
            }

            var floorBits = packet.BitsRead - startBits;
            startBits = packet.BitsRead;

            foreach (var subMap in _mode.Mapping.Submaps)
            {
                for (int j = 0; j < _channels; j++)
                {
                    if (_mode.Mapping.ChannelSubmap[j] != subMap)
                    {
                        _floorData[j].ForceNoEnergy = true;
                    }
                }

                var rTemp = subMap.Residue.Decode(packet, _noExecuteChannel, _channels, _mode.BlockSize);
                for (int c = 0; c < _channels; c++)
                {
                    var r = _residue[c];
                    var rt = rTemp[c];
                    for (int i = 0; i < halfBlockSize; i++)
                    {
                        r[i] += rt[i];
                    }
                }
            }

            _glueBits += 1;
            _modeBits += modeBits;
            _floorBits += floorBits;
            _resBits += packet.BitsRead - startBits;
            _wasteBits += 8 * packet.Length - packet.BitsRead;

            _packetCount += 1;

            return true;
        }

        void DecodePacket()
        {
            // inverse coupling
            var steps = _mode.Mapping.CouplingSteps;
            var halfSizeW = _mode.BlockSize / 2;
            for (int i = steps.Length - 1; i >= 0; i--)
            {
                if (_floorData[steps[i].Angle].ExecuteChannel || _floorData[steps[i].Magnitude].ExecuteChannel)
                {
                    var magnitude = _residue[steps[i].Magnitude];
                    var angle = _residue[steps[i].Angle];

                    // we only have to do the first half; MDCT ignores the last half
                    for (int j = 0; j < halfSizeW; j++)
                    {
                        float newM, newA;

                        if (magnitude[j] > 0)
                        {
                            if (angle[j] > 0)
                            {
                                newM = magnitude[j];
                                newA = magnitude[j] - angle[j];
                            }
                            else
                            {
                                newA = magnitude[j];
                                newM = magnitude[j] + angle[j];
                            }
                        }
                        else
                        {
                            if (angle[j] > 0)
                            {
                                newM = magnitude[j];
                                newA = magnitude[j] + angle[j];
                            }
                            else
                            {
                                newA = magnitude[j];
                                newM = magnitude[j] - angle[j];
                            }
                        }

                        magnitude[j] = newM;
                        angle[j] = newA;
                    }
                }
            }

            // apply floor / dot product / MDCT (only run if we have sound energy in that channel)
            for (int c = 0; c < _channels; c++)
            {
                var floorData = _floorData[c];
                var res = _residue[c];
                if (floorData.ExecuteChannel)
                {
                    _mode.Mapping.ChannelSubmap[c].Floor.Apply(floorData, res);
                    Mdct.Reverse(res, _mode.BlockSize);
                }
                else
                {
                    // since we aren't doing the IMDCT, we have to explicitly clear the back half of the block
                    Array.Clear(res, halfSizeW, halfSizeW);
                }
            }
        }

        int OverlapSamples()
        {
            // window
            var window = _mode.GetWindow(_prevFlag, _nextFlag);
            // this is applied as part of the lapping operation

            // now lap the data into the buffer...

            var sizeW = _mode.BlockSize;
            var right = sizeW;
            var center = right >> 1;
            var left = 0;
            var begin = -center;
            var end = center;

            if (_mode.BlockFlag)
            {
                // if the flag is true, it's a long block
                // if the flag is false, it's a short block
                if (!_prevFlag)
                {
                    // previous block was short
                    left = Block1Size / 4 - Block0Size / 4;  // where to start in pcm[][]
                    center = left + Block0Size / 2;     // adjust the center so we're correctly clearing the buffer...
                    begin = Block0Size / -2 - left;     // where to start in _outputBuffer[,]
                }

                if (!_nextFlag)
                {
                    // next block is short
                    right -= sizeW / 4 - Block0Size / 4;
                    end = sizeW / 4 + Block0Size / 4;
                }
            }
            // short blocks don't need any adjustments

            var idx = _outputBuffer.Length / _channels + begin;
            for (var c = 0; c < _channels; c++)
            {
                _outputBuffer.Write(c, idx, left, center, right, _residue[c], window);
            }

            var newPrepLen = _outputBuffer.Length / _channels - end;
            var samplesDecoded = newPrepLen - _preparedLength;
            _preparedLength = newPrepLen;

            return samplesDecoded;
        }

        void UpdatePosition(int samplesDecoded, DataPacket packet)
        {
            _samples += samplesDecoded;

            if (packet.IsResync)
            {
                // during a resync, we have to go through and watch for the next "marker"
                _currentPosition = -packet.PageGranulePosition;
                // _currentPosition will now be end of the page...  wait for the value to change, then go back and repopulate the granule positions accordingly...
                _resyncQueue.Push(packet);
            }
            else
            {
                if (samplesDecoded > 0)
                {
                    _currentPosition += samplesDecoded;
                    packet.GranulePosition = _currentPosition;

                    if (_currentPosition < 0)
                    {
                        if (packet.PageGranulePosition > -_currentPosition)
                        {
                            // we now have a valid granuleposition...  populate the queued packets' GranulePositions
                            var gp = _currentPosition - samplesDecoded;
                            while (_resyncQueue.Count > 0)
                            {
                                var pkt = _resyncQueue.Pop();

                                var temp = pkt.GranulePosition + gp;
                                pkt.GranulePosition = gp;
                                gp = temp;
                            }
                        }
                        else
                        {
                            packet.GranulePosition = -samplesDecoded;
                            _resyncQueue.Push(packet);
                        }
                    }
                    else if (packet.IsEndOfStream && _currentPosition > packet.PageGranulePosition)
                    {
                        var diff = (int)(_currentPosition - packet.PageGranulePosition);
                        if (diff >= 0)
                        {
                            _preparedLength -= diff;
                            _currentPosition -= diff;
                        }
                        else
                        {
                            // uh-oh.  We're supposed to have more samples to this point...
                            _preparedLength = 0;
                        }
                        packet.GranulePosition = packet.PageGranulePosition;
                        _eosFound = true;
                    }
                }
            }
        }

        void DecodeNextPacket()
        {
            _sw.Start();

            DataPacket packet = null;
            try
            {
                // get the next packet
                var packetProvider = _packetProvider;
                if (packetProvider != null)
                {
                    packet = packetProvider.GetNextPacket();
                }

                // if the packet is null, we've hit the end or the packet reader has been disposed...
                if (packet == null)
                {
                    _eosFound = true;
                    return;
                }

                // keep our page count in sync
                if (!_pagesSeen.Contains((_lastPageSeen = packet.PageSequenceNumber))) _pagesSeen.Add(_lastPageSeen);

                // check for resync
                if (packet.IsResync)
                {
                    ResetDecoder(false); // if we're a resync, our current decoder state is invalid...
                }

                // check for parameter change
                if (packet == _parameterChangePacket)
                {
                    _isParameterChange = true;
                    ProcessParameterChange(packet);
                    return;
                }

                if (!UnpackPacket(packet))
                {
                    packet.Done();
                    _wasteBits += 8 * packet.Length;
                    return;
                }
                packet.Done();

                // we can now safely decode all the data without having to worry about a corrupt or partial packet

                DecodePacket();
                var samplesDecoded = OverlapSamples();

                // we can do something cool here...  mark down how many samples were decoded in this packet
                if (packet.GranuleCount.HasValue == false)
                {
                    packet.GranuleCount = samplesDecoded;
                }

                // update our position

                UpdatePosition(samplesDecoded, packet);

                // a little statistical housekeeping...
                var sc = Utils.Sum(_sampleCountHistory) + samplesDecoded;

                _bitsPerPacketHistory.Enqueue((int)packet.BitsRead);
                _sampleCountHistory.Enqueue(samplesDecoded);

                while (sc > _sampleRate)
                {
                    _bitsPerPacketHistory.Dequeue();
                    sc -= _sampleCountHistory.Dequeue();
                }
            }
            catch
            {
                if (packet != null)
                {
                    packet.Done();
                }
                throw;
            }
            finally
            {
                _sw.Stop();
            }
        }

        internal int GetPacketLength(DataPacket curPacket, DataPacket lastPacket)
        {
            // if we don't have a previous packet, or we're re-syncing, this packet has no audio data to return
            if (lastPacket == null || curPacket.IsResync) return 0;

            // make sure they are audio packets
            if (curPacket.ReadBit()) return 0;
            if (lastPacket.ReadBit()) return 0;

            // get the current packet's information
            var modeIdx = (int)curPacket.ReadBits(_modeFieldBits);
            if (modeIdx < 0 || modeIdx >= Modes.Length) return 0;
            var mode = Modes[modeIdx];

            // get the last packet's information
            modeIdx = (int)lastPacket.ReadBits(_modeFieldBits);
            if (modeIdx < 0 || modeIdx >= Modes.Length) return 0;
            var prevMode = Modes[modeIdx];

            // now calculate the totals...
            return mode.BlockSize / 4 + prevMode.BlockSize / 4;
        }

        #endregion

        internal int ReadSamples(float[] buffer, int offset, int count)
        {
            int samplesRead = 0;

            lock (_seekLock)
            {
                if (_prevBuffer != null)
                {
                    // get samples from the previous buffer's data
                    var cnt = Math.Min(count, _prevBuffer.Length);
                    Buffer.BlockCopy(_prevBuffer, 0, buffer, offset, cnt * sizeof(float));

                    // if we have samples left over, rebuild the previous buffer array...
                    if (cnt < _prevBuffer.Length)
                    {
                        var buf = new float[_prevBuffer.Length - cnt];
                        Buffer.BlockCopy(_prevBuffer, cnt * sizeof(float), buf, 0, (_prevBuffer.Length - cnt) * sizeof(float));
                        _prevBuffer = buf;
                    }
                    else
                    {
                        // if no samples left over, clear the previous buffer
                        _prevBuffer = null;
                    }

                    // reduce the desired sample count & increase the desired sample offset
                    count -= cnt;
                    offset += cnt;
                    samplesRead = cnt;
                }
                else if (_isParameterChange)
                {
                    throw new InvalidOperationException("Currently pending a parameter change.  Read new parameters before requesting further samples!");
                }

                int minSize = count + Block1Size * _channels;
                _outputBuffer.EnsureSize(minSize);

                while (_preparedLength * _channels < count && !_eosFound && !_isParameterChange)
                {
                    DecodeNextPacket();

                    // we can safely assume the _prevBuffer was null when we entered this loop
                    if (_prevBuffer != null)
                    {
                        // uh-oh... something is wrong...
                        return ReadSamples(buffer, offset, _prevBuffer.Length);
                    }
                }

                if (_preparedLength * _channels < count)
                {
                    // we can safely assume we've read the last packet...
                    count = _preparedLength * _channels;
                }

                _outputBuffer.CopyTo(buffer, offset, count);
                _preparedLength -= count / _channels;
                _reportedPosition = _currentPosition - _preparedLength;
            }

            return samplesRead + count;
        }

        internal bool IsParameterChange
        {
            get { return _isParameterChange; }
            set
            {
                if (value) throw new InvalidOperationException("Only clearing is supported!");
                _isParameterChange = value;
            }
        }

        internal bool CanSeek
        {
            get { return _packetProvider.CanSeek; }
        }

        internal void SeekTo(long granulePos)
        {
            if (!_packetProvider.CanSeek) throw new NotSupportedException();

            if (granulePos < 0) throw new ArgumentOutOfRangeException("granulePos");

            DataPacket packet;
            if (granulePos > 0)
            {
                packet = _packetProvider.FindPacket(granulePos, GetPacketLength);
                if (packet == null) throw new ArgumentOutOfRangeException("granulePos");
            }
            else
            {
                packet = _packetProvider.GetPacket(4);
            }

            lock (_seekLock)
            {
                // seek the stream
                _packetProvider.SeekToPacket(packet, 1);

                // now figure out where we are and how many samples we need to discard...
                // note that we use the granule position of the "current" packet, since it will be discarded no matter what

                // get the packet that we'll decode next
                var dataPacket = _packetProvider.PeekNextPacket();

                // now read samples until we are exactly at the granule position requested
                CurrentPosition = dataPacket.GranulePosition;
                var cnt = (int)((granulePos - CurrentPosition) * _channels);
                if (cnt > 0)
                {
                    var seekBuffer = new float[cnt];
                    while (cnt > 0)
                    {
                        var temp = ReadSamples(seekBuffer, 0, cnt);
                        if (temp == 0) break;   // we're at the end...
                        cnt -= temp;
                    }
                }
            }
        }

        internal long CurrentPosition
        {
            get { return _reportedPosition; }
            private set
            {
                _reportedPosition = value;
                _currentPosition = value;
                _preparedLength = 0;
                _eosFound = false;

                ResetDecoder(false);
                _prevBuffer = null;
            }
        }

        internal long GetLastGranulePos()
        {
            return _packetProvider.GetGranuleCount();
        }

        internal long ContainerBits
        {
            get { return _packetProvider.ContainerBits; }
        }

        public void ResetStats()
        {
            // only reset the stream info...  don't mess with the container, book, and hdr bits...

            _clipped = false;
            _packetCount = 0;
            _floorBits = 0L;
            _glueBits = 0L;
            _modeBits = 0L;
            _resBits = 0L;
            _wasteBits = 0L;
            _samples = 0L;
            _sw.Reset();
        }

        public int EffectiveBitRate
        {
            get
            {
                if (_samples == 0L) return 0;

                var decodedSeconds = (double)(_currentPosition - _preparedLength) / _sampleRate;

                return (int)(AudioBits / decodedSeconds);
            }
        }

        public int InstantBitRate
        {
            get
            {
                var samples = _sampleCountHistory.Sum();
                if (samples > 0)
                {
                    return (int)((long)_bitsPerPacketHistory.Sum() * _sampleRate / samples);
                }
                else
                {
                    return -1;
                }
            }
        }

        public TimeSpan PageLatency
        {
            get
            {
                return TimeSpan.FromTicks(_sw.ElapsedTicks / PagesRead);
            }
        }

        public TimeSpan PacketLatency
        {
            get
            {
                return TimeSpan.FromTicks(_sw.ElapsedTicks / _packetCount);
            }
        }

        public TimeSpan SecondLatency
        {
            get
            {
                return TimeSpan.FromTicks((_sw.ElapsedTicks / _samples) * _sampleRate);
            }
        }

        public long OverheadBits
        {
            get
            {
                return _glueBits + _metaBits + _timeHdrBits + _wasteHdrBits + _wasteBits + _packetProvider.ContainerBits;
            }
        }

        public long AudioBits
        {
            get
            {
                return _bookBits + _floorHdrBits + _resHdrBits + _mapHdrBits + _modeHdrBits + _modeBits + _floorBits + _resBits;
            }
        }

        public int PagesRead
        {
            get { return _pagesSeen.IndexOf(_lastPageSeen) + 1; }
        }

        public int TotalPages
        {
            get { return _packetProvider.GetTotalPageCount(); }
        }

        public bool Clipped
        {
            get { return _clipped; }
        }
    }
}
