/****************************************************************************
 * NVorbis                                                                  *
 * Copyright (C) 2014, Andrew Ward <afward@gmail.com>                       *
 *                                                                          *
 * See COPYING for license terms (Ms-PL).                                   *
 *                                                                          *
 ***************************************************************************/
using System.IO;

namespace NVorbis
{
    abstract class VorbisMapping
    {
        internal static VorbisMapping Init(VorbisStreamDecoder vorbis, DataPacket packet)
        {
            var type = (int)packet.ReadBits(16);

            VorbisMapping mapping = null;
            switch (type)
            {
                case 0: mapping = new Mapping0(vorbis); break;
            }
            if (mapping == null) throw new InvalidDataException();

            mapping.Init(packet);
            return mapping;
        }

        VorbisStreamDecoder _vorbis;

        protected VorbisMapping(VorbisStreamDecoder vorbis)
        {
            _vorbis = vorbis;
        }

        abstract protected void Init(DataPacket packet);

        internal Submap[] Submaps;

        internal Submap[] ChannelSubmap;

        internal CouplingStep[] CouplingSteps;

        class Mapping0 : VorbisMapping
        {
            internal Mapping0(VorbisStreamDecoder vorbis) : base(vorbis) { }

            protected override void Init(DataPacket packet)
            {
                var submapCount = 1;
                if (packet.ReadBit()) submapCount += (int)packet.ReadBits(4);

                // square polar mapping
                var couplingSteps = 0;
                if (packet.ReadBit())
                {
                    couplingSteps = (int)packet.ReadBits(8) + 1;
                }

                var couplingBits = Utils.ilog(_vorbis._channels - 1);
                CouplingSteps = new CouplingStep[couplingSteps];
                for (int j = 0; j < couplingSteps; j++)
                {
                    var magnitude = (int)packet.ReadBits(couplingBits);
                    var angle = (int)packet.ReadBits(couplingBits);
                    if (magnitude == angle || magnitude > _vorbis._channels - 1 || angle > _vorbis._channels - 1)
                        throw new InvalidDataException();
                    CouplingSteps[j] = new CouplingStep { Angle = angle, Magnitude = magnitude };
                }

                // reserved bits
                if (packet.ReadBits(2) != 0UL) throw new InvalidDataException();

                // channel multiplex
                var mux = new int[_vorbis._channels];
                if (submapCount > 1)
                {
                    for (int c = 0; c < ChannelSubmap.Length; c++)
                    {
                        mux[c] = (int)packet.ReadBits(4);
                        if (mux[c] >= submapCount) throw new InvalidDataException();
                    }
                }

                // submaps
                Submaps = new Submap[submapCount];
                for (int j = 0; j < submapCount; j++)
                {
                    packet.ReadBits(8); // unused placeholder
                    var floorNum = (int)packet.ReadBits(8);
                    if (floorNum >= _vorbis.Floors.Length) throw new InvalidDataException();
                    var residueNum = (int)packet.ReadBits(8);
                    if (residueNum >= _vorbis.Residues.Length) throw new InvalidDataException();

                    Submaps[j] = new Submap
                    {
                        Floor = _vorbis.Floors[floorNum],
                        Residue = _vorbis.Residues[floorNum]
                    };
                }

                ChannelSubmap = new Submap[_vorbis._channels];
                for (int c = 0; c < ChannelSubmap.Length; c++)
                {
                    ChannelSubmap[c] = Submaps[mux[c]];
                }
            }
        }

        internal class Submap
        {
            internal Submap() { }

            internal VorbisFloor Floor;
            internal VorbisResidue Residue;
        }

        internal class CouplingStep
        {
            internal CouplingStep() { }

            internal int Magnitude;
            internal int Angle;
        }
    }
}
